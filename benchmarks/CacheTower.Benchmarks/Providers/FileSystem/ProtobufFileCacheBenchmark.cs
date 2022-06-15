using BenchmarkDotNet.Attributes;
using CacheTower.Providers.FileSystem;
using CacheTower.Serializers.Protobuf;

namespace CacheTower.Benchmarks.Providers.FileSystem
{
	public class ProtobufFileCacheBenchmark : BaseFileCacheLayerBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			DirectoryPath = "FileCache/Protobuf";
			CacheLayerProvider = () => new FileCacheLayer(new(DirectoryPath, ProtobufCacheSerializer.Instance));
		}
	}
}
