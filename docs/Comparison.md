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

|                                    Method | UnrollFactor | Iterations |               Mean |            Error |           StdDev |             Median |    Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------------------------ |------------- |----------- |-------------------:|-----------------:|-----------------:|-------------------:|---------:|--------:|-----------:|-----------:|------:|-----------:|
|             CacheManager_DictionaryHandle |            1 |          1 |       101,948.9 ns |      7,119.80 ns |     28,876.72 ns |        97,400.0 ns |        ? |       ? |          - |          - |     - |     6936 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |          1 |         2,380.7 ns |         47.48 ns |         50.80 ns |         2,350.8 ns |     1.00 |    0.00 |     0.5531 |          - |     - |     1736 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |          1 |           747.8 ns |          8.87 ns |          7.86 ns |           748.4 ns |     0.31 |    0.01 |     0.1631 |          - |     - |      512 B |
|         CacheManager_MicrosoftMemoryCache |           16 |          1 |        17,362.4 ns |         79.21 ns |         70.22 ns |        17,348.6 ns |     7.31 |    0.16 |     2.4719 |     1.2207 |     - |     7848 B |
|                         Akavache_InMemory |           16 |          1 |     1,279,184.1 ns |     25,094.78 ns |     24,646.43 ns |     1,273,521.5 ns |   536.74 |   16.36 |    19.5313 |     9.7656 |     - |    66211 B |
|                      EasyCaching_InMemory |           16 |          1 |         9,337.9 ns |        161.75 ns |        151.30 ns |         9,427.1 ns |     3.92 |    0.11 |     1.3580 |          - |     - |     4281 B |
|                 EasyCaching_InMemoryAsync |           16 |          1 |        22,590.8 ns |        445.59 ns |        495.27 ns |        22,494.1 ns |     9.49 |    0.22 |     2.0142 |          - |     - |     6255 B |
|                  LazyCache_MemoryProvider |           16 |          1 |         1,708.6 ns |         26.82 ns |         25.09 ns |         1,715.0 ns |     0.72 |    0.02 |     0.4139 |          - |     - |     1304 B |
|             LazyCache_MemoryProviderAsync |           16 |          1 |         1,932.7 ns |         38.39 ns |         39.43 ns |         1,952.5 ns |     0.81 |    0.03 |     0.4845 |          - |     - |     1520 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|             CacheManager_DictionaryHandle |            1 |        100 |       230,153.8 ns |     14,050.08 ns |     57,144.47 ns |       218,250.0 ns |        ? |       ? |          - |          - |     - |    33864 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |        100 |        84,846.1 ns |      1,679.18 ns |      1,649.18 ns |        84,110.3 ns |     1.00 |    0.00 |    14.4043 |          - |     - |    45297 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |        100 |        37,250.0 ns |        708.67 ns |        787.69 ns |        37,390.3 ns |     0.44 |    0.01 |     1.4038 |          - |     - |     4472 B |
|         CacheManager_MicrosoftMemoryCache |           16 |        100 |       191,768.1 ns |      3,620.24 ns |      3,386.37 ns |       193,383.7 ns |     2.26 |    0.06 |    10.9863 |     3.6621 |     - |    34789 B |
|                         Akavache_InMemory |           16 |        100 |   123,695,475.0 ns |  1,600,868.13 ns |  1,497,453.04 ns |   123,656,450.0 ns | 1,460.57 |   32.76 |  2000.0000 |  1000.0000 |     - |  6577278 B |
|                      EasyCaching_InMemory |           16 |        100 |       426,539.2 ns |      4,349.09 ns |      3,631.69 ns |       425,248.9 ns |     5.04 |    0.12 |    35.6445 |          - |     - |   112806 B |
|                 EasyCaching_InMemoryAsync |           16 |        100 |       910,037.1 ns |     16,143.48 ns |     15,100.62 ns |       911,232.6 ns |    10.75 |    0.26 |    66.4063 |          - |     - |   210689 B |
|                  LazyCache_MemoryProvider |           16 |        100 |       159,493.9 ns |      3,010.58 ns |      2,668.80 ns |       160,328.1 ns |     1.89 |    0.04 |    33.9355 |          - |     - |   106640 B |
|             LazyCache_MemoryProviderAsync |           16 |        100 |       180,006.1 ns |      1,511.01 ns |      1,339.47 ns |       179,538.8 ns |     2.13 |    0.04 |    40.7715 |          - |     - |   128240 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|             CacheManager_DictionaryHandle |            1 |       1000 |     1,140,777.8 ns |     51,845.44 ns |    218,383.15 ns |     1,089,150.0 ns |        ? |       ? |          - |          - |     - |   278664 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |       1000 |       794,296.9 ns |     15,185.57 ns |     16,248.39 ns |       784,814.0 ns |     1.00 |    0.00 |   140.6250 |          - |     - |   441296 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |       1000 |       378,342.7 ns |      4,539.92 ns |      4,024.52 ns |       379,493.7 ns |     0.47 |    0.01 |    12.6953 |          - |     - |    40472 B |
|         CacheManager_MicrosoftMemoryCache |           16 |       1000 |     1,735,839.3 ns |     21,393.55 ns |     20,011.54 ns |     1,741,060.4 ns |     2.18 |    0.05 |    87.8906 |          - |     - |   279715 B |
|                         Akavache_InMemory |           16 |       1000 | 1,262,692,113.3 ns | 20,328,759.96 ns | 19,015,534.68 ns | 1,267,469,300.0 ns | 1,585.05 |   31.42 | 20000.0000 | 10000.0000 |     - | 65753224 B |
|                      EasyCaching_InMemory |           16 |       1000 |     4,312,032.8 ns |     74,329.49 ns |     65,891.16 ns |     4,325,863.3 ns |     5.41 |    0.16 |   343.7500 |          - |     - |  1099399 B |
|                 EasyCaching_InMemoryAsync |           16 |       1000 |     7,605,286.5 ns |     98,221.00 ns |     91,875.98 ns |     7,596,018.8 ns |     9.55 |    0.26 |   656.2500 |          - |     - |  2068654 B |
|                  LazyCache_MemoryProvider |           16 |       1000 |     1,582,980.9 ns |     28,400.94 ns |     26,566.25 ns |     1,595,474.2 ns |     1.99 |    0.05 |   337.8906 |          - |     - |  1064243 B |
|             LazyCache_MemoryProviderAsync |           16 |       1000 |     1,783,911.1 ns |      9,608.12 ns |      8,987.44 ns |     1,784,143.9 ns |     2.24 |    0.04 |   406.2500 |          - |     - |  1280259 B |

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