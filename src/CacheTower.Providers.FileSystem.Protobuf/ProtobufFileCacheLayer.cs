using System;
using CacheTower.Serializers.Protobuf;

namespace CacheTower.Providers.FileSystem.Protobuf
{
	/// <remarks>
	/// The <see cref="ProtobufFileCacheLayer"/> uses <a href="https://github.com/protobuf-net/protobuf-net">protobuf-net</a> to serialize and deserialize the cache items to the file system.
	/// <para>
	/// When caching custom types, you will need to <a href="https://github.com/protobuf-net/protobuf-net#1-first-decorate-your-classes">decorate your class</a> with <c>[ProtoContact]</c> and <c>[ProtoMember]</c> attributes per protobuf-net's documentation.
	/// While this can be inconvienent, using protobuf-net ensures high performance and low allocations for serializing.
	/// </para>
	/// </remarks>
	/// <inheritdoc/>
	[Obsolete("Use FileCacheLayer directly and specify the ProtobufCacheSerializer. This cache layer (and the associated package) will be discontinued in a future release.")]
	public class ProtobufFileCacheLayer : FileCacheLayer
	{
		/// <summary>
		/// Creates a <see cref="ProtobufFileCacheLayer"/>, using the given <paramref name="directoryPath"/> as the location to store the cache.
		/// </summary>
		/// <param name="directoryPath"></param>
		public ProtobufFileCacheLayer(string directoryPath) : base(new ProtobufCacheSerializer(), directoryPath, ".bin") { }
	}
}
