using BenchmarkDotNet.Attributes;
using CacheTower.Providers.FileSystem;
using CacheTower.Serializers.NewtonsoftJson;

namespace CacheTower.Benchmarks.Providers.FileSystem
{
	public class NewtonsoftJsonFileCacheBenchmark : BaseFileCacheLayerBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			DirectoryPath = "FileCache/NewtonsoftJson";
			CacheLayerProvider = () => new FileCacheLayer(NewtonsoftJsonCacheSerializer.Instance, DirectoryPath);
		}
	}
}
