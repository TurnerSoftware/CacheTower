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
		public void GetStaleDate_WithStaleAfter()
		{
			var expiry = DateTime.UtcNow;
			var entry = new CacheEntry<int>(0, expiry);
			expiry = entry.Expiry;

			var staleDate = entry.GetStaleDate(new CacheSettings(TimeSpan.FromDays(3), TimeSpan.FromDays(2)));
			Assert.AreEqual(expiry.AddDays(-1), staleDate);
		}

		[TestMethod]
		public void GetStaleDate_WithoutStaleAfter()
		{
			var expiry = DateTime.UtcNow;
			var entry = new CacheEntry<int>(0, expiry);
			expiry = entry.Expiry;

			var staleDate = entry.GetStaleDate(new CacheSettings(TimeSpan.FromDays(3)));
			Assert.AreEqual(expiry, staleDate);
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
