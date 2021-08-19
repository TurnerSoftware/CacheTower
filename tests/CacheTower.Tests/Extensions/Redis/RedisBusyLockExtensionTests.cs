using System;
using System.Threading.Tasks;
using CacheTower.Extensions.Redis;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CacheTower.Tests.Extensions.Redis
{
	[TestClass]
	public class RedisBusyLockExtensionTests
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowForNullConnection()
		{
			new RedisBusyLockExtension(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ThrowForInvalidDatabaseIndex()
		{
			new RedisBusyLockExtension(RedisHelper.GetConnection(), new RedisBusyLockOptions(databaseIndex: -10));
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void ThrowForRegisteringTwoCacheStacks()
		{
			var extension = new RedisBusyLockExtension(RedisHelper.GetConnection());
			var cacheStack = new Mock<ICacheStack>().Object;
			extension.Register(cacheStack);
			extension.Register(cacheStack);
		}

		[TestMethod]
		public async Task RefreshValueClearsLock()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisBusyLockExtension(connection, RedisBusyLockOptions.Default);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			await extension.WithRefreshAsync("TestKey",
				() => new ValueTask<CacheEntry<int>>(cacheEntry), new CacheSettings(TimeSpan.FromDays(1)));

			var lockStatus = await connection.GetDatabase().LockQueryAsync("TestKey_lock");

			Assert.IsFalse(lockStatus.HasValue, "Lock was not cleared after refreshValue");
		}


		[TestMethod]
		public async Task ObservedLockSingle()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisBusyLockExtension(connection, RedisBusyLockOptions.Default);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			//Establish lock
			await connection.GetDatabase().LockTakeAsync("TestKey_lock", "token", TimeSpan.FromMinutes(1));

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
			await connection.GetDatabase().LockReleaseAsync("TestKey_lock", "token");

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
			var extension = new RedisBusyLockExtension(connection, RedisBusyLockOptions.Default);
			extension.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

			//Establish lock
			await connection.GetDatabase().LockTakeAsync("TestKey_lock", "token", TimeSpan.FromMinutes(1));

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
			await connection.GetDatabase().LockReleaseAsync("TestKey_lock", "token");

			var whenAllRefreshesTask = Task.WhenAll(refreshTask1, refreshTask2);
			var succeedingTask = await Task.WhenAny(whenAllRefreshesTask, Task.Delay(TimeSpan.FromSeconds(10)));
			if (!succeedingTask.Equals(whenAllRefreshesTask))
			{
				RedisHelper.DebugInfo(connection);
				Assert.Fail("Refresh has timed out - something has gone very wrong");
			}

			cacheStackMock.Verify(c => c.GetAsync<int>("TestKey"), Times.Exactly(4), "Two checks to the cache stack are expected");
		}
	}
}