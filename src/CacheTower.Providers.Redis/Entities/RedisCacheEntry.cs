using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace CacheTower.Providers.Redis.Entities
{
	[ProtoContract]
	public class RedisCacheEntry<T>
	{
		[ProtoMember(1)]
		public DateTime CachedAt { get; set; }
		[ProtoMember(2)]
		public TimeSpan TimeToLive { get; set; }
		[ProtoMember(3)]
		public T Value { get; set; }
	}
}
