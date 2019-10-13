using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.Memory;
using System.IO;
using CacheTower.Providers.FileSystem;
using CacheTower.Providers.FileSystem.Json;

namespace CacheTower.Benchmarks.Providers.FileSystem.Json
{
	public class JsonFileCacheBenchmark : BaseFileCacheLayerBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			DirectoryPath = "FileSystemProviders/JsonFileCacheLayer";
			CacheLayerProvider = () => new JsonFileCacheLayer(DirectoryPath);
		}
	}
}
