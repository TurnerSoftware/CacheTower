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
			await AssertGetSetCacheAsync(new MemoryCacheLayer());
		}

		[TestMethod]
		public async Task IsCacheAvailable()
		{
			await AssertCacheAvailabilityAsync(new MemoryCacheLayer(), true);
		}

		[TestMethod]
		public async Task EvictFromCache()
		{
			await AssertCacheEvictionAsync(new MemoryCacheLayer());
		}

		[TestMethod]
		public async Task FlushFromCache()
		{
			await AssertCacheFlushAsync(new MemoryCacheLayer());
		}

		[TestMethod]
		public async Task CacheCleanup()
		{
			await AssertCacheCleanupAsync(new MemoryCacheLayer());
		}

		[TestMethod]
		public async Task CachingComplexTypes()
		{
			await AssertComplexTypeCachingAsync(new MemoryCacheLayer());
		}
	}
}
