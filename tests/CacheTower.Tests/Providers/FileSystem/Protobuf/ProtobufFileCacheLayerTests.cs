using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Providers.FileSystem.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Providers.FileSystem.Protobuf
{
	[TestClass]
	public class ProtobufFileCacheLayerTests : BaseCacheLayerTests
	{
		public const string DirectoryPath = "FileSystemProviders/ProtobufFileCacheLayer";

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
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertGetSetCache(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheAvailability(cacheLayer, true);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheEviction(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheCleanup(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertComplexTypeCaching(cacheLayer);
			await DisposeOf(cacheLayer);
		}
	}
}
