using System.Linq;
using System.Threading.Tasks;
using CacheTower.Providers.Database.MongoDB.Commands;
using CacheTower.Providers.Database.MongoDB.Entities;
using MongoFramework;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Linq;

namespace CacheTower.Providers.Database.MongoDB
{
	public class MongoDbCacheLayer : ICacheLayer
	{
		private bool? IsDatabaseAvailable { get; set; }

		private IMongoDbConnection Connection { get; }

		private bool HasSetIndexes = false;

		public MongoDbCacheLayer(IMongoDbConnection connection)
		{
			Connection = connection;
		}

		private async ValueTask TryConfigureIndexes()
		{
			if (!HasSetIndexes)
			{
				HasSetIndexes = true;
				await EntityIndexWriter.ApplyIndexingAsync<DbCachedEntry>(Connection);
			}
		}

		public async ValueTask CleanupAsync()
		{
			await TryConfigureIndexes();
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new CleanupCommand() }, default);
		}

		public async ValueTask EvictAsync(string cacheKey)
		{
			await TryConfigureIndexes();
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new EvictCommand(cacheKey) }, default);
		}

		public async ValueTask FlushAsync()
		{
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new FlushCommand() }, default);
		}

		public async ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			await TryConfigureIndexes();

			var provider = new MongoFrameworkQueryProvider<DbCachedEntry>(Connection);
			var queryable = new MongoFrameworkQueryable<DbCachedEntry>(provider);

			var dbEntry = queryable.Where(e => e.CacheKey == cacheKey).FirstOrDefault();
			var cacheEntry = default(CacheEntry<T>);

			if (dbEntry != default)
			{
				cacheEntry = new CacheEntry<T>((T)dbEntry.Value, dbEntry.Expiry);
			}

			return cacheEntry;
		}

		public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			await TryConfigureIndexes();
			var command = new SetCommand(new DbCachedEntry
			{
				CacheKey = cacheKey,
				Expiry = cacheEntry.Expiry,
				Value = cacheEntry.Value
			});

			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { command }, default);
		}

		public async ValueTask<bool> IsAvailableAsync(string cacheKey)
		{
			if (IsDatabaseAvailable == null)
			{
				try
				{
					await TryConfigureIndexes();
					IsDatabaseAvailable = true;
				}
				catch
				{
					IsDatabaseAvailable = false;
				}
			}

			return IsDatabaseAvailable.Value;
		}
	}
}
