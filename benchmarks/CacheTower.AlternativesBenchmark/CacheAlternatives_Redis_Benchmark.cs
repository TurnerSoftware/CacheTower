﻿using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheManager.Core;
using CacheTower.AlternativesBenchmark.Utils;
using CacheTower.Providers.Redis;
using CacheTower.Serializers.Protobuf;
using EasyCaching.Redis;
using EasyCaching.Serialization.Protobuf;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace CacheTower.AlternativesBenchmark
{
	public class CacheAlternatives_Redis_Benchmark : BaseBenchmark
	{
		private readonly CacheStack CacheTower;
		private readonly ICacheManager<ProtobufCacheItem> CacheManager;
		private readonly DefaultRedisCachingProvider EasyCaching;
		private readonly IntelligentHack.IntelligentCache.RedisCache IntelligentCache;

		public CacheAlternatives_Redis_Benchmark()
		{
			CacheTower = new CacheStack(null, new(new[] { new RedisCacheLayer(RedisHelper.GetConnection(), new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance)) }));
			CacheManager = CacheFactory.Build<ProtobufCacheItem>(b =>
			{
				b.WithRedisConfiguration("redisLocal", "localhost:6379,ssl=false");
				b.WithRedisCacheHandle("redisLocal", true);
				b.WithProtoBufSerializer();
			});

			var easyCachingRedisOptions = new RedisOptions
			{
				DBConfig = new RedisDBOptions
				{
					Configuration = "localhost:6379,ssl=false"
				}
			};
			EasyCaching = new DefaultRedisCachingProvider("EasyCaching", 
				new[] { new RedisDatabaseProvider("EasyCaching", easyCachingRedisOptions) }, 
				new[] { new DefaultProtobufSerializer("EasyCaching") }, 
				easyCachingRedisOptions,
				(ILoggerFactory)null
			);
			IntelligentCache = new IntelligentHack.IntelligentCache.RedisCache(RedisHelper.GetConnection(), string.Empty);
		}

		[GlobalSetup]
		public void Setup()
		{
			RedisHelper.FlushDatabase();
		}

		[Benchmark(Baseline = true)]
		public async Task<string> CacheTower_RedisCacheLayer()
		{
			return await CacheTower.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
			{
				return Task.FromResult("Hello World");
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
		}

		[Serializable]
		[ProtoContract]
		public class ProtobufCacheItem
		{
			[ProtoMember(1)]
			public string Value { get; set; }
		}

		[Benchmark]
		public string CacheManager_Redis()
		{
			return CacheManager.GetOrAdd("GetOrSet_TestKey", (key) =>
			{
				return new ProtobufCacheItem
				{
					Value = "Hello World"
				};
			}).Value;
		}

		[Benchmark]
		public async Task<string> EasyCaching_Redis()
		{
			return (await EasyCaching.GetAsync("GetOrSet_TestKey", () => Task.FromResult("Hello World"), TimeSpan.FromDays(1))).Value;
		}

		[Benchmark]
		public async Task<string> IntelligentCache_Redis()
		{
			return await IntelligentCache.GetSetAsync("GetOrSet_TestKey", (cancellationToken) => Task.FromResult("Hello World"), TimeSpan.FromDays(1));
		}
	}
}
