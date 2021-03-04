using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Utils;
using CacheTower.Extensions.Redis;

namespace CacheTower.Benchmarks.Extensions.Redis
{
	public class RedisLockExtensionBenchmark : BaseRefreshWrapperExtensionBenchmark
	{
		protected override void SetupBenchmark()
		{
			base.SetupBenchmark();

			CacheExtension = new RedisLockExtension(RedisHelper.GetConnection());
			RedisHelper.FlushDatabase();
		}

		protected override void CleanupBenchmark()
		{
			base.CleanupBenchmark();
			RedisHelper.FlushDatabase();
		}
	}
}
