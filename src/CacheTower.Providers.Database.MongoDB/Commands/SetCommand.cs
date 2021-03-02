using CacheTower.Providers.Database.MongoDB.Entities;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;

namespace CacheTower.Providers.Database.MongoDB.Commands
{
	internal class SetCommand : IWriteCommand<DbCachedEntry>
	{
		public DbCachedEntry Entry { get; }

		public Type EntityType => typeof(DbCachedEntry);

		public SetCommand(DbCachedEntry dbCachedEntry)
		{
			Entry = dbCachedEntry;
		}

		public IEnumerable<WriteModel<DbCachedEntry>> GetModel(WriteModelOptions options)
		{
			var filter = Builders<DbCachedEntry>.Filter.Eq(e => e.CacheKey, Entry.CacheKey);
			var updateDefinition = Builders<DbCachedEntry>.Update
				.Set(e => e.CacheKey, Entry.CacheKey)
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
