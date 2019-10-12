using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.Memory;
using CacheTower.Benchmarks.CacheLayers;
using System.IO;
using CacheTower.Providers.FileSystem;
using CacheTower.Providers.FileSystem.Json;

namespace CacheTower.Benchmarks.Providers.FileSystem.Json
{
	[Config(typeof(ConfigSettings))]
	public class JsonFileCacheBenchmark : BaseCacheLayerBenchmark
	{
		public const string DirectoryPath = "FileSystemProviders/JsonFileCacheLayer";

		[GlobalSetup]
		public void Setup()
		{
			CacheLayerProvider = () => new JsonFileCacheLayer(DirectoryPath);

			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, true);
			}
		}

		[IterationSetup]
		public void IterationSetup()
		{
			Directory.CreateDirectory(DirectoryPath);
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
