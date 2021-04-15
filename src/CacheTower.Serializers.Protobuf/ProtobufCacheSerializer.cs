using System.IO;
using ProtoBuf;

namespace CacheTower.Serializers.Protobuf
{
	/// <inheritdoc />
	public class ProtobufCacheSerializer : ICacheSerializer
	{
		/// <inheritdoc />
		public MemoryStream Serialize<T>(T cacheEntry)
		{
			var stream = new MemoryStream();
			Serializer.Serialize(stream, cacheEntry);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		/// <inheritdoc />
		public T Deserialize<T>(MemoryStream stream)
		{
			return Serializer.Deserialize<T>(stream);
		}
	}
}
