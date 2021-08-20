using System;
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
			new RedisLockExtension(RedisHelper.GetConnection(), new RedisLockOptions(databaseIndex: -10));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowForNullChannel()
		{
			new RedisLockExtension(RedisHelper.GetConnection(), new RedisLockOptions(redisChannel: null));
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
			RedisHelper.ResetState();

			var extension = new RedisLockExtension(RedisHelper.GetConnection(), new RedisLockOptions(lockTimeout: TimeSpan.FromDays(1)));
			var refreshWaiterTask = new TaskCompletionSource<bool>();
			var lockWaiterTask = new TaskCompletionSource<bool>();

			var refreshTask = extension.WithRefreshAsync("TestLock", async () =>
			{
				lockWaiterTask.SetResult(true);
				await refreshWaiterTask.Task;
				return new CacheEntry<int>(5, TimeSpan.FromDays(1));
			}, new CacheSettings(TimeSpan.FromHours(3)));

			await lockWaiterTask.Task;

			var database = RedisHelper.GetConnection().GetDatabase();
			var keyWithExpiry = await database.StringGetWithExpiryAsync("Lock:TestLock");

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
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection, RedisLockOptions.Default);
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

			await extension.WithRefreshAsync("TestKey",
				() => new ValueTask<CacheEntry<int>>(cacheEntry), new CacheSettings(TimeSpan.FromDays(1)));

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			if (!succeedingTask.Equals(completionSource.Task))
			{
				RedisHelper.DebugInfo(connection);
				Assert.Fail("Subscriber response took too long");
			}

			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the refreshed value");
		}


		[TestMethod]
		public async Task ObservedLockSingle()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection, RedisLockOptions.Default);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			//Establish lock
			await connection.GetDatabase().StringSetAsync("Lock:TestKey", RedisValue.EmptyString);

			var refreshTask = extension.WithRefreshAsync("TestKey",
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
			if (!succeedingTask.Equals(refreshTask))
			{
				RedisHelper.DebugInfo(connection);
				Assert.Fail("Refresh has timed out - something has gone very wrong");
			}

			cacheStackMock.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(2), "Two checks to the cache stack are expected");
		}


		[TestMethod]
		public async Task ObservedLockMultiple()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection, RedisLockOptions.Default);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			//Establish lock
			await connection.GetDatabase().StringSetAsync("Lock:TestKey", RedisValue.EmptyString);

			var refreshTask1 = extension.WithRefreshAsync("TestKey",
					() =>
					{
						return new ValueTask<CacheEntry<int>>(cacheEntry);
					},
					new CacheSettings(TimeSpan.FromDays(1))
				).AsTask();

			var refreshTask2 = extension.WithRefreshAsync("TestKey",
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
			if (!succeedingTask.Equals(whenAllRefreshesTask))
			{
				RedisHelper.DebugInfo(connection);
				Assert.Fail("Refresh has timed out - something has gone very wrong");
			}

			cacheStackMock.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(4), "Two checks to the cache stack are expected");
		}

		[TestMethod]
		public async Task FailsafeOnSubscriberFailure()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection, new RedisLockOptions(lockTimeout: TimeSpan.FromSeconds(1)));
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			//Establish lock
			await connection.GetDatabase().StringSetAsync("Lock:TestKey", RedisValue.EmptyString);

			var refreshTask = extension.WithRefreshAsync("TestKey",
				() =>
				{
					return new ValueTask<CacheEntry<int>>(cacheEntry);
				},
				new CacheSettings(TimeSpan.FromDays(1))
			).AsTask();

			//Delay to allow for Redis check and self-entry into lock
			await Task.Delay(TimeSpan.FromSeconds(1));

			Assert.IsTrue(extension.LockedOnKeyRefresh.ContainsKey("TestKey"), "Lock was not established");

			//We don't publish to end lock

			//However, we still expect to succeed
			var succeedingTask = await Task.WhenAny(refreshTask, Task.Delay(TimeSpan.FromSeconds(10)));
			if (!succeedingTask.Equals(refreshTask))
			{
				RedisHelper.DebugInfo(connection);
				Assert.Fail("Refresh has timed out - something has gone very wrong");
			}

			cacheStackMock.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(1), "One checks to the cache stack are expected as it will fail to resolve lock");
		}



		[TestMethod]
		public async Task BusyLockCheckWorksWhenSubscriberFails()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection, new RedisLockOptions(spinTime: TimeSpan.FromMilliseconds(50)));
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			//Establish lock
			await connection.GetDatabase().StringSetAsync("Lock:TestKey", RedisValue.EmptyString);

			var refreshTask = extension.WithRefreshAsync("TestKey",
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
			await connection.GetDatabase().KeyDeleteAsync("Lock:TestKey");

			//Note we don't publish the value was refreshed

			var succeedingTask = await Task.WhenAny(refreshTask, Task.Delay(TimeSpan.FromSeconds(10)));
			if (!succeedingTask.Equals(refreshTask))
			{
				RedisHelper.DebugInfo(connection);
				Assert.Fail("Refresh has timed out - something has gone very wrong");
			}

			cacheStackMock.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(2), "Two checks to the cache stack are expected");
		}
	}
}