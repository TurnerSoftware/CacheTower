using System;
using System.IO;
using System.Threading.Tasks;
using CacheTower.Providers.Redis.Entities;
using ProtoBuf;
using StackExchange.Redis;

namespace CacheTower.Providers.Redis
{
	public class RedisCacheLayer : IAsyncCacheLayer
	{
		private IConnectionMultiplexer Connection { get; }
		private IDatabaseAsync Database { get; }
		private bool? IsCacheAvailable { get; set; }

		public RedisCacheLayer(IConnectionMultiplexer connection, int databaseIndex = -1)
		{
			Connection = connection;
			Database = connection.GetDatabase(databaseIndex);
		}

		public Task CleanupAsync()
		{
			//Noop as Redis handles this directly
			return Task.CompletedTask;
		}

		public Task EvictAsync(string cacheKey)
		{
			return Database.KeyDeleteAsync(cacheKey);
		}

		public async Task<CacheEntry<T>> GetAsync<T>(string cacheKey)
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

		public Task<bool> IsAvailableAsync(string cacheKey)
		{
			return Task.FromResult(Connection.IsConnected);
		}

		public async Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
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
