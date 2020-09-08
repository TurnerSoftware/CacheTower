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
		ValueTask<CacheEntry<T>> SetAsync<T>(string cacheKey, T value, TimeSpan timeToLive, CacheSettings storageSettings = default);
		ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry, CacheSettings storageSettings = default);
		ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey);
		ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, Task<T>> getter, CacheEntryLifetime settings, CacheSettings storageSettings = default);
	}

	public interface ICacheStack<out TContext> : ICacheStack
	{
		ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, TContext, Task<T>> getter, CacheEntryLifetime settings, CacheSettings storageSettings = default);
	}
}
