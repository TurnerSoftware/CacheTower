using CacheTower.Serializers.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Serializers
{
	[TestClass]
	public class ProtobufCacheSerializerTests : BaseSerializerTests<ProtobufCacheSerializer>
	{
		[TestMethod]
		public void CacheEntrySerialization()
		{
			AssertCacheEntrySerialization(ProtobufCacheSerializer.Instance);
		}

		[TestMethod]
		public void AssertManifestSerialization()
		{
			AssertCacheEntrySerialization(ProtobufCacheSerializer.Instance);
		}
	}
}
