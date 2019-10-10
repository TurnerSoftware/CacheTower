using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.CacheLayers
{
	[CoreJob, MemoryDiagnoser]
	public class MemoryCacheComparisonBenchmark : BaseCacheLayerBenchmark
	{
		[Params(1000, 100000)]
		public int Iterations;

		[Benchmark]
		public async Task MemoryCacheGet()
		{
			await GetCacheBenchmark(new MemoryCacheLayer(), Iterations);
		}
		[Benchmark]
		public async Task MemoryCacheSet()
		{
			await SetCacheBenchmark(new MemoryCacheLayer(), Iterations);
		}
	}
}
