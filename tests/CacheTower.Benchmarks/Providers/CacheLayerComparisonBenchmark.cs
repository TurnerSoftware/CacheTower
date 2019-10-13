using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Providers.Database.MongoDB;
using CacheTower.Providers.Database.MongoDB;
using CacheTower.Providers.FileSystem.Json;
using CacheTower.Providers.FileSystem.Protobuf;
using CacheTower.Providers.Memory;
using ProtoBuf;

namespace CacheTower.Benchmarks.Providers
{
	[CoreJob, MemoryDiagnoser]
	public class CacheLayerComparisonBenchmark
	{
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

		private async Task BenchmarkWork(ICacheLayer cacheLayer)
		{
			for (var i = 0; i < 100; i++)
			{
				await cacheLayer.Get<int>("GetMiss_" + i);
			}

			//Set first 100 (simple type)
			for (var i = 0; i < 100; i++)
			{
				await cacheLayer.Set("SetMany_" + i, new CacheEntry<int>(1, DateTime.UtcNow.AddDays(-2), TimeSpan.FromDays(1)));
			}
			//Set last 100 (complex type)
			for (var i = 100; i < 200; i++)
			{
				await cacheLayer.Set("SetMany_" + i, new CacheEntry<ComplexType>(new ComplexType
				{
					ExampleString = "Hello World",
					ExampleNumber = 42,
					ExampleDate = new DateTime(2000, 1, 1),
					DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 }, { "C", 3 } }
				}, DateTime.UtcNow.AddDays(-2), TimeSpan.FromDays(1)));
			}

			//Get first 50 (simple type)
			for (var i = 0; i < 50; i++)
			{
				await cacheLayer.Get<int>("SetMany_" + i);
			}
			//Get last 50 (complex type)
			for (var i = 150; i < 200; i++)
			{
				await cacheLayer.Get<ComplexType>("SetMany_" + i);
			}

			//Evict 100
			for (var i = 100; i < 200; i++)
			{
				await cacheLayer.Evict("SetMany_" + i);
			}

			//Cleanup 100
			await cacheLayer.Cleanup();
		}

		[GlobalSetup]
		public void Setup()
		{
			MongoDbHelper.DropDatabase();
		}

		[Benchmark(Baseline = true)]
		public async Task MemoryCacheLayer()
		{
			var cacheLayer = new MemoryCacheLayer();
			await BenchmarkWork(cacheLayer);
		}

		[Benchmark]
		public async Task JsonFileCacheLayer()
		{
			var directoryPath = "CacheLayerComparison/JsonFileCacheLayer";
			await using (var cacheLayer = new JsonFileCacheLayer(directoryPath))
			{
				await BenchmarkWork(cacheLayer);
			}
			Directory.Delete(directoryPath, true);
		}

		[Benchmark]
		public async Task ProtobufFileCacheLayer()
		{
			var directoryPath = "CacheLayerComparison/ProtobufFileCacheLayer";
			await using (var cacheLayer = new ProtobufFileCacheLayer(directoryPath))
			{
				await BenchmarkWork(cacheLayer);
			}
			Directory.Delete(directoryPath, true);
		}

		[Benchmark]
		public async Task MongoDbCacheLayer()
		{
			var cacheLayer = new MongoDbCacheLayer(MongoDbHelper.GetConnection());
			await BenchmarkWork(cacheLayer);
			await MongoDbHelper.DropDatabaseAsync();
		}
	}
}
