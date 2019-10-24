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
|             CacheManager_DictionaryHandle |            1 |          1 |       102,874.3 ns |      6,854.03 ns |     28,031.89 ns |        96,600.0 ns |        ? |       ? |          - |          - |     - |     6936 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |          1 |         2,363.6 ns |         36.78 ns |         34.40 ns |         2,378.7 ns |     1.00 |    0.00 |     0.5531 |          - |     - |     1736 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |          1 |           725.1 ns |         12.86 ns |         12.03 ns |           720.5 ns |     0.31 |    0.01 |     0.1631 |          - |     - |      512 B |
|         CacheManager_MicrosoftMemoryCache |           16 |          1 |        17,813.4 ns |        134.67 ns |        125.97 ns |        17,782.5 ns |     7.54 |    0.14 |     2.4719 |     1.2207 |     - |     7848 B |
|                         Akavache_InMemory |           16 |          1 |     1,228,004.4 ns |     11,708.51 ns |     10,952.15 ns |     1,231,932.4 ns |   519.66 |   10.06 |    19.5313 |     9.7656 |     - |    65451 B |
|                      EasyCaching_InMemory |           16 |          1 |         9,362.9 ns |         67.57 ns |         59.90 ns |         9,363.1 ns |     3.96 |    0.07 |     1.3580 |          - |     - |     4281 B |
|                 EasyCaching_InMemoryAsync |           16 |          1 |        25,167.8 ns |        499.17 ns |        554.82 ns |        25,043.6 ns |    10.61 |    0.28 |     2.0142 |          - |     - |     3417 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|             CacheManager_DictionaryHandle |            1 |        100 |       225,140.1 ns |     12,088.33 ns |     48,613.62 ns |       213,450.0 ns |        ? |       ? |          - |          - |     - |    33864 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |        100 |        84,938.1 ns |      1,595.96 ns |      1,492.86 ns |        84,345.2 ns |     1.00 |    0.00 |    14.4043 |          - |     - |    45296 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |        100 |        39,133.5 ns |        621.74 ns |        551.15 ns |        39,317.7 ns |     0.46 |    0.01 |     1.4038 |          - |     - |     4472 B |
|         CacheManager_MicrosoftMemoryCache |           16 |        100 |       193,887.9 ns |      1,778.82 ns |      1,663.90 ns |       193,913.6 ns |     2.28 |    0.04 |    10.9863 |     3.6621 |     - |    34790 B |
|                         Akavache_InMemory |           16 |        100 |   121,686,116.7 ns |  1,898,708.54 ns |  1,776,053.15 ns |   121,397,200.0 ns | 1,433.13 |   36.12 |  2000.0000 |  1000.0000 |     - |  6506536 B |
|                      EasyCaching_InMemory |           16 |        100 |       444,132.5 ns |      6,894.46 ns |      6,111.76 ns |       446,411.0 ns |     5.23 |    0.12 |    35.6445 |          - |     - |   112806 B |
|                 EasyCaching_InMemoryAsync |           16 |        100 |       878,805.1 ns |     10,523.32 ns |      9,843.52 ns |       881,458.8 ns |    10.35 |    0.15 |    66.4063 |          - |     - |     3469 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|             CacheManager_DictionaryHandle |            1 |       1000 |     1,165,418.1 ns |     52,424.46 ns |    221,396.01 ns |     1,136,200.0 ns |        ? |       ? |          - |          - |     - |   278664 B |
|                                           |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
| CacheTower_MemoryCacheLayer_ViaCacheStack |           16 |       1000 |       837,542.4 ns |     11,060.61 ns |      9,236.11 ns |       835,549.2 ns |     1.00 |    0.00 |   140.6250 |          - |     - |   441296 B |
|        CacheTower_MemoryCacheLayer_Direct |           16 |       1000 |       397,780.1 ns |      5,732.45 ns |      5,362.13 ns |       399,531.2 ns |     0.48 |    0.01 |    12.6953 |          - |     - |    40472 B |
|         CacheManager_MicrosoftMemoryCache |           16 |       1000 |     1,802,808.2 ns |      8,927.00 ns |      6,969.61 ns |     1,802,693.6 ns |     2.16 |    0.02 |    87.8906 |          - |     - |   279715 B |
|                         Akavache_InMemory |           16 |       1000 | 1,256,349,207.1 ns | 16,880,526.42 ns | 14,964,146.98 ns | 1,255,431,900.0 ns | 1,502.75 |   24.24 | 20000.0000 | 10000.0000 |     - | 64996696 B |
|                      EasyCaching_InMemory |           16 |       1000 |     4,377,197.0 ns |     72,426.01 ns |     67,747.34 ns |     4,397,218.0 ns |     5.23 |    0.09 |   343.7500 |          - |     - |  1099395 B |
|                 EasyCaching_InMemoryAsync |           16 |       1000 |     8,253,563.5 ns |     89,285.10 ns |     83,517.34 ns |     8,251,742.2 ns |     9.85 |    0.13 |   656.2500 |          - |     - |     3473 B |

## File Caching

|                            Method | Iterations |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|---------------------------------- |----------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|------:|------:|------------:|
|     CacheTower_JsonFileCacheLayer |          1 |     4.975 ms |  0.1135 ms |  0.4640 ms |     4.919 ms |  1.00 |    0.00 |          - |     - |     - |    13.79 KB |
| CacheTower_ProtobufFileCacheLayer |          1 |     4.062 ms |  0.1039 ms |  0.4214 ms |     3.934 ms |  0.82 |    0.11 |          - |     - |     - |     9.82 KB |
|             MonkeyCache_FileStore |          1 |     4.605 ms |  0.1015 ms |  0.4127 ms |     4.596 ms |  0.93 |    0.12 |          - |     - |     - |    65.81 KB |
|                  EasyCaching_Disk |          1 |     3.720 ms |  0.1068 ms |  0.4453 ms |     3.652 ms |  0.76 |    0.11 |          - |     - |     - |    24.94 KB |
|                                   |            |              |            |            |              |       |         |            |       |       |             |
|     CacheTower_JsonFileCacheLayer |        100 |   122.376 ms |  2.4451 ms |  6.5265 ms |   121.047 ms |  1.00 |    0.00 |          - |     - |     - |    13.37 KB |
| CacheTower_ProtobufFileCacheLayer |        100 |    84.165 ms |  1.6597 ms |  3.2761 ms |    82.949 ms |  0.68 |    0.05 |          - |     - |     - |     9.33 KB |
|             MonkeyCache_FileStore |        100 |   155.872 ms |  3.0938 ms |  4.7245 ms |   155.691 ms |  1.24 |    0.08 |  1000.0000 |     - |     - |  4379.57 KB |
|                  EasyCaching_Disk |        100 |    96.196 ms |  1.9459 ms |  4.5098 ms |    95.281 ms |  0.78 |    0.06 |          - |     - |     - |    24.59 KB |
|                                   |            |              |            |            |              |       |         |            |       |       |             |
|     CacheTower_JsonFileCacheLayer |       1000 | 1,152.253 ms | 22.5597 ms | 30.8799 ms | 1,162.070 ms |  1.00 |    0.00 |  9000.0000 |     - |     - |    13.37 KB |
| CacheTower_ProtobufFileCacheLayer |       1000 |   820.094 ms |  9.5298 ms |  8.9142 ms |   820.166 ms |  0.72 |    0.02 |  3000.0000 |     - |     - |     9.46 KB |
|             MonkeyCache_FileStore |       1000 | 1,566.567 ms | 16.8368 ms | 15.7491 ms | 1,573.903 ms |  1.37 |    0.03 | 14000.0000 |     - |     - | 43596.71 KB |
|                  EasyCaching_Disk |       1000 |   932.551 ms | 16.2090 ms | 14.3689 ms |   931.394 ms |  0.82 |    0.03 |  5000.0000 |     - |     - |     24.7 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.431 ms | 0.0856 ms | 0.2498 ms |   1.443 ms |  1.00 |    0.00 |         - |     - |     - |    2.34 KB |
|         CacheManager_Redis |          1 |   2.209 ms | 0.1170 ms | 0.3299 ms |   2.219 ms |  1.57 |    0.33 |         - |     - |     - |   25.41 KB |
|          EasyCaching_Redis |          1 |   3.646 ms | 0.1585 ms | 0.4549 ms |   3.602 ms |  2.63 |    0.54 |         - |     - |     - |    19.7 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |        100 |  39.128 ms | 0.7974 ms | 2.0439 ms |  38.350 ms |  1.00 |    0.00 |         - |     - |     - |    1.94 KB |
|         CacheManager_Redis |        100 |  42.298 ms | 0.8405 ms | 2.2867 ms |  42.185 ms |  1.08 |    0.06 |         - |     - |     - |  378.88 KB |
|          EasyCaching_Redis |        100 |  41.061 ms | 0.8201 ms | 2.1022 ms |  40.322 ms |  1.05 |    0.05 |         - |     - |     - |   18.77 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |       1000 | 366.997 ms | 1.7721 ms | 1.6576 ms | 366.749 ms |  1.00 |    0.00 | 1000.0000 |     - |     - |    2.07 KB |
|         CacheManager_Redis |       1000 | 389.769 ms | 2.7135 ms | 2.5382 ms | 390.026 ms |  1.06 |    0.01 | 1000.0000 |     - |     - | 3592.35 KB |
|          EasyCaching_Redis |       1000 | 367.802 ms | 3.9140 ms | 3.4696 ms | 367.685 ms |  1.00 |    0.01 | 1000.0000 |     - |     - |   19.36 KB |