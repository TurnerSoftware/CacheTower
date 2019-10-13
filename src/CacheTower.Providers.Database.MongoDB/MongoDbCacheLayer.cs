using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheTower.Entities;
using CacheTower.Providers.Database.MongoDB.Commands;
using MongoFramework;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Commands;

namespace CacheTower.Providers.Database.MongoDB
{
	public class MongoDbCacheLayer : ICacheLayer
	{
		private bool? IsDatabaseAvailable { get; set; }
		private IEntityReader<DbCachedEntry> EntityReader { get; }
		private ICommandWriter<DbCachedEntry> CommandWriter { get; }

		public MongoDbCacheLayer(IMongoDbConnection connection)
		{
			EntityReader = new EntityReader<DbCachedEntry>(connection);
			CommandWriter = new CommandWriter<DbCachedEntry>(connection);
		}

		public async Task Cleanup()
		{
			var expiredEntityIds = EntityReader.AsQueryable().Where(e => e.Expiry < DateTime.UtcNow).Select(e => e.Id);
			var writeCommands = new List<IWriteCommand<DbCachedEntry>>();

			foreach (var entityId in expiredEntityIds)
			{
				writeCommands.Add(new RemoveEntityByIdCommand<DbCachedEntry>(entityId));
			}

			await CommandWriter.WriteAsync(writeCommands);
		}

		public async Task Evict(string cacheKey)
		{
			var entityId = EntityReader.AsQueryable().Where(e => e.CacheKey == cacheKey).Select(e => e.Id).FirstOrDefault();
			await CommandWriter.WriteAsync(new[] { new RemoveEntityByIdCommand<DbCachedEntry>(entityId) });
		}

		public Task<CacheEntry<T>> Get<T>(string cacheKey)
		{
			var dbEntry = EntityReader.AsQueryable().Where(e => e.CacheKey == cacheKey).FirstOrDefault();
			var cacheEntry = default(CacheEntry<T>);

			if (dbEntry != default)
			{
				cacheEntry = new CacheEntry<T>((T)dbEntry.Value, dbEntry.CachedAt, dbEntry.TimeToLive);
			}

			return Task.FromResult(cacheEntry);
		}

		public async Task Set<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			var dbEntry = EntityReader.AsQueryable().Where(e => e.CacheKey == cacheKey).FirstOrDefault();

			IWriteCommand<DbCachedEntry> command;

			if (dbEntry != default)
			{
				dbEntry.CachedAt = cacheEntry.CachedAt;
				dbEntry.TimeToLive = cacheEntry.TimeToLive;
				dbEntry.Value = cacheEntry.Value;
				command = new ReplaceEntityCommand<DbCachedEntry>(dbEntry);
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
				command = new Commands.AddEntityCommand<DbCachedEntry>(dbEntry);
			}

			await CommandWriter.WriteAsync(new[] { command });
		}

		public Task<bool> IsAvailable(string cacheKey)
		{
			if (IsDatabaseAvailable == null)
			{
				try
				{
					EntityReader.AsQueryable().Select(e => e.Id).FirstOrDefault();
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
