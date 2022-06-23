using CacheTower.Serializers.SystemTextJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Serializers
{
	[TestClass]
	public class SystemTextJsonCacheSerializerTests : BaseSerializerTests<SystemTextJsonCacheSerializer>
	{
		[TestMethod]
		public void CacheEntrySerialization()
		{
			AssertCacheEntrySerialization(SystemTextJsonCacheSerializer.Instance);
		}

		[TestMethod]
		public void ManifestSerialization()
		{
			AssertManifestSerialization(SystemTextJsonCacheSerializer.Instance);
		}

		[TestMethod]
		public void CustomTypeSerialization()
		{
			AssertCustomTypeSerialization(SystemTextJsonCacheSerializer.Instance);
		}
	}
}
