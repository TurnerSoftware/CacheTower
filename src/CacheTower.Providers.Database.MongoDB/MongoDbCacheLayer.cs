using System.Linq;
using System.Threading.Tasks;
using CacheTower.Providers.Database.MongoDB.Commands;
using CacheTower.Providers.Database.MongoDB.Entities;
using MongoFramework;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Linq;

namespace CacheTower.Providers.Database.MongoDB
{
	/// <remarks>
	/// The <see cref="MongoDbCacheLayer"/> allows caching through a MongoDB server.
	/// Cache entries are serialized to BSON using <see cref="global::MongoDB.Bson.Serialization.BsonSerializer"/>.
	/// </remarks>
	/// <inheritdoc cref="ICacheLayer"/>
	public class MongoDbCacheLayer : ICacheLayer
	{
		private bool? IsDatabaseAvailable { get; set; }

		private IMongoDbConnection Connection { get; }

		private bool HasSetIndexes = false;

		/// <summary>
		/// Creates a new instance of <see cref="MongoDbCacheLayer"/> with the given <paramref name="connection"/>.
		/// </summary>
		/// <param name="connection">The connection to the MongoDB database.</param>
		public MongoDbCacheLayer(IMongoDbConnection connection)
		{
			Connection = connection;
		}

		private async ValueTask TryConfigureIndexes()
		{
			if (!HasSetIndexes)
			{
				HasSetIndexes = true;
				await EntityIndexWriter.ApplyIndexingAsync<DbCachedEntry>(Connection);
			}
		}

		/// <inheritdoc/>
		public async ValueTask CleanupAsync()
		{
			await TryConfigureIndexes();
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new CleanupCommand() }, default);
		}

		/// <inheritdoc/>
		public async ValueTask EvictAsync(string cacheKey)
		{
			await TryConfigureIndexes();
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new EvictCommand(cacheKey) }, default);
		}

		/// <inheritdoc/>
		public async ValueTask FlushAsync()
		{
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new FlushCommand() }, default);
		}

		/// <inheritdoc/>
		public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
		{
			await TryConfigureIndexes();

			var provider = new MongoFrameworkQueryProvider<DbCachedEntry>(Connection);
			var queryable = new MongoFrameworkQueryable<DbCachedEntry>(provider);

			var dbEntry = queryable.Where(e => e.CacheKey == cacheKey).FirstOrDefault();
			var cacheEntry = default(CacheEntry<T>);

			if (dbEntry != default)
			{
				cacheEntry = new CacheEntry<T>((T)dbEntry.Value!, dbEntry.Expiry);
			}

			return cacheEntry;
		}

		/// <inheritdoc/>
		public async ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry)
		{
			await TryConfigureIndexes();
			var command = new SetCommand(new DbCachedEntry
			{
				CacheKey = cacheKey,
				Expiry = cacheEntry.Expiry,
				Value = cacheEntry.Value!
			});

			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { command }, default);
		}

		/// <inheritdoc/>
		public async ValueTask<bool> IsAvailableAsync(string cacheKey)
		{
			if (IsDatabaseAvailable == null)
			{
				try
				{
					await TryConfigureIndexes();
					IsDatabaseAvailable = true;
				}
				catch
				{
					IsDatabaseAvailable = false;
				}
			}

			return IsDatabaseAvailable.Value;
		}
	}
}
