using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower
{
	public struct CacheSettings
	{
		public TimeSpan TimeToLive { get; }
		public TimeSpan StaleAfter { get; }

		public CacheSettings(TimeSpan timeToLive)
		{
			TimeToLive = timeToLive;
			StaleAfter = timeToLive;
		}

		public CacheSettings(TimeSpan timeToLive, TimeSpan staleAfter)
		{
			TimeToLive = timeToLive;
			StaleAfter = staleAfter;
		}
	}
}
