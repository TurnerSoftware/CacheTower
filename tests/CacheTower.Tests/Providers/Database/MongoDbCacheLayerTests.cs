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
using MongoFramework.Infrastructure.Indexing;
using Moq;

namespace CacheTower.Tests.Providers.Database
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
			EntityIndexWriter.ClearCache();

			await AssertCacheAvailabilityAsync(new MongoDbCacheLayer(MongoDbHelper.GetConnection()), true);

			var connectionMock = new Mock<IMongoDbConnection>();
			connectionMock.Setup(c => c.GetDatabase()).Throws<Exception>();
			EntityIndexWriter.ClearCache();

			await AssertCacheAvailabilityAsync(new MongoDbCacheLayer(connectionMock.Object), false);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEvictionAsync(new MongoDbCacheLayer(MongoDbHelper.GetConnection()));
		}

		[TestMethod]
		public async Task FlushFromCache()
		{
			await AssertCacheFlushAsync(new MongoDbCacheLayer(MongoDbHelper.GetConnection()));
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
