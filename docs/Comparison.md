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

|                                    Method | UnrollFactor | Iterations |               Mean |            Error |           StdDev |             Median |    Ratio | RatioSD |      Gen 0 |      Gen 1 |  Gen 2 |  Allocated |
|------------------------------------------ |------------- |----------- |-------------------:|-----------------:|-----------------:|-------------------:|---------:|--------:|-----------:|-----------:|-------:|-----------:|
|             CacheManager_DictionaryHandle |            1 |          1 |        94,975.0 ns |      6,254.77 ns |     25,651.54 ns |        90,850.0 ns |        ? |       ? |          - |          - |      - |     6936 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |          1 |         2,378.6 ns |         36.03 ns |         33.70 ns |         2,393.0 ns |     1.00 |    0.00 |     0.5531 |          - |      - |     1736 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |          1 |           748.9 ns |         11.56 ns |         10.81 ns |           754.1 ns |     0.31 |    0.01 |     0.1631 |          - |      - |      512 B |
|         CacheManager_MicrosoftMemoryCache |           16 |          1 |        18,146.4 ns |        124.49 ns |        116.45 ns |        18,146.6 ns |     7.63 |    0.11 |     2.4719 |     1.2207 | 0.0305 |     7848 B |
|                         Akavache_InMemory |           16 |          1 |     1,269,868.7 ns |      9,311.71 ns |      8,254.58 ns |     1,269,408.1 ns |   534.91 |    5.19 |    19.5313 |     9.7656 |      - |    66168 B |
|                      EasyCaching_InMemory |           16 |          1 |         9,550.4 ns |        167.38 ns |        156.57 ns |         9,637.7 ns |     4.02 |    0.07 |     1.3580 |          - |      - |     4281 B |
|                 EasyCaching_InMemoryAsync |           16 |          1 |        24,614.1 ns |        476.42 ns |        652.13 ns |        24,397.6 ns |    10.38 |    0.27 |     2.0142 |          - |      - |     6261 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
|             CacheManager_DictionaryHandle |            1 |        100 |       229,333.5 ns |     13,921.62 ns |     56,145.86 ns |       213,950.0 ns |        ? |       ? |          - |          - |      - |    33864 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |        100 |        85,873.5 ns |        690.09 ns |        611.75 ns |        86,023.1 ns |     1.00 |    0.00 |    14.4043 |          - |      - |    45296 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |        100 |        37,470.2 ns |        698.89 ns |        653.74 ns |        37,761.5 ns |     0.44 |    0.01 |     1.4038 |          - |      - |     4472 B |
|         CacheManager_MicrosoftMemoryCache |           16 |        100 |       191,005.5 ns |      3,379.28 ns |      3,160.98 ns |       189,967.9 ns |     2.23 |    0.04 |    10.9863 |     3.6621 |      - |    34789 B |
|                         Akavache_InMemory |           16 |        100 |   126,701,933.3 ns |    940,769.15 ns |    879,996.05 ns |   126,595,650.0 ns | 1,476.18 |   18.49 |  2000.0000 |  1000.0000 |      - |  6577278 B |
|                      EasyCaching_InMemory |           16 |        100 |       435,219.8 ns |      7,804.62 ns |      6,918.59 ns |       438,705.5 ns |     5.07 |    0.08 |    35.6445 |          - |      - |   112806 B |
|                 EasyCaching_InMemoryAsync |           16 |        100 |       907,483.9 ns |     11,447.75 ns |     10,708.24 ns |       908,318.8 ns |    10.56 |    0.15 |    66.4063 |          - |      - |   210689 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
|             CacheManager_DictionaryHandle |            1 |       1000 |     1,166,243.7 ns |     56,778.04 ns |    238,537.00 ns |     1,160,500.0 ns |        ? |       ? |          - |          - |      - |   278664 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |       1000 |       832,764.9 ns |     11,178.85 ns |     10,456.70 ns |       834,564.6 ns |     1.00 |    0.00 |   140.6250 |          - |      - |   441296 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |       1000 |       373,502.2 ns |      6,804.68 ns |      6,365.10 ns |       376,248.9 ns |     0.45 |    0.01 |    12.6953 |          - |      - |    40472 B |
|         CacheManager_MicrosoftMemoryCache |           16 |       1000 |     1,762,839.0 ns |     22,509.17 ns |     21,055.09 ns |     1,771,745.5 ns |     2.12 |    0.04 |    87.8906 |          - |      - |   279715 B |
|                         Akavache_InMemory |           16 |       1000 | 1,297,092,021.4 ns | 13,802,942.69 ns | 12,235,949.17 ns | 1,296,223,000.0 ns | 1,558.22 |   25.58 | 20000.0000 | 10000.0000 |      - | 65745312 B |
|                      EasyCaching_InMemory |           16 |       1000 |     4,341,685.9 ns |     78,654.68 ns |     73,573.64 ns |     4,310,616.4 ns |     5.21 |    0.13 |   343.7500 |          - |      - |  1099400 B |
|                 EasyCaching_InMemoryAsync |           16 |       1000 |     8,265,603.4 ns |    113,291.75 ns |    105,973.17 ns |     8,284,434.4 ns |     9.93 |    0.14 |   656.2500 |          - |      - |  2068646 B |

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