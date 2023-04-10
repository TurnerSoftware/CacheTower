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
	public class CacheAlternatives_Memory_Benchmark : BaseBenchmark
	{
		private readonly CacheStack CacheTower;
		private readonly ICacheManager<string> CacheManager;
		private readonly DefaultInMemoryCachingProvider EasyCaching;
		private readonly CachingService LazyCache;
		private readonly FusionCache FusionCache;
		private readonly IntelligentHack.IntelligentCache.MemoryCache IntelligentCache;

		public CacheAlternatives_Memory_Benchmark()
		{
			CacheTower = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
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
			IntelligentCache = new IntelligentHack.IntelligentCache.MemoryCache("IntelligentCache");
		}

		[Benchmark(Baseline = true)]
		public async Task<string> CacheTower_MemoryCacheLayer()
		{
			return await CacheTower.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
			{
				return Task.FromResult("Hello World");
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
		}

		[Benchmark]
		public string CacheManager_MicrosoftMemoryCache()
		{
			return CacheManager.GetOrAdd("GetOrSet_TestKey", (key) =>
			{
				return new CacheItem<string>(key, "Hello World");
			}).Value;
		}

		[Benchmark]
		public string EasyCaching_InMemory()
		{
			return EasyCaching.Get("GetOrSet_TestKey", () => "Hello World", TimeSpan.FromDays(1)).Value;
		}

		[Benchmark]
		public string LazyCache_MemoryProvider()
		{
			return LazyCache.GetOrAdd("GetOrSet_TestKey", () => "Hello World", TimeSpan.FromDays(1));
		}

		[Benchmark]
		public string FusionCache_MemoryProvider()
		{
			return FusionCache.GetOrSet("GetOrSet_TestKey", (cancellationToken) => "Hello World", TimeSpan.FromDays(1));
		}

		[Benchmark]
		public string IntelligentCache_MemoryCache()
		{
			return IntelligentCache.GetSet("GetOrSet_TestKey", () => "Hello World", TimeSpan.FromDays(1));
		}
	}
}
