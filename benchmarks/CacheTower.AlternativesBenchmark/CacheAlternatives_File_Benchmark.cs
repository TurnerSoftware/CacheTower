using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.FileSystem.Json;
using CacheTower.Providers.FileSystem.Protobuf;
using EasyCaching.Disk;
using MonkeyCache.FileStore;

namespace CacheTower.AlternativesBenchmark
{
	public class CacheAlternatives_File_Benchmark : BaseBenchmark
	{
		[Params(1, 100, 1000)]
		public int Iterations;

		private const string DirectoryPath = "CacheAlternatives/FileCache";

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

		[Benchmark(Baseline = true)]
		public async Task CacheTower_JsonFileCacheLayer()
		{
			await using (var cacheStack = new CacheStack(new[] { new JsonFileCacheLayer(DirectoryPath) }, Array.Empty<ICacheExtension>()))
			{
				await LoopActionAsync(Iterations, async () =>
				{
					await cacheStack.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
					await cacheStack.GetAsync<int>("TestKey");
					await cacheStack.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
					{
						return Task.FromResult("Hello World");
					}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
				});
			}
		}

		[Benchmark]
		public async Task CacheTower_ProtobufFileCacheLayer()
		{
			await using (var cacheStack = new CacheStack(new[] { new ProtobufFileCacheLayer(DirectoryPath) }, Array.Empty<ICacheExtension>()))
			{
				await LoopActionAsync(Iterations, async () =>
				{
					await cacheStack.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
					await cacheStack.GetAsync<int>("TestKey");
					await cacheStack.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
					{
						return Task.FromResult("Hello World");
					}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
				});
			}
		}

		[Benchmark]
		public void MonkeyCache_FileStore()
		{
			var barrel = Barrel.Create(DirectoryPath);

			LoopAction(Iterations, () =>
			{
				barrel.Add("TestKey", 123, TimeSpan.FromDays(1));
				barrel.Get<int>("TestKey");

				var getOrSetResult = barrel.Get<string>("GetOrSet_TestKey");
				if (getOrSetResult == null)
				{
					barrel.Add("GetOrSet_TestKey", "Hello World", TimeSpan.FromDays(1));
				}
			});
		}

		[Benchmark]
		public async Task EasyCaching_Disk()
		{
			var easyCaching = new DefaultDiskCachingProvider("EasyCaching", new DiskOptions
			{
				DBConfig = new DiskDbOptions
				{
					BasePath = DirectoryPath
				}
			});
			
			await LoopActionAsync(Iterations, async () =>
			{
				await easyCaching.SetAsync("TestKey", 123, TimeSpan.FromDays(1));
				await easyCaching.GetAsync<int>("TestKey");

				var getOrSetResult = easyCaching.Get<string>("GetOrSet_TestKey");
				if (getOrSetResult == null)
				{
					easyCaching.Set("GetOrSet_TestKey", "Hello World", TimeSpan.FromDays(1));
				}
			});
		}
	}
}
