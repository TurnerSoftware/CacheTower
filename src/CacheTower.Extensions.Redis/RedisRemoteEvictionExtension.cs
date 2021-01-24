using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	public class RedisRemoteEvictionExtension : ICacheChangeExtension
	{
		private ISubscriber Subscriber { get; }
		private string FlushChannel { get; }
		private string EvictionChannel { get; }

		private bool IsRegistered { get; set; }

		private readonly object LockObj = new object();
		private HashSet<string> FlaggedEvictions { get; }
		private bool HasFlushTriggered { get; set; }
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
			FlushChannel = $"{channelPrefix}.RemoteFlush";
			EvictionChannel = $"{channelPrefix}.RemoteEviction";
			FlaggedEvictions = new HashSet<string>(StringComparer.Ordinal);
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
			lock (LockObj)
			{
				FlaggedEvictions.Add(cacheKey);
			}

			await Subscriber.PublishAsync(EvictionChannel, cacheKey, CommandFlags.FireAndForget);
		}

		public async ValueTask OnCacheFlushAsync()
		{
			lock (LockObj)
			{
				HasFlushTriggered = true;
			}

			await Subscriber.PublishAsync(FlushChannel, RedisValue.EmptyString, CommandFlags.FireAndForget);
		}

		public void Register(ICacheStack cacheStack)
		{
			if (IsRegistered)
			{
				throw new InvalidOperationException($"{nameof(RedisRemoteEvictionExtension)} can only be registered to one {nameof(ICacheStack)}");
			}
			IsRegistered = true;

			Subscriber.Subscribe(EvictionChannel)
				.OnMessage(async (channelMessage) =>
				{
					string cacheKey = channelMessage.Message;

					var shouldEvictLocally = false;
					lock (LockObj)
					{
						shouldEvictLocally = FlaggedEvictions.Remove(cacheKey) == false;
					}

					if (shouldEvictLocally)
					{
						for (var i = 0; i < EvictFromLayers.Length; i++)
						{
							await EvictFromLayers[i].EvictAsync(cacheKey);
						}
					}
				});

			Subscriber.Subscribe(FlushChannel)
				.OnMessage(async (channelMessage) =>
				{
					var shouldFlushLocally = false;
					lock (LockObj)
					{
						shouldFlushLocally = !HasFlushTriggered;
						HasFlushTriggered = false;
					}

					if (shouldFlushLocally)
					{
						for (var i = 0; i < EvictFromLayers.Length; i++)
						{
							await EvictFromLayers[i].FlushAsync();
						}
					}
				});
		}
	}
}
