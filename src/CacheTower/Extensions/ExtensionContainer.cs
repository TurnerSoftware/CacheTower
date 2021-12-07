﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CacheTower.Extensions
{
	internal class ExtensionContainer : ICacheExtension, ICacheChangeExtension, ICacheRefreshCallSiteWrapperExtension, IAsyncDisposable, IDisposable
	{
		private bool Disposed;

		private bool HasCacheRefreshCallSiteWrapperExtension { get; }
		private ICacheRefreshCallSiteWrapperExtension? CacheRefreshCallSiteWrapperExtension { get; }
		private bool HasCacheChangeExtensions { get; }
		private ICacheChangeExtension[] CacheChangeExtensions { get; }
		private ICacheExtension[] AllExtensions { get; }

		public ExtensionContainer(ICacheExtension[] extensions)
		{
			if (extensions != null && extensions.Length > 0)
			{
				var cacheChangeExtensions = new List<ICacheChangeExtension>();

				foreach (var extension in extensions)
				{
					if (CacheRefreshCallSiteWrapperExtension == null && extension is ICacheRefreshCallSiteWrapperExtension remoteLockExtension)
					{
						HasCacheRefreshCallSiteWrapperExtension = true;
						CacheRefreshCallSiteWrapperExtension = remoteLockExtension;
					}

					if (extension is ICacheChangeExtension cacheChangeExtension)
					{
						HasCacheChangeExtensions = true;
						cacheChangeExtensions.Add(cacheChangeExtension);
					}
				}

				CacheChangeExtensions = cacheChangeExtensions.ToArray();
				AllExtensions = extensions;
			}
			else
			{
				CacheChangeExtensions = Array.Empty<ICacheChangeExtension>();
				AllExtensions = CacheChangeExtensions;
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
		public ValueTask<CacheEntry<T>> WithRefreshAsync<T>(string cacheKey, Func<ValueTask<CacheEntry<T>>> valueProvider, CacheSettings settings)
		{
			if (!HasCacheRefreshCallSiteWrapperExtension)
			{
				return valueProvider();
			}
			else
			{
				return CacheRefreshCallSiteWrapperExtension!.WithRefreshAsync(cacheKey, valueProvider, settings);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async ValueTask OnCacheUpdateAsync(string cacheKey, DateTime expiry, CacheUpdateType cacheUpdateType)
		{
			if (HasCacheChangeExtensions)
			{
				foreach (var extension in CacheChangeExtensions)
				{
					await extension.OnCacheUpdateAsync(cacheKey, expiry, cacheUpdateType);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async ValueTask OnCacheEvictionAsync(string cacheKey)
		{
			if (HasCacheChangeExtensions)
			{
				foreach (var extension in CacheChangeExtensions)
				{
					await extension.OnCacheEvictionAsync(cacheKey);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async ValueTask OnCacheFlushAsync()
		{
			if (HasCacheChangeExtensions)
			{
				foreach (var extension in CacheChangeExtensions)
				{
					await extension.OnCacheFlushAsync();
				}
			}
		}

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

		public void Dispose()
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
			}

			Disposed = true;
		}
	}
}
