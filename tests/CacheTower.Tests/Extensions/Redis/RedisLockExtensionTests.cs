using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Extensions.Redis;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StackExchange.Redis;

namespace CacheTower.Tests.Extensions.Redis
{
	[TestClass]
	public class RedisLockExtensionTests
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowForNullConnection()
		{
			new RedisLockExtension(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ThrowForInvalidDatabaseIndex()
		{
			new RedisLockExtension(RedisHelper.GetConnection(), -10);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowForNullChannel()
		{
			new RedisLockExtension(RedisHelper.GetConnection(), channelPrefix: null);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void ThrowForRegisteringTwoCacheStacks()
		{
			var extension = new RedisLockExtension(RedisHelper.GetConnection());
			var cacheStack = new Mock<ICacheStack>().Object;
			extension.Register(cacheStack);
			extension.Register(cacheStack);
		}

		[TestMethod]
		public async Task CustomLockTimeout()
		{
			RedisHelper.FlushDatabase();

			var extension = new RedisLockExtension(RedisHelper.GetConnection(), lockTimeout: TimeSpan.FromDays(1));
			var refreshWaiterTask = new TaskCompletionSource<bool>();
			var lockWaiterTask = new TaskCompletionSource<bool>();

			var refreshTask = extension.RefreshValueAsync("TestLock", async () =>
			{
				lockWaiterTask.SetResult(true);
				await refreshWaiterTask.Task;
				return new CacheEntry<int>(5, TimeSpan.FromDays(1));
			}, new CacheSettings(TimeSpan.FromHours(3)));

			await lockWaiterTask.Task;

			var database = RedisHelper.GetConnection().GetDatabase();
			var keyWithExpiry = await database.StringGetWithExpiryAsync("TestLock");

			refreshWaiterTask.SetResult(true);

			var lockTimeout = TimeSpan.FromDays(1);
			var actualExpiry = keyWithExpiry.Expiry.Value;

			//Due to the logistics of the wait etc, we can't do an exact comparison
			//Instead, we can safely say the above code should be completed within a minute
			//With that in mind, remove that from the set lock timeout and compare the expiry
			//is greater than this new "comparison timeout".
			var comparisonTimeout = lockTimeout - TimeSpan.FromMinutes(1);

			Assert.IsTrue(comparisonTimeout < keyWithExpiry.Expiry.Value);
		}

		[TestMethod]
		public async Task RefreshValueNotifiesChannelSubscribers()
		{
			RedisHelper.FlushDatabase();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection);
			extension.Register(cacheStackMock.Object);

			var completionSource = new TaskCompletionSource<bool>();

			await connection.GetSubscriber().SubscribeAsync("CacheTower.CacheLock", (channel, value) =>
			{
				if (value == "TestKey")
				{
					completionSource.SetResult(true);
				}
				else
				{
					completionSource.SetResult(false);
				}
			});

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			await extension.RefreshValueAsync("TestKey",
				() => new ValueTask<CacheEntry<int>>(cacheEntry), new CacheSettings(TimeSpan.FromDays(1)));

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(completionSource.Task, succeedingTask, "Subscriber response took too long");
			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the refreshed value");
		}


		[TestMethod]
		public async Task ObservedLockSingle()
		{
			RedisHelper.FlushDatabase();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			//Establish lock
			await connection.GetDatabase().StringSetAsync("TestKey", RedisValue.EmptyString);

			var refreshTask = extension.RefreshValueAsync("TestKey",
					() =>
					{
						return new ValueTask<CacheEntry<int>>(cacheEntry);
					},
					new CacheSettings(TimeSpan.FromDays(1))
				).AsTask();

			//Delay to allow for Redis check and self-entry into lock
			await Task.Delay(TimeSpan.FromSeconds(1));

			Assert.IsTrue(extension.LockedOnKeyRefresh.ContainsKey("TestKey"), "Lock was not established");

			//Trigger the end of the lock
			await connection.GetSubscriber().PublishAsync("CacheTower.CacheLock", "TestKey");

			var succeedingTask = await Task.WhenAny(refreshTask, Task.Delay(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(refreshTask, succeedingTask, "Refresh has timed out - something has gone very wrong");
			cacheStackMock.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(2), "Two checks to the cache stack are expected");
		}


		[TestMethod]
		public async Task ObservedLockMultiple()
		{
			RedisHelper.FlushDatabase();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			//Establish lock
			await connection.GetDatabase().StringSetAsync("TestKey", RedisValue.EmptyString);

			var refreshTask1 = extension.RefreshValueAsync("TestKey",
					() =>
					{
						return new ValueTask<CacheEntry<int>>(cacheEntry);
					},
					new CacheSettings(TimeSpan.FromDays(1))
				).AsTask();

			var refreshTask2 = extension.RefreshValueAsync("TestKey",
					() =>
					{
						return new ValueTask<CacheEntry<int>>(cacheEntry);
					},
					new CacheSettings(TimeSpan.FromDays(1))
				).AsTask();

			//Delay to allow for Redis check and self-entry into lock
			await Task.Delay(TimeSpan.FromSeconds(2));

			Assert.IsTrue(extension.LockedOnKeyRefresh.ContainsKey("TestKey"), "Lock was not established");

			//Trigger the end of the lock
			await connection.GetSubscriber().PublishAsync("CacheTower.CacheLock", "TestKey");

			var whenAllRefreshesTask = Task.WhenAll(refreshTask1, refreshTask2);
			var succeedingTask = await Task.WhenAny(whenAllRefreshesTask, Task.Delay(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(whenAllRefreshesTask, succeedingTask, "Refresh has timed out - something has gone very wrong");
			cacheStackMock.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(4), "Two checks to the cache stack are expected");
		}
	}
}