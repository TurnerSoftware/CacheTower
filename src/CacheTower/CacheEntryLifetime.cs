using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower
{
	public struct CacheEntryLifetime
	{
		public TimeSpan TimeToLive { get; }
		public TimeSpan StaleAfter { get; }

		public CacheEntryLifetime(TimeSpan timeToLive)
		{
			TimeToLive = timeToLive;
			StaleAfter = TimeSpan.Zero;
		}

		public CacheEntryLifetime(TimeSpan timeToLive, TimeSpan staleAfter)
		{
			TimeToLive = timeToLive;
			StaleAfter = staleAfter;
		}
	}
}
