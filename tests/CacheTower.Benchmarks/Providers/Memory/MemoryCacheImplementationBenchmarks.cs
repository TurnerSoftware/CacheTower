using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CacheTower.Providers.Memory;

namespace CacheTower.Benchmarks.Providers.Memory
{
	[SimpleJob(RuntimeMoniker.NetCoreApp30), MemoryDiagnoser]
	public class MemoryCacheImplementationBenchmarks
	{
		private static readonly ConcurrentDictionary<string, object> _concurrent = new ConcurrentDictionary<string, object>();
		private static readonly Dictionary<string, object> _regular = new Dictionary<string, object>();
		private static readonly MemoryCacheLayer _memoryCacheLayer = new MemoryCacheLayer();

		[Params(100, 100_000, 1_000_000, 10_000_000)]
		public int MemberCount { get; set; }
		public Random Rand { get; set; } = new Random();
		public ReaderWriterLockSlim LockObj { get; } = new ReaderWriterLockSlim();

		[GlobalSetup]
		public void Setup()
		{
			for (var i = 0; i < MemberCount; i++)
			{
				var val = i.ToString();
				_concurrent[val] = val;
				_regular[val] = val;
				_memoryCacheLayer.Set(val, new CacheEntry<string>(val, DateTime.UtcNow));
			}
		}

		/// <summary>
		/// Note: This is here purely to see how much overhead any specific implementation of MemoryCacheLayer has
		///	It will in no way perform as well as the most raw/direct implementations
		/// </summary>
		/// <returns></returns>
		[Benchmark]
		public object MemoryCacheLayer()
		{
			return _memoryCacheLayer.Get<string>(Rand.Next(0, MemberCount).ToString());
		}

		[Benchmark]
		public object Dictionary()
		{
			LockObj.EnterReadLock();
			try
			{
				return _regular.TryGetValue(Rand.Next(0, MemberCount).ToString(), out var obj) ? obj : null;
			}
			finally
			{
				LockObj.ExitReadLock();
			}
		}

		[Benchmark]
		public object ConcurrentDictionary()
		{
			return _concurrent.TryGetValue(Rand.Next(0, MemberCount).ToString(), out var obj) ? obj : null;
		}
	}
}
