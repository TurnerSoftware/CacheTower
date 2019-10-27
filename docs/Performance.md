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

|                    Method |       Mean [ns] |    Error [ns] |   StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|-------------------------- |----------------:|--------------:|--------------:|-------:|------:|------:|--------------:|
|          SetupAndTeardown |        275.0 ns |       1.30 ns |       1.22 ns | 0.1326 |     - |     - |         416 B |
|                       Set |        678.8 ns |       3.35 ns |       3.13 ns | 0.1879 |     - |     - |         592 B |
|             Set_TwoLayers |        773.4 ns |       2.06 ns |       1.93 ns | 0.2575 |     - |     - |         808 B |
|                     Evict |        790.9 ns |       3.48 ns |       3.25 ns | 0.1879 |     - |     - |         592 B |
|           Evict_TwoLayers |        941.2 ns |       4.91 ns |       4.60 ns | 0.2575 |     - |     - |         808 B |
|                   Cleanup |        974.4 ns |       7.65 ns |       7.16 ns | 0.1869 |     - |     - |         592 B |
|         Cleanup_TwoLayers |      1,208.6 ns |       4.53 ns |       3.54 ns | 0.2575 |     - |     - |         808 B |
|                   GetMiss |        388.1 ns |       2.34 ns |       2.07 ns | 0.1326 |     - |     - |         416 B |
|                    GetHit |        800.7 ns |       5.10 ns |       4.52 ns | 0.1879 |     - |     - |         592 B |
|                  GetOrSet |      2,043.9 ns |      14.51 ns |      13.58 ns | 0.3357 |     - |     - |        1064 B |
|  GetOrSet_TwoSimultaneous | 62,232,054.8 ns | 329,492.60 ns | 308,207.58 ns |      - |     - |     - |        3325 B |
| GetOrSet_FourSimultaneous | 62,144,815.6 ns | 395,286.63 ns | 369,751.36 ns |      - |     - |     - |        3677 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |       Error |       StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|----------------------- |-------------:|------------:|-------------:|-------------:|---------:|--------:|-----------:|------:|------:|------------:|
|       MemoryCacheLayer |     124.3 us |     1.40 us |      1.17 us |     124.7 us |     1.00 |    0.00 |    30.7617 |     - |     - |    94.34 KB |
|        RedisCacheLayer |  50,689.9 us | 1,008.28 us |  2,529.59 us |  49,719.1 us |   439.95 |   23.84 |          - |     - |     - |   320.88 KB |
| ProtobufFileCacheLayer | 214,656.5 us | 4,261.38 us |  7,001.57 us | 214,159.9 us | 1,753.22 |   63.32 |          - |     - |     - |  1585.24 KB |
|     JsonFileCacheLayer | 262,911.9 us | 5,165.79 us |  9,182.18 us | 261,568.6 us | 2,103.16 |   74.19 |  1000.0000 |     - |     - |  3298.81 KB |
|      MongoDbCacheLayer | 466,444.2 us | 9,211.19 us | 20,024.37 us | 464,238.5 us | 3,859.08 |  207.18 | 11000.0000 |     - |     - | 33892.99 KB |

## In-Memory Benchmarks

### MemoryCacheLayer

|      Method |   Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------ |------------:|-----------:|------------:|-------:|------:|------:|--------------:|
|    Overhead |    142.7 ns |    1.15 ns |     1.02 ns | 0.0663 |     - |     - |         208 B |
|     GetMiss |    208.5 ns |    1.99 ns |     1.77 ns | 0.0663 |     - |     - |         208 B |
|      GetHit |    533.8 ns |    3.10 ns |     2.90 ns | 0.1221 |     - |     - |         384 B |
|      SetNew |    448.7 ns |    3.02 ns |     2.83 ns | 0.1221 |     - |     - |         384 B |
| SetExisting |    709.7 ns |    3.25 ns |     3.04 ns | 0.1345 |     - |     - |         424 B |
|     SetMany | 34,425.9 ns |  160.00 ns |   149.66 ns | 6.9580 |     - |     - |       21920 B |

## File System Benchmarks

### FileCacheLayerBase (Overhead)

|                  Method |       Mean [ns] |   Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|---------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  2,341,841.8 ns |  76,116.2 ns |   318,943.0 ns |     - |     - |     - |       16120 B |
| SetExistingSimultaneous |  2,339,074.7 ns |  75,701.7 ns |   312,161.1 ns |     - |     - |     - |       12664 B |
|                Overhead |  1,073,281.4 ns |  39,850.4 ns |   166,101.1 ns |     - |     - |     - |        4560 B |
|                 GetMiss |  1,482,845.9 ns |  50,339.9 ns |   209,822.5 ns |     - |     - |     - |        7400 B |
|                  GetHit |  2,113,604.9 ns |  58,434.3 ns |   237,663.9 ns |     - |     - |     - |       13424 B |
|                  SetNew |  1,957,598.5 ns |  59,701.2 ns |   249,502.0 ns |     - |     - |     - |        9856 B |
|             SetExisting |  2,160,977.6 ns |  70,054.7 ns |   293,544.4 ns |     - |     - |     - |       11704 B |
|                 SetMany | 44,174,175.0 ns | 877,421.9 ns | 2,102,246.8 ns |     - |     - |     - |      269352 B |

### JsonFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  5,034,252.0 ns |   159,425.6 ns |   671,531.7 ns |  4,960,550.0 ns |     - |     - |     - |       51816 B |
| SetExistingSimultaneous |  5,403,373.1 ns |   174,596.2 ns |   733,517.1 ns |  5,318,100.0 ns |     - |     - |     - |       51432 B |
|                Overhead |  1,668,915.7 ns |    55,233.5 ns |   232,048.2 ns |  1,618,800.0 ns |     - |     - |     - |       12488 B |
|                 GetMiss |  2,696,957.2 ns |    85,972.3 ns |   358,342.7 ns |  2,612,950.0 ns |     - |     - |     - |       23816 B |
|                  GetHit |  4,511,776.6 ns |   163,788.1 ns |   688,109.6 ns |  4,472,800.0 ns |     - |     - |     - |       42816 B |
|                  SetNew |  3,760,407.9 ns |   112,108.2 ns |   463,539.5 ns |  3,740,600.0 ns |     - |     - |     - |       33568 B |
|             SetExisting |  4,620,312.1 ns |   155,706.6 ns |   655,866.8 ns |  4,574,850.0 ns |     - |     - |     - |       42984 B |
|                 SetMany | 90,178,101.8 ns | 1,798,547.6 ns | 3,909,894.4 ns | 89,256,200.0 ns |     - |     - |     - |     1090800 B |

### ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  3,580,929.3 ns |   100,444.5 ns |   415,313.0 ns |     - |     - |     - |       21304 B |
| SetExistingSimultaneous |  3,978,112.4 ns |   112,489.1 ns |   467,620.1 ns |     - |     - |     - |       21240 B |
|                Overhead |  1,269,990.8 ns |    49,961.3 ns |   209,348.7 ns |     - |     - |     - |        4608 B |
|                 GetMiss |  1,697,858.4 ns |    54,100.6 ns |   223,087.7 ns |     - |     - |     - |        7440 B |
|                  GetHit |  3,366,020.6 ns |    93,677.2 ns |   385,234.3 ns |     - |     - |     - |       17768 B |
|                  SetNew |  3,222,964.6 ns |   103,708.6 ns |   429,965.9 ns |     - |     - |     - |       14040 B |
|             SetExisting |  3,634,562.3 ns |   114,139.8 ns |   471,939.9 ns |     - |     - |     - |       17856 B |
|                 SetMany | 62,429,050.0 ns | 1,259,236.3 ns | 2,365,151.0 ns |     - |     - |     - |      490696 B |

## Database Benchmarks

### MongoDbCacheLayer

|                  Method |        Mean [ns] |     Error [ns] |    StdDev [ns] |      Median [ns] |     Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |-----------------:|---------------:|---------------:|-----------------:|----------:|------:|------:|--------------:|
|      GetHitSimultaneous |  70,652,243.9 ns | 1,847,772.3 ns | 7,783,178.4 ns |  69,219,350.0 ns |         - |     - |     - |      277504 B |
| SetExistingSimultaneous |  70,761,139.7 ns | 1,785,866.2 ns | 7,443,701.3 ns |  69,789,050.0 ns |         - |     - |     - |      253016 B |
|                Overhead |       7,777.6 ns |       493.5 ns |     1,891.7 ns |       7,400.0 ns |         - |     - |     - |         424 B |
|                 GetMiss |  67,504,475.6 ns | 1,778,059.7 ns | 7,470,019.1 ns |  65,904,500.0 ns |         - |     - |     - |      121680 B |
|                  GetHit |  71,031,643.2 ns | 1,820,413.6 ns | 7,687,867.3 ns |  70,016,600.0 ns |         - |     - |     - |      198000 B |
|                  SetNew |  69,524,188.8 ns | 1,715,738.5 ns | 7,208,194.0 ns |  68,905,900.0 ns |         - |     - |     - |      109984 B |
|             SetExisting |  70,536,871.4 ns | 1,895,253.2 ns | 7,941,517.9 ns |  68,789,700.0 ns |         - |     - |     - |      181504 B |
|                 SetMany | 128,326,730.3 ns | 2,562,373.3 ns | 7,514,997.3 ns | 127,383,300.0 ns | 2000.0000 |     - |     - |     7294544 B |

## Other

### Redis

|                  Method |       Mean [ns] |   Error [ns] |  StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|-------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |    689,292.9 ns |  28,848.7 ns | 117,005.6 ns |    683,250.0 ns |     - |     - |     - |        3040 B |
| SetExistingSimultaneous |    694,434.0 ns |  30,077.4 ns | 123,011.6 ns |    688,150.0 ns |     - |     - |     - |        3232 B |
|                Overhead |      8,091.0 ns |   1,256.6 ns |   4,847.1 ns |      6,300.0 ns |     - |     - |     - |         136 B |
|                 GetMiss |    404,778.8 ns |  21,734.5 ns |  89,380.1 ns |    389,800.0 ns |     - |     - |     - |         840 B |
|                  GetHit |    663,348.3 ns |  22,604.0 ns |  90,642.7 ns |    658,800.0 ns |     - |     - |     - |        2296 B |
|                  SetNew |    424,456.9 ns |  24,487.5 ns | 100,426.2 ns |    400,350.0 ns |     - |     - |     - |        1304 B |
|             SetExisting |    694,905.3 ns |  28,549.4 ns | 117,084.4 ns |    684,850.0 ns |     - |     - |     - |        2288 B |
|                 SetMany | 13,767,330.3 ns | 268,302.3 ns | 985,616.9 ns | 13,577,950.0 ns |     - |     - |     - |       97456 B |


## Extensions

### RedisRemoteEvictionExtension

|         Method |   Mean [ns] | Error [ns] | StdDev [ns] | Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|--------------- |------------:|-----------:|------------:|------------:|------:|------:|------:|--------------:|
| OnValueRefresh | 50,831.5 ns | 7,238.7 ns | 29,110.7 ns | 37,700.0 ns |     - |     - |     - |        1000 B |
|       Overhead |  6,028.0 ns |   574.0 ns |  2,193.5 ns |  5,450.0 ns |     - |     - |     - |         352 B |
|       Register | 11,579.1 ns |   868.4 ns |  3,255.1 ns | 10,950.0 ns |     - |     - |     - |         568 B |

### RedisLockExtension

|       Method |    Mean [ns] |  Error [ns] | StdDev [ns] |  Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------- |-------------:|------------:|------------:|-------------:|------:|------:|------:|--------------:|
| RefreshValue | 365,707.6 ns | 16,378.3 ns | 66,613.7 ns | 358,900.0 ns |     - |     - |     - |       11848 B |
|     Overhead |  16,570.1 ns |  2,097.4 ns |  8,014.9 ns |  14,750.0 ns |     - |     - |     - |        1128 B |
|     Register |  25,740.4 ns |  4,293.5 ns | 17,118.0 ns |  17,850.0 ns |     - |     - |     - |        1128 B |