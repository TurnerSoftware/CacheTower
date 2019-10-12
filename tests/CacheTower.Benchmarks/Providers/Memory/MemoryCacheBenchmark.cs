using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheTower.Providers.Memory;

namespace CacheTower.Benchmarks.Providers.Memory
{
	[Config(typeof(ConfigSettings))]
	public class MemoryCacheBenchmark : BaseCacheLayerBenchmark
	{
		[GlobalSetup]
		public void Setup()
		{
			CacheLayerProvider = () => new MemoryCacheLayer();
		}
	}
}
