using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	public class RemoteEvictionExtension : IValueRefreshExtension
	{
		private ConnectionMultiplexer Connection { get; }
		private ISubscriber Subscriber { get; }
		private string RedisChannel { get; }

		private bool IsRegistered { get; set;  }
		private ConcurrentDictionary<string, string> FlaggedRefreshes { get; } = new ConcurrentDictionary<string, string>();

		public RemoteEvictionExtension(ConnectionMultiplexer connection, string channelPrefix = "CacheTower")
		{
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));

			if (channelPrefix == null)
			{
				throw new ArgumentNullException(nameof(channelPrefix));
			}

			Subscriber = Connection.GetSubscriber();
			RedisChannel = $"{channelPrefix}.RemoteEviction";
		}

		public async Task OnValueRefreshAsync(string requestId, string cacheKey, TimeSpan timeToLive)
		{
			FlaggedRefreshes.TryAdd(cacheKey, requestId);
			await Subscriber.PublishAsync(RedisChannel, cacheKey, CommandFlags.FireAndForget);
		}

		public void Register(ICacheStack cacheStack)
		{
			if (IsRegistered)
			{
				throw new InvalidOperationException($"{nameof(RemoteEvictionExtension)} can only be registered to one {nameof(ICacheStack)}");
			}
			IsRegistered = true;

			Subscriber.Subscribe(RedisChannel, async (channel, value) =>
			{
				string cacheKey = value;
				if (!FlaggedRefreshes.TryRemove(cacheKey, out var _))
				{
					await cacheStack.EvictAsync(cacheKey);
				}
			}, CommandFlags.FireAndForget);
		}
	}
}
