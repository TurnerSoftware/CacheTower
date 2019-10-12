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

## Comparison Benchmark

|                 Method |         Mean |         Error |        StdDev |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|--------------:|--------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     179.7 us |      3.528 us |      3.300 us |     1.00 |    0.00 |    39.7949 |     - |     - | 122.28 KB |
|     JsonFileCacheLayer | 355,699.6 us |  6,982.727 us |  8,830.914 us | 1,972.87 |   62.05 |  1000.0000 |     - |     - |   37.2 KB |
| ProtobufFileCacheLayer | 249,569.9 us |  4,938.428 us |  6,064.833 us | 1,386.50 |   32.39 |          - |     - |     - |   9.14 KB |
|      MongoDbCacheLayer | 996,704.0 us | 19,785.338 us | 27,736.281 us | 5,605.15 |  236.00 | 33000.0000 |     - |     - | 394.85 KB |

## Individual Benchmarks

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

### MongoDbCacheLayer

|                  Method |        Mean [ns] |     Error [ns] |     StdDev [ns] |      Median [ns] |     Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |-----------------:|---------------:|----------------:|-----------------:|----------:|------:|------:|--------------:|
|                 GetMiss |   1,850,303.2 ns |    67,453.3 ns |    274,345.8 ns |   1,784,400.0 ns |         - |     - |     - |       94536 B |
|                  GetHit |  68,589,052.3 ns | 1,998,220.0 ns |  8,394,961.1 ns |  66,567,500.0 ns |         - |     - |     - |      140496 B |
|      GetHitSimultaneous |  71,153,578.5 ns | 1,771,531.9 ns |  7,500,776.4 ns |  69,892,350.0 ns |         - |     - |     - |      140928 B |
|                  SetNew |  68,047,650.0 ns | 1,679,722.1 ns |  7,038,396.4 ns |  66,889,550.0 ns |         - |     - |     - |      142248 B |
|             SetExisting |  68,403,433.0 ns | 1,818,346.2 ns |  7,639,271.8 ns |  67,323,200.0 ns |         - |     - |     - |      142272 B |
| SetExistingSimultaneous |  70,529,574.1 ns | 1,865,530.6 ns |  7,837,503.7 ns |  69,525,000.0 ns |         - |     - |     - |      141144 B |
|                 SetMany | 269,710,224.7 ns | 5,367,795.8 ns | 13,759,712.8 ns | 268,828,500.0 ns | 5000.0000 |     - |     - |      140720 B |