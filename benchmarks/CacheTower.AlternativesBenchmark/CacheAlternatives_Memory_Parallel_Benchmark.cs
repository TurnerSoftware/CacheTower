using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheManager.Core;
using CacheTower.Providers.Memory;
using EasyCaching.InMemory;
using LazyCache;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace CacheTower.AlternativesBenchmark
{
	public class CacheAlternatives_Memory_Parallel_Benchmark : BaseBenchmark
	{
		private readonly int ParallelIterations = 1000;

		private readonly CacheStack CacheTower;
		private readonly ICacheManager<string> CacheManager;
		private readonly DefaultInMemoryCachingProvider EasyCaching;
		private readonly CachingService LazyCache;
		private readonly FusionCache FusionCache;
		private readonly IntelligentHack.IntelligentCache.MemoryCache IntelligentCache;

		public CacheAlternatives_Memory_Parallel_Benchmark()
		{
			CacheTower = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			CacheManager = CacheFactory.Build<string>(b =>
			{
				b.WithMicrosoftMemoryCacheHandle();
			});
			EasyCaching = new DefaultInMemoryCachingProvider(
				"EasyCaching",
				new[] { new InMemoryCaching("EasyCaching", new InMemoryCachingOptions()) },
				new InMemoryOptions(),
				(ILoggerFactory)null
			);
			LazyCache = new CachingService();
			FusionCache = new FusionCache(new FusionCacheOptions());
			IntelligentCache = new IntelligentHack.IntelligentCache.MemoryCache(string.Empty);
		}

		[Benchmark(Baseline = true)]
		public void CacheTower_MemoryCacheLayer()
		{
			Parallel.For(0, ParallelIterations, async i =>
			{
				await CacheTower.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
				{
					return Task.FromResult("Hello World");
				}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
			});
		}

		[Benchmark]
		public void CacheManager_MicrosoftMemoryCache()
		{
			Parallel.For(0, ParallelIterations, i =>
			{
				var _ = CacheManager.GetOrAdd("GetOrSet_TestKey", (key) =>
				{
					return new CacheItem<string>(key, "Hello World");
				}).Value;
			});
		}

		[Benchmark]
		public void EasyCaching_InMemory()
		{
			Parallel.For(0, ParallelIterations, i =>
			{
				_ = EasyCaching.Get("GetOrSet_TestKey", () => "Hello World", TimeSpan.FromDays(1)).Value;
			});
		}

		[Benchmark]
		public void LazyCache_MemoryProvider()
		{
			Parallel.For(0, ParallelIterations, i =>
			{
				LazyCache.GetOrAdd("GetOrSet_TestKey", () => "Hello World", TimeSpan.FromDays(1));
			});
		}

		[Benchmark]
		public void FusionCache_MemoryProvider()
		{
			Parallel.For(0, ParallelIterations, i =>
			{
				FusionCache.GetOrSet("GetOrSet_TestKey", (cancellationToken) => "Hello World", TimeSpan.FromDays(1));
			});
		}

		[Benchmark]
		public void IntelligentCache_MemoryCache()
		{
			Parallel.For(0, ParallelIterations, i =>
			{
				IntelligentCache.GetSet("GetOrSet_TestKey", () => "Hello World", TimeSpan.FromDays(1));
			});
		}
	}
}
