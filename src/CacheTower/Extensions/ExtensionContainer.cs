using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower.Extensions
{
	public class ExtensionContainer : ICacheExtension, IValueRefreshExtension, IRefreshWrapperExtension,
#if NETSTANDARD2_0
		IDisposable
#elif NETSTANDARD2_1
		IAsyncDisposable
#endif
	{
		private bool Disposed;

		private bool HasRefreshWrapperExtension { get; }
		private IRefreshWrapperExtension RefreshWrapperExtension { get; }
		private bool HasValueRefreshExtensions { get; }
		private IValueRefreshExtension[] ValueRefreshExtensions { get; }
		private ICacheExtension[] AllExtensions { get; }

		public ExtensionContainer(ICacheExtension[] extensions)
		{
			if (extensions != null && extensions.Length > 0)
			{
				var valueRefreshExtensions = new List<IValueRefreshExtension>();

				foreach (var extension in extensions)
				{
					if (RefreshWrapperExtension == null && extension is IRefreshWrapperExtension refreshWrapperExtension)
					{
						HasRefreshWrapperExtension = true;
						RefreshWrapperExtension = refreshWrapperExtension;
					}

					if (extension is IValueRefreshExtension valueRefreshExtension)
					{
						HasValueRefreshExtensions = true;
						valueRefreshExtensions.Add(valueRefreshExtension);
					}
				}

				ValueRefreshExtensions = valueRefreshExtensions.ToArray();
				AllExtensions = extensions;
			}
			else
			{
				ValueRefreshExtensions = Array.Empty<IValueRefreshExtension>();
				AllExtensions = ValueRefreshExtensions;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Register(ICacheStack cacheStack)
		{
			foreach (var extension in AllExtensions)
			{
				extension.Register(cacheStack);
			}

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<CacheEntry<T>> RefreshValueAsync<T>(string cacheKey, Func<Task<CacheEntry<T>>> valueProvider, CacheSettings settings)
		{
			if (!HasRefreshWrapperExtension)
			{
				return valueProvider();
			}
			else
			{
				return RefreshWrapperExtension.RefreshValueAsync(cacheKey, valueProvider, settings);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task OnValueRefreshAsync(string cacheKey, TimeSpan timeToLive)
		{
			if (HasValueRefreshExtensions)
			{
				foreach (var extension in ValueRefreshExtensions)
				{
					await extension.OnValueRefreshAsync(cacheKey, timeToLive);
				}
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
