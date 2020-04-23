using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using CacheTower.Providers.Memory;
using Perfolizer.Horology;

namespace CacheTower.Benchmarks.Extensions
{
	[Config(typeof(ConfigSettings))]
	public abstract class BaseExtensionsBenchmark
	{
		public class ConfigSettings : ManualConfig
		{
			public ConfigSettings()
			{
				AddJob(Job.Default.WithRuntime(CoreRuntime.Core30).WithMaxIterationCount(200));
				AddDiagnoser(MemoryDiagnoser.Default);

				SummaryStyle = new BenchmarkDotNet.Reports.SummaryStyle(CultureInfo, true, SizeUnit.B, TimeUnit.Nanosecond);
			}
		}
		protected Func<ICacheExtension> CacheExtensionProvider { get; set; }

		protected static ICacheStack CacheStack { get; } = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());

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
