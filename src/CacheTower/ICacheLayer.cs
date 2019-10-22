using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheLayer
	{
	}

	public interface ISyncCacheLayer : ICacheLayer
	{
		void Cleanup();
		void Evict(string cacheKey);
		CacheEntry<T> Get<T>(string cacheKey);
		void Set<T>(string cacheKey, CacheEntry<T> cacheEntry);
		bool IsAvailable(string cacheKey);
	}
	public interface IAsyncCacheLayer : ICacheLayer
	{
		Task CleanupAsync();
		Task EvictAsync(string cacheKey);
		Task<CacheEntry<T>> GetAsync<T>(string cacheKey);
		Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry);
		Task<bool> IsAvailableAsync(string cacheKey);
	}
}
