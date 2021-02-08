using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace CacheTower.AlternativesBenchmark
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31), MemoryDiagnoser, MaxIterationCount(200), Orderer(SummaryOrderPolicy.FastestToSlowest)]
	public abstract class BaseBenchmark
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void LoopAction(int iterations, Action action)
		{
			for (var i = 0; i < iterations; i++)
			{
				action();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected async ValueTask LoopActionAsync(int iterations, Func<ValueTask> action)
		{
			for (var i = 0; i < iterations; i++)
			{
				await action();
			}
		}
	}
}
