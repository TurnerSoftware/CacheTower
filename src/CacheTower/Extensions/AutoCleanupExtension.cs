using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheTower.Extensions
{
	public class AutoCleanupExtension : ICacheExtension, IDisposable
	{
		public TimeSpan Frequency { get; }

		private Task BackgroundTask { get; set; }

		private CancellationTokenSource TokenSource { get; }

		private bool Disposed;

		public AutoCleanupExtension(TimeSpan frequency, CancellationToken cancellationToken = default)
		{
			Frequency = frequency;
			TokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		}

		public void Register(ICacheStack cacheStack)
		{
			if (BackgroundTask != null)
			{
				throw new ArgumentException($"{nameof(AutoCleanupExtension)} can only be registered to one {nameof(ICacheStack)}");
			}

			BackgroundTask = BackgroundCleanup(cacheStack);
		}

		private async Task BackgroundCleanup(ICacheStack cacheStack)
		{
			var cancellationToken = TokenSource.Token;
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(Frequency, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				await cacheStack.CleanupAsync();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (Disposed)
			{
				return;
			}

			if (disposing)
			{
				TokenSource.Cancel();
				BackgroundTask.Wait();
			}

			Disposed = true;
		}
	}
}
