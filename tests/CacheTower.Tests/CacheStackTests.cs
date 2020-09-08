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
			new CacheStack(null, Array.Empty<ICacheExtension>());
		}
		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void ConstructorThrowsOnEmptyCacheLayer()
		{
			new CacheStack(Array.Empty<ICacheLayer>(), Array.Empty<ICacheExtension>());
		}
		[TestMethod]
		public async Task ConstructorAllowsNullExtensions()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await DisposeOf(cacheStack);
		}

		[TestMethod]
		public async Task Cleanup_CleansAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			
			var cacheStack = new CacheStack(new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());

			var cacheEntry = new CacheEntry<int>(42, DateTime.UtcNow.AddDays(-1));
			await cacheStack.SetAsync("Cleanup_CleansAllTheLayers", cacheEntry);

			Assert.AreEqual(cacheEntry, layer1.Get<int>("Cleanup_CleansAllTheLayers"));
			Assert.AreEqual(cacheEntry, layer2.Get<int>("Cleanup_CleansAllTheLayers"));

			await cacheStack.CleanupAsync();

			Assert.IsNull(layer1.Get<int>("Cleanup_CleansAllTheLayers"));
			Assert.IsNull(layer2.Get<int>("Cleanup_CleansAllTheLayers"));

			await DisposeOf(cacheStack);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Cleanup_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await DisposeOf(cacheStack);

			await cacheStack.CleanupAsync();
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Evict_ThrowsOnNullKey()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.EvictAsync(null);
		}
		[TestMethod]
		public async Task Evict_EvictsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());
			var cacheEntry = await cacheStack.SetAsync("Evict_EvictsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, layer1.Get<int>("Evict_EvictsAllTheLayers"));
			Assert.AreEqual(cacheEntry, layer2.Get<int>("Evict_EvictsAllTheLayers"));

			await cacheStack.EvictAsync("Evict_EvictsAllTheLayers");

			Assert.IsNull(layer1.Get<int>("Evict_EvictsAllTheLayers"));
			Assert.IsNull(layer2.Get<int>("Evict_EvictsAllTheLayers"));

			await DisposeOf(cacheStack);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Evict_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await DisposeOf(cacheStack);

			await cacheStack.EvictAsync("KeyDoesntMatter");
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Get_ThrowsOnNullKey()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetAsync<int>(null);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Get_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await DisposeOf(cacheStack);

			await cacheStack.GetAsync<int>("KeyDoesntMatter");
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Set_ThrowsOnNullKey()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync(null, new CacheEntry<int>(1, TimeSpan.FromDays(1)));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Set_ThrowsOnNullCacheEntry()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync("MyCacheKey", (CacheEntry<int>)null);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Set_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await DisposeOf(cacheStack);

			await cacheStack.SetAsync("KeyDoesntMatter", 1, TimeSpan.FromDays(1));
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Set_ThrowsOnUseAfterDisposal_CacheEntry()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await DisposeOf(cacheStack);

			await cacheStack.SetAsync("KeyDoesntMatter", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task Set_SetsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			var cacheStack = new CacheStack(new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());
			var cacheEntry = await cacheStack.SetAsync("Set_SetsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, layer1.Get<int>("Set_SetsAllTheLayers"));
			Assert.AreEqual(cacheEntry, layer2.Get<int>("Set_SetsAllTheLayers"));

			await DisposeOf(cacheStack);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullKey()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetOrSetAsync<int>(null, (old) => Task.FromResult(5), new CacheEntryLifetime(TimeSpan.FromDays(1)));
		}
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullGetter()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetOrSetAsync<int>("MyCacheKey", null, new CacheEntryLifetime(TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task GetOrSet_CacheMiss()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
   			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheMiss", (oldValue) =>
			{
				return Task.FromResult(5);
			}, new CacheEntryLifetime(TimeSpan.FromDays(1)));

			Assert.AreEqual(5, result);

			await DisposeOf(cacheStack);
		}
		[TestMethod]
		public async Task GetOrSet_CacheHit()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync("GetOrSet_CacheHit", 17, TimeSpan.FromDays(2));

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHit", (oldValue) =>
			{
				return Task.FromResult(27);
			}, new CacheEntryLifetime(TimeSpan.FromDays(1)));

			Assert.AreEqual(17, result);

			await DisposeOf(cacheStack);
		}
		[TestMethod]
		public async Task GetOrSet_CacheHitBackgroundRefresh()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(17, DateTime.UtcNow.AddDays(1));
			await cacheStack.SetAsync("GetOrSet_CacheHitBackgroundRefresh", cacheEntry);

			var waitingOnBackgroundTask = new TaskCompletionSource<int>();

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHitBackgroundRefresh", (oldValue) =>
			{
				waitingOnBackgroundTask.TrySetResult(27);
				return Task.FromResult(27);
			}, new CacheEntryLifetime(TimeSpan.FromDays(2), TimeSpan.Zero));
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

			var cacheStack = new CacheStack(new[] { layer1, layer2, layer3 }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(42, TimeSpan.FromDays(1));
			layer2.Set("GetOrSet_BackPropagatesToEarlierCacheLayers", cacheEntry);

			var cacheEntryFromStack = await cacheStack.GetOrSetAsync<int>("GetOrSet_BackPropagatesToEarlierCacheLayers", (old) =>
			{
				return Task.FromResult(14);
			}, new CacheEntryLifetime(TimeSpan.FromDays(1), TimeSpan.FromMinutes(1)));

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
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(17, DateTime.UtcNow.AddDays(-1));
			await cacheStack.SetAsync("GetOrSet_CacheHitButAllowedStalePoint", cacheEntry);

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHitButAllowedStalePoint", (oldValue) =>
			{
				return Task.FromResult(27);
			}, new CacheEntryLifetime(TimeSpan.FromDays(1), TimeSpan.Zero));
			Assert.AreEqual(27, result);
			
			await DisposeOf(cacheStack);
		}
		[TestMethod]
		public async Task GetOrSet_ConcurrentStaleCacheHits()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(23, DateTime.UtcNow.AddDays(-2));
			await cacheStack.SetAsync("GetOrSet_ConcurrentStaleCacheHits", cacheEntry);

			var request1LockSource = new TaskCompletionSource<bool>();
			var request2StartLockSource = new TaskCompletionSource<bool>();

			//Request 1 gets the lock on the refresh and ends up being tied up due to the TaskCompletionSource
			var request1Task = cacheStack.GetOrSetAsync<int>("GetOrSet_ConcurrentStaleCacheHits", async (oldValue) =>
			{
				request2StartLockSource.SetResult(true);
				await request1LockSource.Task;
				return 99;
			}, new CacheEntryLifetime(TimeSpan.FromDays(2), TimeSpan.Zero));

			await request2StartLockSource.Task;

			//Request 2 sees there is a lock already and because we still at least have old data, rather than wait
			//it is given the old cache data even though we are past the point where even stale data should be removed
			var request2Result = await cacheStack.GetOrSetAsync<int>("GetOrSet_ConcurrentStaleCacheHits", (oldValue) =>
			{
				return Task.FromResult(99);
			}, new CacheEntryLifetime(TimeSpan.FromDays(2), TimeSpan.Zero));

			//Unlock Request 1 to to continue
			request1LockSource.SetResult(true);
			//Wait for Request 1 to complete so we get the new data
			var request1Result = await request1Task;

			Assert.AreEqual(99, request1Result);
			Assert.AreEqual(23, request2Result);
			
			await DisposeOf(cacheStack);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task GetOrSet_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await DisposeOf(cacheStack);

			await cacheStack.GetOrSetAsync<int>("KeyDoesntMatter", (old) => Task.FromResult(1), new CacheEntryLifetime(TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task GetOrSet_WaitingForRefresh()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			var gettingLockSource = new TaskCompletionSource<bool>();
			var continueRefreshSource = new TaskCompletionSource<bool>();
 
			var cacheGetThatLocksOnRefresh = cacheStack.GetOrSetAsync<int>("GetOrSet_WaitingForRefresh", async (old) =>
			{
				gettingLockSource.SetResult(true);
				await continueRefreshSource.Task;
				return 42;
			}, new CacheEntryLifetime(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));

			await gettingLockSource.Task;

			var awaitingTasks = new List<Task<int>>();

			for (var i = 0; i < 3; i++)
			{
				var task = cacheStack.GetOrSetAsync<int>("GetOrSet_WaitingForRefresh", (old) =>
				{
					return Task.FromResult(99);
				}, new CacheEntryLifetime(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
				awaitingTasks.Add(task.AsTask());
			}

			continueRefreshSource.SetResult(true);

			Assert.AreEqual(42, await cacheGetThatLocksOnRefresh);
			Assert.AreEqual(42, await awaitingTasks[0]);
			Assert.AreEqual(42, await awaitingTasks[1]);
			Assert.AreEqual(42, await awaitingTasks[2]);

			await DisposeOf(cacheStack);
		}
	}
}
