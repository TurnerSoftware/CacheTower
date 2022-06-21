using CacheTower;
using CacheTower.Providers.Database.MongoDB;
using MongoFramework;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Microsoft <see cref="IServiceCollection"/> extensions for Cache Tower.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds a <see cref="MongoDbCacheLayer"/> to the <see cref="CacheStack"/> with the specified <paramref name="connection"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="connection">The connection to the MongoDB server.</param>
	/// <returns></returns>
	public static ICacheStackBuilder AddMongoDbCacheLayer(this ICacheStackBuilder builder, IMongoDbConnection connection)
	{
		builder.CacheLayers.Add(new MongoDbCacheLayer(connection));
		return builder;
	}
}
