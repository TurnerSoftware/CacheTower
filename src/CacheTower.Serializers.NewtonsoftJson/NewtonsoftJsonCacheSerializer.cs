using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CacheTower.Serializers.NewtonsoftJson
{
	/// <summary>
	/// Allows serializing to and from JSON via Newtonsoft.Json
	/// </summary>
	public class NewtonsoftJsonCacheSerializer : ICacheSerializer
	{
		private JsonSerializer Serializer { get; } = new JsonSerializer();

		/// <inheritdoc />
		public void Serialize<T>(Stream stream, T value)
		{
			using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				Serializer.Serialize(jsonWriter, value);
			}
		}

		/// <inheritdoc />
		public T Deserialize<T>(Stream stream)
		{
			using (var streamReader = new StreamReader(stream, Encoding.UTF8, false, 1024))
			using (var jsonReader = new JsonTextReader(streamReader))
			{
				return Serializer.Deserialize<T>(jsonReader);
			}
		}
	}
}
