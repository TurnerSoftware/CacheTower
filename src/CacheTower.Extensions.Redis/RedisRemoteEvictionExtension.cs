using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Extensions.Redis
{
	public class RedisRemoteEvictionExtension : IValueRefreshExtension
	{
		private ISubscriber Subscriber { get; }
		private string RedisChannel { get; }

		private bool IsRegistered { get; set;  }

		private readonly object FlaggedRefreshesLockObj = new object();
		private HashSet<string> FlaggedRefreshes { get; }

		public RedisRemoteEvictionExtension(ConnectionMultiplexer connection, string channelPrefix = "CacheTower")
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			if (channelPrefix == null)
			{
				throw new ArgumentNullException(nameof(channelPrefix));
			}

			Subscriber = connection.GetSubscriber();
			RedisChannel = $"{channelPrefix}.RemoteEviction";
			FlaggedRefreshes = new HashSet<string>(StringComparer.Ordinal);
		}

		public async ValueTask OnValueRefreshAsync(string cacheKey, TimeSpan timeToLive)
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

			Subscriber.Subscribe(RedisChannel, async (channel, value) =>
			{
				string cacheKey = value;
				var shouldEvictLocally = false;
				lock (FlaggedRefreshesLockObj)
				{
					shouldEvictLocally = FlaggedRefreshes.Remove(cacheKey) == false;
				}

				if (shouldEvictLocally)
				{
					await cacheStack.EvictAsync(cacheKey);
				}
			}, CommandFlags.FireAndForget);
		}
	}
}
