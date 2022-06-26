using System;
using System.Runtime.CompilerServices;
using CacheTower.Internal;

namespace CacheTower;

/// <summary>
/// Container for the cache entry expiry date.
/// </summary>
public interface ICacheEntry
{
	/// <summary>
	/// The expiry date for the cache entry.
	/// </summary>
	DateTime Expiry { get; }
}

/// <summary>
/// Container for the cached value and its expiry date.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICacheEntry<T> : ICacheEntry
{
	/// <summary>
	/// The cached value.
	/// </summary>
	T? Value { get; }
}

/// <summary>
/// Extension methods for <see cref="ICacheEntry"/>.
/// </summary>
public static class CacheEntryExtensions
{
	/// <summary>
	/// Calculates the stale date for an <see cref="ICacheEntry"/> based on the <paramref name="cacheEntry"/>'s expiry and <paramref name="cacheSettings"/>.
	/// </summary>
	/// <remarks>
	/// When <see cref="CacheSettings.StaleAfter"/> is <see langword="null"/>, this will return the <paramref name="cacheEntry"/>'s expiry.
	/// </remarks>
	/// <param name="cacheEntry">The cache entry to get the stale date for.</param>
	/// <param name="cacheSettings">The cache settings to use as part of the stale date calculation.</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DateTime GetStaleDate(this ICacheEntry cacheEntry, CacheSettings cacheSettings)
	{
		if (cacheSettings.StaleAfter.HasValue)
		{
			return cacheEntry.Expiry - cacheSettings.TimeToLive + cacheSettings.StaleAfter!.Value;
		}
		else
		{
			return cacheEntry.Expiry;
		}
	}
}

/// <summary>
/// Container for both the cached value and its expiry date.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Value">The value to cache.</param>
/// <param name="Expiry">The expiry date of the cache entry. This will be rounded down to the second.</param>
public sealed record CacheEntry<T>(T? Value, DateTime Expiry) : ICacheEntry<T>
{
	/// <summary>
	/// The cached value.
	/// </summary>
	public T? Value { get; } = Value;

	/// <summary>
	/// The expiry date for the cache entry.
	/// </summary>
	public DateTime Expiry { get; } = new DateTime(
		Expiry.Year, Expiry.Month, Expiry.Day, Expiry.Hour, Expiry.Minute, Expiry.Second, DateTimeKind.Utc
	);

	/// <summary>
	/// Creates a new <see cref="ICacheEntry"/> with the given <paramref name="value"/> and an expiry adjusted to the <paramref name="timeToLive"/>.
	/// </summary>
	/// <param name="value">The value to cache.</param>
	/// <param name="timeToLive">The amount of time before the cache entry expires.</param>
	internal CacheEntry(T? value, TimeSpan timeToLive) : this(value, DateTimeProvider.Now + timeToLive) { }
}
