using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using StackExchange.Redis;

namespace CacheTower.Tests.Utils
{
	public static class RedisHelper
	{
		public static string Endpoint => Environment.GetEnvironmentVariable("REDIS_ENDPOINT") ?? "localhost:6379";

		private static readonly ConcurrentQueue<string> Errors = new();

		static RedisHelper()
		{
			var connection = GetConnection();
			connection.ErrorMessage += (sender, args) =>
			{
				Errors.Enqueue(args.Message);
			};
			connection.InternalError += (sender, args) =>
			{
				if (args.Exception is not null)
				{
					Errors.Enqueue(args.Exception.Message);
				}
			};
		}

		public static ConnectionMultiplexer GetConnection()
		{
			var config = new ConfigurationOptions
			{
				AllowAdmin = true
			};
			config.EndPoints.Add(Endpoint);
			return ConnectionMultiplexer.Connect(config);
		}

		/// <summary>
		/// Flushes Redis and resets the state of error logging
		/// </summary>
		public static void ResetState()
		{
			GetConnection().GetServer(Endpoint).FlushDatabase();
			
			//.NET Framework doesn't support `Clear()` on Errors so we do it manually
			while (!Errors.IsEmpty)
			{
				Errors.TryDequeue(out _);
			}
		}

		public static void DebugInfo(IConnectionMultiplexer connection)
		{
			Debug.WriteLine("== Redis Connection Status ==");
			Debug.WriteLine(connection.GetStatus());

			Debug.WriteLine("== Errors (Redis and Internal) ==");
			while (!Errors.IsEmpty)
			{
				if (Errors.TryDequeue(out var message))
				{
					Debug.WriteLine(message);
				}
			}
		}
	}
}
