using System;
using System.Threading.Tasks;

namespace CacheTower;

/// <summary>
/// An <see cref="ICacheExtension"/> provides a method of extending the behaviour of Cache Tower.
/// </summary>
public interface ICacheExtension
{
	/// <summary>
	/// Registers the provided <paramref name="cacheStack"/> to the current cache extension.
	/// </summary>
	/// <param name="cacheStack">The cache stack you want to register.</param>
	void Register(ICacheStack cacheStack);
}

/// <remarks>
/// An <see cref="ICacheChangeExtension"/> exposes events into the inner workings of a cache stack.
/// </remarks>
/// <inheritdoc/>
public interface ICacheChangeExtension : ICacheExtension
{
	/// <summary>
	/// Triggers after a cache entry has been updated.
	/// </summary>
	/// <param name="cacheKey">The cache key for the entry that was updated.</param>
	/// <param name="expiry">The new expiry date for the cache entry.</param>
	/// <param name="updateType">The type of cache update that has occurred.</param>
	/// <returns></returns>
	ValueTask OnCacheUpdateAsync(string cacheKey, DateTime expiry, CacheUpdateType updateType);
	/// <summary>
	/// Triggers after a cache entry has been evicted.
	/// </summary>
	/// <param name="cacheKey">The cache key for the entry that was evicted.</param>
	/// <returns></returns>
	ValueTask OnCacheEvictionAsync(string cacheKey);
	/// <summary>
	/// Triggers after the cache stack is flushed.
	/// </summary>
	/// <returns></returns>
	ValueTask OnCacheFlushAsync();
}
/// <summary>
/// Describes the type of cache update that the cache stack experienced.
/// </summary>
/// <remarks>
/// When you set a new cache entry, it isn't always known what the state of the cache currently is.
/// Calling <c>SetAsync</c> doesn't check if it already exists.
/// Calling <c>GetOrSetAsync</c> however is required to do such a check so this state can be passed along.
/// </remarks>
public enum CacheUpdateType
{
	/// <summary>
	/// When the state of an existing cache entry is unknown, a cache update could 
	/// be triggered from adding a new entry or updating an existing one.
	/// </summary>
	AddOrUpdateEntry,
	/// <summary>
	/// When the state of an existing cache entry is known to not exist, a cache 
	/// update is triggered specifically with the adding of an entry.
	/// </summary>
	AddEntry
}

/// <remarks>
/// An <see cref="IDistributedLockExtension"/> allows a cache lock to be acquired, avoiding cache stampedes across multiple systems.
/// </remarks>
/// <inheritdoc/>
public interface IDistributedLockExtension : ICacheExtension
{
	/// <summary>
	/// Performs a distributed lock where, for a specific <paramref name="cacheKey"/>, the task is completed in order based on when the lock is free.
	/// The owner of the distributed lock is released first and when completed (or timed out), any waiting consumers are completed too.
	/// </summary>
	/// <param name="cacheKey">The cache key to use perform a distributed lock on.</param>
	/// <returns>A <see cref="DistributedLock"/> with details about whether the current <see cref="CacheStack"/> is the lock owner.</returns>
	ValueTask<DistributedLock> AwaitAccessAsync(string cacheKey);
}

/// <summary>
/// Represents a method that signals to release an existing lock for the specified <paramref name="cacheKey"/>.
/// </summary>
/// <param name="cacheKey">The cache key to release the lock for.</param>
/// <returns></returns>
public delegate ValueTask LockReleaseDelegate(string cacheKey);

/// <summary>
/// A distributed lock for a specific cache key. If the lock was acquired, it will be released on dispose.
/// </summary>
public readonly struct DistributedLock : IAsyncDisposable
{
	/// <summary>
	/// The cache key for the distributed lock.
	/// </summary>
	public readonly string CacheKey;

	/// <summary>
	/// Whether the lock is owned or not by the current <see cref="CacheStack"/> for the <see cref="CacheKey"/>.
	/// When it is owned, the value refresh can be performed.
	/// </summary>
	public readonly bool IsLockOwner;

	private readonly LockReleaseDelegate? LockReleaseDelegate;

	private DistributedLock(string cacheKey, bool isLockOwner, LockReleaseDelegate? lockReleaseSignal)
	{
		CacheKey = cacheKey;
		IsLockOwner = isLockOwner;
		LockReleaseDelegate = lockReleaseSignal;
	}

	/// <summary>
	/// Creates a <see cref="DistributedLock"/> in the locked state for a specific <paramref name="cacheKey"/>.
	/// The <paramref name="lockReleaseSignal"/> is used on dispose to signal lock release.
	/// </summary>
	/// <param name="cacheKey">The cache key for the distributed lock.</param>
	/// <param name="lockReleaseSignal">The delegate to trigger lock release.</param>
	/// <returns></returns>
	public static DistributedLock Locked(string cacheKey, LockReleaseDelegate lockReleaseSignal) => new(cacheKey, true, lockReleaseSignal);

	/// <summary>
	/// Creates a <see cref="DistributedLock"/> in the unlocked state for a specific <paramref name="cacheKey"/>.
	/// As there was no lock acquired, there is nothing to release on dispose.
	/// </summary>
	/// <param name="cacheKey">The cache key for the distributed lock.</param>
	/// <returns></returns>
	public static DistributedLock Unlocked(string cacheKey) => new(cacheKey, false, null);

	/// <summary>
	/// Creates a <see cref="DistributedLock"/> for when no distributed locking is enabled.
	/// It fakes itself as an acquired lock however has no lock release mechanism.
	/// </summary>
	/// <param name="cacheKey">The cache key for the distributed lock.</param>
	/// <returns></returns>
	internal static DistributedLock NotEnabled(string cacheKey) => new(cacheKey, true, null);

	/// <summary>
	/// If <see cref="IsLockOwner"/> is <see langword="true"/>, signals lock release.
	/// </summary>
	/// <returns></returns>
	public ValueTask DisposeAsync()
	{
		if (LockReleaseDelegate is not null)
		{
			return LockReleaseDelegate(CacheKey);
		}

		return default;
	}
}