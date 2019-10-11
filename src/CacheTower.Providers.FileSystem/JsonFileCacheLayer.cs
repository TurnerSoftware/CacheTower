using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;

namespace CacheTower.Providers.FileSystem
{
	public class JsonFileCacheLayer : ICacheLayer
	{
		private string Directory { get; }

		public const string ManifestFileName = "manifest.json";

		private AsyncLock ManifestLock { get; } = new AsyncLock();
		private bool? IsManifestAvailable { get; set; }

		private ConcurrentDictionary<string, string> CacheManifest { get; set; }
		private ConcurrentDictionary<string, AsyncReaderWriterLock> FileLock { get; } = new ConcurrentDictionary<string, AsyncReaderWriterLock>();

		public JsonFileCacheLayer(string directoryPath)
		{
			Directory = directoryPath;
		}

		private async Task<T> DeserializeJson<T>(Stream stream)
		{
			using (var streamReader = new StreamReader(stream))
			using (var jsonReader = new JsonTextReader(streamReader))
			{
				var jObj = await JObject.LoadAsync(jsonReader);
				return jObj.ToObject<T>();
			}
		}

		private async Task SerializeJson<T>(Stream stream, T value)
		{
			using (var streamWriter = new StreamWriter(stream))
			using (var jsonWriter = new JsonTextWriter(streamWriter))
			{
				var jObj = JObject.FromObject(value);
				await jObj.WriteToAsync(jsonWriter);
			}
		}

		private async Task TryLoadManifest()
		{
			//Avoid unnecessary lock contention way after manifest is loaded by checking before lock
			if (CacheManifest == null)
			{
				using (await ManifestLock.LockAsync())
				{
					//Check that once we have lock (due to a race condition on the outer check) that we still need to load the manifest
					if (CacheManifest == null)
					{
						var manifestPath = Path.Combine(Directory, ManifestFileName);
						if (File.Exists(manifestPath))
						{
							using (var stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Read))
							{

								CacheManifest = await DeserializeJson<ConcurrentDictionary<string, string>>(stream);
							}
						}
						else
						{
							CacheManifest = new ConcurrentDictionary<string, string>();
							using (var stream = new FileStream(manifestPath, FileMode.OpenOrCreate, FileAccess.Write))
							{
								await SerializeJson(stream, CacheManifest);
							}
						}
					}
				}
			}
		}

		private async Task SaveManifest()
		{
			using (await ManifestLock.LockAsync())
			{
				var manifestPath = Path.Combine(Directory, ManifestFileName);
				using (var stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Write))
				{
					await SerializeJson(stream, CacheManifest);
				}
			}
		}

		private string GetFileName(string cacheKey)
		{
			using (var md5 = MD5.Create())
			{
				var bytes = Encoding.UTF8.GetBytes(cacheKey);
				var hashBytes = md5.ComputeHash(bytes);
				var builder = new StringBuilder();

				for (var i = 0; i < hashBytes.Length; i++)
				{
					builder.Append(hashBytes[i].ToString("X2"));
				}

				builder.Append(".json");
				return builder.ToString();
			}
		}

		public async Task Cleanup()
		{
			await TryLoadManifest();
			throw new NotImplementedException();
		}

		public async Task Evict(string cacheKey)
		{
			await TryLoadManifest();

			if (CacheManifest.TryRemove(cacheKey, out var fileName))
			{
				var lockObj = FileLock.GetOrAdd(fileName, (name) => new AsyncReaderWriterLock());
				using (await lockObj.WriterLockAsync())
				{
					var path = Path.Combine(Directory, fileName);
					if (File.Exists(path))
					{
						File.Delete(path);
						await SaveManifest();
					}
				}
			}
		}

		public async Task<CacheEntry<T>> Get<T>(string cacheKey)
		{
			await TryLoadManifest();

			if (CacheManifest.TryGetValue(cacheKey, out var fileName))
			{
				var lockObj = FileLock.GetOrAdd(fileName, (name) => new AsyncReaderWriterLock());
				using (await lockObj.ReaderLockAsync())
				{
					//By the time we have the lock, confirm we still have a cache
					if (CacheManifest.ContainsKey(cacheKey))
					{
						var path = Path.Combine(Directory, fileName);
						using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
						{
							return await DeserializeJson<CacheEntry<T>>(stream);
						}
					}
				}
			}

			return default;
		}

		public async Task<bool> IsAvailable(string cacheKey)
		{
			if (IsManifestAvailable == null)
			{
				try
				{
					await TryLoadManifest();
					IsManifestAvailable = true;
				}
				catch
				{
					IsManifestAvailable = false;
				}
			}

			return IsManifestAvailable.Value;
		}

		public async Task Set<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			await TryLoadManifest();

			var isUpdatingExisting = CacheManifest.ContainsKey(cacheKey);

			var fileName = CacheManifest.GetOrAdd(cacheKey, (key) => GetFileName(cacheKey));
			var lockObj = FileLock.GetOrAdd(fileName, (name) => new AsyncReaderWriterLock());

			using (await lockObj.WriterLockAsync())
			{
				var path = Path.Combine(Directory, fileName);
				using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
				{
					await SerializeJson(stream, cacheEntry);
				}
			}

			//Even with the potential race condition from ContainsKey to GetOrAdd, this still limits
			//the number of potential saves performed of the manifest if we are just updating data
			//rather than adding new items to the cache
			if (!isUpdatingExisting)
			{
				await SaveManifest();
			}
		}
	}
}
