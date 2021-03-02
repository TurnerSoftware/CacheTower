using System.Threading.Tasks;

namespace CacheTower
{
	/// <summary>
	/// Cache layers represent individual types of caching solutions including in-memory, file-based and Redis.
	/// It is with cache layers that items are set, retrieved or evicted from the cache.
	/// </summary>
	public interface ICacheLayer
	{
		/// <summary>
		/// Flushes the cache layer, removing every item from the cache.
		/// </summary>
		/// <returns></returns>
		ValueTask FlushAsync();
		/// <summary>
		/// Triggers the cleanup of any cache entries that are expired.
		/// </summary>
		/// <returns></returns>
		ValueTask CleanupAsync();
		/// <summary>
		/// Removes an entry with the corresponding <paramref name="cacheKey"/> from the cache layer.
		/// </summary>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <returns></returns>
		ValueTask EvictAsync(string cacheKey);
		/// <summary>
		/// Retrieves the <see cref="CacheEntry{T}"/> for a given <paramref name="cacheKey"/>.
		/// </summary>
		/// <typeparam name="T">The type of value in the cache entry.</typeparam>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <returns>The existing cache entry or <c>null</c> if no entry is found.</returns>
		ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey);
		/// <summary>
		/// Caches <paramref name="cacheEntry"/> against the <paramref name="cacheKey"/>.
		/// </summary>
		/// <typeparam name="T">The type of value in the cache entry.</typeparam>
		/// <param name="cacheKey">The cache entry's key.</param>
		/// <param name="cacheEntry">The cache entry to store.</param>
		/// <returns></returns>
		ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry);
		/// <summary>
		/// Retrieves the current availability status of the cache layer.
		/// This is used by <see cref="CacheStack"/> to determine whether a value can even be cached at that moment in time.
		/// </summary>
		/// <param name="cacheKey"></param>
		/// <returns></returns>
		ValueTask<bool> IsAvailableAsync(string cacheKey);
	}
}
