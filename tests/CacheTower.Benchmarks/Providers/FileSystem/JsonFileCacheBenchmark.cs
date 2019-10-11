using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.Memory;
using CacheTower.Benchmarks.CacheLayers;
using System.IO;
using CacheTower.Providers.FileSystem;

namespace CacheTower.Benchmarks.Providers.FileSystem
{
	[CoreJob, MemoryDiagnoser]
	public class JsonFileCacheBenchmark : BaseCacheLayerBenchmark
	{
		public const string DirectoryPath = "FileSystemProviders/JsonFileCacheLayer";

		[GlobalSetup]
		public void Setup()
		{
			CacheLayerProvider = () => new JsonFileCacheLayer(DirectoryPath);
		}

		[IterationSetup]
		public void IterationSetup()
		{
			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, true);
			}

			Directory.CreateDirectory(DirectoryPath);
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
