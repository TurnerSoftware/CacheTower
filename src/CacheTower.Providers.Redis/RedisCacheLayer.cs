using System;
using System.IO;
using System.Threading.Tasks;
using CacheTower.Providers.Redis.Entities;
using ProtoBuf;
using StackExchange.Redis;

namespace CacheTower.Providers.Redis
{
	/// <remarks>
	/// The <see cref="RedisCacheLayer"/> allows caching data in Redis. Data will be serialized to Protobuf using <a href="https://github.com/protobuf-net/protobuf-net">protobuf-net</a>.
	/// <para>
	/// When caching custom types, you will need to <a href="https://github.com/protobuf-net/protobuf-net#1-first-decorate-your-classes">decorate your class</a> with <c>[ProtoContact]</c> and <c>[ProtoMember]</c> attributes per protobuf-net's documentation.<br/>
	/// Additionally, as the Protobuf format doesn't have a way to represent an empty collection, these will be returned as <c>null</c>.
	/// </para>
	/// <para>
	/// While this can be inconvienent, using Protobuf ensures high performance and low allocations for serializing.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="ICacheLayer"/>
	public class RedisCacheLayer : ICacheLayer
	{
		private IConnectionMultiplexer Connection { get; }
		private IDatabaseAsync Database { get; }
		private int DatabaseIndex { get; }

		/// <summary>
		/// Creates a new instance of <see cref="RedisCacheLayer"/> with the given <paramref name="connection"/> and <paramref name="databaseIndex"/>.
		/// </summary>
		/// <param name="connection">The primary connection to Redis where the cache will be stored.</param>
		/// <param name="databaseIndex">
		/// The database index to use for Redis.
		/// If not specified, uses the default database as configured on the <paramref name="connection"/>.
		/// </param>
		public RedisCacheLayer(IConnectionMultiplexer connection, int databaseIndex = -1)
		{
			Connection = connection;
			Database = connection.GetDatabase(databaseIndex);
			DatabaseIndex = databaseIndex;
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
			await Database.KeyDeleteAsync(cacheKey);
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
				await Connection.GetServer(endpoint).FlushDatabaseAsync(DatabaseIndex);
			}
		}

		/// <inheritdoc/>
		public async ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			var redisValue = await Database.StringGetAsync(cacheKey);
			if (redisValue != RedisValue.Null)
			{
				using (var stream = new MemoryStream(redisValue))
				{
					var redisCacheEntry = Serializer.Deserialize<RedisCacheEntry<T>>(stream);
					return new CacheEntry<T>(redisCacheEntry.Value, redisCacheEntry.Expiry);
				}
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

			var redisCacheEntry = new RedisCacheEntry<T>
			{
				Expiry = cacheEntry.Expiry,
				Value = cacheEntry.Value
			};

			using (var stream = new MemoryStream())
			{
				Serializer.Serialize(stream, redisCacheEntry);
				stream.Seek(0, SeekOrigin.Begin);

				var redisValue = RedisValue.CreateFrom(stream);
				await Database.StringSetAsync(cacheKey, redisValue, expiryOffset);
			}
		}
	}
}
