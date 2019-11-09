using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheTower.Providers.Memory
{
	public class MemoryCacheLayer : ISyncCacheLayer
	{
		private ConcurrentDictionary<string, CacheEntry> Cache { get; } = new ConcurrentDictionary<string, CacheEntry>(StringComparer.Ordinal);

		public void Cleanup()
		{
			var currentTime = DateTime.UtcNow;

			foreach (var cachePair in Cache)
			{
				var cacheEntry = cachePair.Value;
				if (cacheEntry.Expiry < currentTime)
				{
					Cache.TryRemove(cachePair.Key, out _);
				}
			}
		}

		public void Evict(string cacheKey)
		{
			Cache.TryRemove(cacheKey, out _);
		}

		public CacheEntry<T> Get<T>(string cacheKey)
		{
			if (Cache.TryGetValue(cacheKey, out var cacheEntry))
			{
				return cacheEntry as CacheEntry<T>;
			}

			return default;
		}

		public bool IsAvailable(string cacheKey)
		{
			return true;
		}

		public void Set<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			Cache[cacheKey] = cacheEntry;
		}
	}
}
