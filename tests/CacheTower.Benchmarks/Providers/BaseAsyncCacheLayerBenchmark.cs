using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Providers
{
	public abstract class BaseAsyncCacheLayerBenchmark : BaseCacheLayerBenchmark
	{
		[Benchmark]
		public async Task GetHitSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke() as IAsyncCacheLayer;

			await cacheLayer.SetAsync("GetHitSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			var aTask = cacheLayer.GetAsync<int>("GetHitSimultaneous");
			var bTask = cacheLayer.GetAsync<int>("GetHitSimultaneous");

			await aTask;
			await bTask;

			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetExistingSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke() as IAsyncCacheLayer;

			await cacheLayer.SetAsync("SetExistingSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			var aTask = cacheLayer.SetAsync("SetExistingSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			var bTask = cacheLayer.SetAsync("SetExistingSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			await aTask;
			await bTask;

			await DisposeOf(cacheLayer);
		}
	}
}
