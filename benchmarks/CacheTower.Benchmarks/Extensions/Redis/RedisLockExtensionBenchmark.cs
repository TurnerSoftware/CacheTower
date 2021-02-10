using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Utils;
using CacheTower.Extensions.Redis;

namespace CacheTower.Benchmarks.Extensions.Redis
{
	public class RedisLockExtensionBenchmark : BaseRefreshWrapperExtensionBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			CacheExtensionProvider = () => new RedisLockExtension(RedisHelper.GetConnection());
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
