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
		public async Task EvictionOccursOnRefresh()
		{
			RedisHelper.FlushDatabase();

			var cacheStackMockOne = new Mock<ICacheStack>();
			var cacheLayerOne = new Mock<ICacheLayer>();
			var extensionOne = new RedisRemoteEvictionExtension(RedisHelper.GetConnection(), new ICacheLayer[] { cacheLayerOne.Object });
			extensionOne.Register(cacheStackMockOne.Object);

			var cacheStackMockTwo = new Mock<ICacheStack>();
			var cacheLayerTwo = new Mock<ICacheLayer>();
			var extensionTwo = new RedisRemoteEvictionExtension(RedisHelper.GetConnection(), new ICacheLayer[] { cacheLayerTwo.Object });
			extensionTwo.Register(cacheStackMockTwo.Object);

			var completionSource = new TaskCompletionSource<bool>();
			RedisHelper.GetConnection().GetSubscriber().Subscribe("CacheTower.RemoteEviction").OnMessage(channelMessage =>
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

			await extensionOne.OnValueRefreshAsync("TestKey", TimeSpan.FromDays(1));

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(completionSource.Task, succeedingTask, "Subscriber response took too long");
			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the refreshed value");

			cacheLayerOne.Verify(c => c.EvictAsync("TestKey"), Times.Never, "Eviction took place locally where it should have been skipped");
			cacheLayerTwo.Verify(c => c.EvictAsync("TestKey"), Times.Once, "Eviction was skipped where it should have taken place locally");
		}
	}
}