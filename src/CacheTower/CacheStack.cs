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

		public async Task Set<T>(string cacheKey, T value, TimeSpan timeToLive)
		{
			var entry = new CacheEntry<T>(value, timeToLive);
			foreach (var layer in CacheLayers)
			{
				await layer.Set(cacheKey, entry);
			}
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
						return await RefreshValue(cacheKey, getter, settings);
					}

					BackgroundRefreshValue(cacheKey, getter, settings);
				}

				return cacheEntry.Value;
			}
			else
			{
				return await RefreshValue(cacheKey, getter, settings);
			}
		}

		private async Task<T> RefreshValue<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings)
		{
			var lockObj = CacheKeyLock.GetOrAdd(cacheKey, (key) => new object());
			Monitor.Enter(lockObj);

			T value = default;

			try
			{
				var lockCheckCacheEntry = await Get<T>(cacheKey);
				value = lockCheckCacheEntry.Value;

				//Confirm that once we have the lock, the latest cache entry still needs updating
				if (lockCheckCacheEntry.HasElapsed(settings.TimeAllowedStale))
				{
					value = await getter(lockCheckCacheEntry.Value, Context);
					await Set(cacheKey, value, settings.TimeToLive);
				}
			}
			finally
			{
				CacheKeyLock.TryRemove(cacheKey, out var _);
				Monitor.Exit(lockObj);
			}

			return value;
		}

		private void BackgroundRefreshValue<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings)
		{
			if (!CacheKeyLock.ContainsKey(cacheKey))
			{
				Task.Run(() => RefreshValue(cacheKey, getter, settings));
			}
		}
	}
}
