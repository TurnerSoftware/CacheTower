using System;
using System.Threading.Tasks;

namespace CacheTower.Monitoring
{
	public interface ICacheMonitor
	{
		Task Evict(string layerName, string cacheKey, TimeSpan timeTaken);
		Task Set(string layerName, string cacheKey, TimeSpan timeTaken);
		Task GetHit(string layerName, string cacheKey, TimeSpan timeTaken);
		Task GetMiss(string layerName, string cacheKey, TimeSpan timeTaken);
		Task RefreshForeground(string cacheKey, TimeSpan timeTaken);
		Task RefreshBackground(string cacheKey, TimeSpan timeTaken);
		Task BackPopulate(string cacheKey, TimeSpan timeTaken);
	}
}
