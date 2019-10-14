using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower.Providers.Memory
{
	public class MemoryCacheLayer : ICacheLayer
	{
		private ConcurrentDictionary<string, CacheEntry> Cache { get; } = new ConcurrentDictionary<string, CacheEntry>();

		private static readonly Task<bool> CompletedTaskTrue = Task.FromResult(true);

		public Task CleanupAsync()
		{
			foreach (var cachePair in Cache)
			{
				var cacheEntry = cachePair.Value;
				if (cacheEntry.HasElapsed(cacheEntry.TimeToLive))
				{
					Cache.TryRemove(cachePair.Key, out var _);
				}
			}

			return Task.CompletedTask;
		}

		public Task EvictAsync(string cacheKey)
		{
			Cache.TryRemove(cacheKey, out var _);
			return Task.CompletedTask;
		}

		public Task<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			if (Cache.TryGetValue(cacheKey, out var cacheEntry))
			{
				return Task.FromResult(cacheEntry as CacheEntry<T>);
			}

			return Task.FromResult(default(CacheEntry<T>));
		}

		public Task<bool> IsAvailableAsync(string cacheKey)
		{
			return CompletedTaskTrue;
		}

		public Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			Cache.AddOrUpdate(cacheKey, cacheEntry, (key, old) =>
			{
				return cacheEntry;
			});

			return Task.CompletedTask;
		}
	}
}
