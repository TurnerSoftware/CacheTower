using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;

namespace CacheTower.Providers.FileSystem.Json
{
	public class JsonFileCacheLayer : FileCacheLayerBase<ManifestEntry>, ICacheLayer
	{
		public JsonFileCacheLayer(string directoryPath) : base(directoryPath, ".json") { }

		private class DataWrapper<T>
		{
			public T Value { get; set; }
		}

		protected override async Task<T> DeserializeAsync<T>(Stream stream)
		{
			using (var memStream = new MemoryStream((int)stream.Length))
			{
				await stream.CopyToAsync(memStream);
				memStream.Seek(0, SeekOrigin.Begin);

				using (var streamReader = new StreamReader(memStream))
				using (var jsonReader = new JsonTextReader(streamReader))
				{
					var serializer = new JsonSerializer();
					var wrapper = serializer.Deserialize<DataWrapper<T>>(jsonReader);

					if (wrapper == default)
					{
						return default;
					}

					return wrapper.Value;
				}
			}
		}

		protected override async Task SerializeAsync<T>(Stream stream, T value)
		{
			using (var memStream = new MemoryStream())
			using (var streamWriter = new StreamWriter(memStream))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				var wrapper = new DataWrapper<T>
				{
					Value = value
				};

				var serializer = new JsonSerializer();
				serializer.Serialize(jsonWriter, wrapper);
				await jsonWriter.FlushAsync();

				memStream.Seek(0, SeekOrigin.Begin);
				await memStream.CopyToAsync(stream);
			}
		}
	}
}
