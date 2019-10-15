using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	public class DistributedEvictionExtension : IValueRefreshExtension
	{
		private ConnectionMultiplexer ConnectionMultiplexer { get; }
		private ISubscriber Subscriber { get; }
		private string RedisChannel { get; }

		private ConcurrentDictionary<Guid, ConcurrentDictionary<string, bool>> TrackedRefreshes { get; } = new ConcurrentDictionary<Guid, ConcurrentDictionary<string, bool>>();

		public DistributedEvictionExtension(ConnectionMultiplexer connectionMultiplexer, string channelPrefix = "CacheTower")
		{
			ConnectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

			if (channelPrefix == null)
			{
				throw new ArgumentNullException(nameof(channelPrefix));
			}

			Subscriber = ConnectionMultiplexer.GetSubscriber();
			RedisChannel = $"{channelPrefix}.ValueRefresh";
		}

		public async Task OnValueRefreshAsync(Guid stackId, string cacheKey, TimeSpan timeToLive)
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
