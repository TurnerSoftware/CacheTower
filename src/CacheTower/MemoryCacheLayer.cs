using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public class MemoryCacheLayer : ICacheLayer
	{
		private ConcurrentDictionary<string, CacheEntry> Cache { get; } = new ConcurrentDictionary<string, CacheEntry>();

		public Task Cleanup(TimeSpan maxTimeStale)
		{
			foreach (var cachePair in Cache)
			{
				var cacheEntry = cachePair.Value;
				if (cacheEntry.HasElapsed(maxTimeStale))
				{
					Cache.TryRemove(cachePair.Key, out var _);
				}
			}

			return Task.CompletedTask;
		}

		public Task Evict(string cacheKey)
		{
			Cache.TryRemove(cacheKey, out var _);
			return Task.CompletedTask;
		}

		public Task<CacheEntry<T>> Get<T>(string cacheKey)
		{
			if (Cache.TryGetValue(cacheKey, out var cacheEntry))
			{
				return Task.FromResult(cacheEntry as CacheEntry<T>);
			}

			return Task.FromResult(default(CacheEntry<T>));
		}

		public Task<bool> IsAvailable(string cacheKey)
		{
			return Task.FromResult(true);
		}

		public Task Set<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			Cache.AddOrUpdate(cacheKey, cacheEntry, (key, old) =>
			{
				return cacheEntry;
			});

			return Task.CompletedTask;
		}
	}
}
