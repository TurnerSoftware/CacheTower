using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests
{
	[TestClass]
	public class CacheStackTests
	{
		[TestMethod]
		public async Task Cleanup_CleansAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(null, new[] { layer1, layer2 });
			var cacheEntry = await cacheStack.Set("Cleanup_CleansAllTheLayers", 42, TimeSpan.FromDays(-1));

			Assert.AreEqual(cacheEntry, await layer1.Get<int>("Cleanup_CleansAllTheLayers"));
			Assert.AreEqual(cacheEntry, await layer2.Get<int>("Cleanup_CleansAllTheLayers"));

			await cacheStack.Cleanup(TimeSpan.Zero);

			Assert.IsNull(await layer1.Get<int>("Cleanup_CleansAllTheLayers"));
			Assert.IsNull(await layer2.Get<int>("Cleanup_CleansAllTheLayers"));
		}

		[TestMethod]
		public async Task Evict_EvictsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(null, new[] { layer1, layer2 });
			var cacheEntry = await cacheStack.Set("Evict_EvictsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, await layer1.Get<int>("Evict_EvictsAllTheLayers"));
			Assert.AreEqual(cacheEntry, await layer2.Get<int>("Evict_EvictsAllTheLayers"));

			await cacheStack.Evict("Evict_EvictsAllTheLayers");

			Assert.IsNull(await layer1.Get<int>("Evict_EvictsAllTheLayers"));
			Assert.IsNull(await layer2.Get<int>("Evict_EvictsAllTheLayers"));
		}

		[TestMethod]
		public async Task Set_SetsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(null, new[] { layer1, layer2 });
			var cacheEntry = await cacheStack.Set("Set_SetsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, await layer1.Get<int>("Set_SetsAllTheLayers"));
			Assert.AreEqual(cacheEntry, await layer2.Get<int>("Set_SetsAllTheLayers"));
		}

		[TestMethod]
		public async Task Get_BackPropagatesToEarlierCacheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			var layer3 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(null, new[] { layer1, layer2, layer3 });

			var cacheEntry = new CacheEntry<int>(42, TimeSpan.FromDays(1));
			await layer2.Set("Get_BackPropagatesToEarlierCacheLayers", cacheEntry);

			var cacheEntryFromStack = await cacheStack.Get<int>("Get_BackPropagatesToEarlierCacheLayers");
			Assert.AreEqual(cacheEntry, cacheEntryFromStack);
			Assert.AreEqual(cacheEntry, await layer1.Get<int>("Get_BackPropagatesToEarlierCacheLayers"));
			Assert.IsNull(await layer3.Get<int>("Get_BackPropagatesToEarlierCacheLayers"));
		}

		[TestMethod]
		public async Task GetOrSet_CacheMiss()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() });

			var result = await cacheStack.GetOrSet<int>("GetOrSet_CacheMiss", (oldValue, context) =>
			{
				return Task.FromResult(5);
			}, new CacheSettings { TimeToLive = TimeSpan.FromDays(1) });

			Assert.AreEqual(5, result);
		}

		[TestMethod]
		public async Task GetOrSet_CacheHit()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() });

			await cacheStack.Set("GetOrSet_CacheHit", 17, TimeSpan.FromDays(2));

			var result = await cacheStack.GetOrSet<int>("GetOrSet_CacheHit", (oldValue, context) =>
			{
				return Task.FromResult(27);
			}, new CacheSettings { TimeToLive = TimeSpan.FromDays(1) });

			Assert.AreEqual(17, result);
		}

		[TestMethod]
		public async Task GetOrSet_CacheHitBackgroundRefresh()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() });

			await cacheStack.Set("GetOrSet_CacheHitBackgroundRefresh", 17, TimeSpan.FromDays(1));

			var result = await cacheStack.GetOrSet<int>("GetOrSet_CacheHitBackgroundRefresh", (oldValue, context) =>
			{
				return Task.FromResult(27);
			}, new CacheSettings { TimeToLive = TimeSpan.FromDays(2) });
			Assert.AreEqual(17, result);

			await Task.Delay(TimeSpan.FromSeconds(1));

			var refetchedResult = await cacheStack.Get<int>("GetOrSet_CacheHitBackgroundRefresh");
			Assert.AreEqual(27, refetchedResult.Value);
		}

		[TestMethod]
		public async Task GetOrSet_CacheHitButAllowedStalePoint()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() });

			await cacheStack.Set("GetOrSet_CacheHitButAllowedStalePoint", 17, TimeSpan.FromDays(-1));

			var result = await cacheStack.GetOrSet<int>("GetOrSet_CacheHitButAllowedStalePoint", (oldValue, context) =>
			{
				return Task.FromResult(27);
			}, new CacheSettings { TimeToLive = TimeSpan.FromDays(1) });
			Assert.AreEqual(27, result);
		}

		[TestMethod]
		public async Task GetOrSet_ConcurrentStaleCacheHits()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() });

			await cacheStack.Set("GetOrSet_ConcurrentStaleCacheHits", 23, TimeSpan.FromDays(-1));

			Task<int> DoRequest()
			{
				return cacheStack.GetOrSet<int>("GetOrSet_ConcurrentStaleCacheHits", async (oldValue, context) =>
				{
					await Task.Delay(1000);
					return 99;
				}, new CacheSettings { TimeToLive = TimeSpan.FromDays(2) });
			}

			//Request 1 gets the lock on the refresh and ends up being tied up due to the Task.Delay(1000) above
			var request1Task = DoRequest();
			//Request 2 sees there is a lock already and because we still at least have old data, rather than wait
			//it is given the old cache data even though we are past the point where even stale data should be removed
			var request2Result = await DoRequest();
			//We wait for Request 1 to complete so we can confirm it gets the newer data
			var request1Result = await request1Task;

			Assert.AreEqual(99, request1Result);
			Assert.AreEqual(23, request2Result);
		}
	}
}
