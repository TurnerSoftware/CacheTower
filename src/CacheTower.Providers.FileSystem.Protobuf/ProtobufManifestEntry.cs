using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace CacheTower.Providers.FileSystem.Protobuf
{
	/// <summary>
	/// The manifest entry type used by <see cref="ProtobufFileCacheLayer"/>.
	/// </summary>
	[ProtoContract]
	public class ProtobufManifestEntry : IManifestEntry
	{
		/// <inheritdoc/>
		[ProtoMember(1)]
		public string FileName { get; set; }
		/// <inheritdoc/>
		[ProtoMember(2)]
		public DateTime Expiry { get; set; }
	}
}
