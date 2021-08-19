using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace CacheTower.Extensions.Redis
{
	/// <summary>
	/// Lock options for use by the <see cref="RedisBusyLockExtension"/>.
	/// </summary>
	public class RedisBusyLockOptions
	{
		/// <summary>
		/// The default options for <see cref="RedisBusyLockExtension"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// - <see cref="LockTimeout"/>: 1 minute<br/>
		/// - <see cref="SpinTime"/>: 100 milliseconds<br/>
		/// - <see cref="DatabaseIndex"/>: <i>The default database configured on the connection.</i>
		/// </para>
		/// </remarks>
		public static readonly RedisBusyLockOptions Default = new RedisBusyLockOptions();

		/// <summary>
		/// The database index used for the Redis lock.
		/// If not specified, uses the default database as configured on the connection.
		/// </summary>
		public int DatabaseIndex { get; }

		/// <summary>
		/// How long to wait on the lock before having it expire.
		/// </summary>
		public TimeSpan LockTimeout { get; }

		/// <summary>
		/// How open to recheck the lock when we were not the context that acquired the lock
		/// </summary>
		public TimeSpan SpinTime { get; }

		/// <summary>
		/// Number of attempts to check the lock before giving up
		/// </summary>
		public int SpinAttempts { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="RedisBusyLockOptions"/>.
		/// </summary>
		/// <param name="lockTimeout">How long to wait on the lock before having it expire. Defaults to 1 minute.</param>
		/// <param name="spinTime">How long to wait between rechecking the lock. Defaults to 100 milliseconds.</param>
		/// <param name="databaseIndex">
		/// The database index used for the Redis lock.
		/// If not specified, uses the default database as configured on the connection.
		/// </param>
		public RedisBusyLockOptions(
			TimeSpan? lockTimeout = default,
			TimeSpan? spinTime = default,
			int databaseIndex = -1
		)
		{
			LockTimeout = lockTimeout ?? TimeSpan.FromMinutes(1);
			SpinTime = spinTime ?? TimeSpan.FromMilliseconds(100);
			SpinAttempts = (int)Math.Ceiling(LockTimeout.TotalMilliseconds / SpinTime.TotalMilliseconds);
			DatabaseIndex = databaseIndex;
		}
	}
}
