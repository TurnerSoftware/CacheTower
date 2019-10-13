using CacheTower.Providers.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CacheTower.Tests.Providers.Memory
{
	[TestClass]
	public class MemoryCacheLayerTests : BaseCacheLayerTests
	{
		[TestMethod]
		public async Task GetSetCache()
		{
			await AssertGetSetCache(new MemoryCacheLayer());
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await AssertCacheAvailability(new MemoryCacheLayer(), true);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEviction(new MemoryCacheLayer());
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await AssertCacheCleanup(new MemoryCacheLayer());
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			await AssertComplexTypeCaching(new MemoryCacheLayer());
		}
	}
}
