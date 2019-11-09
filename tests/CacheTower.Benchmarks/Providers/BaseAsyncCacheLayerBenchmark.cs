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

			await cacheLayer.SetAsync("GetHitSimultaneous", new CacheEntry<int>(1, TimeSpan.FromDays(1)));

			var tasks = new List<Task>();

			for (var i = 0; i < WorkIterations; i++)
			{
				var task = cacheLayer.GetAsync<int>("GetHitSimultaneous");
				tasks.Add(task);
			}

			await Task.WhenAll(tasks);

			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetExistingSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke() as IAsyncCacheLayer;

			await cacheLayer.SetAsync("SetExistingSimultaneous", new CacheEntry<int>(1, TimeSpan.FromDays(1)));

			var tasks = new List<Task>();

			for (var i = 0; i < WorkIterations; i++)
			{
				var task = cacheLayer.SetAsync("SetExistingSimultaneous", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				tasks.Add(task);
			}

			await Task.WhenAll(tasks);

			await DisposeOf(cacheLayer);
		}
	}
}
