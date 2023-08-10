using System;
using System.Threading.Tasks;
using CacheTower.Providers.Redis;
using CacheTower.Serializers.Protobuf;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
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
			await AssertGetSetCacheAsync(new RedisCacheLayer(RedisHelper.GetConnection(), new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance)));
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await AssertCacheAvailabilityAsync(new RedisCacheLayer(RedisHelper.GetConnection(), new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance)), true);

			var connectionMock = Substitute.For<IConnectionMultiplexer>();
			var databaseMock = Substitute.For<IDatabase>();
			connectionMock.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(databaseMock);
			databaseMock.PingAsync(Arg.Any<CommandFlags>()).Returns(Task.FromException<TimeSpan>(new Exception()));

			await AssertCacheAvailabilityAsync(new RedisCacheLayer(connectionMock, new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance)), false);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEvictionAsync(new RedisCacheLayer(RedisHelper.GetConnection(), new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance)));
		}

		[TestMethod]
		public async Task FlushFromCache()
		{
			await AssertCacheFlushAsync(new RedisCacheLayer(RedisHelper.GetConnection(), new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance)));
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await AssertCacheCleanupAsync(new RedisCacheLayer(RedisHelper.GetConnection(), new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance)));
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			await AssertComplexTypeCachingAsync(new RedisCacheLayer(RedisHelper.GetConnection(), new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance)));
		}
	}
}
