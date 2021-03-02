using CacheTower.Providers.Database.MongoDB.Entities;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;

namespace CacheTower.Providers.Database.MongoDB.Commands
{
	internal class EvictCommand : IWriteCommand<DbCachedEntry>
	{
		private string CacheKey { get; }

		public Type EntityType => typeof(DbCachedEntry);

		public EvictCommand(string cacheKey)
		{
			CacheKey = cacheKey;
		}

		public IEnumerable<WriteModel<DbCachedEntry>> GetModel(WriteModelOptions options)
		{
			var filter = Builders<DbCachedEntry>.Filter.Eq(e => e.CacheKey, CacheKey);
			yield return new DeleteManyModel<DbCachedEntry>(filter);
		}
	}
}
