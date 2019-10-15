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
		private string ValueRefreshChannel { get; }

		private Dictionary<ICacheStack, ConcurrentDictionary<string, bool>> TrackedRefreshes { get; } = new Dictionary<ICacheStack, ConcurrentDictionary<string, bool>>();

		public DistributedEvictionExtension(ConnectionMultiplexer connectionMultiplexer, string channelPrefix = "CacheTower")
		{
			ConnectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

			if (channelPrefix == null)
			{
				throw new ArgumentNullException(nameof(channelPrefix));
			}

			Subscriber = ConnectionMultiplexer.GetSubscriber();
			ValueRefreshChannel = $"{channelPrefix}.ValueRefresh";
		}

		public async Task OnValueRefreshAsync(ICacheStack cacheStack, string cacheKey, TimeSpan timeToLive)
		{
			TrackedRefreshes[cacheStack].TryAdd(cacheKey, default);
			await Subscriber.PublishAsync(ValueRefreshChannel, cacheKey, CommandFlags.FireAndForget);
		}

		public void Register(ICacheStack cacheStack)
		{
			var knownRefreshes = new ConcurrentDictionary<string, bool>();
			TrackedRefreshes.Add(cacheStack, knownRefreshes);

			Subscriber.Subscribe(ValueRefreshChannel, async (channel, value) =>
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
