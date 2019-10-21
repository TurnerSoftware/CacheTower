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

|                 Method |         Mean |         Error |         StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|--------------:|---------------:|-------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     127.1 us |     0.6869 us |      0.6425 us |     127.1 us |     1.00 |    0.00 |    30.7617 |     - |     - |   96600 B |
|        RedisCacheLayer |  52,279.4 us | 1,377.8691 us |  4,019.3089 us |  50,232.0 us |   462.05 |   19.27 |          - |     - |     - |     840 B |
| ProtobufFileCacheLayer | 210,497.9 us | 4,193.3948 us |  6,771.5510 us | 209,521.9 us | 1,634.41 |   53.48 |          - |     - |     - |   16784 B |
|     JsonFileCacheLayer | 265,080.5 us | 5,170.8486 us |  7,248.8076 us | 264,019.5 us | 2,105.12 |   57.40 |  1000.0000 |     - |     - |   13320 B |
|      MongoDbCacheLayer | 491,621.7 us | 9,705.1026 us | 20,258.1631 us | 493,954.0 us | 3,872.12 |  228.72 | 11000.0000 |     - |     - |   45152 B |

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
|                 GetMiss |  1,281,926.0 ns |  46,841.7 ns |   194,201.4 ns |  1,237,800.0 ns |     - |     - |     - |        6680 B |
|                  GetHit |  2,111,450.8 ns |  76,632.0 ns |   321,947.8 ns |  2,089,500.0 ns |     - |     - |     - |       10336 B |
|      GetHitSimultaneous |  2,256,543.9 ns |  67,694.5 ns |   285,142.5 ns |  2,236,050.0 ns |     - |     - |     - |       11760 B |
|                  SetNew |  1,700,250.3 ns |  54,588.8 ns |   222,023.5 ns |  1,670,400.0 ns |     - |     - |     - |        8992 B |
|             SetExisting |  1,998,325.3 ns |  71,329.0 ns |   297,307.7 ns |  1,971,000.0 ns |     - |     - |     - |       10336 B |
| SetExistingSimultaneous |  2,112,100.5 ns |  69,023.0 ns |   283,847.2 ns |  2,099,600.0 ns |     - |     - |     - |       11872 B |
|                 SetMany | 40,403,148.6 ns | 818,490.7 ns | 1,344,803.7 ns | 40,151,400.0 ns |     - |     - |     - |      261376 B |

### JsonFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  4,943,068.7 ns |   176,246.7 ns |   742,385.4 ns |     - |     - |     - |       13408 B |
| SetExistingSimultaneous |  5,385,037.1 ns |   173,781.6 ns |   730,094.7 ns |     - |     - |     - |       13072 B |
|                Overhead |  1,709,841.5 ns |    60,401.8 ns |   251,092.0 ns |     - |     - |     - |       11976 B |
|                 GetMiss |  2,719,034.7 ns |    94,216.2 ns |   394,786.2 ns |     - |     - |     - |       13232 B |
|                  GetHit |  4,526,985.4 ns |   164,361.5 ns |   686,896.4 ns |     - |     - |     - |       13336 B |
|                  SetNew |  3,840,744.1 ns |   130,142.1 ns |   543,887.6 ns |     - |     - |     - |       13376 B |
|             SetExisting |  4,691,545.9 ns |   151,706.6 ns |   632,331.0 ns |     - |     - |     - |       13392 B |
|                 SetMany | 84,695,738.9 ns | 1,680,934.4 ns | 3,545,661.2 ns |     - |     - |     - |       13048 B |

### ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  3,328,122.8 ns |   112,135.6 ns |   466,150.7 ns |     - |     - |     - |        9448 B |
| SetExistingSimultaneous |  3,799,503.7 ns |   116,817.2 ns |   481,703.9 ns |     - |     - |     - |        9432 B |
|                Overhead |  1,191,453.9 ns |    46,952.1 ns |   194,135.2 ns |     - |     - |     - |        4664 B |
|                 GetMiss |  1,592,036.2 ns |    55,088.2 ns |   224,054.5 ns |     - |     - |     - |        7440 B |
|                  GetHit |  3,214,307.2 ns |   100,680.8 ns |   419,649.4 ns |     - |     - |     - |        9448 B |
|                  SetNew |  3,041,033.2 ns |   108,489.0 ns |   450,991.7 ns |     - |     - |     - |        9280 B |
|             SetExisting |  3,378,383.2 ns |   115,940.8 ns |   479,386.3 ns |     - |     - |     - |        9432 B |
|                 SetMany | 56,135,765.0 ns | 1,115,750.2 ns | 1,983,244.8 ns |     - |     - |     - |        8984 B |

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