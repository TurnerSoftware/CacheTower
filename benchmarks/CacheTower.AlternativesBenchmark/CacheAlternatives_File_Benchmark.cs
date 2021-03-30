using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.FileSystem.Json;
using CacheTower.Providers.FileSystem.Protobuf;
using EasyCaching.Disk;

namespace CacheTower.AlternativesBenchmark
{
	public class CacheAlternatives_File_Benchmark : BaseBenchmark
	{
		private const string DirectoryPath = "CacheAlternatives/FileCache";

		private readonly CacheStack CacheTowerJson;
		private readonly CacheStack CacheTowerProtobuf;
		private DefaultDiskCachingProvider EasyCaching;

		public CacheAlternatives_File_Benchmark()
		{
			CacheTowerJson = new CacheStack(new[] { new JsonFileCacheLayer(DirectoryPath) }, Array.Empty<ICacheExtension>());
			CacheTowerProtobuf = new CacheStack(new[] { new ProtobufFileCacheLayer(DirectoryPath) }, Array.Empty<ICacheExtension>());
		}

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

		[GlobalSetup]
		public void Setup()
		{
			CleanupFileSystem();

			// Easy Caching seems to generate a folder structure at initialization - this is required to be established for benchmarking.
			EasyCaching = new DefaultDiskCachingProvider("EasyCaching", new DiskOptions
			{
				DBConfig = new DiskDbOptions
				{
					BasePath = DirectoryPath
				}
			});
		}

		[Benchmark(Baseline = true)]
		public async Task<string> CacheTower_JsonFileCacheLayer()
		{
			return await CacheTowerJson.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
			{
				return Task.FromResult("Hello World");
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
		}

		[Benchmark]
		public async Task<string> CacheTower_ProtobufFileCacheLayer()
		{
			return await CacheTowerProtobuf.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
			{
				return Task.FromResult("Hello World");
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
		}

		[Benchmark]
		public async Task<string> EasyCaching_Disk()
		{
			return (await EasyCaching.GetAsync("GetOrSet_TestKey", () => Task.FromResult("Hello World"), TimeSpan.FromDays(1))).Value;
		}
	}
}
