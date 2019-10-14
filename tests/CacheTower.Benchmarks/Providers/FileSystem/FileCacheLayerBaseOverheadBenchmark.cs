using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.FileSystem;

namespace CacheTower.Benchmarks.Providers.FileSystem
{
	public class FileCacheLayerBaseOverheadBenchmark : BaseFileCacheLayerBenchmark
	{
		private class NoOpFileCacheLayer : FileCacheLayerBase<ManifestEntry>
		{
			public NoOpFileCacheLayer(string directoryPath) : base(directoryPath, ".noop") { }

			protected override Task<T> DeserializeAsync<T>(Stream stream)
			{
				return Task.FromResult(default(T));
			}

			protected override Task SerializeAsync<T>(Stream stream, T value)
			{
				return Task.CompletedTask;
			}
		}

		[GlobalSetup]
		public void Setup()
		{
			DirectoryPath = "FileSystemProviders/NoOpFileCacheLayer";
			CacheLayerProvider = () => new NoOpFileCacheLayer(DirectoryPath);
		}
	}
}
