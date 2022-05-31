using BenchmarkDotNet.Attributes;
using CacheTower.Benchmarks.Utils;
using CacheTower.Providers.Database.MongoDB;
using MongoFramework;

namespace CacheTower.Benchmarks.Providers.Database
{
	public class MongoDbCacheLayerBenchmark : BaseCacheLayerBenchmark
	{
		private IMongoDbConnection Connection { get; set; }

		[GlobalSetup]
		public void Setup()
		{
			Connection = MongoDbHelper.GetConnection();
			CacheLayerProvider = () => new MongoDbCacheLayer(Connection);
			MongoDbHelper.DropDatabase();
		}

		[IterationCleanup]
		public void IterationCleanup()
		{
			MongoDbHelper.DropDatabase();
		}
	}
}
