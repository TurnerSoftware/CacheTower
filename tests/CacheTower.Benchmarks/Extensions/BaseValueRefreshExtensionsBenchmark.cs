using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Extensions
{
	public abstract class BaseValueRefreshExtensionsBenchmark : BaseExtensionsBenchmark
	{
		[Benchmark]
		public async Task OnValueRefresh()
		{
			var extension = CacheExtensionProvider() as IValueRefreshExtension;
			extension.Register(CacheStack);
			await extension.OnValueRefreshAsync("OnValueRefreshCacheKey", TimeSpan.FromDays(1));
			await DisposeOf(extension);
		}
	}
}
