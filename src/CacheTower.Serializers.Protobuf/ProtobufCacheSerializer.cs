using System.IO;
using CacheTower.Providers.FileSystem;
using ProtoBuf;
using ProtoBuf.Meta;

namespace CacheTower.Serializers.Protobuf
{
	/// <summary>
	/// Allows serializing to and from Protobuf format via protobuf-net.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When caching custom types, you will need to <a href="https://github.com/protobuf-net/protobuf-net#1-first-decorate-your-classes">decorate your class</a> with <c>[ProtoContact]</c> and <c>[ProtoMember]</c> attributes per protobuf-net's documentation.<br/>
	/// Additionally, as the Protobuf format doesn't have a way to represent an empty collection, these will be returned as <c>null</c>.
	/// </para>
	/// </remarks>
	public class ProtobufCacheSerializer : ICacheSerializer
	{
		static ProtobufCacheSerializer()
		{
			RuntimeTypeModel.Default.Add<ManifestEntry>()
				.Add(1, nameof(ManifestEntry.FileName))
				.Add(2, nameof(ManifestEntry.Expiry));

			//TODO: This doesn't work! See https://github.com/protobuf-net/protobuf-net/issues/802
			RuntimeTypeModel.Default.Add(typeof(CacheEntry<>))
				.Add(1, nameof(CacheEntry<object>.Expiry))
				.Add(2, nameof(CacheEntry<object>.Value));
		}

		/// <inheritdoc />
		public void Serialize<T>(Stream stream, T cacheEntry)
		{
			Serializer.Serialize(stream, cacheEntry);
		}

		/// <inheritdoc />
		public T Deserialize<T>(Stream stream)
		{
			return Serializer.Deserialize<T>(stream);
		}
	}
}
