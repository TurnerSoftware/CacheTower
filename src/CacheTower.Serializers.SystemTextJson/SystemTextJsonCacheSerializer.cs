using System;
using System.IO;
using System.Text.Json;

namespace CacheTower.Serializers.SystemTextJson;

/// <summary>
/// Allows serializing to and from JSON via System.Text.Json
/// </summary>
public class SystemTextJsonCacheSerializer : ICacheSerializer
{
	private readonly JsonSerializerOptions? options;

	/// <summary>
	/// An existing instance of <see cref="SystemTextJsonCacheSerializer"/>.
	/// </summary>
	public static SystemTextJsonCacheSerializer Instance { get; } = new();

	private SystemTextJsonCacheSerializer() { }

	/// <summary>
	/// Creates a new instance of <see cref="SystemTextJsonCacheSerializer"/> with the specified <paramref name="options"/>.
	/// </summary>
	public SystemTextJsonCacheSerializer(JsonSerializerOptions options)
	{
		this.options = options;
	}

	/// <inheritdoc/>
	public void Serialize<T>(Stream stream, T? value)
	{
		try
		{
			JsonSerializer.Serialize(stream, value, options);
		}
		catch (Exception ex)
		{
			throw new CacheSerializationException("A serialization error has occurred when serializing with System.Text.Json", ex);
		}
	}

	/// <inheritdoc/>
	public T? Deserialize<T>(Stream stream)
	{
		try
		{
			return JsonSerializer.Deserialize<T>(stream, options);
		}
		catch (Exception ex)
		{
			throw new CacheSerializationException("A serialization error has occurred when deserializing with System.Text.Json", ex);
		}
	}
}