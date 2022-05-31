using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Extensions
{
	public abstract class BaseRefreshWrapperExtensionBenchmark : BaseExtensionsBenchmark
	{
		[Benchmark]
		public async Task WithRefresh()
		{
			var extension = CacheExtension as ICacheRefreshCallSiteWrapperExtension;
			await extension.WithRefreshAsync<int, object>("RefreshValue", _ =>
			{
				return new ValueTask<CacheEntry<int>>(new CacheEntry<int>(5, TimeSpan.FromDays(1)));
			}, null, new CacheSettings(TimeSpan.FromDays(1)));
		}
	}
}
