using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CacheTower.Extensions;

namespace CacheTower
{
	public class CacheStack : ICacheStack, IFlushableCacheStack, IAsyncDisposable
	{
		private bool Disposed;

		private Dictionary<string, IEnumerable<TaskCompletionSource<object>>> WaitingKeyRefresh { get; }

		private ICacheLayer[] CacheLayers { get; }

		private ExtensionContainer Extensions { get; }

		public CacheStack(ICacheLayer[] cacheLayers, ICacheExtension[] extensions)
		{
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
		protected void ThrowIfDisposed()
		{
			if (Disposed)
			{
				throw new ObjectDisposedException("CacheStack is disposed");
			}
		}

		public async ValueTask FlushAsync()
		{
			ThrowIfDisposed();

			for (int i = 0, l = CacheLayers.Length; i < l; i++)
			{
				var layer = CacheLayers[i];
				await layer.FlushAsync();
			}

			await Extensions.OnCacheFlushAsync();
		}

		public async ValueTask CleanupAsync()
		{
			ThrowIfDisposed();
			
			for (int i = 0, l = CacheLayers.Length; i < l; i++)
			{
				var layer = CacheLayers[i];
				await layer.CleanupAsync();
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
				await layer.EvictAsync(cacheKey);
			}

			await Extensions.OnCacheEvictionAsync(cacheKey);
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
				await layer.SetAsync(cacheKey, cacheEntry);
			}

			await Extensions.OnCacheUpdateAsync(cacheKey, cacheEntry.Expiry);
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
				if (await layer.IsAvailableAsync(cacheKey))
				{
					var cacheEntry = await layer.GetAsync<T>(cacheKey);
					if (cacheEntry != default)
					{
						return cacheEntry;
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
				if (await layer.IsAvailableAsync(cacheKey))
				{
					var cacheEntry = await layer.GetAsync<T>(cacheKey);
					if (cacheEntry != default)
					{
						return (layerIndex, cacheEntry);
					}
				}
			}

			return default;
		}

		public async ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, Task<T>> getter, CacheSettings settings)
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
						if (await previousLayer.IsAvailableAsync(cacheKey))
						{
							await previousLayer.SetAsync(cacheKey, cacheEntry);
						}
					}
				}
				finally
				{
					UnlockWaitingTasks(cacheKey, cacheEntry);
				}
			}
		}

		private async ValueTask<CacheEntry<T>> RefreshValueAsync<T>(string cacheKey, Func<T, Task<T>> getter, CacheSettings settings, bool waitForRefresh)
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
					return await Extensions.WithRefreshAsync(cacheKey, async () =>
					{
						var previousEntry = await GetAsync<T>(cacheKey);

						var oldValue = default(T);
						if (previousEntry != null)
						{
							oldValue = previousEntry.Value;
						}

						var value = await getter(oldValue);
						var refreshedEntry = await SetAsync(cacheKey, value, settings.TimeToLive);

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
	}
}
