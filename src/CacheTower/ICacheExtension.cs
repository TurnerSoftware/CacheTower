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
		Task OnValueRefreshAsync(Guid stackId, string cacheKey, TimeSpan timeToLive);
	}

	public interface IExternalLockExtension : ICacheExtension
	{
		Task<IDisposable> LockAsync(Guid stackId, string cacheKey);
	}
}
