﻿using System;
using CacheTower.Extensions;
using CacheTower.Extensions.Redis;
using CacheTower.Providers.Database.MongoDB;
using CacheTower.Providers.FileSystem;
using CacheTower.Providers.Memory;
using CacheTower.Providers.Redis;
using CacheTower.Serializers.NewtonsoftJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace CacheTower.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
	[TestMethod]
	public void CacheStack_InvalidCacheStackBuilder()
	{
		var serviceCollection = new ServiceCollection();

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack(builder =>
		{
			hasBuilderBeenCalled = true;
		});
		var serviceProvider = serviceCollection.BuildServiceProvider();
		
		Assert.ThrowsException<ArgumentException>(() => serviceProvider.GetRequiredService<ICacheStack>());
		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");
	}

	[TestMethod]
	public void CacheStack_CacheStackBuilder()
	{
		var serviceCollection = new ServiceCollection();

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack(builder =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});
		var serviceProvider = serviceCollection.BuildServiceProvider();

		var result = (IExtendableCacheStack)serviceProvider.GetRequiredService<ICacheStack>();
		var cacheLayers = result.GetCacheLayers();
		Assert.AreEqual(1, cacheLayers.Count);
		Assert.AreEqual(typeof(MemoryCacheLayer), cacheLayers[0].GetType());
		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");
	}

	[TestMethod]
	public void CacheStack_NamedCacheStackBuilder_InvalidName()
	{
		var serviceCollection = new ServiceCollection();

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack("MyNamedCacheStack", (serviceProvider, builder) =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});
		var provider = serviceCollection.BuildServiceProvider();
		var cacheStackAccessor = provider.GetRequiredService<ICacheStackAccessor>();

		Assert.ThrowsException<ArgumentException>(() => cacheStackAccessor.GetCacheStack("NotTheRealName"));
		Assert.IsFalse(hasBuilderBeenCalled, "Builder has been called");
	}

	[TestMethod]
	public void CacheStack_NamedCacheStackBuilder_ValidName()
	{
		var serviceCollection = new ServiceCollection();

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack("MyNamedCacheStack", (serviceProvider, builder) =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});
		var provider = serviceCollection.BuildServiceProvider();
		var cacheStackAccessor = provider.GetRequiredService<ICacheStackAccessor>();
		var cacheStack = cacheStackAccessor.GetCacheStack("MyNamedCacheStack");

		var cacheLayers = (cacheStack as IExtendableCacheStack).GetCacheLayers();
		Assert.AreEqual(1, cacheLayers.Count);
		Assert.AreEqual(typeof(MemoryCacheLayer), cacheLayers[0].GetType());
		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");
	}

	[TestMethod]
	public void CacheStack_NamedCacheStackBuilder_GenericCacheStackConversion()
	{
		var serviceCollection = new ServiceCollection();

		serviceCollection.AddCacheStack<object>("MyGenericNamedCacheStack", (serviceProvider, builder) =>
		{
			builder.AddMemoryCacheLayer();
		});

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack("MyNamedCacheStack", (serviceProvider, builder) =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});
		var provider = serviceCollection.BuildServiceProvider();
		var cacheStackAccessor = provider.GetRequiredService<ICacheStackAccessor<object>>();

		Assert.ThrowsException<InvalidOperationException>(() => cacheStackAccessor.GetCacheStack("MyNamedCacheStack"));
		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");
	}

	[TestMethod]
	public void GenericCacheStack_CacheStackBuilder()
	{
		var serviceCollection = new ServiceCollection();

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack<int>(builder =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});
		var serviceProvider = serviceCollection.BuildServiceProvider();

		var result = (IExtendableCacheStack)serviceProvider.GetRequiredService<ICacheStack<int>>();
		var cacheLayers = result.GetCacheLayers();
		Assert.AreEqual(1, cacheLayers.Count);
		Assert.AreEqual(typeof(MemoryCacheLayer), cacheLayers[0].GetType());
		Assert.IsTrue(result is ICacheStack<int>);
		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");
	}

	[TestMethod]
	public void GenericCacheStack_NamedCacheStackBuilder_InvalidName()
	{
		var serviceCollection = new ServiceCollection();

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack<int>("MyNamedCacheStack", (serviceProvider, builder) =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});
		var provider = serviceCollection.BuildServiceProvider();
		var cacheStackAccessor = provider.GetRequiredService<ICacheStackAccessor<int>>();

		Assert.ThrowsException<ArgumentException>(() => cacheStackAccessor.GetCacheStack("NotTheRealName"));
		Assert.IsFalse(hasBuilderBeenCalled, "Builder has been called");
	}

	[TestMethod]
	public void GenericCacheStack_NamedCacheStackBuilder_ValidName()
	{
		var serviceCollection = new ServiceCollection();

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack<int>("MyNamedCacheStack", (serviceProvider, builder) =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});
		var provider = serviceCollection.BuildServiceProvider();
		var cacheStackAccessor = provider.GetRequiredService<ICacheStackAccessor<int>>();
		var cacheStack = cacheStackAccessor.GetCacheStack("MyNamedCacheStack");

		var cacheLayers = (cacheStack as IExtendableCacheStack).GetCacheLayers();
		Assert.AreEqual(1, cacheLayers.Count);
		Assert.AreEqual(typeof(MemoryCacheLayer), cacheLayers[0].GetType());
		Assert.IsTrue(cacheStack is ICacheStack<int>);
		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");
	}

	[TestMethod]
	public void GenericCacheStack_CacheStackBuilder_CustomCacheContextActivator()
	{
		var serviceCollection = new ServiceCollection();
		var contextActivatorMock = Substitute.For<ICacheContextActivator>();

		var hasBuilderBeenCalled = false;
		serviceCollection.AddCacheStack<int>(contextActivatorMock, builder =>
		{
			hasBuilderBeenCalled = true;
			builder.AddMemoryCacheLayer();
		});
		var serviceProvider = serviceCollection.BuildServiceProvider();

		var result = (IExtendableCacheStack)serviceProvider.GetRequiredService<ICacheStack<int>>();
		var cacheLayers = result.GetCacheLayers();
		Assert.AreEqual(1, cacheLayers.Count);
		Assert.AreEqual(typeof(MemoryCacheLayer), cacheLayers[0].GetType());
		Assert.IsTrue(result is ICacheStack<int>);
		Assert.IsTrue(hasBuilderBeenCalled, "Builder has not been called");
	}

	[TestMethod]
	public void CacheStackBuilder_AddCacheLayers()
	{
		var cacheStackBuilder = new CacheStackBuilder();
		var mongoDbConnectionMock = Substitute.For<MongoFramework.IMongoDbConnection>();
		var redisConnectionMock = Substitute.For<StackExchange.Redis.IConnectionMultiplexer>();

		cacheStackBuilder
			.AddMemoryCacheLayer()
			.AddFileCacheLayer(new FileCacheLayerOptions("./FileCacheLayer", NewtonsoftJsonCacheSerializer.Instance))
			.AddMongoDbCacheLayer(mongoDbConnectionMock)
			.AddRedisCacheLayer(redisConnectionMock, new RedisCacheLayerOptions(NewtonsoftJsonCacheSerializer.Instance));

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
		var redisConnectionMock = Substitute.For<StackExchange.Redis.IConnectionMultiplexer>();
		redisConnectionMock.GetSubscriber(Arg.Any<object>())
			.Returns(Substitute.For<StackExchange.Redis.ISubscriber>());

		cacheStackBuilder
			.WithCleanupFrequency(TimeSpan.FromSeconds(5))
			.WithRedisDistributedLocking(redisConnectionMock)
			.WithRedisRemoteEviction(redisConnectionMock);

		Assert.AreEqual(3, cacheStackBuilder.Extensions.Count);
		Assert.IsInstanceOfType(cacheStackBuilder.Extensions[0], typeof(AutoCleanupExtension));
		Assert.IsInstanceOfType(cacheStackBuilder.Extensions[1], typeof(RedisLockExtension));
		Assert.IsInstanceOfType(cacheStackBuilder.Extensions[2], typeof(RedisRemoteEvictionExtension));
	}
}
