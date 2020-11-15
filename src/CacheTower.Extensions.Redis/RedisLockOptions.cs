using System;

namespace CacheTower.Extensions.Redis
{
	public class RedisLockOptions
	{
		public static readonly RedisLockOptions Default = new RedisLockOptions();

		public int DatabaseIndex { get; }
		public string RedisChannel { get; }
		public TimeSpan LockTimeout { get; }
		public string KeyFormat { get; }

		public RedisLockOptions(
			TimeSpan? lockTimeout = default, 
			string redisChannel = "CacheTower.CacheLock",
			string keyFormat = "Lock:{0}",
			int databaseIndex = -1
		)
		{
			LockTimeout = lockTimeout.HasValue ? lockTimeout.Value : TimeSpan.FromMinutes(1);
			RedisChannel = redisChannel ?? throw new ArgumentNullException(nameof(redisChannel));
			KeyFormat = keyFormat ?? throw new ArgumentNullException(nameof(keyFormat));
			DatabaseIndex = databaseIndex;
		}
	}
}
