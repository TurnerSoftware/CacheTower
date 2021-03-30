using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Validators;

namespace CacheTower.AlternativesBenchmark
{
	[Config(typeof(Config))]
	public abstract class BaseBenchmark
	{
		public class Config : ManualConfig
		{
			public Config()
			{
				AddLogger(ConsoleLogger.Default);

				AddDiagnoser(MemoryDiagnoser.Default);
				AddColumn(StatisticColumn.OperationsPerSecond);
				AddColumnProvider(DefaultColumnProviders.Instance);

				WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

				AddValidator(JitOptimizationsValidator.FailOnError);

				AddJob(Job.Default
					.WithRuntime(CoreRuntime.Core50)
					.WithMaxIterationCount(200));

			}
		}
	}
}
