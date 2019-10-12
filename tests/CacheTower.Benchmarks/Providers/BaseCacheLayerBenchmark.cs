using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;

namespace CacheTower.Benchmarks.Providers
{
	public abstract class BaseCacheLayerBenchmark
	{
		public class ConfigSettings : ManualConfig
		{
			public ConfigSettings()
			{
				Add(Job.Core.WithMaxIterationCount(200));
				Add(MemoryDiagnoser.Default);
				
				SummaryStyle = new BenchmarkDotNet.Reports.SummaryStyle(true, SizeUnit.B, TimeUnit.Nanosecond);
			}

		}
		protected Func<ICacheLayer> CacheLayerProvider { get; set; }

		private async Task DisposeOf(ICacheLayer cacheLayer)
		{
			if (cacheLayer is IDisposable disposableLayer)
			{
				disposableLayer.Dispose();
			}
			else if (cacheLayer is IAsyncDisposable asyncDisposableLayer)
			{
				await asyncDisposableLayer.DisposeAsync();
			}
		}

		[Benchmark]
		public async Task GetMiss()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.Get<int>("GetMiss");
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task GetHit()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.Set("GetHit", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			await cacheLayer.Get<int>("GetHit");
			await DisposeOf(cacheLayer);
		}
		[Benchmark]
		public async Task GetHitSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.Set("GetHitSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			var aTask = cacheLayer.Get<int>("GetHitSimultaneous");
			var bTask = cacheLayer.Get<int>("GetHitSimultaneous");

			await aTask;
			await bTask;
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetNew()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.Set("SetNew", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			await DisposeOf(cacheLayer);
		}
		[Benchmark]
		public async Task SetExisting()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.Set("SetExisting", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			await cacheLayer.Set("SetExisting", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			await DisposeOf(cacheLayer);
		}
		[Benchmark]
		public async Task SetExistingSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.Set("SetExistingSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			var aTask = cacheLayer.Set("SetExistingSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			var bTask = cacheLayer.Set("SetExistingSimultaneous", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));

			await aTask;
			await bTask;
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetMany()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			for (var i = 0; i < 100; i++)
			{
				await cacheLayer.Set("SetMany_" + i, new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
			await DisposeOf(cacheLayer);
		}
	}
}
