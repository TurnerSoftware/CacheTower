using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis;

/// <summary>
/// Provides distributed cache locking via Redis.
/// </summary>
/// <remarks>
/// Based on <a href="https://github.com/kristoff-it/redis-memolock/blob/77da8f82711309b9dd81eafd02cb7ccfb22344c7/csharp/redis-memolock/RedisMemoLock.cs">Loris Cro's RedisMemoLock"</a>
/// </remarks>
public class RedisLockExtension : IDistributedLockExtension
{
	private ISubscriber Subscriber { get; }
	private IDatabaseAsync Database { get; }
	private RedisLockOptions Options { get; }

	private ICacheStack? RegisteredStack { get; set; }
	
	internal ConcurrentDictionary<string, TaskCompletionSource<bool>> LockedOnKeyRefresh { get; }

	/// <summary>
	/// Creates a new instance of <see cref="RedisLockExtension"/> with the given <paramref name="connection"/> and default lock options.
	/// </summary>
	/// <param name="connection">The primary connection to Redis where the distributed lock will be co-ordinated through.</param>
	public RedisLockExtension(IConnectionMultiplexer connection) : this(connection, RedisLockOptions.Default) { }

	/// <summary>
	/// Creates a new instance of <see cref="RedisLockExtension"/> with the given <paramref name="connection"/> and <paramref name="options"/>.
	/// </summary>
	/// <param name="connection">The primary connection to Redis where the distributed lock will be co-ordinated through.</param>
	/// <param name="options">The lock options to configure the behaviour of locking.</param>
	public RedisLockExtension(IConnectionMultiplexer connection, RedisLockOptions options)
	{
		if (connection == null)
		{
			throw new ArgumentNullException(nameof(connection));
		}

		Options = options;
		Database = connection.GetDatabase(options.DatabaseIndex);
		Subscriber = connection.GetSubscriber();

		LockedOnKeyRefresh = new ConcurrentDictionary<string, TaskCompletionSource<bool>>(StringComparer.Ordinal);

		Subscriber.Subscribe(GetRedisChannel(), (channel, value) =>
		{
			if (!value.IsNull)
			{
				UnlockWaitingTasks(value!);
			}
		});
	}

	private RedisChannel GetRedisChannel() => new(Options.RedisChannel, RedisChannel.PatternMode.Auto);

	/// <inheritdoc/>
	public void Register(ICacheStack cacheStack)
	{
		if (RegisteredStack != null)
		{
			throw new InvalidOperationException($"{nameof(RedisLockExtension)} can only be registered to one {nameof(ICacheStack)}");
		}

		RegisteredStack = cacheStack;
	}

	private async ValueTask ReleaseLockAsync(string cacheKey)
	{
		var lockKey = string.Format(Options.KeyFormat, cacheKey);
		await Subscriber.PublishAsync(GetRedisChannel(), cacheKey, CommandFlags.FireAndForget).ConfigureAwait(false);
		await Database.KeyDeleteAsync(lockKey, CommandFlags.FireAndForget).ConfigureAwait(false);
		UnlockWaitingTasks(cacheKey);
	}

	private async ValueTask SpinWaitAsync(TaskCompletionSource<bool> taskCompletionSource, string lockKey)
	{
		var spinAttempt = 0;
		var maxSpinAttempts = Options.LockCheckStrategy.CalculateSpinAttempts(Options.LockTimeout);
		while (spinAttempt <= maxSpinAttempts && !taskCompletionSource.Task.IsCanceled && !taskCompletionSource.Task.IsCompleted)
		{
			spinAttempt++;

			var lockExists = await Database.KeyExistsAsync(lockKey).ConfigureAwait(false);
			if (lockExists)
			{
				await Task.Delay(Options.LockCheckStrategy.SpinTime).ConfigureAwait(false);
				continue;
			}

			taskCompletionSource.TrySetResult(true);
			return;
		}

		taskCompletionSource.TrySetCanceled();
	}

	private async ValueTask DelayWaitAsync(TaskCompletionSource<bool> taskCompletionSource)
	{
		await Task.Delay(Options.LockTimeout).ConfigureAwait(false);
		taskCompletionSource.TrySetCanceled();
	}

	/// <inheritdoc/>
	public async ValueTask<DistributedLock> AwaitAccessAsync(string cacheKey)
	{
		var lockKey = string.Format(Options.KeyFormat, cacheKey);
		var hasLock = await Database.StringSetAsync(lockKey, RedisValue.EmptyString, expiry: Options.LockTimeout, when: When.NotExists).ConfigureAwait(false);
		
		if (hasLock)
		{
			return DistributedLock.Locked(cacheKey, ReleaseLockAsync);
		}
		else
		{
			var completionSource = LockedOnKeyRefresh.GetOrAdd(cacheKey, key =>
			{
				var taskCompletionSource = new TaskCompletionSource<bool>();

				if (Options.LockCheckStrategy.UseSpinLock)
				{
					_ = SpinWaitAsync(taskCompletionSource, lockKey);
				}
				else
				{
					_ = DelayWaitAsync(taskCompletionSource);
				}

				return taskCompletionSource;
			});

			//Last minute check to confirm whether waiting is required (in case the notification is missed)
			if (!await Database.KeyExistsAsync(lockKey).ConfigureAwait(false))
			{
				UnlockWaitingTasks(cacheKey);
				return DistributedLock.Unlocked(cacheKey);
			}

			await completionSource.Task.ConfigureAwait(false);
			return DistributedLock.Unlocked(cacheKey);
		}
	}

	private void UnlockWaitingTasks(string cacheKey)
	{
		if (LockedOnKeyRefresh.TryRemove(cacheKey, out var waitingTasks))
		{
			waitingTasks.TrySetResult(true);
		}
	}
}
