using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using CacheTower.Benchmarks.Utils;
using CacheTower.Providers.Database.MongoDB;
using CacheTower.Providers.FileSystem.Json;
using CacheTower.Providers.FileSystem.Protobuf;
using CacheTower.Providers.Memory;
using CacheTower.Providers.Redis;
using ProtoBuf;

namespace CacheTower.Benchmarks.Providers
{
	[SimpleJob(RuntimeMoniker.NetCoreApp50), MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
	public class CacheLayerComparisonBenchmark
	{
		[Params(1, 10)]
		public int WorkIterations { get; set; }

		[ProtoContract]
		private class ComplexType
		{
			[ProtoMember(1)]
			public string ExampleString { get; set; }
			[ProtoMember(2)]
			public int ExampleNumber { get; set; }
			[ProtoMember(3)]
			public DateTime ExampleDate { get; set; }
			[ProtoMember(4)]
			public Dictionary<string, int> DictionaryOfNumbers { get; set; }
		}

		protected async ValueTask BenchmarkWork(ICacheLayer cacheLayer)
		{
			for (var iterationCount = 0; iterationCount < WorkIterations; iterationCount++)
			{
				//Get 100 misses
				for (var i = 0; i < 100; i++)
				{
					await cacheLayer.GetAsync<int>("GetMiss_" + i);
				}

				var startDate = DateTime.UtcNow.AddDays(-50);

				//Set first 100 (simple type)
				for (var i = 0; i < 100; i++)
				{
					await cacheLayer.SetAsync("Comparison_" + i, new CacheEntry<int>(1, startDate.AddDays(i) + TimeSpan.FromDays(1)));
				}
				//Set last 100 (complex type)
				for (var i = 100; i < 200; i++)
				{
					await cacheLayer.SetAsync("Comparison_" + i, new CacheEntry<ComplexType>(new ComplexType
					{
						ExampleString = "Hello World",
						ExampleNumber = 42,
						ExampleDate = new DateTime(2000, 1, 1),
						DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 }, { "C", 3 } }
					}, startDate.AddDays(i - 100) + TimeSpan.FromDays(1)));
				}

				//Get first 50 (simple type)
				for (var i = 0; i < 50; i++)
				{
					await cacheLayer.GetAsync<int>("Comparison_" + i);
				}
				//Get last 50 (complex type)
				for (var i = 150; i < 200; i++)
				{
					await cacheLayer.GetAsync<ComplexType>("Comparison_" + i);
				}

				//Evict middle 100
				for (var i = 50; i < 150; i++)
				{
					await cacheLayer.EvictAsync("Comparison_" + i);
				}

				//Cleanup outer 100
				await cacheLayer.CleanupAsync();
			}
		}

		[GlobalSetup]
		public void Setup()
		{
			MongoDbHelper.DropDatabase();
		}

		[Benchmark(Baseline = true)]
		public async ValueTask MemoryCacheLayer()
		{
			var cacheLayer = new MemoryCacheLayer();
			await BenchmarkWork(cacheLayer);
		}

		[Benchmark]
		public async ValueTask JsonFileCacheLayer()
		{
			var directoryPath = "CacheLayerComparison/JsonFileCacheLayer";
			await using (var cacheLayer = new JsonFileCacheLayer(directoryPath))
			{
				await BenchmarkWork(cacheLayer);
			}
			Directory.Delete(directoryPath, true);
		}

		[Benchmark]
		public async ValueTask ProtobufFileCacheLayer()
		{
			var directoryPath = "CacheLayerComparison/ProtobufFileCacheLayer";
			await using (var cacheLayer = new ProtobufFileCacheLayer(directoryPath))
			{
				await BenchmarkWork(cacheLayer);
			}
			Directory.Delete(directoryPath, true);
		}

		[Benchmark]
		public async ValueTask MongoDbCacheLayer()
		{
			var cacheLayer = new MongoDbCacheLayer(MongoDbHelper.GetConnection());
			await BenchmarkWork(cacheLayer);
			await MongoDbHelper.DropDatabaseAsync();
		}

		[Benchmark]
		public async ValueTask RedisCacheLayer()
		{
			var cacheLayer = new RedisCacheLayer(RedisHelper.GetConnection());
			await BenchmarkWork(cacheLayer);
			RedisHelper.FlushDatabase();
		}
	}
}
