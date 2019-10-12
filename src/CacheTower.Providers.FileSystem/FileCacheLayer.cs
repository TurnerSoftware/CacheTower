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
	public abstract class FileCacheLayer : ICacheLayer
	{
		private string DirectoryPath { get; }
		private string ManifestPath { get; }
		private string FileExtension { get; }

		private AsyncLock ManifestLock { get; } = new AsyncLock();
		private bool? IsManifestAvailable { get; set; }

		private ConcurrentDictionary<string, string> CacheManifest { get; set; }
		private ConcurrentDictionary<string, AsyncReaderWriterLock> FileLock { get; } = new ConcurrentDictionary<string, AsyncReaderWriterLock>();

		protected FileCacheLayer(string directoryPath, string fileExtension)
		{
			DirectoryPath = directoryPath;
			FileExtension = fileExtension;
			ManifestPath = Path.Combine(directoryPath, "manifest" + fileExtension);
		}

		protected abstract Task<T> Deserialize<T>(Stream stream);

		protected abstract Task Serialize<T>(Stream stream, T value);

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
						if (File.Exists(ManifestPath))
						{
							using (var stream = new FileStream(ManifestPath, FileMode.Open, FileAccess.Read))
							{

								CacheManifest = await Deserialize<ConcurrentDictionary<string, string>>(stream);
							}
						}
						else
						{
							CacheManifest = new ConcurrentDictionary<string, string>();
							using (var stream = new FileStream(ManifestPath, FileMode.OpenOrCreate, FileAccess.Write))
							{
								await Serialize(stream, CacheManifest);
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
				using (var stream = new FileStream(ManifestPath, FileMode.Open, FileAccess.Write))
				{
					await Serialize(stream, CacheManifest);
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
				
				if (FileExtension != null)
				{
					builder.Append(FileExtension);
				}

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
				if (FileLock.TryRemove(fileName, out var lockObj))
				{
					using (await lockObj.WriterLockAsync())
					{
						var path = Path.Combine(DirectoryPath, fileName);
						if (File.Exists(path))
						{
							File.Delete(path);
							await SaveManifest();
						}
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
						var path = Path.Combine(DirectoryPath, fileName);
						using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
						{
							return await Deserialize<CacheEntry<T>>(stream);
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
				var path = Path.Combine(DirectoryPath, fileName);
				using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
				{
					await Serialize(stream, cacheEntry);
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
