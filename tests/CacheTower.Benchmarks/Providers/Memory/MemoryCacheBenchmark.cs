using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.Memory;
using CacheTower.Benchmarks.CacheLayers;

namespace CacheTower.Benchmarks.Providers.Memory
{
	[Config(typeof(ConfigSettings))]
	public class MemoryCacheComparisonBenchmark : BaseCacheLayerBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			CacheLayerProvider = () => new MemoryCacheLayer();
		}
	}
}
