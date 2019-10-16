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

		private ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> TrackedRefreshes { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();

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

		public async Task OnValueRefreshAsync(string stackId, string requestId, string cacheKey, TimeSpan timeToLive)
		{
			TrackedRefreshes[stackId].TryAdd(cacheKey, default);
			await Subscriber.PublishAsync(RedisChannel, cacheKey, CommandFlags.FireAndForget);
		}

		public void Register(ICacheStack cacheStack)
		{
			var knownRefreshes = new ConcurrentDictionary<string, bool>();
			TrackedRefreshes.TryAdd(cacheStack.StackId, knownRefreshes);

			Subscriber.Subscribe(RedisChannel, async (channel, value) =>
			{
				string cacheKey = value;
				if (!knownRefreshes.TryRemove(cacheKey, out var _))
				{
					await cacheStack.EvictAsync(cacheKey);
				}
			}, CommandFlags.FireAndForget);
		}
	}
}
