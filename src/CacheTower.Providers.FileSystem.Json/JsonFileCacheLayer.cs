using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CacheTower.Providers.FileSystem.Json
{
	/// <remarks>
	/// The <see cref="JsonFileCacheLayer"/> uses <a href="https://github.com/JamesNK/Newtonsoft.Json/">Newtonsoft.Json</a> to serialize and deserialize the cache items to the file system.
	/// </remarks>
	/// <inheritdoc/>
	public class JsonFileCacheLayer : FileCacheLayerBase<ManifestEntry>, ICacheLayer
	{
		private static readonly JsonSerializer Serializer = new JsonSerializer();

		/// <summary>
		/// Creates a <see cref="JsonFileCacheLayer"/>, using the given <paramref name="directoryPath"/> as the location to store the cache.
		/// </summary>
		/// <param name="directoryPath"></param>
		public JsonFileCacheLayer(string directoryPath) : base(directoryPath, ".json") { }

		/// <inheritdoc/>
		protected override T Deserialize<T>(Stream stream)
		{
			using (var streamReader = new StreamReader(stream, Encoding.UTF8, false, 1024))
			using (var jsonReader = new JsonTextReader(streamReader))
			{
				//Read start object
				if (!jsonReader.Read() || jsonReader.TokenType != JsonToken.StartObject)
				{
					return default!;
				}

				//Read property name
				if (!jsonReader.Read() || jsonReader.TokenType != JsonToken.PropertyName)
				{
					return default!;
				}
				
				//Read value start
				if (!jsonReader.Read() || jsonReader.TokenType == JsonToken.Null)
				{
					return default!;
				}

				return Serializer.Deserialize<T>(jsonReader)!;
			}
		}

		/// <inheritdoc/>
		protected override void Serialize<T>(Stream stream, T value)
		{
			using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				jsonWriter.WriteStartObject();
				jsonWriter.WritePropertyName("Value");

				if (value is null)
				{
					jsonWriter.WriteNull();
				}
				else if (value.GetType().IsValueType || value is string)
				{
					jsonWriter.WriteValue(value);
				}
				else
				{
					Serializer.Serialize(jsonWriter, value);
				}

				jsonWriter.WriteEndObject();
				
				jsonWriter.CloseOutput = false;
			}
		}
	}
}
