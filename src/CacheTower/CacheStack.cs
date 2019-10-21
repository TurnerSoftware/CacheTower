using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheTower.Extensions;
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

		private ConcurrentDictionary<string, IEnumerable<TaskCompletionSource<bool>>> WaitingKeyRefresh { get; } = new ConcurrentDictionary<string, IEnumerable<TaskCompletionSource<bool>>>();

		private ICacheLayer[] CacheLayers { get; }

		private ExtensionContainer Extensions { get; }

		private ICacheContext Context { get; }

		public CacheStack(ICacheContext context, ICacheLayer[] cacheLayers, ICacheExtension[] extensions)
		{
			Context = context;

			if (cacheLayers == null || cacheLayers.Length == 0)
			{
				throw new ArgumentException("There must be at least one cache layer", nameof(cacheLayers));
			}

			CacheLayers = cacheLayers;

			if (extensions == null)
			{
				throw new ArgumentNullException(nameof(extensions));
			}

			Extensions = new ExtensionContainer(extensions);
			Extensions.Register(this);
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
				if (layer is ISyncCacheLayer syncLayer)
				{
					syncLayer.Cleanup();
				}
				else if (layer is IAsyncCacheLayer asyncLayer)
				{
					await asyncLayer.CleanupAsync();
				}
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
				if (layer is ISyncCacheLayer syncLayer)
				{
					syncLayer.Evict(cacheKey);
				}
				else if (layer is IAsyncCacheLayer asyncLayer)
				{
					await asyncLayer.EvictAsync(cacheKey);
				}
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
				if (layer is ISyncCacheLayer syncLayer)
				{
					syncLayer.Set(cacheKey, cacheEntry);
				}
				else if (layer is IAsyncCacheLayer asyncLayer)
				{
					await asyncLayer.SetAsync(cacheKey, cacheEntry);
				}
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

				if (cacheLayer is ISyncCacheLayer syncLayer)
				{
					if (syncLayer.IsAvailable(cacheKey))
					{
						var cacheEntry = syncLayer.Get<T>(cacheKey);
						if (cacheEntry != default)
						{
							//Populate previous cache layers
							for (; --i >= 0;)
							{
								var previousLayer = CacheLayers[i];

								if (previousLayer is ISyncCacheLayer prevSyncLayer)
								{
									prevSyncLayer.Set(cacheKey, cacheEntry);
								}
								else if (previousLayer is IAsyncCacheLayer prevAsyncLayer)
								{
									await prevAsyncLayer.SetAsync(cacheKey, cacheEntry);
								}
							}

							return cacheEntry;
						}
					}
				}
				else if (cacheLayer is IAsyncCacheLayer asyncLayer)
				{
					if (await asyncLayer.IsAvailableAsync(cacheKey))
					{
						var cacheEntry = await asyncLayer.GetAsync<T>(cacheKey);
						if (cacheEntry != default)
						{
							//Populate previous cache layers
							for (; --i >= 0;)
							{
								var previousLayer = CacheLayers[i];

								if (previousLayer is ISyncCacheLayer prevSyncLayer)
								{
									prevSyncLayer.Set(cacheKey, cacheEntry);
								}
								else if (previousLayer is IAsyncCacheLayer prevAsyncLayer)
								{
									await prevAsyncLayer.SetAsync(cacheKey, cacheEntry);
								}
							}

							return cacheEntry;
						}
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

			CacheEntry<T> cacheEntry = default;
			var requestId = Guid.NewGuid().ToString();

			if (WaitingKeyRefresh.TryAdd(cacheKey, Array.Empty<TaskCompletionSource<bool>>()))
			{
				try
				{
					return await Extensions.RefreshValueAsync(requestId, cacheKey, async () =>
					{
						var previousEntry = await GetAsync<T>(cacheKey);

						var oldValue = default(T);
						if (previousEntry != null)
						{
							oldValue = previousEntry.Value;
						}

						var value = await getter(oldValue, Context);
						var refreshedEntry = await SetAsync(cacheKey, value, settings.TimeToLive);

						await Extensions.OnValueRefreshAsync(requestId, cacheKey, settings.TimeToLive);

						return refreshedEntry;
					}, settings);
				}
				finally
				{
					UnlockWaitingTasks(cacheKey);
				}
			}
			else if (!exitIfLocked)
			{
				var delayedResultSource = new TaskCompletionSource<bool>();
				var waitList = new[] { delayedResultSource };
				WaitingKeyRefresh.AddOrUpdate(cacheKey, waitList, (key, oldList) => oldList.Concat(waitList));

				//Last minute check to confirm whether waiting is required
				var currentEntry = await GetAsync<T>(cacheKey);
				if (currentEntry != null && !currentEntry.HasElapsed(settings.StaleAfter))
				{
					UnlockWaitingTasks(cacheKey);
					return currentEntry;
				}

				//Lock until we are notified to be unlocked
				await delayedResultSource.Task;

				//Get the updated value from the cache stack
				return await GetAsync<T>(cacheKey);
			}

			return cacheEntry;
		}
		private void UnlockWaitingTasks(string cacheKey)
		{
			if (WaitingKeyRefresh.TryRemove(cacheKey, out var waitingTasks))
			{
				foreach (var task in waitingTasks)
				{
					task.TrySetResult(true);
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
