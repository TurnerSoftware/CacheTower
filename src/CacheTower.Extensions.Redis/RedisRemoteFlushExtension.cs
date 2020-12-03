using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	public class RedisRemoteFlushExtension : ICacheFlushExtension
	{
		private ISubscriber Subscriber { get; }
		private string RedisChannel { get; }

		private bool IsRegistered { get; set;  }

		private readonly object FlaggedCacheFlushObj = new object();
		private bool HasFlushTriggered { set; get; }
		private ICacheLayer[] FlushFromLayers { get; }

		public RedisRemoteFlushExtension(ConnectionMultiplexer connection, ICacheLayer[] flushFromLayers, string channelPrefix = "CacheTower")
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			if (flushFromLayers == null)
			{
				throw new ArgumentNullException(nameof(flushFromLayers));
			}

			if (channelPrefix == null)
			{
				throw new ArgumentNullException(nameof(channelPrefix));
			}

			Subscriber = connection.GetSubscriber();
			RedisChannel = $"{channelPrefix}.RemoteFlush";
			FlushFromLayers = flushFromLayers;
		}

		public async ValueTask OnCacheFlushAsync()
		{
			lock (FlaggedCacheFlushObj)
			{
				HasFlushTriggered = true;
			}

			await Subscriber.PublishAsync(RedisChannel, RedisValue.Null, CommandFlags.FireAndForget);
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
					var shouldFlushLocally = false;
					lock (FlaggedCacheFlushObj)
					{
						shouldFlushLocally = HasFlushTriggered;
						if (shouldFlushLocally)
						{
							HasFlushTriggered = false;
						}
					}

					if (shouldFlushLocally)
					{
						for (var i = 0; i < FlushFromLayers.Length; i++)
						{
							await FlushFromLayers[i].FlushAsync();
						}
					}
				});
		}
	}
}
