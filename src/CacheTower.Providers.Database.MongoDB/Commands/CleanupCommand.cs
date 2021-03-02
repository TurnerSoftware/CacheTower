using CacheTower.Providers.Database.MongoDB.Entities;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;

namespace CacheTower.Providers.Database.MongoDB.Commands
{
	internal class CleanupCommand : IWriteCommand<DbCachedEntry>
	{
		public Type EntityType => typeof(DbCachedEntry);

		public IEnumerable<WriteModel<DbCachedEntry>> GetModel(WriteModelOptions options)
		{
			var filter = Builders<DbCachedEntry>.Filter.Lt(e => e.Expiry, DateTime.UtcNow);
			yield return new DeleteManyModel<DbCachedEntry>(filter);
		}
	}
}
