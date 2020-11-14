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
		public async Task EvictsFromChannelButNotFromRegisteredCacheStack()
		{
			RedisHelper.FlushDatabase();

			var connection = RedisHelper.GetConnection();

			var cacheStackMock = new Mock<ICacheStack>();
			var extension = new RedisRemoteEvictionExtension(connection, new ICacheLayer[] { new MemoryCacheLayer() });
			extension.Register(cacheStackMock.Object);

			var completionSource = new TaskCompletionSource<bool>();

			await connection.GetSubscriber().SubscribeAsync("CacheTower.RemoteEviction", (channel, value) =>
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

			await extension.OnValueRefreshAsync("TestKey", TimeSpan.FromDays(1));

			var succeedingTask = await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(completionSource.Task, succeedingTask, "Subscriber response took too long");
			Assert.IsTrue(completionSource.Task.Result, "Subscribers were not notified about the refreshed value");
			cacheStackMock.Verify(c => c.EvictAsync("TestKey"), Times.Never, "The CacheStack that published the refresh was told to evict its own cache");
		}
	}
}