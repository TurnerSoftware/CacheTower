# Performance

The performance figures below are as a guide only. Different systems and configurations can drastically change performance results.
Run the benchmark suite on your target machine to best gauge how the performance is for your use case.

Regarding specific tests, it is best to look at the implementations themselves to understand what the test is doing in that time/allocation allotment.

**Test Machine**

```
BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100
  [Host] : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  Core   : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT

Job=Core  Runtime=Core
```

## Cache Stack Benchmark

|                    Method |       Mean [ns] |    Error [ns] |   StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|-------------------------- |----------------:|--------------:|--------------:|-------:|------:|------:|--------------:|
|          SetupAndTeardown |        286.4 ns |       6.94 ns |       6.16 ns | 0.1326 |     - |     - |         416 B |
|                       Set |        668.6 ns |       6.07 ns |       5.07 ns | 0.2108 |     - |     - |         664 B |
|             Set_TwoLayers |        906.2 ns |       5.14 ns |       4.81 ns | 0.3233 |     - |     - |        1016 B |
|                     Evict |        778.8 ns |       6.55 ns |       5.81 ns | 0.2108 |     - |     - |         664 B |
|           Evict_TwoLayers |      1,116.9 ns |       5.70 ns |       5.33 ns | 0.3223 |     - |     - |        1016 B |
|                   Cleanup |      1,006.9 ns |       9.76 ns |       9.13 ns | 0.2098 |     - |     - |         664 B |
|         Cleanup_TwoLayers |      1,489.0 ns |      10.36 ns |       9.19 ns | 0.3223 |     - |     - |        1016 B |
|                   GetMiss |        401.0 ns |       3.81 ns |       3.57 ns | 0.1326 |     - |     - |         416 B |
|                    GetHit |        815.8 ns |       6.47 ns |       5.41 ns | 0.2346 |     - |     - |         736 B |
|                  GetOrSet |      1,985.8 ns |       9.84 ns |       9.20 ns | 0.4768 |     - |     - |        1504 B |
|  GetOrSet_TwoSimultaneous | 62,171,738.5 ns | 418,770.92 ns | 391,718.58 ns |      - |     - |     - |        3771 B |
| GetOrSet_FourSimultaneous | 62,111,005.8 ns | 477,178.35 ns | 446,352.92 ns |      - |     - |     - |        3599 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |       Error |       StdDev |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|----------------------- |-------------:|------------:|-------------:|---------:|--------:|-----------:|------:|------:|------------:|
|       MemoryCacheLayer |     117.8 us |     2.13 us |      1.77 us |     1.00 |    0.00 |    30.7617 |     - |     - |    94.34 KB |
|        RedisCacheLayer |  50,325.4 us |   995.02 us |  2,204.89 us |   456.51 |   19.36 |          - |     - |     - |    320.8 KB |
| ProtobufFileCacheLayer | 208,518.6 us | 4,149.30 us |  5,095.71 us | 1,769.01 |   59.62 |          - |     - |     - |   1574.7 KB |
|     JsonFileCacheLayer | 260,626.9 us | 5,117.03 us |  7,814.25 us | 2,214.22 |   67.60 |  1000.0000 |     - |     - |  3385.54 KB |
|      MongoDbCacheLayer | 449,438.9 us | 8,916.56 us | 21,704.14 us | 3,968.43 |  223.46 | 11000.0000 |     - |     - | 33891.22 KB |

## In-Memory Benchmarks

### MemoryCacheLayer

|      Method |   Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------ |------------:|-----------:|------------:|-------:|------:|------:|--------------:|
|    Overhead |    142.7 ns |    1.15 ns |     1.02 ns | 0.0663 |     - |     - |         208 B |
|     GetMiss |    208.5 ns |    1.99 ns |     1.77 ns | 0.0663 |     - |     - |         208 B |
|      GetHit |    533.8 ns |    3.10 ns |     2.90 ns | 0.1221 |     - |     - |         384 B |
|      SetNew |    448.7 ns |    3.02 ns |     2.83 ns | 0.1221 |     - |     - |         384 B |
| SetExisting |    709.7 ns |    3.25 ns |     3.04 ns | 0.1345 |     - |     - |         424 B |
|     SetMany | 34,425.9 ns |  160.00 ns |   149.66 ns | 6.9580 |     - |     - |       21920 B |

## File System Benchmarks

### FileCacheLayerBase (Overhead)

|                  Method |       Mean [ns] |   Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|---------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  2,341,841.8 ns |  76,116.2 ns |   318,943.0 ns |     - |     - |     - |       16120 B |
| SetExistingSimultaneous |  2,339,074.7 ns |  75,701.7 ns |   312,161.1 ns |     - |     - |     - |       12664 B |
|                Overhead |  1,073,281.4 ns |  39,850.4 ns |   166,101.1 ns |     - |     - |     - |        4560 B |
|                 GetMiss |  1,482,845.9 ns |  50,339.9 ns |   209,822.5 ns |     - |     - |     - |        7400 B |
|                  GetHit |  2,113,604.9 ns |  58,434.3 ns |   237,663.9 ns |     - |     - |     - |       13424 B |
|                  SetNew |  1,957,598.5 ns |  59,701.2 ns |   249,502.0 ns |     - |     - |     - |        9856 B |
|             SetExisting |  2,160,977.6 ns |  70,054.7 ns |   293,544.4 ns |     - |     - |     - |       11704 B |
|                 SetMany | 44,174,175.0 ns | 877,421.9 ns | 2,102,246.8 ns |     - |     - |     - |      269352 B |

### JsonFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  4,837,957.4 ns |   149,293.4 ns |   627,214.2 ns |     - |     - |     - |       53112 B |
| SetExistingSimultaneous |  5,414,622.7 ns |   167,736.1 ns |   706,537.2 ns |     - |     - |     - |       52728 B |
|                Overhead |  1,708,236.0 ns |    50,023.8 ns |   205,715.7 ns |     - |     - |     - |       12808 B |
|                 GetMiss |  2,691,072.0 ns |    88,532.0 ns |   368,029.8 ns |     - |     - |     - |       24120 B |
|                  GetHit |  4,488,429.6 ns |   156,856.1 ns |   657,260.7 ns |     - |     - |     - |       43704 B |
|                  SetNew |  3,823,031.4 ns |   113,956.9 ns |   474,985.9 ns |     - |     - |     - |       34264 B |
|             SetExisting |  4,617,172.4 ns |   143,627.5 ns |   595,465.9 ns |     - |     - |     - |       43976 B |
|                 SetMany | 89,669,192.1 ns | 1,789,640.4 ns | 3,087,041.7 ns |     - |     - |     - |     1110376 B |

### ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  3,228,216.1 ns |    96,328.3 ns |   396,136.6 ns |     - |     - |     - |       21304 B |
| SetExistingSimultaneous |  3,722,526.7 ns |    94,185.7 ns |   389,434.4 ns |     - |     - |     - |       21240 B |
|                Overhead |  1,127,971.4 ns |    43,197.2 ns |   177,642.5 ns |     - |     - |     - |        4608 B |
|                 GetMiss |  1,596,811.1 ns |    53,908.5 ns |   222,295.6 ns |     - |     - |     - |        7440 B |
|                  GetHit |  3,194,483.0 ns |    91,887.1 ns |   376,839.4 ns |     - |     - |     - |       17768 B |
|                  SetNew |  3,017,028.6 ns |    95,326.3 ns |   392,016.0 ns |     - |     - |     - |       14200 B |
|             SetExisting |  3,377,455.3 ns |    96,328.4 ns |   395,054.0 ns |     - |     - |     - |       18000 B |
|                 SetMany | 57,944,462.1 ns | 1,150,520.1 ns | 3,301,057.8 ns |     - |     - |     - |      467248 B |

## Database Benchmarks

### MongoDbCacheLayer

|                  Method |        Mean [ns] |     Error [ns] |    StdDev [ns] |      Median [ns] |     Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |-----------------:|---------------:|---------------:|-----------------:|----------:|------:|------:|--------------:|
|      GetHitSimultaneous |  70,652,243.9 ns | 1,847,772.3 ns | 7,783,178.4 ns |  69,219,350.0 ns |         - |     - |     - |      277504 B |
| SetExistingSimultaneous |  70,761,139.7 ns | 1,785,866.2 ns | 7,443,701.3 ns |  69,789,050.0 ns |         - |     - |     - |      253016 B |
|                Overhead |       7,777.6 ns |       493.5 ns |     1,891.7 ns |       7,400.0 ns |         - |     - |     - |         424 B |
|                 GetMiss |  67,504,475.6 ns | 1,778,059.7 ns | 7,470,019.1 ns |  65,904,500.0 ns |         - |     - |     - |      121680 B |
|                  GetHit |  71,031,643.2 ns | 1,820,413.6 ns | 7,687,867.3 ns |  70,016,600.0 ns |         - |     - |     - |      198000 B |
|                  SetNew |  69,524,188.8 ns | 1,715,738.5 ns | 7,208,194.0 ns |  68,905,900.0 ns |         - |     - |     - |      109984 B |
|             SetExisting |  70,536,871.4 ns | 1,895,253.2 ns | 7,941,517.9 ns |  68,789,700.0 ns |         - |     - |     - |      181504 B |
|                 SetMany | 128,326,730.3 ns | 2,562,373.3 ns | 7,514,997.3 ns | 127,383,300.0 ns | 2000.0000 |     - |     - |     7294544 B |

## Other

### Redis

|                  Method |       Mean [ns] |   Error [ns] |  StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|-------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |    700,355.7 ns |  25,161.2 ns | 101,762.9 ns |    684,300.0 ns |     - |     - |     - |        3232 B |
| SetExistingSimultaneous |    630,156.5 ns |  23,031.0 ns |  93,932.6 ns |    617,500.0 ns |     - |     - |     - |        3232 B |
|                Overhead |      6,183.9 ns |     470.5 ns |   1,781.0 ns |      5,700.0 ns |     - |     - |     - |         136 B |
|                 GetMiss |    401,902.1 ns |  22,266.2 ns |  92,065.4 ns |    384,900.0 ns |     - |     - |     - |         840 B |
|                  GetHit |    667,375.1 ns |  26,241.7 ns | 105,531.8 ns |    653,800.0 ns |     - |     - |     - |        2296 B |
|                  SetNew |    397,266.3 ns |  19,918.8 ns |  79,415.3 ns |    392,950.0 ns |     - |     - |     - |        1304 B |
|             SetExisting |    605,903.8 ns |  25,876.6 ns | 104,360.3 ns |    606,650.0 ns |     - |     - |     - |        2288 B |
|                 SetMany | 13,544,324.1 ns | 270,956.3 ns | 723,237.1 ns | 13,494,900.0 ns |     - |     - |     - |       97552 B |


## Extensions

### RedisRemoteEvictionExtension

|         Method |   Mean [ns] | Error [ns] | StdDev [ns] | Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|--------------- |------------:|-----------:|------------:|------------:|------:|------:|------:|--------------:|
| OnValueRefresh | 54,195.5 ns | 5,770.1 ns | 22,871.1 ns | 45,500.0 ns |     - |     - |     - |         976 B |
|       Overhead |  7,189.4 ns |   493.2 ns |  1,860.8 ns |  6,900.0 ns |     - |     - |     - |         352 B |
|       Register | 19,111.9 ns | 2,889.9 ns | 11,488.4 ns | 14,700.0 ns |     - |     - |     - |         568 B |

### RedisLockExtension

|       Method |    Mean [ns] |  Error [ns] | StdDev [ns] |  Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------- |-------------:|------------:|------------:|-------------:|------:|------:|------:|--------------:|
| RefreshValue | 407,611.7 ns | 17,617.5 ns | 72,251.6 ns | 398,150.0 ns |     - |     - |     - |       12768 B |
|     Overhead |  23,471.5 ns |  3,210.1 ns | 12,573.6 ns |  18,550.0 ns |     - |     - |     - |        1128 B |
|     Register |  22,177.2 ns |  2,915.5 ns | 11,385.0 ns |  17,800.0 ns |     - |     - |     - |        1128 B |