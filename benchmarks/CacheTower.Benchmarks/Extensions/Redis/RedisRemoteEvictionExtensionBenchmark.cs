using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Utils;
using CacheTower.Extensions.Redis;
using CacheTower.Providers.Memory;

namespace CacheTower.Benchmarks.Extensions.Redis
{
	public class RedisRemoteEvictionExtensionBenchmark : BaseCacheChangeExtensionBenchmark
	{
		protected override void SetupBenchmark()
		{
			base.SetupBenchmark();

			CacheExtension = new RedisRemoteEvictionExtension(RedisHelper.GetConnection(), new ICacheLayer[] { new MemoryCacheLayer() });
			RedisHelper.FlushDatabase();
		}

		protected override void CleanupBenchmark()
		{
			base.CleanupBenchmark();
			RedisHelper.FlushDatabase();
		}
	}
}
