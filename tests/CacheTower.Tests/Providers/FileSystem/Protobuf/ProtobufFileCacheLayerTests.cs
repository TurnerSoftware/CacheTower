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
		}

		[TestMethod]
		public async Task GetSetCache()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertGetSetCacheAsync(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheAvailabilityAsync(cacheLayer, true);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheEvictionAsync(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheCleanupAsync(cacheLayer);
			await DisposeOf(cacheLayer);
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertComplexTypeCachingAsync(cacheLayer);
			await DisposeOf(cacheLayer);
		}
	}
}
