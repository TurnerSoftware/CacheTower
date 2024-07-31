using System;
using System.IO;
using System.Threading.Tasks;
using CacheTower.Serializers.Protobuf;
using StackExchange.Redis;

namespace CacheTower.Providers.Redis
{
	/// <inheritdoc cref="ICacheLayer"/>
	public class RedisCacheLayer : IDistributedCacheLayer
	{
		private IConnectionMultiplexer Connection { get; }
		private IDatabaseAsync Database { get; }
		private readonly RedisCacheLayerOptions Options;

		/// <summary>
		/// Creates a new instance of <see cref="RedisCacheLayer"/> with the given <paramref name="connection"/> and <paramref name="databaseIndex"/>.
		/// If using this constructor, Protobuf encoding will be used.
		/// </summary>
		/// <param name="connection">The primary connection to Redis where the cache will be stored.</param>
		/// <param name="databaseIndex">
		/// The database index to use for Redis.
		/// If not specified, uses the default database as configured on the <paramref name="connection"/>.
		/// </param>
		[Obsolete("Use other constructor. Specifying cache serializers will become the default behaviour going forward.")]
		public RedisCacheLayer(IConnectionMultiplexer connection, int databaseIndex = -1) : this(connection, new RedisCacheLayerOptions(ProtobufCacheSerializer.Instance, databaseIndex))
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="RedisCacheLayer"/> with the given <paramref name="connection"/> and <paramref name="options"/>.
		/// </summary>
		/// <param name="connection">The primary connection to Redis where the cache will be stored.</param>
		/// <param name="options">Various options that control the behaviour of the <see cref="RedisCacheLayer"/>.</param>
		public RedisCacheLayer(IConnectionMultiplexer connection, RedisCacheLayerOptions options)
		{
			Connection = connection;
			Database = connection.GetDatabase(options.DatabaseIndex);
			Options = options;
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Cleanup is unnecessary for the <see cref="RedisCacheLayer"/> as Redis handles removing expired keys automatically.
		/// </remarks>
		public ValueTask CleanupAsync()
		{
			//Noop as Redis handles this directly
			return new ValueTask();
		}

		/// <inheritdoc/>
		public async ValueTask EvictAsync(string cacheKey)
		{
			await Database.KeyDeleteAsync(cacheKey).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Flushing the <see cref="RedisCacheLayer"/> performs a database flush in Redis.
		/// Every key associated to the database index will be removed.
		/// </remarks>
		public async ValueTask FlushAsync()
		{
			var redisEndpoints = Connection.GetEndPoints();
			foreach (var endpoint in redisEndpoints)
			{
				await Connection.GetServer(endpoint).FlushDatabaseAsync(Options.DatabaseIndex).ConfigureAwait(false);
			}
		}

		/// <inheritdoc/>
		public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
		{
			var redisValue = await Database.StringGetAsync(cacheKey).ConfigureAwait(false);
			if (redisValue != RedisValue.Null)
			{
				using var stream = new MemoryStream(redisValue);
				return Options.Serializer.Deserialize<CacheEntry<T>>(stream);
			}

			return default;
		}

		/// <inheritdoc/>
		public ValueTask<bool> IsAvailableAsync(string cacheKey)
		{
			return new ValueTask<bool>(Connection.IsConnected);
		}

		/// <inheritdoc/>
		public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			var expiryOffset = cacheEntry.Expiry - DateTime.UtcNow;
			if (expiryOffset < TimeSpan.Zero)
			{
				return;
			}

			using var stream = new MemoryStream();
			Options.Serializer.Serialize(stream, cacheEntry);
			stream.Seek(0, SeekOrigin.Begin);
			var redisValue = RedisValue.CreateFrom(stream);
			await Database.StringSetAsync(cacheKey, redisValue, expiryOffset).ConfigureAwait(false);
		}
	}
}
