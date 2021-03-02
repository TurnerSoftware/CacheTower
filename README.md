# Cache Tower
An efficient multi-layered caching system for .NET

[![AppVeyor](https://img.shields.io/appveyor/ci/Turnerj/cachetower/master.svg)](https://ci.appveyor.com/project/Turnerj/cachetower)
[![Codecov](https://img.shields.io/codecov/c/github/turnersoftware/cachetower/master.svg)](https://codecov.io/gh/TurnerSoftware/CacheTower)
[![NuGet](https://img.shields.io/nuget/v/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/)

## Overview

Computers have multiple layers of caching from L1/L2/L3 CPU caches to RAM or even disk caches, each with a different purpose and performance profile.

_Why don't we do this with our code?_

Cache Tower isn't a single type of cache, its a multi-layer solution to caching with each layer on top of another.
A multi-layer cache provides the performance benefits of a fast cache like in-memory with the resilience of a file, database or Redis-backed cache.

This library was inspired by a blog post by Nick Craver about [how Stack Overflow do caching](https://nickcraver.com/blog/2019/08/06/stack-overflow-how-we-do-app-caching/).<br>
They use a custom 2-layer caching solution with in-memory and Redis.

## Features

- Local system caching with in-memory and file-based caches.
- Distributed system caching with MongoDB and Redis.
- Background refreshes of non-expired but "stale" data, helping avoid expired data cache misses.
- Local refresh locking (only refreshing the cache in one thread).
- Distributed refresh locking (only refreshing the cache in one application across multiple servers).
- Distributed evictions for clearing out old data from shared caches.
- All-async API, ready for high performance workloads.
- Targets minimum .NET Standard 2.0 for wide compatibility.

## Table of Contents

- [Installation](#Installation)
- [Understanding a Multi-Layer Caching System](#Understanding-a-Multi-Layer-Caching-System)
- [The Cache Layers of Cache Tower](#The-Cache-Layers-of-Cache-Tower)
- [Background Refreshing of Stale Data](#Background-Refreshing-of-Stale-Data)
- [Cache Tower Extensions](#Cache-Tower-Extensions)
  - [Automatic Cleanup](#Automatic-Cleanup)
  - [Distributed Locking via Redis](#Distributed-Locking-via-Redis)
  - [Distributed Eviction via Redis](#Distributed-Eviction-via-Redis)
- [Performance and Comparisons](#Performance-and-Comparisons)
- [Example Usage](#Example-Usage)
- [Advanced Usage](#Advanced-Usage)
  - [Flushing the Cache](#Flushing-the-Cache)

## Installation

You will need the `CacheTower` package on NuGet - it provides the core infrastructure for Cache Tower as well as an in-memory cache layer.
To add additional cache layers, you will need to install the appropriate packages as listed below.

| Package | NuGet | Downloads |
| ------- | ----- | --------- |
| [CacheTower](https://www.nuget.org/packages/CacheTower/)<br><small>The core library with in-memory caching support.</small> | [![NuGet](https://img.shields.io/nuget/v/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.svg)](https://www.nuget.org/packages/CacheTower/) |
| [CacheTower.Extensions.Redis](https://www.nuget.org/packages/CacheTower.Extensions.Redis/)<br><small>Provides distributed locking &amp; eviction via Redis.</small> | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Extensions.Redis.svg)](https://www.nuget.org/packages/CacheTower.Extensions.Redis/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Extensions.Redis.svg)](https://www.nuget.org/packages/CacheTower.Extensions.Redis/) |
| [CacheTower.Providers.Database.MongoDB](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/)<br><small>Provides a cache layer for MongoDB.</small> | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.Database.MongoDB.svg)](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.Database.MongoDB.svg)](https://www.nuget.org/packages/CacheTower.Providers.Database.MongoDB/) |
| [CacheTower.Providers.FileSystem.Json](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/)<br><small>Provides a file-based cache layer using JSON.</small> | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.FileSystem.Json.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.FileSystem.Json.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Json/) |
| [CacheTower.Providers.FileSystem.Protobuf](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/)<br><small>Provides a file-based cache layer using Protobuf.</small> | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.FileSystem.Protobuf.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.FileSystem.Protobuf.svg)](https://www.nuget.org/packages/CacheTower.Providers.FileSystem.Protobuf/) |
| [CacheTower.Providers.Redis](https://www.nuget.org/packages/CacheTower.Providers.Redis/)<br><small>Provides a cache layer for Redis.</small> | [![NuGet](https://img.shields.io/nuget/v/CacheTower.Providers.Redis.svg)](https://www.nuget.org/packages/CacheTower.Providers.Redis/) | [![NuGet](https://img.shields.io/nuget/dt/CacheTower.Providers.Redis.svg)](https://www.nuget.org/packages/CacheTower.Providers.Redis/) |

## Understanding a Multi-Layer Caching System

At its most basic level, caching is designed to prevent reprocessing of data by storing the result _somewhere_.
In turn, preventing the reprocessing of data makes our code faster and more scaleable.
Depending on the method of storage or transportation, the performance profile can vary drastically.
Not only that, limitations of different types of caches can affect what you can do with your application.

----

#### In-memory Caching

✔ **Pro**: The fastest cache you can possible have!

❌ **Con**: Only lasts the lifetime of the application.

❌ **Con**: Memory capacity is more limited than other types of storage.

#### File-based Caching

✔ **Pro**: Caching huge amounts of data is not just possible, it is usually cheap!

✔ **Pro**: Resilient to application restarts!

❌ **Con**: Even with fast SSDs, it can be orders-of-magnitude slower than in-memory!

#### Database Caching

✔ **Pro**: Database can run on the local machine _OR_ a remote machine!

✔ **Pro**: Resilient to application restarts!

✔ **Pro**: Can support multiple systems at the same time!

❌ **Con**: Performance is only as good as the database provider itself.

#### Redis Caching

✔ **Pro**: Redis can run on the local machine _OR_ a remote machine!

✔ **Pro**: Resilient to application restarts!

✔ **Pro**: Can support multiple systems at the same time!

✔ **Pro**: Extremely fast, losing out only to in-memory!

❌ **Con**: Linux only. *

<small>_* On Windows, [Memurai is your best Redis-compatible alternative](https://www.memurai.com/) - just need to list some sort of con for Redis and what it ran on was all I could think of at the time._</small>

----

An ideal caching solution should be fast, flexible, resilient and scale with your usage.
It is through combining these different cache types that this can be achieved.

Cache Tower supports n-layers of caching with flexibility to even make your own.
You "stack" the cache layers from the fastest to slowest for your particular usage.

For example, you might have:
1. In-memory cache
1. File-based cache

With this setup, you have:
- A fast first-layer cache
- A resilient second-layer cache

If your application restarts and your in-memory cache is empty, your second-layer cache will be checked.
If a valid cache entry is found, that will be returned.

Which combination of cache layers you use to build your cache stack is up to you and what is best for your application.

## The Cache Layers of Cache Tower

Cache Tower has a number of officially supported cache layers that you can use.

#### MemoryCacheLayer

> Bundled with Cache Tower

Allows for fast, local memory caching.
The data is kept as a reference in memory and _not serialized_.
It is strongly recommended to treat the cached instance as immutable.
Modification of an in-memory cached value won't be updated to other cache layers.

#### JsonFileCacheLayer

```powershell
PM> Install-Package CacheTower.Providers.FileSystem.Json
```

Using [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json/), provides a basic file-based caching solution.
It stores each serialized cache item into its own file and uses a singular manifest file to track the status of the cache.

#### ProtobufFileCacheLayer

```powershell
PM> Install-Package CacheTower.Providers.FileSystem.Protobuf
```

Using [protobuf-net](https://github.com/protobuf-net/protobuf-net), provides a basic file-based caching solution.
It stores each serialized cache item into its own file and uses a singular manifest file to track the status of the cache.

The use of [protobuf-net requires decorating the class](https://github.com/protobuf-net/protobuf-net#1-first-decorate-your-classes) you want to cache with attributes `[ProtoContract]` and `[ProtoMember]`.

**Example with Protobuf Attributes**
```csharp
[ProtoContract]
public class UserProfile
{
	[ProtoMember(1)]
	public int UserId { get; set; }
	[ProtoMember(2)]
	public string UserName { get; set; }

	...
}
```

Additionally, as the Protobuf format doesn't have a way to represent an empty collection, these will be returned as `null`.
While this can be inconvienent, using Protobuf ensures high performance and low allocations for serializing.

#### MongoDbCacheLayer

```powershell
PM> Install-Package CacheTower.Providers.Database.MongoDB
```

Allows caching through a MongoDB server.
Cache entries are serialized to BSON using `MongoDB.Bson.Serialization.BsonSerializer`.

#### RedisCacheLayer

```powershell
PM> Install-Package CacheTower.Providers.Redis
```

Allows caching of data in Redis. Data is serialized to Protobuf using [protobuf-net](https://github.com/protobuf-net/protobuf-net).

Like the [ProtobufFileCacheLayer](#ProtobufFileCacheLayer), the use of [protobuf-net requires decorating the class](https://github.com/protobuf-net/protobuf-net#1-first-decorate-your-classes) you want to cache with attributes `[ProtoContract]` and `[ProtoMember]`.

## Getting Started


> In this example, `UserContext` is a type added to the service collection.
It will be retrieved from the service provider every time a cache refresh is required.

Create and configure your `CacheStack`, this is the backbone for Cache Tower.

```csharp
services.AddCacheStack<UserContext>(
	new [] {
		new MemoryCacheLayer(),
		new RedisCacheLayer(/* Your Redis Connection */)
	}, 
	new [] {
		new AutoCleanupExtension(TimeSpan.FromMinutes(5))
	}
);
```

The cache stack will be injected into constructors that accept `ICacheStack<UserContext>`.
Once you have your cache stack, you can call `GetOrSetAsync` - this is the primary way to access the data in the cache.

```csharp
var userId = 17;

await cacheStack.GetOrSetAsync<UserProfile>($"user-{userId}", async (old, context) => {
	
	return await context.GetUserForIdAsync(userId);
}, new CacheSettings(TimeSpan.FromDays(1), TimeSpan.FromMinutes(60));
```

This call to `GetOrSetAsync` is configured with a cache expiry of `1 day` and an effective stale time after `60 minutes`.
A good stale time is extremely useful for high performance scenarios where background refreshing is leveraged.

## Background Refreshing of Stale Data

A high-performance cache needs to keep throughput high.
Having a cache miss because of expired data stalls the potential throughput.

Rather than only having a cache expiry, Cache Tower supports specifying a stale time for the cache entry.
If there is a cache hit on an item and the item is considered stale, it will perform a background refresh.
By doing this, it avoids blocking the request on a potential cache miss later.

```csharp
await cacheStack.GetOrSetAsync<MyCachedType>("my-cache-key", async (oldValue, context) => {
	return await DoWorkThatNeedsToBeCached();
}, new CacheSettings(timeToLive: TimeSpan.FromMinutes(60), staleAfter: TimeSpan.FromMinutes(30)));
```

In the example above, the cache would expire in 60 minutes time.
However, after 30 minutes, the cache will be considered stale.

If this code was run again at least 30 minutes from now but no later than 60 minutes from now, it would perform a background refresh.
If we call this code again after 60 minutes from now, it will be a cache miss and block the thread till the cache is refreshed.

## Cache Tower Extensions

To allow more flexibility, Cache Tower uses an extension system to enhance functionality.
Some of these extensions rely on third party libraries and software to function correctly.

### Automatic Cleanup

> Bundled with Cache Tower

The cache layers themselves, for the most part, don't directly manage the co-ordination of when they need to delete expired data.
While the `RedisCacheLayer` does handle cache expiration directly via Redis, none of the other official cache layers do.
Unless you are only using the Redis cache layer, you will be wanting to include this extension in your cache stack.

### Distributed Locking via Redis

```powershell
PM> Install-Package CacheTower.Extensions.Redis
```

The `RedisLockExtension` uses Redis as a shared lock between multiple instances of your application.
Using Redis in this way can avoid cache stampedes where multiple different web servers are refreshing values at the same instant.

If you are only running one web server/instance of your application, you won't need this extension.

#### Distributed Eviction via Redis

```powershell
PM> Install-Package CacheTower.Extensions.Redis
```

The `RedisRemoteEvictionExtension` extension uses the pub/sub feature of Redis to co-ordinate cache invalidation across multiple instances of your application.
This works in the situation where one web server has refreshed a key and wants to let the other web servers know their data is now old.

## Performance and Comparisons

Cache Tower has been built from the ground up for high performance and low memory consumption.
Across a number of benchmarks against other caching solutions, Cache Tower performs similarly or better than the competition.

Where Cache Tower makes up in speed, it may lack a variety of features common amongst other caching solutions.
It is important to weigh both the feature set and performance when deciding on a caching solution.

For internal benchmarks between cache layers, see: [Performance Figures & Cache Layer Comparisons](/docs/Performance.md).<br>
For comparisons between alternative caching libraries, see: [Comparisons to Cache Tower Alternatives](/docs/Comparison.md).

## Advanced Usage

### Flushing the Cache

There are times where you want to clear all cache layers - whether to help with debugging an issue or force fresh data on subsequent calls to the cache.
This type of action is available in Cache Tower however is obfuscated somewhat to prevent accidental use.
Please only flush the cache if you know what you're doing and what it would mean!

If you have injected `ICacheStack` or `ICacheStack<UserContext>` into your current method or class, you can cast to `IFlushableCacheStack`.
This interface exposes the method `FlushAsync`.

```csharp
await (myCacheStack as IFlushableCacheStack).FlushAsync();
```

For the `MemoryCacheLayer`, the backing store is cleared.
For file cache layers, all cache files are removed.
For MongoDB, all documents are deleted in the cache collection.
For Redis, a `FlushDB` command is sent.

Combined with the `RedisRemoteEvictionExtension`, a call to `FlushAsync` will additionally be sent to all connected `CacheStack` instances.