using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using CacheTower.Providers.Memory;
using Perfolizer.Horology;

namespace CacheTower.Benchmarks
{
	[Config(typeof(ConfigSettings))]
	public class CacheStackBenchmark
	{
		[Params(100)]
		public int WorkIterations { get; set; }

		public class ConfigSettings : ManualConfig
		{
			public ConfigSettings()
			{
				AddJob(Job.Default.WithRuntime(CoreRuntime.Core50).WithMaxIterationCount(50));
				AddDiagnoser(MemoryDiagnoser.Default);

				SummaryStyle = new BenchmarkDotNet.Reports.SummaryStyle(CultureInfo, true, SizeUnit.B, TimeUnit.Nanosecond);
			}
		}

		[Benchmark]
		public async Task Set()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.SetAsync("Set", 15, TimeSpan.FromDays(1));
				}
			}
		}
		[Benchmark]
		public async Task Set_TwoLayers()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer(), new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.SetAsync("Set", 15, TimeSpan.FromDays(1));
				}
			}
		}
		[Benchmark]
		public async Task Evict()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.SetAsync("Evict", 15, TimeSpan.FromDays(1));
					await cacheStack.EvictAsync("Evict");
				}
			}
		}
		[Benchmark]
		public async Task Evict_TwoLayers()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer(), new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.SetAsync("Evict", 15, TimeSpan.FromDays(1));
					await cacheStack.EvictAsync("Evict");
				}
			}
		}
		[Benchmark]
		public async Task Cleanup()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.SetAsync($"Cleanup_{i}", 15, TimeSpan.FromDays(1));
				}

				await cacheStack.CleanupAsync();
			}
		}
		[Benchmark]
		public async Task Cleanup_TwoLayers()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer(), new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.SetAsync("Cleanup", 15, TimeSpan.FromDays(1));
					await cacheStack.CleanupAsync();
				}
			}
		}
		[Benchmark]
		public async Task GetMiss()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.GetAsync<int>("GetMiss");
				}
			}
		}
		[Benchmark]
		public async Task GetHit()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				await cacheStack.SetAsync("GetHit", 15, TimeSpan.FromDays(1));

				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.GetAsync<int>("GetHit");
				}
			}
		}
		[Benchmark]
		public async Task GetOrSet_NeverStale()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.GetOrSetAsync<int>("GetOrSet", (old) =>
					{
						return Task.FromResult(12);
					}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
				}
			}
		}
		[Benchmark]
		public async Task GetOrSet_AlwaysStale()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				for (var i = 0; i < WorkIterations; i++)
				{
					await cacheStack.GetOrSetAsync<int>("GetOrSet", (old) =>
					{
						return Task.FromResult(12);
					}, new CacheSettings(TimeSpan.FromDays(1)));
				}
			}
		}
		[Benchmark]
		public async Task GetOrSet_UnderLoad()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				await cacheStack.SetAsync("GetOrSet", new CacheEntry<int>(15, DateTime.UtcNow.AddDays(-1)));

				Parallel.For(0, WorkIterations, async value =>
				{
					await cacheStack.GetOrSetAsync<int>("GetOrSet", async (old) =>
					{
						await Task.Delay(30);
						return 12;
					}, new CacheSettings(TimeSpan.FromDays(1)));
				});
			}
		}
	}
}
