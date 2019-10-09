using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CacheTower
{
	public class CacheStack
	{
		private ConcurrentDictionary<string, object> CacheKeyLock { get; } = new ConcurrentDictionary<string, object>();

		private ICacheLayer[] CacheLayers { get; }

		private ICacheContext Context { get; }

		public CacheStack(ICacheContext context, ICacheLayer[] cacheLayers)
		{
			Context = context;
			CacheLayers = cacheLayers;
		}

		public async Task Cleanup(TimeSpan maxTimeStale)
		{
			foreach (var layer in CacheLayers)
			{
				await layer.Cleanup(maxTimeStale);
			}
		}

		public async Task Evict(string cacheKey)
		{
			foreach (var layer in CacheLayers)
			{
				await layer.Evict(cacheKey);
			}
		}

		public async Task<CacheEntry<T>> Set<T>(string cacheKey, T value, TimeSpan timeToLive)
		{
			var entry = new CacheEntry<T>(value, timeToLive);
			foreach (var layer in CacheLayers)
			{
				await layer.Set(cacheKey, entry);
			}
			return entry;
		}

		public async Task<CacheEntry<T>> Get<T>(string cacheKey)
		{
			for (int i = 0; i < CacheLayers.Length; i++)
			{
				var cacheLayer = CacheLayers[i];
				if (await cacheLayer.IsAvailable(cacheKey))
				{
					var cacheEntry = await cacheLayer.Get<T>(cacheKey);
					if (cacheEntry != default)
					{
						//Populate previous cache layers
						for (; --i >= 0;)
						{
							cacheLayer = CacheLayers[i];
							await cacheLayer.Set(cacheKey, cacheEntry);
						}
						
						return cacheEntry;
					}
				}
			}

			return default;
		}

		public async Task<T> GetOrSet<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings)
		{
			var cacheEntry = await Get<T>(cacheKey);
			if (cacheEntry != default)
			{
				if (cacheEntry.HasElapsed(settings.TimeToLive) && !CacheKeyLock.ContainsKey(cacheKey))
				{
					if (cacheEntry.HasElapsed(settings.TimeAllowedStale))
					{
						//Refresh the value in the current thread though short circuit if we're unable to establish a lock
						//If the lock isn't established, it will instead use the stale cache entry (even if past the allowed stale period)
						var refreshedCacheEntry = await RefreshValue(cacheKey, getter, settings, exitOnLockFailure: true);
						if (refreshedCacheEntry != default)
						{
							cacheEntry = refreshedCacheEntry;
						}
					}
					else
					{
						//Refresh the value in the background
						_ = Task.Run(() => RefreshValue(cacheKey, getter, settings, exitOnLockFailure: true));
					}
				}

				return cacheEntry.Value;
			}
			else
			{
				//Refresh the value in the current thread though because we have no old cache value, we have to lock and wait
				cacheEntry = await RefreshValue(cacheKey, getter, settings, exitOnLockFailure: false);
			}

			return cacheEntry.Value;
		}

		private async Task<CacheEntry<T>> RefreshValue<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings, bool exitOnLockFailure)
		{
			var lockObj = CacheKeyLock.GetOrAdd(cacheKey, (key) => new object());

			if (exitOnLockFailure && !Monitor.TryEnter(lockObj))
			{
				return default;
			}
			else
			{
				Monitor.Enter(lockObj);
			}

			CacheEntry<T> cacheEntry = default;

			try
			{
				cacheEntry = await Get<T>(cacheKey);

				//Confirm that once we have the lock, the latest cache entry still needs updating
				if (cacheEntry == null || cacheEntry.HasElapsed(settings.TimeAllowedStale))
				{
					var oldValue = default(T);
					if (cacheEntry == null)
					{
						oldValue = cacheEntry.Value;
					}

					var value = await getter(oldValue, Context);
					cacheEntry = await Set(cacheKey, value, settings.TimeToLive);
				}
			}
			finally
			{
				CacheKeyLock.TryRemove(cacheKey, out var _);
				Monitor.Exit(lockObj);
			}

			return cacheEntry;
		}
	}
}
