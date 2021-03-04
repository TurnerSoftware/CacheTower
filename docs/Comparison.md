# Comparison

The performance figures below are as a guide only. Different systems and configurations can drastically change performance results.
Run the benchmark suite on your target machine to best gauge how the performance is for your use case.

Keep in mind:
- The scale of time and the size of allocation differences between these libraries (some differences are so small that they are within a margin of error)
- The various libraries have different features, you would hopefully be choosing one not just because of its performance

If you are one of the maintainers of the libraries referenced below and you feel the test is unfair (eg. not properly configured), open a PR to fix it!

**Test Machine**

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.804 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.200
  [Host]     : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT
  Job-RWRNYY : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT

Runtime=.NET Core 5.0  MaxIterationCount=200
```

## Memory Caching

|                            Method | Iterations |         Mean |      Error |     StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |  Gen 2 |  Allocated |
|---------------------------------- |----------- |-------------:|-----------:|-----------:|------:|--------:|---------:|---------:|-------:|-----------:|
|          LazyCache_MemoryProvider |          1 |     1.705 us |  0.0123 us |  0.0109 us |  0.89 |    0.01 |   0.4730 |        - |      - |    1.45 KB |
|       CacheTower_MemoryCacheLayer |          1 |     1.915 us |  0.0326 us |  0.0305 us |  1.00 |    0.00 |   0.4768 |        - |      - |    1.46 KB |
|              EasyCaching_InMemory |          1 |     9.623 us |  0.1007 us |  0.0942 us |  5.03 |    0.10 |   1.3580 |        - |      - |    4.19 KB |
| CacheManager_MicrosoftMemoryCache |          1 |    18.550 us |  0.3646 us |  0.3744 us |  9.71 |    0.31 |   2.5635 |   1.2817 |      - |    7.94 KB |
|        FusionCache_MemoryProvider |          1 |   104.460 us |  1.4347 us |  1.3421 us | 54.56 |    1.18 |  66.7725 |  25.5127 | 0.2441 |  269.56 KB |
|                                   |            |              |            |            |       |         |          |          |        |            |
|       CacheTower_MemoryCacheLayer |       1000 |   637.165 us |  4.1835 us |  3.9133 us |  1.00 |    0.00 |   9.7656 |        - |      - |   32.68 KB |
|          LazyCache_MemoryProvider |       1000 | 1,646.337 us | 12.1031 us | 11.3212 us |  2.58 |    0.02 | 335.9375 |        - |      - | 1031.67 KB |
| CacheManager_MicrosoftMemoryCache |       1000 | 1,746.701 us | 24.4290 us | 22.8509 us |  2.74 |    0.04 |  87.8906 |        - |      - |  273.45 KB |
|        FusionCache_MemoryProvider |       1000 | 2,045.240 us | 23.3107 us | 21.8048 us |  3.21 |    0.04 | 277.3438 | 121.0938 |      - | 1260.82 KB |
|              EasyCaching_InMemory |       1000 | 4,381.147 us | 43.2235 us | 40.4313 us |  6.88 |    0.08 | 351.5625 |        - |      - | 1081.46 KB |

## File Caching

|                            Method | Iterations |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|---------------------------------- |----------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|------:|------:|------------:|
|                  EasyCaching_Disk |          1 |     3.389 ms |  0.0678 ms |  0.2040 ms |     3.394 ms |  0.82 |    0.08 |          - |     - |     - |    33.09 KB |
| CacheTower_ProtobufFileCacheLayer |          1 |     3.543 ms |  0.0706 ms |  0.2741 ms |     3.482 ms |  0.87 |    0.09 |          - |     - |     - |    22.66 KB |
|             MonkeyCache_FileStore |          1 |     3.947 ms |  0.0789 ms |  0.3025 ms |     3.884 ms |  0.97 |    0.12 |          - |     - |     - |    65.77 KB |
|     CacheTower_JsonFileCacheLayer |          1 |     4.103 ms |  0.0858 ms |  0.3605 ms |     3.993 ms |  1.00 |    0.00 |          - |     - |     - |    49.48 KB |
|                                   |            |              |            |            |              |       |         |            |       |       |             |
| CacheTower_ProtobufFileCacheLayer |        100 |    89.434 ms |  1.5269 ms |  2.2381 ms |    89.053 ms |  0.82 |    0.02 |          - |     - |     - |   904.32 KB |
|                  EasyCaching_Disk |        100 |    95.867 ms |  1.8962 ms |  1.7737 ms |    95.687 ms |  0.88 |    0.02 |          - |     - |     - |  1722.27 KB |
|     CacheTower_JsonFileCacheLayer |        100 |   109.103 ms |  1.7466 ms |  1.8689 ms |   108.858 ms |  1.00 |    0.00 |          - |     - |     - |  2573.85 KB |
|             MonkeyCache_FileStore |        100 |   157.297 ms |  2.8629 ms |  2.3907 ms |   156.770 ms |  1.44 |    0.03 |  1000.0000 |     - |     - |  4376.43 KB |
|                                   |            |              |            |            |              |       |         |            |       |       |             |
|                  EasyCaching_Disk |       1000 |   840.535 ms | 13.2710 ms | 16.2980 ms |   839.398 ms |  0.80 |    0.02 |  5000.0000 |     - |     - | 17088.03 KB |
| CacheTower_ProtobufFileCacheLayer |       1000 |   870.348 ms | 17.2801 ms | 25.3290 ms |   861.878 ms |  0.83 |    0.03 |  3000.0000 |     - |     - |  8948.18 KB |
|     CacheTower_JsonFileCacheLayer |       1000 | 1,054.265 ms | 15.7163 ms | 14.7011 ms | 1,055.984 ms |  1.00 |    0.00 |  8000.0000 |     - |     - | 25523.94 KB |
|             MonkeyCache_FileStore |       1000 | 1,523.386 ms | 14.5468 ms | 12.1473 ms | 1,524.757 ms |  1.45 |    0.01 | 14000.0000 |     - |     - | 43565.45 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|----------:|------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.192 ms | 0.0458 ms | 0.1919 ms |  1.00 |    0.00 |         - |     - |     - |    3.95 KB |
|         CacheManager_Redis |          1 |   2.415 ms | 0.0875 ms | 0.3599 ms |  2.08 |    0.48 |         - |     - |     - |   27.33 KB |
|          EasyCaching_Redis |          1 |   3.409 ms | 0.0681 ms | 0.2643 ms |  2.90 |    0.54 |         - |     - |     - |  505.03 KB |
|                            |            |            |           |           |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |        100 |  40.693 ms | 0.8086 ms | 1.8085 ms |  1.00 |    0.00 |         - |     - |     - |  199.63 KB |
|         CacheManager_Redis |        100 |  41.982 ms | 0.7964 ms | 2.0841 ms |  1.04 |    0.05 |         - |     - |     - |  547.08 KB |
|          EasyCaching_Redis |        100 |  42.166 ms | 0.8371 ms | 1.8895 ms |  1.04 |    0.06 |         - |     - |     - |   829.1 KB |
|                            |            |            |           |           |       |         |           |       |       |            |
|          EasyCaching_Redis |       1000 | 381.029 ms | 3.5180 ms | 2.9377 ms |  0.98 |    0.01 | 1000.0000 |     - |     - | 3775.95 KB |
| CacheTower_RedisCacheLayer |       1000 | 386.703 ms | 1.7611 ms | 1.6473 ms |  1.00 |    0.00 |         - |     - |     - | 1978.62 KB |
|         CacheManager_Redis |       1000 | 392.653 ms | 3.2438 ms | 2.7087 ms |  1.01 |    0.01 | 1000.0000 |     - |     - | 5272.27 KB |