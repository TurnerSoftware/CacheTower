using System.IO;
using System.Text.Json;

namespace CacheTower.Serializers.SystemTextJson;

/// <summary>
/// Allows serializing to and from JSON via System.Text.Json
/// </summary>
public class SystemTextJsonCacheSerializer : ICacheSerializer
{
	private readonly JsonSerializerOptions? options;

	private SystemTextJsonCacheSerializer() { }

	/// <summary>
	/// An existing instance of <see cref="SystemTextJsonCacheSerializer"/>.
	/// </summary>
	public static SystemTextJsonCacheSerializer Instance { get; } = new();

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
		JsonSerializer.Serialize(stream, value, options);
	}

	/// <inheritdoc/>
	public T? Deserialize<T>(Stream stream)
	{
		return JsonSerializer.Deserialize<T>(stream, options);
	}
}