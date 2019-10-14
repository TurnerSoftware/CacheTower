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

		protected override async Task<T> DeserializeAsync<T>(Stream stream)
		{
			using (var memStream = new MemoryStream((int)stream.Length))
			{
				await stream.CopyToAsync(memStream);
				memStream.Seek(0, SeekOrigin.Begin);
				return Serializer.Deserialize<T>(memStream);
			}
		}

		protected override async Task SerializeAsync<T>(Stream stream, T value)
		{
			using (var memStream = new MemoryStream())
			{
				Serializer.Serialize(memStream, value);
				memStream.Seek(0, SeekOrigin.Begin);
				await memStream.CopyToAsync(stream);
			}
		}
	}
}
