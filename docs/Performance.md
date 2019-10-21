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

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|                 GetMiss |  2,340,184.2 ns |    51,383.6 ns |   208,403.0 ns |  2,324,500.0 ns |     - |     - |     - |       12800 B |
|                  GetHit |  4,304,379.9 ns |    93,759.6 ns |   385,572.8 ns |  4,227,500.0 ns |     - |     - |     - |       13208 B |
|      GetHitSimultaneous |  4,688,859.9 ns |   113,208.5 ns |   469,351.4 ns |  4,587,500.0 ns |     - |     - |     - |       13144 B |
|                  SetNew |  3,373,868.4 ns |    79,515.0 ns |   327,885.8 ns |  3,312,350.0 ns |     - |     - |     - |       13192 B |
|             SetExisting |  4,099,604.2 ns |    94,425.6 ns |   389,370.8 ns |  4,056,300.0 ns |     - |     - |     - |       13192 B |
| SetExistingSimultaneous |  4,856,573.7 ns |   113,313.8 ns |   472,305.2 ns |  4,832,100.0 ns |     - |     - |     - |       13208 B |
|                 SetMany | 86,122,224.4 ns | 1,693,499.7 ns | 3,222,059.1 ns | 85,759,100.0 ns |     - |     - |     - |       12912 B |

### ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|                 GetMiss |  1,271,346.6 ns |    35,358.0 ns |   145,404.7 ns |     - |     - |     - |        7016 B |
|                  GetHit |  2,999,372.6 ns |    72,460.4 ns |   298,795.4 ns |     - |     - |     - |        9184 B |
|      GetHitSimultaneous |  3,071,480.9 ns |    61,421.4 ns |   242,740.2 ns |     - |     - |     - |        9200 B |
|                  SetNew |  2,599,525.8 ns |    71,344.9 ns |   294,195.5 ns |     - |     - |     - |        9104 B |
|             SetExisting |  2,959,244.4 ns |    73,356.4 ns |   301,667.9 ns |     - |     - |     - |        9168 B |
| SetExistingSimultaneous |  3,311,651.1 ns |    79,057.1 ns |   325,997.4 ns |     - |     - |     - |        9184 B |
|                 SetMany | 54,680,116.9 ns | 1,092,052.1 ns | 2,419,912.0 ns |     - |     - |     - |        8848 B |

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