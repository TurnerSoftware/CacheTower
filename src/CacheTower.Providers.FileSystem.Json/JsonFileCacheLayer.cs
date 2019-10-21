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

		protected override T Deserialize<T>(Stream stream)
		{
			using (var streamReader = new StreamReader(stream, Encoding.UTF8, false, 1024))
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

		protected override void Serialize<T>(Stream stream, T value)
		{
			using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				var wrapper = new DataWrapper<T>
				{
					Value = value
				};

				var serializer = new JsonSerializer();
				serializer.Serialize(jsonWriter, wrapper);
				jsonWriter.Flush();
				jsonWriter.CloseOutput = false;
			}
		}
	}
}
