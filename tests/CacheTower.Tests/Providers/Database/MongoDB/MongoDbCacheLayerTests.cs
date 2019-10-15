using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Providers.Database.MongoDB;
using CacheTower.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework;

namespace CacheTower.Tests.Providers.Database.MongoDB
{
	[TestClass]
	public class MongoDbCacheLayerTests : BaseCacheLayerTests
	{
		[TestInitialize]
		public async Task Setup()
		{
			await MongoDbHelper.DropDatabaseAsync();
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await MongoDbHelper.DropDatabaseAsync();
		}

		[TestMethod]
		public async Task GetSetCache()
		{
			await AssertGetSetCacheAsync(new MongoDbCacheLayer(MongoDbHelper.GetConnection()));
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await AssertCacheAvailabilityAsync(new MongoDbCacheLayer(MongoDbHelper.GetConnection()), true);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEvictionAsync(new MongoDbCacheLayer(MongoDbHelper.GetConnection()));
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await AssertCacheCleanupAsync(new MongoDbCacheLayer(MongoDbHelper.GetConnection()));
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			await AssertComplexTypeCachingAsync(new MongoDbCacheLayer(MongoDbHelper.GetConnection()));
		}
	}
}
