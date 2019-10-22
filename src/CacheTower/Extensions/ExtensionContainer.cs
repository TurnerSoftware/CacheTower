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
		private IRefreshWrapperExtension RefreshWrapperExtension { get; }
		private IValueRefreshExtension[] ValueRefreshExtensions { get; }
		private ICacheExtension[] AllExtensions { get; }

		public ExtensionContainer(ICacheExtension[] extensions)
		{
			var valueRefreshExtensions = new List<IValueRefreshExtension>();
			
			foreach (var extension in extensions)
			{
				if (RefreshWrapperExtension == null && extension is IRefreshWrapperExtension refreshWrapperExtension)
				{
					RefreshWrapperExtension = refreshWrapperExtension;
				}

				if (extension is IValueRefreshExtension valueRefreshExtension)
				{
					valueRefreshExtensions.Add(valueRefreshExtension);
				}
			}

			ValueRefreshExtensions = valueRefreshExtensions.ToArray();
			AllExtensions = extensions;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Register(ICacheStack cacheStack)
		{
			RefreshWrapperExtension?.Register(cacheStack);

			foreach (var extension in ValueRefreshExtensions)
			{
				extension.Register(cacheStack);
			}

			foreach (var extension in AllExtensions)
			{
				extension.Register(cacheStack);
			}

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<CacheEntry<T>> RefreshValueAsync<T>(string requestId, string cacheKey, Func<Task<CacheEntry<T>>> valueProvider, CacheSettings settings)
		{
			if (RefreshWrapperExtension == null)
			{
				return valueProvider();
			}
			else
			{
				return RefreshWrapperExtension.RefreshValueAsync(requestId, cacheKey, valueProvider, settings);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task OnValueRefreshAsync(string requestId, string cacheKey, TimeSpan timeToLive)
		{
			foreach (var extension in ValueRefreshExtensions)
			{
				await extension.OnValueRefreshAsync(requestId, cacheKey, timeToLive);
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
