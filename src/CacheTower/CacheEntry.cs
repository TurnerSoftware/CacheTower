using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower
{
	public abstract class CacheEntry
	{
		public DateTime EndOfLife { get; protected set; }

		public bool HasElapsed(TimeSpan timeSpan)
		{
			return EndOfLife < DateTime.UtcNow.Add(timeSpan);
		}
	}

	public class CacheEntry<T> : CacheEntry
	{
		public T Value { get; }

		public CacheEntry(T value, TimeSpan timeToLive)
		{
			Value = value;
			EndOfLife = DateTime.UtcNow + timeToLive;
		}
	}
}
