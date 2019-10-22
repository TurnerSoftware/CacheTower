# Performance

Test Machine

```
BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100
  [Host] : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  Core   : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT

Job=Core  Runtime=Core
```

## Cache Stack Benchmark

|                    Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|-------------------------- |----------------:|---------------:|---------------:|-------:|------:|------:|--------------:|
|          SetupAndTeardown |        476.5 ns |       5.426 ns |       5.076 ns | 0.3386 |     - |     - |        1064 B |
|                       Set |        877.5 ns |       7.397 ns |       6.557 ns | 0.4177 |     - |     - |        1312 B |
|             Set_TwoLayers |      1,146.7 ns |      10.545 ns |       9.864 ns | 0.5302 |     - |     - |        1664 B |
|                     Evict |      1,004.0 ns |      12.049 ns |      11.270 ns | 0.4177 |     - |     - |        1312 B |
|           Evict_TwoLayers |      1,310.8 ns |       6.213 ns |       5.812 ns | 0.5302 |     - |     - |        1664 B |
|                   Cleanup |      1,205.9 ns |      10.961 ns |      10.253 ns | 0.4177 |     - |     - |        1312 B |
|         Cleanup_TwoLayers |      1,737.4 ns |      11.586 ns |      10.837 ns | 0.5302 |     - |     - |        1664 B |
|                   GetMiss |        658.8 ns |       4.087 ns |       3.823 ns | 0.3643 |     - |     - |        1144 B |
|                    GetHit |      1,065.5 ns |       9.901 ns |       9.262 ns | 0.4654 |     - |     - |        1464 B |
|                  GetOrSet |      2,568.5 ns |      16.940 ns |      15.846 ns | 0.7133 |     - |     - |        2248 B |
|  GetOrSet_TwoSimultaneous | 62,232,245.2 ns | 314,169.982 ns | 293,874.796 ns |      - |     - |     - |        3328 B |
| GetOrSet_FourSimultaneous | 62,190,605.2 ns | 326,271.430 ns | 305,194.498 ns |      - |     - |     - |        4000 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |        Error |        StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|-------------:|--------------:|-------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     127.1 us |     1.261 us |      1.118 us |     127.3 us |     1.00 |    0.00 |    30.7617 |     - |     - |   96600 B |
|        RedisCacheLayer |  51,408.1 us | 1,101.967 us |  2,300.215 us |  50,481.5 us |   419.00 |   19.55 |          - |     - |     - |     832 B |
| ProtobufFileCacheLayer | 217,127.2 us | 4,304.501 us |  8,395.598 us | 216,152.7 us | 1,736.01 |   60.04 |          - |     - |     - |   16664 B |
|     JsonFileCacheLayer | 261,546.2 us | 5,120.906 us |  7,820.165 us | 259,921.1 us | 2,054.30 |   60.10 |  1000.0000 |     - |     - |   13016 B |
|      MongoDbCacheLayer | 493,755.1 us | 9,702.750 us | 20,886.193 us | 492,509.5 us | 3,984.89 |  193.48 | 11000.0000 |     - |     - |   45152 B |

## In-Memory Benchmarks

### MemoryCacheLayer

|      Method |   Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------ |------------:|-----------:|------------:|-------:|------:|------:|--------------:|
|    Overhead |    199.1 ns |   1.260 ns |    1.179 ns | 0.0663 |     - |     - |         208 B |
|     GetMiss |    274.8 ns |   1.978 ns |    1.753 ns | 0.0663 |     - |     - |         208 B |
|      GetHit |    266.6 ns |   1.197 ns |    1.061 ns | 0.0663 |     - |     - |         208 B |
|      SetNew |    506.9 ns |   2.258 ns |    2.002 ns | 0.1221 |     - |     - |         384 B |
| SetExisting |    790.6 ns |  13.305 ns |   11.795 ns | 0.1345 |     - |     - |         424 B |
|     SetMany | 33,610.6 ns | 255.101 ns |  238.622 ns | 6.9580 |     - |     - |       21920 B |

## File System Benchmarks

### FileCacheLayerBase (Overhead)

|                  Method |       Mean [ns] |   Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  2,259,366.7 ns |  80,744.4 ns |   334,758.3 ns |  2,242,100.0 ns |     - |     - |     - |       12184 B |
| SetExistingSimultaneous |  2,384,848.0 ns |  83,743.7 ns |   350,904.2 ns |  2,351,750.0 ns |     - |     - |     - |       12824 B |
|                Overhead |  1,121,524.2 ns |  45,191.5 ns |   188,363.6 ns |  1,084,200.0 ns |     - |     - |     - |        4560 B |
|                 GetMiss |  1,492,265.8 ns |  50,981.9 ns |   210,227.4 ns |  1,489,300.0 ns |     - |     - |     - |        7400 B |
|                  GetHit |  2,179,239.9 ns |  73,719.9 ns |   302,333.8 ns |  2,182,550.0 ns |     - |     - |     - |       10600 B |
|                  SetNew |  1,985,619.3 ns |  78,975.1 ns |   331,791.5 ns |  1,941,200.0 ns |     - |     - |     - |        9856 B |
|             SetExisting |  2,170,291.8 ns |  72,972.7 ns |   304,158.9 ns |  2,134,750.0 ns |     - |     - |     - |       11704 B |
|                 SetMany | 42,628,656.9 ns | 848,261.6 ns | 3,040,574.2 ns | 42,358,850.0 ns |     - |     - |     - |      281216 B |

### JsonFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  4,949,382.5 ns |   149,083.2 ns |   621,396.6 ns |  4,926,300.0 ns |     - |     - |     - |       13224 B |
| SetExistingSimultaneous |  5,444,543.7 ns |   174,214.8 ns |   731,914.5 ns |  5,360,000.0 ns |     - |     - |     - |       13032 B |
|                Overhead |  1,721,482.6 ns |    61,255.6 ns |   254,641.2 ns |  1,673,750.0 ns |     - |     - |     - |       11864 B |
|                 GetMiss |  2,714,927.3 ns |    85,018.9 ns |   354,368.7 ns |  2,691,200.0 ns |     - |     - |     - |       13040 B |
|                  GetHit |  4,432,687.6 ns |   151,656.7 ns |   632,123.0 ns |  4,377,450.0 ns |     - |     - |     - |       13296 B |
|                  SetNew |  3,825,177.7 ns |   122,343.1 ns |   508,583.3 ns |  3,874,600.0 ns |     - |     - |     - |       13264 B |
|             SetExisting |  4,619,996.9 ns |   145,580.1 ns |   603,561.3 ns |  4,607,450.0 ns |     - |     - |     - |       13280 B |
|                 SetMany | 85,445,234.5 ns | 1,707,170.0 ns | 3,638,118.4 ns | 84,873,300.0 ns |     - |     - |     - |       12936 B |

### ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  3,308,117.3 ns |   101,339.2 ns |   419,012.5 ns |     - |     - |     - |        9224 B |
| SetExistingSimultaneous |  3,851,802.6 ns |   128,619.2 ns |   533,242.7 ns |     - |     - |     - |        8888 B |
|                Overhead |  1,245,231.1 ns |    50,730.5 ns |   212,571.8 ns |     - |     - |     - |        4608 B |
|                 GetMiss |  1,655,874.6 ns |    62,696.0 ns |   260,629.0 ns |     - |     - |     - |        7440 B |
|                  GetHit |  3,253,526.0 ns |   103,809.3 ns |   430,383.6 ns |     - |     - |     - |        9224 B |
|                  SetNew |  3,016,203.6 ns |   109,169.9 ns |   457,445.5 ns |     - |     - |     - |        9192 B |
|             SetExisting |  3,388,019.2 ns |   118,549.3 ns |   492,812.4 ns |     - |     - |     - |        9208 B |
|                 SetMany | 56,811,847.5 ns | 1,125,570.3 ns | 2,945,421.3 ns |     - |     - |     - |        8864 B |

## Database Benchmarks

### MongoDbCacheLayer

|                  Method |        Mean [ns] |     Error [ns] |    StdDev [ns] |      Median [ns] |     Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |-----------------:|---------------:|---------------:|-----------------:|----------:|------:|------:|--------------:|
|                 GetMiss |  70,315,516.5 ns | 1,891,429.3 ns | 7,883,700.9 ns |  68,604,850.0 ns |         - |     - |     - |       29888 B |
|                  GetHit |  70,835,174.7 ns | 1,844,892.9 ns | 7,771,049.7 ns |  69,796,050.0 ns |         - |     - |     - |       29904 B |
|      GetHitSimultaneous |  73,326,742.6 ns | 1,812,371.9 ns | 7,574,230.1 ns |  71,462,800.0 ns |         - |     - |     - |       29912 B |
|                  SetNew |  68,713,562.9 ns | 1,685,478.1 ns | 7,025,271.7 ns |  67,795,100.0 ns |         - |     - |     - |       29896 B |
|             SetExisting |  72,124,037.7 ns | 1,840,507.2 ns | 7,772,725.2 ns |  71,159,100.0 ns |         - |     - |     - |       29896 B |
| SetExistingSimultaneous |  70,912,074.5 ns | 1,750,174.7 ns | 7,333,607.9 ns |  70,078,800.0 ns |         - |     - |     - |       29904 B |
|                 SetMany | 129,780,455.7 ns | 2,590,455.3 ns | 7,515,380.5 ns | 128,263,500.0 ns | 2000.0000 |     - |     - |       29936 B |

## Other

### Redis

|                  Method |       Mean [ns] |   Error [ns] |  StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|-------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |    792,440.4 ns |  37,254.8 ns | 152,786.3 ns |    800,800.0 ns |     - |     - |     - |        1328 B |
| SetExistingSimultaneous |    720,163.7 ns |  37,624.7 ns | 156,406.7 ns |    713,500.0 ns |     - |     - |     - |        1240 B |
|                Overhead |      6,397.0 ns |     979.9 ns |   3,791.7 ns |      5,050.0 ns |     - |     - |     - |          32 B |
|                 GetMiss |    395,827.3 ns |  25,463.9 ns | 106,136.6 ns |    381,450.0 ns |     - |     - |     - |         784 B |
|                  GetHit |    672,535.8 ns |  36,073.7 ns | 147,535.5 ns |    632,100.0 ns |     - |     - |     - |        1328 B |
|                  SetNew |    427,622.5 ns |  25,171.8 ns | 102,948.5 ns |    415,300.0 ns |     - |     - |     - |        1296 B |
|             SetExisting |    718,655.3 ns |  37,569.8 ns | 154,921.8 ns |    684,850.0 ns |     - |     - |     - |        1240 B |
|                 SetMany | 13,766,335.1 ns | 274,239.7 ns | 854,522.7 ns | 13,713,300.0 ns |     - |     - |     - |        1168 B |