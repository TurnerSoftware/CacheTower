using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CacheTower.Extensions;
using CacheTower.Providers.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace CacheTower.Tests.Extensions
{
	[TestClass]
	public class AutoCleanupExtensionTests : TestBase
	{
		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ThrowForInvalidFrequency()
		{
			new AutoCleanupExtension(TimeSpan.Zero);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public async Task ThrowForRegisteringTwoCacheStacks()
		{
			await using var extension = new AutoCleanupExtension(TimeSpan.FromSeconds(30));
			//Will register as part of the CacheStack constructor
			await using var cacheStack = new CacheStack(null, new(new[] { new MemoryCacheLayer() }) { Extensions = new[] { extension } });
			//Force the second register manually
			extension.Register(cacheStack);
		}

		[TestMethod]
		public async Task RunsBackgroundCleanup()
		{
			await using var extension = new AutoCleanupExtension(TimeSpan.FromMilliseconds(500));
			var cacheStackMock = Substitute.For<ICacheStack>();
			extension.Register(cacheStackMock);
			await Task.Delay(TimeSpan.FromSeconds(2));
			await cacheStackMock.Received(Quantity.Within(2, int.MaxValue)).CleanupAsync();
		}

		[TestMethod]
		public async Task BackgroundCleanupObeysCancel()
		{
			await using var extension = new AutoCleanupExtension(TimeSpan.FromMilliseconds(500), new CancellationToken(true));
			var cacheStackMock = Substitute.For<ICacheStack>();
			extension.Register(cacheStackMock);
			await Task.Delay(TimeSpan.FromSeconds(2));
			await cacheStackMock.DidNotReceive().CleanupAsync();
		}
	}
}
