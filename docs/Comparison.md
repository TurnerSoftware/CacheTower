# Caching Performance Comparison

**Test Machine**

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1766 (21H1/May2021Update)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.300
  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  Job-BJQIPU : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT

Runtime=.NET 6.0  MaxIterationCount=200
```

_Note: The performance figures below are as a guide only. Different systems and configurations can drastically change performance results._

## Sequential In-Memory Caching

|                            Method |     Mean |   Error |  StdDev |        Op/s | Ratio | RatioSD |  Gen 0 | Allocated |
|---------------------------------- |---------:|--------:|--------:|------------:|------:|--------:|-------:|----------:|
|      IntelligentCache_MemoryCache | 177.1 ns | 3.43 ns | 3.36 ns | 5,647,113.9 |  0.78 |    0.02 | 0.0279 |      88 B |
|       CacheTower_MemoryCacheLayer | 226.5 ns | 4.35 ns | 4.28 ns | 4,415,623.1 |  1.00 |    0.00 | 0.0229 |      72 B |
| CacheManager_MicrosoftMemoryCache | 263.9 ns | 5.12 ns | 5.26 ns | 3,789,315.3 |  1.16 |    0.03 | 0.0277 |      88 B |
|        FusionCache_MemoryProvider | 295.7 ns | 5.60 ns | 5.50 ns | 3,382,054.7 |  1.31 |    0.04 | 0.1016 |     320 B |
|          LazyCache_MemoryProvider | 297.3 ns | 4.90 ns | 5.45 ns | 3,363,940.7 |  1.31 |    0.04 | 0.1144 |     360 B |
|              EasyCaching_InMemory | 301.1 ns | 5.86 ns | 7.41 ns | 3,321,052.2 |  1.33 |    0.03 | 0.0482 |     152 B |

## Parallel In-Memory Caching

|                            Method |      Mean |     Error |    StdDev |     Op/s | Ratio | RatioSD |    Gen 0 | Allocated |
|---------------------------------- |----------:|----------:|----------:|---------:|------:|--------:|---------:|----------:|
|      IntelligentCache_MemoryCache |  66.12 us |  0.479 us |  0.425 us | 15,124.8 |  0.89 |    0.01 |  29.2969 |     89 KB |
|       CacheTower_MemoryCacheLayer |  74.29 us |  1.292 us |  1.208 us | 13,460.5 |  1.00 |    0.00 |   0.9766 |      3 KB |
| CacheManager_MicrosoftMemoryCache |  93.83 us |  0.639 us |  0.566 us | 10,657.6 |  1.27 |    0.02 |  29.2969 |     89 KB |
|              EasyCaching_InMemory | 119.71 us |  1.501 us |  1.404 us |  8,353.7 |  1.61 |    0.02 |  49.9268 |    151 KB |
|        FusionCache_MemoryProvider | 132.27 us |  0.775 us |  0.605 us |  7,560.2 |  1.79 |    0.02 | 104.2480 |    316 KB |
|          LazyCache_MemoryProvider | 917.56 us | 16.636 us | 15.561 us |  1,089.9 | 12.35 |    0.14 | 118.1641 |    356 KB |


## Sequential Redis Caching

- Redis benchmarks unable to run due to [bug in StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/pull/2166).
  The results below are from an older version of .NET

|                     Method |     Mean |   Error |  StdDev |    Op/s | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |---------:|--------:|--------:|--------:|------:|-------:|------:|------:|----------:|
|         CacheManager_Redis | 134.7 us | 0.72 us | 0.67 us | 7,425.3 |  0.84 | 0.7324 |     - |     - |    2376 B |
|     IntelligentCache_Redis | 156.2 us | 0.91 us | 0.85 us | 6,403.4 |  0.97 | 0.9766 |     - |     - |    3456 B |
|          EasyCaching_Redis | 158.1 us | 0.50 us | 0.47 us | 6,326.3 |  0.99 | 0.2441 |     - |     - |    1144 B |
| CacheTower_RedisCacheLayer | 160.3 us | 0.47 us | 0.44 us | 6,238.4 |  1.00 | 0.2441 |     - |     - |     936 B |

## Parallel Redis Caching

- Redis benchmarks unable to run due to [bug in StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/pull/2166)
  The results below are from an older version of .NET

|                     Method |       Mean |    Error |   StdDev |    Op/s | Ratio | RatioSD |   Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|--------------------------- |-----------:|---------:|---------:|--------:|------:|--------:|--------:|--------:|-------:|----------:|
| CacheTower_RedisCacheLayer |   413.2 us |  9.73 us | 40.90 us | 2,420.0 |  1.00 |    0.00 | 24.4141 |  9.7656 | 1.9531 | 110.38 KB |
|          EasyCaching_Redis |   452.3 us |  8.99 us | 31.63 us | 2,211.0 |  1.11 |    0.13 | 22.4609 |  9.7656 | 2.9297 | 108.74 KB |
|     IntelligentCache_Redis |   506.8 us | 10.02 us | 25.85 us | 1,973.1 |  1.26 |    0.15 | 72.2656 | 23.9258 | 3.9063 | 332.72 KB |
|         CacheManager_Redis | 2,999.1 us | 52.07 us | 46.16 us |   333.4 |  7.94 |    0.80 | 82.0313 |       - |      - | 241.57 KB |


## File Caching

|                                   Method |     Mean |   Error |  StdDev |    Op/s | Ratio | RatioSD |  Gen 0 |  Gen 1 | Allocated |
|----------------------------------------- |---------:|--------:|--------:|--------:|------:|--------:|-------:|-------:|----------:|
| CacheTower_FileCacheLayer_SystemTextJson | 333.9 us | 6.40 us | 8.32 us | 2,994.5 |  0.99 |    0.03 | 0.9766 | 0.4883 |      3 KB |
|       CacheTower_FileCacheLayer_Protobuf | 337.1 us | 6.71 us | 7.45 us | 2,966.5 |  0.99 |    0.03 | 0.9766 | 0.4883 |      3 KB |
| CacheTower_FileCacheLayer_NewtonsoftJson | 340.1 us | 3.43 us | 3.21 us | 2,940.5 |  1.00 |    0.00 | 2.9297 | 1.4648 |      9 KB |
|                         EasyCaching_Disk | 359.7 us | 5.32 us | 4.97 us | 2,780.4 |  1.06 |    0.02 | 1.4648 |      - |      5 KB |