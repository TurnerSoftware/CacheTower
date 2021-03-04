using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheManager.Core;
using CacheTower.Providers.Memory;
using EasyCaching.InMemory;
using LazyCache;
using ZiggyCreatures.Caching.Fusion;

namespace CacheTower.AlternativesBenchmark
{
	public class CacheAlternatives_Memory_Benchmark : BaseBenchmark
	{
		[Params(1, 1000)]
		public int Iterations;

		[Benchmark(Baseline = true)]
		public async Task CacheTower_MemoryCacheLayer()
		{
			await using (var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				await LoopActionAsync(Iterations, async () =>
				{
					await cacheStack.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
					await cacheStack.GetAsync<int>("TestKey");
					await cacheStack.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
					{
						return Task.FromResult("Hello World");
					}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
				});
			}
		}

		[Benchmark]
		public void CacheManager_MicrosoftMemoryCache()
		{
			var cacheManager = CacheFactory.Build(b =>
			{
				b.WithMicrosoftMemoryCacheHandle();
			});

			using (cacheManager)
			{
				LoopAction(Iterations, () =>
				{
					cacheManager.Add("TestKey", 123);
					cacheManager.GetCacheItem("TestKey");
					cacheManager.GetOrAdd("GetOrSet_TestKey", (key) =>
					{
						return new CacheItem<string>(key, "Hello World");
					});
				});
			}
		}

		[Benchmark]
		public void EasyCaching_InMemory()
		{
			var easyCaching = new DefaultInMemoryCachingProvider(
				"EasyCaching",
				new[] { new InMemoryCaching("EasyCaching", new InMemoryCachingOptions()) },
				new InMemoryOptions()
			);

			LoopAction(Iterations, () =>
			{
				easyCaching.Set("TestKey", 123, TimeSpan.FromDays(1));
				easyCaching.Get<int>("TestKey");
				easyCaching.Get("GetOrSet_TestKey", () => "Hello World", TimeSpan.FromDays(1));
			});
		}

		[Benchmark]
		public void LazyCache_MemoryProvider()
		{
			var lazyCache = new CachingService();

			LoopAction(Iterations, () =>
			{
				lazyCache.Add("TestKey", 123, TimeSpan.FromDays(1));
				lazyCache.Get<int>("TestKey");
				lazyCache.GetOrAdd("GetOrSet_TestKey", () => "Hello World", TimeSpan.FromDays(1));
			});
		}

		[Benchmark]
		public void FusionCache_MemoryProvider()
		{
			var fusionCache = new FusionCache(new FusionCacheOptions());

			LoopAction(Iterations, () =>
			{
				fusionCache.Set("TestKey", 123, TimeSpan.FromDays(1));
				fusionCache.GetOrDefault<int>("TestKey");
				fusionCache.GetOrSet("GetOrSet_TestKey", (cancellationToken) => "Hello World", TimeSpan.FromDays(1));
			});
		}
	}
}
