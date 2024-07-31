using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheTower.Extensions
{
	/// <summary>
	/// A basic delay-based cleanup extension for removing expired entries from cache layers.
	/// </summary>
	/// <remarks>
	/// Not all cache layers manage their own cleanup of expired entries.
	/// This calls <see cref="ICacheStack.CleanupAsync"/> which triggers the cleanup on each layer.
	/// </remarks>
	public class AutoCleanupExtension : ICacheExtension, IAsyncDisposable
	{
		/// <summary>
		/// The frequency at which an automatic cleanup is performed.
		/// </summary>
		public TimeSpan Frequency { get; }

		private Task? BackgroundTask;
		private readonly CancellationTokenSource TokenSource;

		/// <summary>
		/// Creates a new <see cref="AutoCleanupExtension"/> with the given <paramref name="frequency"/>.
		/// </summary>
		/// <param name="frequency">The frequency at which an automatic cleanup is performed.</param>
		/// <param name="cancellationToken">Optional cancellation token to end automatic cleanups.</param>
		public AutoCleanupExtension(TimeSpan frequency, CancellationToken cancellationToken = default)
		{
			if (frequency <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be greater than zero");
			}

			Frequency = frequency;
			TokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default);
		}

		/// <inheritdoc/>
		public void Register(ICacheStack cacheStack)
		{
			if (BackgroundTask is not null)
			{
				throw new InvalidOperationException($"{nameof(AutoCleanupExtension)} can only be registered to one {nameof(ICacheStack)}");
			}

			BackgroundTask = BackgroundCleanup(cacheStack);
		}

		private async Task BackgroundCleanup(ICacheStack cacheStack)
		{
			try
			{
				var cancellationToken = TokenSource.Token;
				while (!cancellationToken.IsCancellationRequested)
				{
					await Task.Delay(Frequency, cancellationToken).ConfigureAwait(false);
					cancellationToken.ThrowIfCancellationRequested();
					await cacheStack.CleanupAsync().ConfigureAwait(false);
				}
			}
			catch (OperationCanceledException) { }
		}

		/// <summary>
		/// Cancels the automatic cleanup and releases all resources that were being used.
		/// </summary>
		/// <returns></returns>
		public async ValueTask DisposeAsync()
		{
			TokenSource.Cancel();

			if (BackgroundTask is not null && !BackgroundTask.IsFaulted)
			{
				await BackgroundTask.ConfigureAwait(false);
			}
		}
	}
}
