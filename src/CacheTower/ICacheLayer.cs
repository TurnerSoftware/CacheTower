using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheLayer
	{
		Task Cleanup();
		Task Evict(string cacheKey);
		Task<CacheEntry<T>> Get<T>(string cacheKey);
		Task Set<T>(string cacheKey, CacheEntry<T> cacheEntry);
		Task<bool> IsAvailable(string cacheKey);
	}
}
