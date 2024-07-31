using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CacheTower.Internal;
using Nito.AsyncEx;

namespace CacheTower.Providers.FileSystem
{
	/// <summary>
	/// Provides flexible file system caching.
	/// This uses a cache manifest file to keep track of the cache entries and their corresponding files.
	/// The individual cache entries are stored within their own files.
	/// </summary>
	public class FileCacheLayer : ILocalCacheLayer, IAsyncDisposable
	{
		private bool Disposed = false;
		private readonly FileCacheLayerOptions Options;
		private readonly string ManifestPath;

		private readonly CancellationTokenSource ManifestSavingCancellationTokenSource;
		private readonly Task BackgroundManifestSavingTask;

		private readonly SemaphoreSlim ManifestLock = new(1, 1);

		private bool? IsManifestAvailable;

		private ConcurrentDictionary<string?, ManifestEntry>? CacheManifest;
		private readonly ConcurrentDictionary<string?, AsyncReaderWriterLock> FileLock;

		/// <summary>
		/// Initialises the <see cref="FileCacheLayer"/> with the provided <paramref name="options"/>.
		/// </summary>
		/// <param name="options">Various options that control the behaviour of the <see cref="FileCacheLayer"/>.</param>
		public FileCacheLayer(FileCacheLayerOptions options)
		{
			Options = options;
			ManifestPath = Path.Combine(options.DirectoryPath, "manifest");
			FileLock = new ConcurrentDictionary<string?, AsyncReaderWriterLock>(StringComparer.Ordinal);

			ManifestSavingCancellationTokenSource = new();
			BackgroundManifestSavingTask = BackgroundManifestSaving();
		}

		private async Task BackgroundManifestSaving()
		{
			try
			{
				var cancellationToken = ManifestSavingCancellationTokenSource.Token;
				while (!cancellationToken.IsCancellationRequested)
				{
					await Task.Delay(Options.ManifestSaveInterval, cancellationToken).ConfigureAwait(false);
					await SaveManifestAsync().ConfigureAwait(false);
				}
			}
			catch (OperationCanceledException) { }
		}

		private async Task<T?> DeserializeFileAsync<T>(string path)
		{
			using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
			using var memStream = new MemoryStream((int)stream.Length);
			await stream.CopyToAsync(memStream).ConfigureAwait(false);
			memStream.Seek(0, SeekOrigin.Begin);
			return Options.Serializer.Deserialize<T>(memStream);
		}

		private async Task SerializeFileAsync<T>(string path, T value)
		{
			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1024);
			using var memStream = new MemoryStream();
			Options.Serializer.Serialize(memStream, value);
			memStream.Seek(0, SeekOrigin.Begin);
			await memStream.CopyToAsync(stream).ConfigureAwait(false);
		}

		private async Task TryLoadManifestAsync()
		{
			//Avoid unnecessary lock contention way after manifest is loaded by checking before lock
			if (CacheManifest is null)
			{
				await ManifestLock.WaitAsync().ConfigureAwait(false);
				try
				{
					//Check that once we have lock (due to a race condition on the outer check) that we still need to load the manifest
					if (CacheManifest is null)
					{
						if (File.Exists(ManifestPath))
						{
							CacheManifest = await DeserializeFileAsync<ConcurrentDictionary<string?, ManifestEntry>>(ManifestPath).ConfigureAwait(false);
							CacheManifest ??= new();
						}
						else
						{
							if (!Directory.Exists(Options.DirectoryPath))
							{
								Directory.CreateDirectory(Options.DirectoryPath);
							}

							CacheManifest = new();
							await SerializeFileAsync(ManifestPath, CacheManifest).ConfigureAwait(false);
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
			await ManifestLock.WaitAsync().ConfigureAwait(false);
			try
			{
				if (!Directory.Exists(Options.DirectoryPath))
				{
					Directory.CreateDirectory(Options.DirectoryPath);
				}

				await SerializeFileAsync(ManifestPath, CacheManifest).ConfigureAwait(false);
			}
			finally
			{
				ManifestLock.Release();
			}
		}

		/// <inheritdoc/>
		public async ValueTask CleanupAsync()
		{
			await TryLoadManifestAsync().ConfigureAwait(false);

			var currentTime = DateTimeProvider.Now;
			foreach (var cachePair in CacheManifest!)
			{
				var manifestEntry = cachePair.Value;
				if (manifestEntry.Expiry < currentTime && CacheManifest.TryRemove(cachePair.Key, out var _))
				{
					if (FileLock.TryRemove(manifestEntry.FileName, out var lockObj))
					{
						using (await lockObj.WriterLockAsync())
						{
							var path = Path.Combine(Options.DirectoryPath, manifestEntry.FileName);
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
			await TryLoadManifestAsync().ConfigureAwait(false);

			if (CacheManifest!.TryRemove(cacheKey, out var manifestEntry))
			{
				if (FileLock.TryRemove(manifestEntry.FileName, out var lockObj))
				{
					using (await lockObj.WriterLockAsync())
					{
						var path = Path.Combine(Options.DirectoryPath, manifestEntry.FileName);
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
			await TryLoadManifestAsync().ConfigureAwait(false);

			foreach (var manifestEntry in CacheManifest!.Values)
			{
				if (FileLock.TryRemove(manifestEntry.FileName, out var lockObj))
				{
					using (await lockObj.WriterLockAsync())
					{
						var path = Path.Combine(Options.DirectoryPath, manifestEntry.FileName);
						if (File.Exists(path))
						{
							File.Delete(path);
						}
					}
				}
			}

			CacheManifest.Clear();

			await SaveManifestAsync().ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
		{
			await TryLoadManifestAsync().ConfigureAwait(false);

			if (CacheManifest!.TryGetValue(cacheKey, out var manifestEntry))
			{
				var lockObj = FileLock.GetOrAdd(manifestEntry.FileName, static (name) => new AsyncReaderWriterLock());
				using (await lockObj.ReaderLockAsync())
				{
					//By the time we have the lock, confirm we still have a cache
					if (CacheManifest.ContainsKey(cacheKey))
					{
						var path = Path.Combine(Options.DirectoryPath, manifestEntry.FileName);
						if (File.Exists(path))
						{
							var value = await DeserializeFileAsync<T>(path).ConfigureAwait(false);
							return new CacheEntry<T>(value, manifestEntry.Expiry);

						}
						else
						{
							//Mismatch between manifest and file system - remove from manifest
							CacheManifest.TryRemove(cacheKey, out _);
							FileLock.TryRemove(cacheKey, out _);
						}
					}
				}
			}

			return default;
		}

		/// <remarks>
		/// For file caching, availability is determined by the ability to load the cache manifest.
		/// </remarks>
		/// <inheritdoc/>
		public async ValueTask<bool> IsAvailableAsync(string cacheKey)
		{
			if (IsManifestAvailable is null)
			{
				try
				{
					await TryLoadManifestAsync().ConfigureAwait(false);
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
			await TryLoadManifestAsync().ConfigureAwait(false);

			//Update the manifest entry with the new expiry
			if (CacheManifest!.TryGetValue(cacheKey, out var manifestEntry))
			{
				manifestEntry = manifestEntry with
				{
					Expiry = cacheEntry.Expiry
				};
			}
			else
			{
				manifestEntry = new(
					MD5HashUtility.ComputeHash(cacheKey!),
					cacheEntry.Expiry
				);
			}
			CacheManifest[cacheKey] = manifestEntry;

			var lockObj = FileLock.GetOrAdd(manifestEntry.FileName, static (name) => new AsyncReaderWriterLock());

			using (await lockObj.WriterLockAsync())
			{
				var path = Path.Combine(Options.DirectoryPath, manifestEntry.FileName);
				await SerializeFileAsync(path, cacheEntry.Value).ConfigureAwait(false);
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

			ManifestSavingCancellationTokenSource.Cancel();
			if (!BackgroundManifestSavingTask.IsFaulted)
			{
				await BackgroundManifestSavingTask.ConfigureAwait(false);
			}
			await SaveManifestAsync().ConfigureAwait(false);

			ManifestLock.Dispose();

			Disposed = true;
		}
	}
}
