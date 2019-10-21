using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Providers.FileSystem
{
	public abstract class BaseFileCacheLayerBenchmark : BaseAsyncCacheLayerBenchmark
	{
		protected string DirectoryPath { get; set; }

		[IterationSetup]
		public void PreIterationDirectoryCleanup()
		{
			if (Directory.Exists(DirectoryPath))
			{
				Directory.Delete(DirectoryPath, true);
			}
		}

		[IterationCleanup]
		public void PostIterationDirectoryCleanup()
		{
			Directory.Delete(DirectoryPath, true);
		}
	}
}
