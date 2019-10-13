using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoFramework.Attributes;

namespace CacheTower.Providers.Database.MongoDB.Entities
{
	public class DbCachedEntry
	{
		public ObjectId Id { get; set; }

		[Index(MongoFramework.IndexSortOrder.Ascending)]
		public string CacheKey { get; set; }
		public DateTime CachedAt { get; set; }
		public TimeSpan TimeToLive { get; set; }

		[Index(MongoFramework.IndexSortOrder.Ascending)]
		public DateTime Expiry
		{
			get => CachedAt + TimeToLive;
			set { }
		}

		public object Value { get; set; }
	}
}
