using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Extensions
{
	public abstract class BaseCacheChangeExtensionBenchmark : BaseExtensionsBenchmark
	{
		public DateTime BenchmarkValue;

		[GlobalSetup]
		public virtual void Setup()
		{
			BenchmarkValue = DateTime.UtcNow;
		}

		[Benchmark]
		public async Task OnCacheUpdate()
		{
			var extension = CacheExtensionProvider() as ICacheChangeExtension;
			extension.Register(CacheStack);
			await extension.OnCacheUpdateAsync("OnCacheUpdate_CacheKey", BenchmarkValue);
			await DisposeOf(extension);
		}

		[Benchmark]
		public async Task OnCacheEviction()
		{
			var extension = CacheExtensionProvider() as ICacheChangeExtension;
			extension.Register(CacheStack);
			await extension.OnCacheEvictionAsync("OnCacheEviction_CacheKey");
			await DisposeOf(extension);
		}

		[Benchmark]
		public async Task OnCacheFlush()
		{
			var extension = CacheExtensionProvider() as ICacheChangeExtension;
			extension.Register(CacheStack);
			await extension.OnCacheFlushAsync();
			await DisposeOf(extension);
		}
	}
}
