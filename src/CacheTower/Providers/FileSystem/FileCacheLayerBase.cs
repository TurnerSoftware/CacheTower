using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace CacheTower.Providers.FileSystem
{
#if NETSTANDARD2_0
	public abstract class FileCacheLayerBase<TManifest> : IAsyncCacheLayer, IDisposable where TManifest : IManifestEntry, new()
#elif NETSTANDARD2_1
	public abstract class FileCacheLayerBase<TManifest> : IAsyncCacheLayer, IAsyncDisposable where TManifest : IManifestEntry, new()
#endif
	{
		private bool Disposed = false;
		private string DirectoryPath { get; }
		private string ManifestPath { get; }
		private string FileExtension { get; }

		private SemaphoreSlim ManifestLock { get; } = new SemaphoreSlim(1, 1);
		private bool? IsManifestAvailable { get; set; }

		private HashAlgorithm FileNameHashAlgorithm { get; } = MD5.Create();

		private ConcurrentDictionary<string, IManifestEntry> CacheManifest { get; set; }
		private ConcurrentDictionary<string, AsyncReaderWriterLock> FileLock { get; }

		protected FileCacheLayerBase(string directoryPath, string fileExtension)
		{
			DirectoryPath = directoryPath;
			FileExtension = fileExtension;
			ManifestPath = Path.Combine(directoryPath, "manifest" + fileExtension);
			FileLock = new ConcurrentDictionary<string, AsyncReaderWriterLock>(StringComparer.Ordinal);
		}

		protected abstract T Deserialize<T>(Stream stream);

		protected abstract void Serialize<T>(Stream stream, T value);

		private async Task<T> DeserializeFileAsync<T>(string path)
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024))
			using (var memStream = new MemoryStream((int)stream.Length))
			{
				await stream.CopyToAsync(memStream);
				memStream.Seek(0, SeekOrigin.Begin);
				return Deserialize<T>(memStream);
			}
		}

		private async Task SerializeFileAsync<T>(string path, T value)
		{
			using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1024))
			using (var memStream = new MemoryStream())
			{
				Serialize(memStream, value);
				memStream.Seek(0, SeekOrigin.Begin);
				await memStream.CopyToAsync(stream);
			}
		}

		private async Task TryLoadManifestAsync()
		{
			//Avoid unnecessary lock contention way after manifest is loaded by checking before lock
			if (CacheManifest == null)
			{
				await ManifestLock.WaitAsync();
				try
				{
					//Check that once we have lock (due to a race condition on the outer check) that we still need to load the manifest
					if (CacheManifest == null)
					{
						if (File.Exists(ManifestPath))
						{
							CacheManifest = await DeserializeFileAsync<ConcurrentDictionary<string, IManifestEntry>>(ManifestPath);
						}
						else
						{
							if (!Directory.Exists(DirectoryPath))
							{
								Directory.CreateDirectory(DirectoryPath);
							}

							CacheManifest = new ConcurrentDictionary<string, IManifestEntry>();
							await SerializeFileAsync(ManifestPath, CacheManifest);
						}
					}
				}
				finally
				{
					ManifestLock.Release();
				}
			}
		}

		public async Task SaveManifestAsync()
		{
			await ManifestLock.WaitAsync();
			try
			{
				if (!Directory.Exists(DirectoryPath))
				{
					Directory.CreateDirectory(DirectoryPath);
				}

				await SerializeFileAsync(ManifestPath, CacheManifest);
			}
			finally
			{
				ManifestLock.Release();
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
			//https://github.com/dotnet/coreclr/blob/fbc11ea6afdaa2fe7b9377446d6bb0bd447d5cb5/src/mscorlib/shared/System/BitConverter.cs#L409-L440
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

		public async Task CleanupAsync()
		{
			await TryLoadManifestAsync();

			var currentTime = DateTime.UtcNow;
			foreach (var cachePair in CacheManifest)
			{
				var manifestEntry = cachePair.Value;
				if (manifestEntry.Expiry < currentTime && CacheManifest.TryRemove(cachePair.Key, out var _))
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

		public async Task EvictAsync(string cacheKey)
		{
			await TryLoadManifestAsync();

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

		public async Task<CacheEntry<T>> GetAsync<T>(string cacheKey)
		{
			await TryLoadManifestAsync();

			if (CacheManifest.TryGetValue(cacheKey, out var manifestEntry))
			{
				var lockObj = FileLock.GetOrAdd(manifestEntry.FileName, (name) => new AsyncReaderWriterLock());
				using (await lockObj.ReaderLockAsync())
				{
					//By the time we have the lock, confirm we still have a cache
					if (CacheManifest.ContainsKey(cacheKey))
					{
						var path = Path.Combine(DirectoryPath, manifestEntry.FileName);
						var value = await DeserializeFileAsync<T>(path);
						return new CacheEntry<T>(value, manifestEntry.Expiry);
					}
				}
			}

			return default;
		}

		public async Task<bool> IsAvailableAsync(string cacheKey)
		{
			if (IsManifestAvailable == null)
			{
				try
				{
					await TryLoadManifestAsync();
					IsManifestAvailable = true;
				}
				catch
				{
					IsManifestAvailable = false;
				}
			}

			return IsManifestAvailable.Value;
		}

		public async Task SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			await TryLoadManifestAsync();

			var manifestEntry = CacheManifest.GetOrAdd(cacheKey, (key) => new TManifest
			{
				FileName = GetFileName(cacheKey)
			});

			//Update the manifest entry with the new expiry
			manifestEntry.Expiry = cacheEntry.Expiry;

			var lockObj = FileLock.GetOrAdd(manifestEntry.FileName, (name) => new AsyncReaderWriterLock());

			using (await lockObj.WriterLockAsync())
			{
				var path = Path.Combine(DirectoryPath, manifestEntry.FileName);
				await SerializeFileAsync(path, cacheEntry.Value);
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
				SaveManifestAsync().Wait();
				ManifestLock.Dispose();
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

			await SaveManifestAsync();
			ManifestLock.Dispose();
			FileNameHashAlgorithm.Dispose();

			Disposed = true;
		}
#endif
	}
}
