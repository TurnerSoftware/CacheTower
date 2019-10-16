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
		Task OnValueRefreshAsync(string stackId, string requestId, string cacheKey, TimeSpan timeToLive);
	}

	public interface IRefreshWrapperExtension : ICacheExtension
	{
		Task<CacheEntry<T>> RefreshValueAsync<T>(string stackId, string requestId, string cacheKey, Func<Task<CacheEntry<T>>> valueProvider, CacheSettings settings);
	}
}
