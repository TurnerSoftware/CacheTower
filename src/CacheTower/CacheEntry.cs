using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CacheTower
{
	public abstract class CacheEntry
	{
		public DateTime Expiry { get; }

		protected CacheEntry(DateTime expiry)
		{
			//Force the resolution of the expiry date to be to the second
			Expiry = new DateTime(
				expiry.Year, expiry.Month, expiry.Day, expiry.Hour, expiry.Minute, expiry.Second, DateTimeKind.Utc
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTime GetStaleDate(CacheSettings cacheSettings)
		{
			return Expiry - cacheSettings.TimeToLive + cacheSettings.StaleAfter;
		}
	}

	public class CacheEntry<T> : CacheEntry, IEquatable<CacheEntry<T>>
	{
		public T Value { get; }

		public CacheEntry(T value, TimeSpan timeToLive) : this(value, DateTime.UtcNow + timeToLive) { }
		public CacheEntry(T value, DateTime expiry) : base(expiry)
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
				Expiry == other.Expiry;
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
			return (Value?.GetHashCode() ?? 1) ^ Expiry.GetHashCode();
		}
	}
}
