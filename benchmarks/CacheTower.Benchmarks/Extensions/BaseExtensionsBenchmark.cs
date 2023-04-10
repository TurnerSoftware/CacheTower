using System;
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
				AddJob(Job.Default.WithRuntime(CoreRuntime.Core60).WithMaxIterationCount(200));
				AddDiagnoser(MemoryDiagnoser.Default);

				SummaryStyle = new BenchmarkDotNet.Reports.SummaryStyle(CultureInfo, true, SizeUnit.B, TimeUnit.Nanosecond);
			}
		}

		protected ICacheExtension CacheExtension { get; set; }

		protected virtual void SetupBenchmark() { }
		protected virtual void CleanupBenchmark() { }

		protected static CacheStack CacheStack { get; } = new CacheStack(null, new(new[] { new MemoryCacheLayer() }));

		[GlobalSetup]
		public void Setup()
		{
			SetupBenchmark();
			CacheExtension.Register(CacheStack);
		}

		[GlobalCleanup]
		public async Task CleanupAsync()
		{
			CleanupBenchmark();
			await CacheStack.DisposeAsync();
		}
	}
}
