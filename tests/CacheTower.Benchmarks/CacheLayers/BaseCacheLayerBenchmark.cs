using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower.Benchmarks.CacheLayers
{
	public abstract class BaseCacheLayerBenchmark
	{
		protected async Task GetCacheBenchmark(ICacheLayer cacheLayer, int iterations)
		{
			await cacheLayer.Set("Get", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			for (var i = 0; i < iterations; i++)
			{
				await cacheLayer.Get<int>("Get");
			}
		}
		protected async Task SetCacheBenchmark(ICacheLayer cacheLayer, int iterations)
		{
			for (var i = 0; i < iterations; i++)
			{
				await cacheLayer.Set("Set", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
		}
	}
}
