using System;
using CacheTower.Extensions;
using CacheTower.Extensions.Redis;
using CacheTower.Providers.Database.MongoDB;
using CacheTower.Providers.FileSystem;
using CacheTower.Providers.Memory;
using CacheTower.Providers.Redis;
using CacheTower.Serializers.NewtonsoftJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CacheTower.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
	[TestMethod]
	public void CacheStack_CacheStackBuilder()
	{
		var serviceCollectionMock = new Mock<IServiceCollection>();
		serviceCollectionMock.Setup(s => s.Add(It.IsAny<ServiceDescriptor>()))
			.Callback<ServiceDescriptor>(sd =>
			{
				Assert.AreEqual(ServiceLifetime.Singleton, sd.Lifetime);
				var result = (IExtendableCacheStack)sd.ImplementationFactory(null);
				var cacheLayers = result.GetCacheLayers();
				Assert.AreEqual(1, cacheLayers.Count);
				Assert.AreEqual(typeof(MemoryCacheLayer), cacheLayers[0].GetType());
			}).Verifiable();

		var hasBuilderBeenCalled = false;
		serviceCollectionMock.Object.AddCacheStack(builder =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});

		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");

		serviceCollectionMock.Verify();
	}

	[TestMethod]
	public void GenericCacheStack_CacheStackBuilder()
	{
		var serviceCollectionMock = new Mock<IServiceCollection>();
		serviceCollectionMock.Setup(s => s.Add(It.IsAny<ServiceDescriptor>()))
			.Callback<ServiceDescriptor>(sd =>
			{
				Assert.AreEqual(ServiceLifetime.Singleton, sd.Lifetime);
				var result = (IExtendableCacheStack)sd.ImplementationFactory(null);
				var cacheLayers = result.GetCacheLayers();
				Assert.AreEqual(1, cacheLayers.Count);
				Assert.AreEqual(typeof(MemoryCacheLayer), cacheLayers[0].GetType());
				Assert.IsTrue(result is ICacheStack<int>);
			}).Verifiable();

		var hasBuilderBeenCalled = false;
		serviceCollectionMock.Object.AddCacheStack<int>(builder =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});

		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");

		serviceCollectionMock.Verify();
	}

	[TestMethod]
	public void GenericCacheStack_CacheStackBuilder_CustomCacheContextActivator()
	{
		var serviceCollectionMock = new Mock<IServiceCollection>();
		serviceCollectionMock.Setup(s => s.Add(It.IsAny<ServiceDescriptor>()))
			.Callback<ServiceDescriptor>(sd =>
			{
				Assert.AreEqual(ServiceLifetime.Singleton, sd.Lifetime);
				var result = (IExtendableCacheStack)sd.ImplementationFactory(null);
				var cacheLayers = result.GetCacheLayers();
				Assert.AreEqual(1, cacheLayers.Count);
				Assert.AreEqual(typeof(MemoryCacheLayer), cacheLayers[0].GetType());
				Assert.IsTrue(result is ICacheStack<int>);
			}).Verifiable();

		var contextActivatorMock = new Mock<ICacheContextActivator>();

		var hasBuilderBeenCalled = false;
		serviceCollectionMock.Object.AddCacheStack<int>(contextActivatorMock.Object, builder =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});

		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");

		serviceCollectionMock.Verify();
	}

	[TestMethod]
	public void CacheStackBuilder_AddCacheLayers()
	{
		var cacheStackBuilder = new CacheStackBuilder();
		var mongoDbConnectionMock = new Mock<MongoFramework.IMongoDbConnection>();
		var redisConnectionMock = new Mock<StackExchange.Redis.IConnectionMultiplexer>();

		cacheStackBuilder
			.AddMemoryCacheLayer()
			.AddFileCacheLayer(new FileCacheLayerOptions("./FileCacheLayer", NewtonsoftJsonCacheSerializer.Instance))
			.AddMongoDbCacheLayer(mongoDbConnectionMock.Object)
			.AddRedisCacheLayer(redisConnectionMock.Object, new RedisCacheLayerOptions(NewtonsoftJsonCacheSerializer.Instance));

		Assert.AreEqual(4, cacheStackBuilder.CacheLayers.Count);
		Assert.IsInstanceOfType(cacheStackBuilder.CacheLayers[0], typeof(MemoryCacheLayer));
		Assert.IsInstanceOfType(cacheStackBuilder.CacheLayers[1], typeof(FileCacheLayer));
		Assert.IsInstanceOfType(cacheStackBuilder.CacheLayers[2], typeof(MongoDbCacheLayer));
		Assert.IsInstanceOfType(cacheStackBuilder.CacheLayers[3], typeof(RedisCacheLayer));
	}

	[TestMethod]
	public void CacheStackBuilder_WithExtensions()
	{
		var cacheStackBuilder = new CacheStackBuilder();
		var redisConnectionMock = new Mock<StackExchange.Redis.IConnectionMultiplexer>();
		redisConnectionMock.Setup(r => r.GetSubscriber(It.IsAny<object>()))
			.Returns(new Mock<StackExchange.Redis.ISubscriber>().Object);

		cacheStackBuilder
			.WithCleanupFrequency(TimeSpan.FromSeconds(5))
			.WithRedisDistributedLocking(redisConnectionMock.Object)
			.WithRedisRemoteEviction(redisConnectionMock.Object);

		Assert.AreEqual(3, cacheStackBuilder.Extensions.Count);
		Assert.IsInstanceOfType(cacheStackBuilder.Extensions[0], typeof(AutoCleanupExtension));
		Assert.IsInstanceOfType(cacheStackBuilder.Extensions[1], typeof(RedisLockExtension));
		Assert.IsInstanceOfType(cacheStackBuilder.Extensions[2], typeof(RedisRemoteEvictionExtension));
	}
}
