using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;

namespace CacheTower.Benchmarks.Providers
{
	[Config(typeof(ConfigSettings))]
	public abstract class BaseCacheLayerBenchmark
	{
		public class ConfigSettings : ManualConfig
		{
			public ConfigSettings()
			{
				Add(Job.Default.With(CoreRuntime.Core30).WithMaxIterationCount(200));
				Add(MemoryDiagnoser.Default);
				
				SummaryStyle = new BenchmarkDotNet.Reports.SummaryStyle(true, SizeUnit.B, TimeUnit.Nanosecond);
			}

		}
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
		public async Task Overhead()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task GetMiss()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				syncCacheLayer.Get<int>("GetMiss");
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				await asyncLayer.GetAsync<int>("GetMiss");
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task GetHit()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				syncCacheLayer.Get<int>("GetMiss");
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				await asyncLayer.SetAsync("GetHit", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
				await asyncLayer.GetAsync<int>("GetHit");
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetNew()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				syncCacheLayer.Set("SetNew", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				await asyncLayer.SetAsync("SetNew", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
			await DisposeOf(cacheLayer);
		}
		[Benchmark]
		public async Task SetExisting()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				syncCacheLayer.Set("SetExisting", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
				syncCacheLayer.Set("SetExisting", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				await asyncLayer.SetAsync("SetExisting", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
				await asyncLayer.SetAsync("SetExisting", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetMany()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				for (var i = 0; i < 100; i++)
				{
					syncCacheLayer.Set("SetMany_" + i, new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
				}
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				for (var i = 0; i < 100; i++)
				{
					await asyncLayer.SetAsync("SetMany_" + i, new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
				}
			}
			await DisposeOf(cacheLayer);
		}
	}
}
