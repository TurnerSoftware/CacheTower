using CacheTower.Providers.Database.MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mapping;

namespace CacheTower.Providers.Database.MongoDB.Commands
{
	public class CleanupCommand : IWriteCommand<DbCachedEntry>
	{
		public IEnumerable<WriteModel<DbCachedEntry>> GetModel()
		{
			var filter = Builders<DbCachedEntry>.Filter.Lt(e => e.Expiry, DateTime.UtcNow);
			yield return new DeleteManyModel<DbCachedEntry>(filter);
		}
	}
}
