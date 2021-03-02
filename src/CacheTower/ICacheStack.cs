using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	/// <summary>
	/// An <see cref="ICacheStack"/> is the backbone for Cache Tower. It is the primary user-facing type for interacting with the cache.
	/// </summary>
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
		/// <returns>The item from the cache that corresponds to the given <paramref name="cacheKey"/>.</returns>
		ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, Task<T>> getter, CacheSettings settings);
	}

	/// <remarks>
	/// An <see cref="IFlushableCacheStack"/> exposes an extra method to completely clear all the data from a cache stack.
	/// This is intentionally exposed as a separate interface in an attempt to prevent developers from inadvertently clearing all their cache data in production.
	/// </remarks>
	/// <inheritdoc/>
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

	/// <remarks>
	/// An <see cref="ICacheStack{TContext}"/> provides an additional <c>GetOrSetAsync</c> method that passes in a context object.
	/// This context allows easier access to inject dependencies like database contexts or any other type needed to help generate the item to cache.
	/// </remarks>
	/// <typeparam name="TContext">The type of context that is passed during the cache entry generation process.</typeparam>
	/// <inheritdoc/>
	public interface ICacheStack<out TContext> : ICacheStack
	{
		/// <remarks>
		/// Additionally provides access to a context object during refreshing, allowing easier access to inject dependencies.
		/// </remarks>
		/// <inheritdoc cref="ICacheStack.GetOrSetAsync{T}(string, Func{T, Task{T}}, CacheSettings)"/>
		ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, TContext, Task<T>> getter, CacheSettings settings);
	}
}
