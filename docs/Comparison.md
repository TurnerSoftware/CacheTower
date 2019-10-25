# Comparison

The performance figures below are as a guide only. Different systems and configurations can drastically change performance results.
Run the benchmark suite on your target machine to best gauge how the performance is for your use case.

Keep in mind:
- The scale of time and the size of allocation differences between these libraries (some differences are so small that they are within a margin of error)
- The various libraries have different features, you would hopefully be choosing one not just because of its performance

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

## Memory Caching

|                                    Method | Iterations |               Mean |           Error |          StdDev |    Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------------------------ |----------- |-------------------:|----------------:|----------------:|---------:|--------:|-----------:|-----------:|------:|-----------:|
|        CacheTower_MemoryCacheLayer_Direct |          1 |           800.4 ns |         4.45 ns |         4.16 ns |     0.31 |    0.00 |     0.1631 |          - |     - |      512 B |
|                  LazyCache_MemoryProvider |          1 |         1,822.9 ns |        14.96 ns |        12.50 ns |     0.70 |    0.01 |     0.4139 |          - |     - |     1304 B |
|             LazyCache_MemoryProviderAsync |          1 |         2,183.7 ns |        15.53 ns |        14.53 ns |     0.84 |    0.01 |     0.4845 |          - |     - |     1520 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |          1 |         2,609.8 ns |        22.08 ns |        20.65 ns |     1.00 |    0.00 |     0.5531 |          - |     - |     1736 B |
|                      EasyCaching_InMemory |          1 |        10,078.5 ns |        62.07 ns |        58.06 ns |     3.86 |    0.03 |     1.3580 |          - |     - |     4281 B |
|         CacheManager_MicrosoftMemoryCache |          1 |        19,198.9 ns |       100.04 ns |        88.68 ns |     7.36 |    0.06 |     2.4719 |     1.2207 |     - |     7848 B |
|                 EasyCaching_InMemoryAsync |          1 |        25,209.6 ns |       491.28 ns |       638.80 ns |     9.67 |    0.24 |     2.0142 |          - |     - |     6266 B |
|                         Akavache_InMemory |          1 |     1,346,591.5 ns |     7,072.73 ns |     6,615.83 ns |   516.01 |    5.51 |    19.5313 |     9.7656 |     - |    66169 B |
|                                           |            |                    |                 |                 |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer_Direct |        100 |        43,656.3 ns |       266.85 ns |       249.61 ns |     0.48 |    0.00 |     1.4038 |          - |     - |     4472 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |        100 |        90,679.2 ns |       447.10 ns |       418.22 ns |     1.00 |    0.00 |    14.4043 |          - |     - |    45296 B |
|                  LazyCache_MemoryProvider |        100 |       176,236.9 ns |     1,424.50 ns |     1,262.78 ns |     1.94 |    0.02 |    33.9355 |          - |     - |   106640 B |
|             LazyCache_MemoryProviderAsync |        100 |       194,498.3 ns |     1,237.00 ns |     1,096.57 ns |     2.15 |    0.01 |    40.7715 |          - |     - |   128240 B |
|         CacheManager_MicrosoftMemoryCache |        100 |       213,216.2 ns |     2,130.58 ns |     1,779.13 ns |     2.35 |    0.02 |    10.9863 |     3.6621 |     - |    34789 B |
|                      EasyCaching_InMemory |        100 |       467,129.9 ns |     3,353.38 ns |     3,136.76 ns |     5.15 |    0.04 |    35.6445 |          - |     - |   112806 B |
|                 EasyCaching_InMemoryAsync |        100 |       909,292.0 ns |    10,709.25 ns |    10,017.44 ns |    10.03 |    0.13 |    66.4063 |          - |     - |   210692 B |
|                         Akavache_InMemory |        100 |   126,858,307.1 ns | 2,506,590.77 ns | 4,583,442.95 ns | 1,440.78 |   50.78 |  2000.0000 |  1000.0000 |     - |  6585576 B |
|                                           |            |                    |                 |                 |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer_Direct |       1000 |       412,113.5 ns |     2,554.85 ns |     2,389.81 ns |     0.47 |    0.00 |    12.6953 |          - |     - |    40472 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |       1000 |       881,748.3 ns |     7,727.92 ns |     6,453.17 ns |     1.00 |    0.00 |   140.6250 |          - |     - |   441296 B |
|                  LazyCache_MemoryProvider |       1000 |     1,787,082.6 ns |     6,151.78 ns |     5,137.02 ns |     2.03 |    0.01 |   337.8906 |          - |     - |  1064241 B |
|         CacheManager_MicrosoftMemoryCache |       1000 |     1,874,094.6 ns |    15,057.51 ns |    13,348.09 ns |     2.13 |    0.02 |    87.8906 |          - |     - |   279714 B |
|             LazyCache_MemoryProviderAsync |       1000 |     1,984,201.9 ns |    19,066.82 ns |    17,835.12 ns |     2.25 |    0.03 |   406.2500 |          - |     - |  1280245 B |
|                      EasyCaching_InMemory |       1000 |     4,670,012.8 ns |    30,198.01 ns |    28,247.23 ns |     5.29 |    0.04 |   343.7500 |          - |     - |  1099405 B |
|                 EasyCaching_InMemoryAsync |       1000 |     8,388,602.9 ns |   107,537.46 ns |   100,590.60 ns |     9.51 |    0.11 |   656.2500 |          - |     - |  2068652 B |
|                         Akavache_InMemory |       1000 | 1,385,010,306.7 ns | 6,051,655.26 ns | 5,660,722.08 ns | 1,570.06 |   11.90 | 20000.0000 | 10000.0000 |     - | 65753656 B |

## File Caching

|                            Method | Iterations |         Mean |      Error |     StdDev | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|---------------------------------- |----------- |-------------:|-----------:|-----------:|------:|--------:|-----------:|------:|------:|------------:|
|     CacheTower_JsonFileCacheLayer |          1 |     5.644 ms |  0.1909 ms |  0.7851 ms |  1.00 |    0.00 |          - |     - |     - |    56.77 KB |
| CacheTower_ProtobufFileCacheLayer |          1 |     4.695 ms |  0.1735 ms |  0.7194 ms |  0.85 |    0.16 |          - |     - |     - |    24.53 KB |
|             MonkeyCache_FileStore |          1 |     5.168 ms |  0.1727 ms |  0.7063 ms |  0.93 |    0.18 |          - |     - |     - |    65.81 KB |
|                  EasyCaching_Disk |          1 |     4.128 ms |  0.1357 ms |  0.5699 ms |  0.75 |    0.14 |          - |     - |     - |    38.03 KB |
|                                   |            |              |            |            |       |         |            |       |       |             |
|     CacheTower_JsonFileCacheLayer |        100 |   128.335 ms |  2.5653 ms |  9.2281 ms |  1.00 |    0.00 |          - |     - |     - |  2845.21 KB |
| CacheTower_ProtobufFileCacheLayer |        100 |    87.715 ms |  1.7388 ms |  3.8531 ms |  0.67 |    0.05 |          - |     - |     - |  1093.86 KB |
|             MonkeyCache_FileStore |        100 |   159.282 ms |  3.1349 ms |  5.3233 ms |  1.20 |    0.09 |  1000.0000 |     - |     - |  4379.57 KB |
|                  EasyCaching_Disk |        100 |    98.035 ms |  2.0259 ms |  4.1838 ms |  0.75 |    0.05 |          - |     - |     - |  1766.05 KB |
|                                   |            |              |            |            |       |         |            |       |       |             |
|     CacheTower_JsonFileCacheLayer |       1000 | 1,226.926 ms | 23.2123 ms | 23.8373 ms |  1.00 |    0.00 |  9000.0000 |     - |     - |  28216.2 KB |
| CacheTower_ProtobufFileCacheLayer |       1000 |   839.174 ms |  6.4918 ms |  5.7548 ms |  0.68 |    0.01 |  2000.0000 |     - |     - | 10833.09 KB |
|             MonkeyCache_FileStore |       1000 | 1,568.234 ms | 31.2706 ms | 30.7119 ms |  1.28 |    0.03 | 14000.0000 |     - |     - | 43597.98 KB |
|                  EasyCaching_Disk |       1000 |   975.009 ms | 19.0377 ms | 23.3801 ms |  0.79 |    0.02 |  5000.0000 |     - |     - | 17561.49 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|----------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.501 ms | 0.0712 ms | 0.2912 ms |   1.516 ms |  1.00 |    0.00 |         - |         - |     - |    8.23 KB |
|         CacheManager_Redis |          1 |   2.237 ms | 0.0847 ms | 0.3523 ms |   2.273 ms |  1.55 |    0.42 |         - |         - |     - |   27.32 KB |
|          EasyCaching_Redis |          1 |   4.075 ms | 0.1169 ms | 0.4858 ms |   4.005 ms |  2.85 |    0.76 |         - |         - |     - |  509.45 KB |
|                            |            |            |           |           |            |       |         |           |           |       |            |
| CacheTower_RedisCacheLayer |        100 |  42.882 ms | 0.7538 ms | 0.6682 ms |  42.820 ms |  1.00 |    0.00 |         - |         - |     - |  432.07 KB |
|         CacheManager_Redis |        100 |  41.358 ms | 0.8259 ms | 2.2328 ms |  41.335 ms |  1.03 |    0.04 |         - |         - |     - |  554.92 KB |
|          EasyCaching_Redis |        100 |  41.563 ms | 0.8297 ms | 2.1857 ms |  40.822 ms |  1.03 |    0.07 |         - |         - |     - |  851.61 KB |
|                            |            |            |           |           |            |       |         |           |           |       |            |
| CacheTower_RedisCacheLayer |       1000 | 366.280 ms | 1.0475 ms | 0.8747 ms | 366.247 ms |  1.00 |    0.00 |         - |         - |     - | 3575.61 KB |
|         CacheManager_Redis |       1000 | 392.528 ms | 2.2638 ms | 2.0068 ms | 392.768 ms |  1.07 |    0.01 | 1000.0000 |         - |     - | 5353.37 KB |
|          EasyCaching_Redis |       1000 | 376.162 ms | 2.1069 ms | 1.8677 ms | 376.136 ms |  1.03 |    0.01 | 1000.0000 | 1000.0000 |     - | 3999.44 KB |