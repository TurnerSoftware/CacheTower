using CacheTower.Providers.Database.MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mapping;
using MongoDB.Bson.Serialization;

namespace CacheTower.Providers.Database.MongoDB.Commands
{
	public class SetCommand : IWriteCommand<DbCachedEntry>
	{
		public DbCachedEntry Entry { get; }

		public SetCommand(DbCachedEntry dbCachedEntry)
		{
			Entry = dbCachedEntry;
		}

		public IEnumerable<WriteModel<DbCachedEntry>> GetModel()
		{
			var filter = Builders<DbCachedEntry>.Filter.Eq(e => e.CacheKey, Entry.CacheKey);
			var updateDefinition = Builders<DbCachedEntry>.Update
				.Set(e => e.CacheKey, Entry.CacheKey)
				.Set(e => e.CachedAt, Entry.CachedAt)
				.Set(e => e.TimeToLive, Entry.TimeToLive)
				.Set(e => e.Expiry, Entry.Expiry)
				.Set(e => e.Value, Entry.Value);

			var model = new UpdateOneModel<DbCachedEntry>(filter, updateDefinition)
			{
				IsUpsert = true
			};

			yield return model;
		}
	}
}
