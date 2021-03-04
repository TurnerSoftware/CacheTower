using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using Perfolizer.Horology;

namespace CacheTower.Benchmarks.Providers
{
	[Config(typeof(ConfigSettings))]
	public abstract class BaseCacheLayerBenchmark
	{
		public class ConfigSettings : ManualConfig
		{
			public ConfigSettings()
			{
				AddJob(Job.Default.WithRuntime(CoreRuntime.Core50).WithMaxIterationCount(200));
				AddDiagnoser(MemoryDiagnoser.Default);

				SummaryStyle = new BenchmarkDotNet.Reports.SummaryStyle(CultureInfo, true, SizeUnit.B, TimeUnit.Nanosecond);
			}
		}

		[Params(1, 100)]
		public int WorkIterations { get; set; }

		protected Func<ICacheLayer> CacheLayerProvider { get; set; }

		protected static async Task DisposeOf(ICacheLayer cacheLayer)
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
			for (var i = 0; i < WorkIterations; i++)
			{
				await cacheLayer.GetAsync<int>("GetMiss");
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task GetHit()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.SetAsync("GetHit", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
			for (var i = 0; i < WorkIterations; i++)
			{
				await cacheLayer.GetAsync<int>("GetHit");
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetExisting()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await cacheLayer.SetAsync("SetExisting", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
			for (var i = 0; i < WorkIterations; i++)
			{
				await cacheLayer.SetAsync("SetExisting", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task EvictMiss()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			for (var i = 0; i < WorkIterations; i++)
			{
				await cacheLayer.EvictAsync("EvictMiss");
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task EvictHit()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			for (var i = 0; i < WorkIterations; i++)
			{
				await cacheLayer.SetAsync("EvictHit", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				await cacheLayer.EvictAsync("EvictHit");
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task Cleanup()
		{
			var expiredDate = DateTime.UtcNow.AddDays(-1);
			var cacheLayer = CacheLayerProvider.Invoke();
			for (var i = 0; i < WorkIterations; i++)
			{
				await cacheLayer.SetAsync($"Cleanup_{i}", new CacheEntry<int>(1, expiredDate));
			}
			await cacheLayer.CleanupAsync();
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task GetHitSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke();

			await cacheLayer.SetAsync("GetHitSimultaneous", new CacheEntry<int>(1, TimeSpan.FromDays(1)));

			var tasks = new List<Task>();

			for (var i = 0; i < WorkIterations; i++)
			{
				var task = cacheLayer.GetAsync<int>("GetHitSimultaneous");
				tasks.Add(task.AsTask());
			}

			await Task.WhenAll(tasks);

			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetExistingSimultaneous()
		{
			var cacheLayer = CacheLayerProvider.Invoke();

			await cacheLayer.SetAsync("SetExistingSimultaneous", new CacheEntry<int>(1, TimeSpan.FromDays(1)));

			var tasks = new List<Task>();

			for (var i = 0; i < WorkIterations; i++)
			{
				var task = cacheLayer.SetAsync("SetExistingSimultaneous", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				tasks.Add(task.AsTask());
			}

			await Task.WhenAll(tasks);

			await DisposeOf(cacheLayer);
		}
	}
}
