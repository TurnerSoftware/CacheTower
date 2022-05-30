using System.IO;
using System.Threading.Tasks;
using CacheTower.Providers.FileSystem;
using CacheTower.Serializers.NewtonsoftJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Providers.FileSystem
{
	[TestClass]
	public class FileCacheLayerTests : BaseCacheLayerTests
	{
		private class TestCacheSerializer : ICacheSerializer
		{
			public int SerializeCount { get; private set; }
			public int DeserializeCount { get; private set; }

			public T Deserialize<T>(Stream stream)
			{
				DeserializeCount++;
				return default;
			}

			public void Serialize<T>(Stream stream, T value)
			{
				SerializeCount++;
			}
		}

		public const string DirectoryPath = "FileCacheLayer";

		[TestInitialize]
		public void Setup()
		{
			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, true);
			}
		}

		[TestMethod]
		public async Task CanLoadExistingManifest()
		{
			var testSerializer = new TestCacheSerializer();
			var cacheLayer = new FileCacheLayer(testSerializer, DirectoryPath);
			await using (cacheLayer)
			{
				//IsAvailableAsync triggers load of manifest which in turn creates it because it doesn't exist
				Assert.IsTrue(await cacheLayer.IsAvailableAsync("AnyKey"));
				//Disposing will do any other final saves to the manifest
			}
			Assert.AreEqual(2, testSerializer.SerializeCount);
			Assert.AreEqual(0, testSerializer.DeserializeCount);

			testSerializer = new TestCacheSerializer();
			cacheLayer = new FileCacheLayer(testSerializer, DirectoryPath);
			await using (cacheLayer)
			{
				Assert.IsTrue(await cacheLayer.IsAvailableAsync("AnyKey"));
			}
			Assert.AreEqual(1, testSerializer.SerializeCount);
			Assert.AreEqual(1, testSerializer.DeserializeCount);
		}

		[TestMethod]
		public async Task PersistentGetSetCache()
		{
			await AssertPersistentGetSetCacheAsync(() => new FileCacheLayer(new NewtonsoftJsonCacheSerializer(), DirectoryPath));
		}

		[TestMethod]
		public async Task GetSetCache()
		{
			await using var cacheLayer = new FileCacheLayer(new NewtonsoftJsonCacheSerializer(), DirectoryPath);
			await AssertGetSetCacheAsync(cacheLayer);
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await using var cacheLayer = new FileCacheLayer(new NewtonsoftJsonCacheSerializer(), DirectoryPath);
			await AssertCacheAvailabilityAsync(cacheLayer, true);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await using var cacheLayer = new FileCacheLayer(new NewtonsoftJsonCacheSerializer(), DirectoryPath);
			await AssertCacheEvictionAsync(cacheLayer);
		}

		[TestMethod]
		public async Task FlushFromCache()
		{
			await using var cacheLayer = new FileCacheLayer(new NewtonsoftJsonCacheSerializer(), DirectoryPath);
			await AssertCacheFlushAsync(cacheLayer);
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await using var cacheLayer = new FileCacheLayer(new NewtonsoftJsonCacheSerializer(), DirectoryPath);
			await AssertCacheCleanupAsync(cacheLayer);
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			await using var cacheLayer = new FileCacheLayer(new NewtonsoftJsonCacheSerializer(), DirectoryPath);
			await AssertComplexTypeCachingAsync(cacheLayer);
		}
	}
}
