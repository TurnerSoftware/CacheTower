using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Utils;
using CacheTower.Extensions.Redis;
using CacheTower.Providers.Memory;

namespace CacheTower.Benchmarks.Extensions.Redis
{
	public class RedisRemoteEvictionExtensionBenchmark : BaseCacheChangeExtensionBenchmark
	{
		public override void Setup()
		{
			CacheExtensionProvider = () => new RedisRemoteEvictionExtension(RedisHelper.GetConnection(), new ICacheLayer[] { new MemoryCacheLayer() });
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
