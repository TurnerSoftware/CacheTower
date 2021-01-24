using System;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheExtension
	{
		void Register(ICacheStack cacheStack);
	}

	public interface ICacheChangeExtension : ICacheExtension
	{
		ValueTask OnCacheUpdateAsync(string cacheKey, DateTime expiry);
		ValueTask OnCacheEvictionAsync(string cacheKey);
		ValueTask OnCacheFlushAsync();
	}

	public interface ICacheRefreshCallSiteWrapperExtension : ICacheExtension
	{
		ValueTask<CacheEntry<T>> WithRefreshAsync<T>(string cacheKey, Func<ValueTask<CacheEntry<T>>> valueProvider, CacheSettings settings);
	}
}
