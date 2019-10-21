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
|          SetupAndTeardown |        469.9 ns |       3.637 ns |       3.402 ns | 0.3390 |     - |     - |        1064 B |
|                       Set |        837.7 ns |       5.638 ns |       5.273 ns | 0.4177 |     - |     - |        1312 B |
|             Set_TwoLayers |      1,098.4 ns |       4.695 ns |       4.392 ns | 0.5302 |     - |     - |        1664 B |
|                     Evict |        949.5 ns |       8.219 ns |       7.688 ns | 0.4177 |     - |     - |        1312 B |
|           Evict_TwoLayers |      1,275.0 ns |       8.269 ns |       7.735 ns | 0.5302 |     - |     - |        1664 B |
|                   Cleanup |        991.2 ns |      10.777 ns |      10.081 ns | 0.4177 |     - |     - |        1312 B |
|         Cleanup_TwoLayers |      1,370.6 ns |      11.352 ns |      10.618 ns | 0.5302 |     - |     - |        1664 B |
|                   GetMiss |        579.6 ns |      10.564 ns |       9.365 ns | 0.3614 |     - |     - |        1136 B |
|                    GetHit |        984.6 ns |      11.510 ns |      10.767 ns | 0.4635 |     - |     - |        1456 B |
|                  GetOrSet |      2,443.7 ns |      21.829 ns |      20.419 ns | 0.7401 |     - |     - |        2328 B |
|  GetOrSet_TwoSimultaneous | 62,170,193.3 ns | 440,882.743 ns | 412,401.991 ns |      - |     - |     - |        3600 B |
| GetOrSet_FourSimultaneous | 62,174,628.1 ns | 382,121.655 ns | 357,436.833 ns |      - |     - |     - |        4496 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |        Error |        StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|-------------:|--------------:|-------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     130.6 us |     2.226 us |      1.973 us |     129.6 us |     1.00 |    0.00 |    35.1563 |     - |     - |  111000 B |
|        RedisCacheLayer |  51,928.1 us | 1,029.633 us |  2,544.998 us |  50,942.5 us |   428.18 |   13.73 |          - |     - |     - |     840 B |
| ProtobufFileCacheLayer | 222,912.8 us | 4,378.253 us |  6,686.055 us | 222,889.9 us | 1,724.04 |   50.39 |          - |     - |     - |   16816 B |
|     JsonFileCacheLayer | 267,009.2 us | 5,292.742 us |  9,407.843 us | 265,086.5 us | 2,036.97 |   78.87 |  1000.0000 |     - |     - |   13032 B |
|      MongoDbCacheLayer | 490,872.8 us | 9,731.806 us | 24,593.515 us | 492,071.8 us | 3,968.94 |  168.68 | 11000.0000 |     - |     - |   45152 B |

## In-Memory Benchmarks

### MemoryCacheLayer

|                  Method |   Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |------------:|-----------:|------------:|-------:|------:|------:|--------------:|
|                 GetMiss |    268.5 ns |   2.053 ns |    1.920 ns | 0.0892 |     - |     - |         280 B |
|                  GetHit |    572.6 ns |   6.720 ns |    6.286 ns | 0.1450 |     - |     - |         456 B |
|      GetHitSimultaneous |    653.7 ns |   8.282 ns |    7.342 ns | 0.1678 |     - |     - |         528 B |
|                  SetNew |    477.4 ns |   5.536 ns |    5.178 ns | 0.1221 |     - |     - |         384 B |
|             SetExisting |    714.4 ns |   9.202 ns |    8.608 ns | 0.1345 |     - |     - |         424 B |
| SetExistingSimultaneous |    990.4 ns |  10.608 ns |    9.922 ns | 0.1469 |     - |     - |         464 B |
|                 SetMany | 31,995.6 ns | 303.621 ns |  284.007 ns | 6.9580 |     - |     - |       21920 B |

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