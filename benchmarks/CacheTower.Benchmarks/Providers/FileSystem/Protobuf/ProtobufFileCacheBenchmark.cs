using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.Memory;
using System.IO;
using CacheTower.Providers.FileSystem.Protobuf;

namespace CacheTower.Benchmarks.Providers.FileSystem.Protobuf
{
	public class ProtobufFileCacheBenchmark : BaseFileCacheLayerBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			DirectoryPath = "FileSystemProviders/ProtobufFileCacheLayer";
			CacheLayerProvider = () => new ProtobufFileCacheLayer(DirectoryPath);
		}
	}
}
