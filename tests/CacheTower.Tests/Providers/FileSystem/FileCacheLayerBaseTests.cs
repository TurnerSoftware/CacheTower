using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Providers.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Providers.FileSystem
{
	[TestClass]
	public class FileCacheLayerBaseTests : TestBase
	{
		private class TestFileCacheLayer : FileCacheLayerBase<ManifestEntry>
		{
			public TestFileCacheLayer(string directoryPath, string fileExtension) : base(directoryPath, fileExtension) { }

			public int SerializeCount { get; private set; }
			public int DeserializeCount { get; private set; }


			protected override T Deserialize<T>(Stream stream)
			{
				DeserializeCount++;
				return default;
			}

			protected override void Serialize<T>(Stream stream, T value)
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
			var cacheLayer = new TestFileCacheLayer(DirectoryPath, ".test");
			//IsAvailableAsync triggers load of manifest which in turn creates it because it doesn't exist
			Assert.IsTrue(await cacheLayer.IsAvailableAsync("AnyKey"));
			//Disposing will do any other final saves to the manifest
			await DisposeOf(cacheLayer);
			Assert.AreEqual(2, cacheLayer.SerializeCount);
			Assert.AreEqual(0, cacheLayer.DeserializeCount);

			cacheLayer = new TestFileCacheLayer(DirectoryPath, ".test");
			Assert.IsTrue(await cacheLayer.IsAvailableAsync("AnyKey"));
			await DisposeOf(cacheLayer);
			Assert.AreEqual(1, cacheLayer.SerializeCount);
			Assert.AreEqual(1, cacheLayer.DeserializeCount);
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
			var cacheLayer = new TestFileCacheLayer(DirectoryPath, null);
			await cacheLayer.SetAsync("NoFileExtension", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
			await DisposeOf(cacheLayer);

			var md5String = GetMD5String("NoFileExtension");
			var expectedFilePath = Path.Combine(DirectoryPath, md5String);
			Assert.IsTrue(File.Exists(expectedFilePath));
		}
		[TestMethod]
		public async Task CustomFileExtension()
		{
			var cacheLayer = new TestFileCacheLayer(DirectoryPath, ".mycustomfileextension");
			await cacheLayer.SetAsync("CustomFileExtension", new CacheEntry<int>(1, TimeSpan.FromDays(1)));
			await DisposeOf(cacheLayer);

			var md5String = GetMD5String("CustomFileExtension");
			var expectedFilePath = Path.Combine(DirectoryPath, md5String + ".mycustomfileextension");
			Assert.IsTrue(File.Exists(expectedFilePath));
		}
	}
}
