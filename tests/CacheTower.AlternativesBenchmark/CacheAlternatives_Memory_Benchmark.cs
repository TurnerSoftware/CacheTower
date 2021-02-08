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
		public async Task CacheTower_MemoryCacheLayer_ViaCacheStack()
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
		public async ValueTask CacheTower_MemoryCacheLayer_Direct()
		{
			var layer = new MemoryCacheLayer();
			await LoopActionAsync(Iterations, async () =>
			{
				await layer.SetAsync("TestKey", new CacheEntry<int>(123, DateTime.UtcNow + TimeSpan.FromDays(1)));
				await layer.GetAsync<int>("TestKey");

				var getOrSetResult = await layer.GetAsync<string>("GetOrSet_TestKey");
				if (getOrSetResult == null)
				{
					await layer.SetAsync("GetOrSet_TestKey", new CacheEntry<string>("Hello World", TimeSpan.FromDays(1)));
				}
			});
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
		public async Task EasyCaching_InMemoryAsync()
		{
			var easyCaching = new DefaultInMemoryCachingProvider(
				"EasyCaching",
				new[] { new InMemoryCaching("EasyCaching", new InMemoryCachingOptions()) },
				new InMemoryOptions()
			);

			await LoopActionAsync(Iterations, async () =>
			{
				await easyCaching.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
				await easyCaching.GetAsync<int>("TestKey");
				await easyCaching.GetAsync("GetOrSet_TestKey", () => Task.FromResult("Hello World"), TimeSpan.FromDays(1));
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
				lazyCache.GetOrAdd("TestKey", () => "Hello World", TimeSpan.FromDays(1));
			});
		}

		[Benchmark]
		public async Task LazyCache_MemoryProviderAsync()
		{
			var lazyCache = new CachingService();

			await LoopActionAsync(Iterations, async () =>
			{
				lazyCache.Add("TestKey", 123, TimeSpan.FromDays(1));
				await lazyCache.GetAsync<int>("TestKey");
				await lazyCache.GetOrAddAsync("TestKey", () => Task.FromResult("Hello World"), TimeSpan.FromDays(1));
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
				fusionCache.GetOrSet("TestKey", (cancellationToken) => "Hello World", TimeSpan.FromDays(1));
			});
		}

		[Benchmark]
		public async Task FusionCache_MemoryProviderAsync()
		{
			var fusionCache = new FusionCache(new FusionCacheOptions());

			await LoopActionAsync(Iterations, async () =>
			{
				await fusionCache.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
				await fusionCache.GetOrDefaultAsync<int>("TestKey");
				await fusionCache.GetOrSetAsync("TestKey", (cancellationToken) => Task.FromResult("Hello World"), TimeSpan.FromDays(1));
			});
		}
	}
}
