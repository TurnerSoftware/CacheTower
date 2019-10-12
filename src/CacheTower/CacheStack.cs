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
	public class CacheStack : IDisposable
#elif NETSTANDARD2_1
	public class CacheStack : IAsyncDisposable
#endif
	{
		private bool Disposed = false;

		private ConcurrentDictionary<string, AsyncLock> CacheKeyLock { get; } = new ConcurrentDictionary<string, AsyncLock>();

		private ICacheLayer[] CacheLayers { get; }

		private ICacheContext Context { get; }

		public CacheStack(ICacheContext context, ICacheLayer[] cacheLayers)
		{
			Context = context;
			CacheLayers = cacheLayers;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ThrowIfDisposed()
		{
			if (Disposed)
			{
				throw new ObjectDisposedException("CacheStack is disposed");
			}
		}

		public async Task Cleanup()
		{
			ThrowIfDisposed();
			
			foreach (var layer in CacheLayers)
			{
				await layer.Cleanup();
			}
		}

		public async Task Evict(string cacheKey)
		{
			ThrowIfDisposed();

			foreach (var layer in CacheLayers)
			{
				await layer.Evict(cacheKey);
			}
		}

		public async Task<CacheEntry<T>> Set<T>(string cacheKey, T value, TimeSpan timeToLive)
		{
			ThrowIfDisposed();

			var cacheEntry = new CacheEntry<T>(value, DateTime.UtcNow, timeToLive);
			await Set(cacheKey, cacheEntry);
			return cacheEntry;
		}

		public async Task Set<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			ThrowIfDisposed();

			foreach (var layer in CacheLayers)
			{
				await layer.Set(cacheKey, cacheEntry);
			}
		}

		public async Task<CacheEntry<T>> Get<T>(string cacheKey)
		{
			ThrowIfDisposed();

			for (var i = 0; i < CacheLayers.Length; i++)
			{
				var cacheLayer = CacheLayers[i];
				if (await cacheLayer.IsAvailable(cacheKey))
				{
					var cacheEntry = await cacheLayer.Get<T>(cacheKey);
					if (cacheEntry != default)
					{
						//Populate previous cache layers
						for (; --i >= 0;)
						{
							cacheLayer = CacheLayers[i];
							await cacheLayer.Set(cacheKey, cacheEntry);
						}
						
						return cacheEntry;
					}
				}
			}

			return default;
		}

		public async Task<T> GetOrSet<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings)
		{
			ThrowIfDisposed();

			var cacheEntry = await Get<T>(cacheKey);
			if (cacheEntry != default)
			{
				if (cacheEntry.HasElapsed(settings.StaleAfter))
				{
					if (cacheEntry.HasElapsed(settings.TimeToLive))
					{
						//Refresh the value in the current thread though short circuit if we're unable to establish a lock
						//If the lock isn't established, it will instead use the stale cache entry (even if past the allowed stale period)
						var refreshedCacheEntry = await RefreshValue(cacheKey, getter, settings, exitIfLocked: true);
						if (refreshedCacheEntry != default)
						{
							cacheEntry = refreshedCacheEntry;
						}
					}
					else
					{
						//Refresh the value in the background
						_ = Task.Run(() => RefreshValue(cacheKey, getter, settings, exitIfLocked: true));
					}
				}

				return cacheEntry.Value;
			}
			else
			{
				//Refresh the value in the current thread though because we have no old cache value, we have to lock and wait
				cacheEntry = await RefreshValue(cacheKey, getter, settings, exitIfLocked: false);
			}

			return cacheEntry.Value;
		}

		private async Task<CacheEntry<T>> RefreshValue<T>(string cacheKey, Func<T, ICacheContext, Task<T>> getter, CacheSettings settings, bool exitIfLocked)
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
					cacheEntry = await Get<T>(cacheKey);

					//Confirm that once we have the lock, the latest cache entry still needs updating
					if (cacheEntry == null || cacheEntry.HasElapsed(settings.StaleAfter))
					{
						var oldValue = default(T);
						if (cacheEntry != null)
						{
							oldValue = cacheEntry.Value;
						}

						var value = await getter(oldValue, Context);
						cacheEntry = await Set(cacheKey, value, settings.TimeToLive);
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
					if (layer is IDisposable disposableLayer)
					{
						disposableLayer.Dispose();
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

			Disposed = true;
		}
#endif
	}
}
