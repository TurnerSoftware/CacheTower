using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheStack
	{
		/// <summary>
		/// Triggers the cleanup of any cache entries across the stack.
		/// </summary>
		/// <remarks>
		/// Some cache layers, like Redis, provide automatic cleanup of expired entries whereas other cache layers do not.
		/// <br/>
		/// This is used by <see cref="Extensions.AutoCleanupExtension"/> where cache layers are cleaned up on a timer.
		/// </remarks>
		/// <returns></returns>
		ValueTask CleanupAsync();
		/// <summary>
		/// Removes an entry with the corresponding <paramref name="cacheKey"/> from all cache layers.
		/// </summary>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <returns></returns>
		ValueTask EvictAsync(string cacheKey);
		/// <summary>
		/// Sets the entry for <paramref name="cacheKey"/> to <paramref name="value"/> with a corresponding <paramref name="timeToLive"/> across all cache layers.
		/// </summary>
		/// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <param name="value">The value to store in the cache.</param>
		/// <param name="timeToLive">How long till the entry is expired.</param>
		/// <returns></returns>
		ValueTask<CacheEntry<T>> SetAsync<T>(string cacheKey, T value, TimeSpan timeToLive);
		/// <summary>
		/// Sets the <paramref name="cacheEntry"/> to a specific <paramref name="cacheKey"/> across all cache layers.
		/// </summary>
		/// <typeparam name="T">The type of value in <paramref name="cacheEntry"/>.</typeparam>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <param name="cacheEntry">The cache entry to set.</param>
		/// <returns></returns>
		ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry);
		/// <summary>
		/// Retrieves the <see cref="CacheEntry{T}"/> for a given <paramref name="cacheKey"/>.
		/// </summary>
		/// <remarks>
		/// The entry returned corresponds to the first cache layer that contains it.
		/// <br/>
		/// If no cache layer contains it, Null is returned.
		/// </remarks>
		/// <typeparam name="T">The type of value in the cache entry.</typeparam>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <returns></returns>
		ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey);
		/// <summary>
		/// Attempts to retrieve the value for the given <paramref name="cacheKey"/>.
		/// When unavailable, will fallback to use <paramref name="getter"/> to generate the value, storing it in the cache.
		/// </summary>
		/// <typeparam name="T">The type of value in the cache entry.</typeparam>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <param name="getter">The value generator when no cache entry is available.</param>
		/// <param name="settings">Cache control settings.</param>
		/// <returns></returns>
		ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, Task<T>> getter, CacheSettings settings);
	}

	public interface IFlushableCacheStack : ICacheStack
	{
		/// <summary>
		/// Flushes all cache layers, removing every item from the cache.
		/// </summary>
		/// <remarks>
		/// Warning: Do not call this unless you understand the gravity of clearing all cache layers entirely.
		/// <br/>
		/// Additionally, when using the `RedisRemoteEvictionExtension`, the flushing of caches is co-ordinated across all instances.
		/// </remarks>
		/// <returns></returns>
		ValueTask FlushAsync();
	}

	public interface ICacheStack<out TContext> : ICacheStack
	{
		/// <summary>
		/// Attempts to retrieve the value for the given <paramref name="cacheKey"/>.
		/// When unavailable, will fallback to use <paramref name="getter"/> to generate the value, storing it in the cache.
		/// </summary>
		/// <remarks>Additionally provides access to a context object, allowing easier access to inject dependencies.</remarks>
		/// <typeparam name="T">The type of value in the cache entry.</typeparam>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <param name="getter">The value generator when no cache entry is available.</param>
		/// <param name="settings">Cache control settings.</param>
		/// <returns></returns>
		ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, TContext, Task<T>> getter, CacheSettings settings);
	}
}
