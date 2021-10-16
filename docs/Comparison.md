# Caching Performance Comparison

**Test Machine**

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=6.0.100-rc.2.21505.57
  [Host]     : .NET Core 5.0.9 (CoreCLR 5.0.921.35908, CoreFX 5.0.921.35908), X64 RyuJIT
  Job-RQNFQX : .NET Core 5.0.9 (CoreCLR 5.0.921.35908, CoreFX 5.0.921.35908), X64 RyuJIT

Runtime=.NET Core 5.0  MaxIterationCount=200
```

_Note: The performance figures below are as a guide only. Different systems and configurations can drastically change performance results._

## Sequential In-Memory Caching

|                            Method |     Mean |   Error |  StdDev |        Op/s | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|------------:|------:|--------:|-------:|------:|------:|----------:|
|       CacheTower_MemoryCacheLayer | 249.2 ns | 3.41 ns | 3.19 ns | 4,012,492.0 |  1.00 |    0.00 | 0.0229 |     - |     - |      72 B |
|      IntelligentCache_MemoryCache | 307.0 ns | 2.30 ns | 2.04 ns | 3,257,086.2 |  1.23 |    0.02 | 0.0277 |     - |     - |      88 B |
|        FusionCache_MemoryProvider | 419.7 ns | 3.37 ns | 2.98 ns | 2,382,914.8 |  1.68 |    0.03 | 0.0458 |     - |     - |     144 B |
|          LazyCache_MemoryProvider | 461.2 ns | 6.73 ns | 6.29 ns | 2,168,316.7 |  1.85 |    0.03 | 0.1144 |     - |     - |     360 B |
|              EasyCaching_InMemory | 471.8 ns | 3.23 ns | 2.86 ns | 2,119,738.2 |  1.89 |    0.03 | 0.0482 |     - |     - |     152 B |
| CacheManager_MicrosoftMemoryCache | 546.0 ns | 6.13 ns | 4.78 ns | 1,831,499.9 |  2.19 |    0.03 | 0.0277 |     - |     - |      88 B |

## Parallel In-Memory Caching

|                            Method |        Mean |     Error |    StdDev |     Op/s | Ratio | RatioSD |    Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|----------:|----------:|---------:|------:|--------:|---------:|------:|------:|----------:|
|       CacheTower_MemoryCacheLayer |    84.34 us |  1.129 us |  1.056 us | 11,856.5 |  1.00 |    0.00 |   2.0752 |     - |     - |   6.26 KB |
|      IntelligentCache_MemoryCache |    95.79 us |  0.825 us |  0.771 us | 10,439.0 |  1.14 |    0.02 |  30.2734 |     - |     - |  92.13 KB |
|        FusionCache_MemoryProvider |   140.79 us |  1.075 us |  1.005 us |  7,103.0 |  1.67 |    0.02 |  50.5371 |     - |     - | 152.98 KB |
| CacheManager_MicrosoftMemoryCache |   157.12 us |  2.271 us |  2.013 us |  6,364.6 |  1.86 |    0.03 |  31.9824 |     - |     - |  97.54 KB |
|              EasyCaching_InMemory |   157.99 us |  1.144 us |  1.014 us |  6,329.4 |  1.87 |    0.03 |  50.7813 |     - |     - | 155.06 KB |
|          LazyCache_MemoryProvider | 1,253.85 us | 25.031 us | 26.783 us |    797.5 | 14.92 |    0.38 | 121.0938 |     - |     - | 365.37 KB |


## Sequential Redis Caching

|                     Method |     Mean |   Error |  StdDev |    Op/s | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |---------:|--------:|--------:|--------:|------:|-------:|------:|------:|----------:|
|         CacheManager_Redis | 134.7 us | 0.72 us | 0.67 us | 7,425.3 |  0.84 | 0.7324 |     - |     - |    2376 B |
|     IntelligentCache_Redis | 156.2 us | 0.91 us | 0.85 us | 6,403.4 |  0.97 | 0.9766 |     - |     - |    3456 B |
|          EasyCaching_Redis | 158.1 us | 0.50 us | 0.47 us | 6,326.3 |  0.99 | 0.2441 |     - |     - |    1144 B |
| CacheTower_RedisCacheLayer | 160.3 us | 0.47 us | 0.44 us | 6,238.4 |  1.00 | 0.2441 |     - |     - |     936 B |

## Parallel Redis Caching

|                     Method |       Mean |    Error |   StdDev |    Op/s | Ratio | RatioSD |   Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|--------------------------- |-----------:|---------:|---------:|--------:|------:|--------:|--------:|--------:|-------:|----------:|
| CacheTower_RedisCacheLayer |   413.2 us |  9.73 us | 40.90 us | 2,420.0 |  1.00 |    0.00 | 24.4141 |  9.7656 | 1.9531 | 110.38 KB |
|          EasyCaching_Redis |   452.3 us |  8.99 us | 31.63 us | 2,211.0 |  1.11 |    0.13 | 22.4609 |  9.7656 | 2.9297 | 108.74 KB |
|     IntelligentCache_Redis |   506.8 us | 10.02 us | 25.85 us | 1,973.1 |  1.26 |    0.15 | 72.2656 | 23.9258 | 3.9063 | 332.72 KB |
|         CacheManager_Redis | 2,999.1 us | 52.07 us | 46.16 us |   333.4 |  7.94 |    0.80 | 82.0313 |       - |      - | 241.57 KB |


## File Caching

|                            Method |     Mean |   Error |  StdDev |    Op/s | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|--------:|------:|--------:|-------:|------:|------:|----------:|
| CacheTower_ProtobufFileCacheLayer | 283.9 us | 5.54 us | 6.80 us | 3,522.7 |  0.99 |    0.04 | 0.9766 |     - |     - |   3.13 KB |
|     CacheTower_JsonFileCacheLayer | 288.1 us | 5.67 us | 6.07 us | 3,471.4 |  1.00 |    0.00 | 2.9297 |     - |     - |   8.66 KB |
|                  EasyCaching_Disk | 341.8 us | 6.82 us | 6.38 us | 2,925.6 |  1.19 |    0.03 | 1.4648 |     - |     - |   5.09 KB |