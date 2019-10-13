using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.FileSystem;

namespace CacheTower.Benchmarks.Providers.FileSystem
{
	[Config(typeof(ConfigSettings))]
	public class FileCacheLayerBaseOverheadBenchmark : BaseCacheLayerBenchmark
	{
		public const string DirectoryPath = "FileSystemProviders/NoOpFileCacheLayer";

		private class NoOpFileCacheLayer : FileCacheLayerBase<ManifestEntry>
		{
			public NoOpFileCacheLayer(string directoryPath) : base(directoryPath, ".noop") { }

			protected override Task<T> Deserialize<T>(Stream stream)
			{
				return Task.FromResult(default(T));
			}

			protected override Task Serialize<T>(Stream stream, T value)
			{
				return Task.CompletedTask;
			}
		}

		[GlobalSetup]
		public void Setup()
		{
			CacheLayerProvider = () => new NoOpFileCacheLayer(DirectoryPath);

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
