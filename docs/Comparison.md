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
|        CacheTower_MemoryCacheLayer_Direct |          1 |           701.1 ns |          9.47 ns |          8.86 ns |  0.30 |    0.01 |      0.3052 |      - |     - |        960 B |
|                  LazyCache_MemoryProvider |          1 |         1,756.1 ns |         17.35 ns |         15.38 ns |  0.76 |    0.01 |      0.4139 |      - |     - |       1304 B |
|             LazyCache_MemoryProviderAsync |          1 |         2,019.4 ns |         17.05 ns |         15.12 ns |  0.88 |    0.01 |      0.4807 |      - |     - |       1520 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |          1 |         2,299.6 ns |         30.90 ns |         28.90 ns |  1.00 |    0.00 |      0.4997 |      - |     - |       1568 B |
|                      EasyCaching_InMemory |          1 |         9,735.8 ns |        172.42 ns |        161.28 ns |  4.23 |    0.09 |      1.3580 |      - |     - |       4281 B |
|         CacheManager_MicrosoftMemoryCache |          1 |        19,057.7 ns |        305.02 ns |        285.31 ns |  8.29 |    0.16 |      2.4719 | 1.2207 |     - |       7848 B |
|                 EasyCaching_InMemoryAsync |          1 |        27,157.6 ns |        535.62 ns |      1,006.03 ns | 11.90 |    0.50 |      2.0142 |      - |     - |       6267 B |
|                                           |            |                    |                  |                  |       |         |             |        |       |              |
|        CacheTower_MemoryCacheLayer_Direct |       1000 |       280,535.1 ns |      3,236.10 ns |      2,868.72 ns |  0.33 |    0.01 |     10.2539 |      - |     - |      32928 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |       1000 |       861,614.4 ns |      6,863.37 ns |      6,420.00 ns |  1.00 |    0.00 |     42.9688 |      - |     - |     137433 B |
|                  LazyCache_MemoryProvider |       1000 |     1,706,565.0 ns |     20,750.18 ns |     19,409.73 ns |  1.98 |    0.03 |    337.8906 |      - |     - |    1064243 B |
|         CacheManager_MicrosoftMemoryCache |       1000 |     1,831,263.0 ns |     21,845.59 ns |     20,434.38 ns |  2.13 |    0.03 |     87.8906 |      - |     - |     279716 B |
|             LazyCache_MemoryProviderAsync |       1000 |     1,869,017.2 ns |     25,313.27 ns |     21,137.73 ns |  2.17 |    0.03 |    406.2500 |      - |     - |    1280259 B |
|                      EasyCaching_InMemory |       1000 |     4,429,187.9 ns |     59,125.83 ns |     55,306.34 ns |  5.14 |    0.08 |    343.7500 |      - |     - |    1099400 B |
|                 EasyCaching_InMemoryAsync |       1000 |     8,049,749.6 ns |     97,886.67 ns |     91,563.25 ns |  9.34 |    0.12 |    656.2500 |      - |     - |    2068721 B |
|                                           |            |                    |                  |                  |       |         |             |        |       |              |
|        CacheTower_MemoryCacheLayer_Direct |    1000000 |   278,457,132.1 ns |  4,260,314.63 ns |  3,776,657.95 ns |  0.35 |    0.01 |  10000.0000 |      - |     - |   32001548 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |    1000000 |   806,479,780.0 ns | 13,639,605.45 ns | 12,758,495.40 ns |  1.00 |    0.00 |  43000.0000 |      - |     - |  136002320 B |
|                  LazyCache_MemoryProvider |    1000000 | 1,681,657,740.0 ns | 12,395,890.49 ns | 11,595,123.66 ns |  2.09 |    0.03 | 338000.0000 |      - |     - | 1064000240 B |
|         CacheManager_MicrosoftMemoryCache |    1000000 | 1,739,761,400.0 ns | 20,948,619.18 ns | 19,595,351.38 ns |  2.16 |    0.04 |  86000.0000 |      - |     - |  272010480 B |
|             LazyCache_MemoryProviderAsync |    1000000 | 1,893,998,260.0 ns | 26,179,232.58 ns | 24,488,070.40 ns |  2.35 |    0.03 | 407000.0000 |      - |     - | 1280000344 B |
|                      EasyCaching_InMemory |    1000000 | 4,378,900,528.6 ns | 57,203,648.95 ns | 50,709,544.81 ns |  5.44 |    0.12 | 349000.0000 |      - |     - | 1096213184 B |
|                 EasyCaching_InMemoryAsync |    1000000 | 7,855,221,933.3 ns | 53,236,829.45 ns | 49,797,763.30 ns |  9.74 |    0.18 | 663000.0000 |      - |     - | 2064420576 B |

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