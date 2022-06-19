using CacheTower;
using CacheTower.Extensions.Redis;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Microsoft <see cref="IServiceCollection"/> extensions for Cache Tower.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds the <see cref="RedisLockExtension"/> to the <see cref="CacheStack"/> with the specified <paramref name="connection"/> and <see cref="RedisLockOptions.Default"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="connection">The connection to the Redis server.</param>
	/// <returns></returns>
	public static ICacheStackBuilder WithRedisDistributedLocking(this ICacheStackBuilder builder, IConnectionMultiplexer connection)
	{
		return builder.WithRedisDistributedLocking(connection, RedisLockOptions.Default);
	}

	/// <summary>
	/// Adds the <see cref="RedisLockExtension"/> to the <see cref="CacheStack"/> with the specified <paramref name="connection"/> and <paramref name="options"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="connection">The connection to the Redis server.</param>
	/// <param name="options">Options to configure the Redis distributed locking extension.</param>
	/// <returns></returns>
	public static ICacheStackBuilder WithRedisDistributedLocking(this ICacheStackBuilder builder, IConnectionMultiplexer connection, RedisLockOptions options)
	{
		builder.Extensions.Add(new RedisLockExtension(connection, options));
		return builder;
	}
}
