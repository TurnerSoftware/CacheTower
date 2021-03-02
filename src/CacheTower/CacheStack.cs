using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CacheTower.Extensions;

namespace CacheTower
{
	/// <summary>
	/// A <see cref="CacheStack"/> is the backbone of caching for Cache Tower. This manages coordination between the various cache layers, manages the cache extensions and handles background refreshing.
	/// </summary>
	public class CacheStack : ICacheStack, IFlushableCacheStack, IAsyncDisposable
	{
		private bool Disposed;

		private Dictionary<string, IEnumerable<TaskCompletionSource<object>>> WaitingKeyRefresh { get; }

		private ICacheLayer[] CacheLayers { get; }

		private ExtensionContainer Extensions { get; }

		/// <summary>
		/// Creates a new <see cref="CacheStack"/> with the given <paramref name="cacheLayers"/> and <paramref name="extensions"/>.
		/// </summary>
		/// <param name="cacheLayers">The cache layers to use for the current cache stack. The layers should be ordered from the highest priority to the lowest. At least one cache layer is required.</param>
		/// <param name="extensions">The cache extensions to use for the current cache stack.</param>
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

		/// <summary>
		/// Helper for throwing if disposed.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void ThrowIfDisposed()
		{
			if (Disposed)
			{
				throw new ObjectDisposedException("CacheStack is disposed");
			}
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public async ValueTask CleanupAsync()
		{
			ThrowIfDisposed();
			
			for (int i = 0, l = CacheLayers.Length; i < l; i++)
			{
				var layer = CacheLayers[i];
				await layer.CleanupAsync();
			}
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public async ValueTask<CacheEntry<T>> SetAsync<T>(string cacheKey, T value, TimeSpan timeToLive)
		{
			ThrowIfDisposed();

			var expiry = DateTime.UtcNow + timeToLive;
			var cacheEntry = new CacheEntry<T>(value, expiry);
			await SetAsync(cacheKey, cacheEntry);
			return cacheEntry;
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

			var currentTime = DateTime.UtcNow;
			var cacheEntryPoint = await GetWithLayerIndexAsync<T>(cacheKey);
			if (cacheEntryPoint != default && cacheEntryPoint.CacheEntry.Expiry > currentTime)
			{
				var cacheEntry = cacheEntryPoint.CacheEntry;
				if (cacheEntry.GetStaleDate(settings) < currentTime)
				{
					//Refresh the value in the background
					_ = RefreshValueAsync(cacheKey, getter, settings, noExistingValueAvailable: false);
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
				return (await RefreshValueAsync(cacheKey, getter, settings, noExistingValueAvailable: true)).Value;
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

		private async ValueTask<CacheEntry<T>> RefreshValueAsync<T>(string cacheKey, Func<T, Task<T>> getter, CacheSettings settings, bool noExistingValueAvailable)
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
					var previousEntry = await GetAsync<T>(cacheKey);
					if (previousEntry != default && noExistingValueAvailable && previousEntry.Expiry < DateTime.UtcNow)
					{
						//The Cache Stack will always return an unexpired value if one exists.
						//If we are told to refresh because one doesn't and we find one, we return the existing value, ignoring the refresh.
						//This can happen due to the race condition of getting the values out of the cache.
						//We can only do any of this because we have the local lock.
						UnlockWaitingTasks(cacheKey, previousEntry);
						return previousEntry;
					}

					return await Extensions.WithRefreshAsync(cacheKey, async () =>
					{
						var oldValue = default(T);
						if (previousEntry != default)
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
			else if (noExistingValueAvailable)
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

		/// <summary>
		/// Disposes the current instance of <see cref="CacheStack"/> and all associated layers and extensions.
		/// </summary>
		/// <returns></returns>
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
