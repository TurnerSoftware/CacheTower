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

# Memory Caching

|                             Method | UnrollFactor | Iterations |               Mean |            Error |           StdDev |             Median |    Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|----------------------------------- |------------- |----------- |-------------------:|-----------------:|-----------------:|-------------------:|---------:|--------:|-----------:|-----------:|------:|-----------:|
|      CacheManager_DictionaryHandle |            1 |          1 |       102,720.8 ns |      7,540.56 ns |     30,497.28 ns |        95,500.0 ns |        ? |       ? |          - |          - |     - |     6936 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer |           16 |          1 |         2,336.9 ns |         45.01 ns |         46.22 ns |         2,312.0 ns |     1.00 |    0.00 |     0.5531 |          - |     - |     1736 B |
|  CacheManager_MicrosoftMemoryCache |           16 |          1 |        17,776.1 ns |        342.95 ns |        320.80 ns |        17,750.0 ns |     7.60 |    0.20 |     2.4719 |     1.2207 |     - |     7848 B |
|                  Akavache_InMemory |           16 |          1 |     1,218,537.6 ns |     19,712.87 ns |     18,439.43 ns |     1,223,144.7 ns |   520.85 |   12.39 |    19.5313 |          - |     - |    65449 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|      CacheManager_DictionaryHandle |            1 |        100 |       221,213.7 ns |     12,638.66 ns |     51,116.20 ns |       213,200.0 ns |        ? |       ? |          - |          - |     - |    33864 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer |           16 |        100 |        84,774.1 ns |      1,271.53 ns |      1,189.39 ns |        84,890.4 ns |     1.00 |    0.00 |    14.4043 |          - |     - |    45296 B |
|  CacheManager_MicrosoftMemoryCache |           16 |        100 |       188,186.7 ns |      2,808.54 ns |      2,345.26 ns |       188,292.4 ns |     2.23 |    0.04 |    10.9863 |     3.6621 |     - |    34789 B |
|                  Akavache_InMemory |           16 |        100 |   120,504,963.3 ns |  2,004,651.03 ns |  1,875,151.81 ns |   120,689,575.0 ns | 1,421.68 |   26.30 |  2000.0000 |  1000.0000 |     - |  6500296 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|      CacheManager_DictionaryHandle |            1 |       1000 |     1,150,654.0 ns |     52,415.17 ns |    221,356.78 ns |     1,094,950.0 ns |        ? |       ? |          - |          - |     - |   278664 B |
|                                    |              |            |                    |                  |                  |                    |          |         |            |            |       |            |
|        CacheTower_MemoryCacheLayer |           16 |       1000 |       825,188.8 ns |     13,762.55 ns |     12,873.50 ns |       829,297.7 ns |     1.00 |    0.00 |   140.6250 |          - |     - |   441296 B |
|  CacheManager_MicrosoftMemoryCache |           16 |       1000 |     1,723,561.9 ns |     25,869.58 ns |     24,198.42 ns |     1,724,690.6 ns |     2.09 |    0.04 |    87.8906 |          - |     - |   279712 B |
|                  Akavache_InMemory |           16 |       1000 | 1,242,895,193.3 ns | 22,760,744.74 ns | 21,290,414.75 ns | 1,239,531,300.0 ns | 1,506.76 |   44.52 | 20000.0000 | 10000.0000 |     - | 64996696 B |

# JSON File Caching

|                        Method | Iterations |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |   Allocated |
|------------------------------ |----------- |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|-----------:|------:|------------:|
| CacheTower_JsonFileCacheLayer |          1 |     5.917 ms |  0.1710 ms |  0.7071 ms |     5.943 ms |  1.00 |    0.00 |          - |          - |     - |    13.79 KB |
|         MonkeyCache_FileStore |          1 |     5.307 ms |  0.1586 ms |  0.6630 ms |     5.279 ms |  0.91 |    0.15 |          - |          - |     - |    65.81 KB |
|         Akavache_LocalMachine |          1 |     1.935 ms |  0.0717 ms |  0.2915 ms |     1.869 ms |  0.33 |    0.07 |          - |          - |     - |    81.13 KB |
|                               |            |              |            |            |              |       |         |            |            |       |             |
| CacheTower_JsonFileCacheLayer |        100 |   129.356 ms |  2.5593 ms |  4.6150 ms |   129.773 ms |  1.00 |    0.00 |          - |          - |     - |    13.37 KB |
|         MonkeyCache_FileStore |        100 |   168.396 ms |  3.3247 ms |  3.6954 ms |   168.258 ms |  1.30 |    0.05 |  1000.0000 |          - |     - |  4379.57 KB |
|         Akavache_LocalMachine |        100 |   116.599 ms |  2.3230 ms |  5.8280 ms |   114.316 ms |  0.92 |    0.06 |  2000.0000 |  1000.0000 |     - |  6367.09 KB |
|                               |            |              |            |            |              |       |         |            |            |       |             |
| CacheTower_JsonFileCacheLayer |       1000 | 1,199.861 ms | 23.3654 ms | 31.1922 ms | 1,196.665 ms |  1.00 |    0.00 |  9000.0000 |          - |     - |    13.37 KB |
|         MonkeyCache_FileStore |       1000 | 1,628.246 ms | 24.0842 ms | 21.3500 ms | 1,627.499 ms |  1.38 |    0.02 | 14000.0000 |          - |     - | 43596.71 KB |
|         Akavache_LocalMachine |       1000 | 1,258.250 ms | 24.8065 ms | 38.6208 ms | 1,242.826 ms |  1.06 |    0.05 | 20000.0000 | 10000.0000 |     - | 63472.94 KB |

## Redis Caching

|                     Method | Iterations |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------:|------:|-----------:|
| CacheTower_RedisCacheLayer |          1 |   1.469 ms | 0.1404 ms | 0.4139 ms |   1.441 ms |  1.00 |    0.00 |         - |     - |     - |    3.56 KB |
|         CacheManager_Redis |          1 |   1.876 ms | 0.1078 ms | 0.3093 ms |   1.791 ms |  1.36 |    0.44 |         - |     - |     - |   22.67 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |        100 |  39.652 ms | 0.7891 ms | 2.0084 ms |  39.359 ms |  1.00 |    0.00 |         - |     - |     - |    3.16 KB |
|         CacheManager_Redis |        100 |  41.894 ms | 0.8328 ms | 2.0738 ms |  41.980 ms |  1.06 |    0.05 |         - |     - |     - |  399.34 KB |
|                            |            |            |           |           |            |       |         |           |       |       |            |
| CacheTower_RedisCacheLayer |       1000 | 365.597 ms | 1.9306 ms | 1.8058 ms | 365.437 ms |  1.00 |    0.00 | 1000.0000 |     - |     - |     3.3 KB |
|         CacheManager_Redis |       1000 | 392.764 ms | 4.0479 ms | 3.7864 ms | 391.458 ms |  1.07 |    0.01 | 1000.0000 |     - |     - | 3824.77 KB |