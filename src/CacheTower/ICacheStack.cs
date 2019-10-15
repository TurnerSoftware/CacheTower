using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheStack
	{
		Guid StackId { get; }
		IEnumerable<ICacheLayer> Layers { get; }
		Task CleanupAsync();
		Task EvictAsync(string cacheKey);
		Task<CacheEntry<T>> SetAsync<T>(string cacheKey, T value, TimeSpan timeToLive);
		Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry);
		Task<CacheEntry<T>> GetAsync<T>(string cacheKey);
		Task<T> GetOrSetAsync<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings);
	}
}
