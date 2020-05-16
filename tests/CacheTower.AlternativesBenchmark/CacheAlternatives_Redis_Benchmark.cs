using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheManager.Core;
using CacheTower.AlternativesBenchmark.Utils;
using CacheTower.Providers.Redis;
using EasyCaching.Redis;
using EasyCaching.Serialization.Protobuf;
using ProtoBuf;

namespace CacheTower.AlternativesBenchmark
{
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
			await using (var cacheStack = new CacheStack(new[] { new RedisCacheLayer(RedisHelper.GetConnection()) }, Array.Empty<ICacheExtension>()))
			{
				await LoopActionAsync(Iterations, async () =>
				{
					await cacheStack.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
					await cacheStack.GetAsync<int>("TestKey");
					await cacheStack.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
					{
						return Task.FromResult("Hello World");
					}, new CacheEntryLifetime(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
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

		[Benchmark]
		public async Task EasyCaching_Redis()
		{
			var redisOptions = new RedisOptions
			{
				DBConfig = new RedisDBOptions
				{
					Configuration = "localhost:6379,ssl=false"
				}
			};

			var easyCaching = new DefaultRedisCachingProvider("EasyCaching", new[] {
				new RedisDatabaseProvider("EasyCaching", redisOptions)
			}, 
			new[] { new DefaultProtobufSerializer("EasyCaching") }, redisOptions);

			await LoopActionAsync(Iterations, async () =>
			{
				await easyCaching.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
				await easyCaching.GetAsync<int>("TestKey");
				await easyCaching.GetAsync("GetOrSet_TestKey", () => Task.FromResult("Hello World"), TimeSpan.FromDays(1));
			});
		}
	}
}
