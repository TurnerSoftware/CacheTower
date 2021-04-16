using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace CacheTower.Providers.Redis.Entities
{
	/// <summary>
	/// Wrapper class holding the cache expiry and cache value for storing in Redis.
	/// </summary>
	/// <typeparam name="T">The type of the cached value</typeparam>
	[ProtoContract]
	[DataContract]
	public class RedisCacheEntry<T>
	{
		/// <summary>
		/// The expiry date of the cache entry.
		/// </summary>
		[ProtoMember(1)]
		[DataMember(Name = "expiry")]
		public DateTime Expiry { get; set; }
		/// <summary>
		/// The cached value itself.
		/// </summary>
		[ProtoMember(2)]
		[DataMember(Name = "value")]
		public T? Value { get; set; }
	}
}
