using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CacheTower.Extensions
{
	internal class ExtensionContainer : ICacheExtension, ICacheChangeExtension, IDistributedLockExtension, IAsyncDisposable
	{
		private bool Disposed;

		private readonly bool HasDistributedLockExtension;
		private readonly IDistributedLockExtension? DistributedLockExtension;
		private readonly bool HasCacheChangeExtensions;
		private readonly ICacheChangeExtension[] CacheChangeExtensions;
		private readonly ICacheExtension[] AllExtensions;

		public ExtensionContainer(ICacheExtension[] extensions)
		{
			if (extensions != null && extensions.Length > 0)
			{
				var cacheChangeExtensions = new List<ICacheChangeExtension>();

				foreach (var extension in extensions)
				{
					if (!HasDistributedLockExtension && extension is IDistributedLockExtension distributedLockExtension)
					{
						HasDistributedLockExtension = true;
						DistributedLockExtension = distributedLockExtension;
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
		public ValueTask<DistributedLock> AwaitAccessAsync(string cacheKey)
		{
			if (HasDistributedLockExtension)
			{
				return DistributedLockExtension!.AwaitAccessAsync(cacheKey);
			}

			return new(DistributedLock.NotEnabled(cacheKey));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async ValueTask OnCacheUpdateAsync(string cacheKey, DateTime expiry, CacheUpdateType cacheUpdateType)
		{
			if (HasCacheChangeExtensions)
			{
				foreach (var extension in CacheChangeExtensions)
				{
					await extension.OnCacheUpdateAsync(cacheKey, expiry, cacheUpdateType).ConfigureAwait(false);
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
					await extension.OnCacheEvictionAsync(cacheKey).ConfigureAwait(false);
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
					await extension.OnCacheFlushAsync().ConfigureAwait(false);
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
					await asyncDisposable.DisposeAsync().ConfigureAwait(false);
				}
			}

			Disposed = true;
		}
	}
}
