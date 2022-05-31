using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Utils;
using CacheTower.Providers.Redis;
using CacheTower.Serializers.Protobuf;

namespace CacheTower.Benchmarks.Providers.Redis
{
	public class RedisCacheLayerBenchmark : BaseCacheLayerBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			CacheLayerProvider = () => new RedisCacheLayer(RedisHelper.GetConnection(), ProtobufCacheSerializer.Instance);
		}

		[IterationSetup]
		public void PreIterationRedisCleanup()
		{
			RedisHelper.FlushDatabase();
		}

		[IterationCleanup]
		public void PostIterationRedisCleanup()
		{
			RedisHelper.FlushDatabase();
		}
	}
}
