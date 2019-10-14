using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace CacheTower
{
#if NETSTANDARD2_0
	public class CacheStack : ICacheStack, IDisposable
#elif NETSTANDARD2_1
	public class CacheStack : ICacheStack, IAsyncDisposable
#endif
	{
		private bool Disposed;

		private ConcurrentDictionary<string, AsyncLock> CacheKeyLock { get; } = new ConcurrentDictionary<string, AsyncLock>();

		private ICacheLayer[] CacheLayers { get; }
		private ICacheExtension[] Extensions { get; }

		private ICacheContext Context { get; }

		public CacheStack(ICacheContext context, ICacheLayer[] cacheLayers, ICacheExtension[] extensions)
		{
			Context = context;

			if (cacheLayers == null || cacheLayers.Length == 0)
			{
				throw new ArgumentException("There must be at least one cache layer", nameof(cacheLayers));
			}

			CacheLayers = cacheLayers;

			Extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
			
			foreach (var extension in extensions)
			{
				extension.Register(this);
			}
		}

		public IEnumerable<ICacheLayer> Layers => CacheLayers.AsEnumerable();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ThrowIfDisposed()
		{
			if (Disposed)
			{
				throw new ObjectDisposedException("CacheStack is disposed");
			}
		}

		public async Task CleanupAsync()
		{
			ThrowIfDisposed();
			
			foreach (var layer in CacheLayers)
			{
				await layer.CleanupAsync();
			}
		}

		public async Task EvictAsync(string cacheKey)
		{
			ThrowIfDisposed();

			if (cacheKey == null)
			{
				throw new ArgumentNullException(nameof(cacheKey));
			}

			foreach (var layer in CacheLayers)
			{
				await layer.EvictAsync(cacheKey);
			}
		}

		public async Task<CacheEntry<T>> SetAsync<T>(string cacheKey, T value, TimeSpan timeToLive)
		{
			ThrowIfDisposed();

			var cacheEntry = new CacheEntry<T>(value, DateTime.UtcNow, timeToLive);
			await SetAsync(cacheKey, cacheEntry);
			return cacheEntry;
		}

		public async Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			ThrowIfDisposed();

			if (cacheKey == null)
			{
				throw new ArgumentNullException(nameof(cacheKey));
			}

			foreach (var layer in CacheLayers)
			{
				await layer.SetAsync(cacheKey, cacheEntry);
			}
		}

		public async Task<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			ThrowIfDisposed();

			if (cacheKey == null)
			{
				throw new ArgumentNullException(nameof(cacheKey));
			}

			for (var i = 0; i < CacheLayers.Length; i++)
			{
				var cacheLayer = CacheLayers[i];
				if (await cacheLayer.IsAvailableAsync(cacheKey))
				{
					var cacheEntry = await cacheLayer.GetAsync<T>(cacheKey);
					if (cacheEntry != default)
					{
						//Populate previous cache layers
						for (; --i >= 0;)
						{
							cacheLayer = CacheLayers[i];
							await cacheLayer.SetAsync(cacheKey, cacheEntry);
						}
						
						return cacheEntry;
					}
				}
			}

			return default;
		}

		public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings)
		{
			ThrowIfDisposed();

			if (cacheKey == null)
			{
				throw new ArgumentNullException(nameof(cacheKey));
			}

			if (getter == null)
			{
				throw new ArgumentNullException(nameof(getter));
			}

			var cacheEntry = await GetAsync<T>(cacheKey);
			if (cacheEntry != default)
			{
				if (cacheEntry.HasElapsed(settings.StaleAfter))
				{
					if (cacheEntry.HasElapsed(settings.TimeToLive))
					{
						//Refresh the value in the current thread though short circuit if we're unable to establish a lock
						//If the lock isn't established, it will instead use the stale cache entry (even if past the allowed stale period)
						var refreshedCacheEntry = await RefreshValueAsync(cacheKey, getter, settings, exitIfLocked: true);
						if (refreshedCacheEntry != default)
						{
							cacheEntry = refreshedCacheEntry;
						}
					}
					else
					{
						//Refresh the value in the background
						_ = Task.Run(() => RefreshValueAsync(cacheKey, getter, settings, exitIfLocked: true));
					}
				}

				return cacheEntry.Value;
			}
			else
			{
				//Refresh the value in the current thread though because we have no old cache value, we have to lock and wait
				cacheEntry = await RefreshValueAsync(cacheKey, getter, settings, exitIfLocked: false);
			}

			return cacheEntry.Value;
		}

		private async Task<CacheEntry<T>> RefreshValueAsync<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings, bool exitIfLocked)
		{
			ThrowIfDisposed();

			//Technically this doesn't confirm it is locked, just the presence of a key
			//This does mean there is a race condition where multiple threads still get locked
			//Ultimately though, once each releases and they find the key in the cache, they will still exit without reprocessing
			if (exitIfLocked && CacheKeyLock.ContainsKey(cacheKey))
			{
				return default;
			}

			var lockObj = CacheKeyLock.GetOrAdd(cacheKey, (key) => new AsyncLock());
			
			using (await lockObj.LockAsync())
			{
				CacheEntry<T> cacheEntry = default;

				try
				{
					cacheEntry = await GetAsync<T>(cacheKey);

					//Confirm that once we have the lock, the latest cache entry still needs updating
					if (cacheEntry == null || cacheEntry.HasElapsed(settings.StaleAfter))
					{
						var oldValue = default(T);
						if (cacheEntry != null)
						{
							oldValue = cacheEntry.Value;
						}

						var value = await getter(oldValue, Context);
						cacheEntry = await SetAsync(cacheKey, value, settings.TimeToLive);

						foreach (var extension in Extensions)
						{
							if (extension is IValueRefreshExtension valueRefreshExtension)
							{
								await valueRefreshExtension.OnValueRefreshAsync(cacheKey, settings.TimeToLive);
							}
						}
					}
				}
				finally
				{
					CacheKeyLock.TryRemove(cacheKey, out var _);
				}

				return cacheEntry;
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
				foreach (var layer in CacheLayers)
				{
					if (layer is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}

				foreach (var extension in Extensions)
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

			foreach (var layer in CacheLayers)
			{
				if (layer is IDisposable disposableLayer)
				{
					disposableLayer.Dispose();
				}
				else if (layer is IAsyncDisposable asyncDisposableLayer)
				{
					await asyncDisposableLayer.DisposeAsync();
				}
			}

			foreach (var extension in Extensions)
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
