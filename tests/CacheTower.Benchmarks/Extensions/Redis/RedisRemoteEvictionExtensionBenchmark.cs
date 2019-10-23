using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Utils;
using CacheTower.Extensions.Redis;

namespace CacheTower.Benchmarks.Extensions.Redis
{
	public class RedisRemoteEvictionExtensionBenchmark : BaseValueRefreshExtensionsBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			CacheExtensionProvider = () => new RedisRemoteEvictionExtension(RedisHelper.GetConnection());
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
