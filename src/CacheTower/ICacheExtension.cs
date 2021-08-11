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
		/// <param name="updateType">The type of cache update that has occurred.</param>
		/// <returns></returns>
		ValueTask OnCacheUpdateAsync(string cacheKey, DateTime expiry, CacheUpdateType updateType);
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
	/// <summary>
	/// Describes the type of cache update that the cache stack experienced.
	/// </summary>
	/// <remarks>
	/// When you set a new cache entry, it isn't always known what the state of the cache currently is.
	/// Calling <c>SetAsync</c> doesn't check if it already exists.
	/// Calling <c>GetOrSetAsync</c> however is required to do such a check so this state can be passed along.
	/// </remarks>
	public enum CacheUpdateType
	{
		/// <summary>
		/// When the state of an existing cache entry is unknown, a cache update could 
		/// be triggered from adding a new entry or updating an existing one.
		/// </summary>
		AddOrUpdateEntry,
		/// <summary>
		/// When the state of an existing cache entry is known to not exist, a cache 
		/// update is triggered specifically with the adding of an entry.
		/// </summary>
		AddEntry
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
