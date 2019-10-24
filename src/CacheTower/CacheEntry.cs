using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CacheTower
{
	public abstract class CacheEntry
	{
		public DateTime CachedAt { get; }

		public TimeSpan TimeToLive { get; }

		protected CacheEntry(DateTime cachedAt, TimeSpan timeToLive)
		{
			if (timeToLive < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(timeToLive), "TimeSpan must be greater than or equal to zero");
			}

			CachedAt = new DateTime(
				cachedAt.Year, cachedAt.Month, cachedAt.Day, cachedAt.Hour, cachedAt.Minute, cachedAt.Second, DateTimeKind.Utc
			);
			TimeToLive = timeToLive;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElapsed(TimeSpan timeSpan)
		{
			return CachedAt.Add(timeSpan) < DateTime.UtcNow;
		}
	}

	public class CacheEntry<T> : CacheEntry, IEquatable<CacheEntry<T>>
	{
		public T Value { get; }

		public CacheEntry(T value, DateTime cachedAt, TimeSpan timeToLive) : base(cachedAt, timeToLive)
		{
			Value = value;
		}

		public bool Equals(CacheEntry<T> other)
		{
			if (other == null)
			{
				return false;
			}

			return Equals(Value, other.Value) &&
				CachedAt == other.CachedAt &&
				TimeToLive == other.TimeToLive;
		}

		public override bool Equals(object obj)
		{
			if (obj is CacheEntry<T> objOfType)
			{
				return Equals(objOfType);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return (Value?.GetHashCode() ?? 1) ^ CachedAt.GetHashCode() ^ TimeToLive.GetHashCode();
		}
	}
}
