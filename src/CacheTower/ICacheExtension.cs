using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheExtension
	{
		void Register(ICacheStack cacheStack);
	}

	public interface IValueRefreshExtension : ICacheExtension
	{
		ValueTask OnValueRefreshAsync(string cacheKey, TimeSpan timeToLive);
	}

	public interface IRefreshWrapperExtension : ICacheExtension
	{
		ValueTask<CacheEntry<T>> RefreshValueAsync<T>(string cacheKey, Func<ValueTask<CacheEntry<T>>> valueProvider, CacheSettings settings);
	}
}
