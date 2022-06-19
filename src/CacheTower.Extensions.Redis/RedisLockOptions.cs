using System;

namespace CacheTower.Extensions.Redis;

/// <summary>
/// Lock options for use by the <see cref="RedisLockExtension"/>.
/// </summary>
/// <param name="LockTimeout">How long to wait on the lock before having it expire.</param>
/// <param name="RedisChannel">The Redis channel to communicate unlocking events across.</param>
/// <param name="KeyFormat">
/// A <see cref="string.Format(string, object[])"/> compatible string used to create the lock key stored in Redis.
/// The cache key is provided as argument <c>{0}</c>.
/// </param>
/// <param name="DatabaseIndex">
/// The database index used for the Redis lock.
/// If not specified, uses the default database as configured on the connection.
/// </param>
/// <param name="LockCheckStrategy">
/// The lock checking strategy to use, like pub/sub or spin lock, to detect lock release locally.
/// The waiter on the lock also performs a tight loop to check for lock release.
/// This can avoid the situation of a missed message on Redis pub/sub.
/// </param>
public record struct RedisLockOptions(
	TimeSpan LockTimeout,
	string RedisChannel,
	string KeyFormat,
	int DatabaseIndex,
	LockCheckStrategy LockCheckStrategy
)
{
	/// <summary>
	/// The default options for <see cref="RedisLockExtension"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// - <see cref="LockTimeout"/>: 1 minute<br/>
	/// - <see cref="RedisChannel"/>: <c>"CacheTower.CacheLock"</c><br/>
	/// - <see cref="KeyFormat"/>: <c>"Lock:{0}"</c><br/>
	/// - <see cref="DatabaseIndex"/>: <i>The default database configured on the connection.</i><br/>
	/// - <see cref="LockCheckStrategy"/>: Use Redis pub/sub notification to determine end of lock.
	/// </para>
	/// </remarks>
	public static readonly RedisLockOptions Default = new(
		LockTimeout: TimeSpan.FromMinutes(1),
		RedisChannel: "CacheTower.CacheLock",
		KeyFormat: "Lock:{0}",
		DatabaseIndex: -1,
		LockCheckStrategy: LockCheckStrategy.WithPubSubNotification()
	);
}

/// <summary>
/// The lock checking strategy to use for the <see cref="RedisLockExtension"/>.
/// </summary>
public readonly struct LockCheckStrategy
{
	/// <summary>
	/// Whether a "spin lock" strategy will be used.
	/// </summary>
	public readonly bool UseSpinLock { get; private init; }
	/// <summary>
	/// For spin lock strategies, the time to wait between lock checks.
	/// </summary>
	public readonly TimeSpan SpinTime { get; private init; }

	/// <summary>
	/// Use a Redis pub/sub notification lock checking strategy.
	/// </summary>
	/// <returns></returns>
	public static LockCheckStrategy WithPubSubNotification() => new();

	/// <summary>
	/// Use a "spin lock" lock checking strategy.
	/// This can avoid the situation of a missed message on Redis pub/sub.
	/// </summary>
	/// <param name="spinTime">The time to wait between lock checks.</param>
	/// <returns></returns>
	public static LockCheckStrategy WithSpinLock(TimeSpan spinTime)
	{
		return new LockCheckStrategy
		{
			UseSpinLock = true,
			SpinTime = spinTime
		};
	}

	internal int CalculateSpinAttempts(TimeSpan lockTimeout)
	{
		if (!UseSpinLock)
		{
			return 0;
		}

		return (int)Math.Ceiling(lockTimeout.TotalMilliseconds / SpinTime.TotalMilliseconds);
	}
}