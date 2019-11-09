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
		public void GetStaleDate()
		{
			var expiry = DateTime.UtcNow;
			var entry = new CacheEntry<int>(0, expiry);
			Assert.IsTrue(expiry.AddDays(-3) - entry.GetStaleDate(new CacheSettings(TimeSpan.FromDays(3))) < TimeSpan.FromSeconds(1));
			Assert.IsFalse(expiry - entry.GetStaleDate(new CacheSettings(TimeSpan.FromDays(3), TimeSpan.FromDays(2))) < TimeSpan.FromSeconds(1));
			Assert.IsTrue(expiry.AddDays(-1) - entry.GetStaleDate(new CacheSettings(TimeSpan.FromDays(3), TimeSpan.FromDays(2))) < TimeSpan.FromSeconds(1));
		}

		[TestMethod]
		public void EqualityTests_AreEqual()
		{
			var utcNow = DateTime.UtcNow;
			Assert.AreEqual(new CacheEntry<int>(0, utcNow), new CacheEntry<int>(0, utcNow));
			Assert.AreEqual(new CacheEntry<string>(string.Empty, utcNow), new CacheEntry<string>(string.Empty, utcNow));
		}

		[TestMethod]
		public void EqualityTests_AreNotEqual()
		{
			var utcNow = DateTime.UtcNow;
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow), new CacheEntry<int>(0, utcNow.AddSeconds(1)));
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow), new CacheEntry<int>(1, utcNow));

			Assert.IsFalse(new CacheEntry<int>(0, utcNow).Equals(null));
			Assert.IsFalse(new CacheEntry<int>(0, utcNow).Equals(string.Empty));
		}

		[TestMethod]
		public void EqualityTests_HashCode()
		{
			var utcNow = DateTime.UtcNow;
			Assert.AreEqual(new CacheEntry<int>(0, utcNow).GetHashCode(), new CacheEntry<int>(0, utcNow).GetHashCode());
			Assert.AreEqual(new CacheEntry<string>(string.Empty, utcNow).GetHashCode(), new CacheEntry<string>(string.Empty, utcNow).GetHashCode());
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow).GetHashCode(), new CacheEntry<int>(0, utcNow.AddSeconds(1)).GetHashCode());
			Assert.AreNotEqual(new CacheEntry<int>(0, utcNow).GetHashCode(), new CacheEntry<int>(1, utcNow).GetHashCode());
		}
	}
}
