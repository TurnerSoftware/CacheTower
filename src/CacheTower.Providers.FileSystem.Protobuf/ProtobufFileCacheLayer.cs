using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using ProtoBuf.Meta;

namespace CacheTower.Providers.FileSystem.Protobuf
{
	public class ProtobufFileCacheLayer : FileCacheLayerBase<ProtobufManifestEntry>
	{
		private static readonly object RuntimeTypeLock = new object();

		public ProtobufFileCacheLayer(string directoryPath) : base(directoryPath, ".bin")
		{
			lock (RuntimeTypeLock)
			{
				if (!RuntimeTypeModel.Default.IsDefined(typeof(IManifestEntry)))
				{
					RuntimeTypeModel.Default.Add(typeof(IManifestEntry))
						.AddSubType(1, typeof(ProtobufManifestEntry));
				}
			}
		}

		protected override T Deserialize<T>(Stream stream)
		{
			return Serializer.Deserialize<T>(stream);
		}

		protected override void Serialize<T>(Stream stream, T value)
		{
			Serializer.Serialize(stream, value);
		}
	}
}
