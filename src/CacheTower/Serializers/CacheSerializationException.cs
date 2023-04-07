using System;

namespace CacheTower.Serializers;

/// <summary>
/// An exception for any cache serialization exceptions that occur.
/// </summary>
public class CacheSerializationException : Exception
{
	/// <summary>
	/// Creates a new <see cref="CacheSerializationException"/>.
	/// </summary>
	public CacheSerializationException() : base() { }
	/// <summary>
	/// Creates a new <see cref="CacheSerializationException"/> with the specified <paramref name="message"/>.
	/// </summary>
	/// <param name="message">The error message</param>
	public CacheSerializationException(string message) : base(message) { }
	/// <summary>
	/// Creates a new <see cref="CacheSerializationException"/> with the specified <paramref name="message"/> and <paramref name="innerException"/>.
	/// </summary>
	/// <param name="message">The error message</param>
	/// <param name="innerException">The inner exception</param>
	public CacheSerializationException(string message, Exception innerException) : base(message, innerException) { }
}
