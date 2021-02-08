using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CacheTower.Providers.Memory;

namespace CacheTower.Benchmarks
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31), MemoryDiagnoser]
	public class CacheStackRefreshWaitingBenchmark
	{
		private readonly static CacheStack CacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());

		[Params(10, 10000)]
		public int WorkIterations { get; set; }

		[Benchmark]
		public async ValueTask Benchmark()
		{
			var gettingLockSource = new TaskCompletionSource<bool>();
			var continueRefreshSource = new TaskCompletionSource<bool>();

			_ = CacheStack.GetOrSetAsync<int>("RefreshWaiting", async (old) =>
			{
				gettingLockSource.SetResult(true);
				await continueRefreshSource.Task;
				return 42;
			}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));

			await gettingLockSource.Task;

			var awaitingTasks = new List<Task>();

			for (var i = 0; i < WorkIterations; i++)
			{
				var task = CacheStack.GetOrSetAsync<int>("RefreshWaiting", (old) =>
				{
					return Task.FromResult(99);
				}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromDays(1)));
				awaitingTasks.Add(task.AsTask());
			}

			continueRefreshSource.SetResult(true);

			await Task.WhenAll(awaitingTasks);

			//Remove value for next run
			await CacheStack.EvictAsync("RefreshWaiting");
		}
	}
}
