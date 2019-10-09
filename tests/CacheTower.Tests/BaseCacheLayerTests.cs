using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower.Tests
{
	public abstract class BaseCacheLayerTests
	{
		protected static async Task AssertGetSetCache(ICacheLayer cacheLayer)
		{
			var cacheEntry = new CacheEntry<int>(12, TimeSpan.FromDays(1));
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
			var cacheEntry = new CacheEntry<int>(77, TimeSpan.FromDays(1));
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
			async Task<CacheEntry<int>> DoCleanupTest(TimeSpan timeToLive, TimeSpan maxTimeStale)
			{
				var cacheKey = $"AssertCacheCleanup-(TTL:{timeToLive})-(MaxTimeStale:{maxTimeStale})";

				var cacheEntry = new CacheEntry<int>(98, timeToLive);
				await cacheLayer.Set(cacheKey, cacheEntry);

				var cacheEntryGetPreCleanup = await cacheLayer.Get<int>(cacheKey);
				Assert.AreEqual(cacheEntry, cacheEntryGetPreCleanup);

				await cacheLayer.Cleanup(maxTimeStale);

				return await cacheLayer.Get<int>(cacheKey);
			}

			Assert.IsNotNull(await DoCleanupTest(timeToLive: TimeSpan.FromDays(1), maxTimeStale: TimeSpan.Zero), "Cleanup removed entry where TTL is ahead of Max Time Stale");
			Assert.IsNull(await DoCleanupTest(timeToLive: TimeSpan.FromDays(-1), maxTimeStale: TimeSpan.Zero), "Cleanup kept entry where TTL is behind Max Time Stale");

			Assert.IsNotNull(await DoCleanupTest(timeToLive: TimeSpan.Zero, maxTimeStale: TimeSpan.FromDays(-1)), "Cleanup removed entry where TTL is ahead of Max Time Stale");
			Assert.IsNull(await DoCleanupTest(timeToLive: TimeSpan.Zero, maxTimeStale: TimeSpan.FromDays(1)), "Cleanup kept entry where TTL is behind Max Time Stale");
		}
	}
}
