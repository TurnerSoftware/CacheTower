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

|                            Method | Iterations |             Mean |          Error |         StdDev |    Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |   Allocated |
|---------------------------------- |----------- |-----------------:|---------------:|---------------:|---------:|--------:|-----------:|-----------:|------:|------------:|
|       CacheTower_MemoryCacheLayer |          1 |         2.706 us |      0.0401 us |      0.0355 us |     1.00 |    0.00 |     0.7820 |          - |     - |      2.4 KB |
| CacheManager_MicrosoftMemoryCache |          1 |        18.080 us |      0.2101 us |      0.1862 us |     6.68 |    0.11 |     2.4719 |     1.2207 |     - |     7.66 KB |
|                 Akavache_InMemory |          1 |     1,236.764 us |     12.2634 us |     10.2405 us |   457.00 |    8.45 |    19.5313 |     9.7656 |     - |    63.92 KB |
|                                   |            |                  |                |                |          |         |            |            |       |             |
|       CacheTower_MemoryCacheLayer |        100 |        91.540 us |      1.2362 us |      1.0958 us |     1.00 |    0.00 |    17.0898 |          - |     - |    52.67 KB |
| CacheManager_MicrosoftMemoryCache |        100 |       194.456 us |      0.6527 us |      0.5450 us |     2.13 |    0.03 |    10.9863 |     3.6621 |     - |    33.97 KB |
|                 Akavache_InMemory |        100 |   122,575.242 us |  1,777.0420 us |  1,662.2462 us | 1,340.69 |   21.96 |  2000.0000 |  1000.0000 |     - |  6354.04 KB |
|                                   |            |                  |                |                |          |         |            |            |       |             |
|       CacheTower_MemoryCacheLayer |       1000 |       897.207 us |      3.2611 us |      2.7231 us |     1.00 |    0.00 |   166.0156 |          - |     - |    509.7 KB |
| CacheManager_MicrosoftMemoryCache |       1000 |     1,742.979 us |     11.1251 us |      9.8621 us |     1.94 |    0.01 |    87.8906 |          - |     - |   273.16 KB |
|                 Akavache_InMemory |       1000 | 1,261,903.893 us | 12,830.5187 us | 11,373.9206 us | 1,407.22 |   13.64 | 20000.0000 | 10000.0000 |     - | 63473.34 KB |

# JSON File Caching

|                        Method | Iterations |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |   Allocated |
|------------------------------ |----------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|-----------:|------:|------------:|
| CacheTower_JsonFileCacheLayer |          1 |     5.689 ms |  0.1689 ms |  0.7057 ms |     5.630 ms |  1.00 |    0.00 |          - |          - |     - |    14.42 KB |
|         MonkeyCache_FileStore |          1 |     5.218 ms |  0.1625 ms |  0.6863 ms |     5.160 ms |  0.93 |    0.17 |          - |          - |     - |    65.81 KB |
|         Akavache_LocalMachine |          1 |     1.891 ms |  0.0703 ms |  0.2868 ms |     1.813 ms |  0.34 |    0.06 |          - |          - |     - |    81.07 KB |
|                               |            |              |            |            |              |       |         |            |            |       |             |
| CacheTower_JsonFileCacheLayer |        100 |   135.478 ms |  2.7366 ms | 10.9423 ms |   133.238 ms |  1.00 |    0.00 |          - |          - |     - |       14 KB |
|         MonkeyCache_FileStore |        100 |   162.651 ms |  3.2236 ms |  8.5485 ms |   160.330 ms |  1.20 |    0.11 |  1000.0000 |          - |     - |  4379.57 KB |
|         Akavache_LocalMachine |        100 |   118.770 ms |  2.4258 ms |  6.3479 ms |   116.760 ms |  0.87 |    0.08 |  2000.0000 |  1000.0000 |     - |  6367.09 KB |
|                               |            |              |            |            |              |       |         |            |            |       |             |
| CacheTower_JsonFileCacheLayer |       1000 | 1,259.683 ms | 24.7294 ms | 47.0502 ms | 1,260.364 ms |  1.00 |    0.00 |  9000.0000 |          - |     - |       14 KB |
|         MonkeyCache_FileStore |       1000 | 1,637.846 ms | 32.4476 ms | 64.0485 ms | 1,645.843 ms |  1.30 |    0.08 | 14000.0000 |          - |     - | 43596.71 KB |
|         Akavache_LocalMachine |       1000 | 1,345.352 ms | 21.2547 ms | 19.8817 ms | 1,344.432 ms |  1.10 |    0.04 | 20000.0000 | 10000.0000 |     - | 63472.94 KB |

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