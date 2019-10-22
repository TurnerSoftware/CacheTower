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
			var extension = new RedisLockExtension(RedisHelper.GetConnection(), lockTimeout: TimeSpan.FromDays(1));
			var refreshWaiterTask = new TaskCompletionSource<bool>();
			var lockWaiterTask = new TaskCompletionSource<bool>();

			var refreshTask = extension.RefreshValueAsync("RequestId", "TestLock", async () =>
			{
				lockWaiterTask.SetResult(true);
				await refreshWaiterTask.Task;
				return new CacheEntry<int>(5, DateTime.UtcNow, TimeSpan.FromDays(1));
			}, new CacheSettings(TimeSpan.FromHours(3)));

			await lockWaiterTask.Task;

			var database = RedisHelper.GetConnection().GetDatabase();
			var keyWithExpiry = await database.StringGetWithExpiryAsync("TestLock");

			refreshWaiterTask.SetResult(true);

			//Confirm we are reading the right key
			Assert.AreEqual("RequestId", (string)keyWithExpiry.Value);

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
			var connection = RedisHelper.GetConnection();
			var taskCompletionSource = new TaskCompletionSource<bool>();

			await connection.GetSubscriber().SubscribeAsync("CacheTower.CacheLock", (channel, value) =>
			{
				if (value == "TestKey")
				{
					taskCompletionSource.SetResult(true);
				}

				taskCompletionSource.SetResult(false);
			});

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, DateTime.UtcNow, TimeSpan.FromDays(1));

			await extension.RefreshValueAsync(string.Empty, "TestKey", 
				() => Task.FromResult(cacheEntry), new CacheSettings(TimeSpan.FromDays(1)));


			var waitTask = taskCompletionSource.Task;
			await Task.WhenAny(waitTask, Task.Delay(TimeSpan.FromSeconds(10)));

			if (!waitTask.IsCompleted)
			{
				Assert.Fail("Subscriber response took too long");
			}

			Assert.IsTrue(waitTask.Result, "Subscribers were not notified about the refreshed value");
		}


		[TestMethod]
		public async Task WaitingTaskInSameInstanceUnlocksAndCompletes()
		{
			var connection = RedisHelper.GetConnection();
			
			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, DateTime.UtcNow, TimeSpan.FromDays(1));

			async Task<CacheEntry<int>> DoWorkAsync()
			{
				return await extension.RefreshValueAsync(string.Empty, "TestKey",
					async () =>
					{
						await Task.Delay(2000);
						return cacheEntry;
					},
					new CacheSettings(TimeSpan.FromDays(1))
				);
			}

			var primaryTask = DoWorkAsync();
			var secondaryTask = DoWorkAsync();

			var succeedingTask = await Task.WhenAny(primaryTask, secondaryTask);
			Assert.AreEqual(await primaryTask, await succeedingTask, "Processing task call didn't complete first - something has gone very wrong.");

			await secondaryTask;

			cacheStackMock.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(2), "Missed checks whether waiting was required or retrieving the updated value");
		}


		[TestMethod]
		public async Task WaitingTaskInDifferentInstanceUnlocksAndCompletes()
		{
			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = new Mock<ICacheStack>();
			var extensionOne = new RedisLockExtension(connection);
			extensionOne.Register(cacheStackMockOne.Object);

			var cacheStackMockTwo = new Mock<ICacheStack>();
			var extensionTwo = new RedisLockExtension(connection);
			extensionTwo.Register(cacheStackMockTwo.Object);

			var cacheEntry = new CacheEntry<int>(13, DateTime.UtcNow, TimeSpan.FromDays(1));
			var secondaryTaskKickoff = new TaskCompletionSource<bool>();

			var primaryTask = extensionOne.RefreshValueAsync(string.Empty, "TestKey",
					async () =>
					{
						secondaryTaskKickoff.SetResult(true);
						await Task.Delay(3000);
						return cacheEntry;
					},
					new CacheSettings(TimeSpan.FromDays(1))
				);

			await secondaryTaskKickoff.Task;

			var secondaryTask = extensionTwo.RefreshValueAsync(string.Empty, "TestKey",
					() =>
					{
						return Task.FromResult(cacheEntry);
					},
					new CacheSettings(TimeSpan.FromDays(1))
				);

			var succeedingTask = await Task.WhenAny(primaryTask, secondaryTask);
			Assert.AreEqual(await primaryTask, await succeedingTask, "Processing task call didn't complete first - something has gone very wrong.");

			//Let the secondary task finish before we verify ICacheStack method calls
			await secondaryTask;

			cacheStackMockOne.Verify(c => c.GetAsync<int>("TestKey"), Times.Never, "Processing task shouldn't be querying existing values");
			cacheStackMockTwo.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(2), "Missed GetAsync for retrieving the updated value - this means the registered stack returned the updated value early");
		}
	}
}
