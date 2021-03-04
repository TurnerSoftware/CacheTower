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
	/// <summary>
	/// Provides flexible file system caching.
	/// This uses a cache manifest file to keep track of the cache entries and their corresponding files.
	/// The individual cache entries are stored within their own files.
	/// </summary>
	/// <typeparam name="TManifest"></typeparam>
	public abstract class FileCacheLayerBase<TManifest> : ICacheLayer, IAsyncDisposable where TManifest : IManifestEntry, new()
	{
		private bool Disposed = false;
		private string DirectoryPath { get; }
		private string ManifestPath { get; }
		private string FileExtension { get; }

		private SemaphoreSlim ManifestLock { get; } = new SemaphoreSlim(1, 1);
		private bool? IsManifestAvailable { get; set; }

		private HashAlgorithm FileNameHashAlgorithm { get; } = MD5.Create();

		private ConcurrentDictionary<string?, IManifestEntry>? CacheManifest { get; set; }
		private ConcurrentDictionary<string?, AsyncReaderWriterLock> FileLock { get; }

		/// <summary>
		/// Initialises the file cache layer with the given <paramref name="directoryPath"/> and <paramref name="fileExtension"/>.
		/// </summary>
		/// <param name="directoryPath"></param>
		/// <param name="fileExtension"></param>
		protected FileCacheLayerBase(string directoryPath, string fileExtension)
		{
			DirectoryPath = directoryPath;
			FileExtension = fileExtension;
			ManifestPath = Path.Combine(directoryPath, "manifest" + fileExtension);
			FileLock = new ConcurrentDictionary<string?, AsyncReaderWriterLock>(StringComparer.Ordinal);
		}

		/// <summary>
		/// Provides stream deserialization to <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type to deserialize to.</typeparam>
		/// <param name="stream">The stream to deserialize from.</param>
		/// <returns></returns>
		protected abstract T Deserialize<T>(Stream stream);

		/// <summary>
		/// Provides <typeparamref name="T"/> serialization to a stream.
		/// </summary>
		/// <typeparam name="T">The type for <paramref name="value"/> that will be serialized.</typeparam>
		/// <param name="stream">The stream that the serialization is written to.</param>
		/// <param name="value">The value to be serialized.</param>
		protected abstract void Serialize<T>(Stream stream, T value);

		private async Task<T?> DeserializeFileAsync<T>(string path)
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
			if (CacheManifest is null)
			{
				await ManifestLock.WaitAsync();
				try
				{
					//Check that once we have lock (due to a race condition on the outer check) that we still need to load the manifest
					if (CacheManifest is null)
					{
						if (File.Exists(ManifestPath))
						{
							CacheManifest = await DeserializeFileAsync<ConcurrentDictionary<string?, IManifestEntry>>(ManifestPath);
						}
						else
						{
							if (!Directory.Exists(DirectoryPath))
							{
								Directory.CreateDirectory(DirectoryPath);
							}

							CacheManifest = new ConcurrentDictionary<string?, IManifestEntry>();
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

		/// <summary>
		/// Saves the cache manifest to the file system.
		/// </summary>
		/// <returns></returns>
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
				*charPtr++ = FileExtension![i];
			}

			return new string(charArrayPtr, 0, charArrayLength);
		}

		/// <inheritdoc/>
		public async ValueTask CleanupAsync()
		{
			await TryLoadManifestAsync();

			var currentTime = DateTime.UtcNow;
			foreach (var cachePair in CacheManifest!)
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

		/// <inheritdoc/>
		public async ValueTask EvictAsync(string cacheKey)
		{
			await TryLoadManifestAsync();

			if (CacheManifest!.TryRemove(cacheKey, out var manifestEntry))
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

		/// <inheritdoc/>
		public async ValueTask FlushAsync()
		{
			await TryLoadManifestAsync();

			foreach (var manifestEntry in CacheManifest!.Values)
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

			CacheManifest.Clear();

			await SaveManifestAsync();
		}

		/// <inheritdoc/>
		public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
		{
			await TryLoadManifestAsync();

			if (CacheManifest!.TryGetValue(cacheKey, out var manifestEntry))
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

		/// <remarks>
		/// For <see cref="FileCacheLayerBase{TManifest}"/>, availability is determined by being able to load the manifest.
		/// </remarks>
		/// <inheritdoc/>
		public async ValueTask<bool> IsAvailableAsync(string cacheKey)
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

		/// <inheritdoc/>
		public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			await TryLoadManifestAsync();

			var manifestEntry = CacheManifest!.GetOrAdd(cacheKey, (key) => new TManifest
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

		/// <summary>
		/// Saves the manifest to the file system and releases all resources associated.
		/// </summary>
		/// <returns></returns>
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
	}
}
