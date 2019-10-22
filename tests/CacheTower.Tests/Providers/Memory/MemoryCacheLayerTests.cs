using CacheTower.Providers.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CacheTower.Tests.Providers.Memory
{
	[TestClass]
	public class MemoryCacheLayerTests : BaseCacheLayerTests
	{
		[TestMethod]
		public void GetSetCache()
		{
			AssertGetSetCache(new MemoryCacheLayer());
		}

		[TestMethod]
		public void IsCacheAvailable()
		{
			AssertCacheAvailability(new MemoryCacheLayer(), true);
		}

		[TestMethod]
		public void EvictFromCache()
		{
			AssertCacheEviction(new MemoryCacheLayer());
		}

		[TestMethod]
		public void CacheCleanup()
		{
			AssertCacheCleanup(new MemoryCacheLayer());
		}

		[TestMethod]
		public void CachingComplexTypes()
		{
			AssertComplexTypeCaching(new MemoryCacheLayer());
		}
	}
}
