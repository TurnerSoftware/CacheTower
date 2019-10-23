using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheManager.Core;
using CacheTower.AlternativesBenchmark.Utils;
using CacheTower.Providers.Redis;
using ProtoBuf;

namespace CacheTower.AlternativesBenchmark
{
	[CoreJob, MemoryDiagnoser]
	public class CacheAlternatives_Redis_Benchmark : BaseBenchmark
	{
		[Params(1, 100, 1000)]
		public int Iterations;

		[IterationSetup]
		public void IterationSetup()
		{
			RedisHelper.FlushDatabase();
		}

		[Benchmark(Baseline = true)]
		public async Task CacheTower_RedisCacheLayer()
		{
			await using (var cacheStack = new CacheStack(null, new[] { new RedisCacheLayer(RedisHelper.GetConnection()) }, Array.Empty<ICacheExtension>()))
			{
				await LoopActionAsync(Iterations, async () =>
				{
					await cacheStack.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
					await cacheStack.GetAsync<int>("TestKey");
					await cacheStack.GetOrSetAsync<string>("GetOrSet_TestKey", (old, context) =>
					{
						return Task.FromResult("Hello World");
					}, new CacheSettings(TimeSpan.FromDays(1)));
				});
			}
		}

		[Serializable]
		[ProtoContract]
		public class ProtobufCacheItem
		{
			[ProtoMember(1)]
			public string Value { get; set; }
		}

		[Benchmark]
		public void CacheManager_Redis()
		{
			var cacheManager = CacheFactory.Build(b =>
			{
				b.WithRedisConfiguration("redisLocal", "localhost:6379,ssl=false");
				b.WithRedisCacheHandle("redisLocal", true);
				b.WithProtoBufSerializer();
			});

			using (cacheManager)
			{
				LoopAction(Iterations, () =>
				{
					cacheManager.Add("TestKey", 123);
					cacheManager.GetCacheItem("TestKey");
					cacheManager.GetOrAdd("GetOrSet_TestKey", (key) =>
					{
						return new ProtobufCacheItem
						{
							Value = "Hello World"
						};
					});
				});
			}
		}
	}
}
