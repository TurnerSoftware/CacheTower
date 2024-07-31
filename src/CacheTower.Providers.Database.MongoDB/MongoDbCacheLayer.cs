using System.Linq;
using System.Threading.Tasks;
using CacheTower.Providers.Database.MongoDB.Commands;
using CacheTower.Providers.Database.MongoDB.Entities;
using MongoFramework;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Indexing;
using MongoFramework.Infrastructure.Linq;
using MongoFramework.Infrastructure.Mapping;

namespace CacheTower.Providers.Database.MongoDB
{
	/// <remarks>
	/// The <see cref="MongoDbCacheLayer"/> allows caching through a MongoDB server.
	/// Cache entries are serialized to BSON using <see cref="global::MongoDB.Bson.Serialization.BsonSerializer"/>.
	/// </remarks>
	/// <inheritdoc cref="ICacheLayer"/>
	public class MongoDbCacheLayer : IDistributedCacheLayer
	{
		private bool? IsDatabaseAvailable { get; set; }

		private IMongoDbConnection Connection { get; }

		private bool HasSetIndexes = false;
		
		static MongoDbCacheLayer()
		{
			//Due to a change in 2.19.0, we need to ensure that DbCachedEntry is registered early.
			//More importantly than the type itself is that the TypeDiscoverySerializer is registered
			//which is done automatically with use of type discovery serialization.
			//This may need to be revisited later with a future update to MongoFramework.
			_ = EntityMapping.RegisterType(typeof(DbCachedEntry));
		}

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
				await EntityIndexWriter.ApplyIndexingAsync<DbCachedEntry>(Connection).ConfigureAwait(false);
			}
		}

		/// <inheritdoc/>
		public async ValueTask CleanupAsync()
		{
			await TryConfigureIndexes().ConfigureAwait(false);
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new CleanupCommand() }, default).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async ValueTask EvictAsync(string cacheKey)
		{
			await TryConfigureIndexes().ConfigureAwait(false);
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new EvictCommand(cacheKey) }, default).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async ValueTask FlushAsync()
		{
			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { new FlushCommand() }, default).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async ValueTask<CacheEntry<T>?> GetAsync<T>(string cacheKey)
		{
			await TryConfigureIndexes().ConfigureAwait(false);

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
			await TryConfigureIndexes().ConfigureAwait(false);
			var command = new SetCommand(new DbCachedEntry
			{
				CacheKey = cacheKey,
				Expiry = cacheEntry.Expiry,
				Value = cacheEntry.Value!
			});

			await EntityCommandWriter.WriteAsync<DbCachedEntry>(Connection, new[] { command }, default).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async ValueTask<bool> IsAvailableAsync(string cacheKey)
		{
			if (IsDatabaseAvailable == null)
			{
				try
				{
					await TryConfigureIndexes().ConfigureAwait(false);
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
