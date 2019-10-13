using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace CacheTower.Providers.FileSystem
{
#if NETSTANDARD2_0
	public abstract class FileCacheLayerBase<TManifest> : ICacheLayer, IDisposable where TManifest : IManifestEntry, new()
#elif NETSTANDARD2_1
	public abstract class FileCacheLayerBase<TManifest> : ICacheLayer, IAsyncDisposable where TManifest : IManifestEntry, new()
#endif
	{
		private bool Disposed = false;
		private string DirectoryPath { get; }
		private string ManifestPath { get; }
		private string FileExtension { get; }

		private AsyncLock ManifestLock { get; } = new AsyncLock();
		private bool? IsManifestAvailable { get; set; }

		private HashAlgorithm FileNameHashAlgorithm { get; } = MD5.Create();

		private ConcurrentDictionary<string, IManifestEntry> CacheManifest { get; set; }
		private ConcurrentDictionary<string, AsyncReaderWriterLock> FileLock { get; } = new ConcurrentDictionary<string, AsyncReaderWriterLock>();

		protected FileCacheLayerBase(string directoryPath, string fileExtension)
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

								CacheManifest = await Deserialize<ConcurrentDictionary<string, IManifestEntry>>(stream);
							}
						}
						else
						{
							if (!Directory.Exists(DirectoryPath))
							{
								Directory.CreateDirectory(DirectoryPath);
							}

							CacheManifest = new ConcurrentDictionary<string, IManifestEntry>();
							using (var stream = new FileStream(ManifestPath, FileMode.OpenOrCreate, FileAccess.Write))
							{
								await Serialize(stream, CacheManifest);
							}
						}
					}
				}
			}
		}

		public async Task SaveManifest()
		{
			using (await ManifestLock.LockAsync())
			{
				using (var stream = new FileStream(ManifestPath, FileMode.Open, FileAccess.Write))
				{
					await Serialize(stream, CacheManifest);
				}
			}
		}

#if NETSTANDARD2_0
		private unsafe string GetFileName(string cacheKey)
		{
			var bytes = Encoding.UTF8.GetBytes(cacheKey);
			var hashBytes = FileNameHashAlgorithm.ComputeHash(bytes);
		
#elif NETSTANDARD2_1
		private unsafe string GetFileName(ReadOnlySpan<char> cacheKey)
		{
			var encoding = Encoding.UTF8;
			var bytesRequired = encoding.GetByteCount(cacheKey);
			Span<byte> bytes = stackalloc byte[bytesRequired];
			encoding.GetBytes(cacheKey, bytes);

			Span<byte> hashBytes = stackalloc byte[16];
			FileNameHashAlgorithm.TryComputeHash(bytes, hashBytes, out var _);
#endif

			var fileExtensionLength = FileExtension?.Length ?? 0;

			//Based on byte conversion implementation in BitConverter (but with the dash stripped)
			//https://github.com/nchikanov/coreclr/blob/fbc11ea6afdaa2fe7b9377446d6bb0bd447d5cb5/src/mscorlib/shared/System/BitConverter.cs#L409-L440
			static char GetHexValue(int i)
			{
				if (i < 10)
				{
					return (char)(i + '0');
				}

				return (char)(i - 10 + 'A');
			}

			var charArrayLength = 32 + fileExtensionLength;
			var charArrayPtr = stackalloc char[charArrayLength];

			var charPtr = charArrayPtr;
			for (var i = 0; i < 16; i++)
			{
				var hashByte = hashBytes[i];
				*charPtr++ = GetHexValue(hashByte >> 4);
				*charPtr++ = GetHexValue(hashByte & 0xF);
			}

			for (var i = 0; i < fileExtensionLength; i++)
			{
				*charPtr++ = FileExtension[i];
			}

			return new string(charArrayPtr, 0, charArrayLength);
		}

		public async Task Cleanup()
		{
			await TryLoadManifest();

			foreach (var cachePair in CacheManifest)
			{
				var manifestEntry = cachePair.Value;
				var expiryDate = manifestEntry.CachedAt.Add(manifestEntry.TimeToLive);
				if (expiryDate < DateTime.UtcNow && CacheManifest.TryRemove(cachePair.Key, out var _))
				{
					if (FileLock.TryRemove(manifestEntry.FileName, out var lockObj))
					{
						using (await lockObj.WriterLockAsync())
						{
							var path = Path.Combine(DirectoryPath, manifestEntry.FileName);
							if (File.Exists(path))
							{
								File.Delete(path);
							}
						}
					}
				}
			}
		}

		public async Task Evict(string cacheKey)
		{
			await TryLoadManifest();

			if (CacheManifest.TryRemove(cacheKey, out var manifestEntry))
			{
				if (FileLock.TryRemove(manifestEntry.FileName, out var lockObj))
				{
					using (await lockObj.WriterLockAsync())
					{
						var path = Path.Combine(DirectoryPath, manifestEntry.FileName);
						if (File.Exists(path))
						{
							File.Delete(path);
						}
					}
				}
			}
		}

		public async Task<CacheEntry<T>> Get<T>(string cacheKey)
		{
			await TryLoadManifest();

			if (CacheManifest.TryGetValue(cacheKey, out var manifestEntry))
			{
				var lockObj = FileLock.GetOrAdd(manifestEntry.FileName, (name) => new AsyncReaderWriterLock());
				using (await lockObj.ReaderLockAsync())
				{
					//By the time we have the lock, confirm we still have a cache
					if (CacheManifest.ContainsKey(cacheKey))
					{
						var path = Path.Combine(DirectoryPath, manifestEntry.FileName);
						using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
						{
							var value = await Deserialize<T>(stream);
							return new CacheEntry<T>(value, manifestEntry.CachedAt, manifestEntry.TimeToLive);
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

			var manifestEntry = CacheManifest.GetOrAdd(cacheKey, (key) => new TManifest
			{
				FileName = GetFileName(cacheKey)
			});

			//Update the manifest entry with the new cache entry date/times
			manifestEntry.CachedAt = cacheEntry.CachedAt;
			manifestEntry.TimeToLive = cacheEntry.TimeToLive;

			var lockObj = FileLock.GetOrAdd(manifestEntry.FileName, (name) => new AsyncReaderWriterLock());

			using (await lockObj.WriterLockAsync())
			{
				var path = Path.Combine(DirectoryPath, manifestEntry.FileName);
				using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
				{
					await Serialize(stream, cacheEntry.Value);
				}
			}
		}

#if NETSTANDARD2_0
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (Disposed)
			{
				return;
			}

			if (disposing)
			{
				_ = SaveManifest();
				FileNameHashAlgorithm.Dispose();
			}

			Disposed = true;
		}
#elif NETSTANDARD2_1
		public async ValueTask DisposeAsync()
		{
			if (Disposed)
			{
				return;
			}

			await SaveManifest();
			FileNameHashAlgorithm.Dispose();

			Disposed = true;
		}
#endif
	}
}
