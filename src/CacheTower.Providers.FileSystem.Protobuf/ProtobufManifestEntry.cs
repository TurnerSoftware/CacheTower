using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace CacheTower.Providers.FileSystem.Protobuf
{
	[ProtoContract]
	public class ProtobufManifestEntry : IManifestEntry
	{
		[ProtoMember(1)]
		public string FileName { get; set; }
		[ProtoMember(2)]
		public DateTime Expiry { get; set; }
	}
}
