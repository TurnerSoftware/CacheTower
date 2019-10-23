# Cache Tower
A multi-layered caching system for .NET

[![AppVeyor](https://img.shields.io/appveyor/ci/Turnerj/cachetower/master.svg)](https://ci.appveyor.com/project/Turnerj/cachetower)
[![Codecov](https://img.shields.io/codecov/c/github/turnersoftware/cachetower/master.svg)](https://codecov.io/gh/TurnerSoftware/CacheTower)
[![NuGet](https://img.shields.io/nuget/v/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/)

## Overview

Cache Tower isn't a single type of cache, it allows control of various layers of caching on top of each other.

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

## Package Status

| Package | NuGet | Downloads |
| ------- | ----- | --------- |
| [CacheTower](https://www.nuget.org/packages/CacheTower/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/) |
| [CacheTower.Extensions.Redis](https://www.nuget.org/packages/CacheTower.Extensions.Redis/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Extensions.Redis.svg)](https://www.nuget.org/packages/CacheTower.Extensions.Redis/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Extensions.Redis.svg)](https://www.nuget.org/packages/CacheTower.Extensions.Redis/) |
| [CacheTower.Providers.Database.MongoDB](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.Database.MongoDB.svg)](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.Database.MongoDB.svg)](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/) |
| [CacheTower.Providers.FileSystem.Json](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.FileSystem.Json.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.FileSystem.Json.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/) |
| [CacheTower.Providers.FileSystem.Protobuf](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.FileSystem.Protobuf.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.FileSystem.Protobuf.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/) |
| [CacheTower.Providers.Redis](https://www.nuget.org/packages/CacheTower.Providers.Redis/) | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.Redis.svg)](https://www.nuget.org/packages/CacheTower.Providers.Redis/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.Redis.svg)](https://www.nuget.org/packages/CacheTower.Providers.Redis/) |

## Choosing Cache Layers

There are pros-and-cons to each of the types of cache layers though the idea is to use each layer to your advantage.

- In-memory caching provides the best performance though would use the most memory.
- File system caching can cache a lot of data but is extremely slow relative to in-memory caching.
- Protobuf file caching is faster than JSON file caching but [requires defining various attributes across the model](https://github.com/protobuf-net/protobuf-net#basic-usage).
- MongoDB caching can be fast and convenient under certain workloads (eg. when you don't have Redis).
- Redis caching is about as fast as possible while not being in-memory in the same process (or potentially server).

See also: [Performance Figures & Cache Layer Comparisons](/docs/Performance.md)
See also: [Comparisons to Cache Tower Alternatives](/docs/Comparison.md)

## Extension System

To allow more flexibility, Cache Tower uses an extension system to enhance functionality. Some of these extensions rely on third party libraries and software to function correctly.

### CacheTower

The main library comes with one extension by default but it is an important one: `AutoCleanupExtension`

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

## Example Usage

Setup and store a singleton of `CacheStack` (whether that is via Dependency Injection or a static variable).
```csharp

//Context is allowed to be null if you don't need it
var myContext = new SomeClassThatExtendsICacheContext();

var myCacheStack = new CacheStack(myContext, new [] {
	new MemoryCacheLayer(),
	new ProtobufFileCacheLayer("directory/where/the/cache/can/write")
}, new [] {
	new AutoCleanupExtension(TimeSpan.FromMinutes(5))
});

```

Somewhere in your code base where you are wanting to optionally pull data from your cache.
```csharp
await myCacheStack.GetOrSetAsync<MyTypeInTheCache>("MyCacheKey", async (old, context) => {
	//Here is where your heavy work code goes, maybe a call to the DB or API
	//Your returned value will be cached
	//"context" here is what you declared in the CacheStack constructor
	return await HoweverIGetMyData();
}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromMinutes(60));
```