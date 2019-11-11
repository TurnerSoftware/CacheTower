using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheTower.Extensions;

namespace CacheTower
{
#if NETSTANDARD2_0
	public class CacheStack<TContext> : ICacheStack<TContext>, IDisposable where TContext : ICacheContext
#elif NETSTANDARD2_1
	public class CacheStack<TContext> : ICacheStack<TContext>, IAsyncDisposable where TContext : ICacheContext
#endif
	{
		private bool Disposed;

		private Dictionary<string, IEnumerable<TaskCompletionSource<object>>> WaitingKeyRefresh { get; }

		private ICacheLayer[] CacheLayers { get; }

		private ExtensionContainer Extensions { get; }

		private TContext Context { get; }

		public CacheStack(TContext context, ICacheLayer[] cacheLayers, ICacheExtension[] extensions)
		{
			Context = context;

			if (cacheLayers == null || cacheLayers.Length == 0)
			{
				throw new ArgumentException("There must be at least one cache layer", nameof(cacheLayers));
			}

			CacheLayers = cacheLayers;

			Extensions = new ExtensionContainer(extensions);
			Extensions.Register(this);

			WaitingKeyRefresh = new Dictionary<string, IEnumerable<TaskCompletionSource<object>>>(StringComparer.Ordinal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ThrowIfDisposed()
		{
			if (Disposed)
			{
				throw new ObjectDisposedException("CacheStack is disposed");
			}
		}

		public async ValueTask CleanupAsync()
		{
			ThrowIfDisposed();
			
			for (int i = 0, l = CacheLayers.Length; i < l; i++)
			{
				var layer = CacheLayers[i];
				if (layer is ISyncCacheLayer syncLayer)
				{
					syncLayer.Cleanup();
				}
				else
				{
					await (layer as IAsyncCacheLayer).CleanupAsync();
				}
			}
		}

		public async ValueTask EvictAsync(string cacheKey)
		{
			ThrowIfDisposed();

			if (cacheKey == null)
			{
				throw new ArgumentNullException(nameof(cacheKey));
			}

			for (int i = 0, l = CacheLayers.Length; i < l; i++)
			{
				var layer = CacheLayers[i];
				if (layer is ISyncCacheLayer syncLayer)
				{
					syncLayer.Evict(cacheKey);
				}
				else
				{
					await (layer as IAsyncCacheLayer).EvictAsync(cacheKey);
				}
			}
		}

		public async ValueTask<CacheEntry<T>> SetAsync<T>(string cacheKey, T value, TimeSpan timeToLive)
		{
			ThrowIfDisposed();

			var expiry = DateTime.UtcNow + timeToLive;
			var cacheEntry = new CacheEntry<T>(value, expiry);
			await SetAsync(cacheKey, cacheEntry);
			return cacheEntry;
		}

		public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			ThrowIfDisposed();

			if (cacheKey == null)
			{
				throw new ArgumentNullException(nameof(cacheKey));
			}

			if (cacheEntry == null)
			{
				throw new ArgumentNullException(nameof(cacheEntry));
			}

			for (int i = 0, l = CacheLayers.Length; i < l; i++)
			{
				var layer = CacheLayers[i];
				if (layer is ISyncCacheLayer syncLayerOne)
				{
					syncLayerOne.Set(cacheKey, cacheEntry);
				}
				else
				{
					await (layer as IAsyncCacheLayer).SetAsync(cacheKey, cacheEntry);
				}
			}
		}

		public async ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			ThrowIfDisposed();

			if (cacheKey == null)
			{
				throw new ArgumentNullException(nameof(cacheKey));
			}

			for (var layerIndex = 0; layerIndex < CacheLayers.Length; layerIndex++)
			{
				var layer = CacheLayers[layerIndex];
				if (layer is ISyncCacheLayer syncLayer)
				{
					if (syncLayer.IsAvailable(cacheKey))
					{
						var cacheEntry = syncLayer.Get<T>(cacheKey);
						if (cacheEntry != default)
						{
							return cacheEntry;
						}
					}
				}
				else
				{
					var asyncLayer = layer as IAsyncCacheLayer;
					if (await asyncLayer.IsAvailableAsync(cacheKey))
					{
						var cacheEntry = await asyncLayer.GetAsync<T>(cacheKey);
						if (cacheEntry != default)
						{
							return cacheEntry;
						}
					}
				}
			}

			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async ValueTask<(int LayerIndex, CacheEntry<T> CacheEntry)> GetWithLayerIndexAsync<T>(string cacheKey)
		{
			for (var layerIndex = 0; layerIndex < CacheLayers.Length; layerIndex++)
			{
				var layer = CacheLayers[layerIndex];
				if (layer is ISyncCacheLayer syncLayer)
				{
					if (syncLayer.IsAvailable(cacheKey))
					{
						var cacheEntry = syncLayer.Get<T>(cacheKey);
						if (cacheEntry != default)
						{
							return (layerIndex, cacheEntry);
						}
					}
				}
				else
				{
					var asyncLayer = layer as IAsyncCacheLayer;
					if (await asyncLayer.IsAvailableAsync(cacheKey))
					{
						var cacheEntry = await asyncLayer.GetAsync<T>(cacheKey);
						if (cacheEntry != default)
						{
							return (layerIndex, cacheEntry);
						}
					}
				}
			}

			return default;
		}

		public async ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, TContext, Task<T>> getter, CacheSettings settings)
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

			var cacheEntryPoint = await GetWithLayerIndexAsync<T>(cacheKey);
			if (cacheEntryPoint != default)
			{
				var cacheEntry = cacheEntryPoint.CacheEntry;
				var currentTime = DateTime.UtcNow;
				if (cacheEntry.GetStaleDate(settings) < currentTime)
				{
					if (cacheEntry.Expiry < currentTime)
					{
						//Refresh the value in the current thread though short circuit if we're unable to establish a lock
						//If the lock isn't established, it will instead use the stale cache entry (even if past the allowed stale period)
						var refreshedCacheEntry = await RefreshValueAsync(cacheKey, getter, settings, waitForRefresh: false);
						if (refreshedCacheEntry != default)
						{
							cacheEntry = refreshedCacheEntry;
						}
					}
					else
					{
						//Refresh the value in the background
						_ = RefreshValueAsync(cacheKey, getter, settings, waitForRefresh: false);
					}
				}
				else if (cacheEntryPoint.LayerIndex > 0)
				{
					//If a lower-level cache is missing the latest data, attempt to set it in the background
					_ = BackPopulateCacheAsync(cacheEntryPoint.LayerIndex, cacheKey, cacheEntry);
				}

				return cacheEntry.Value;
			}
			else
			{
				//Refresh the value in the current thread though because we have no old cache value, we have to lock and wait
				return (await RefreshValueAsync(cacheKey, getter, settings, waitForRefresh: true)).Value;
			}
		}

		private async ValueTask BackPopulateCacheAsync<T>(int fromIndexExclusive, string cacheKey, CacheEntry<T> cacheEntry)
		{
			ThrowIfDisposed();

			var hasLock = false;
			lock (WaitingKeyRefresh)
			{
#if NETSTANDARD2_0
				hasLock = !WaitingKeyRefresh.ContainsKey(cacheKey);
				if (hasLock)
				{
					WaitingKeyRefresh[cacheKey] = Array.Empty<TaskCompletionSource<object>>();
				}
#elif NETSTANDARD2_1
				hasLock = WaitingKeyRefresh.TryAdd(cacheKey, Array.Empty<TaskCompletionSource<object>>());
#endif
			}

			if (hasLock)
			{
				try
				{
					for (; --fromIndexExclusive >= 0;)
					{
						var previousLayer = CacheLayers[fromIndexExclusive];
						if (previousLayer is ISyncCacheLayer prevSyncLayer)
						{
							if (prevSyncLayer.IsAvailable(cacheKey))
							{
								prevSyncLayer.Set(cacheKey, cacheEntry);
							}
						}
						else
						{
							var prevAsyncLayer = previousLayer as IAsyncCacheLayer;
							if (await prevAsyncLayer.IsAvailableAsync(cacheKey))
							{
								await prevAsyncLayer.SetAsync(cacheKey, cacheEntry);
							}
						}
					}
				}
				finally
				{
					UnlockWaitingTasks(cacheKey, cacheEntry);
				}
			}
		}

		private async ValueTask<CacheEntry<T>> RefreshValueAsync<T>(string cacheKey, Func<T, TContext, Task<T>> getter, CacheSettings settings, bool waitForRefresh)
		{
			ThrowIfDisposed();

			var hasLock = false;
			lock (WaitingKeyRefresh)
			{
#if NETSTANDARD2_0
				hasLock = !WaitingKeyRefresh.ContainsKey(cacheKey);
				if (hasLock)
				{
					WaitingKeyRefresh[cacheKey] = Array.Empty<TaskCompletionSource<object>>();
				}
#elif NETSTANDARD2_1
				hasLock = WaitingKeyRefresh.TryAdd(cacheKey, Array.Empty<TaskCompletionSource<object>>());
#endif
			}

			if (hasLock)
			{
				try
				{
					return await Extensions.RefreshValueAsync(cacheKey, async () =>
					{
						var previousEntry = await GetAsync<T>(cacheKey);

						var oldValue = default(T);
						if (previousEntry != null)
						{
							oldValue = previousEntry.Value;
						}

						var value = await getter(oldValue, Context);
						var refreshedEntry = await SetAsync(cacheKey, value, settings.TimeToLive);

						_ = Extensions.OnValueRefreshAsync(cacheKey, settings.TimeToLive);

						UnlockWaitingTasks(cacheKey, refreshedEntry);

						return refreshedEntry;
					}, settings);
				}
				catch
				{
					UnlockWaitingTasks(cacheKey, null);
					throw;
				}
			}
			else if (waitForRefresh)
			{
				var delayedResultSource = new TaskCompletionSource<object>();

				lock (WaitingKeyRefresh)
				{
					var waitList = new[] { delayedResultSource };
					if (WaitingKeyRefresh.TryGetValue(cacheKey, out var oldList))
					{
						WaitingKeyRefresh[cacheKey] = oldList.Concat(waitList);
					}
					else
					{
						WaitingKeyRefresh[cacheKey] = waitList;
					}
				}

				//Last minute check to confirm whether waiting is required
				var currentEntry = await GetAsync<T>(cacheKey);
				if (currentEntry != null && currentEntry.GetStaleDate(settings) > DateTime.UtcNow)
				{
					UnlockWaitingTasks(cacheKey, currentEntry);
					return currentEntry;
				}

				//Lock until we are notified to be unlocked
				var result = await delayedResultSource.Task;
				return result as CacheEntry<T>;
			}

			return default;
		}

		private void UnlockWaitingTasks(string cacheKey, CacheEntry cacheEntry)
		{
			lock (WaitingKeyRefresh)
			{
				if (WaitingKeyRefresh.TryGetValue(cacheKey, out var waitingTasks))
				{
					WaitingKeyRefresh.Remove(cacheKey);

					var tasks = waitingTasks.ToArray();
					for (int i = 0, l = tasks.Length; i < l; i++)
					{
						tasks[i].TrySetResult(cacheEntry);
					}
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
				foreach (var layer in CacheLayers)
				{
					if (layer is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}

				Extensions.Dispose();
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

			await Extensions.DisposeAsync();

			Disposed = true;
		}
#endif
	}
}
