using System;
using System.Threading.Tasks;
using CacheTower.Providers.Redis;
using CacheTower.Serializers.Protobuf;
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
			RedisHelper.ResetState();
		}

		[TestMethod]
		public async Task GetSetCache()
		{
			await AssertGetSetCacheAsync(new RedisCacheLayer(RedisHelper.GetConnection(), ProtobufCacheSerializer.Instance));
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await AssertCacheAvailabilityAsync(new RedisCacheLayer(RedisHelper.GetConnection(), ProtobufCacheSerializer.Instance), true);

			var connectionMock = new Mock<IConnectionMultiplexer>();
			var databaseMock = new Mock<IDatabase>();
			connectionMock.Setup(cm => cm.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(databaseMock.Object);
			databaseMock.Setup(db => db.PingAsync(It.IsAny<CommandFlags>())).Throws<Exception>();

			await AssertCacheAvailabilityAsync(new RedisCacheLayer(connectionMock.Object, ProtobufCacheSerializer.Instance), false);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEvictionAsync(new RedisCacheLayer(RedisHelper.GetConnection(), ProtobufCacheSerializer.Instance));
		}

		[TestMethod]
		public async Task FlushFromCache()
		{
			await AssertCacheFlushAsync(new RedisCacheLayer(RedisHelper.GetConnection(), ProtobufCacheSerializer.Instance));
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await AssertCacheCleanupAsync(new RedisCacheLayer(RedisHelper.GetConnection(), ProtobufCacheSerializer.Instance));
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			await AssertComplexTypeCachingAsync(new RedisCacheLayer(RedisHelper.GetConnection(), ProtobufCacheSerializer.Instance));
		}
	}
}
