using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Providers.Database.MongoDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoFramework;

namespace CacheTower.Tests.Providers.Database.MongoDB
{
	[TestClass]
	public class MongoDbCacheLayerTests : BaseCacheLayerTests
	{
		private static string ConnectionString => Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost";

		private static string GetDatabaseName()
		{
			return "CacheTowerTests";
		}

		private static IMongoDbConnection GetConnection()
		{
			var urlBuilder = new MongoUrlBuilder(ConnectionString)
			{
				DatabaseName = GetDatabaseName()
			};
			return MongoDbConnection.FromUrl(urlBuilder.ToMongoUrl());
		}

		private static async Task DropDatabase()
		{
			await GetConnection().Client.DropDatabaseAsync(GetDatabaseName());
		}

		[TestInitialize]
		public async Task Setup()
		{
			await DropDatabase();
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await DropDatabase();
		}

		[TestMethod]
		public async Task GetSetCache()
		{
			await AssertGetSetCache(new MongoDbCacheLayer(GetConnection()));
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await AssertCacheAvailability(new MongoDbCacheLayer(GetConnection()), true);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEviction(new MongoDbCacheLayer(GetConnection()));
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await AssertCacheCleanup(new MongoDbCacheLayer(GetConnection()));
		}
	}
}
