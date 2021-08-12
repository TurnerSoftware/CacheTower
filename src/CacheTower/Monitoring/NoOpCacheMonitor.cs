using System;
using System.Threading.Tasks;

namespace CacheTower.Monitoring
{
	public class NoOpCacheMonitor : ICacheMonitor
	{
		public Task Evict(string layerName, string cacheKey, TimeSpan timeTaken)
		{
			return Task.CompletedTask;
		}

		public Task Set(string layerName, string cacheKey, TimeSpan timeTaken)
		{
			return Task.CompletedTask;
		}

		public Task GetHit(string layerName, string cacheKey, TimeSpan timeTaken)
		{
			return Task.CompletedTask;
		}

		public Task GetMiss(string layerName, string cacheKey, TimeSpan timeTaken)
		{
			return Task.CompletedTask;
		}

		public Task RefreshForeground(string cacheKey, TimeSpan timeTaken)
		{
			return Task.CompletedTask;
		}

		public Task RefreshBackground(string cacheKey, TimeSpan timeTaken)
		{
			return Task.CompletedTask;
		}

		public Task BackPopulate(string cacheKey, TimeSpan timeTaken)
		{
			return Task.CompletedTask;
		}
	}
}