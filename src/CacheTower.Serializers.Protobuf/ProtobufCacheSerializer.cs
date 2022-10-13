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
			RuntimeTypeModel.Default.Add<ManifestEntry>(applyDefaultBehaviour: false)
				.Add(1, nameof(ManifestEntry.FileName))
				.Add(2, nameof(ManifestEntry.Expiry));
		}

		/// <summary>
		/// An existing instance of <see cref="ProtobufCacheSerializer"/>.
		/// </summary>
		public static ProtobufCacheSerializer Instance { get; } = new();

		//Because we can't use an open generic for protobuf-net (see https://github.com/protobuf-net/protobuf-net/issues/802)
		//we instead use/abuse a static class with a generic parameter to dynamically create the serialization config for us.
		private static class SerializerConfig<T>
		{
			static SerializerConfig()
			{
				if (typeof(ICacheEntry).IsAssignableFrom(typeof(T)))
				{
					RuntimeTypeModel.Default.Add(typeof(T), applyDefaultBehaviour: false)
						.Add(1, nameof(CacheEntry<object>.Expiry))
						.Add(2, nameof(CacheEntry<object>.Value));
				}
			}

			//This method is only here to make the action more explicit
			public static void EnsureConfigured() { }
		}

		/// <inheritdoc />
		public void Serialize<T>(Stream stream, T? value)
		{
			SerializerConfig<T>.EnsureConfigured();
			Serializer.Serialize(stream, value);
		}

		/// <inheritdoc />
		public T? Deserialize<T>(Stream stream)
		{
			SerializerConfig<T>.EnsureConfigured();
			return Serializer.Deserialize<T>(stream);
		}
	}
}
