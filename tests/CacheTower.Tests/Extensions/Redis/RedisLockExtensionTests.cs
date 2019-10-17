using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Extensions.Redis;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CacheTower.Tests.Extensions.Redis
{
	[TestClass, Ignore]
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
		public async Task RefreshValueNotifiesChannelSubscribers()
		{
			var connection = RedisHelper.GetConnection();
			var hasMessagedSubscribers = false;

			await connection.GetSubscriber().SubscribeAsync("CacheTower.CacheLock", (channel, value) =>
			{
				if (value == "TestKey")
				{
					hasMessagedSubscribers = true;
				}
			});

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisLockExtension(connection);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, DateTime.UtcNow, TimeSpan.FromDays(1));

			await extension.RefreshValueAsync(string.Empty, "TestKey", 
				() => Task.FromResult(cacheEntry), new CacheSettings(TimeSpan.FromDays(1)));
			await Task.Delay(1000);

			Assert.IsTrue(hasMessagedSubscribers, "Subscribers were not notified about the refreshed value");
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
						await Task.Delay(1000);
						return cacheEntry;
					},
					new CacheSettings(TimeSpan.FromDays(1))
				);
			}

			var primaryTask = DoWorkAsync();
			var secondaryTask = DoWorkAsync();

			var succeedingTask = await Task.WhenAny(primaryTask, secondaryTask);
			Assert.AreEqual(await primaryTask, await succeedingTask, "Processing task call didn't complete first - something has gone very wrong.");

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

			var primaryTask = extensionOne.RefreshValueAsync(string.Empty, "TestKey",
					async () =>
					{
						await Task.Delay(1000);
						return cacheEntry;
					},
					new CacheSettings(TimeSpan.FromDays(1))
				);
			var secondaryTask = extensionTwo.RefreshValueAsync(string.Empty, "TestKey",
					async () =>
					{
						await Task.Delay(1000);
						return cacheEntry;
					},
					new CacheSettings(TimeSpan.FromDays(1))
				);

			var succeedingTask = await Task.WhenAny(primaryTask, secondaryTask);
			Assert.AreEqual(await primaryTask, await succeedingTask, "Processing task call didn't complete first - something has gone very wrong.");

			cacheStackMockOne.Verify(c => c.GetAsync<int>("TestKey"), Times.Never, "Processing task shouldn't be querying existing values");
			cacheStackMockTwo.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(2), "Missed checks whether waiting was required or retrieving the updated value");
		}
	}
}
