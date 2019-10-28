using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower.Tests.Providers
{
	public abstract class BaseCacheLayerTests : TestBase
	{
		protected static void AssertGetSetCache(ISyncCacheLayer cacheLayer)
		{
			var cacheEntry = new CacheEntry<int>(12, TimeSpan.FromDays(1));
			cacheLayer.Set("AssertGetSetCache", cacheEntry);
			var cacheEntryGet = cacheLayer.Get<int>("AssertGetSetCache");

			Assert.AreEqual(cacheEntry, cacheEntryGet, "Set value in cache doesn't match retrieved value");
		}
		protected static async Task AssertGetSetCacheAsync(IAsyncCacheLayer cacheLayer)
		{
			var cacheEntry = new CacheEntry<int>(12, TimeSpan.FromDays(1));
			await cacheLayer.SetAsync("AssertGetSetCache", cacheEntry);
			var cacheEntryGet = await cacheLayer.GetAsync<int>("AssertGetSetCache");

			Assert.AreEqual(cacheEntry, cacheEntryGet, "Set value in cache doesn't match retrieved value");
		}

		protected static void AssertCacheAvailability(ISyncCacheLayer cacheLayer, bool expected)
		{
			Assert.AreEqual(expected, cacheLayer.IsAvailable("AnyCacheKey-DoesntNeedToExist"));
		}
		protected static async Task AssertCacheAvailabilityAsync(IAsyncCacheLayer cacheLayer, bool expected)
		{
			Assert.AreEqual(expected, await cacheLayer.IsAvailableAsync("AnyCacheKey-DoesntNeedToExist"));
		}

		protected static void AssertCacheEviction(ISyncCacheLayer cacheLayer)
		{
			var cacheEntry = new CacheEntry<int>(77, TimeSpan.FromDays(1));
			cacheLayer.Set("AssertCacheEviction-ToEvict", cacheEntry);
			cacheLayer.Set("AssertCacheEviction-ToKeep", cacheEntry);

			var cacheEntryGetPreEviction1 = cacheLayer.Get<int>("AssertCacheEviction-ToEvict");
			var cacheEntryGetPreEviction2 = cacheLayer.Get<int>("AssertCacheEviction-ToKeep");
			Assert.IsNotNull(cacheEntryGetPreEviction1, "Value not set in cache");
			Assert.IsNotNull(cacheEntryGetPreEviction2, "Value not set in cache");

			cacheLayer.Evict("AssertCacheEviction-ToEvict");

			var cacheEntryGetPostEviction1 = cacheLayer.Get<int>("AssertCacheEviction-ToEvict");
			var cacheEntryGetPostEviction2 = cacheLayer.Get<int>("AssertCacheEviction-ToKeep");
			Assert.IsNull(cacheEntryGetPostEviction1, "Didn't evict value that should have been");
			Assert.IsNotNull(cacheEntryGetPostEviction2, "Evicted entry that should have been kept");
		}
		protected static async Task AssertCacheEvictionAsync(IAsyncCacheLayer cacheLayer)
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

		protected static void AssertCacheCleanup(ISyncCacheLayer cacheLayer)
		{
			CacheEntry<int> DoCleanupTest(DateTime dateTime)
			{
				var cacheKey = $"AssertCacheCleanup-(DateTime:{dateTime})";

				var cacheEntry = new CacheEntry<int>(98, dateTime);
				cacheLayer.Set(cacheKey, cacheEntry);

				cacheLayer.Cleanup();

				return cacheLayer.Get<int>(cacheKey);
			}

			Assert.IsNotNull(DoCleanupTest(DateTime.UtcNow.AddDays(1)), "Cleanup removed entry that was still live");
			Assert.IsNull(DoCleanupTest(DateTime.UtcNow.AddDays(-1)), "Cleanup kept entry past the end of life");
		}
		protected static async Task AssertCacheCleanupAsync(IAsyncCacheLayer cacheLayer)
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

		protected static void AssertComplexTypeCaching(ISyncCacheLayer cacheLayer)
		{
			var complexTypeOneEntry = new CacheEntry<ComplexTypeCaching_TypeOne>(new ComplexTypeCaching_TypeOne
			{
				ExampleString = "Hello World",
				ExampleNumber = 99,
				ListOfNumbers = new List<int>() { 1, 2, 4, 8 }
			}, TimeSpan.FromDays(1));
			cacheLayer.Set("ComplexTypeOne", complexTypeOneEntry);
			var complexTypeOneEntryGet = cacheLayer.Get<ComplexTypeCaching_TypeOne>("ComplexTypeOne");

			Assert.AreEqual(complexTypeOneEntry, complexTypeOneEntryGet, "Set value in cache doesn't match retrieved value");

			var complexTypeTwoEntry = new CacheEntry<ComplexTypeCaching_TypeTwo>(new ComplexTypeCaching_TypeTwo
			{
				ExampleString = "Hello World",
				ArrayOfObjects = new[] { complexTypeOneEntry.Value },
				DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "Z", 26 } }
			}, TimeSpan.FromDays(1));
			cacheLayer.Set("ComplexTypeTwo", complexTypeTwoEntry);
			var complexTypeTwoEntryGet = cacheLayer.Get<ComplexTypeCaching_TypeTwo>("ComplexTypeTwo");

			Assert.AreEqual(complexTypeTwoEntry, complexTypeTwoEntryGet, "Set value in cache doesn't match retrieved value");
		}
		protected static async Task AssertComplexTypeCachingAsync(IAsyncCacheLayer cacheLayer)
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
