using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	/// <summary>
	/// Based on Loris Cro's "RedisMemoLock"
	/// https://github.com/kristoff-it/redis-memolock/blob/77da8f82711309b9dd81eafd02cb7ccfb22344c7/csharp/redis-memolock/RedisMemoLock.cs
	/// </summary>
	public class RedisLockExtension : ICacheRefreshCallSiteWrapperExtension
	{
		private ISubscriber Subscriber { get; }
		private IDatabaseAsync Database { get; }
		private RedisLockOptions Options { get; }

		private ICacheStack RegisteredStack { get; set; }
		
		internal ConcurrentDictionary<string, IEnumerable<TaskCompletionSource<bool>>> LockedOnKeyRefresh { get; }

		public RedisLockExtension(ConnectionMultiplexer connection) : this(connection, RedisLockOptions.Default) { }

		public RedisLockExtension(ConnectionMultiplexer connection, RedisLockOptions options)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			Options = options;
			Database = connection.GetDatabase(options.DatabaseIndex);
			Subscriber = connection.GetSubscriber();

			LockedOnKeyRefresh = new ConcurrentDictionary<string, IEnumerable<TaskCompletionSource<bool>>>(StringComparer.Ordinal);

			Subscriber.Subscribe(options.RedisChannel, (channel, value) => UnlockWaitingTasks(value));
		}

		public void Register(ICacheStack cacheStack)
		{
			if (RegisteredStack != null)
			{
				throw new InvalidOperationException($"{nameof(RedisLockExtension)} can only be registered to one {nameof(ICacheStack)}");
			}

			RegisteredStack = cacheStack;
		}

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
				return await WaitForResult<T>(cacheKey, settings);
			}
		}

		private async Task<CacheEntry<T>> WaitForResult<T>(string cacheKey, CacheSettings settings)
		{
			var delayedResultSource = new TaskCompletionSource<bool>();
			var waitList = new[] { delayedResultSource };
			LockedOnKeyRefresh.AddOrUpdate(cacheKey, waitList, (key, oldList) => oldList.Concat(waitList));

			//Last minute check to confirm whether waiting is required (in case the notification is missed)
			var currentEntry = await RegisteredStack.GetAsync<T>(cacheKey);
			if (currentEntry != null && currentEntry.GetStaleDate(settings) > DateTime.UtcNow)
			{
				UnlockWaitingTasks(cacheKey);
				return currentEntry;
			}

			//Lock until we are notified to be unlocked
			await delayedResultSource.Task;

			//Get the updated value from the cache stack
			return await RegisteredStack.GetAsync<T>(cacheKey);
		}

		private void UnlockWaitingTasks(string cacheKey)
		{
			if (LockedOnKeyRefresh.TryRemove(cacheKey, out var waitingTasks))
			{
				foreach (var task in waitingTasks)
				{
					task.TrySetResult(true);
				}
			}
		}
	}
}
