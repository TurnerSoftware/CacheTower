namespace CacheTower.Providers.Redis;

/// <summary>
/// Options for controlling a <see cref="RedisCacheLayer"/>.
/// </summary>
/// <param name="Serializer">The serializer to use for the data.</param>
/// <param name="DatabaseIndex">
/// The database index used for the cached data.
/// If none is specified, uses the default database as configured on the connection.
/// </param>
public readonly record struct RedisCacheLayerOptions(
	ICacheSerializer Serializer,
	int DatabaseIndex = -1
);
