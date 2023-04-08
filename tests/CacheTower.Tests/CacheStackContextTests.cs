using CacheTower.Providers.Memory;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests
{
	[TestClass]
	public class CacheStackContextTests : TestBase
	{
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullKey()
		{
			await using var cacheStack = new CacheStack<object>(null, new FuncCacheContextActivator<object>(() => null), new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetOrSetAsync<int>(null, (old, context) => Task.FromResult(5), new CacheSettings(TimeSpan.FromDays(1)));
		}
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetOrSet_ThrowsOnNullGetter()
		{
			await using var cacheStack = new CacheStack<object>(null, new FuncCacheContextActivator<object>(() => null), new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
			await cacheStack.GetOrSetAsync<int>("MyCacheKey", null, new CacheSettings(TimeSpan.FromDays(1)));
		}
		[TestMethod]
		public async Task GetOrSet_CacheMiss_ContextHasValue()
		{
			await using var cacheStack = new CacheStack<int>(null, new FuncCacheContextActivator<object>(() => 123), new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());
   			var result = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheMiss_ContextHasValue", (oldValue, context) =>
			{
				Assert.AreEqual(123, context);
				return Task.FromResult(5);
			}, new CacheSettings(TimeSpan.FromDays(1)));

			Assert.AreEqual(5, result);
		}
		[TestMethod]
		public async Task GetOrSet_CacheMiss_ContextFactoryCalledEachTime()
		{
			var contextValue = 0;
			await using var cacheStack = new CacheStack<int>(null, new FuncCacheContextActivator<object>(() => contextValue++), new[] { new MemoryCacheLayer() }, Array.Empty<ICacheExtension>());

			var result1 = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheMiss_ContextFactoryCalledEachTime_1", (oldValue, context) =>
			{
				Assert.AreEqual(0, context);
				return Task.FromResult(5);
			}, new CacheSettings(TimeSpan.FromDays(1)));
			Assert.AreEqual(5, result1);

			var result2 = await cacheStack.GetOrSetAsync<int>("GetOrSet_CacheMiss_ContextFactoryCalledEachTime_2", (oldValue, context) =>
			{
				Assert.AreEqual(1, context);
				return Task.FromResult(5);
			}, new CacheSettings(TimeSpan.FromDays(1)));
			Assert.AreEqual(5, result2);
		}
	}
}
