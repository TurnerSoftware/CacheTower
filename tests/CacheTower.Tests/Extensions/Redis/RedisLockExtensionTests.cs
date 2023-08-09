using System;
using System.Threading.Tasks;
using CacheTower.Extensions.Redis;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using StackExchange.Redis;

namespace CacheTower.Tests.Extensions.Redis;

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
		new RedisLockExtension(RedisHelper.GetConnection(), RedisLockOptions.Default with { DatabaseIndex = -10 });
	}

	[TestMethod, ExpectedException(typeof(ArgumentNullException))]
	public void ThrowForNullChannel()
	{
		new RedisLockExtension(RedisHelper.GetConnection(), RedisLockOptions.Default with { RedisChannel = null });
	}

	[TestMethod, ExpectedException(typeof(InvalidOperationException))]
	public void ThrowForRegisteringTwoCacheStacks()
	{
		var extension = new RedisLockExtension(RedisHelper.GetConnection());
		var cacheStack = Substitute.For<ICacheStack>();
		extension.Register(cacheStack);
		extension.Register(cacheStack);
	}

	[TestMethod]
	public async Task CustomLockTimeout()
	{
		RedisHelper.ResetState();

		var extension = new RedisLockExtension(RedisHelper.GetConnection(), RedisLockOptions.Default with { LockTimeout = TimeSpan.FromDays(1) });
		await using var distributedLock = await extension.AwaitAccessAsync("TestLock");

		var database = RedisHelper.GetConnection().GetDatabase();
		var keyWithExpiry = await database.StringGetWithExpiryAsync("Lock:TestLock");

		var lockTimeout = TimeSpan.FromDays(1);
		var actualExpiry = keyWithExpiry.Expiry.Value;

		//Due to the logistics of the wait etc, we can't do an exact comparison
		//Instead, we can safely say the above code should be completed within a minute
		//With that in mind, remove that from the set lock timeout and compare the expiry
		//is greater than this new "comparison timeout".
		var comparisonTimeout = lockTimeout - TimeSpan.FromMinutes(1);

		Assert.IsTrue(comparisonTimeout < actualExpiry);
	}

	[TestMethod]
	public async Task RefreshValueNotifiesChannelSubscribers()
	{
		RedisHelper.ResetState();

		var connection = RedisHelper.GetConnection();

		var cacheStackMock = Substitute.For<ICacheStack>();
		var extension = new RedisLockExtension(connection, RedisLockOptions.Default);
		extension.Register(cacheStackMock);

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

		// Delay to avoid race condition with pub/sub
		// See: https://github.com/StackExchange/StackExchange.Redis/issues/1827
		await Task.Delay(TimeSpan.FromMilliseconds(250));

		var cacheEntry = new CacheEntry<int>(13, TimeSpan.FromDays(1));

		var distributedLock = await extension.AwaitAccessAsync("TestKey");
		await distributedLock.DisposeAsync();

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

		var cacheStackMock = Substitute.For<ICacheStack>();
		var extension = new RedisLockExtension(connection, RedisLockOptions.Default);
		extension.Register(cacheStackMock);

		//Establish lock
		await connection.GetDatabase().StringSetAsync("Lock:TestKey", RedisValue.EmptyString);

		var lockTask = extension.AwaitAccessAsync("TestKey").AsTask();

		await Task.Delay(TimeSpan.FromSeconds(1));

		Assert.IsTrue(extension.LockedOnKeyRefresh.ContainsKey("TestKey"), "Lock was not established");

		//Trigger the end of the lock
		await connection.GetSubscriber().PublishAsync("CacheTower.CacheLock", "TestKey");

		var succeedingTask = await Task.WhenAny(lockTask, Task.Delay(TimeSpan.FromSeconds(10)));
		if (!succeedingTask.Equals(lockTask))
		{
			RedisHelper.DebugInfo(connection);
			Assert.Fail("Refresh has timed out - something has gone very wrong");
		}
	}

	[TestMethod]
	public async Task ObservedLockMultiple()
	{
		RedisHelper.ResetState();

		var connection = RedisHelper.GetConnection();

		var cacheStackMock = Substitute.For<ICacheStack>();
		var extension = new RedisLockExtension(connection, RedisLockOptions.Default);
		extension.Register(cacheStackMock);

		//Establish lock
		await connection.GetDatabase().StringSetAsync("Lock:TestKey", RedisValue.EmptyString);

		var lockTask1 = extension.AwaitAccessAsync("TestKey").AsTask();

		var lockTask2 = extension.AwaitAccessAsync("TestKey").AsTask();

		//Delay to allow for Redis check and self-entry into lock
		await Task.Delay(TimeSpan.FromSeconds(2));

		Assert.IsTrue(extension.LockedOnKeyRefresh.ContainsKey("TestKey"), "Lock was not established");

		//Trigger the end of the lock
		await connection.GetSubscriber().PublishAsync("CacheTower.CacheLock", "TestKey");

		var whenAllRefreshesTask = Task.WhenAll(lockTask1, lockTask2);
		var succeedingTask = await Task.WhenAny(whenAllRefreshesTask, Task.Delay(TimeSpan.FromSeconds(10)));
		if (!succeedingTask.Equals(whenAllRefreshesTask))
		{
			RedisHelper.DebugInfo(connection);
			Assert.Fail("Refresh has timed out - something has gone very wrong");
		}
	}

	[TestMethod]
	public async Task FailsafeOnSubscriberFailure()
	{
		RedisHelper.ResetState();

		var connection = RedisHelper.GetConnection();

		var cacheStackMock = Substitute.For<ICacheStack>();
		var extension = new RedisLockExtension(connection, RedisLockOptions.Default with { LockTimeout = TimeSpan.FromSeconds(1) });
		extension.Register(cacheStackMock);

		//Establish lock
		await connection.GetDatabase().StringSetAsync("Lock:TestKey", RedisValue.EmptyString);

		var lockTask = extension.AwaitAccessAsync("TestKey").AsTask();

		//Delay to allow for Redis check and self-entry into lock
		await Task.Delay(TimeSpan.FromSeconds(1));

		Assert.IsTrue(extension.LockedOnKeyRefresh.ContainsKey("TestKey"), "Lock was not established");

		//We don't publish to end lock

		//However, we still expect to succeed
		var succeedingTask = await Task.WhenAny(lockTask, Task.Delay(TimeSpan.FromSeconds(10)));
		if (!succeedingTask.Equals(lockTask))
		{
			RedisHelper.DebugInfo(connection);
			Assert.Fail("Refresh has timed out - something has gone very wrong");
		}
	}



	[TestMethod]
	public async Task BusyLockCheckWorksWhenSubscriberFails()
	{
		RedisHelper.ResetState();

		var connection = RedisHelper.GetConnection();

		var cacheStackMock = Substitute.For<ICacheStack>();
		var extension = new RedisLockExtension(connection, RedisLockOptions.Default with { LockCheckStrategy = LockCheckStrategy.WithSpinLock(TimeSpan.FromMilliseconds(50)) });
		extension.Register(cacheStackMock);

		//Establish lock
		await connection.GetDatabase().StringSetAsync("Lock:TestKey", RedisValue.EmptyString);

		var lockTask = extension.AwaitAccessAsync("TestKey").AsTask();

		//Delay to allow for Redis check and self-entry into lock
		await Task.Delay(TimeSpan.FromSeconds(1));

		Assert.IsTrue(extension.LockedOnKeyRefresh.ContainsKey("TestKey"), "Lock was not established");

		//Trigger the end of the lock
		await connection.GetDatabase().KeyDeleteAsync("Lock:TestKey");

		//Note we don't publish the value was refreshed

		var succeedingTask = await Task.WhenAny(lockTask, Task.Delay(TimeSpan.FromSeconds(10)));
		if (!succeedingTask.Equals(lockTask))
		{
			RedisHelper.DebugInfo(connection);
			Assert.Fail("Refresh has timed out - something has gone very wrong");
		}
	}
}