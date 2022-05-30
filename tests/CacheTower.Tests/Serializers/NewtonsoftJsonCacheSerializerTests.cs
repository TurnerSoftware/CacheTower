using CacheTower.Serializers.NewtonsoftJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Serializers
{
	[TestClass]
	public class NewtonsoftJsonCacheSerializerTests : BaseSerializerTests<NewtonsoftJsonCacheSerializer>
	{
		[TestMethod]
		public void CacheEntrySerialization()
		{
			AssertCacheEntrySerialization(NewtonsoftJsonCacheSerializer.Instance);
		}

		[TestMethod]
		public void AssertManifestSerialization()
		{
			AssertCacheEntrySerialization(NewtonsoftJsonCacheSerializer.Instance);
		}
	}
}
