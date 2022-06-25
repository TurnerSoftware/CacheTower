using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	/// <summary>
	/// Provides distributed cache locking via Redis.
	/// </summary>
	/// <remarks>
	/// Based on <a href="https://github.com/kristoff-it/redis-memolock/blob/77da8f82711309b9dd81eafd02cb7ccfb22344c7/csharp/redis-memolock/RedisMemoLock.cs">Loris Cro's RedisMemoLock"</a>
	/// </remarks>
	public class RedisLockExtension : ICacheRefreshCallSiteWrapperExtension
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

			Subscriber.Subscribe(options.RedisChannel, (channel, value) =>
			{
				if (!value.IsNull)
				{
					UnlockWaitingTasks(value!);
				}
			});
		}

		/// <inheritdoc/>
		public void Register(ICacheStack cacheStack)
		{
			if (RegisteredStack != null)
			{
				throw new InvalidOperationException($"{nameof(RedisLockExtension)} can only be registered to one {nameof(ICacheStack)}");
			}

			RegisteredStack = cacheStack;
		}

		/// <remarks>
		/// The <see cref="RedisLockExtension"/> attempts to set a key in Redis representing whether it has achieved a lock.
		/// If it succeeds to set the key, it continues to refresh the value, broadcasting the success of the updated value to all subscribers.
		/// If it fails to set the key, it waits until notified that the cache is updated, retrieving it from the cache stack and returning the value.
		/// </remarks>
		/// <inheritdoc/>
		public async ValueTask<CacheEntry<TValue>> WithRefreshAsync<TValue, TState>(string cacheKey, Func<TState, ValueTask<CacheEntry<TValue>>> asyncValueFactory, TState state, CacheSettings settings)
		{
			var lockKey = string.Format(Options.KeyFormat, cacheKey);
			var hasLock = await Database.StringSetAsync(lockKey, RedisValue.EmptyString, expiry: Options.LockTimeout, when: When.NotExists);

			if (hasLock)
			{
				try
				{
					var cacheEntry = await asyncValueFactory(state);
					return cacheEntry;
				}
				finally
				{
					await Subscriber.PublishAsync(Options.RedisChannel, cacheKey, CommandFlags.FireAndForget);
					await Database.KeyDeleteAsync(lockKey, CommandFlags.FireAndForget);
				}
			}
			else
			{
				var completionSource = LockedOnKeyRefresh.GetOrAdd(cacheKey, key =>
				{
					var tcs = new TaskCompletionSource<bool>();
			
					if (Options.LockCheckStrategy.UseSpinLock)
					{
						_ = TestLock(tcs, Options);
					}
					else
					{
						var cts = new CancellationTokenSource(Options.LockTimeout);
						cts.Token.Register(tcs => ((TaskCompletionSource<bool>)tcs).TrySetCanceled(), tcs, useSynchronizationContext: false);
					}

					return tcs;

					async Task TestLock(TaskCompletionSource<bool> taskCompletionSource, RedisLockOptions options)
					{
						var spinAttempt = 0;
						var maxSpinAttempts = options.LockCheckStrategy.CalculateSpinAttempts(options.LockTimeout);
						while (spinAttempt <= maxSpinAttempts && 
						       !taskCompletionSource.Task.IsCanceled && 
						       !taskCompletionSource.Task.IsCompleted)
						{
							spinAttempt++;

							var lockExists = await Database.KeyExistsAsync(lockKey);

							if (lockExists)
							{
								await Task.Delay(options.LockCheckStrategy.SpinTime);
								continue;
							}

							taskCompletionSource.TrySetResult(true);
							return;
						}

						taskCompletionSource.TrySetCanceled();
					}
				});

				//Last minute check to confirm whether waiting is required (in case the notification is missed)
				var currentEntry = await RegisteredStack!.GetAsync<TValue>(cacheKey);
				if (currentEntry != null && currentEntry.GetStaleDate(settings) > Internal.DateTimeProvider.Now)
				{
					UnlockWaitingTasks(cacheKey);
					return currentEntry;
				}

				//Lock until we are notified to be unlocked
				await completionSource.Task;

				//Get the updated value from the cache stack
				return (await RegisteredStack.GetAsync<TValue>(cacheKey))!;
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
}
