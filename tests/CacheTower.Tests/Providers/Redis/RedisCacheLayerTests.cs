using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Providers.Redis;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StackExchange.Redis;

namespace CacheTower.Tests.Providers.Redis
{
	[TestClass]
	public class RedisCacheLayerTests : BaseCacheLayerTests
	{
		[TestInitialize]
		public void Setup()
		{
			RedisHelper.FlushDatabase();
		}

		[TestMethod]
		public async Task GetSetCache()
		{
			await AssertGetSetCacheAsync(new RedisCacheLayer(RedisHelper.GetConnection()));
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await AssertCacheAvailabilityAsync(new RedisCacheLayer(RedisHelper.GetConnection()), true);

			var connectionMock = new Mock<IConnectionMultiplexer>();
			var databaseMock = new Mock<IDatabase>();
			connectionMock.Setup(cm => cm.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(databaseMock.Object);
			databaseMock.Setup(db => db.PingAsync(It.IsAny<CommandFlags>())).Throws<Exception>();

			await AssertCacheAvailabilityAsync(new RedisCacheLayer(connectionMock.Object), false);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEvictionAsync(new RedisCacheLayer(RedisHelper.GetConnection()));
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await AssertCacheCleanupAsync(new RedisCacheLayer(RedisHelper.GetConnection()));
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			await AssertComplexTypeCachingAsync(new RedisCacheLayer(RedisHelper.GetConnection()));
		}
	}
}
