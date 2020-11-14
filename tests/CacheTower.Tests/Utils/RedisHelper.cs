using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheTower.Tests.Utils
{
	public static class RedisHelper
	{
		public static string Endpoint => Environment.GetEnvironmentVariable("REDIS_ENDPOINT") ?? "localhost:6379";

		public static ConnectionMultiplexer GetConnection()
		{
			var config = new ConfigurationOptions
			{
				AllowAdmin = true
			};
			config.EndPoints.Add(Endpoint);
			return ConnectionMultiplexer.Connect(config);
		}

		public static void FlushDatabase()
		{
			GetConnection().GetServer(Endpoint).FlushDatabase();
		}
	}
}
