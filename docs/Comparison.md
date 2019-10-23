# Comparison

The performance figures below are as a guide only. Different systems and configurations can drastically change performance results.
Run the benchmark suite on your target machine to best gauge how the performance is for your use case.

If you are one of the maintainers of the libraries referenced below and you feel the test is unfair (eg. not properly configured), open a PR to fix it!

**Test Machine**

```
BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100
  [Host] : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  Core   : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT

Job=Core  Runtime=Core
```

# Memory Caching

|                            Method | Iterations |             Mean |         Error |        StdDev |    Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|---------------------------------- |----------- |-----------------:|--------------:|--------------:|---------:|--------:|-----------:|-----------:|------:|-----------:|
|       CacheTower_MemoryCacheLayer |          1 |         3.054 us |     0.0304 us |     0.0284 us |     1.00 |    0.00 |     0.7820 |          - |     - |     2.4 KB |
| CacheManager_MicrosoftMemoryCache |          1 |        20.241 us |     0.2066 us |     0.1933 us |     6.63 |    0.07 |     2.4719 |     1.2207 |     - |    7.66 KB |
|                 Akavache_InMemory |          1 |     1,377.079 us |    10.2108 us |     9.0516 us |   450.89 |    6.15 |    19.5313 |     9.7656 |     - |   63.62 KB |
|                                   |            |                  |               |               |          |         |            |            |       |            |
|       CacheTower_MemoryCacheLayer |        100 |        97.900 us |     0.6145 us |     0.5748 us |     1.00 |    0.00 |    17.0898 |          - |     - |   52.67 KB |
| CacheManager_MicrosoftMemoryCache |        100 |       209.274 us |     1.2602 us |     1.0523 us |     2.14 |    0.02 |    10.9863 |     3.6621 |     - |   33.97 KB |
|                 Akavache_InMemory |        100 |   137,334.057 us |   957.2473 us |   895.4097 us | 1,402.85 |   12.22 |  2000.0000 |  1000.0000 |     - | 6364.58 KB |
|                                   |            |                  |               |               |          |         |            |            |       |            |
|       CacheTower_MemoryCacheLayer |       1000 |       945.631 us |    13.1357 us |    11.6445 us |     1.00 |    0.00 |   166.0156 |          - |     - |   509.7 KB |
| CacheManager_MicrosoftMemoryCache |       1000 |     1,928.871 us |    24.4260 us |    22.8481 us |     2.04 |    0.04 |    85.9375 |          - |     - |  273.16 KB |
|                 Akavache_InMemory |       1000 | 1,403,949.033 us | 9,306.6741 us | 8,705.4687 us | 1,485.49 |   23.32 | 20000.0000 | 10000.0000 |     - | 63594.5 KB |

# JSON File Caching

|                        Method | Iterations |         Mean |      Error |     StdDev | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|------------------------------ |----------- |-------------:|-----------:|-----------:|------:|--------:|-----------:|------:|------:|------------:|
| CacheTower_JsonFileCacheLayer |          1 |     6.489 ms |  0.2167 ms |  0.9008 ms |  1.00 |    0.00 |          - |     - |     - |    14.42 KB |
|                   MonkeyCache |          1 |     6.153 ms |  0.2143 ms |  0.9050 ms |  0.97 |    0.20 |          - |     - |     - |    65.81 KB |
|                               |            |              |            |            |       |         |            |       |       |             |
| CacheTower_JsonFileCacheLayer |        100 |   147.167 ms |  2.9970 ms |  8.2546 ms |  1.00 |    0.00 |          - |     - |     - |       14 KB |
|                   MonkeyCache |        100 |   180.314 ms |  3.4049 ms |  3.3440 ms |  1.24 |    0.06 |  1000.0000 |     - |     - |  4379.57 KB |
|                               |            |              |            |            |       |         |            |       |       |             |
| CacheTower_JsonFileCacheLayer |       1000 | 1,445.278 ms | 28.6562 ms | 55.2107 ms |  1.00 |    0.00 |  9000.0000 |     - |     - |       14 KB |
|                   MonkeyCache |       1000 | 1,850.698 ms | 24.4773 ms | 22.8961 ms |  1.28 |    0.06 | 14000.0000 |     - |     - | 43596.71 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.471 ms | 0.1311 ms | 0.3761 ms |   1.408 ms |  1.00 |    0.00 |         - |     - |     - |     4.2 KB |
|         CacheManager_Redis |          1 |   2.340 ms | 0.1235 ms | 0.3523 ms |   2.340 ms |  1.70 |    0.52 |         - |     - |     - |   22.77 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |        100 |  42.174 ms | 0.8972 ms | 2.4711 ms |  41.323 ms |  1.00 |    0.00 |         - |     - |     - |     3.8 KB |
|         CacheManager_Redis |        100 |  45.689 ms | 1.0551 ms | 3.0611 ms |  44.689 ms |  1.09 |    0.08 |         - |     - |     - |  399.34 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |       1000 | 393.530 ms | 4.8321 ms | 4.0350 ms | 394.228 ms |  1.00 |    0.00 | 1000.0000 |     - |     - |    3.93 KB |
|         CacheManager_Redis |       1000 | 422.326 ms | 4.8577 ms | 4.5439 ms | 421.350 ms |  1.07 |    0.02 | 1000.0000 |     - |     - | 3824.77 KB |