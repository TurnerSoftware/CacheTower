using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Extensions
{
	public abstract class BaseCacheChangeExtensionBenchmark : BaseExtensionsBenchmark
	{
		public DateTime BenchmarkValue;

		protected override void SetupBenchmark()
		{
			BenchmarkValue = DateTime.UtcNow;
		}

		[Benchmark]
		public async Task OnCacheUpdate()
		{
			var extension = CacheExtension as ICacheChangeExtension;
			await extension.OnCacheUpdateAsync("OnCacheUpdate_CacheKey", BenchmarkValue, CacheUpdateType.AddOrUpdateEntry);
		}

		[Benchmark]
		public async Task OnCacheEviction()
		{
			var extension = CacheExtension as ICacheChangeExtension;
			await extension.OnCacheEvictionAsync("OnCacheEviction_CacheKey");
		}

		[Benchmark]
		public async Task OnCacheFlush()
		{
			var extension = CacheExtension as ICacheChangeExtension;
			await extension.OnCacheFlushAsync();
		}
	}
}
