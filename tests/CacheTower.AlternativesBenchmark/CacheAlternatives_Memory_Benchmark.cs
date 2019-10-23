using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Akavache;
using BenchmarkDotNet.Attributes;
using CacheManager.Core;
using CacheTower.Providers.Memory;
using Microsoft.Extensions.Caching.Memory;

namespace CacheTower.AlternativesBenchmark
{
	[CoreJob, MemoryDiagnoser]
	public class CacheAlternatives_Memory_Benchmark : BaseBenchmark
	{
		[Params(1, 100, 1000)]
		public int Iterations;

		[Benchmark(Baseline = true)]
		public async Task CacheTower_MemoryCacheLayer()
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
	}
}
