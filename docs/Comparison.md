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

|                                    Method | Iterations |               Mean |            Error |           StdDev |    Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------------------------ |----------- |-------------------:|-----------------:|-----------------:|---------:|--------:|-----------:|-----------:|------:|-----------:|
|        CacheTower_MemoryCacheLayer_Direct |          1 |           778.2 ns |         16.37 ns |         14.51 ns |     0.33 |    0.01 |     0.1574 |          - |     - |      496 B |
|                  LazyCache_MemoryProvider |          1 |         1,747.6 ns |         31.36 ns |         29.34 ns |     0.73 |    0.01 |     0.4139 |          - |     - |     1304 B |
|             LazyCache_MemoryProviderAsync |          1 |         2,048.3 ns |         11.69 ns |         10.36 ns |     0.86 |    0.01 |     0.4845 |          - |     - |     1520 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |          1 |         2,392.3 ns |         22.62 ns |         18.89 ns |     1.00 |    0.00 |     0.3510 |          - |     - |     1104 B |
|                      EasyCaching_InMemory |          1 |         9,788.1 ns |        139.27 ns |        130.28 ns |     4.09 |    0.07 |     1.3580 |          - |     - |     4281 B |
|         CacheManager_MicrosoftMemoryCache |          1 |        18,617.1 ns |        172.65 ns |        161.50 ns |     7.77 |    0.08 |     2.4719 |     1.2207 |     - |     7848 B |
|                 EasyCaching_InMemoryAsync |          1 |        25,383.4 ns |        503.14 ns |        654.23 ns |    10.70 |    0.28 |     2.0142 |          - |     - |     6260 B |
|                         Akavache_InMemory |          1 |     1,316,664.7 ns |     11,369.24 ns |     10,634.80 ns |   550.04 |    7.27 |    19.5313 |     9.7656 |     - |    66232 B |
|                                           |            |                    |                  |                  |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer_Direct |        100 |        39,798.7 ns |        425.38 ns |        377.09 ns |     0.44 |    0.01 |     1.1597 |          - |     - |     3664 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |        100 |        90,246.3 ns |      1,062.95 ns |        994.28 ns |     1.00 |    0.00 |     4.6387 |          - |     - |    14570 B |
|                  LazyCache_MemoryProvider |        100 |       172,256.9 ns |      2,014.73 ns |      1,884.58 ns |     1.91 |    0.02 |    33.9355 |          - |     - |   106641 B |
|             LazyCache_MemoryProviderAsync |        100 |       191,666.9 ns |      1,486.80 ns |      1,390.75 ns |     2.12 |    0.02 |    40.7715 |          - |     - |   128240 B |
|         CacheManager_MicrosoftMemoryCache |        100 |       204,438.9 ns |      3,591.56 ns |      3,359.54 ns |     2.27 |    0.05 |    10.9863 |     3.6621 |     - |    34790 B |
|                      EasyCaching_InMemory |        100 |       452,204.4 ns |      4,000.21 ns |      3,741.80 ns |     5.01 |    0.07 |    35.6445 |          - |     - |   112806 B |
|                 EasyCaching_InMemoryAsync |        100 |       903,208.0 ns |     17,237.22 ns |     18,443.63 ns |    10.01 |    0.25 |    66.4063 |          - |     - |   210689 B |
|                         Akavache_InMemory |        100 |   131,507,873.3 ns |  1,732,164.29 ns |  1,620,267.55 ns | 1,457.31 |   19.56 |  2000.0000 |  1000.0000 |     - |  6577278 B |
|                                           |            |                    |                  |                  |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer_Direct |       1000 |       405,720.8 ns |      7,902.60 ns |      8,115.39 ns |     0.45 |    0.01 |    10.2539 |          - |     - |    32464 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |       1000 |       893,106.3 ns |     16,676.90 ns |     16,378.95 ns |     1.00 |    0.00 |    42.9688 |          - |     - |   136968 B |
|                  LazyCache_MemoryProvider |       1000 |     1,830,634.8 ns |     35,334.35 ns |     37,807.36 ns |     2.05 |    0.04 |   337.8906 |          - |     - |  1064240 B |
|         CacheManager_MicrosoftMemoryCache |       1000 |     1,838,591.6 ns |     24,330.29 ns |     22,758.57 ns |     2.06 |    0.06 |    87.8906 |          - |     - |   279714 B |
|             LazyCache_MemoryProviderAsync |       1000 |     2,056,004.3 ns |     39,896.97 ns |     57,219.05 ns |     2.30 |    0.06 |   406.2500 |          - |     - |  1280241 B |
|                      EasyCaching_InMemory |       1000 |     4,985,703.7 ns |     99,385.27 ns |    218,153.07 ns |     5.82 |    0.33 |   343.7500 |          - |     - |  1099395 B |
|                 EasyCaching_InMemoryAsync |       1000 |     8,306,778.8 ns |    159,580.60 ns |    149,271.80 ns |     9.30 |    0.25 |   656.2500 |          - |     - |  2068651 B |
|                         Akavache_InMemory |       1000 | 1,326,507,671.4 ns | 18,398,324.62 ns | 16,309,635.56 ns | 1,484.44 |   34.87 | 20000.0000 | 10000.0000 |     - | 65745312 B |

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