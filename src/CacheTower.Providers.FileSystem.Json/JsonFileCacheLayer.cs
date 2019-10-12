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
	public class JsonFileCacheLayer : FileCacheLayer, ICacheLayer
	{
		public JsonFileCacheLayer(string directoryPath) : base(directoryPath, ".json") { }

		private class DataWrapper<T>
		{
			public T Value { get; set; }
		}

		protected override async Task<T> Deserialize<T>(Stream stream)
		{
			using (var streamReader = new StreamReader(stream))
			using (var jsonReader = new JsonTextReader(streamReader))
			{
				var jObj = await JObject.LoadAsync(jsonReader);
				var wrapper = jObj.ToObject<DataWrapper<T>>();

				if (wrapper == default)
				{
					return default;
				}

				return wrapper.Value;
			}
		}

		protected override async Task Serialize<T>(Stream stream, T value)
		{
			using (var streamWriter = new StreamWriter(stream))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				var wrapper = new DataWrapper<T>
				{
					Value = value
				};

				var jObj = JObject.FromObject(wrapper);
				await jObj.WriteToAsync(jsonWriter);
			}
		}
	}
}
