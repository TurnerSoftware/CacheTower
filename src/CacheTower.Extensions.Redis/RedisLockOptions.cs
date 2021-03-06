﻿using System;

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
		/// </para>
		/// </remarks>
		public static readonly RedisLockOptions Default = new RedisLockOptions();

		/// <summary>
		/// The database index used for the Redis lock.
		/// If not specified, uses the default database as configured on the connection.
		/// </summary>
		public int DatabaseIndex { get; }
		/// <summary>
		/// The Redis channel to communicate unlocking events across.
		/// </summary>
		public string RedisChannel { get; }
		/// <summary>
		/// How long to wait on the lock before having it expire.
		/// </summary>
		public TimeSpan LockTimeout { get; }
		/// <summary>
		/// A <see cref="string.Format(string, object[])"/> compatible string used to create the lock key stored in Redis.
		/// The cache key is provided as argument {0}.
		/// </summary>
		public string KeyFormat { get; }

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
		public RedisLockOptions(
			TimeSpan? lockTimeout = default, 
			string redisChannel = "CacheTower.CacheLock",
			string keyFormat = "Lock:{0}",
			int databaseIndex = -1
		)
		{
			LockTimeout = lockTimeout ?? TimeSpan.FromMinutes(1);
			RedisChannel = redisChannel ?? throw new ArgumentNullException(nameof(redisChannel));
			KeyFormat = keyFormat ?? throw new ArgumentNullException(nameof(keyFormat));
			DatabaseIndex = databaseIndex;
		}
	}
}
