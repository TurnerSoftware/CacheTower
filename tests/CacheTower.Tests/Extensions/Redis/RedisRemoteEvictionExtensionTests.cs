using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Extensions.Redis;
using CacheTower.Providers.Memory;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
			var cacheStackMock = new Mock<ICacheStack>();
			extension.Register(cacheStackMock.Object);
			extension.Register(cacheStackMock.Object);
		}

		[TestMethod]
		public async Task RemoteEvictionOccursOnRefresh()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = new Mock<CacheStack>();
			var cacheLayerOne = new Mock<ICacheLayer>();
			cacheStackMockOne.Setup(c => c.GetCacheLayers())
				.Returns(new[] { cacheLayerOne.Object });
			var extensionOne = new RedisRemoteEvictionExtension(connection);
			extensionOne.Register(cacheStackMockOne.Object);

			var cacheStackMockTwo = new Mock<CacheStack>();
			var cacheLayerTwo = new Mock<ICacheLayer>();
			cacheStackMockTwo.Setup(c => c.GetCacheLayers())
				.Returns(new[] { cacheLayerTwo.Object });
			var extensionTwo = new RedisRemoteEvictionExtension(connection);
			extensionTwo.Register(cacheStackMockTwo.Object);

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

			cacheLayerOne.Verify(c => c.EvictAsync("TestKey"), Times.Never, "Eviction took place locally where it should have been skipped");
			cacheLayerTwo.Verify(c => c.EvictAsync("TestKey"), Times.Once, "Eviction was skipped where it should have taken place locally");
		}

		[TestMethod]
		public async Task RemoteEvictionOccursOnLocalEviction()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = new Mock<CacheStack>();
			var cacheLayerOne = new Mock<ICacheLayer>();
			cacheStackMockOne.Setup(c => c.GetCacheLayers())
				.Returns(new[] { cacheLayerOne.Object });
			var extensionOne = new RedisRemoteEvictionExtension(connection);
			extensionOne.Register(cacheStackMockOne.Object);

			var cacheStackMockTwo = new Mock<CacheStack>();
			var cacheLayerTwo = new Mock<ICacheLayer>();
			cacheStackMockTwo.Setup(c => c.GetCacheLayers())
				.Returns(new[] { cacheLayerTwo.Object });
			var extensionTwo = new RedisRemoteEvictionExtension(connection);
			extensionTwo.Register(cacheStackMockTwo.Object);

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

			cacheLayerOne.Verify(c => c.EvictAsync("TestKey"), Times.Never, "Eviction took place locally where it should have been skipped");
			cacheLayerTwo.Verify(c => c.EvictAsync("TestKey"), Times.Once, "Eviction was skipped where it should have taken place locally");
		}

		[TestMethod]
		public async Task RemoteFlush()
		{
			RedisHelper.ResetState();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = new Mock<CacheStack>();
			var cacheLayerOne = new Mock<ICacheLayer>();
			cacheStackMockOne.Setup(c => c.GetCacheLayers())
				.Returns(new[] { cacheLayerOne.Object });
			var extensionOne = new RedisRemoteEvictionExtension(connection);
			extensionOne.Register(cacheStackMockOne.Object);

			var cacheStackMockTwo = new Mock<CacheStack>();
			var cacheLayerTwo = new Mock<ICacheLayer>();
			cacheStackMockTwo.Setup(c => c.GetCacheLayers())
				.Returns(new[] { cacheLayerTwo.Object });
			var extensionTwo = new RedisRemoteEvictionExtension(connection);
			extensionTwo.Register(cacheStackMockTwo.Object);

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

			cacheLayerOne.Verify(c => c.FlushAsync(), Times.Never, "Flush took place locally where it should have been skipped");
			cacheLayerTwo.Verify(c => c.FlushAsync(), Times.Once, "Flush was skipped where it should have taken place locally");
		}

		[TestMethod]
		public async Task NoEvictionOnNewEntries()
		{
			RedisHelper.ResetState();

			var realConnection = RedisHelper.GetConnection();

			var connectionMock = new Mock<IConnectionMultiplexer>();
			var subscriberMock = new Mock<ISubscriber>();

			subscriberMock
				.Setup(subscriber => subscriber.Subscribe(It.IsAny<RedisChannel>(), It.IsAny<CommandFlags>()))
				.Returns(() => realConnection.GetSubscriber().Subscribe("DummyMessageQueue"));
			connectionMock
				.Setup(connection => connection.GetSubscriber(It.IsAny<object>()))
				.Returns(subscriberMock.Object);

			var cacheLayerOne = new Mock<ICacheLayer>();
			var extensionOne = new RedisRemoteEvictionExtension(connectionMock.Object);
			var cacheStackOne = new CacheStack(new[] { cacheLayerOne.Object }, new[] { extensionOne });

			await cacheStackOne.GetOrSetAsync<int>("NoEvictionOnNewEntries", _ => Task.FromResult(1), new CacheSettings(TimeSpan.FromMinutes(5)));

			subscriberMock
				.Verify(
					subscriber => subscriber.PublishAsync("CacheTower.RemoteEviction", It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()),
					Times.Never
				);
		}
	}
}