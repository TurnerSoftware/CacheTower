using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Internal;

namespace CacheTower.Extensions
{
	public class ExtensionContainer : ICacheExtension, IValueRefreshExtension, IExternalLockExtension,
#if NETSTANDARD2_0
		IDisposable
#elif NETSTANDARD2_1
		IAsyncDisposable
#endif
	{
		private bool Disposed;
		private IExternalLockExtension ExternalLockExtension { get; }
		private IValueRefreshExtension[] ValueRefreshExtensions { get; }
		private ICacheExtension[] AllExtensions { get; }

		public ExtensionContainer(ICacheExtension[] extensions)
		{
			var valueRefreshExtensions = new List<IValueRefreshExtension>();
			
			foreach (var extension in extensions)
			{
				if (ExternalLockExtension == null && extension is IExternalLockExtension externalLockExtension)
				{
					ExternalLockExtension = externalLockExtension;
				}

				if (extension is IValueRefreshExtension valueRefreshExtension)
				{
					valueRefreshExtensions.Add(valueRefreshExtension);
				}
			}

			ValueRefreshExtensions = valueRefreshExtensions.ToArray();
			AllExtensions = extensions;
		}

		public void Register(ICacheStack cacheStack)
		{
			ExternalLockExtension?.Register(cacheStack);

			foreach (var extension in ValueRefreshExtensions)
			{
				extension.Register(cacheStack);
			}

			foreach (var extension in AllExtensions)
			{
				extension.Register(cacheStack);
			}

		}
		public async Task<IDisposable> LockAsync(Guid stackId, string cacheKey)
		{
			if (ExternalLockExtension == null)
			{
				return new NoopDisposable();
			}
			else
			{
				return await ExternalLockExtension.LockAsync(stackId, cacheKey);
			}
		}

		public async Task OnValueRefreshAsync(Guid stackId, string cacheKey, TimeSpan timeToLive)
		{
			foreach (var extension in ValueRefreshExtensions)
			{
				await extension.OnValueRefreshAsync(stackId, cacheKey, timeToLive);
			}
		}

#if NETSTANDARD2_0
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
				foreach (var extension in AllExtensions)
				{
					if (extension is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}

			Disposed = true;
		}
#elif NETSTANDARD2_1
		public async ValueTask DisposeAsync()
		{
			if (Disposed)
			{
				return;
			}
			
			foreach (var extension in AllExtensions)
			{
				if (extension is IDisposable disposable)
				{
					disposable.Dispose();
				}
				else if (extension is IAsyncDisposable asyncDisposable)
				{
					await asyncDisposable.DisposeAsync();
				}
			}

			Disposed = true;
		}
#endif
	}
}
