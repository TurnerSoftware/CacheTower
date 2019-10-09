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

	public class CacheEntry<T> : CacheEntry, IEquatable<CacheEntry<T>>
	{
		public T Value { get; }

		public CacheEntry(T value, TimeSpan timeToLive)
		{
			Value = value;
			EndOfLife = DateTime.UtcNow + timeToLive;
		}

		public bool Equals(CacheEntry<T> other)
		{
			if (other == null)
			{
				return false;
			}

			return Equals(Value, other.Value) &&
				EndOfLife == other.EndOfLife;
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
			return (Value?.GetHashCode() ?? 1) ^ EndOfLife.GetHashCode();
		}
	}
}
