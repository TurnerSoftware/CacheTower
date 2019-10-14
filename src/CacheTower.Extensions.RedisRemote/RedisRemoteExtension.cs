using System;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.RedisRemote
{
	public class RedisRemoteExtension : IValueRefreshExtension
	{
		private ConnectionMultiplexer ConnectionMultiplexer { get; }
		private ISubscriber Subscriber { get; }
		private string ValueRefreshChannel { get; }

		public RedisRemoteExtension(ConnectionMultiplexer connectionMultiplexer, string channelPrefix = "CacheTower")
		{
			ConnectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

			if (channelPrefix == null)
			{
				throw new ArgumentNullException(nameof(channelPrefix));
			}

			Subscriber = ConnectionMultiplexer.GetSubscriber();
			ValueRefreshChannel = $"{channelPrefix}.ValueRefresh";
		}

		public async Task OnValueRefreshAsync(string cacheKey, TimeSpan timeToLive)
		{
			await Subscriber.PublishAsync(ValueRefreshChannel, cacheKey, CommandFlags.FireAndForget);
		}

		public void Register(ICacheStack cacheStack)
		{
			Subscriber.Subscribe(ValueRefreshChannel, async (channel, value) =>
			{
				string cacheKey = value;
				await cacheStack.EvictAsync(cacheKey);
			}, CommandFlags.FireAndForget);
		}
	}
}
