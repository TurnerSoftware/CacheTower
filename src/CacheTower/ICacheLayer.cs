using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheLayer
	{
		Task CleanupAsync();
		Task EvictAsync(string cacheKey);
		Task<CacheEntry<T>> GetAsync<T>(string cacheKey);
		Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry);
		Task<bool> IsAvailableAsync(string cacheKey);
	}
}
