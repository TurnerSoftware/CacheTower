using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace CacheTower.Extensions.Redis
{
	/// <summary>
	/// Provides distributed cache locking via Redis.
	/// This approach uses busy wait instead of pub/sub to check locks.
	/// </summary>
	public class RedisBusyLockExtension
	{
		private IDatabaseAsync Database { get; }
		private RedisBusyLockOptions Options { get; }

		private ICacheStack? RegisteredStack { get; set; }

		internal ConcurrentDictionary<string, TaskCompletionSource<bool>> LockedOnKeyRefresh { get; }

		/// <summary>
		/// Creates a new instance of <see cref="RedisBusyLockExtension"/> with the given <paramref name="connection"/> and default lock options.
		/// </summary>
		/// <param name="connection">The primary connection to Redis where the distributed lock will be co-ordinated through.</param>
		public RedisBusyLockExtension(IConnectionMultiplexer connection) : this(connection, RedisBusyLockOptions.Default) { }

		/// <summary>
		/// Creates a new instance of <see cref="RedisBusyLockExtension"/> with the given <paramref name="connection"/> and <paramref name="options"/>.
		/// </summary>
		/// <param name="connection">The primary connection to Redis where the distributed lock will be co-ordinated through.</param>
		/// <param name="options">The lock options to configure the behaviour of locking.</param>
		public RedisBusyLockExtension(IConnectionMultiplexer connection, RedisBusyLockOptions options)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			Options = options;
			Database = connection.GetDatabase(options.DatabaseIndex);

			LockedOnKeyRefresh = new ConcurrentDictionary<string, TaskCompletionSource<bool>>(StringComparer.Ordinal);
		}

		/// <inheritdoc/>
		public void Register(ICacheStack cacheStack)
		{
			if (RegisteredStack != null)
			{
				throw new InvalidOperationException($"{nameof(RedisBusyLockExtension)} can only be registered to one {nameof(ICacheStack)}");
			}

			RegisteredStack = cacheStack;
		}

		/// <remarks>
		/// The <see cref="RedisBusyLockExtension"/> attempts to set a key in Redis representing whether it has achieved a lock.
		/// If it succeeds to set the key, it continues to refresh the value, removing the lock when finished.
		/// If it fails to set the key, the first context will set a check process to check for the lock being released,
		/// once done it can retrieve latest update from the cache stack and returning the value.
		/// </remarks>
		/// <inheritdoc/>
		public async ValueTask<CacheEntry<T>> WithRefreshAsync<T>(string cacheKey, Func<ValueTask<CacheEntry<T>>> valueProvider, CacheSettings settings)
		{
			var lockToken = Guid.NewGuid().ToString();
			var lockKey = $"{cacheKey}_lock";
			var hasLock = await Database.LockTakeAsync(lockKey, lockToken, Options.LockTimeout);

			if (hasLock)
			{
				try
				{
					var cacheEntry = await valueProvider();
					return cacheEntry;
				}
				finally
				{
					await Database.LockReleaseAsync(lockKey, lockToken);
				}
			}

			var completionSource = LockedOnKeyRefresh.GetOrAdd(cacheKey, key =>
			{
				var tcs = new TaskCompletionSource<bool>();

				_ = TestLock(tcs);

				return tcs;

				async Task TestLock(TaskCompletionSource<bool> taskCompletionSource)
				{
					var spinAttempt = 0;

					while (spinAttempt <= Options.SpinAttempts)
					{
						spinAttempt++;

						var lockQuery = await Database.LockQueryAsync(lockKey);

						if (lockQuery.HasValue)
						{
							await Task.Delay(Options.SpinTime);
							continue;
						}

						taskCompletionSource.TrySetResult(true);
						return;
					}

					taskCompletionSource.TrySetCanceled();
				}
			});

			//Last minute check to confirm whether waiting is required (in case the notification is missed)
			var currentEntry = await RegisteredStack!.GetAsync<T>(cacheKey);
			if (currentEntry != null && currentEntry.GetStaleDate(settings) > Internal.DateTimeProvider.Now)
			{
				UnlockWaitingTasks(cacheKey);
				return currentEntry;
			}

			//Lock until we are notified to be unlocked
			await completionSource.Task;

			//Get the updated value from the cache stack
			return (await RegisteredStack.GetAsync<T>(cacheKey))!;
		}

		private void UnlockWaitingTasks(string cacheKey)
		{
			if (LockedOnKeyRefresh.TryRemove(cacheKey, out var waitingTasks))
			{
				waitingTasks.TrySetResult(true);
			}
		}
	}
}
