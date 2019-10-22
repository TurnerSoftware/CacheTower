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
|          SetupAndTeardown |        489.6 ns |       2.400 ns |       2.127 ns | 0.3386 |     - |     - |        1064 B |
|                       Set |        881.8 ns |       3.606 ns |       3.373 ns | 0.4177 |     - |     - |        1312 B |
|             Set_TwoLayers |      1,156.6 ns |       7.954 ns |       7.440 ns | 0.5302 |     - |     - |        1664 B |
|                     Evict |      1,007.2 ns |       5.244 ns |       4.905 ns | 0.4177 |     - |     - |        1312 B |
|           Evict_TwoLayers |      1,328.5 ns |       8.798 ns |       8.229 ns | 0.5302 |     - |     - |        1664 B |
|                   Cleanup |      1,198.0 ns |       4.993 ns |       4.426 ns | 0.4177 |     - |     - |        1312 B |
|         Cleanup_TwoLayers |      1,756.5 ns |       8.129 ns |       7.206 ns | 0.5302 |     - |     - |        1664 B |
|                   GetMiss |        635.2 ns |       3.860 ns |       3.422 ns | 0.3386 |     - |     - |        1064 B |
|                    GetHit |      1,099.9 ns |      14.481 ns |      12.837 ns | 0.4406 |     - |     - |        1384 B |
|                  GetOrSet |      2,644.8 ns |      22.942 ns |      21.460 ns | 0.6943 |     - |     - |        2184 B |
|  GetOrSet_TwoSimultaneous | 62,150,056.3 ns | 388,432.918 ns | 363,340.393 ns |      - |     - |     - |        3384 B |
| GetOrSet_FourSimultaneous | 62,165,273.3 ns | 380,129.959 ns | 355,573.800 ns |      - |     - |     - |        4136 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |        Error |        StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|-------------:|--------------:|-------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     128.6 us |     1.201 us |      1.123 us |     128.5 us |     1.00 |    0.00 |    30.7617 |     - |     - |   96600 B |
|        RedisCacheLayer |  48,684.7 us |   968.726 us |  2,376.302 us |  47,723.1 us |   409.60 |   11.57 |          - |     - |     - |     840 B |
| ProtobufFileCacheLayer | 209,649.9 us | 4,180.915 us |  5,996.143 us | 210,177.8 us | 1,639.47 |   47.25 |          - |     - |     - |   16824 B |
|     JsonFileCacheLayer | 256,823.7 us | 5,131.888 us |  8,431.839 us | 257,005.4 us | 2,002.03 |   65.46 |  1000.0000 |     - |     - |   13016 B |
|      MongoDbCacheLayer | 500,212.8 us | 9,844.756 us | 14,735.168 us | 498,753.5 us | 3,940.35 |  108.96 | 11000.0000 |     - |     - |   45152 B |

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
|                 GetMiss |    434,365.2 ns |  21,990.6 ns |  90,925.6 ns |    443,850.0 ns |     - |     - |     - |         672 B |
|                  GetHit |    796,113.8 ns |  35,921.3 ns | 147,317.2 ns |    805,050.0 ns |     - |     - |     - |        1320 B |
|      GetHitSimultaneous |    803,823.8 ns |  32,461.0 ns | 133,491.2 ns |    803,600.0 ns |     - |     - |     - |        1336 B |
|                  SetNew |    458,291.6 ns |  23,512.5 ns |  96,955.5 ns |    453,100.0 ns |     - |     - |     - |        1304 B |
|             SetExisting |    709,992.7 ns |  44,334.2 ns | 184,298.2 ns |    674,000.0 ns |     - |     - |     - |        1232 B |
| SetExistingSimultaneous |    737,590.6 ns |  33,309.7 ns | 137,727.2 ns |    738,400.0 ns |     - |     - |     - |        1248 B |
|                 SetMany | 13,530,768.7 ns | 273,468.5 ns | 868,173.5 ns | 13,260,300.0 ns |     - |     - |     - |        1168 B |