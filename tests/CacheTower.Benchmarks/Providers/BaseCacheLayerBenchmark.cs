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
				for (var i = 0; i < WorkIterations; i++)
				{
					syncCacheLayer.Get<int>("GetMiss");
				}
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await asyncLayer.GetAsync<int>("GetMiss");
				}
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task GetHit()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				syncCacheLayer.Set("GetHit", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				for (var i = 0; i < WorkIterations; i++)
				{
					syncCacheLayer.Get<int>("GetHit");
				}
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				await asyncLayer.SetAsync("GetHit", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				for (var i = 0; i < WorkIterations; i++)
				{
					await asyncLayer.GetAsync<int>("GetHit");
				}
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task SetExisting()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				syncCacheLayer.Set("SetExisting", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				for (var i = 0; i < WorkIterations; i++)
				{
					syncCacheLayer.Set("SetExisting", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				}
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				await asyncLayer.SetAsync("SetExisting", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				for (var i = 0; i < WorkIterations; i++)
				{
					await asyncLayer.SetAsync("SetExisting", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
				}
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task EvictMiss()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					syncCacheLayer.Evict("EvictMiss");
				}
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await asyncLayer.EvictAsync("EvictMiss");
				}
			}
			await DisposeOf(cacheLayer);
		}

		[Benchmark]
		public async Task EvictHit()
		{
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					syncCacheLayer.Set("EvictHit", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
					syncCacheLayer.Evict("EvictHit");
				}
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await asyncLayer.SetAsync("EvictHit", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
					await asyncLayer.EvictAsync("EvictHit");
				}
			}
			await DisposeOf(cacheLayer);
		}



		[Benchmark]
		public async Task Cleanup()
		{
			var expiredDate = DateTime.UtcNow.AddDays(-1);
			var cacheLayer = CacheLayerProvider.Invoke();
			if (cacheLayer is ISyncCacheLayer syncCacheLayer)
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					syncCacheLayer.Set($"Cleanup_{i}", new CacheEntry<int>(1, expiredDate));
				}
				syncCacheLayer.Cleanup();
			}
			else if (cacheLayer is IAsyncCacheLayer asyncLayer)
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await asyncLayer.SetAsync($"Cleanup_{i}", new CacheEntry<int>(1, expiredDate));
				}
				await asyncLayer.CleanupAsync();
			}
			await DisposeOf(cacheLayer);
		}
	}
}
