using System;
using System.Threading.Tasks;

namespace CacheTower
{
	/// <summary>
	/// An <see cref="ICacheExtension"/> provides a method of extending the behaviour of Cache Tower.
	/// </summary>
	public interface ICacheExtension
	{
		/// <summary>
		/// Registers the provided <paramref name="cacheStack"/> to the current cache extension.
		/// </summary>
		/// <param name="cacheStack">The cache stack you want to register.</param>
		void Register(ICacheStack cacheStack);
	}

	/// <remarks>
	/// An <see cref="ICacheChangeExtension"/> exposes events into the inner workings of a cache stack.
	/// </remarks>
	/// <inheritdoc/>
	public interface ICacheChangeExtension : ICacheExtension
	{
		/// <summary>
		/// Triggers after a cache entry has been updated.
		/// </summary>
		/// <param name="cacheKey">The cache key for the entry that was updated.</param>
		/// <param name="expiry">The new expiry date for the cache entry.</param>
		/// <returns></returns>
		ValueTask OnCacheUpdateAsync(string cacheKey, DateTime expiry);
		/// <summary>
		/// Triggers after a cache entry has been evicted.
		/// </summary>
		/// <param name="cacheKey">The cache key for the entry that was evicted.</param>
		/// <returns></returns>
		ValueTask OnCacheEvictionAsync(string cacheKey);
		/// <summary>
		/// Triggers after the cache stack is flushed.
		/// </summary>
		/// <returns></returns>
		ValueTask OnCacheFlushAsync();
	}

	/// <remarks>
	/// An <see cref="ICacheRefreshCallSiteWrapperExtension"/> allows control over the behavious of refreshing.
	/// This interface is used by the <c>RedisLockExtension</c> for providing a distributed lock.
	/// </remarks>
	/// <inheritdoc/>
	public interface ICacheRefreshCallSiteWrapperExtension : ICacheExtension
	{
		/// <summary>
		/// Triggered when the cache entry needs to be refreshed, allowing control over the refreshing behaviour.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cacheKey">The cache key for the entry that is to be refreshed.</param>
		/// <param name="valueProvider">A delegate that, when called, will return a refreshed value for the cache entry.</param>
		/// <param name="settings">The settings supplied for the cache refresh.</param>
		/// <returns>A cache entry for the given <paramref name="cacheKey"/>.</returns>
		ValueTask<CacheEntry<T>> WithRefreshAsync<T>(string cacheKey, Func<ValueTask<CacheEntry<T>>> valueProvider, CacheSettings settings);
	}
}
