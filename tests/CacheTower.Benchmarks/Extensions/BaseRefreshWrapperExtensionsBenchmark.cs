using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Extensions
{
	public abstract class BaseRefreshWrapperExtensionsBenchmark : BaseExtensionsBenchmark
	{
		[Benchmark]
		public async Task RefreshValue()
		{
			var extension = CacheExtensionProvider() as IRefreshWrapperExtension;
			extension.Register(CacheStack);
			await extension.RefreshValueAsync<int>("RefreshValue", () =>
			{
				return new ValueTask<CacheEntry<int>>(new CacheEntry<int>(5, TimeSpan.FromDays(1)));
			}, new CacheSettings(TimeSpan.FromDays(1)));
			await DisposeOf(extension);
		}
	}
}
