using System;
using System.Diagnostics;

namespace CacheTower.Monitoring
{
	internal class DisposableTimer : IDisposable
	{
		private readonly Stopwatch Stopwatch;

		public DisposableTimer()
		{
			Stopwatch = Stopwatch.StartNew();
		}

		public TimeSpan TimeElapsed => Stopwatch.Elapsed;

		public void Dispose()
		{
			Stopwatch.Stop();
		}
	}
}