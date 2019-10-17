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

|               Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|--------------------- |----------------:|---------------:|---------------:|-------:|------:|------:|--------------:|
|     SetupAndTeardown |        500.0 ns |       5.565 ns |       4.933 ns | 0.4997 |     - |     - |        1568 B |
|              GetMiss |        617.4 ns |       6.225 ns |       5.823 ns | 0.5226 |     - |     - |        1640 B |
|               GetHit |      1,088.7 ns |      11.158 ns |      10.437 ns | 0.6237 |     - |     - |        1960 B |
|             GetOrSet |      2,580.7 ns |      23.559 ns |      20.884 ns | 0.9460 |     - |     - |        2968 B |
| GetOrSetSimultaneous | 62,159,771.1 ns | 408,788.932 ns | 382,381.420 ns |      - |     - |     - |        5448 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |        Error |        StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|-------------:|--------------:|-------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     152.2 us |     1.213 us |      1.076 us |     152.3 us |     1.00 |    0.00 |    44.1895 |     - |     - |  139208 B |
|        RedisCacheLayer |  50,994.4 us | 1,232.881 us |  3,537.367 us |  49,445.9 us |   377.68 |   12.18 |          - |     - |     - |     840 B |
| ProtobufFileCacheLayer | 216,405.6 us | 4,303.942 us |  5,891.284 us | 215,675.0 us | 1,423.73 |   45.49 |          - |     - |     - |   16816 B |
|     JsonFileCacheLayer | 261,365.9 us | 5,115.156 us |  8,113.179 us | 260,971.6 us | 1,717.69 |   60.11 |  1000.0000 |     - |     - | 3034464 B |
|      MongoDbCacheLayer | 480,715.2 us | 9,562.321 us | 22,910.710 us | 482,436.5 us | 3,290.93 |  168.84 | 11000.0000 |     - |     - |   61376 B |

## In-Memory Benchmarks

### MemoryCacheLayer

|                  Method |   Mean [ns] | Error [ns] |  StdDev [ns] |   Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |------------:|-----------:|-------------:|--------:|------:|------:|--------------:|
|                 GetMiss |    263.8 ns |   5.107 ns |     7.160 ns |  0.2499 |     - |     - |         784 B |
|                  GetHit |    500.5 ns |   9.462 ns |     8.850 ns |  0.3052 |     - |     - |         960 B |
|      GetHitSimultaneous |    597.5 ns |  11.746 ns |    17.217 ns |  0.3290 |     - |     - |        1032 B |
|                  SetNew |    461.4 ns |  11.080 ns |    32.670 ns |  0.2828 |     - |     - |         888 B |
|             SetExisting |    668.6 ns |  13.347 ns |    12.485 ns |  0.3386 |     - |     - |        1064 B |
| SetExistingSimultaneous |    976.5 ns |  20.851 ns |    31.209 ns |  0.3948 |     - |     - |        1240 B |
|                 SetMany | 37,740.0 ns | 747.389 ns | 1,790.697 ns | 12.0239 |     - |     - |       37776 B |

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
|                 GetMiss |  67,312,750.5 ns | 1,829,057.4 ns | 7,664,143.3 ns |  66,021,250.0 ns |         - |     - |     - |       46112 B |
|                  GetHit |  70,373,239.7 ns | 1,969,885.4 ns | 8,319,108.0 ns |  68,596,900.0 ns |         - |     - |     - |       46128 B |
|      GetHitSimultaneous |  70,556,696.4 ns | 1,939,482.8 ns | 8,105,449.6 ns |  69,205,600.0 ns |         - |     - |     - |       46136 B |
|                  SetNew |  68,458,878.7 ns | 1,753,486.3 ns | 7,366,781.0 ns |  67,103,500.0 ns |         - |     - |     - |       46120 B |
|             SetExisting |  67,184,433.5 ns | 1,690,627.5 ns | 7,102,697.2 ns |  65,954,100.0 ns |         - |     - |     - |       46120 B |
| SetExistingSimultaneous |  68,841,099.5 ns | 1,744,541.0 ns | 7,252,101.9 ns |  68,265,100.0 ns |         - |     - |     - |       46128 B |
|                 SetMany | 127,680,529.0 ns | 2,553,497.0 ns | 7,803,844.4 ns | 125,743,400.0 ns | 2000.0000 |     - |     - |       46160 B |

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