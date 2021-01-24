using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CacheTower.Providers.Memory
{
	public class MemoryCacheLayer : ICacheLayer
	{
		private ConcurrentDictionary<string, CacheEntry> Cache { get; } = new ConcurrentDictionary<string, CacheEntry>(StringComparer.Ordinal);

		public ValueTask CleanupAsync()
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

			return new ValueTask();
		}

		public ValueTask EvictAsync(string cacheKey)
		{
			Cache.TryRemove(cacheKey, out _);
			return new ValueTask();
		}

		public ValueTask FlushAsync()
		{
			Cache.Clear();
			return new ValueTask();
		}

		public ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			if (Cache.TryGetValue(cacheKey, out var cacheEntry))
			{
				return new ValueTask<CacheEntry<T>>(cacheEntry as CacheEntry<T>);
			}

			return new ValueTask<CacheEntry<T>>(default(CacheEntry<T>));
		}

		public ValueTask<bool> IsAvailableAsync(string cacheKey)
		{
			return new ValueTask<bool>(true);
		}

		public ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			Cache[cacheKey] = cacheEntry;
			return new ValueTask();
		}
	}
}
