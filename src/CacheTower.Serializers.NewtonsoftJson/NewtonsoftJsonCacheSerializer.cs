using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CacheTower.Serializers.NewtonsoftJson;

/// <summary>
/// Allows serializing to and from JSON via Newtonsoft.Json
/// </summary>
public class NewtonsoftJsonCacheSerializer : ICacheSerializer
{
	private readonly JsonSerializer serializer;

	/// <summary>
	/// An existing instance of <see cref="NewtonsoftJsonCacheSerializer"/>.
	/// </summary>
	public static NewtonsoftJsonCacheSerializer Instance { get; } = new();

	private NewtonsoftJsonCacheSerializer()
	{
		serializer = new JsonSerializer();
	}

	/// <summary>
	/// Creates a new instance of <see cref="NewtonsoftJsonCacheSerializer"/> with the specified <paramref name="settings"/>.
	/// </summary>
	public NewtonsoftJsonCacheSerializer(JsonSerializerSettings settings)
	{
		serializer = JsonSerializer.Create(settings);
	}

	/// <inheritdoc />
	public void Serialize<T>(Stream stream, T? value)
	{
		try
		{
			using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);
			using var jsonWriter = new JsonTextWriter(streamWriter);
			serializer.Serialize(jsonWriter, value);
		}
		catch (Exception ex)
		{
			throw new CacheSerializationException("A serialization error has occurred when serializing with Newtonsoft.Json", ex);
		}
	}

	/// <inheritdoc />
	public T? Deserialize<T>(Stream stream)
	{
		try
		{
			using var streamReader = new StreamReader(stream, Encoding.UTF8, false, 1024);
			using var jsonReader = new JsonTextReader(streamReader);
			return serializer.Deserialize<T>(jsonReader);
		}
		catch (Exception ex)
		{
			throw new CacheSerializationException("A serialization error has occurred when deserializing with Newtonsoft.Json", ex);
		}
	}
}
