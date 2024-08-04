using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheTower.Providers.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace CacheTower.Tests
{
	[TestClass]
	public class CacheStackTests : TestBase
	{
		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void ConstructorThrowsOnNullCacheLayer()
		{
			new CacheStack(null, new(null));
		}
		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void ConstructorThrowsOnEmptyCacheLayer()
		{
			new CacheStack(null, new(Array.Empty<ICacheLayer>()));
		}
		[TestMethod]
		public async Task ConstructorAllowsNullExtensions()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
		}

		[TestMethod]
		public async Task Cleanup_CleansAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(null, new(new[] { layer1, layer2 }));

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
			var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await using (cacheStack)
			{ }

			await cacheStack.CleanupAsync();
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Evict_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.EvictAsync(null);
		}
		[TestMethod]
		public async Task Evict_EvictsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(null, new(new[] { layer1, layer2 }));
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
			var mockExtension = Substitute.For<ICacheChangeExtension>();
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }) { Extensions = new[] { mockExtension } });
			var cacheEntry = await cacheStack.SetAsync("Evict_TriggerCacheChangeExtension", 42, TimeSpan.FromDays(1));

			await cacheStack.EvictAsync("Evict_TriggerCacheChangeExtension");

			await mockExtension.Received(1).OnCacheEvictionAsync("Evict_TriggerCacheChangeExtension");
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Evict_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await using (cacheStack)
			{ }

			await cacheStack.EvictAsync("KeyDoesntMatter");
		}


		[TestMethod]
		public async Task Flush_FlushesAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(null, new(new[] { layer1, layer2 }));
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
			var mockExtension = Substitute.For<ICacheChangeExtension>();
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }) { Extensions = new[] { mockExtension } });

			await cacheStack.FlushAsync();

			await mockExtension.Received(1).OnCacheFlushAsync();
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Flush_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await using (cacheStack)
			{ }

			await cacheStack.FlushAsync();
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Get_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.GetAsync<int>(null);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Get_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await using (cacheStack)
			{ }

			await cacheStack.GetAsync<int>("KeyDoesntMatter");
		}
		[DataTestMethod, ExpectedException(typeof(ArgumentNullException))]
		[DataRow(true)]
		[DataRow(false)]
		public async Task Get_ThrowsOnNullKeyWithBackPopulation(bool enabled)
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.GetAsync<int>(null, enabled);
		}
		[TestMethod]
		public async Task Get_BackPopulatesToEarlierCacheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			var layer3 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(null, new(new[] { layer1, layer2, layer3 }));
			var cacheEntry = new CacheEntry<int>(42, TimeSpan.FromDays(1));
			await layer2.SetAsync("Get_BackPopulatesToEarlierCacheLayers", cacheEntry);

			Internal.DateTimeProvider.UpdateTime();

			var cacheEntryFromStack = await cacheStack.GetAsync<int>("Get_BackPopulatesToEarlierCacheLayers", true);

			Assert.IsNotNull(cacheEntryFromStack);
			Assert.AreEqual(cacheEntry.Value, cacheEntryFromStack.Value);

			//Give enough time for the background task back propagation to happen
			await Task.Delay(2000);

			Assert.AreEqual(cacheEntry, await layer1.GetAsync<int>("Get_BackPopulatesToEarlierCacheLayers"));
			Assert.IsNull(await layer3.GetAsync<int>("Get_BackPopulatesToEarlierCacheLayers"));
		}
		[TestMethod]
		public async Task Get_DoesNotBackPopulateToEarlierCacheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			var layer3 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(null, new(new[] { layer1, layer2, layer3 }));
			var cacheEntry = new CacheEntry<int>(42, TimeSpan.FromDays(1));
			await layer2.SetAsync("Get_DoesNotBackPopulateToEarlierCacheLayers", cacheEntry);

			Internal.DateTimeProvider.UpdateTime();

			var cacheEntryFromStack = await cacheStack.GetAsync<int>("Get_DoesNotBackPopulateToEarlierCacheLayers", false);

			Assert.IsNotNull(cacheEntryFromStack);
			Assert.AreEqual(cacheEntry.Value, cacheEntryFromStack.Value);

			//Give enough time for the background task back propagation to happen if it had been requested
			await Task.Delay(2000);

			Assert.IsNull(await layer1.GetAsync<int>("Get_DoesNotBackPopulateToEarlierCacheLayers"));
			Assert.IsNull(await layer3.GetAsync<int>("Get_DoesNotBackPopulateToEarlierCacheLayers"));
		}
		[TestMethod]
		public async Task Get_DoesNotBackPopulateToEarlierCacheLayersByDefault()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();
			var layer3 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(null, new(new[] { layer1, layer2, layer3 }));
			var cacheEntry = new CacheEntry<int>(42, TimeSpan.FromDays(1));
			await layer2.SetAsync("Get_DoesNotBackPopulateToEarlierCacheLayersByDefault", cacheEntry);

			Internal.DateTimeProvider.UpdateTime();

			var cacheEntryFromStack = await cacheStack.GetAsync<int>("Get_DoesNotBackPopulateToEarlierCacheLayersByDefault");

			Assert.IsNotNull(cacheEntryFromStack);
			Assert.AreEqual(cacheEntry.Value, cacheEntryFromStack.Value);

			//Give enough time for the background task back propagation to happen if it had been requested
			await Task.Delay(2000);

			Assert.IsNull(await layer1.GetAsync<int>("Get_DoesNotBackPopulateToEarlierCacheLayersByDefault"));
			Assert.IsNull(await layer3.GetAsync<int>("Get_DoesNotBackPopulateToEarlierCacheLayersByDefault"));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Set_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.SetAsync(null, new CacheEntry<int>(1, TimeSpan.FromDays(1)));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task Set_ThrowsOnNullCacheEntry()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.SetAsync("MyCacheKey", (CacheEntry<int>)null);
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Set_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await using (cacheStack)
			{ }

			await cacheStack.SetAsync("KeyDoesntMatter", 1, TimeSpan.FromDays(1));
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task Set_ThrowsOnUseAfterDisposal_CacheEntry()
		{
			var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await using (cacheStack)
			{ }

			await cacheStack.SetAsync("KeyDoesntMatter", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task Set_SetsAllTheLayers()
		{
			var layer1 = new MemoryCacheLayer();
			var layer2 = new MemoryCacheLayer();

			await using var cacheStack = new CacheStack(null, new(new[] { layer1, layer2 }));
			var cacheEntry = await cacheStack.SetAsync("Set_SetsAllTheLayers", 42, TimeSpan.FromDays(1));

			Assert.AreEqual(cacheEntry, await layer1.GetAsync<int>("Set_SetsAllTheLayers"));
			Assert.AreEqual(cacheEntry, await layer2.GetAsync<int>("Set_SetsAllTheLayers"));
		}
		[TestMethod]
		public async Task Set_TriggersCacheChangeExtension()
		{
			var mockExtension = Substitute.For<ICacheChangeExtension>();
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }) { Extensions = new[] { mockExtension } });
			var cacheEntry = await cacheStack.SetAsync("Set_TriggersCacheChangeExtension", 42, TimeSpan.FromDays(1));

			await mockExtension.Received(1).OnCacheUpdateAsync("Set_TriggersCacheChangeExtension", cacheEntry.Expiry, CacheUpdateType.AddOrUpdateEntry);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.GetOrSetAsync<int>(null, (old) => Task.FromResult(5), new CacheSettings(TimeSpan.FromDays(1)));
		}
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullGetter()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.GetOrSetAsync<int>("MyCacheKey", null, new CacheSettings(TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task GetOrSet_CacheMiss()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheMiss", (oldValue) =>
		 {
			 return Task.FromResult(5);
		 }, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(5, result);
		}
		[TestMethod]
		public async Task GetOrSet_CacheHit()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.SetAsync("GetOrSet_CacheHit", 17, TimeSpan.FromDays(2));

			Internal.DateTimeProvider.UpdateTime();

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHit", (oldValue) =>
			{
				return Task.FromResult(27);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(17, result);
		}
		[TestMethod]
		public async Task GetOrSet_StaleCacheHit()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			var cacheEntry = new CacheEntry<int>(17, DateTime.UtcNow.AddDays(2));
			await cacheStack.SetAsync("GetOrSet_StaleCacheHit", cacheEntry);

			Internal.DateTimeProvider.UpdateTime();

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

			await using var cacheStack = new CacheStack(null, new(new[] { layer1, layer2, layer3 }));
			var cacheEntry = new CacheEntry<int>(42, TimeSpan.FromDays(1));
			await layer2.SetAsync("GetOrSet_BackPropagatesToEarlierCacheLayers", cacheEntry);

			Internal.DateTimeProvider.UpdateTime();

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
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			var cacheEntry = new CacheEntry<int>(23, DateTime.UtcNow.AddDays(2));
			await cacheStack.SetAsync("GetOrSet_ConcurrentStaleCacheHits_OnlyOneRefresh", cacheEntry);

			Internal.DateTimeProvider.UpdateTime();

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
		[TestMethod]
		public async Task GetOrSet_ConcurrentAccess_OnException()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));

			Internal.DateTimeProvider.UpdateTime();
			var getterCallCount = 0;

			var expectedException = new InvalidOperationException("Some exception");
			async Task act()
			{
				await cacheStack.GetOrSetAsync<int>(
					"GetOrSet_ConcurrentAccess_OnException",
					async _ =>
					{
						Interlocked.Increment(ref getterCallCount);
						await Task.Delay(100);
						throw expectedException;
					},
					new CacheSettings(TimeSpan.FromDays(2), TimeSpan.Zero)
				);
			}

			var tasks = Enumerable.Range(1, 2)
								  .Select(i => act())
								  .ToArray();

			try
			{
				await Task.WhenAll(tasks);
			}
			catch (Exception e)
			{
				Assert.IsInstanceOfType(e, expectedException.GetType());
			}

			Assert.AreEqual(1, getterCallCount);
			foreach (var task in tasks)
			{
				Assert.AreSame(expectedException, task.Exception.InnerException);
			}
		}
		[TestMethod]
		[DataRow(null)]
		[DataRow(42)]
		public async Task GetOrSet_ConcurrentAccess_SameResultForAllTasks(int? expectedResult)
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));

			Internal.DateTimeProvider.UpdateTime();
			var getterCallCount = 0;

			async Task<int?> act()
			{
				return await cacheStack.GetOrSetAsync<int?>(
					"GetOrSet_ConcurrentAccess_SameResultForAllTasks",
					async _ =>
					{
						Interlocked.Increment(ref getterCallCount);
						await Task.Delay(100);
						return expectedResult;
					},
					new CacheSettings(TimeSpan.FromDays(2), TimeSpan.Zero)
				);
			}

			var tasks = Enumerable.Range(1, 4)
								  .Select(i => act())
								  .ToArray();

			var whenAll = Task.WhenAll(tasks);
			await Task.WhenAny(whenAll, Task.Delay(TimeSpan.FromSeconds(1)));

			Assert.AreEqual(1, getterCallCount);
			foreach (var task in tasks)
			{
				Assert.AreEqual(expectedResult, task.Result);
			}
		}
		[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
		public async Task GetOrSet_ThrowsOnUseAfterDisposal()
		{
			var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await using (cacheStack)
			{ }

			await cacheStack.GetOrSetAsync<int>("KeyDoesntMatter", (old) => Task.FromResult(1), new CacheSettings(TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task GetOrSet_WaitingForRefresh()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
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

			Internal.DateTimeProvider.UpdateTime();

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
		[TestMethod]
		public async Task GetOrSet_ExpiredCacheHit()
		{
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));
			await cacheStack.SetAsync("GetOrSet_CacheHit", 17, TimeSpan.FromDays(-1));

			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheHit", (oldValue) =>
			{
				Assert.AreEqual(17, oldValue);
				return Task.FromResult(27);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(27, result);
		}
	}
}
