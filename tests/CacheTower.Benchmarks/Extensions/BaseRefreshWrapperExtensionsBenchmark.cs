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
			await extension.RefreshValueAsync<int>("a611d518-a94e-4b83-a3ac-e8edc13222c5", "RefreshValue", () =>
			{
				return Task.FromResult(new CacheEntry<int>(5, DateTime.UtcNow, TimeSpan.FromDays(1)));
			}, new CacheSettings(TimeSpan.FromDays(1)));
			await DisposeOf(extension);
		}
	}
}
