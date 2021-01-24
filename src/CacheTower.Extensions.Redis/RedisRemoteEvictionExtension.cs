using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	public class RedisRemoteEvictionExtension : ICacheChangeExtension
	{
		private ISubscriber Subscriber { get; }
		private string RedisChannel { get; }

		private bool IsRegistered { get; set;  }

		private readonly object FlaggedRefreshesLockObj = new object();
		private HashSet<string> FlaggedRefreshes { get; }
		private ICacheLayer[] EvictFromLayers { get; }

		public RedisRemoteEvictionExtension(ConnectionMultiplexer connection, ICacheLayer[] evictFromLayers, string channelPrefix = "CacheTower")
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			if (evictFromLayers == null)
			{
				throw new ArgumentNullException(nameof(evictFromLayers));
			}

			if (channelPrefix == null)
			{
				throw new ArgumentNullException(nameof(channelPrefix));
			}

			Subscriber = connection.GetSubscriber();
			RedisChannel = $"{channelPrefix}.RemoteEviction";
			FlaggedRefreshes = new HashSet<string>(StringComparer.Ordinal);
			EvictFromLayers = evictFromLayers;
		}

		public ValueTask OnCacheUpdateAsync(string cacheKey, DateTime expiry)
		{
			return FlagEvictionAsync(cacheKey);
		}
		public ValueTask OnCacheEvictionAsync(string cacheKey)
		{
			return FlagEvictionAsync(cacheKey);
		}

		private async ValueTask FlagEvictionAsync(string cacheKey)
		{
			lock (FlaggedRefreshesLockObj)
			{
				FlaggedRefreshes.Add(cacheKey);
			}

			await Subscriber.PublishAsync(RedisChannel, cacheKey, CommandFlags.FireAndForget);
		}

		public void Register(ICacheStack cacheStack)
		{
			if (IsRegistered)
			{
				throw new InvalidOperationException($"{nameof(RedisRemoteEvictionExtension)} can only be registered to one {nameof(ICacheStack)}");
			}
			IsRegistered = true;

			Subscriber.Subscribe(RedisChannel)
				.OnMessage(async (channelMessage) =>
				{
					string cacheKey = channelMessage.Message;
					var shouldEvictLocally = false;
					lock (FlaggedRefreshesLockObj)
					{
						shouldEvictLocally = FlaggedRefreshes.Remove(cacheKey) == false;
					}

					if (shouldEvictLocally)
					{
						for (var i = 0; i < EvictFromLayers.Length; i++)
						{
							await EvictFromLayers[i].EvictAsync(cacheKey);
						}
					}
				});
		}
	}
}
