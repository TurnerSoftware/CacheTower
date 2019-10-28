using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheTower.Providers.Memory
{
	public class MemoryCacheLayer : ISyncCacheLayer, IDisposable
	{
		private bool Disposed;
		private ReaderWriterLockSlim LockObj { get; } = new ReaderWriterLockSlim();
		private Dictionary<string, CacheEntry> Cache { get; } = new Dictionary<string, CacheEntry>(StringComparer.Ordinal);

		public void Cleanup()
		{
			LockObj.EnterUpgradeableReadLock();

			try
			{
				var keysToRemove = ArrayPool<string>.Shared.Rent(Cache.Count);
				var index = 0;
				var currentTime = DateTime.UtcNow;

				foreach (var cachePair in Cache)
				{
					var cacheEntry = cachePair.Value;
					if (cacheEntry.Expiry < currentTime)
					{
						keysToRemove[index] = cachePair.Key;
						index++;
					}
				}

				LockObj.EnterWriteLock();
				try
				{
					for (var i = index - 1; i >= 0; i--)
					{
						Cache.Remove(keysToRemove[i]);
					}
				}
				finally
				{
					LockObj.ExitWriteLock();
				}

				ArrayPool<string>.Shared.Return(keysToRemove);
			}
			finally
			{
				LockObj.ExitUpgradeableReadLock();
			}
		}

		public void Evict(string cacheKey)
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
		}

		public CacheEntry<T> Get<T>(string cacheKey)
		{
			LockObj.EnterReadLock();

			try
			{
				if (Cache.TryGetValue(cacheKey, out var cacheEntry))
				{
					return cacheEntry as CacheEntry<T>;
				}

				return default;
			}
			finally
			{
				LockObj.ExitReadLock();
			}
		}

		public bool IsAvailable(string cacheKey)
		{
			return true;
		}

		public void Set<T>(string cacheKey, CacheEntry<T> cacheEntry)
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
