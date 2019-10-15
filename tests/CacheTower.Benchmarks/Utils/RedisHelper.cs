using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Benchmarks.Utils
{
	public static class RedisHelper
	{
		public static string Endpoint => Environment.GetEnvironmentVariable("REDIS_ENDPOINT") ?? "localhost:6379";

		private static ConnectionMultiplexer Connection { get; set; }

		public static ConnectionMultiplexer GetConnection()
		{
			if (Connection == null)
			{
				var config = new ConfigurationOptions
				{
					AllowAdmin = true
				};
				config.EndPoints.Add(Endpoint);
				Connection = ConnectionMultiplexer.Connect(config);
			}
			return Connection;
		}

		public static void FlushDatabase()
		{
			GetConnection().GetServer(Endpoint).FlushDatabase();
		}
	}
}
