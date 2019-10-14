using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests
{
	[TestClass]
	public class CacheEntryTests
	{
		[TestMethod]
		public void HasElapsed_NoOffset()
		{
			var entryInPast = new CacheEntry<int>(0, DateTime.UtcNow.AddDays(-1), TimeSpan.Zero);
			Assert.IsTrue(entryInPast.HasElapsed(TimeSpan.Zero));

			var entryInFuture = new CacheEntry<int>(0, DateTime.UtcNow.AddDays(1), TimeSpan.Zero);
			Assert.IsFalse(entryInFuture.HasElapsed(TimeSpan.Zero));
		}

		[TestMethod]
		public void HasElapsed_WithOffset()
		{
			var entryInPast1 = new CacheEntry<int>(0, DateTime.UtcNow.AddDays(-2), TimeSpan.FromDays(1));
			Assert.IsTrue(entryInPast1.HasElapsed(TimeSpan.FromDays(1)));

			var entryInPast2 = new CacheEntry<int>(0, DateTime.UtcNow.AddDays(-2), TimeSpan.FromDays(1));
			Assert.IsFalse(entryInPast2.HasElapsed(TimeSpan.FromDays(3)));
		}

		[TestMethod]
		public void EqualityTests_AreEqual()
		{
			var utcNow = DateTime.UtcNow;
			Assert.AreEqual(new CacheEntry<int>(0, utcNow, TimeSpan.Zero), new CacheEntry<int>(0, utcNow, TimeSpan.Zero));
			Assert.AreEqual(new CacheEntry<string>(string.Empty, utcNow, TimeSpan.Zero), new CacheEntry<string>(string.Empty, utcNow, TimeSpan.Zero));
		}

		[TestMethod]
		public void EqualityTests_AreNotEqual()
		{
			var utcNow = DateTime.UtcNow;
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow, TimeSpan.Zero), new CacheEntry<int>(0, utcNow, TimeSpan.FromSeconds(1)));
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow, TimeSpan.Zero), new CacheEntry<int>(0, utcNow.AddDays(1), TimeSpan.Zero));
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow, TimeSpan.Zero), new CacheEntry<int>(1, utcNow, TimeSpan.Zero));

			Assert.IsFalse(new CacheEntry<int>(0, utcNow, TimeSpan.Zero).Equals(null));
			Assert.IsFalse(new CacheEntry<int>(0, utcNow, TimeSpan.Zero).Equals(string.Empty));
		}

		[TestMethod]
		public void EqualityTests_HashCode()
		{
			var utcNow = DateTime.UtcNow;
			Assert.AreEqual(new CacheEntry<int>(0, utcNow, TimeSpan.Zero).GetHashCode(), new CacheEntry<int>(0, utcNow, TimeSpan.Zero).GetHashCode());
			Assert.AreEqual(new CacheEntry<string>(string.Empty, utcNow, TimeSpan.Zero).GetHashCode(), new CacheEntry<string>(string.Empty, utcNow, TimeSpan.Zero).GetHashCode());
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow, TimeSpan.Zero).GetHashCode(), new CacheEntry<int>(0, utcNow, TimeSpan.FromSeconds(1)).GetHashCode());
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow, TimeSpan.Zero).GetHashCode(), new CacheEntry<int>(0, utcNow.AddDays(1), TimeSpan.Zero).GetHashCode());
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow, TimeSpan.Zero).GetHashCode(), new CacheEntry<int>(1, utcNow, TimeSpan.Zero).GetHashCode());
		}
	}
}
