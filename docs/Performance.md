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

|                 Method |         Mean |        Error |         StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|-------------:|---------------:|-------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     127.3 us |     1.138 us |      0.9506 us |     127.3 us |     1.00 |    0.00 |    30.7617 |     - |     - |   96600 B |
|        RedisCacheLayer |  51,948.3 us | 1,143.442 us |  3,032.2452 us |  50,881.3 us |   452.25 |   15.45 |          - |     - |     - |     832 B |
| ProtobufFileCacheLayer | 218,407.3 us | 4,320.885 us |  7,900.9821 us | 217,617.4 us | 1,738.22 |   84.23 |          - |     - |     - |   16664 B |
|     JsonFileCacheLayer | 267,451.9 us | 5,154.154 us |  5,514.8874 us | 267,027.1 us | 2,105.71 |   48.91 |  1000.0000 |     - |     - |   13176 B |
|      MongoDbCacheLayer | 484,016.5 us | 9,502.061 us | 20,043.0717 us | 481,374.3 us | 3,863.60 |  105.21 | 11000.0000 |     - |     - |   45032 B |

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
|      GetHitSimultaneous |  74,843,634.2 ns | 2,097,790.9 ns | 8,859,271.3 ns |  74,105,100.0 ns |         - |     - |     - |       29792 B |
| SetExistingSimultaneous |  70,319,325.1 ns | 2,089,282.0 ns | 8,823,336.7 ns |  69,083,500.0 ns |         - |     - |     - |       29784 B |
|                Overhead |       6,810.9 ns |       505.6 ns |     1,992.2 ns |       6,200.0 ns |         - |     - |     - |         424 B |
|                 GetMiss |  67,708,003.1 ns | 1,517,429.5 ns | 6,291,116.8 ns |  67,191,050.0 ns |         - |     - |     - |       29768 B |
|                  GetHit |  72,197,165.3 ns | 2,041,595.2 ns | 8,621,948.7 ns |  70,447,000.0 ns |         - |     - |     - |       29792 B |
|                  SetNew |  70,021,427.3 ns | 1,698,603.8 ns | 7,154,851.3 ns |  68,528,900.0 ns |         - |     - |     - |       29776 B |
|             SetExisting |  71,360,042.2 ns | 1,836,259.8 ns | 7,754,788.1 ns |  69,742,700.0 ns |         - |     - |     - |       29784 B |
|                 SetMany | 133,314,422.1 ns | 2,672,796.6 ns | 9,580,578.0 ns | 131,180,800.0 ns | 2000.0000 |     - |     - |       29824 B |

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


## Extensions

### RedisRemoteEvictionExtension

|         Method |   Mean [ns] | Error [ns] | StdDev [ns] | Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|--------------- |------------:|-----------:|------------:|------------:|------:|------:|------:|--------------:|
| OnValueRefresh | 68,108.3 ns | 8,428.0 ns | 33,893.6 ns | 60,500.0 ns |     - |     - |     - |         984 B |
|       Overhead | 14,151.9 ns | 3,183.4 ns | 12,802.1 ns |  8,100.0 ns |     - |     - |     - |         360 B |
|       Register | 23,652.5 ns | 3,855.3 ns | 15,415.6 ns | 16,300.0 ns |     - |     - |     - |         576 B |