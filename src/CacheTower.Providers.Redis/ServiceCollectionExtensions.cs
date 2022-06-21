using CacheTower;
using CacheTower.Providers.Redis;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Microsoft <see cref="IServiceCollection"/> extensions for Cache Tower.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds a <see cref="RedisCacheLayer"/> to the <see cref="CacheStack"/> with the specified <paramref name="connection"/> and <paramref name="options"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="connection">The connection to the MongoDB server.</param>
	/// <param name="options">The <see cref="RedisCacheLayer"/> options for configuring serializer and database index.</param>
	/// <returns></returns>
	public static ICacheStackBuilder AddRedisCacheLayer(this ICacheStackBuilder builder, IConnectionMultiplexer connection, RedisCacheLayerOptions options)
	{
		builder.CacheLayers.Add(new RedisCacheLayer(connection, options));
		return builder;
	}
}
