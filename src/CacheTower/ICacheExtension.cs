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
		Task OnValueRefreshAsync(ICacheStack cacheStack, string cacheKey, TimeSpan timeToLive);
	}
}
