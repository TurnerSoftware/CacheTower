using System;
using System.Collections.Generic;
using CacheTower.Providers.Database.MongoDB.Entities;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;

namespace CacheTower.Providers.Database.MongoDB.Commands
{
	public class FlushCommand : IWriteCommand<DbCachedEntry>
	{
		public Type EntityType => typeof(DbCachedEntry);

		public IEnumerable<WriteModel<DbCachedEntry>> GetModel(WriteModelOptions options)
		{
			var filter = Builders<DbCachedEntry>.Filter.Empty;
			yield return new DeleteManyModel<DbCachedEntry>(filter);
		}
	}
}
