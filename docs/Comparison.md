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
|        CacheTower_MemoryCacheLayer_Direct |          1 |           829.0 ns |         3.94 ns |         3.50 ns |     0.38 |    0.00 |     0.1631 |          - |     - |      512 B |
|                  LazyCache_MemoryProvider |          1 |         1,796.9 ns |        15.46 ns |        13.70 ns |     0.82 |    0.01 |     0.4139 |          - |     - |     1304 B |
|             LazyCache_MemoryProviderAsync |          1 |         2,034.3 ns |        18.94 ns |        17.71 ns |     0.93 |    0.01 |     0.4845 |          - |     - |     1520 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |          1 |         2,197.8 ns |        19.40 ns |        18.15 ns |     1.00 |    0.00 |     0.5188 |          - |     - |     1632 B |
|                      EasyCaching_InMemory |          1 |        10,317.6 ns |        50.98 ns |        47.69 ns |     4.69 |    0.03 |     1.3580 |          - |     - |     4281 B |
|         CacheManager_MicrosoftMemoryCache |          1 |        18,947.9 ns |       109.43 ns |       102.36 ns |     8.62 |    0.08 |     2.4719 |     1.2207 |     - |     7848 B |
|                 EasyCaching_InMemoryAsync |          1 |        23,780.1 ns |       425.61 ns |       398.12 ns |    10.82 |    0.20 |     2.0142 |          - |     - |     6258 B |
|                         Akavache_InMemory |          1 |     1,344,837.9 ns |     8,827.20 ns |     8,256.96 ns |   611.94 |    6.48 |    19.5313 |     9.7656 |     - |    66169 B |
|                                           |            |                    |                 |                 |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer_Direct |        100 |        41,459.5 ns |       233.33 ns |       218.26 ns |     0.45 |    0.00 |     1.4038 |          - |     - |     4472 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |        100 |        92,160.5 ns |       729.80 ns |       609.42 ns |     1.00 |    0.00 |    14.4043 |          - |     - |    45192 B |
|                  LazyCache_MemoryProvider |        100 |       172,992.0 ns |     1,779.93 ns |     1,577.86 ns |     1.88 |    0.03 |    33.9355 |          - |     - |   106640 B |
|             LazyCache_MemoryProviderAsync |        100 |       198,018.9 ns |       962.31 ns |       853.06 ns |     2.15 |    0.02 |    40.7715 |          - |     - |   128240 B |
|         CacheManager_MicrosoftMemoryCache |        100 |       208,237.8 ns |     1,491.10 ns |     1,321.82 ns |     2.26 |    0.02 |    10.9863 |     3.6621 |     - |    34789 B |
|                      EasyCaching_InMemory |        100 |       467,834.8 ns |     2,548.40 ns |     2,383.77 ns |     5.08 |    0.04 |    35.6445 |          - |     - |   112806 B |
|                 EasyCaching_InMemoryAsync |        100 |       935,646.7 ns |    13,201.31 ns |    12,348.51 ns |    10.14 |    0.13 |    66.4063 |          - |     - |   210691 B |
|                         Akavache_InMemory |        100 |   133,486,176.8 ns | 1,032,571.68 ns |   915,347.90 ns | 1,448.51 |   14.78 |  2000.0000 |  1000.0000 |     - |  6577278 B |
|                                           |            |                    |                 |                 |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer_Direct |       1000 |       410,212.4 ns |     2,965.37 ns |     2,773.81 ns |     0.46 |    0.00 |    12.6953 |          - |     - |    40473 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |       1000 |       899,263.0 ns |     5,384.80 ns |     5,036.94 ns |     1.00 |    0.00 |   140.6250 |          - |     - |   441192 B |
|                  LazyCache_MemoryProvider |       1000 |     1,739,943.8 ns |    10,162.96 ns |     9,506.44 ns |     1.93 |    0.01 |   337.8906 |          - |     - |  1064240 B |
|         CacheManager_MicrosoftMemoryCache |       1000 |     1,912,899.5 ns |    15,118.94 ns |    14,142.26 ns |     2.13 |    0.02 |    87.8906 |          - |     - |   279715 B |
|             LazyCache_MemoryProviderAsync |       1000 |     1,966,197.6 ns |    12,363.41 ns |    11,564.74 ns |     2.19 |    0.02 |   406.2500 |          - |     - |  1280240 B |
|                      EasyCaching_InMemory |       1000 |     4,656,266.4 ns |    21,699.45 ns |    20,297.68 ns |     5.18 |    0.04 |   343.7500 |          - |     - |  1099395 B |
|                 EasyCaching_InMemoryAsync |       1000 |     8,359,266.5 ns |    90,245.19 ns |    75,358.82 ns |     9.31 |    0.08 |   656.2500 |          - |     - |  2068642 B |
|                         Akavache_InMemory |       1000 | 1,382,768,780.0 ns | 7,372,870.32 ns | 6,896,587.47 ns | 1,537.72 |   12.22 | 20000.0000 | 10000.0000 |     - | 65745312 B |

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