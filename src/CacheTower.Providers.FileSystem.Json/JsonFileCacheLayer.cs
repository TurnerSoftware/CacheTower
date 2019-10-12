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

		protected override async Task<T> Deserialize<T>(Stream stream)
		{
			using (var streamReader = new StreamReader(stream))
			using (var jsonReader = new JsonTextReader(streamReader))
			{
				var jObj = await JObject.LoadAsync(jsonReader);
				return jObj.ToObject<T>();
			}
		}

		protected override async Task Serialize<T>(Stream stream, T value)
		{
			using (var streamWriter = new StreamWriter(stream))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				var jObj = JObject.FromObject(value);
				await jObj.WriteToAsync(jsonWriter);
			}
		}
	}
}
