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
		private ConnectionMultiplexer Connection { get; }
		private IDatabaseAsync Database { get; }
		private bool? IsCacheAvailable { get; set; }

		public RedisCacheLayer(ConnectionMultiplexer connection, int databaseIndex = -1)
		{
			Connection = connection;
			Database = connection.GetDatabase(databaseIndex);
		}

		public Task Cleanup()
		{
			throw new NotImplementedException();
		}

		public async Task Evict(string cacheKey)
		{
			await Database.KeyDeleteAsync(cacheKey);
		}

		public async Task<CacheEntry<T>> Get<T>(string cacheKey)
		{
			var redisValue = await Database.StringGetAsync(cacheKey);
			if (redisValue != RedisValue.Null)
			{
				using (var stream = new ReadOnlyMemoryStream(redisValue))
				{
					var redisCacheEntry = Serializer.Deserialize<RedisCacheEntry>(stream);
					return new CacheEntry<T>((T)redisCacheEntry.Value, redisCacheEntry.CachedAt, redisCacheEntry.TimeToLive);
				}
			}

			return default;
		}

		public async Task<bool> IsAvailable(string cacheKey)
		{
			if (IsCacheAvailable == null)
			{
				try
				{
					await Database.PingAsync();
					IsCacheAvailable = true;
				}
				catch
				{
					IsCacheAvailable = false;
				}
			}

			return IsCacheAvailable.Value;
		}

		public async Task Set<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			var redisCacheEntry = new RedisCacheEntry
			{
				CachedAt = cacheEntry.CachedAt,
				TimeToLive = cacheEntry.TimeToLive,
				Value = cacheEntry.Value
			};

			using (var stream = new MemoryStream())
			{
				Serializer.Serialize(stream, redisCacheEntry);
				stream.Seek(0, SeekOrigin.Begin);

				var redisValue = RedisValue.CreateFrom(stream);
				await Database.StringSetAsync(cacheKey, redisValue, cacheEntry.TimeToLive);
			}
		}
	}
}
