using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Providers.FileSystem
{
	public abstract class BaseFileCacheLayerBenchmark : BaseCacheLayerBenchmark
	{
		protected string DirectoryPath { get; set; }

		private void CleanupFileSystem()
		{
			var attempts = 0;
			while (attempts < 5)
			{
				try
				{
					if (Directory.Exists(DirectoryPath))
					{
						Directory.Delete(DirectoryPath, true);
					}

					break;
				}
				catch
				{
					Thread.Sleep(200);
				}
				attempts++;
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
