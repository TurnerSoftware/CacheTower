using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using CacheTower.Providers.Memory;

namespace CacheTower.Benchmarks.Extensions
{
	[Config(typeof(ConfigSettings))]
	public abstract class BaseExtensionsBenchmark
	{
		public class ConfigSettings : ManualConfig
		{
			public ConfigSettings()
			{
				Add(Job.Core.WithMaxIterationCount(200));
				Add(MemoryDiagnoser.Default);

				SummaryStyle = new BenchmarkDotNet.Reports.SummaryStyle(true, SizeUnit.B, TimeUnit.Nanosecond);
			}

		}
		protected Func<ICacheExtension> CacheExtensionProvider { get; set; }

		protected static ICacheStack CacheStack { get; } = new CacheStack(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());

		protected static async Task DisposeOf(ICacheExtension cacheExtension)
		{
			if (cacheExtension is IDisposable disposableExtension)
			{
				disposableExtension.Dispose();
			}
			else if (cacheExtension is IAsyncDisposable asyncDisposableExtension)
			{
				await asyncDisposableExtension.DisposeAsync();
			}
		}

		[Benchmark]
		public async Task Overhead()
		{
			var extension = CacheExtensionProvider();
			await DisposeOf(extension);
		}

		[Benchmark]
		public async Task Register()
		{
			var extension = CacheExtensionProvider();
			extension.Register(CacheStack);
			await DisposeOf(extension);
		}
	}
}
