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

|                            Method | Iterations |             Mean |         Error |        StdDev |    Ratio | RatioSD |      Gen 0 |     Gen 1 | Gen 2 |   Allocated |
|---------------------------------- |----------- |-----------------:|--------------:|--------------:|---------:|--------:|-----------:|----------:|------:|------------:|
|       CacheTower_MemoryCacheLayer |          1 |         3.209 us |     0.0361 us |     0.0338 us |     1.00 |    0.00 |     0.7820 |         - |     - |      2.4 KB |
| CacheManager_MicrosoftMemoryCache |          1 |        20.125 us |     0.1989 us |     0.1763 us |     6.27 |    0.08 |     2.4719 |    1.1902 |     - |     7.66 KB |
|                 Akavache_InMemory |          1 |     1,390.838 us |    10.1667 us |     9.5099 us |   433.50 |    5.74 |    19.5313 |    9.7656 |     - |    63.87 KB |
|                                   |            |                  |               |               |          |         |            |           |       |             |
|       CacheTower_MemoryCacheLayer |        100 |        96.805 us |     1.0490 us |     0.8760 us |     1.00 |    0.00 |    17.0898 |         - |     - |    52.67 KB |
| CacheManager_MicrosoftMemoryCache |        100 |       211.908 us |     1.5315 us |     1.4325 us |     2.19 |    0.03 |    10.9863 |    3.6621 |     - |    33.97 KB |
|                 Akavache_InMemory |        100 |   132,724.138 us | 2,628.2322 us | 5,187.8740 us | 1,432.98 |   47.54 |  2000.0000 | 1000.0000 |     - |  6367.49 KB |
|                                   |            |                  |               |               |          |         |            |           |       |             |
|       CacheTower_MemoryCacheLayer |       1000 |       937.390 us |     4.7230 us |     4.4179 us |     1.00 |    0.00 |   166.0156 |         - |     - |    509.7 KB |
| CacheManager_MicrosoftMemoryCache |       1000 |     1,900.141 us |    10.0000 us |     9.3540 us |     2.03 |    0.01 |    87.8906 |         - |     - |   273.16 KB |
|                 Akavache_InMemory |       1000 | 1,409,045.413 us | 8,932.9597 us | 8,355.8961 us | 1,503.20 |   12.58 | 20000.0000 | 5000.0000 |     - | 63407.21 KB |

# JSON File Caching

|                        Method | Iterations |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |   Allocated |
|------------------------------ |----------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|-----------:|------:|------------:|
| CacheTower_JsonFileCacheLayer |          1 |     6.450 ms |  0.1971 ms |  0.8171 ms |     6.422 ms |  1.00 |    0.00 |          - |          - |     - |    14.42 KB |
|         MonkeyCache_FileStore |          1 |     6.071 ms |  0.2088 ms |  0.8747 ms |     6.075 ms |  0.96 |    0.20 |          - |          - |     - |    65.81 KB |
|         Akavache_LocalMachine |          1 |     2.218 ms |  0.0945 ms |  0.3905 ms |     2.135 ms |  0.35 |    0.08 |          - |          - |     - |    81.07 KB |
|                               |            |              |            |            |              |       |         |            |            |       |             |
| CacheTower_JsonFileCacheLayer |        100 |   150.829 ms |  3.0392 ms | 11.9040 ms |   149.086 ms |  1.00 |    0.00 |          - |          - |     - |       14 KB |
|         MonkeyCache_FileStore |        100 |   182.428 ms |  3.6303 ms |  5.6519 ms |   182.509 ms |  1.23 |    0.10 |  1000.0000 |          - |     - |  4379.57 KB |
|         Akavache_LocalMachine |        100 |   131.856 ms |  2.6053 ms |  5.0815 ms |   131.755 ms |  0.89 |    0.07 |  2000.0000 |  1000.0000 |     - |  6367.09 KB |
|                               |            |              |            |            |              |       |         |            |            |       |             |
| CacheTower_JsonFileCacheLayer |       1000 | 1,495.962 ms | 31.2089 ms | 60.8705 ms | 1,476.287 ms |  1.00 |    0.00 |  9000.0000 |          - |     - |       14 KB |
|         MonkeyCache_FileStore |       1000 | 1,841.313 ms | 19.7300 ms | 18.4554 ms | 1,835.842 ms |  1.21 |    0.06 | 14000.0000 |          - |     - | 43596.71 KB |
|         Akavache_LocalMachine |       1000 | 1,435.708 ms | 18.4453 ms | 17.2538 ms | 1,429.102 ms |  0.94 |    0.05 | 20000.0000 | 10000.0000 |     - | 63475.94 KB |

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