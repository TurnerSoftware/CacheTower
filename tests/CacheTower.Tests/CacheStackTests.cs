using CacheTower.Providers.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests
{
	[TestClass]
	public class CacheStackTests : TestBase
	{
		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void ConstructorThrowsOnNullCacheLayer()
		{
			new CacheStack(null, null, Array.Empty<ICacheExtension>());
		}
		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void ConstructorThrowsOnEmptyCacheLayer()
		{
			new CacheStack(null, Array.Empty<ICacheLayer>(), Array.Empty<ICacheExtension>());
		}
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ConstructorThrowsOnNullExtensions()
		{
			new CacheStack(null, new[] { new MemoryCacheLayer() }, null);
		}

		[TestMethod]
		public async Task Cleanup_CleansAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			
			var cacheStack = new CacheStack(null, new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());

			var cacheEntry = new CacheEntry<int>(42, DateTime.UtcNow.AddDays(-2), TimeSpan.FromDays(1));
			await cacheStack.SetAsync("Cleanup_CleansAllTheLayers", cacheEntry);

			Assert.AreEqual(cacheEntry, layer1.Get<int>("Cleanup_CleansAllTheLayers"));
			Assert.AreEqual(cacheEntry, layer2.Get<int>("Cleanup_CleansAllTheLayers"));

			await cacheStack.CleanupAsync();

			Assert.IsNull(layer1.Get<int>("Cleanup_CleansAllTheLayers"));
			Assert.IsNull(layer2.Get<int>("Cleanup_CleansAllTheLayers"));

			await DisposeOf(cacheStack);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Evict_ThrowsOnNullKey()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.EvictAsync(null);
		}

		[TestMethod]
		public async Task Evict_EvictsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(null, new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());
			var cacheEntry = await cacheStack.SetAsync("Evict_EvictsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, layer1.Get<int>("Evict_EvictsAllTheLayers"));
			Assert.AreEqual(cacheEntry, layer2.Get<int>("Evict_EvictsAllTheLayers"));

			await cacheStack.EvictAsync("Evict_EvictsAllTheLayers");

			Assert.IsNull(layer1.Get<int>("Evict_EvictsAllTheLayers"));
			Assert.IsNull(layer2.Get<int>("Evict_EvictsAllTheLayers"));

			await DisposeOf(cacheStack);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Get_ThrowsOnNullKey()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetAsync<int>(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Set_ThrowsOnNullKey()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync(null, new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Set_ThrowsOnNullCacheEntry()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync("MyCacheKey", (CacheEntry<int>)null);
		}

		[TestMethod]
		public async Task Set_SetsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(null, new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());
			var cacheEntry = await cacheStack.SetAsync("Set_SetsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, layer1.Get<int>("Set_SetsAllTheLayers"));
			Assert.AreEqual(cacheEntry, layer2.Get<int>("Set_SetsAllTheLayers"));

			await DisposeOf(cacheStack);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullKey()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetOrSetAsync<int>(null, (old, context) => Task.FromResult(5), new CacheSettings(TimeSpan.FromDays(1)));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullGetter()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetOrSetAsync<int>("MyCacheKey", null, new CacheSettings(TimeSpan.FromDays(1)));
		}

		[TestMethod]
		public async Task GetOrSet_CacheMiss()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
   			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheMiss", (oldValue, context) =>
			{
				return Task.FromResult(5);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(5, result);

			await DisposeOf(cacheStack);
		}

		[TestMethod]
		public async Task GetOrSet_CacheHit()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync("GetOrSet_CacheHit", 17, TimeSpan.FromDays(2));

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHit", (oldValue, context) =>
			{
				return Task.FromResult(27);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(17, result);

			await DisposeOf(cacheStack);
		}

		[TestMethod]
		public async Task GetOrSet_CacheHitBackgroundRefresh()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(17, DateTime.UtcNow.AddDays(-1), TimeSpan.FromDays(2));
			await cacheStack.SetAsync("GetOrSet_CacheHitBackgroundRefresh", cacheEntry);

			var waitingOnBackgroundTask = new TaskCompletionSource<int>();

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHitBackgroundRefresh", (oldValue, context) =>
			{
				waitingOnBackgroundTask.TrySetResult(27);
				return Task.FromResult(27);
			}, new CacheSettings(TimeSpan.FromDays(2), TimeSpan.Zero));
			Assert.AreEqual(17, result);

			await waitingOnBackgroundTask.Task;
			//Give 400ms to return the value and set it to the MemoryCacheLayer
			await Task.Delay(400);

			var refetchedResult = await cacheStack.GetAsync<int>("GetOrSet_CacheHitBackgroundRefresh");
			Assert.AreEqual(27, refetchedResult.Value);

			await DisposeOf(cacheStack);
		}

		[TestMethod]
		public async Task GetOrSet_BackPropagatesToEarlierCacheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			var layer3 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(null, new[] { layer1, layer2, layer3 }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(42, DateTime.UtcNow, TimeSpan.FromDays(1));
			layer2.Set("GetOrSet_BackPropagatesToEarlierCacheLayers", cacheEntry);

			var cacheEntryFromStack = await cacheStack.GetOrSetAsync<int>("GetOrSet_BackPropagatesToEarlierCacheLayers", (old, context) =>
			{
				return Task.FromResult(14);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(cacheEntry.Value, cacheEntryFromStack);

			//Give enough time for the background task back propagation to happen
			await Task.Delay(2000);

			Assert.AreEqual(cacheEntry, layer1.Get<int>("GetOrSet_BackPropagatesToEarlierCacheLayers"));
			Assert.IsNull(layer3.Get<int>("GetOrSet_BackPropagatesToEarlierCacheLayers"));

			await DisposeOf(cacheStack);
		}

		[TestMethod]
		public async Task GetOrSet_CacheHitButAllowedStalePoint()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(17, DateTime.UtcNow.AddDays(-2), TimeSpan.FromDays(1));
			await cacheStack.SetAsync("GetOrSet_CacheHitButAllowedStalePoint", cacheEntry);

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHitButAllowedStalePoint", (oldValue, context) =>
			{
				return Task.FromResult(27);
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.Zero));
			Assert.AreEqual(27, result);
			
			await DisposeOf(cacheStack);
		}

		[TestMethod]
		public async Task GetOrSet_ConcurrentStaleCacheHits()
		{
			var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(23, DateTime.UtcNow.AddDays(-3), TimeSpan.FromDays(1));
			await cacheStack.SetAsync("GetOrSet_ConcurrentStaleCacheHits", cacheEntry);

			Task<int> DoRequest()
			{
				return cacheStack.GetOrSetAsync<int>("GetOrSet_ConcurrentStaleCacheHits", async (oldValue, context) =>
				{
					await Task.Delay(2000);
					return 99;
				}, new CacheSettings(TimeSpan.FromDays(2), TimeSpan.Zero));
			}

			//Request 1 gets the lock on the refresh and ends up being tied up due to the Task.Delay(1000) above
			var request1Task = DoRequest();

			await Task.Delay(1000);

			//Request 2 sees there is a lock already and because we still at least have old data, rather than wait
			//it is given the old cache data even though we are past the point where even stale data should be removed
			var request2Result = await DoRequest();
			//We wait for Request 1 to complete so we can confirm it gets the newer data
			var request1Result = await request1Task;

			Assert.AreEqual(99, request1Result);
			Assert.AreEqual(23, request2Result);
			
			await DisposeOf(cacheStack);
		}
	}
}
