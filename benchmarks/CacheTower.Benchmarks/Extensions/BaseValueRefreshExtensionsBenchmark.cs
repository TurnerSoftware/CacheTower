using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Extensions
{
	public abstract class BaseValueRefreshExtensionsBenchmark : BaseExtensionsBenchmark
	{
		public DateTime BenchmarkValue;

		[GlobalSetup]
		public void Setup()
		{
			BenchmarkValue = DateTime.UtcNow;
		}

		[Benchmark]
		public async Task OnValueRefresh()
		{
			var extension = CacheExtensionProvider() as ICacheChangeExtension;
			extension.Register(CacheStack);
			await extension.OnCacheUpdateAsync("OnValueRefreshCacheKey", BenchmarkValue);
			await DisposeOf(extension);
		}
	}
}
