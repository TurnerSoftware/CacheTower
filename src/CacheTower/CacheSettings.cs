using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower
{
	public struct CacheSettings
	{
		public TimeSpan TimeToLive { get; set; }
		public TimeSpan TimeAllowedStale { get; set; }
	}
}
