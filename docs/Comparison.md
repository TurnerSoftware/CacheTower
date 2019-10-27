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

|                                    Method | Iterations |               Mean |            Error |           StdDev |    Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------------------------ |----------- |-------------------:|-----------------:|-----------------:|---------:|--------:|-----------:|-----------:|------:|-----------:|
|        CacheTower_MemoryCacheLayer_Direct |          1 |           758.2 ns |         12.00 ns |         11.22 ns |     0.32 |    0.00 |     0.1631 |          - |     - |      512 B |
|                  LazyCache_MemoryProvider |          1 |         1,698.7 ns |         17.63 ns |         16.49 ns |     0.72 |    0.01 |     0.4139 |          - |     - |     1304 B |
|             LazyCache_MemoryProviderAsync |          1 |         1,984.2 ns |         38.70 ns |         30.21 ns |     0.84 |    0.02 |     0.4845 |          - |     - |     1520 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |          1 |         2,355.6 ns |         23.82 ns |         21.12 ns |     1.00 |    0.00 |     0.3548 |          - |     - |     1120 B |
|                      EasyCaching_InMemory |          1 |         9,869.4 ns |        133.65 ns |        125.02 ns |     4.19 |    0.06 |     1.3580 |          - |     - |     4281 B |
|         CacheManager_MicrosoftMemoryCache |          1 |        18,324.3 ns |        201.38 ns |        188.37 ns |     7.78 |    0.12 |     2.4719 |     1.2207 |     - |     7848 B |
|                 EasyCaching_InMemoryAsync |          1 |        23,729.5 ns |        466.39 ns |        555.21 ns |    10.08 |    0.26 |     2.0142 |          - |     - |     6257 B |
|                         Akavache_InMemory |          1 |     1,297,026.2 ns |     10,505.78 ns |      9,827.12 ns |   550.27 |    4.88 |    19.5313 |     9.7656 |     - |    66232 B |
|                                           |            |                    |                  |                  |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer_Direct |        100 |        38,767.1 ns |        446.65 ns |        417.80 ns |     0.44 |    0.01 |     1.4038 |          - |     - |     4472 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |        100 |        87,286.9 ns |      1,434.07 ns |      1,341.43 ns |     1.00 |    0.00 |     4.8828 |          - |     - |    15376 B |
|                  LazyCache_MemoryProvider |        100 |       168,978.2 ns |      2,303.73 ns |      2,154.91 ns |     1.94 |    0.03 |    33.9355 |          - |     - |   106642 B |
|             LazyCache_MemoryProviderAsync |        100 |       189,229.9 ns |      2,746.71 ns |      2,293.63 ns |     2.17 |    0.04 |    40.7715 |          - |     - |   128240 B |
|         CacheManager_MicrosoftMemoryCache |        100 |       196,564.2 ns |      3,686.03 ns |      3,447.91 ns |     2.25 |    0.06 |    10.9863 |     3.6621 |     - |    34790 B |
|                      EasyCaching_InMemory |        100 |       451,180.2 ns |      6,187.16 ns |      5,166.56 ns |     5.18 |    0.09 |    35.6445 |          - |     - |   112806 B |
|                 EasyCaching_InMemoryAsync |        100 |       913,772.6 ns |     17,705.40 ns |     17,389.07 ns |    10.46 |    0.25 |    66.4063 |          - |     - |   210705 B |
|                         Akavache_InMemory |        100 |   128,480,626.8 ns |  1,157,242.01 ns |  1,025,864.90 ns | 1,474.27 |   22.51 |  2000.0000 |  1000.0000 |     - |  6576334 B |
|                                           |            |                    |                  |                  |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer_Direct |       1000 |       398,735.8 ns |      1,764.86 ns |      1,564.50 ns |     0.46 |    0.00 |    12.6953 |          - |     - |    40473 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |       1000 |       873,629.7 ns |      3,261.48 ns |      2,891.22 ns |     1.00 |    0.00 |    45.8984 |          - |     - |   144976 B |
|                  LazyCache_MemoryProvider |       1000 |     1,684,822.1 ns |     25,771.86 ns |     24,107.01 ns |     1.93 |    0.03 |   337.8906 |          - |     - |  1064243 B |
|         CacheManager_MicrosoftMemoryCache |       1000 |     1,809,405.6 ns |     25,329.40 ns |     23,693.13 ns |     2.07 |    0.03 |    87.8906 |          - |     - |   279715 B |
|             LazyCache_MemoryProviderAsync |       1000 |     1,856,364.0 ns |     27,768.92 ns |     25,975.07 ns |     2.12 |    0.03 |   406.2500 |          - |     - |  1280245 B |
|                      EasyCaching_InMemory |       1000 |     4,388,849.5 ns |     54,388.28 ns |     50,874.83 ns |     5.03 |    0.05 |   343.7500 |          - |     - |  1099399 B |
|                 EasyCaching_InMemoryAsync |       1000 |     8,220,673.5 ns |    102,349.65 ns |     95,737.93 ns |     9.41 |    0.12 |   656.2500 |          - |     - |  2068651 B |
|                         Akavache_InMemory |       1000 | 1,327,851,400.0 ns | 13,538,489.80 ns | 12,663,911.76 ns | 1,520.00 |   14.80 | 20000.0000 | 10000.0000 |     - | 65753656 B |

## File Caching

|                            Method | Iterations |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|---------------------------------- |----------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|------:|------:|------------:|
|                  EasyCaching_Disk |          1 |     4.161 ms |  0.1482 ms |  0.6258 ms |     4.098 ms |  0.72 |    0.15 |          - |     - |     - |    38.03 KB |
| CacheTower_ProtobufFileCacheLayer |          1 |     4.699 ms |  0.1536 ms |  0.6301 ms |     4.663 ms |  0.81 |    0.15 |          - |     - |     - |     24.7 KB |
|             MonkeyCache_FileStore |          1 |     5.418 ms |  0.1614 ms |  0.6657 ms |     5.364 ms |  0.93 |    0.17 |          - |     - |     - |    65.81 KB |
|     CacheTower_JsonFileCacheLayer |          1 |     5.907 ms |  0.1843 ms |  0.7560 ms |     5.752 ms |  1.00 |    0.00 |          - |     - |     - |     56.7 KB |
|                                   |            |              |            |            |              |       |         |            |       |       |             |
| CacheTower_ProtobufFileCacheLayer |        100 |    91.344 ms |  1.7985 ms |  3.1500 ms |    90.172 ms |  0.69 |    0.04 |          - |     - |     - |  1110.39 KB |
|                  EasyCaching_Disk |        100 |   100.655 ms |  1.9950 ms |  4.2516 ms |    99.923 ms |  0.76 |    0.05 |          - |     - |     - |  1766.05 KB |
|     CacheTower_JsonFileCacheLayer |        100 |   131.941 ms |  2.6051 ms |  6.3904 ms |   130.206 ms |  1.00 |    0.00 |          - |     - |     - |  2860.73 KB |
|             MonkeyCache_FileStore |        100 |   169.378 ms |  3.3031 ms |  3.6714 ms |   168.393 ms |  1.27 |    0.08 |  1000.0000 |     - |     - |  4379.57 KB |
|                                   |            |              |            |            |              |       |         |            |       |       |             |
| CacheTower_ProtobufFileCacheLayer |       1000 |   876.082 ms |  8.8316 ms |  8.2611 ms |   877.591 ms |  0.68 |    0.02 |  3000.0000 |     - |     - | 10989.37 KB |
|                  EasyCaching_Disk |       1000 |   977.346 ms | 15.0591 ms | 14.0863 ms |   980.300 ms |  0.76 |    0.02 |  5000.0000 |     - |     - | 17495.91 KB |
|     CacheTower_JsonFileCacheLayer |       1000 | 1,286.669 ms | 25.0659 ms | 38.2783 ms | 1,295.780 ms |  1.00 |    0.00 |  9000.0000 |     - |     - | 28370.11 KB |
|             MonkeyCache_FileStore |       1000 | 1,677.821 ms | 13.4943 ms | 12.6225 ms | 1,676.498 ms |  1.31 |    0.04 | 14000.0000 |     - |     - | 43596.71 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|----------:|------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.613 ms | 0.0718 ms | 0.2954 ms |  1.00 |    0.00 |         - |     - |     - |    8.74 KB |
|         CacheManager_Redis |          1 |   2.119 ms | 0.0772 ms | 0.3123 ms |  1.37 |    0.33 |         - |     - |     - |   27.34 KB |
|          EasyCaching_Redis |          1 |   4.264 ms | 0.1397 ms | 0.5821 ms |  2.75 |    0.65 |         - |     - |     - |  508.97 KB |
|                            |            |            |           |           |       |         |           |       |       |            |
|          EasyCaching_Redis |        100 |  41.823 ms | 0.8332 ms | 2.1508 ms |  1.05 |    0.04 |         - |     - |     - |  851.62 KB |
|         CacheManager_Redis |        100 |  42.825 ms | 0.8544 ms | 2.5983 ms |  1.08 |    0.06 |         - |     - |     - |  554.92 KB |
| CacheTower_RedisCacheLayer |        100 |  42.844 ms | 0.9575 ms | 0.8956 ms |  1.00 |    0.00 |         - |     - |     - |  463.46 KB |
|                            |            |            |           |           |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |       1000 | 374.554 ms | 1.9974 ms | 1.8683 ms |  1.00 |    0.00 | 1000.0000 |     - |     - | 3722.93 KB |
|          EasyCaching_Redis |       1000 | 376.346 ms | 3.2294 ms | 3.0208 ms |  1.00 |    0.01 | 1000.0000 |     - |     - |  3999.2 KB |
|         CacheManager_Redis |       1000 | 397.207 ms | 3.7284 ms | 3.3051 ms |  1.06 |    0.01 | 1000.0000 |     - |     - | 5351.55 KB |