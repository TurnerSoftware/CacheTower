# Comparison

The performance figures below are as a guide only. Different systems and configurations can drastically change performance results.
Run the benchmark suite on your target machine to best gauge how the performance is for your use case.

Keep in mind:
- The scale of time and the size of allocation differences between these libraries (some differences are so small that they are within a margin of error)
- The various libraries have different features, you would hopefully be choosing one not just because of its performance

If you are one of the maintainers of the libraries referenced below and you feel the test is unfair (eg. not properly configured), open a PR to fix it!

**Test Machine**

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.685 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.101
  [Host]     : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT
  Job-CRREIG : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT

Runtime=.NET Core 3.1  MaxIterationCount=200
```

## Memory Caching

|                                    Method | Iterations |           Mean |         Error |        StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |  Gen 2 | Allocated |
|------------------------------------------ |----------- |---------------:|--------------:|--------------:|------:|--------:|---------:|---------:|-------:|----------:|
|        CacheTower_MemoryCacheLayer_Direct |          1 |       885.0 ns |      11.96 ns |      10.61 ns |  0.36 |    0.00 |   0.3052 |        - |      - |     960 B |
|                  LazyCache_MemoryProvider |          1 |     2,300.4 ns |      30.79 ns |      28.80 ns |  0.93 |    0.01 |   0.4730 |        - |      - |    1488 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |          1 |     2,485.3 ns |      20.51 ns |      17.13 ns |  1.00 |    0.00 |   0.4768 |        - |      - |    1496 B |
|             LazyCache_MemoryProviderAsync |          1 |     2,588.4 ns |      29.06 ns |      27.18 ns |  1.04 |    0.02 |   0.5417 |        - |      - |    1704 B |
|                      EasyCaching_InMemory |          1 |     9,539.3 ns |     101.22 ns |      94.68 ns |  3.84 |    0.05 |   1.3580 |        - |      - |    4289 B |
|         CacheManager_MicrosoftMemoryCache |          1 |    19,766.7 ns |     176.55 ns |     165.15 ns |  7.96 |    0.07 |   2.6245 |   1.3123 |      - |    8233 B |
|                 EasyCaching_InMemoryAsync |          1 |    24,658.6 ns |     463.18 ns |     410.59 ns |  9.94 |    0.18 |   2.0142 |        - |      - |    6352 B |
|                FusionCache_MemoryProvider |          1 |   103,763.4 ns |   2,051.76 ns |   2,195.36 ns | 41.78 |    1.06 |  73.4863 |  24.4141 |      - |  276037 B |
|           FusionCache_MemoryProviderAsync |          1 |   117,469.0 ns |     770.45 ns |     643.36 ns | 47.27 |    0.48 |  66.6504 |  23.0713 | 0.1221 |  276650 B |
|                                           |            |                |               |               |       |         |          |          |        |           |
|        CacheTower_MemoryCacheLayer_Direct |       1000 |   375,801.7 ns |   4,559.83 ns |   4,265.27 ns |  0.44 |    0.01 |  10.2539 |        - |      - |   32929 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |       1000 |   853,111.1 ns |   5,827.80 ns |   5,166.19 ns |  1.00 |    0.00 |   9.7656 |        - |      - |   33473 B |
|                  LazyCache_MemoryProvider |       1000 | 1,838,846.9 ns |  35,950.82 ns |  49,209.89 ns |  2.15 |    0.06 | 335.9375 |        - |      - | 1056432 B |
|         CacheManager_MicrosoftMemoryCache |       1000 | 1,888,184.9 ns |  22,452.01 ns |  21,001.63 ns |  2.21 |    0.03 |  87.8906 |        - |      - |  280124 B |
|             LazyCache_MemoryProviderAsync |       1000 | 2,012,515.3 ns |  35,843.18 ns |  55,803.52 ns |  2.39 |    0.08 | 402.3438 |        - |      - | 1272432 B |
|                      EasyCaching_InMemory |       1000 | 4,352,250.4 ns |  36,686.89 ns |  34,316.94 ns |  5.10 |    0.05 | 343.7500 |        - |      - | 1099433 B |
|                FusionCache_MemoryProvider |       1000 | 4,961,877.9 ns |  82,086.29 ns | 103,812.88 ns |  5.82 |    0.16 | 492.1875 | 234.3750 |      - | 2282142 B |
|           FusionCache_MemoryProviderAsync |       1000 | 5,798,224.7 ns | 115,828.55 ns | 108,346.10 ns |  6.80 |    0.13 | 609.3750 | 296.8750 |      - | 2898282 B |
|                 EasyCaching_InMemoryAsync |       1000 | 9,013,300.8 ns | 177,566.94 ns | 166,096.22 ns | 10.57 |    0.21 | 656.2500 |        - |      - | 2076721 B |

## File Caching

|                            Method | Iterations |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|---------------------------------- |----------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|------:|------:|------------:|
|                  EasyCaching_Disk |          1 |     4.017 ms |  0.0800 ms |  0.3143 ms |     4.081 ms |  0.65 |    0.10 |          - |     - |     - |    37.16 KB |
| CacheTower_ProtobufFileCacheLayer |          1 |     5.129 ms |  0.1625 ms |  0.6737 ms |     5.088 ms |  0.83 |    0.15 |          - |     - |     - |    23.93 KB |
|             MonkeyCache_FileStore |          1 |     6.006 ms |  0.1527 ms |  0.6383 ms |     5.955 ms |  0.98 |    0.16 |          - |     - |     - |    65.81 KB |
|     CacheTower_JsonFileCacheLayer |          1 |     6.268 ms |  0.1941 ms |  0.7962 ms |     6.203 ms |  1.00 |    0.00 |          - |     - |     - |    53.02 KB |
|                                   |            |              |            |            |              |       |         |            |       |       |             |
| CacheTower_ProtobufFileCacheLayer |        100 |    97.837 ms |  1.9485 ms |  3.5630 ms |    96.583 ms |  0.73 |    0.03 |          - |     - |     - |  1097.01 KB |
|                  EasyCaching_Disk |        100 |   117.509 ms |  2.3395 ms |  3.7107 ms |   117.276 ms |  0.87 |    0.04 |          - |     - |     - |   1765.8 KB |
|     CacheTower_JsonFileCacheLayer |        100 |   135.501 ms |  2.6973 ms |  4.1993 ms |   135.479 ms |  1.00 |    0.00 |          - |     - |     - |  2766.57 KB |
|             MonkeyCache_FileStore |        100 |   192.599 ms |  3.5928 ms |  3.5286 ms |   192.630 ms |  1.42 |    0.06 |  1000.0000 |     - |     - |  4379.57 KB |
|                                   |            |              |            |            |              |       |         |            |       |       |             |
| CacheTower_ProtobufFileCacheLayer |       1000 |   917.767 ms |  8.0175 ms |  7.4996 ms |   917.125 ms |  0.71 |    0.01 |  2000.0000 |     - |     - | 10856.57 KB |
|                  EasyCaching_Disk |       1000 | 1,100.793 ms | 14.6517 ms | 13.7052 ms | 1,096.219 ms |  0.85 |    0.01 |  5000.0000 |     - |     - | 17496.22 KB |
|     CacheTower_JsonFileCacheLayer |       1000 | 1,302.846 ms |  8.8092 ms |  7.3561 ms | 1,301.013 ms |  1.00 |    0.00 |  9000.0000 |     - |     - | 27432.45 KB |
|             MonkeyCache_FileStore |       1000 | 1,864.351 ms | 10.0374 ms |  8.8979 ms | 1,863.543 ms |  1.43 |    0.01 | 14000.0000 |     - |     - | 43596.71 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.417 ms | 0.0569 ms | 0.2339 ms |   1.384 ms |  1.00 |    0.00 |         - |     - |     - |    8.25 KB |
|         CacheManager_Redis |          1 |   2.291 ms | 0.0865 ms | 0.3518 ms |   2.276 ms |  1.66 |    0.36 |         - |     - |     - |   27.34 KB |
|          EasyCaching_Redis |          1 |   3.823 ms | 0.1292 ms | 0.5343 ms |   3.780 ms |  2.76 |    0.54 |         - |     - |     - |  509.27 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |        100 |  39.721 ms | 0.7902 ms | 2.3910 ms |  38.750 ms |  1.00 |    0.00 |         - |     - |     - |  383.95 KB |
|          EasyCaching_Redis |        100 |  42.436 ms | 0.8463 ms | 2.5735 ms |  41.941 ms |  1.07 |    0.07 |         - |     - |     - |  851.38 KB |
|         CacheManager_Redis |        100 |  42.910 ms | 0.8545 ms | 2.8931 ms |  42.201 ms |  1.09 |    0.07 |         - |     - |     - |  554.95 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |       1000 | 368.641 ms | 2.2060 ms | 2.0635 ms | 368.086 ms |  1.00 |    0.00 | 1000.0000 |     - |     - | 3817.55 KB |
|          EasyCaching_Redis |       1000 | 374.050 ms | 2.5779 ms | 2.4113 ms | 373.830 ms |  1.01 |    0.01 | 1000.0000 |     - |     - | 3995.38 KB |
|         CacheManager_Redis |       1000 | 394.929 ms | 4.0107 ms | 3.7516 ms | 395.091 ms |  1.07 |    0.01 | 1000.0000 |     - |     - | 5352.86 KB |