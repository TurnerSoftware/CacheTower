# Comparison

The performance figures below are as a guide only. Different systems and configurations can drastically change performance results.
Run the benchmark suite on your target machine to best gauge how the performance is for your use case.

Keep in mind:
- The scale of time and the size of allocation differences between these libraries (some differences are so small that they are within a margin of error)
- The various libraries have different features, you would hopefully be choosing one not just because of its performance

If you are one of the maintainers of the libraries referenced below and you feel the test is unfair (eg. not properly configured), open a PR to fix it!

**Test Machine**

```
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  Job-CQNZSR : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT

Runtime=.NET Core 3.0  MaxIterationCount=200
UnrollFactor=1
```

## Memory Caching

|                                    Method | Iterations |               Mean |            Error |           StdDev | Ratio | RatioSD |       Gen 0 |  Gen 1 | Gen 2 |    Allocated |
|------------------------------------------ |----------- |-------------------:|-----------------:|-----------------:|------:|--------:|------------:|-------:|------:|-------------:|
|        CacheTower_MemoryCacheLayer_Direct |          1 |           795.3 ns |         12.23 ns |         11.44 ns |  0.33 |    0.01 |      0.1574 |      - |     - |        496 B |
|                  LazyCache_MemoryProvider |          1 |         1,740.0 ns |         27.46 ns |         25.69 ns |  0.72 |    0.01 |      0.4139 |      - |     - |       1304 B |
|             LazyCache_MemoryProviderAsync |          1 |         2,052.1 ns |         12.22 ns |         10.83 ns |  0.85 |    0.01 |      0.4845 |      - |     - |       1520 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |          1 |         2,407.9 ns |         21.61 ns |         20.22 ns |  1.00 |    0.00 |      0.3510 |      - |     - |       1104 B |
|                      EasyCaching_InMemory |          1 |         9,941.8 ns |        196.09 ns |        183.43 ns |  4.13 |    0.08 |      1.3580 |      - |     - |       4281 B |
|         CacheManager_MicrosoftMemoryCache |          1 |        18,799.0 ns |        372.29 ns |        365.64 ns |  7.81 |    0.17 |      2.4719 | 1.2207 |     - |       7848 B |
|                 EasyCaching_InMemoryAsync |          1 |        27,683.8 ns |        553.57 ns |        615.29 ns | 11.49 |    0.29 |      2.0142 |      - |     - |       6266 B |
|                                           |            |                    |                  |                  |       |         |             |        |       |              |
|        CacheTower_MemoryCacheLayer_Direct |       1000 |       430,786.6 ns |      6,016.08 ns |      5,627.44 ns |  0.48 |    0.01 |     10.2539 |      - |     - |      32465 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |       1000 |       896,728.5 ns |     14,853.17 ns |     13,893.66 ns |  1.00 |    0.00 |     42.9688 |      - |     - |     136968 B |
|                  LazyCache_MemoryProvider |       1000 |     1,693,478.7 ns |     28,667.29 ns |     26,815.40 ns |  1.89 |    0.04 |    337.8906 |      - |     - |    1064243 B |
|         CacheManager_MicrosoftMemoryCache |       1000 |     1,820,095.5 ns |     31,687.21 ns |     29,640.24 ns |  2.03 |    0.05 |     87.8906 |      - |     - |     279734 B |
|             LazyCache_MemoryProviderAsync |       1000 |     1,912,512.3 ns |     33,585.82 ns |     34,490.16 ns |  2.13 |    0.06 |    402.3438 |      - |     - |    1280240 B |
|                      EasyCaching_InMemory |       1000 |     4,414,280.7 ns |     54,222.64 ns |     50,719.89 ns |  4.92 |    0.11 |    343.7500 |      - |     - |    1099405 B |
|                 EasyCaching_InMemoryAsync |       1000 |     7,788,066.7 ns |    116,938.68 ns |    109,384.51 ns |  8.69 |    0.17 |    656.2500 |      - |     - |    2068649 B |
|                                           |            |                    |                  |                  |       |         |             |        |       |              |
|        CacheTower_MemoryCacheLayer_Direct |    1000000 |   393,689,757.1 ns |  6,382,979.38 ns |  5,658,344.96 ns |  0.43 |    0.01 |  10000.0000 |      - |     - |   32000464 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |    1000000 |   914,179,914.3 ns | 16,917,945.32 ns | 14,997,317.87 ns |  1.00 |    0.00 |  43000.0000 |      - |     - |  136001856 B |
|                  LazyCache_MemoryProvider |    1000000 | 1,698,096,420.0 ns | 15,115,066.25 ns | 14,138,642.35 ns |  1.86 |    0.04 | 339000.0000 |      - |     - | 1064000240 B |
|         CacheManager_MicrosoftMemoryCache |    1000000 | 1,779,357,233.3 ns | 20,503,848.31 ns | 19,179,312.43 ns |  1.95 |    0.04 |  86000.0000 |      - |     - |  272009128 B |
|             LazyCache_MemoryProviderAsync |    1000000 | 1,872,181,553.3 ns | 28,356,110.92 ns | 26,524,323.75 ns |  2.05 |    0.04 | 407000.0000 |      - |     - | 1280000344 B |
|                      EasyCaching_InMemory |    1000000 | 4,442,721,393.3 ns | 52,468,407.99 ns | 49,078,981.39 ns |  4.86 |    0.08 | 349000.0000 |      - |     - | 1096213184 B |
|                 EasyCaching_InMemoryAsync |    1000000 | 8,595,730,426.7 ns | 62,264,829.37 ns | 58,242,560.03 ns |  9.41 |    0.14 | 663000.0000 |      - |     - | 2064402152 B |

## File Caching

|                            Method | Iterations |         Mean |      Error |     StdDev | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|---------------------------------- |----------- |-------------:|-----------:|-----------:|------:|--------:|-----------:|------:|------:|------------:|
|                  EasyCaching_Disk |          1 |     4.466 ms |  0.1541 ms |  0.6508 ms |  0.73 |    0.14 |          - |     - |     - |    38.03 KB |
| CacheTower_ProtobufFileCacheLayer |          1 |     4.824 ms |  0.1557 ms |  0.6402 ms |  0.79 |    0.16 |          - |     - |     - |    22.66 KB |
|             MonkeyCache_FileStore |          1 |     5.790 ms |  0.1885 ms |  0.7773 ms |  0.95 |    0.18 |          - |     - |     - |    65.81 KB |
|     CacheTower_JsonFileCacheLayer |          1 |     6.180 ms |  0.2037 ms |  0.8353 ms |  1.00 |    0.00 |          - |     - |     - |    54.63 KB |
|                                   |            |              |            |            |       |         |            |       |       |             |
| CacheTower_ProtobufFileCacheLayer |        100 |    96.250 ms |  1.9033 ms |  3.3830 ms |  0.71 |    0.03 |          - |     - |     - |   1108.1 KB |
|                  EasyCaching_Disk |        100 |   111.801 ms |  2.2223 ms |  5.1945 ms |  0.83 |    0.05 |          - |     - |     - |  1766.05 KB |
|     CacheTower_JsonFileCacheLayer |        100 |   134.760 ms |  2.6579 ms |  5.9993 ms |  1.00 |    0.00 |          - |     - |     - |  2776.66 KB |
|             MonkeyCache_FileStore |        100 |   189.205 ms |  3.6147 ms |  3.8677 ms |  1.39 |    0.08 |  1000.0000 |     - |     - |  4379.57 KB |
|                                   |            |              |            |            |       |         |            |       |       |             |
| CacheTower_ProtobufFileCacheLayer |       1000 |   896.184 ms | 17.0967 ms | 16.7913 ms |  0.71 |    0.02 |  3000.0000 |     - |     - | 10967.19 KB |
|                  EasyCaching_Disk |       1000 | 1,084.950 ms | 21.6665 ms | 28.1726 ms |  0.85 |    0.02 |  5000.0000 |     - |     - | 17494.84 KB |
|     CacheTower_JsonFileCacheLayer |       1000 | 1,264.125 ms |  9.6589 ms |  8.5624 ms |  1.00 |    0.00 |  9000.0000 |     - |     - | 27543.12 KB |
|             MonkeyCache_FileStore |       1000 | 1,837.440 ms | 27.0019 ms | 25.2576 ms |  1.45 |    0.02 | 14000.0000 |     - |     - | 43596.71 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.392 ms | 0.0824 ms | 0.3399 ms |   1.356 ms |  1.00 |    0.00 |         - |     - |     - |    8.69 KB |
|         CacheManager_Redis |          1 |   2.441 ms | 0.1261 ms | 0.5270 ms |   2.445 ms |  1.86 |    0.61 |         - |     - |     - |   27.34 KB |
|          EasyCaching_Redis |          1 |   3.752 ms | 0.1574 ms | 0.6597 ms |   3.628 ms |  2.85 |    0.84 |         - |     - |     - |  509.45 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |        100 |  40.059 ms | 0.7942 ms | 2.3541 ms |  39.633 ms |  1.00 |    0.00 |         - |     - |     - |  381.15 KB |
|          EasyCaching_Redis |        100 |  41.750 ms | 0.8300 ms | 2.0046 ms |  41.330 ms |  1.04 |    0.06 |         - |     - |     - |  851.66 KB |
|         CacheManager_Redis |        100 |  46.709 ms | 0.9291 ms | 2.9088 ms |  46.250 ms |  1.17 |    0.08 |         - |     - |     - |  554.92 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |       1000 | 379.765 ms | 8.7609 ms | 8.9968 ms | 376.901 ms |  1.00 |    0.00 | 1000.0000 |     - |     - | 3660.41 KB |
|          EasyCaching_Redis |       1000 | 385.377 ms | 7.1510 ms | 6.6891 ms | 385.932 ms |  1.01 |    0.03 | 1000.0000 |     - |     - | 3999.08 KB |
|         CacheManager_Redis |       1000 | 432.212 ms | 6.8948 ms | 6.4494 ms | 432.085 ms |  1.14 |    0.03 | 1000.0000 |     - |     - | 5352.86 KB |