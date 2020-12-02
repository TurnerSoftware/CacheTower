using System;
using System.IO;
using System.Threading.Tasks;
using CacheTower.Providers.Redis.Entities;
using ProtoBuf;
using StackExchange.Redis;

namespace CacheTower.Providers.Redis
{
	public class RedisCacheLayer : ICacheLayer
	{
		private IConnectionMultiplexer Connection { get; }
		private IDatabaseAsync Database { get; }
		private int DatabaseIndex { get; }

		public RedisCacheLayer(IConnectionMultiplexer connection, int databaseIndex = -1)
		{
			Connection = connection;
			Database = connection.GetDatabase(databaseIndex);
			DatabaseIndex = databaseIndex;
		}

		public ValueTask CleanupAsync()
		{
			//Noop as Redis handles this directly
			return new ValueTask();
		}

		public async ValueTask EvictAsync(string cacheKey)
		{
			await Database.KeyDeleteAsync(cacheKey);
		}

		public async ValueTask FlushAsync()
		{
			var redisEndpoints = Connection.GetEndPoints();
			foreach (var endpoint in redisEndpoints)
			{
				await Connection.GetServer(endpoint).FlushDatabaseAsync(DatabaseIndex);
			}
		}

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

		public ValueTask<bool> IsAvailableAsync(string cacheKey)
		{
			return new ValueTask<bool>(Connection.IsConnected);
		}

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
