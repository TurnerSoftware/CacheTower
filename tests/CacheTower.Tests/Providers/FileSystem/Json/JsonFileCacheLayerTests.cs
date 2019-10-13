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

		[TestMethod]
		public async Task GetSetCache()
		{
			var cacheLayer = new JsonFileCacheLayer(DirectoryPath);
			await AssertGetSetCache(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			var cacheLayer = new JsonFileCacheLayer(DirectoryPath);
			await AssertCacheAvailability(cacheLayer, true);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			var cacheLayer = new JsonFileCacheLayer(DirectoryPath);
			await AssertCacheEviction(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			var cacheLayer = new JsonFileCacheLayer(DirectoryPath);
			await AssertCacheCleanup(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			var cacheLayer = new JsonFileCacheLayer(DirectoryPath);
			await AssertComplexTypeCaching(cacheLayer);
			await DisposeOf(cacheLayer);
		}
	}
}
