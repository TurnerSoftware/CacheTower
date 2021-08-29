using System.IO;
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
		public async Task PersistentGetSetCache()
		{
			await AssertPersistentGetSetCacheAsync(() => new ProtobufFileCacheLayer(DirectoryPath));
		}

		[TestMethod]
		public async Task GetSetCache()
		{
			await using var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertGetSetCacheAsync(cacheLayer);
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await using var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheAvailabilityAsync(cacheLayer, true);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await using var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheEvictionAsync(cacheLayer);
		}

		[TestMethod]
		public async Task FlushFromCache()
		{
			await using var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheFlushAsync(cacheLayer);
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await using var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertCacheCleanupAsync(cacheLayer);
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			await using var cacheLayer = new ProtobufFileCacheLayer(DirectoryPath);
			await AssertComplexTypeCachingAsync(cacheLayer);
		}
	}
}
