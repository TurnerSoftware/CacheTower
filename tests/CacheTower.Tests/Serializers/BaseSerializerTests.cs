using System;
using System.Collections.Generic;
using System.IO;
using CacheTower.Providers.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests.Serializers
{
	public abstract class BaseSerializerTests<TSerializer> : TestBase
		where TSerializer : ICacheSerializer
	{
		private static void AssertRoundtripSerialization<TValue>(TSerializer serializer, TValue value)
			where TValue : IEquatable<TValue>
		{
			using var memoryStream = new MemoryStream();
			serializer.Serialize(memoryStream, value);
			memoryStream.Seek(0, SeekOrigin.Begin);
			var result = serializer.Deserialize<TValue>(memoryStream);
			Assert.AreEqual(value, result);
		}

		protected static void AssertCacheEntrySerialization(TSerializer serializer)
		{
			var cacheEntry = new CacheEntry<ComplexTypeCaching_TypeOne>(new ComplexTypeCaching_TypeOne
			{
				ExampleString = "Hello World",
				ExampleNumber = 99,
				ListOfNumbers = new List<int>() { 1, 2, 4, 8 }
			}, TimeSpan.FromDays(1));
			AssertRoundtripSerialization(serializer, cacheEntry);
		}

		protected static void AssertManifestSerialization(TSerializer serializer)
		{
			var manifest = new ManifestEntry
			{
				Expiry = new DateTime(2022, 5, 30),
				FileName = "CacheEntryFileName"
			};
			AssertRoundtripSerialization(serializer, manifest);
		}
	}
}
