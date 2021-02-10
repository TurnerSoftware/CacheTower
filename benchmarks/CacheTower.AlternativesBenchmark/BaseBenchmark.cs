using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace CacheTower.AlternativesBenchmark
{
	[SimpleJob(RuntimeMoniker.NetCoreApp50), MemoryDiagnoser, MaxIterationCount(200), Orderer(SummaryOrderPolicy.FastestToSlowest)]
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
