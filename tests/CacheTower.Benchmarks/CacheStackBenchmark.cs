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
using CacheTower.Providers.Memory;

namespace CacheTower.Benchmarks
{
	[Config(typeof(ConfigSettings))]
	public class CacheStackBenchmark
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

		[Benchmark]
		public async Task SetupAndTeardown()
		{
			await using (new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{

			}
		}

		[Benchmark]
		public async Task GetMiss()
		{
			await using (var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				await cacheStack.GetAsync<int>("GetMiss");
			}
		}
		[Benchmark]
		public async Task GetHit()
		{
			await using (var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				await cacheStack.SetAsync("GetHit", 15, TimeSpan.FromDays(1));
				await cacheStack.GetAsync<int>("GetHit");
			}
		}
		[Benchmark]
		public async Task GetOrSet()
		{
			await using (var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				await cacheStack.SetAsync("GetOrSet", new CacheEntry<int>(15, DateTime.UtcNow.AddDays(-2), TimeSpan.FromDays(1)));
				await cacheStack.GetOrSetAsync<int>("GetOrSet", (old, context) =>
				{
					return Task.FromResult(12);
				}, new CacheSettings(TimeSpan.FromDays(1)));
			}
		}
	}
}
