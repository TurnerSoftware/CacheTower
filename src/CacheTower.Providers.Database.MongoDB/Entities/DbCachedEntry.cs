using System;
using System.Collections.Generic;
using System.Text;
using MongoFramework.Attributes;

namespace CacheTower.Entities
{
	public class DbCachedEntry
	{
		public string Id { get; set; }

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
