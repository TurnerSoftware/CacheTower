using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheTower.Tests.Providers
{
	public abstract class BaseCacheLayerTests : TestBase
	{
		protected static async Task AssertGetSetCacheAsync(ICacheLayer cacheLayer)
		{
			var cacheEntry = new CacheEntry<int>(12, TimeSpan.FromDays(1));
			await cacheLayer.SetAsync("AssertGetSetCache", cacheEntry);
			var cacheEntryGet = await cacheLayer.GetAsync<int>("AssertGetSetCache");

			Assert.AreEqual(cacheEntry, cacheEntryGet, "Set value in cache doesn't match retrieved value");
		}

		protected static async Task AssertCacheAvailabilityAsync(ICacheLayer cacheLayer, bool expected)
		{
			Assert.AreEqual(expected, await cacheLayer.IsAvailableAsync("AnyCacheKey-DoesntNeedToExist"));
		}

		protected static async Task AssertCacheEvictionAsync(ICacheLayer cacheLayer)
		{
			var cacheEntry = new CacheEntry<int>(77, TimeSpan.FromDays(1));
			await cacheLayer.SetAsync("AssertCacheEviction-ToEvict", cacheEntry);
			await cacheLayer.SetAsync("AssertCacheEviction-ToKeep", cacheEntry);

			var cacheEntryGetPreEviction1 = await cacheLayer.GetAsync<int>("AssertCacheEviction-ToEvict");
			var cacheEntryGetPreEviction2 = await cacheLayer.GetAsync<int>("AssertCacheEviction-ToKeep");
			Assert.IsNotNull(cacheEntryGetPreEviction1, "Value not set in cache");
			Assert.IsNotNull(cacheEntryGetPreEviction2, "Value not set in cache");

			await cacheLayer.EvictAsync("AssertCacheEviction-ToEvict");

			var cacheEntryGetPostEviction1 = await cacheLayer.GetAsync<int>("AssertCacheEviction-ToEvict");
			var cacheEntryGetPostEviction2 = await cacheLayer.GetAsync<int>("AssertCacheEviction-ToKeep");
			Assert.IsNull(cacheEntryGetPostEviction1, "Didn't evict value that should have been");
			Assert.IsNotNull(cacheEntryGetPostEviction2, "Evicted entry that should have been kept");
		}

		protected static async Task AssertCacheCleanupAsync(ICacheLayer cacheLayer)
		{
			async Task<CacheEntry<int>> DoCleanupTest(DateTime dateTime)
			{
				var cacheKey = $"AssertCacheCleanup-(DateTime:{dateTime})";

				var cacheEntry = new CacheEntry<int>(98, dateTime);
				await cacheLayer.SetAsync(cacheKey, cacheEntry);

				await cacheLayer.CleanupAsync();

				return await cacheLayer.GetAsync<int>(cacheKey);
			}

			Assert.IsNotNull(await DoCleanupTest(DateTime.UtcNow.AddDays(1)), "Cleanup removed entry that was still live");
			Assert.IsNull(await DoCleanupTest(DateTime.UtcNow.AddDays(-1)), "Cleanup kept entry past the end of life");
		}

		protected static async Task AssertComplexTypeCachingAsync(ICacheLayer cacheLayer)
		{
			var complexTypeOneEntry = new CacheEntry<ComplexTypeCaching_TypeOne>(new ComplexTypeCaching_TypeOne
			{
				ExampleString = "Hello World",
				ExampleNumber = 99,
				ListOfNumbers = new List<int>() { 1, 2, 4, 8 }
			}, TimeSpan.FromDays(1));
			await cacheLayer.SetAsync("ComplexTypeOne", complexTypeOneEntry);
			var complexTypeOneEntryGet = await cacheLayer.GetAsync<ComplexTypeCaching_TypeOne>("ComplexTypeOne");

			Assert.AreEqual(complexTypeOneEntry, complexTypeOneEntryGet, "Set value in cache doesn't match retrieved value");

			var complexTypeTwoEntry = new CacheEntry<ComplexTypeCaching_TypeTwo>(new ComplexTypeCaching_TypeTwo
			{
				ExampleString = "Hello World",
				ArrayOfObjects = new[] { complexTypeOneEntry.Value },
				DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "Z", 26 } }
			}, TimeSpan.FromDays(1));
			await cacheLayer.SetAsync("ComplexTypeTwo", complexTypeTwoEntry);
			var complexTypeTwoEntryGet = await cacheLayer.GetAsync<ComplexTypeCaching_TypeTwo>("ComplexTypeTwo");

			Assert.AreEqual(complexTypeTwoEntry, complexTypeTwoEntryGet, "Set value in cache doesn't match retrieved value");
		}
	}
}
