using System;
using System.Threading.Tasks;
using CacheTower.Extensions.Redis;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using StackExchange.Redis;

namespace CacheTower.Tests.Extensions.Redis
{
	[TestClass]
	public class RedisRemoteEvictionExtensionTests
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowForNullConnection()
		{
			new RedisRemoteEvictionExtension(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowForNullCacheEvictionLayers()
		{
			new RedisRemoteEvictionExtension(RedisHelper.GetConnection(), null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowForNullChannel()
		{
			new RedisRemoteEvictionExtension(RedisHelper.GetConnection(), null);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void ThrowForRegisteringMoreThanOneCacheStack()
		{
			var extension = new RedisRemoteEvictionExtension(RedisHelper.GetConnection());
			var cacheStackMock = Substitute.For<ICacheStack>();
			extension.Register(cacheStackMock);
			extension.Register(cacheStackMock);
		}

		[TestMethod]
		public async Task RemoteEvictionOccursOnRefresh()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = Substitute.For<IExtendableCacheStack>();
			var cacheLayerOne = Substitute.For<ILocalCacheLayer>();
			cacheStackMockOne.GetCacheLayers()
				.Returns(new[] { cacheLayerOne });
			var extensionOne = new RedisRemoteEvictionExtension(connection);
			extensionOne.Register(cacheStackMockOne);

			var cacheStackMockTwo = Substitute.For<IExtendableCacheStack>();
			var cacheLayerTwo = Substitute.For<ILocalCacheLayer>();
			cacheStackMockTwo.GetCacheLayers()
				.Returns(new[] { cacheLayerTwo });
			var extensionTwo = new RedisRemoteEvictionExtension(connection);
			extensionTwo.Register(cacheStackMockTwo);

			var completionSource = new TaskCompletionSource<bool>();
			connection.GetSubscriber().Subscribe("CacheTower.RemoteEviction").OnMessage(channelMessage =>
			{
				if (channelMessage.Message == "TestKey")
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

			var expiry = DateTime.UtcNow.AddDays(1);
			await extensionOne.OnCacheUpdateAsync("TestKey", expiry, CacheUpdateType.AddOrUpdateEntry);

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(completionSource.Task, succeedingTask, "Subscriber response took too long");
			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the refreshed value");

			await Task.Delay(500);

			await cacheLayerOne.DidNotReceive().EvictAsync("TestKey"); //else; Eviction took place locally where it should have been skipped
			await cacheLayerTwo.Received(1).EvictAsync("TestKey"); //else; Eviction was skipped where it should have taken place locally
		}

		[TestMethod]
		public async Task RemoteEvictionOccursOnLocalEviction()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = Substitute.For<IExtendableCacheStack>();
			var cacheLayerOne = Substitute.For<ILocalCacheLayer>();
			cacheStackMockOne.GetCacheLayers()
				.Returns(new[] { cacheLayerOne });
			var extensionOne = new RedisRemoteEvictionExtension(connection);
			extensionOne.Register(cacheStackMockOne);

			var cacheStackMockTwo = Substitute.For<IExtendableCacheStack>();
			var cacheLayerTwo = Substitute.For<ILocalCacheLayer>();
			cacheStackMockTwo.GetCacheLayers()
				.Returns(new[] { cacheLayerTwo });
			var extensionTwo = new RedisRemoteEvictionExtension(connection);
			extensionTwo.Register(cacheStackMockTwo);

			var completionSource = new TaskCompletionSource<bool>();
			connection.GetSubscriber().Subscribe("CacheTower.RemoteEviction").OnMessage(channelMessage =>
			{
				if (channelMessage.Message == "TestKey")
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

			await extensionOne.OnCacheEvictionAsync("TestKey");

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			if (!succeedingTask.Equals(completionSource.Task))
			{
				RedisHelper.DebugInfo(connection);
				Assert.Fail("Subscriber response took too long");
			}

			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the refreshed value");

			await Task.Delay(500);

			await cacheLayerOne.DidNotReceive().EvictAsync("TestKey"); //else; Eviction took place locally where it should have been skipped
			await cacheLayerTwo.Received(1).EvictAsync("TestKey"); //else; Eviction was skipped where it should have taken place locally
		}

		[TestMethod]
		public async Task RemoteFlush()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = Substitute.For<IExtendableCacheStack>();
			var cacheLayerOne = Substitute.For<ILocalCacheLayer>();
			cacheStackMockOne.GetCacheLayers()
				.Returns(new[] { cacheLayerOne });
			var extensionOne = new RedisRemoteEvictionExtension(connection);
			extensionOne.Register(cacheStackMockOne);

			var cacheStackMockTwo = Substitute.For<IExtendableCacheStack>();
			var cacheLayerTwo = Substitute.For<ILocalCacheLayer>();
			cacheStackMockTwo.GetCacheLayers()
				.Returns(new[] { cacheLayerTwo });
			var extensionTwo = new RedisRemoteEvictionExtension(connection);
			extensionTwo.Register(cacheStackMockTwo);

			var completionSource = new TaskCompletionSource<bool>();
			connection.GetSubscriber().Subscribe("CacheTower.RemoteFlush").OnMessage(channelMessage =>
			{
				if (channelMessage.Message == StackExchange.Redis.RedisValue.EmptyString)
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

			await extensionOne.OnCacheFlushAsync();

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			if (!succeedingTask.Equals(completionSource.Task))
			{
				RedisHelper.DebugInfo(connection);
				Assert.Fail("Subscriber response took too long");
			}

			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the flush");

			await Task.Delay(500);

			await cacheLayerOne.DidNotReceive().FlushAsync(); //else; Flush took place locally where it should have been skipped
			await cacheLayerTwo.Received(1).FlushAsync(); //else; Flush was skipped where it should have taken place locally
		}

		[TestMethod]
		public async Task NoEvictionOnNewEntries()
		{
			RedisHelper.ResetState();

			var realConnection = RedisHelper.GetConnection();

			var connectionMock = Substitute.For<IConnectionMultiplexer>();
			var subscriberMock = Substitute.For<ISubscriber>();

			subscriberMock.Subscribe(Arg.Any<RedisChannel>(), Arg.Any<CommandFlags>())
				.Returns(x => realConnection.GetSubscriber().Subscribe("DummyMessageQueue"));
			subscriberMock.Subscribe(Arg.Any<RedisChannel>(), Arg.Any<CommandFlags>())
				.Returns(x => realConnection.GetSubscriber().Subscribe("DummyMessageQueue"));
			connectionMock.GetSubscriber(Arg.Any<object>())
				.Returns(subscriberMock);

			var cacheLayerOne = Substitute.For<ILocalCacheLayer>();
			var extensionOne = new RedisRemoteEvictionExtension(connectionMock);
			var cacheStackOne = new CacheStack(null, new(new[] { cacheLayerOne }) { Extensions = new[] { extensionOne } });

			await cacheStackOne.GetOrSetAsync<int>("NoEvictionOnNewEntries", _ => Task.FromResult(1), new CacheSettings(TimeSpan.FromMinutes(5)));

			await subscriberMock.DidNotReceive().PublishAsync("CacheTower.RemoteEviction", Arg.Any<RedisValue>(), Arg.Any<CommandFlags>());
		}
	}
}