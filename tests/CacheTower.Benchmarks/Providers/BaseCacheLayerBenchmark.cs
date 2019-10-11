using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.CacheLayers
{
	public abstract class BaseCacheLayerBenchmark
	{
		protected Func<ICacheLayer> CacheLayerProvider { get; set; }

		[Params(10, 1000)]
		public int Iterations;

		[Benchmark]
		public async Task GetMiss()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			for (var i = 0; i < Iterations; i++)
			{
				await cacheLayer.Get<int>("CacheMiss");
			}
		}

		[Benchmark]
		public async Task GetHit()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.Set("CacheHit", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			for (var i = 0; i < Iterations; i++)
			{
				await cacheLayer.Get<int>("CacheHit");
			}
		}
		[Benchmark]
		public async Task GetHitSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.Set("CacheHit", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			for (var i = 0; i < Iterations; i += 2)
			{
				var aTask = cacheLayer.Get<int>("CacheHit");
				var bTask = cacheLayer.Get<int>("CacheHit");

				await aTask;
				await bTask;
			}
		}

		[Benchmark]
		public async Task SetExisting()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			for (var i = 0; i < Iterations; i++)
			{
				await cacheLayer.Set("SetSame", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
		}
		[Benchmark]
		public async Task SetExistingSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			for (var i = 0; i < Iterations; i += 2)
			{
				var aTask = cacheLayer.Set("SetSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
				var bTask = cacheLayer.Set("SetSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

				await aTask;
				await bTask;
			}
		}

		[Benchmark]
		public async Task SetUnique()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			for (var i = 0; i < Iterations; i++)
			{
				await cacheLayer.Set("SetDifferent_" + i, new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
		}
	}
}
