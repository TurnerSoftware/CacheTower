using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Akavache;
using BenchmarkDotNet.Attributes;
using CacheManager.Core;
using CacheTower.Providers.Memory;
using EasyCaching.InMemory;
using Microsoft.Extensions.Caching.Memory;

namespace CacheTower.AlternativesBenchmark
{
	public class CacheAlternatives_Memory_Benchmark : BaseBenchmark
	{
		[Params(1, 100, 1000)]
		public int Iterations;

		[Benchmark(Baseline = true)]
		public async Task CacheTower_MemoryCacheLayer_ViaCacheStack()
		{
			await using (var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>()))
			{
				await LoopActionAsync(Iterations, async () =>
				{
					await cacheStack.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
					await cacheStack.GetAsync<int>("TestKey");
					await cacheStack.GetOrSetAsync<string>("GetOrSet_TestKey", (old, context) =>
					{
						return Task.FromResult("Hello World");
					}, new CacheSettings(TimeSpan.FromDays(1)));
				});
			}
		}

		[Benchmark]
		public void CacheTower_MemoryCacheLayer_Direct()
		{
			using (var layer = new MemoryCacheLayer())
			{
				LoopAction(Iterations, () =>
				{
					layer.Set("TestKey", new CacheEntry<int>(123, DateTime.UtcNow, TimeSpan.FromDays(1)));
					layer.Get<int>("TestKey");

					var getOrSetResult = layer.Get<string>("GetOrSet_TestKey");
					if (getOrSetResult == null)
					{
						layer.Set("GetOrSet_TestKey", new CacheEntry<string>("Hello World", DateTime.UtcNow, TimeSpan.FromDays(1)));
					}
				});
			}
		}

		[IterationSetup(Target = nameof(CacheManager_DictionaryHandle))]
		public void CacheManager_DictionaryHandle_Setup()
		{
			//Something is very weird with the dictionary handle, seems to be leaking memory
			//This method (though blank) forces just the CacheManager DictionaryHandle to only have 1 iteration
			//This should prevent issues and allow it to actually complete a test run
		}

		[Benchmark]
		public void CacheManager_DictionaryHandle()
		{
			var cacheManager = CacheFactory.Build(b =>
			{
				b.WithDictionaryHandle();
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
		public void Akavache_InMemory()
		{
			using (var blobCache = new InMemoryBlobCache())
			{
				LoopAction(Iterations, () =>
				{
					blobCache.InsertObject("TestKey", 123, TimeSpan.FromDays(1));
					blobCache.GetObject<int>("TestKey");
					blobCache.GetOrCreateObject("GetOrSet_TestKey", () => "Hello World");
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
	}
}
