# Caching Performance Comparison

**Test Machine**

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.867 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.201
  [Host]     : .NET Core 5.0.4 (CoreCLR 5.0.421.11614, CoreFX 5.0.421.11614), X64 RyuJIT
  Job-UYXMMR : .NET Core 5.0.4 (CoreCLR 5.0.421.11614, CoreFX 5.0.421.11614), X64 RyuJIT

Runtime=.NET Core 5.0  MaxIterationCount=200
```

_Note: The performance figures below are as a guide only. Different systems and configurations can drastically change performance results._

## Sequential In-Memory Caching

|                            Method |     Mean |   Error |  StdDev |        Op/s | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|------------:|------:|--------:|-------:|------:|------:|----------:|
|       CacheTower_MemoryCacheLayer | 248.4 ns | 4.09 ns | 3.63 ns | 4,026,201.7 |  1.00 |    0.00 | 0.0229 |     - |     - |      72 B |
|        FusionCache_MemoryProvider | 412.5 ns | 6.71 ns | 6.27 ns | 2,424,230.6 |  1.66 |    0.04 | 0.0458 |     - |     - |     144 B |
|              EasyCaching_InMemory | 448.8 ns | 5.08 ns | 4.75 ns | 2,228,009.7 |  1.81 |    0.04 | 0.0482 |     - |     - |     152 B |
|          LazyCache_MemoryProvider | 471.1 ns | 1.52 ns | 1.42 ns | 2,122,897.1 |  1.90 |    0.03 | 0.1144 |     - |     - |     360 B |
| CacheManager_MicrosoftMemoryCache | 521.5 ns | 6.82 ns | 6.05 ns | 1,917,499.9 |  2.10 |    0.05 | 0.0277 |     - |     - |      88 B |

## Parallel In-Memory Caching

|                            Method |        Mean |     Error |    StdDev |     Op/s | Ratio | RatioSD |    Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|----------:|----------:|---------:|------:|--------:|---------:|------:|------:|----------:|
|       CacheTower_MemoryCacheLayer |    73.50 us |  1.066 us |  0.997 us | 13,604.8 |  1.00 |    0.00 |   0.8545 |     - |     - |    2.9 KB |
|        FusionCache_MemoryProvider |   135.18 us |  1.685 us |  1.576 us |  7,397.7 |  1.84 |    0.03 |  47.6074 |     - |     - |  144.1 KB |
| CacheManager_MicrosoftMemoryCache |   147.12 us |  1.388 us |  1.230 us |  6,797.1 |  2.00 |    0.03 |  29.2969 |     - |     - |  89.23 KB |
|              EasyCaching_InMemory |   152.40 us |  2.172 us |  2.032 us |  6,561.8 |  2.07 |    0.03 |  49.8047 |     - |     - | 151.49 KB |
|          LazyCache_MemoryProvider | 1,213.94 us | 17.536 us | 14.644 us |    823.8 | 16.56 |    0.28 | 117.1875 |     - |     - | 356.17 KB |


## Sequential Redis Caching

|                     Method |     Mean |   Error |  StdDev |    Op/s | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |---------:|--------:|--------:|--------:|------:|-------:|------:|------:|----------:|
|         CacheManager_Redis | 129.8 us | 1.02 us | 0.80 us | 7,705.0 |  0.84 | 0.7324 |     - |     - |    2376 B |
|          EasyCaching_Redis | 151.0 us | 1.05 us | 0.98 us | 6,622.9 |  0.98 | 0.2441 |     - |     - |    1146 B |
| CacheTower_RedisCacheLayer | 154.5 us | 0.76 us | 0.71 us | 6,470.4 |  1.00 | 0.2441 |     - |     - |     936 B |

## Parallel Redis Caching

|                     Method |       Mean |    Error |   StdDev |    Op/s | Ratio | RatioSD |   Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|--------------------------- |-----------:|---------:|---------:|--------:|------:|--------:|--------:|-------:|-------:|----------:|
| CacheTower_RedisCacheLayer |   393.7 us |  7.87 us | 27.81 us | 2,540.0 |  1.00 |    0.00 | 24.9023 | 9.2773 | 2.9297 | 108.44 KB |
|          EasyCaching_Redis |   433.9 us |  8.68 us | 29.26 us | 2,304.6 |  1.11 |    0.11 | 21.9727 | 9.2773 | 2.4414 | 109.57 KB |
|         CacheManager_Redis | 3,112.3 us | 60.82 us | 70.04 us |   321.3 |  7.98 |    0.74 | 82.0313 |      - |      - | 242.82 KB |

## File Caching

|                            Method |     Mean |   Error |  StdDev |    Op/s | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|--------:|------:|--------:|-------:|------:|------:|----------:|
| CacheTower_ProtobufFileCacheLayer | 283.9 us | 5.54 us | 6.80 us | 3,522.7 |  0.99 |    0.04 | 0.9766 |     - |     - |   3.13 KB |
|     CacheTower_JsonFileCacheLayer | 288.1 us | 5.67 us | 6.07 us | 3,471.4 |  1.00 |    0.00 | 2.9297 |     - |     - |   8.66 KB |
|                  EasyCaching_Disk | 341.8 us | 6.82 us | 6.38 us | 2,925.6 |  1.19 |    0.03 | 1.4648 |     - |     - |   5.09 KB |