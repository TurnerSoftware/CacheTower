using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower.Tests
{
	public abstract class BaseCacheLayerTests : TestBase
	{
		protected static async Task AssertGetSetCache(ICacheLayer cacheLayer)
		{
			var cacheEntry = new CacheEntry<int>(12, DateTime.UtcNow, TimeSpan.FromDays(1));
			await cacheLayer.Set("AssertGetSetCache", cacheEntry);
			var cacheEntryGet = await cacheLayer.Get<int>("AssertGetSetCache");

			Assert.AreEqual(cacheEntry, cacheEntryGet, "Set value in cache doesn't match retrieved value");
		}

		protected static async Task AssertCacheAvailability(ICacheLayer cacheLayer, bool expected)
		{
			Assert.AreEqual(expected, await cacheLayer.IsAvailable("AnyCacheKey-DoesntNeedToExist"));
		}

		protected static async Task AssertCacheEviction(ICacheLayer cacheLayer)
		{
			var cacheEntry = new CacheEntry<int>(77, DateTime.UtcNow, TimeSpan.FromDays(1));
			await cacheLayer.Set("AssertCacheEviction-ToEvict", cacheEntry);
			await cacheLayer.Set("AssertCacheEviction-ToKeep", cacheEntry);

			var cacheEntryGetPreEviction1 = await cacheLayer.Get<int>("AssertCacheEviction-ToEvict");
			var cacheEntryGetPreEviction2 = await cacheLayer.Get<int>("AssertCacheEviction-ToKeep");
			Assert.IsNotNull(cacheEntryGetPreEviction1, "Value not set in cache");
			Assert.IsNotNull(cacheEntryGetPreEviction2, "Value not set in cache");

			await cacheLayer.Evict("AssertCacheEviction-ToEvict");

			var cacheEntryGetPostEviction1 = await cacheLayer.Get<int>("AssertCacheEviction-ToEvict");
			var cacheEntryGetPostEviction2 = await cacheLayer.Get<int>("AssertCacheEviction-ToKeep");
			Assert.IsNull(cacheEntryGetPostEviction1, "Didn't evict value that should have been");
			Assert.IsNotNull(cacheEntryGetPostEviction2, "Evicted entry that should have been kept");
		}

		protected static async Task AssertCacheCleanup(ICacheLayer cacheLayer)
		{
			async Task<CacheEntry<int>> DoCleanupTest(TimeSpan timeToLive)
			{
				var cacheKey = $"AssertCacheCleanup-(TTL:{timeToLive})";

				var cacheEntry = new CacheEntry<int>(98, DateTime.UtcNow, timeToLive);
				await cacheLayer.Set(cacheKey, cacheEntry);

				var cacheEntryGetPreCleanup = await cacheLayer.Get<int>(cacheKey);
				Assert.AreEqual(cacheEntry, cacheEntryGetPreCleanup);

				await cacheLayer.Cleanup();

				return await cacheLayer.Get<int>(cacheKey);
			}

			Assert.IsNotNull(await DoCleanupTest(timeToLive: TimeSpan.FromDays(1)), "Cleanup removed entry that was still live");
			Assert.IsNull(await DoCleanupTest(timeToLive: TimeSpan.FromDays(-1)), "Cleanup kept entry past the end of life");
		}

		protected static async Task AssertComplexTypeCaching(ICacheLayer cacheLayer)
		{
			var complexTypeOneEntry = new CacheEntry<ComplexTypeCaching_TypeOne>(new ComplexTypeCaching_TypeOne
			{
				ExampleString = "Hello World",
				ExampleNumber = 99,
				ListOfNumbers = new List<int>() { 1, 2, 4, 8 }
			}, DateTime.UtcNow, TimeSpan.FromDays(1));
			await cacheLayer.Set("ComplexTypeOne", complexTypeOneEntry);
			var complexTypeOneEntryGet = await cacheLayer.Get<ComplexTypeCaching_TypeOne>("ComplexTypeOne");

			Assert.AreEqual(complexTypeOneEntry, complexTypeOneEntryGet, "Set value in cache doesn't match retrieved value");

			var complexTypeTwoEntry = new CacheEntry<ComplexTypeCaching_TypeTwo>(new ComplexTypeCaching_TypeTwo
			{
				ExampleString = "Hello World",
				ArrayOfObjects = new[] { complexTypeOneEntry.Value },
				DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "Z", 26 } }
			}, DateTime.UtcNow, TimeSpan.FromDays(1));
			await cacheLayer.Set("ComplexTypeTwo", complexTypeTwoEntry);
			var complexTypeTwoEntryGet = await cacheLayer.Get<ComplexTypeCaching_TypeTwo>("ComplexTypeTwo");

			Assert.AreEqual(complexTypeTwoEntry, complexTypeTwoEntryGet, "Set value in cache doesn't match retrieved value");
		}
	}
}
