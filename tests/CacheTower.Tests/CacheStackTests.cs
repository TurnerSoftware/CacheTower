using CacheTower.Providers.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;

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
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
		}

		[TestMethod]
		public async Task Cleanup_CleansAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			
			await using var cacheStack = new CacheStack(new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());

			var cacheEntry = new CacheEntry<int>(42, DateTime.UtcNow.AddDays(-1));
			await cacheStack.SetAsync("Cleanup_CleansAllTheLayers", cacheEntry);

			Assert.AreEqual(cacheEntry, await layer1.GetAsync<int>("Cleanup_CleansAllTheLayers"));
			Assert.AreEqual(cacheEntry, await layer2.GetAsync<int>("Cleanup_CleansAllTheLayers"));

			await cacheStack.CleanupAsync();

			Assert.IsNull(await layer1.GetAsync<int>("Cleanup_CleansAllTheLayers"));
			Assert.IsNull(await layer2.GetAsync<int>("Cleanup_CleansAllTheLayers"));
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Cleanup_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await using (cacheStack) { }

			await cacheStack.CleanupAsync();
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Evict_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.EvictAsync(null);
		}
		[TestMethod]
		public async Task Evict_EvictsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());
			var cacheEntry = await cacheStack.SetAsync("Evict_EvictsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, await layer1.GetAsync<int>("Evict_EvictsAllTheLayers"));
			Assert.AreEqual(cacheEntry, await layer2.GetAsync<int>("Evict_EvictsAllTheLayers"));

			await cacheStack.EvictAsync("Evict_EvictsAllTheLayers");

			Assert.IsNull(await layer1.GetAsync<int>("Evict_EvictsAllTheLayers"));
			Assert.IsNull(await layer2.GetAsync<int>("Evict_EvictsAllTheLayers"));
		}
		[TestMethod]
		public async Task Evict_TriggersCacheChangeExtension()
		{
			var mockExtension = new Mock<ICacheChangeExtension>();
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, new [] { mockExtension.Object });
			var cacheEntry = await cacheStack.SetAsync("Evict_TriggerCacheChangeExtension", 42, TimeSpan.FromDays(1));

			await cacheStack.EvictAsync("Evict_TriggerCacheChangeExtension");

			mockExtension.Verify(e => e.OnCacheEvictionAsync("Evict_TriggerCacheChangeExtension"), Times.Once);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Evict_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await using (cacheStack) { }

			await cacheStack.EvictAsync("KeyDoesntMatter");
		}


		[TestMethod]
		public async Task Flush_FlushesAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());
			var cacheEntry = await cacheStack.SetAsync("Flush_FlushesAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, await layer1.GetAsync<int>("Flush_FlushesAllTheLayers"));
			Assert.AreEqual(cacheEntry, await layer2.GetAsync<int>("Flush_FlushesAllTheLayers"));

			await cacheStack.FlushAsync();

			Assert.IsNull(await layer1.GetAsync<int>("Flush_FlushesAllTheLayers"));
			Assert.IsNull(await layer2.GetAsync<int>("Flush_FlushesAllTheLayers"));
		}
		[TestMethod]
		public async Task Flush_TriggersCacheChangeExtension()
		{
			var mockExtension = new Mock<ICacheChangeExtension>();
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, new[] { mockExtension.Object });

			await cacheStack.FlushAsync();

			mockExtension.Verify(e => e.OnCacheFlushAsync(), Times.Once);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Flush_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await using (cacheStack)
			{ }

			await cacheStack.FlushAsync();
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Get_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetAsync<int>(null);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Get_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await using (cacheStack) { }

			await cacheStack.GetAsync<int>("KeyDoesntMatter");
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Set_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync(null, new CacheEntry<int>(1, TimeSpan.FromDays(1)));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Set_ThrowsOnNullCacheEntry()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync("MyCacheKey", (CacheEntry<int>)null);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Set_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await using (cacheStack) { }

			await cacheStack.SetAsync("KeyDoesntMatter", 1, TimeSpan.FromDays(1));
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Set_ThrowsOnUseAfterDisposal_CacheEntry()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await using (cacheStack) { }

			await cacheStack.SetAsync("KeyDoesntMatter", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task Set_SetsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(new[] { layer1, layer2 }, Array.Empty<ICacheExtension>());
			var cacheEntry = await cacheStack.SetAsync("Set_SetsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, await layer1.GetAsync<int>("Set_SetsAllTheLayers"));
			Assert.AreEqual(cacheEntry, await layer2.GetAsync<int>("Set_SetsAllTheLayers"));
		}
		[TestMethod]
		public async Task Set_TriggersCacheChangeExtension()
		{
			var mockExtension = new Mock<ICacheChangeExtension>();
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, new[] { mockExtension.Object });
			var cacheEntry = await cacheStack.SetAsync("Set_TriggersCacheChangeExtension", 42, TimeSpan.FromDays(1));

			mockExtension.Verify(e => e.OnCacheUpdateAsync("Set_TriggersCacheChangeExtension", cacheEntry.Expiry), Times.Once);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetOrSetAsync<int>(null, (old) => Task.FromResult(5), new CacheSettings(TimeSpan.FromDays(1)));
		}
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullGetter()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetOrSetAsync<int>("MyCacheKey", null, new CacheSettings(TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task GetOrSet_CacheMiss()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
   			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheMiss", (oldValue) =>
			{
				return Task.FromResult(5);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(5, result);
		}
		[TestMethod]
		public async Task GetOrSet_CacheHit()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.SetAsync("GetOrSet_CacheHit", 17, TimeSpan.FromDays(2));

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHit", (oldValue) =>
			{
				return Task.FromResult(27);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(17, result);
		}
		[TestMethod]
		public async Task GetOrSet_StaleCacheHit()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(17, DateTime.UtcNow.AddDays(2));
			await cacheStack.SetAsync("GetOrSet_StaleCacheHit", cacheEntry);

			var refreshWaitSource = new TaskCompletionSource<bool>();

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_StaleCacheHit", (oldValue) =>
			{
				Assert.AreEqual(17, oldValue);
				refreshWaitSource.TrySetResult(true);
				return Task.FromResult(27);
			}, new CacheSettings(TimeSpan.FromDays(2), TimeSpan.Zero));
			Assert.AreEqual(17, result);

			await Task.WhenAny(refreshWaitSource.Task, Task.Delay(TimeSpan.FromSeconds(5)));

			var refetchedResult = await cacheStack.GetAsync<int>("GetOrSet_StaleCacheHit");
			Assert.AreEqual(27, refetchedResult.Value);
		}
		[TestMethod]
		public async Task GetOrSet_BackPropagatesToEarlierCacheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			var layer3 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(new[] { layer1, layer2, layer3 }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(42, TimeSpan.FromDays(1));
			await layer2.SetAsync("GetOrSet_BackPropagatesToEarlierCacheLayers", cacheEntry);

			var cacheEntryFromStack = await cacheStack.GetOrSetAsync<int>("GetOrSet_BackPropagatesToEarlierCacheLayers", (old) =>
			{
				return Task.FromResult(14);
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromMinutes(1)));

			Assert.AreEqual(cacheEntry.Value, cacheEntryFromStack);

			//Give enough time for the background task back propagation to happen
			await Task.Delay(2000);

			Assert.AreEqual(cacheEntry, await layer1.GetAsync<int>("GetOrSet_BackPropagatesToEarlierCacheLayers"));
			Assert.IsNull(await layer3.GetAsync<int>("GetOrSet_BackPropagatesToEarlierCacheLayers"));
		}
		[TestMethod]
		public async Task GetOrSet_ConcurrentStaleCacheHits_OnlyOneRefresh()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			var cacheEntry = new CacheEntry<int>(23, DateTime.UtcNow.AddDays(2));
			await cacheStack.SetAsync("GetOrSet_ConcurrentStaleCacheHits_OnlyOneRefresh", cacheEntry);

			var refreshWaitSource = new TaskCompletionSource<bool>();
			var getterCallCount = 0;

			Parallel.For(0, 100, async v =>
			{
				await cacheStack.GetOrSetAsync<int>(
					"GetOrSet_ConcurrentStaleCacheHits_OnlyOneRefresh",
					async _ =>
					{
						await Task.Delay(250);
						Interlocked.Increment(ref getterCallCount);
						refreshWaitSource.TrySetResult(true);
						return 27;
					},
					new CacheSettings(TimeSpan.FromDays(2), TimeSpan.Zero)
				);
			});

			await Task.WhenAny(refreshWaitSource.Task, Task.Delay(TimeSpan.FromSeconds(5)));

			Assert.AreEqual(1, getterCallCount);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task GetOrSet_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			await using (cacheStack) { }

			await cacheStack.GetOrSetAsync<int>("KeyDoesntMatter", (old) => Task.FromResult(1), new CacheSettings(TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task GetOrSet_WaitingForRefresh()
		{
			await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, null);
			var gettingLockSource = new TaskCompletionSource<bool>();
			var continueRefreshSource = new TaskCompletionSource<bool>();
 
			var cacheGetThatLocksOnRefresh = cacheStack.GetOrSetAsync<int>("GetOrSet_WaitingForRefresh", async (old) =>
			{
				gettingLockSource.SetResult(true);
				await continueRefreshSource.Task;
				return 42;
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));

			await gettingLockSource.Task;

			var awaitingTasks = new List<Task<int>>();

			for (var i = 0; i < 3; i++)
			{
				var task = cacheStack.GetOrSetAsync<int>("GetOrSet_WaitingForRefresh", (old) =>
				{
					return Task.FromResult(99);
				}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
				awaitingTasks.Add(task.AsTask());
			}

			continueRefreshSource.SetResult(true);

			Assert.AreEqual(42, await cacheGetThatLocksOnRefresh);
			Assert.AreEqual(42, await awaitingTasks[0]);
			Assert.AreEqual(42, await awaitingTasks[1]);
			Assert.AreEqual(42, await awaitingTasks[2]);
		}
	}
}
