using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Utils;
using CacheTower.Providers.Redis;
using StackExchange.Redis;

namespace CacheTower.Benchmarks.Providers.Redis
{
	public class RedisCacheLayerBenchmark : BaseCacheLayerBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			CacheLayerProvider = () => new RedisCacheLayer(RedisHelper.GetConnection());
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
