using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower.Entities
{
	public class DbCachedEntry
	{
		public string Id { get; set; }

		public string CacheKey { get; set; }
		public DateTime CachedAt { get; set; }
		public TimeSpan TimeToLive { get; set; }

		public DateTime Expiry
		{
			get => CachedAt + TimeToLive;
			set { }
		}

		public object Value { get; set; }
	}
}
