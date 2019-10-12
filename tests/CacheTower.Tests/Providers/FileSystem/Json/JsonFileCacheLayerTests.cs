using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Providers.FileSystem;
using CacheTower.Providers.FileSystem.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Providers.FileSystem.Json
{
	[TestClass]
	public class JsonFileCacheLayerTests : BaseCacheLayerTests
	{
		public const string DirectoryPath = "FileSystemProviders/JsonFileCacheLayer";

		[TestInitialize]
		public void Setup()
		{
			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, true);
			}

			Directory.CreateDirectory(DirectoryPath);
		}

		private JsonFileCacheLayer CreateCacheLayer()
		{
			return new JsonFileCacheLayer(DirectoryPath);
		}

		[TestMethod]
		public async Task GetSetCache()
		{
			await AssertGetSetCache(CreateCacheLayer());
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await AssertCacheAvailability(CreateCacheLayer(), true);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEviction(CreateCacheLayer());
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await AssertCacheCleanup(CreateCacheLayer());
		}
	}
}
