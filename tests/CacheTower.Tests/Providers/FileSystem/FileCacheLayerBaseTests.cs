using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Providers.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Providers.FileSystem
{
	[TestClass]
	public class FileCacheLayerTests : TestBase
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

		public const string DirectoryPath = "FileSystemProviders/TestFileCacheLayer";

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
			var cacheLayer = new FileCacheLayer(testSerializer, DirectoryPath, ".test");
			await using (cacheLayer)
			{
				//IsAvailableAsync triggers load of manifest which in turn creates it because it doesn't exist
				Assert.IsTrue(await cacheLayer.IsAvailableAsync("AnyKey"));
				//Disposing will do any other final saves to the manifest
			}
			Assert.AreEqual(2, testSerializer.SerializeCount);
			Assert.AreEqual(0, testSerializer.DeserializeCount);

			testSerializer = new TestCacheSerializer();
			cacheLayer = new FileCacheLayer(testSerializer, DirectoryPath, ".test");
			await using (cacheLayer)
			{
				Assert.IsTrue(await cacheLayer.IsAvailableAsync("AnyKey"));
			}
			Assert.AreEqual(1, testSerializer.SerializeCount);
			Assert.AreEqual(1, testSerializer.DeserializeCount);
		}


		private string GetMD5String(string text)
		{
			using (var hashAlgo = MD5.Create())
			{
				var fileNameBytes = Encoding.UTF8.GetBytes(text);
				var md5Bytes = hashAlgo.ComputeHash(fileNameBytes);
				return BitConverter.ToString(md5Bytes).Replace("-", string.Empty);
			}
		}

		[TestMethod]
		public async Task NoFileExtension()
		{
			var testSerializer = new TestCacheSerializer();
			await using var cacheLayer = new FileCacheLayer(testSerializer, DirectoryPath, ".test");
			await cacheLayer.SetAsync("NoFileExtension", new CacheEntry<int>(1, TimeSpan.FromDays(1)));

			var md5String = GetMD5String("NoFileExtension");
			var expectedFilePath = Path.Combine(DirectoryPath, md5String);
			Assert.IsTrue(File.Exists(expectedFilePath));
		}
		[TestMethod]
		public async Task CustomFileExtension()
		{
			var testSerializer = new TestCacheSerializer();
			await using var cacheLayer = new FileCacheLayer(testSerializer, DirectoryPath, ".mycustomfileextension");
			await cacheLayer.SetAsync("CustomFileExtension", new CacheEntry<int>(1, TimeSpan.FromDays(1)));

			var md5String = GetMD5String("CustomFileExtension");
			var expectedFilePath = Path.Combine(DirectoryPath, md5String + ".mycustomfileextension");
			Assert.IsTrue(File.Exists(expectedFilePath));
		}
	}
}
