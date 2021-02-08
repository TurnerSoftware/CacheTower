using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.AlternativesBenchmark
{
	public class OverheadBenchmark : BaseBenchmark
	{
		[Params(1, 100, 1000)]
		public int Iterations;

		[Benchmark]
		public void Overhead()
		{
			LoopAction(Iterations, () => { });
		}

		[Benchmark]
		public async Task OverheadAsync()
		{
			await LoopActionAsync(Iterations, () => new ValueTask());
		}
	}
}
