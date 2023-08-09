using System;
using System.Threading.Tasks;
using CacheTower.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace CacheTower.Tests.Extensions
{
	[TestClass]
	public class ExtensionContainerTests : TestBase
	{
		[TestMethod]
		public async Task AcceptsNullExtensions()
		{
			await using var container = new ExtensionContainer(null);
		}

		[TestMethod]
		public async Task AcceptsEmptyExtensions()
		{
			await using var container = new ExtensionContainer(Array.Empty<ICacheExtension>());
		}
		
		[TestMethod]
		public async Task DistributedLockExtension()
		{
			var cacheStackMock = Substitute.For<ICacheStack>();
			var distributedLockMock = Substitute.For<IDistributedLockExtension>();
			await using var container = new ExtensionContainer(new[] { distributedLockMock });

			container.Register(cacheStackMock);

			var distributedLock = await container.AwaitAccessAsync("DistributedLockCacheKey");

			distributedLockMock.Received(1).Register(cacheStackMock);
			await distributedLockMock.Received(1).AwaitAccessAsync("DistributedLockCacheKey");
		}

		[TestMethod]
		public async Task CacheChangeExtension_Update()
		{
			var cacheStackMock = Substitute.For<ICacheStack>();
			var valueRefreshMock = Substitute.For<ICacheChangeExtension>();
			await using var container = new ExtensionContainer(new[] { valueRefreshMock });

			container.Register(cacheStackMock);

			var expiry = DateTime.UtcNow.AddDays(1);

			await container.OnCacheUpdateAsync("CacheChangeKey", expiry, CacheUpdateType.AddEntry);

			valueRefreshMock.Received(1).Register(cacheStackMock);
			await valueRefreshMock.Received(1).OnCacheUpdateAsync("CacheChangeKey", expiry, CacheUpdateType.AddEntry);
		}

		[TestMethod]
		public async Task CacheChangeExtension_Eviction()
		{
			var cacheStackMock = Substitute.For<ICacheStack>();
			var valueRefreshMock = Substitute.For<ICacheChangeExtension>();
			await using var container = new ExtensionContainer(new[] { valueRefreshMock });

			container.Register(cacheStackMock);

			var expiry = DateTime.UtcNow.AddDays(1);

			await container.OnCacheEvictionAsync("CacheChangeKey");

			valueRefreshMock.Received(1).Register(cacheStackMock);
			await valueRefreshMock.Received(1).OnCacheEvictionAsync("CacheChangeKey");
		}

		[TestMethod]
		public async Task CacheChangeExtension_Flush()
		{
			var cacheStackMock = Substitute.For<ICacheStack>();
			var valueRefreshMock = Substitute.For<ICacheChangeExtension>();
			await using var container = new ExtensionContainer(new[] { valueRefreshMock });

			container.Register(cacheStackMock);

			var expiry = DateTime.UtcNow.AddDays(1);

			await container.OnCacheFlushAsync();

			valueRefreshMock.Received(1).Register(cacheStackMock);
			await valueRefreshMock.Received(1).OnCacheFlushAsync();
		}
	}
}
