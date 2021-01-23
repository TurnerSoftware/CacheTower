using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CacheTower.Extensions;
using CacheTower.Providers.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
			using (var extension = new AutoCleanupExtension(TimeSpan.FromSeconds(30)))
			{
				//Will register as part of the CacheStack constructor
				await using var cacheStack = new CacheStack(new[] { new MemoryCacheLayer() }, new[] { extension });
				//Force the second register manually
				extension.Register(cacheStack);
			}
		}

		[TestMethod]
		public async Task RunsBackgroundCleanup()
		{
			using (var extension = new AutoCleanupExtension(TimeSpan.FromMilliseconds(500)))
			{
				var cacheStackMock = new Mock<ICacheStack>();
				extension.Register(cacheStackMock.Object);
				await Task.Delay(TimeSpan.FromSeconds(2));
				cacheStackMock.Verify(c => c.CleanupAsync(), Times.AtLeast(2));
			}
		}

		[TestMethod]
		public async Task BackgroundCleanupObeysCancel()
		{
			using (var extension = new AutoCleanupExtension(TimeSpan.FromMilliseconds(500), new CancellationToken(true)))
			{
				var cacheStackMock = new Mock<ICacheStack>();
				extension.Register(cacheStackMock.Object);
				await Task.Delay(TimeSpan.FromSeconds(2));
				cacheStackMock.Verify(c => c.CleanupAsync(), Times.Never);
			}
		}
	}
}
