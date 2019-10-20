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
|          SetupAndTeardown |        497.7 ns |       9.551 ns |       9.380 ns | 0.4997 |     - |     - |        1568 B |
|                       Set |        944.8 ns |       7.476 ns |       6.993 ns | 0.5789 |     - |     - |        1816 B |
|             Set_TwoLayers |      1,213.6 ns |      15.993 ns |      14.177 ns | 0.8507 |     - |     - |        2672 B |
|                     Evict |      1,086.6 ns |       6.612 ns |       6.185 ns | 0.5779 |     - |     - |        1816 B |
|           Evict_TwoLayers |      1,445.2 ns |      25.493 ns |      23.846 ns | 0.8507 |     - |     - |        2672 B |
|                   Cleanup |      1,300.9 ns |      16.284 ns |      15.232 ns | 0.5989 |     - |     - |        1880 B |
|         Cleanup_TwoLayers |      1,923.6 ns |      18.760 ns |      15.666 ns | 0.8926 |     - |     - |        2800 B |
|                   GetMiss |        616.8 ns |      10.421 ns |       9.748 ns | 0.5226 |     - |     - |        1640 B |
|                    GetHit |      1,019.7 ns |      11.824 ns |      11.061 ns | 0.6237 |     - |     - |        1960 B |
|                  GetOrSet |      2,629.3 ns |      19.201 ns |      17.961 ns | 0.9460 |     - |     - |        2968 B |
|  GetOrSet_TwoSimultaneous | 62,235,349.6 ns | 228,505.370 ns | 213,744.065 ns |      - |     - |     - |        4104 B |
| GetOrSet_FourSimultaneous | 62,216,323.7 ns | 334,337.878 ns | 312,739.859 ns |      - |     - |     - |        5000 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |        Error |        StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|-------------:|--------------:|-------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     129.5 us |     1.813 us |      1.696 us |     129.3 us |     1.00 |    0.00 |    35.1563 |     - |     - |  111000 B |
|        RedisCacheLayer |  51,005.3 us | 1,060.949 us |  2,479.935 us |  50,179.2 us |   423.89 |   14.55 |          - |     - |     - |     840 B |
| ProtobufFileCacheLayer | 216,167.6 us | 4,017.195 us |  3,757.687 us | 216,462.6 us | 1,669.69 |   35.25 |          - |     - |     - |   16816 B |
|     JsonFileCacheLayer | 263,159.9 us | 5,165.585 us |  8,193.165 us | 263,139.1 us | 2,025.91 |   69.27 |  1000.0000 |     - |     - | 3022792 B |
|      MongoDbCacheLayer | 456,742.1 us | 9,124.671 us | 20,965.448 us | 456,641.1 us | 3,585.83 |  221.79 | 11000.0000 |     - |     - |   45152 B |

## In-Memory Benchmarks

### MemoryCacheLayer

|                  Method |   Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |------------:|-----------:|------------:|-------:|------:|------:|--------------:|
|                 GetMiss |    264.6 ns |   3.055 ns |    2.708 ns | 0.0892 |     - |     - |         280 B |
|                  GetHit |    564.6 ns |   4.581 ns |    4.285 ns | 0.1450 |     - |     - |         456 B |
|      GetHitSimultaneous |    665.1 ns |   5.320 ns |    4.976 ns | 0.1678 |     - |     - |         528 B |
|                  SetNew |    473.1 ns |   5.679 ns |    4.742 ns | 0.1221 |     - |     - |         384 B |
|             SetExisting |    708.8 ns |   4.888 ns |    4.572 ns | 0.1345 |     - |     - |         424 B |
| SetExistingSimultaneous |    967.3 ns |   8.977 ns |    7.958 ns | 0.1469 |     - |     - |         464 B |
|                 SetMany | 32,058.1 ns | 426.115 ns |  398.589 ns | 6.9580 |     - |     - |       21920 B |

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
|                 GetMiss |  2,226,865.3 ns |    65,582.4 ns |   270,433.4 ns |  2,192,450.0 ns |     - |     - |     - |       28832 B |
|                  GetHit |  4,155,211.4 ns |   123,362.9 ns |   512,822.7 ns |  4,035,100.0 ns |     - |     - |     - |       38464 B |
|      GetHitSimultaneous |  4,406,864.6 ns |   115,671.7 ns |   483,412.9 ns |  4,362,300.0 ns |     - |     - |     - |       46344 B |
|                  SetNew |  3,206,788.1 ns |    97,397.1 ns |   404,882.1 ns |  3,160,800.0 ns |     - |     - |     - |       44064 B |
|             SetExisting |  3,831,830.5 ns |    98,102.6 ns |   404,533.1 ns |  3,806,800.0 ns |     - |     - |     - |       55848 B |
| SetExistingSimultaneous |  4,536,801.5 ns |   126,976.1 ns |   529,251.3 ns |  4,523,300.0 ns |     - |     - |     - |       68152 B |
|                 SetMany | 82,476,826.5 ns | 1,643,888.4 ns | 2,654,573.3 ns | 82,139,100.0 ns |     - |     - |     - |     1511520 B |

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