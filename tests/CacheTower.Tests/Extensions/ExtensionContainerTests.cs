using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheTower.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
		public async Task RefreshCallSiteWrapperExtension()
		{
			var cacheStackMock = new Mock<ICacheStack>();
			var refreshWrapperMock = new Mock<ICacheRefreshCallSiteWrapperExtension>();
			await using var container = new ExtensionContainer(new[] { refreshWrapperMock.Object });

			container.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(1, TimeSpan.FromDays(1));

			var refreshedValue = await container.WithRefreshAsync("WrapperTestCacheKey", () =>
			{
				return new ValueTask<CacheEntry<int>>(cacheEntry);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			refreshWrapperMock.Verify(e => e.Register(cacheStackMock.Object), Times.Once);
			refreshWrapperMock.Verify(e => e.WithRefreshAsync(
					"WrapperTestCacheKey",
					It.IsAny<Func<ValueTask<CacheEntry<int>>>>(), new CacheSettings(TimeSpan.FromDays(1))
				),
				Times.Once
			);
		}

		[TestMethod]
		public async Task CacheChangeExtension_Update()
		{
			var cacheStackMock = new Mock<ICacheStack>();
			var valueRefreshMock = new Mock<ICacheChangeExtension>();
			await using var container = new ExtensionContainer(new[] { valueRefreshMock.Object });

			container.Register(cacheStackMock.Object);

			var expiry = DateTime.UtcNow.AddDays(1);

			await container.OnCacheUpdateAsync("CacheChangeKey", expiry, CacheUpdateType.AddEntry);

			valueRefreshMock.Verify(e => e.Register(cacheStackMock.Object), Times.Once);
			valueRefreshMock.Verify(e =>
				e.OnCacheUpdateAsync("CacheChangeKey", expiry, CacheUpdateType.AddEntry),
				Times.Once
			);
		}

		[TestMethod]
		public async Task CacheChangeExtension_Eviction()
		{
			var cacheStackMock = new Mock<ICacheStack>();
			var valueRefreshMock = new Mock<ICacheChangeExtension>();
			await using var container = new ExtensionContainer(new[] { valueRefreshMock.Object });

			container.Register(cacheStackMock.Object);

			var expiry = DateTime.UtcNow.AddDays(1);

			await container.OnCacheEvictionAsync("CacheChangeKey");

			valueRefreshMock.Verify(e => e.Register(cacheStackMock.Object), Times.Once);
			valueRefreshMock.Verify(e =>
				e.OnCacheEvictionAsync("CacheChangeKey"),
				Times.Once
			);
		}

		[TestMethod]
		public async Task CacheChangeExtension_Flush()
		{
			var cacheStackMock = new Mock<ICacheStack>();
			var valueRefreshMock = new Mock<ICacheChangeExtension>();
			await using var container = new ExtensionContainer(new[] { valueRefreshMock.Object });

			container.Register(cacheStackMock.Object);

			var expiry = DateTime.UtcNow.AddDays(1);

			await container.OnCacheFlushAsync();

			valueRefreshMock.Verify(e => e.Register(cacheStackMock.Object), Times.Once);
			valueRefreshMock.Verify(e =>
				e.OnCacheFlushAsync(),
				Times.Once
			);
		}
	}
}
