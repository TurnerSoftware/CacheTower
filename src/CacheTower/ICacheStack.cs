using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheStack
	{
		ValueTask CleanupAsync();
		ValueTask EvictAsync(string cacheKey);
		ValueTask<CacheEntry<T>> SetAsync<T>(string cacheKey, T value, TimeSpan timeToLive);
		ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry);
		ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey);
		ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, Task<T>> getter, CacheSettings settings);
	}

	public interface ICacheStack<out TContext> : ICacheStack
	{
		ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, TContext, Task<T>> getter, CacheSettings settings);
	}
}
