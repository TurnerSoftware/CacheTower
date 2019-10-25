using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	/// <summary>
	/// Based on Loris Cro's "RedisMemoLock"
	/// https://github.com/kristoff-it/redis-memolock/blob/77da8f82711309b9dd81eafd02cb7ccfb22344c7/csharp/redis-memolock/RedisMemoLock.cs
	/// </summary>
	public class RedisLockExtension : IRefreshWrapperExtension
	{
		private ISubscriber Subscriber { get; }
		private IDatabaseAsync Database { get; }
		private string RedisChannel { get; }
		private TimeSpan LockTimeout { get; } = TimeSpan.FromMinutes(1);

		private ICacheStack RegisteredStack { get; set; }
		
		private ConcurrentDictionary<string, IEnumerable<TaskCompletionSource<bool>>> LockedOnKeyRefresh { get; }

		public RedisLockExtension(ConnectionMultiplexer connection, int databaseIndex = -1, string channelPrefix = "CacheTower", TimeSpan? lockTimeout = default)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			if (channelPrefix == null)
			{
				throw new ArgumentNullException(nameof(channelPrefix));
			}
			
			Database = connection.GetDatabase(databaseIndex);
			Subscriber = connection.GetSubscriber();
			RedisChannel = $"{channelPrefix}.CacheLock";

			if (lockTimeout.HasValue)
			{
				LockTimeout = lockTimeout.Value;
			}

			LockedOnKeyRefresh = new ConcurrentDictionary<string, IEnumerable<TaskCompletionSource<bool>>>(StringComparer.Ordinal);

			Subscriber.Subscribe(RedisChannel, (channel, value) => UnlockWaitingTasks(value));
		}

		public void Register(ICacheStack cacheStack)
		{
			if (RegisteredStack != null)
			{
				throw new InvalidOperationException($"{nameof(RedisLockExtension)} can only be registered to one {nameof(ICacheStack)}");
			}

			RegisteredStack = cacheStack;
		}

		public async Task<CacheEntry<T>> RefreshValueAsync<T>(string cacheKey, Func<Task<CacheEntry<T>>> valueProvider, CacheSettings settings)
		{
			var hasLock = await Database.StringSetAsync(cacheKey, RedisValue.EmptyString, expiry: LockTimeout, when: When.NotExists);

			if (hasLock)
			{
				try
				{
					var cacheEntry = await valueProvider();
					await Subscriber.PublishAsync(RedisChannel, cacheKey, CommandFlags.FireAndForget);
					return cacheEntry;
				}
				finally
				{
					await Database.KeyDeleteAsync(cacheKey, CommandFlags.FireAndForget);
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
			if (currentEntry != null && !currentEntry.HasElapsed(settings.StaleAfter))
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
