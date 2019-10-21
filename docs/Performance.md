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
|          SetupAndTeardown |        484.8 ns |       6.192 ns |       5.792 ns | 0.3386 |     - |     - |        1064 B |
|                       Set |        871.7 ns |       7.237 ns |       6.415 ns | 0.4177 |     - |     - |        1312 B |
|             Set_TwoLayers |      1,143.9 ns |       9.170 ns |       8.578 ns | 0.5302 |     - |     - |        1664 B |
|                     Evict |      1,010.5 ns |       5.219 ns |       4.627 ns | 0.4177 |     - |     - |        1312 B |
|           Evict_TwoLayers |      1,345.6 ns |      11.045 ns |       9.791 ns | 0.5302 |     - |     - |        1664 B |
|                   Cleanup |      1,220.5 ns |       7.552 ns |       6.695 ns | 0.4177 |     - |     - |        1312 B |
|         Cleanup_TwoLayers |      1,772.6 ns |      10.120 ns |       8.451 ns | 0.5302 |     - |     - |        1664 B |
|                   GetMiss |        623.4 ns |       3.014 ns |       2.820 ns | 0.3386 |     - |     - |        1064 B |
|                    GetHit |      1,037.1 ns |       7.260 ns |       6.436 ns | 0.4406 |     - |     - |        1384 B |
|                  GetOrSet |      2,593.6 ns |      19.322 ns |      18.074 ns | 0.6943 |     - |     - |        2184 B |
|  GetOrSet_TwoSimultaneous | 62,222,883.7 ns | 306,855.105 ns | 287,032.456 ns |      - |     - |     - |        3384 B |
| GetOrSet_FourSimultaneous | 62,085,778.3 ns | 480,004.197 ns | 448,996.224 ns |      - |     - |     - |        4136 B |

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
|                 GetMiss |  2,308,677.4 ns |    73,558.0 ns |   300,008.9 ns |     - |     - |     - |       13232 B |
|                  GetHit |  3,745,053.1 ns |   117,034.9 ns |   487,815.3 ns |     - |     - |     - |       13392 B |
|      GetHitSimultaneous |  3,983,519.7 ns |   112,228.5 ns |   466,536.6 ns |     - |     - |     - |       13408 B |
|                  SetNew |  3,201,495.8 ns |    99,066.0 ns |   410,718.2 ns |     - |     - |     - |       13376 B |
|             SetExisting |  3,791,254.9 ns |   114,299.5 ns |   477,678.4 ns |     - |     - |     - |       13376 B |
| SetExistingSimultaneous |  4,490,886.7 ns |   133,586.2 ns |   558,281.0 ns |     - |     - |     - |       13224 B |
|                 SetMany | 82,016,744.0 ns | 1,631,748.6 ns | 4,383,586.5 ns |     - |     - |     - |       13040 B |

### ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|                 GetMiss |  1,314,230.0 ns |    39,587.0 ns |   163,239.9 ns |     - |     - |     - |        7264 B |
|                  GetHit |  2,744,315.1 ns |    85,995.6 ns |   356,529.3 ns |     - |     - |     - |        9432 B |
|      GetHitSimultaneous |  2,798,252.7 ns |    78,387.7 ns |   317,927.3 ns |     - |     - |     - |        9448 B |
|                  SetNew |  2,663,939.2 ns |    87,745.4 ns |   365,733.3 ns |     - |     - |     - |        9416 B |
|             SetExisting |  2,970,032.6 ns |    88,565.7 ns |   368,169.9 ns |     - |     - |     - |        9416 B |
| SetExistingSimultaneous |  3,272,453.6 ns |    95,933.7 ns |   399,862.9 ns |     - |     - |     - |        9144 B |
|                 SetMany | 56,809,215.7 ns | 1,112,798.1 ns | 2,273,151.7 ns |     - |     - |     - |        8976 B |

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