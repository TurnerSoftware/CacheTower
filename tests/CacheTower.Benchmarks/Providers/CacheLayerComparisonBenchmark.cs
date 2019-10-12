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

namespace CacheTower.Benchmarks.Providers
{
	[CoreJob, MemoryDiagnoser]
	public class CacheLayerComparisonBenchmark
	{
		private async Task BenchmarkWork(ICacheLayer cacheLayer)
		{
			await cacheLayer.Get<int>("GetMiss1");
			await cacheLayer.Get<string>("GetMiss2");
			await cacheLayer.Get<double>("GetMiss3");

			await cacheLayer.Set("GetHit", new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			await cacheLayer.Get<int>("GetHit");
			await cacheLayer.Set("GetHit", new CacheEntry<int>(2, DateTime.UtcNow, TimeSpan.FromDays(1)));
			await cacheLayer.Get<int>("GetHit");
			await cacheLayer.Set("GetHit", new CacheEntry<int>(3, DateTime.UtcNow, TimeSpan.FromDays(1)));

			for (var i = 0; i < 200; i++)
			{
				await cacheLayer.Set("SetMany_" + i, new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}
			for (var i = 0; i < 200; i++)
			{
				await cacheLayer.Get<int>("SetMany_" + i);
			}
		}

		[GlobalSetup]
		public void Setup()
		{
			MongoDbHelper.DropDatabase();
			CacheTower.Providers.FileSystem.Protobuf.ProtobufFileCacheLayer.ConfigureProtobuf();
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
