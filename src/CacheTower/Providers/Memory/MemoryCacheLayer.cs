using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheTower.Providers.Memory
{
	public class MemoryCacheLayer : ICacheLayer, IDisposable
	{
		private bool Disposed;
		private ReaderWriterLockSlim LockObj { get; } = new ReaderWriterLockSlim();
		private Dictionary<string, CacheEntry> Cache { get; } = new Dictionary<string, CacheEntry>();

		private static readonly Task<bool> CompletedTaskTrue = Task.FromResult(true);

		public Task CleanupAsync()
		{
			LockObj.EnterUpgradeableReadLock();

			try
			{
				var keysToRemove = new List<string>();

				foreach (var cachePair in Cache)
				{
					var cacheEntry = cachePair.Value;
					if (cacheEntry.HasElapsed(cacheEntry.TimeToLive))
					{
						keysToRemove.Add(cachePair.Key);
					}
				}

				LockObj.EnterWriteLock();
				try
				{
					foreach (var key in keysToRemove)
					{
						Cache.Remove(key);
					}
				}
				finally
				{
					LockObj.ExitWriteLock();
				}
			}
			finally
			{
				LockObj.ExitUpgradeableReadLock();
			}

			return Task.CompletedTask;
		}

		public Task EvictAsync(string cacheKey)
		{
			LockObj.EnterWriteLock();
			try
			{
				Cache.Remove(cacheKey);
			}
			finally
			{
				LockObj.ExitWriteLock();
			}

			return Task.CompletedTask;
		}

		public Task<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			LockObj.EnterReadLock();

			try
			{
				if (Cache.TryGetValue(cacheKey, out var cacheEntry))
				{
					return Task.FromResult(cacheEntry as CacheEntry<T>);
				}

				return Task.FromResult(default(CacheEntry<T>));
			}
			finally
			{
				LockObj.ExitReadLock();
			}
		}

		public Task<bool> IsAvailableAsync(string cacheKey)
		{
			return CompletedTaskTrue;
		}

		public Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			LockObj.EnterWriteLock();

			try
			{
				Cache[cacheKey] = cacheEntry;
			}
			finally
			{
				LockObj.ExitWriteLock();
			}

			return Task.CompletedTask;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (Disposed)
			{
				return;
			}

			if (disposing)
			{
				LockObj.Dispose();
			}

			Disposed = true;
		}
	}
}
