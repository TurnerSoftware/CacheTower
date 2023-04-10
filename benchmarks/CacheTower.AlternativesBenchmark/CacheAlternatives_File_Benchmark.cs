using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.FileSystem;
using CacheTower.Serializers.NewtonsoftJson;
using CacheTower.Serializers.Protobuf;
using CacheTower.Serializers.SystemTextJson;
using EasyCaching.Disk;
using Microsoft.Extensions.Logging;

namespace CacheTower.AlternativesBenchmark
{
	public class CacheAlternatives_File_Benchmark : BaseBenchmark
	{
		private const string DirectoryPath = "CacheAlternatives/FileCache";

		private readonly CacheStack CacheTowerNewtonsoftJson;
		private readonly CacheStack CacheTowerSystemTextJson;
		private readonly CacheStack CacheTowerProtobuf;
		private DefaultDiskCachingProvider EasyCaching;

		public CacheAlternatives_File_Benchmark()
		{
			CacheTowerNewtonsoftJson = new CacheStack(null, new(new[] { new FileCacheLayer(new(DirectoryPath, NewtonsoftJsonCacheSerializer.Instance)) }));
			CacheTowerSystemTextJson = new CacheStack(null, new(new[] { new FileCacheLayer(new(DirectoryPath, SystemTextJsonCacheSerializer.Instance)) }));
			CacheTowerProtobuf = new CacheStack(null, new(new[] { new FileCacheLayer(new(DirectoryPath, ProtobufCacheSerializer.Instance)) }));
		}

		private static void CleanupFileSystem()
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
			EasyCaching = new DefaultDiskCachingProvider("EasyCaching", new[] { new EasyCaching.Serialization.Protobuf.DefaultProtobufSerializer("EasyCaching") }, new DiskOptions
			{
				DBConfig = new DiskDbOptions
				{
					BasePath = DirectoryPath
				}
			}, (ILoggerFactory)null);
		}

		[Benchmark(Baseline = true)]
		public async Task<string> CacheTower_FileCacheLayer_NewtonsoftJson()
		{
			return await CacheTowerNewtonsoftJson.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
			{
				return Task.FromResult("Hello World");
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
		}

		[Benchmark]
		public async Task<string> CacheTower_FileCacheLayer_SystemTextJson()
		{
			return await CacheTowerSystemTextJson.GetOrSetAsync<string>("GetOrSet_TestKey", (old) =>
			{
				return Task.FromResult("Hello World");
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
		}

		[Benchmark]
		public async Task<string> CacheTower_FileCacheLayer_Protobuf()
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
