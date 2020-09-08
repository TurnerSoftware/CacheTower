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
			var container = new ExtensionContainer(null);
			await DisposeOf(container);
		}

		[TestMethod]
		public async Task AcceptsEmptyExtensions()
		{
			var container = new ExtensionContainer(Array.Empty<ICacheExtension>());
			await DisposeOf(container);
		}
		
		[TestMethod]
		public async Task RefreshWrapperExtension()
		{
			var cacheStackMock = new Mock<ICacheStack>();
			var refreshWrapperMock = new Mock<IRefreshWrapperExtension>();
			var container = new ExtensionContainer(new[] { refreshWrapperMock.Object });

			container.Register(cacheStackMock.Object);

			var cacheEntry = new CacheEntry<int>(1, TimeSpan.FromDays(1));

			var refreshedValue = await container.RefreshValueAsync("WrapperTestCacheKey", () =>
			{
				return new ValueTask<CacheEntry<int>>(cacheEntry);
			}, new CacheEntryLifetime(TimeSpan.FromDays(1)));

			refreshWrapperMock.Verify(e => e.Register(cacheStackMock.Object), Times.Once);
			refreshWrapperMock.Verify(e => e.RefreshValueAsync(
					"WrapperTestCacheKey",
					It.IsAny<Func<ValueTask<CacheEntry<int>>>>(), new CacheEntryLifetime(TimeSpan.FromDays(1))
				),
				Times.Once
			);

			await DisposeOf(container);
		}

		[TestMethod]
		public async Task ValueRefreshExtension()
		{
			var cacheStackMock = new Mock<ICacheStack>();
			var valueRefreshMock = new Mock<IValueRefreshExtension>();
			var container = new ExtensionContainer(new[] { valueRefreshMock.Object });

			container.Register(cacheStackMock.Object);

			await container.OnValueRefreshAsync("ValueRefreshTestCacheKey", TimeSpan.FromDays(1));

			valueRefreshMock.Verify(e => e.Register(cacheStackMock.Object), Times.Once);
			valueRefreshMock.Verify(e => 
				e.OnValueRefreshAsync("ValueRefreshTestCacheKey", TimeSpan.FromDays(1)),
				Times.Once
			);

			await DisposeOf(container);
		}
	}
}
