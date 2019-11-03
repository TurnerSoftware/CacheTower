using CacheTower.Providers.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace CacheTower.Tests
{
	[TestClass]
	public class CacheStackStressTests : TestBase
	{
		[DataRow(100)]
		[DataRow(10000)]
		[DataTestMethod]
		public async Task SimulatenousGetOrSet_CacheMiss(int iterations)
		{
			var cacheStack = new CacheStack<ICacheContext>(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());

			var allTasks = new List<Task<int>>(iterations);
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			for (var i = 0; i < iterations; i++)
			{
				var index = i;
				var iterationTask = cacheStack.GetOrSetAsync<int>("SimulatenousGetOrSet", async (oldValue, context) => {
					await Task.Delay(100);
					return index;
				}, new CacheSettings(TimeSpan.FromDays(1)));

				allTasks.Add(iterationTask.AsTask());
			}

			if (stopwatch.Elapsed.TotalSeconds > 10)
			{
				Assert.Fail($"Took to long to start {iterations} simulatenous tasks");
			}

			var lastTaskResult = await allTasks[iterations - 1];
			Assert.AreEqual(0, lastTaskResult);

			if (stopwatch.Elapsed.TotalSeconds > 10)
			{
				Assert.Fail($"Took to long to wait for the last task of {iterations} simulatenous tasks");
			}

			await Task.WhenAll(allTasks);

			if (stopwatch.Elapsed.TotalSeconds > 10)
			{
				Assert.Fail($"Took to long to wait for all {iterations} simulatenous tasks to complete");
			}

			await DisposeOf(cacheStack);
		}


		[DataRow(1000)]
		[DataRow(100000)]
		[DataTestMethod]
		public async Task SimulatenousGetOrSet_CacheMiss_UniqueKeys(int iterations)
		{
			var cacheStack = new CacheStack<ICacheContext>(null, new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());

			var allTasks = new List<Task<int>>(iterations);

			for (var i = 0; i < iterations; i++)
			{
				var index = i;
				var iterationTask = cacheStack.GetOrSetAsync<int>($"SimulatenousGetOrSet_{index}", async (oldValue, context) => {
					await Task.Delay(100);
					return index + 1;
				}, new CacheSettings(TimeSpan.FromDays(1)));

				allTasks.Add(iterationTask.AsTask());
			}

			var lastTaskResult = await allTasks[iterations - 1];

			Assert.AreEqual(iterations, lastTaskResult);

			await Task.WhenAll(allTasks);

			await DisposeOf(cacheStack);
		}
	}
}
