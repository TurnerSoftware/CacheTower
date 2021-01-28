# Cache Tower
An efficient multi-layered caching system for .NET

[![AppVeyor](https://img.shields.io/appveyor/ci/Turnerj/cachetower/master.svg)](https://ci.appveyor.com/project/Turnerj/cachetower)
[![Codecov](https://img.shields.io/codecov/c/github/turnersoftware/cachetower/master.svg)](https://codecov.io/gh/TurnerSoftware/CacheTower)
[![NuGet](https://img.shields.io/nuget/v/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/)

## Overview

Computers have multiple layers of caching from L1/L2/L3 CPU caches to RAM or even disk caches, each with a different purpose and performance profile.
Why don't we do this with our code?

Cache Tower isn't a single type of cache, its a multi-layer solution to caching with each layer on top of another.

Officially supported cache layers include:
- MemoryCacheLayer (built-in)
- JsonFileCacheLayer (via `CacheTower.Providers.FileSystem.Json`)
- ProtobufFileCacheLayer (via `CacheTower.Providers.FileSystem.Protobuf`)
- MongoDbCacheLayer (via `CacheTower.Providers.Database.MongoDB`)
- RedisCacheLayer (via `CacheTower.Providers.Redis`)

These various cache layers, configurable by you, are controlled through the `CacheStack` which allows a few other goodies.

- Local refresh locking (a single instance of `CacheStack` prevents multiple threads refreshing at the same time)
- Remote refresh locking ([see details](#redislockextension))
- Optional stale-time for a cache entry (serve stale data while a background task refreshes the cache)
- Remote eviction on refresh ([see details](#remoteevictionextension))

For a comprehensive understanding of why a multi-layered cache is a good idea, have a read [Nick Craver's blog post](https://nickcraver.com/blog/2019/08/06/stack-overflow-how-we-do-app-caching/) about caching.
Nick's blog post was the inspiration behind Cache Tower and a number of the primary features including stale-time for cache entries.

## Package Status

| Package | NuGet | Downloads |
| ------- | ----- | --------- |
| [CacheTower](https://www.nuget.org/packages/CacheTower/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/) |
| [CacheTower.Extensions.Redis](https://www.nuget.org/packages/CacheTower.Extensions.Redis/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Extensions.Redis.svg)](https://www.nuget.org/packages/CacheTower.Extensions.Redis/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Extensions.Redis.svg)](https://www.nuget.org/packages/CacheTower.Extensions.Redis/) |
| [CacheTower.Providers.Database.MongoDB](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.Database.MongoDB.svg)](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.Database.MongoDB.svg)](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/) |
| [CacheTower.Providers.FileSystem.Json](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.FileSystem.Json.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.FileSystem.Json.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/) |
| [CacheTower.Providers.FileSystem.Protobuf](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.FileSystem.Protobuf.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.FileSystem.Protobuf.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/) |
| [CacheTower.Providers.Redis](https://www.nuget.org/packages/CacheTower.Providers.Redis/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.Redis.svg)](https://www.nuget.org/packages/CacheTower.Providers.Redis/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.Redis.svg)](https://www.nuget.org/packages/CacheTower.Providers.Redis/) |

## How do I choose what cache layers to use?

There are pros-and-cons to each of the types of cache layers though the idea is to use each layer to your advantage.

- In-memory caching provides the best performance though would use the most memory.
- File system caching can cache a lot of data but is extremely slow relative to in-memory caching.
- Protobuf file caching is faster than JSON file caching but [requires defining various attributes across the model](https://github.com/protobuf-net/protobuf-net#basic-usage).
- MongoDB caching can be fast and convenient under certain workloads (eg. when you don't have Redis).
- Redis caching is about as fast as possible while not being in-memory in the same process (or potentially server).

For more details: [Performance Figures & Cache Layer Comparisons](/docs/Performance.md)

## How does Cache Tower compare to other caching solutions?

Cache Tower has been built from the ground up for high performance and low memory consumption.
Across a number of benchmarks against other caching solutions, Cache Tower performs similarly or better than the competition.

Where Cache Tower makes up in speed, it may lack a variety of features common amongst other caching solutions.
It is important to weigh both the feature set and performance when deciding on a caching solution.

For more details: [Comparisons to Cache Tower Alternatives](/docs/Comparison.md)

## Extension System

To allow more flexibility, Cache Tower uses an extension system to enhance functionality. Some of these extensions rely on third party libraries and software to function correctly.

### Built-in

Cache Tower comes with one extension by default but it is an important one: `AutoCleanupExtension`

The cache layers themselves, for the most part, don't directly manage the co-ordination of when they need to delete expired data.
While the `RedisCacheLayer` does handle cache expiration directly, none of the other offical cache layers do.
Unless you are only using the Redis cache layer, you will be wanting to include this extension.

### CacheTower.Extensions.Redis

#### RedisLockExtension

This extension uses Redis as a shared lock between multiple instances of `CacheStack`, even across multiple web servers.
Using Redis in this way can avoid cache stampedes where multiple different web servers are refreshing values at the same instant.

If you are only running one web server with one instance (singleton) of `CacheStack`, you won't need this extension.

#### RedisRemoteEvictionExtension

This extension uses the pub/sub feature of Redis to co-ordinate cache invalidation from other `CacheStack` instances.
This works in the situation where one web server has refreshed a key and wants to let the other web servers know their data is now old.

While this can work independently of using `RedisLockExtension` or even the `RedisCacheLayer`, you most likely would be using these all at the same time.

## Usage

### Using Dependency Injection (DI)

```csharp

public void ConfigureServices(IServiceCollection services)
{
	services.AddCacheStack<MyCustomContext>(
		new [] {
			new MemoryCacheLayer(),
			new ProtobufFileCacheLayer("directory/where/the/cache/can/write")
		}, 
		new [] {
			new AutoCleanupExtension(TimeSpan.FromMinutes(5))
		}
	);
}

...

public class MyController
{
	private ICacheStack<MyCustomContext> CacheStack { get; set; }

	public MyController(ICacheStack<MyCustomContext> cacheStack)
	{
		CacheStack = cacheStack;
	}
}
```

### Using a static variable/singleton

```csharp
public static ICacheStack<MyCustomContext> CacheStack { get; private set; }

...

var myContext = new MyCustomContext();
CacheStack = new CacheStack<MyCustomContext>(myContext, new [] {
	new MemoryCacheLayer(),
	new ProtobufFileCacheLayer("directory/where/the/cache/can/write")
}, new [] {
	new AutoCleanupExtension(TimeSpan.FromMinutes(5))
});

```

### Accessing the Cache

Somewhere in your code base where you are wanting to optionally pull data from your cache.

```csharp
await myCacheStack.GetOrSetAsync<MyTypeInTheCache>("MyCacheKey", async (old, context) => {
	//Here is where your heavy work code goes, maybe a call to the DB or API
	//Your returned value will be cached
	//"context" here is what you declared in the CacheStack constructor
	return await HoweverIGetMyData(context);
}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromMinutes(60));
```

## Advanced Usage

### Flushing the Cache

There are times where you want to clear all cache layers - whether to help with debugging an issue or force fresh data on subsequent calls to the cache.
This type of action is available in Cache Tower however is obfuscated somewhat to prevent accidental use.
Please only flush the cache if you know what you're doing and what it would mean!

If you have injected `ICacheContext` or `ICacheContext<MyCacheContext>` into your current method or class, you can cast to `IFlushableCacheStack`.
This interface exposes the method `FlushAsync`.

```csharp
public ICacheContext MyCacheContext { get; set; } // Value injected via DI

...

await (MyCacheContext as IFlushableCacheStack).FlushAsync();
```

For the `MemoryCacheLayer`, the backing store is cleared.
For file cache layers, all cache files are removed.
For MongoDB, all documents are deleted in the cache collection.
For Redis, a `FlushDB` command is sent.

Combined with the `RedisRemoteEvictionExtension`, a call to `FlushAsync` will additionally be sent to all connected `CacheStack` instances.