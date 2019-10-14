using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheTower.Providers.Database.MongoDB.Commands;
using CacheTower.Providers.Database.MongoDB.Entities;
using MongoFramework;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Mapping;

namespace CacheTower.Providers.Database.MongoDB
{
	public class MongoDbCacheLayer : ICacheLayer
	{
		private bool? IsDatabaseAvailable { get; set; }
		private IEntityReader<DbCachedEntry> EntityReader { get; }
		private ICommandWriter<DbCachedEntry> CommandWriter { get; }
		private IEntityIndexWriter<DbCachedEntry> IndexWriter { get; }

		private bool HasSetIndexes = false;

		public MongoDbCacheLayer(IMongoDbConnection connection)
		{
			EntityReader = new EntityReader<DbCachedEntry>(connection);
			CommandWriter = new CommandWriter<DbCachedEntry>(connection);
			IndexWriter = new EntityIndexWriter<DbCachedEntry>(connection);
		}

		private async Task TryConfigureIndexes()
		{
			if (!HasSetIndexes)
			{
				HasSetIndexes = true;
				await IndexWriter.ApplyIndexingAsync();
			}
		}

		public async Task CleanupAsync()
		{
			await TryConfigureIndexes();
			await CommandWriter.WriteAsync(new[] { new CleanupCommand() });
		}

		public async Task EvictAsync(string cacheKey)
		{
			await TryConfigureIndexes();
			await CommandWriter.WriteAsync(new[] { new EvictCommand(cacheKey) });
		}

		public async Task<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			await TryConfigureIndexes();

			var dbEntry = EntityReader.AsQueryable().Where(e => e.CacheKey == cacheKey).FirstOrDefault();
			var cacheEntry = default(CacheEntry<T>);

			if (dbEntry != default)
			{
				cacheEntry = new CacheEntry<T>((T)dbEntry.Value, dbEntry.CachedAt, dbEntry.TimeToLive);
			}

			return cacheEntry;
		}

		public async Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			await TryConfigureIndexes();
			var command = new SetCommand(new DbCachedEntry
			{
				CacheKey = cacheKey,
				CachedAt = cacheEntry.CachedAt,
				TimeToLive = cacheEntry.TimeToLive,
				Value = cacheEntry.Value
			});

			await CommandWriter.WriteAsync(new[] { command });
		}

		public async Task<bool> IsAvailableAsync(string cacheKey)
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
