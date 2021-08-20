using System;

namespace CacheTower.Extensions.Redis
{
	/// <summary>
	/// Lock options for use by the <see cref="RedisLockExtension"/>.
	/// </summary>
	public class RedisLockOptions
	{
		/// <summary>
		/// The default options for <see cref="RedisLockExtension"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// - <see cref="LockTimeout"/>: 1 minute<br/>
		/// - <see cref="RedisChannel"/>: <c>"CacheTower.CacheLock"</c><br/>
		/// - <see cref="KeyFormat"/>: <c>"Lock:{0}"</c><br/>
		/// - <see cref="DatabaseIndex"/>: <i>The default database configured on the connection.</i>
		/// - <see cref="SpinTime"/>: <i>Unused.</i>
		/// </para>
		/// </remarks>
		public static readonly RedisLockOptions Default = new();

		/// <summary>
		/// How long to wait on the lock before having it expire.
		/// </summary>
		public TimeSpan LockTimeout { get; }

		/// <summary>
		/// The Redis channel to communicate unlocking events across.
		/// </summary>
		public string RedisChannel { get; }

		/// <summary>
		/// A <see cref="string.Format(string, object[])"/> compatible string used to create the lock key stored in Redis.
		/// The cache key is provided as argument {0}.
		/// </summary>
		public string KeyFormat { get; }

		/// <summary>
		/// The database index used for the Redis lock.
		/// If not specified, uses the default database as configured on the connection.
		/// </summary>
		public int DatabaseIndex { get; }

		/// <summary>
		/// How often to recheck the lock when we were not the context that acquired the lock.
		/// </summary>
		public TimeSpan SpinTime { get; }

		internal bool UseBusyLockCheck { get; }

		internal int SpinAttempts { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="RedisLockOptions"/>.
		/// </summary>
		/// <param name="lockTimeout">How long to wait on the lock before having it expire. Defaults to 1 minute.</param>
		/// <param name="redisChannel">The Redis channel to communicate unlocking events across. Defaults to "CacheTower.CacheLock".</param>
		/// <param name="keyFormat">
		/// A <see cref="string.Format(string, object[])"/> compatible string used to create the lock key stored in Redis.
		/// The cache key is provided as argument {0}.
		/// Defaults to "Lock:{0}".
		/// </param>
		/// <param name="databaseIndex">
		/// The database index used for the Redis lock.
		/// If not specified, uses the default database as configured on the connection.
		/// </param>
		/// <param name="spinTime">
		/// The waiter on the lock also performs a tight loop to check for lock release.
		/// This can avoid the situation of a missed message on Redis pub/sub.
		/// </param>
		public RedisLockOptions(
			TimeSpan? lockTimeout = default, 
			string redisChannel = "CacheTower.CacheLock",
			string keyFormat = "Lock:{0}",
			int databaseIndex = -1,
			TimeSpan? spinTime = default
		)
		{
			LockTimeout = lockTimeout ?? TimeSpan.FromMinutes(1);
			RedisChannel = redisChannel ?? throw new ArgumentNullException(nameof(redisChannel));
			KeyFormat = keyFormat ?? throw new ArgumentNullException(nameof(keyFormat));
			DatabaseIndex = databaseIndex;

			if (spinTime.HasValue)
			{
				UseBusyLockCheck = true;
				SpinTime = spinTime.Value;
				SpinAttempts = (int)Math.Ceiling(LockTimeout.TotalMilliseconds / SpinTime.TotalMilliseconds);
			}
		}
	}
}
