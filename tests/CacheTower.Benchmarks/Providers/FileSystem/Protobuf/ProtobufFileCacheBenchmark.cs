using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.Memory;
using CacheTower.Benchmarks.CacheLayers;
using System.IO;
using CacheTower.Providers.FileSystem.Protobuf;

namespace CacheTower.Benchmarks.Providers.FileSystem.Protobuf
{
	[Config(typeof(ConfigSettings))]
	public class ProtobufFileCacheBenchmark : BaseCacheLayerBenchmark
	{
		public const string DirectoryPath = "FileSystemProviders/ProtobufFileCacheLayer";

		[GlobalSetup]
		public void Setup()
		{
			ProtobufFileCacheLayer.ConfigureProtobuf();

			CacheLayerProvider = () => new ProtobufFileCacheLayer(DirectoryPath);

			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, true);
			}
		}

		[IterationCleanup]
		public void IterationCleanup()
		{
			Directory.Delete(DirectoryPath, true);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, true);
			}
		}
	}
}
