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

|                             Method | UnrollFactor | Iterations |               Mean |            Error |           StdDev |             Median |    Ratio | RatioSD |      Gen 0 |      Gen 1 |  Gen 2 |  Allocated |
|----------------------------------- |------------- |----------- |-------------------:|-----------------:|-----------------:|-------------------:|---------:|--------:|-----------:|-----------:|-------:|-----------:|
|      CacheManager_DictionaryHandle |            1 |          1 |        94,993.5 ns |      5,682.45 ns |     23,111.64 ns |        93,100.0 ns |        ? |       ? |          - |          - |      - |     6936 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
|        CacheTower_MemoryCacheLayer |           16 |          1 |         2,519.4 ns |         36.70 ns |         34.33 ns |         2,527.4 ns |     1.00 |    0.00 |     0.7286 |          - |      - |     2296 B |
|  CacheManager_MicrosoftMemoryCache |           16 |          1 |        17,253.1 ns |        199.49 ns |        186.61 ns |        17,312.9 ns |     6.85 |    0.12 |     2.4719 |     1.2207 | 0.0305 |     7848 B |
|                  Akavache_InMemory |           16 |          1 |     1,221,610.0 ns |     14,584.16 ns |     13,642.03 ns |     1,218,822.9 ns |   484.95 |    7.53 |    19.5313 |     9.7656 |      - |    65451 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
|      CacheManager_DictionaryHandle |            1 |        100 |       213,070.7 ns |     10,711.59 ns |     42,830.36 ns |       206,050.0 ns |        ? |       ? |          - |          - |      - |    33864 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
|        CacheTower_MemoryCacheLayer |           16 |        100 |        83,600.9 ns |      1,624.69 ns |      1,668.44 ns |        84,408.1 ns |     1.00 |    0.00 |    14.5264 |          - |      - |    45856 B |
|  CacheManager_MicrosoftMemoryCache |           16 |        100 |       189,094.7 ns |        875.95 ns |        731.46 ns |       189,317.2 ns |     2.26 |    0.05 |    10.9863 |     3.6621 |      - |    34789 B |
|                  Akavache_InMemory |           16 |        100 |   120,265,656.7 ns |  1,606,334.01 ns |  1,502,565.83 ns |   120,450,925.0 ns | 1,442.08 |   33.35 |  2000.0000 |  1000.0000 |      - |  6506536 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
|      CacheManager_DictionaryHandle |            1 |       1000 |     1,157,168.8 ns |     57,037.70 ns |    240,878.38 ns |     1,109,200.0 ns |        ? |       ? |          - |          - |      - |   278664 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |        |            |
|        CacheTower_MemoryCacheLayer |           16 |       1000 |       815,082.7 ns |     16,042.87 ns |     19,702.08 ns |       816,285.3 ns |     1.00 |    0.00 |   140.6250 |          - |      - |   441856 B |
|  CacheManager_MicrosoftMemoryCache |           16 |       1000 |     1,689,425.7 ns |     33,436.61 ns |     31,276.63 ns |     1,675,673.2 ns |     2.09 |    0.05 |    87.8906 |          - |      - |   279715 B |
|                  Akavache_InMemory |           16 |       1000 | 1,241,049,440.0 ns | 14,223,316.09 ns | 13,304,498.69 ns | 1,242,759,100.0 ns | 1,534.82 |   48.25 | 20000.0000 | 10000.0000 |      - | 64971736 B |

# JSON File Caching

|                        Method | Iterations |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |   Allocated |
|------------------------------ |----------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|-----------:|------:|------------:|
| CacheTower_JsonFileCacheLayer |          1 |     6.034 ms |  0.1816 ms |  0.7447 ms |     5.959 ms |  1.00 |    0.00 |          - |          - |     - |    14.42 KB |
|         MonkeyCache_FileStore |          1 |     5.205 ms |  0.1518 ms |  0.6295 ms |     5.152 ms |  0.87 |    0.14 |          - |          - |     - |    65.81 KB |
|         Akavache_LocalMachine |          1 |     1.884 ms |  0.0630 ms |  0.2578 ms |     1.825 ms |  0.32 |    0.06 |          - |          - |     - |    81.07 KB |
|                               |            |              |            |            |              |       |         |            |            |       |             |
| CacheTower_JsonFileCacheLayer |        100 |   131.593 ms |  2.6266 ms | 10.6830 ms |   130.260 ms |  1.00 |    0.00 |          - |          - |     - |       14 KB |
|         MonkeyCache_FileStore |        100 |   159.427 ms |  3.1417 ms |  5.3348 ms |   157.128 ms |  1.20 |    0.12 |  1000.0000 |          - |     - |  4379.57 KB |
|         Akavache_LocalMachine |        100 |   115.571 ms |  2.3326 ms |  4.9710 ms |   113.956 ms |  0.87 |    0.08 |  2000.0000 |  1000.0000 |     - |  6367.09 KB |
|                               |            |              |            |            |              |       |         |            |            |       |             |
| CacheTower_JsonFileCacheLayer |       1000 | 1,195.648 ms | 23.7437 ms | 34.8032 ms | 1,194.876 ms |  1.00 |    0.00 |  9000.0000 |          - |     - |       14 KB |
|         MonkeyCache_FileStore |       1000 | 1,620.753 ms | 32.2338 ms | 47.2478 ms | 1,613.729 ms |  1.36 |    0.05 | 14000.0000 |          - |     - | 43596.71 KB |
|         Akavache_LocalMachine |       1000 | 1,308.221 ms | 17.3054 ms | 16.1874 ms | 1,314.735 ms |  1.11 |    0.03 | 20000.0000 | 10000.0000 |     - | 63500.31 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.404 ms | 0.1187 ms | 0.3328 ms |   1.357 ms |  1.00 |    0.00 |         - |     - |     - |     4.2 KB |
|         CacheManager_Redis |          1 |   2.463 ms | 0.1577 ms | 0.4600 ms |   2.332 ms |  1.87 |    0.61 |         - |     - |     - |   22.67 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |        100 |  42.750 ms | 0.4372 ms | 0.3650 ms |  42.666 ms |  1.00 |    0.00 |         - |     - |     - |     4.2 KB |
|         CacheManager_Redis |        100 |  40.962 ms | 0.8096 ms | 2.0607 ms |  40.726 ms |  1.01 |    0.05 |         - |     - |     - |  399.34 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |       1000 | 376.129 ms | 2.6748 ms | 2.5021 ms | 375.899 ms |  1.00 |    0.00 | 1000.0000 |     - |     - |    3.93 KB |
|         CacheManager_Redis |       1000 | 391.740 ms | 3.1877 ms | 2.9817 ms | 390.573 ms |  1.04 |    0.01 | 1000.0000 |     - |     - | 3824.81 KB |