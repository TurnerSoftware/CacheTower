using System;
using System.Linq;
using System.Threading.Tasks;
using CacheTower.Entities;
using MongoFramework;

namespace CacheTower.Providers.Database.MongoDB
{
	public class MongoDbCacheLayer : ICacheLayer
	{
		private bool? IsDatabaseAvailable { get; set; }
		private IMongoDbConnection Connection { get; }

		public MongoDbCacheLayer(IMongoDbConnection connection)
		{
			Connection = connection;
		}

		public async Task Cleanup()
		{
			var dbSet = new MongoDbSet<DbCachedEntry>();
			dbSet.SetConnection(Connection);
			var expiredEntities = dbSet.Where(e => e.Expiry < DateTime.UtcNow);
			foreach (var entity in expiredEntities)
			{
				dbSet.Remove(entity);
			}
			await dbSet.SaveChangesAsync();
		}

		public async Task Evict(string cacheKey)
		{
			var dbSet = new MongoDbSet<DbCachedEntry>();
			dbSet.SetConnection(Connection);
			var dbEntry = dbSet.Where(e => e.CacheKey == cacheKey).FirstOrDefault();
			dbSet.Remove(dbEntry);
			await dbSet.SaveChangesAsync();
		}

		public Task<CacheEntry<T>> Get<T>(string cacheKey)
		{
			var dbSet = new MongoDbSet<DbCachedEntry>();
			dbSet.SetConnection(Connection);
			var dbEntry = dbSet.Where(e => e.CacheKey == cacheKey).FirstOrDefault();
			var cacheEntry = default(CacheEntry<T>);

			if (dbEntry != default)
			{
				cacheEntry = new CacheEntry<T>((T)dbEntry.Value, dbEntry.CachedAt, dbEntry.TimeToLive);
			}

			return Task.FromResult(cacheEntry);
		}

		public async Task Set<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			var dbSet = new MongoDbSet<DbCachedEntry>();
			dbSet.SetConnection(Connection);
			var dbEntry = dbSet.Where(e => e.CacheKey == cacheKey).FirstOrDefault();

			if (dbEntry != default)
			{
				dbEntry.CachedAt = cacheEntry.CachedAt;
				dbEntry.TimeToLive = cacheEntry.TimeToLive;
				dbEntry.Value = cacheEntry.Value;
			}
			else
			{
				dbEntry = new DbCachedEntry
				{
					CacheKey = cacheKey,
					CachedAt = cacheEntry.CachedAt,
					TimeToLive = cacheEntry.TimeToLive,
					Value = cacheEntry.Value
				};
				dbSet.Add(dbEntry);
			}

			await dbSet.SaveChangesAsync();
		}

		public Task<bool> IsAvailable(string cacheKey)
		{
			if (IsDatabaseAvailable == null)
			{
				try
				{
					var dbSet = new MongoDbSet<DbCachedEntry>();
					dbSet.SetConnection(Connection);
					dbSet.Select(e => e.Id).FirstOrDefault();
					IsDatabaseAvailable = true;
				}
				catch
				{
					IsDatabaseAvailable = false;
				}
			}

			return Task.FromResult(IsDatabaseAvailable.Value);
		}
	}
}
