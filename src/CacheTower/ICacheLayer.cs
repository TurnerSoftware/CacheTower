using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheLayer
	{
		ValueTask CleanupAsync();
		ValueTask EvictAsync(string cacheKey);
		ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey);
		ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry);
		ValueTask<bool> IsAvailableAsync(string cacheKey);
	}

	public interface ISyncCacheLayer : ICacheLayer
	{
	}
	public interface IAsyncCacheLayer : ICacheLayer
	{
	}
}
