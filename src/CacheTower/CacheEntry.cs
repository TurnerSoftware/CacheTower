using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CacheTower.Internal;

namespace CacheTower
{
	/// <summary>
	/// Container for the cache entry expiry date.
	/// </summary>
	public abstract class CacheEntry
	{
		/// <summary>
		/// The expiry date for the cache entry.
		/// </summary>
		public DateTime Expiry { get; }

		/// <summary>
		/// Creates a new <see cref="CacheEntry"/> with the given expiry date.
		/// </summary>
		/// <param name="expiry">The expiry date of the cache entry. This will be rounded down to the second.</param>
		protected CacheEntry(DateTime expiry)
		{
			//Force the resolution of the expiry date to be to the second
			Expiry = new DateTime(
				expiry.Year, expiry.Month, expiry.Day, expiry.Hour, expiry.Minute, expiry.Second, DateTimeKind.Utc
			);
		}

		/// <summary>
		/// Calculates the stale date for the cache entry using the provided <paramref name="cacheSettings"/>.
		/// </summary>
		/// <remarks>
		/// If <see cref="CacheSettings.StaleAfter"/> is not configured, the stale date is the expiry date.
		/// </remarks>
		/// <param name="cacheSettings">The cache settings to use for the calculation.</param>
		/// <returns>The date that the cache entry can be considered stale.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTime GetStaleDate(CacheSettings cacheSettings)
		{
			if (cacheSettings.StaleAfter.HasValue)
			{
				return Expiry - cacheSettings.TimeToLive + cacheSettings.StaleAfter!.Value;
			}
			else
			{
				return Expiry;
			}
		}
	}

	/// <summary>
	/// Container for both the cached value and its expiry date.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CacheEntry<T> : CacheEntry, IEquatable<CacheEntry<T?>?>
	{
		/// <summary>
		/// The cached value.
		/// </summary>
		public T? Value { get; }

		/// <summary>
		/// Creates a new <see cref="CacheEntry"/> with the given <paramref name="value"/> and an expiry adjusted to the <paramref name="timeToLive"/>.
		/// </summary>
		/// <param name="value">The value to cache.</param>
		/// <param name="timeToLive">The amount of time before the cache entry expires.</param>
		internal CacheEntry(T? value, TimeSpan timeToLive) : this(value, DateTimeProvider.Now + timeToLive) { }
		/// <summary>
		/// Creates a new <see cref="CacheEntry"/> with the given <paramref name="value"/> and <paramref name="expiry"/>.
		/// </summary>
		/// <param name="value">The value to cache.</param>
		/// <param name="expiry">The expiry date of the cache entry. This will be rounded down to the second.</param>
		public CacheEntry(T? value, DateTime expiry) : base(expiry)
		{
			Value = value;
		}

		/// <inheritdoc/>
		public bool Equals(CacheEntry<T?>? other)
		{
			if (other == null)
			{
				return false;
			}

			return Equals(Value, other.Value) &&
				Expiry == other.Expiry;
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (obj is CacheEntry<T?> objOfType)
			{
				return Equals(objOfType);
			}

			return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return (Value?.GetHashCode() ?? 1) ^ Expiry.GetHashCode();
		}
	}
}
