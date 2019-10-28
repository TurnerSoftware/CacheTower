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
		public DateTime Expiry { get; set; }
		[ProtoMember(2)]
		public T Value { get; set; }
	}
}
