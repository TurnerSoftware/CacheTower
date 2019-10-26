using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using CacheTower.Providers.Memory;

namespace CacheTower.Benchmarks
{
	[SimpleJob(RuntimeMoniker.NetCoreApp30), MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
	public class RealCostOfCacheStackBenchmark
	{
		private class ComplexType
		{
			public string ExampleString { get; set; }
			public int ExampleNumber { get; set; }
			public DateTime ExampleDate { get; set; }
			public Dictionary<string, int> DictionaryOfNumbers { get; set; }
		}

		[Benchmark(Baseline = true)]
		public void MemoryCacheLayer_Independent()
		{
			using (var cacheLayer = new MemoryCacheLayer())
			{
				//Get 100 misses
				for (var i = 0; i < 100; i++)
				{
					cacheLayer.Get<int>("GetMiss_" + i);
				}

				var startDate = DateTime.UtcNow.AddDays(-50);

				//Set first 100 (simple type)
				for (var i = 0; i < 100; i++)
				{
					cacheLayer.Set("Comparison_" + i, new CacheEntry<int>(1, startDate.AddDays(i), TimeSpan.FromDays(1)));
				}
				//Set last 100 (complex type)
				for (var i = 100; i < 200; i++)
				{
					cacheLayer.Set("Comparison_" + i, new CacheEntry<ComplexType>(new ComplexType
					{
						ExampleString = "Hello World",
						ExampleNumber = 42,
						ExampleDate = new DateTime(2000, 1, 1),
						DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 }, { "C", 3 } }
					}, startDate.AddDays(i - 100), TimeSpan.FromDays(1)));
				}

				//Get first 50 (simple type)
				for (var i = 0; i < 50; i++)
				{
					cacheLayer.Get<int>("Comparison_" + i);
				}
				//Get last 50 (complex type)
				for (var i = 150; i < 200; i++)
				{
					cacheLayer.Get<ComplexType>("Comparison_" + i);
				}

				//Evict middle 100
				for (var i = 50; i < 150; i++)
				{
					cacheLayer.Evict("Comparison_" + i);
				}

				//Cleanup outer 100
				cacheLayer.Cleanup();
			}
		}

		[Benchmark]
		public async Task MemoryCacheLayer_CacheStack()
		{
			await using (var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, null))
			{
				//Get 100 misses
				for (var i = 0; i < 100; i++)
				{
					await cacheStack.GetAsync<int>("GetMiss_" + i);
				}

				var startDate = DateTime.UtcNow.AddDays(-50);

				//Set first 100 (simple type)
				for (var i = 0; i < 100; i++)
				{
					await cacheStack.SetAsync("Comparison_" + i, new CacheEntry<int>(1, startDate.AddDays(i), TimeSpan.FromDays(1)));
				}
				//Set last 100 (complex type)
				for (var i = 100; i < 200; i++)
				{
					await cacheStack.SetAsync("Comparison_" + i, new CacheEntry<ComplexType>(new ComplexType
					{
						ExampleString = "Hello World",
						ExampleNumber = 42,
						ExampleDate = new DateTime(2000, 1, 1),
						DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 }, { "C", 3 } }
					}, startDate.AddDays(i - 100), TimeSpan.FromDays(1)));
				}

				//Get first 50 (simple type)
				for (var i = 0; i < 50; i++)
				{
					await cacheStack.GetAsync<int>("Comparison_" + i);
				}
				//Get last 50 (complex type)
				for (var i = 150; i < 200; i++)
				{
					await cacheStack.GetAsync<ComplexType>("Comparison_" + i);
				}

				//Evict middle 100
				for (var i = 50; i < 150; i++)
				{
					await cacheStack.EvictAsync("Comparison_" + i);
				}

				//Cleanup outer 100
				await cacheStack.CleanupAsync();
			}
		}

		[Benchmark]
		public void MemoryCacheLayer_Indepedent_GetOrSet()
		{
			using (var cacheLayer = new MemoryCacheLayer())
			{
				//Set first 100 (simple type)
				for (var i = 0; i < 100; i++)
				{
					var result = cacheLayer.Get<int>("Comparison_" + i);
					if (result == null)
					{
						cacheLayer.Set("Comparison_" + i, new CacheEntry<int>(1, DateTime.UtcNow, TimeSpan.FromDays(1)));
					}
				}

				//Set last 200 (complex type)
				for (var i = 100; i < 200; i++)
				{
					var result = cacheLayer.Get<int>("Comparison_" + i);
					if (result == null)
					{
						cacheLayer.Set("Comparison_" + i, new CacheEntry<ComplexType>(new ComplexType
						{
							ExampleString = "Hello World",
							ExampleNumber = 42,
							ExampleDate = new DateTime(2000, 1, 1),
							DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 }, { "C", 3 } }
						}, DateTime.UtcNow, TimeSpan.FromDays(1)));
					}
				}
			}
		}

		[Benchmark]
		public async Task MemoryCacheLayer_CacheStack_GetOrSet()
		{
			await using (var cacheStack = new CacheStack(null, new[] { new MemoryCacheLayer() }, null))
			{
				//Set first 200 (simple type)
				for (var i = 0; i < 200; i++)
				{
					await cacheStack.GetOrSetAsync<int>("Comparision_" + i, (old, context) =>
					{
						return Task.FromResult(1);
					}, new CacheSettings(TimeSpan.FromDays(1)));
				}

				//Set last 200 (complex type)
				for (var i = 200; i < 400; i++)
				{
					await cacheStack.GetOrSetAsync<ComplexType>("Comparision_" + i, (old, context) =>
					{
						return Task.FromResult(new ComplexType
						{
							ExampleString = "Hello World",
							ExampleNumber = 42,
							ExampleDate = new DateTime(2000, 1, 1),
							DictionaryOfNumbers = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 }, { "C", 3 } }
						});
					}, new CacheSettings(TimeSpan.FromDays(1)));
				}
			}
		}
	}
}
