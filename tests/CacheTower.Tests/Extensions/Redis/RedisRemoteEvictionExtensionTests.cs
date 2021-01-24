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

namespace CacheTower.Tests.Extensions.Redis
{
	[TestClass]
	public class RedisRemoteEvictionExtensionTests
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ThrowForNullConnection()
		{
			new RedisRemoteEvictionExtension(null, new ICacheLayer[] { new MemoryCacheLayer() });
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
			var extension = new RedisRemoteEvictionExtension(RedisHelper.GetConnection(), new ICacheLayer[] { new MemoryCacheLayer() });
			var cacheStackMock = new Mock<ICacheStack>();
			extension.Register(cacheStackMock.Object);
			extension.Register(cacheStackMock.Object);
		}

		[TestMethod]
		public async Task RemoteEvictionOccursOnRefresh()
		{
			RedisHelper.FlushDatabase();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = new Mock<ICacheStack>();
			var cacheLayerOne = new Mock<ICacheLayer>();
			var extensionOne = new RedisRemoteEvictionExtension(connection, new ICacheLayer[] { cacheLayerOne.Object });
			extensionOne.Register(cacheStackMockOne.Object);

			var cacheStackMockTwo = new Mock<ICacheStack>();
			var cacheLayerTwo = new Mock<ICacheLayer>();
			var extensionTwo = new RedisRemoteEvictionExtension(connection, new ICacheLayer[] { cacheLayerTwo.Object });
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

			var expiry = DateTime.UtcNow.AddDays(1);
			await extensionOne.OnCacheUpdateAsync("TestKey", expiry);

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
			RedisHelper.FlushDatabase();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = new Mock<ICacheStack>();
			var cacheLayerOne = new Mock<ICacheLayer>();
			var extensionOne = new RedisRemoteEvictionExtension(connection, new ICacheLayer[] { cacheLayerOne.Object });
			extensionOne.Register(cacheStackMockOne.Object);

			var cacheStackMockTwo = new Mock<ICacheStack>();
			var cacheLayerTwo = new Mock<ICacheLayer>();
			var extensionTwo = new RedisRemoteEvictionExtension(connection, new ICacheLayer[] { cacheLayerTwo.Object });
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

			await extensionOne.OnCacheEvictionAsync("TestKey");

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(completionSource.Task, succeedingTask, "Subscriber response took too long");
			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the refreshed value");

			await Task.Delay(500);

			cacheLayerOne.Verify(c => c.EvictAsync("TestKey"), Times.Never, "Eviction took place locally where it should have been skipped");
			cacheLayerTwo.Verify(c => c.EvictAsync("TestKey"), Times.Once, "Eviction was skipped where it should have taken place locally");
		}

		[TestMethod]
		public async Task RemoteFlush()
		{
			RedisHelper.FlushDatabase();

			var connection = RedisHelper.GetConnection();

			var cacheStackMockOne = new Mock<ICacheStack>();
			var cacheLayerOne = new Mock<ICacheLayer>();
			var extensionOne = new RedisRemoteEvictionExtension(connection, new ICacheLayer[] { cacheLayerOne.Object });
			extensionOne.Register(cacheStackMockOne.Object);

			var cacheStackMockTwo = new Mock<ICacheStack>();
			var cacheLayerTwo = new Mock<ICacheLayer>();
			var extensionTwo = new RedisRemoteEvictionExtension(connection, new ICacheLayer[] { cacheLayerTwo.Object });
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

			await extensionOne.OnCacheFlushAsync();

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(completionSource.Task, succeedingTask, "Subscriber response took too long");
			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the flush");

			await Task.Delay(500);

			cacheLayerOne.Verify(c => c.FlushAsync(), Times.Never, "Flush took place locally where it should have been skipped");
			cacheLayerTwo.Verify(c => c.FlushAsync(), Times.Once, "Flush was skipped where it should have taken place locally");
		}
	}
}