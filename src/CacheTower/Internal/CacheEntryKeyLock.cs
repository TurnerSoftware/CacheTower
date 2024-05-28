using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CacheTower.Internal;

internal readonly struct CacheEntryKeyLock
{
	private readonly Dictionary<string, TaskCompletionSource<ICacheEntry>?> keyLocks = new(StringComparer.Ordinal);

	public CacheEntryKeyLock() { }

	public bool AcquireLock(string cacheKey)
	{
		lock (keyLocks)
		{
			return TryAddImpl(cacheKey, null);
		}
	}

	private bool TryAddImpl (string cacheKey, TaskCompletionSource<ICacheEntry>? value)
	{
#if NETSTANDARD2_0
			var needToAdd = !keyLocks.ContainsKey(cacheKey);
			if (needToAdd)
			{
				keyLocks[cacheKey] = value;
			}
			return needToAdd;
#elif NETSTANDARD2_1
		return keyLocks.TryAdd(cacheKey, value);
#endif
	}

	/// <summary>
	/// Returns null if acquire lock succeeded or returns task that wait until another thread will release lock and publish value.
	/// 
	/// </summary>
	public Task<ICacheEntry>? TryAcquireLockOrWaitAsync(string cacheKey)
	{
		lock (keyLocks)
		{
			if (TryAddImpl(cacheKey, null))
			{
				// Lock acquired
				return null;
			}
			var completionSource = keyLocks[cacheKey]; // Always exists here
			if (completionSource == null)
			{
				completionSource = new TaskCompletionSource<ICacheEntry>();
				keyLocks[cacheKey] = completionSource;
			}
			return completionSource.Task;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryRemove(string cacheKey, out TaskCompletionSource<ICacheEntry>? completionSource)
	{
		lock (keyLocks)
		{
#if NETSTANDARD2_0
			if (keyLocks.TryGetValue(cacheKey, out completionSource))
			{
				keyLocks.Remove(cacheKey);
				return true;
			}
			return false;
#elif NETSTANDARD2_1
			return keyLocks.Remove(cacheKey, out completionSource);
#endif
		}
	}

	public void ReleaseLock(string cacheKey, ICacheEntry cacheEntry)
	{
		if (TryRemove(cacheKey, out var completionSource))
		{
			completionSource?.TrySetResult(cacheEntry);
		}
	}

	public void ReleaseLock(string cacheKey, Exception exception)
	{
		if (TryRemove(cacheKey, out var completionSource))
		{
			completionSource?.SetException(exception);
		}
	}
}
