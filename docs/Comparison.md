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

|                                    Method | Iterations |               Mean |            Error |           StdDev | Ratio | RatioSD |       Gen 0 |  Gen 1 |  Gen 2 |    Allocated |
|------------------------------------------ |----------- |-------------------:|-----------------:|-----------------:|------:|--------:|------------:|-------:|-------:|-------------:|
|        CacheTower_MemoryCacheLayer_Direct |          1 |           696.1 ns |          6.95 ns |          6.50 ns |  0.31 |    0.00 |      0.3052 |      - |      - |        960 B |
|                  LazyCache_MemoryProvider |          1 |         1,723.6 ns |         26.61 ns |         24.89 ns |  0.77 |    0.01 |      0.4139 |      - |      - |       1304 B |
|             LazyCache_MemoryProviderAsync |          1 |         1,951.9 ns |         18.26 ns |         17.08 ns |  0.87 |    0.01 |      0.4807 |      - |      - |       1520 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |          1 |         2,240.1 ns |         15.72 ns |         14.71 ns |  1.00 |    0.00 |      0.4768 |      - |      - |       1496 B |
|                      EasyCaching_InMemory |          1 |         9,527.4 ns |        166.21 ns |        155.48 ns |  4.25 |    0.07 |      1.3580 |      - |      - |       4281 B |
|         CacheManager_MicrosoftMemoryCache |          1 |        18,448.6 ns |        202.04 ns |        188.99 ns |  8.24 |    0.09 |      2.4719 | 1.2207 | 0.0305 |       7848 B |
|                 EasyCaching_InMemoryAsync |          1 |        27,075.0 ns |        436.53 ns |        386.98 ns | 12.09 |    0.18 |      2.0142 |      - |      - |       6262 B |
|                                           |            |                    |                  |                  |       |         |             |        |        |              |
|        CacheTower_MemoryCacheLayer_Direct |       1000 |       274,832.3 ns |      4,810.03 ns |      4,499.30 ns |  0.37 |    0.01 |     10.2539 |      - |      - |      32929 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |       1000 |       751,574.5 ns |      9,515.41 ns |      8,900.72 ns |  1.00 |    0.00 |      9.7656 |      - |      - |      33464 B |
|                  LazyCache_MemoryProvider |       1000 |     1,648,602.8 ns |     28,447.19 ns |     26,609.52 ns |  2.19 |    0.05 |    337.8906 |      - |      - |    1064243 B |
|             LazyCache_MemoryProviderAsync |       1000 |     1,802,759.1 ns |     34,473.06 ns |     35,401.29 ns |  2.40 |    0.06 |    406.2500 |      - |      - |    1280242 B |
|         CacheManager_MicrosoftMemoryCache |       1000 |     1,815,216.5 ns |     21,853.19 ns |     20,441.49 ns |  2.42 |    0.05 |     87.8906 |      - |      - |     279715 B |
|                      EasyCaching_InMemory |       1000 |     4,394,794.9 ns |     61,229.98 ns |     57,274.56 ns |  5.85 |    0.09 |    343.7500 |      - |      - |    1099405 B |
|                 EasyCaching_InMemoryAsync |       1000 |     8,015,753.2 ns |    114,873.54 ns |    107,452.78 ns | 10.67 |    0.20 |    656.2500 |      - |      - |    2068693 B |
|                                           |            |                    |                  |                  |       |         |             |        |        |              |
|        CacheTower_MemoryCacheLayer_Direct |    1000000 |   274,806,796.7 ns |  4,071,864.24 ns |  3,808,824.34 ns |  0.36 |    0.01 |  10000.0000 |      - |      - |   32001596 B |
| CacheTower_MemoryCacheLayer_ViaCacheStack |    1000000 |   756,157,000.0 ns | 13,844,993.35 ns | 12,950,615.37 ns |  1.00 |    0.00 |  10000.0000 |      - |      - |   32002352 B |
|                  LazyCache_MemoryProvider |    1000000 | 1,669,631,493.3 ns | 20,710,058.62 ns | 19,372,201.68 ns |  2.21 |    0.05 | 339000.0000 |      - |      - | 1064000240 B |
|         CacheManager_MicrosoftMemoryCache |    1000000 | 1,727,536,021.4 ns | 19,697,815.51 ns | 17,461,600.38 ns |  2.28 |    0.05 |  86000.0000 |      - |      - |  272009128 B |
|             LazyCache_MemoryProviderAsync |    1000000 | 1,800,238,493.3 ns | 26,653,075.09 ns | 24,931,302.98 ns |  2.38 |    0.04 | 407000.0000 |      - |      - | 1280000344 B |
|                      EasyCaching_InMemory |    1000000 | 4,399,948,778.6 ns | 58,780,947.24 ns | 52,107,778.66 ns |  5.81 |    0.11 | 349000.0000 |      - |      - | 1096213184 B |
|                 EasyCaching_InMemoryAsync |    1000000 | 7,840,752,686.7 ns | 33,673,444.22 ns | 31,498,160.62 ns | 10.37 |    0.16 | 663000.0000 |      - |      - | 2064401328 B |

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