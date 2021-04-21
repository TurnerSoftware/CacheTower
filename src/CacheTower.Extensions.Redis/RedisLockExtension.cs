using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

			Subscriber.Subscribe(options.RedisChannel, (channel, value) => UnlockWaitingTasks(value));
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
		public async ValueTask<CacheEntry<T>> WithRefreshAsync<T>(string cacheKey, Func<ValueTask<CacheEntry<T>>> valueProvider, CacheSettings settings)
		{
			var lockKey = string.Format(Options.KeyFormat, cacheKey);
			var hasLock = await Database.StringSetAsync(lockKey, RedisValue.EmptyString, expiry: Options.LockTimeout, when: When.NotExists);

			if (hasLock)
			{
				try
				{
					var cacheEntry = await valueProvider();
					await Subscriber.PublishAsync(Options.RedisChannel, cacheKey, CommandFlags.FireAndForget);
					return cacheEntry;
				}
				finally
				{
					await Database.KeyDeleteAsync(lockKey, CommandFlags.FireAndForget);
				}
			}
			else
			{
				var completionSource = LockedOnKeyRefresh.GetOrAdd(cacheKey, key => new TaskCompletionSource<bool>());

				//Last minute check to confirm whether waiting is required (in case the notification is missed)
				var currentEntry = await RegisteredStack!.GetAsync<T>(cacheKey);
				if (currentEntry != null && currentEntry.GetStaleDate(settings) > Internal.DateTimeProvider.Now)
				{
					UnlockWaitingTasks(cacheKey);
					return currentEntry;
				}

				//Lock until we are notified to be unlocked
				await completionSource.Task;

				//Get the updated value from the cache stack
				return (await RegisteredStack.GetAsync<T>(cacheKey))!;
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
