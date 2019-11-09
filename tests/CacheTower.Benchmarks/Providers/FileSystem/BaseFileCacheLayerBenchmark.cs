using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Providers.FileSystem
{
	public abstract class BaseFileCacheLayerBenchmark : BaseAsyncCacheLayerBenchmark
	{
		protected string DirectoryPath { get; set; }

		private void CleanupFileSystem()
		{
			if (Directory.Exists(DirectoryPath))
			{
				try
				{
					Directory.Delete(DirectoryPath, true);
				}
				catch
				{
					Thread.Sleep(100);
					if (Directory.Exists(DirectoryPath))
					{
						Directory.Delete(DirectoryPath, true);
					}
				}
			}
		}

		[IterationSetup]
		public void PreIterationDirectoryCleanup()
		{
			CleanupFileSystem();
		}

		[IterationCleanup]
		public void PostIterationDirectoryCleanup()
		{
			CleanupFileSystem();
		}
	}
}
