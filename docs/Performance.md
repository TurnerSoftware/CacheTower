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
|          SetupAndTeardown |        378.7 ns |       1.933 ns |       1.714 ns | 0.3290 |     - |     - |        1032 B |
|                       Set |        739.3 ns |       9.725 ns |       9.097 ns | 0.4072 |     - |     - |        1280 B |
|             Set_TwoLayers |        936.3 ns |      16.343 ns |      15.287 ns | 0.5198 |     - |     - |        1632 B |
|                     Evict |        880.9 ns |       9.571 ns |       8.953 ns | 0.4072 |     - |     - |        1280 B |
|           Evict_TwoLayers |      1,094.2 ns |      20.696 ns |      22.145 ns | 0.5188 |     - |     - |        1632 B |
|                   Cleanup |      1,017.9 ns |      19.565 ns |      19.216 ns | 0.4063 |     - |     - |        1280 B |
|         Cleanup_TwoLayers |      1,487.9 ns |      28.850 ns |      28.334 ns | 0.5188 |     - |     - |        1632 B |
|                   GetMiss |        481.4 ns |       9.470 ns |      13.276 ns | 0.3281 |     - |     - |        1032 B |
|                    GetHit |        863.4 ns |       4.342 ns |       3.849 ns | 0.4301 |     - |     - |        1352 B |
|                  GetOrSet |      2,221.6 ns |      43.897 ns |      41.061 ns | 0.6790 |     - |     - |        2136 B |
|  GetOrSet_TwoSimultaneous | 62,177,148.9 ns | 356,653.407 ns | 333,613.818 ns |      - |     - |     - |        3216 B |
| GetOrSet_FourSimultaneous | 62,158,020.7 ns | 440,775.969 ns | 412,302.115 ns |      - |     - |     - |        3888 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |        Error |        StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------- |-------------:|-------------:|--------------:|-------------:|---------:|--------:|-----------:|------:|------:|----------:|
|       MemoryCacheLayer |     118.4 us |     1.757 us |      1.644 us |     117.9 us |     1.00 |    0.00 |    30.7617 |     - |     - |   96600 B |
|        RedisCacheLayer |  50,350.9 us | 1,004.952 us |  2,407.801 us |  49,419.3 us |   458.49 |   13.88 |          - |     - |     - |     832 B |
| ProtobufFileCacheLayer | 209,781.6 us | 4,106.941 us |  6,147.075 us | 208,410.6 us | 1,791.86 |   54.40 |          - |     - |     - |   16664 B |
|     JsonFileCacheLayer | 253,756.8 us | 5,062.470 us |  7,260.444 us | 251,206.0 us | 2,145.51 |   64.72 |  1000.0000 |     - |     - |   13016 B |
|      MongoDbCacheLayer | 456,320.5 us | 9,425.109 us | 23,818.454 us | 456,467.4 us | 4,114.10 |  182.18 | 11000.0000 |     - |     - |   45032 B |

## In-Memory Benchmarks

### MemoryCacheLayer

|      Method |   Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------ |------------:|-----------:|------------:|-------:|------:|------:|--------------:|
|    Overhead |    132.3 ns |   2.366 ns |    2.097 ns | 0.0663 |     - |     - |         208 B |
|     GetMiss |    200.1 ns |   2.638 ns |    2.338 ns | 0.0663 |     - |     - |         208 B |
|      GetHit |    196.7 ns |   2.583 ns |    2.416 ns | 0.0663 |     - |     - |         208 B |
|      SetNew |    411.8 ns |   4.650 ns |    4.350 ns | 0.1221 |     - |     - |         384 B |
| SetExisting |    663.1 ns |   6.619 ns |    5.868 ns | 0.1345 |     - |     - |         424 B |
|     SetMany | 32,367.1 ns | 312.875 ns |  292.663 ns | 6.9580 |     - |     - |       21920 B |

## File System Benchmarks

### FileCacheLayerBase (Overhead)

|                  Method |       Mean [ns] |   Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  2,091,098.4 ns |  56,988.4 ns |   234,995.6 ns |  2,057,400.0 ns |     - |     - |     - |       12800 B |
| SetExistingSimultaneous |  2,157,005.6 ns |  60,951.4 ns |   254,727.0 ns |  2,135,400.0 ns |     - |     - |     - |       12824 B |
|                Overhead |  1,008,691.4 ns |  32,755.8 ns |   137,614.4 ns |    988,300.0 ns |     - |     - |     - |        4560 B |
|                 GetMiss |  1,386,150.0 ns |  44,570.1 ns |   184,783.4 ns |  1,346,750.0 ns |     - |     - |     - |        7400 B |
|                  GetHit |  2,095,189.0 ns |  64,793.7 ns |   267,905.9 ns |  2,050,900.0 ns |     - |     - |     - |       10600 B |
|                  SetNew |  1,741,728.3 ns |  46,071.4 ns |   188,424.5 ns |  1,710,100.0 ns |     - |     - |     - |        9856 B |
|             SetExisting |  1,995,602.6 ns |  58,967.1 ns |   244,472.0 ns |  1,944,750.0 ns |     - |     - |     - |       11704 B |
|                 SetMany | 44,054,819.0 ns | 855,848.3 ns | 1,018,826.6 ns | 44,217,100.0 ns |     - |     - |     - |      267288 B |

### JsonFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  4,426,663.5 ns |   112,568.5 ns |   466,698.1 ns |  4,354,050.0 ns |     - |     - |     - |       13120 B |
| SetExistingSimultaneous |  4,832,333.5 ns |   129,430.6 ns |   539,481.9 ns |  4,830,800.0 ns |     - |     - |     - |       12928 B |
|                Overhead |  1,645,538.9 ns |    53,738.9 ns |   223,394.1 ns |  1,628,800.0 ns |     - |     - |     - |       11760 B |
|                 GetMiss |  2,561,571.0 ns |    90,549.0 ns |   376,414.6 ns |  2,553,300.0 ns |     - |     - |     - |       13088 B |
|                  GetHit |  4,164,799.0 ns |   117,258.0 ns |   486,140.4 ns |  4,080,400.0 ns |     - |     - |     - |       13192 B |
|                  SetNew |  3,563,922.1 ns |   104,802.8 ns |   437,990.0 ns |  3,476,200.0 ns |     - |     - |     - |       13160 B |
|             SetExisting |  4,197,750.0 ns |   113,722.7 ns |   474,009.7 ns |  4,147,100.0 ns |     - |     - |     - |       13176 B |
|                 SetMany | 82,816,630.8 ns | 1,438,188.7 ns | 1,200,952.7 ns | 83,132,000.0 ns |     - |     - |     - |       13048 B |

### ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  3,120,377.0 ns |    78,879.2 ns |   326,145.7 ns |  3,051,300.0 ns |     - |     - |     - |        9224 B |
| SetExistingSimultaneous |  3,590,853.4 ns |    96,998.0 ns |   403,223.2 ns |  3,565,400.0 ns |     - |     - |     - |        9208 B |
|                Overhead |  1,092,430.1 ns |    32,067.5 ns |   130,788.3 ns |  1,073,300.0 ns |     - |     - |     - |        4608 B |
|                 GetMiss |  1,521,908.7 ns |    49,742.5 ns |   207,883.1 ns |  1,483,000.0 ns |     - |     - |     - |        7600 B |
|                  GetHit |  3,089,942.1 ns |    98,912.6 ns |   413,373.5 ns |  3,019,000.0 ns |     - |     - |     - |        9224 B |
|                  SetNew |  2,925,641.8 ns |    92,330.1 ns |   386,882.8 ns |  2,875,900.0 ns |     - |     - |     - |        9112 B |
|             SetExisting |  3,205,230.2 ns |    86,018.7 ns |   356,625.3 ns |  3,189,250.0 ns |     - |     - |     - |        9208 B |
|                 SetMany | 60,027,888.0 ns | 1,259,421.7 ns | 1,681,291.8 ns | 60,097,600.0 ns |     - |     - |     - |        9008 B |

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
| OnValueRefresh | 48,050.8 ns | 6,005.8 ns | 24,014.1 ns | 38,500.0 ns |     - |     - |     - |         976 B |
|       Overhead |  5,944.7 ns |   432.2 ns |  1,728.1 ns |  5,600.0 ns |     - |     - |     - |         352 B |
|       Register | 12,197.0 ns | 1,205.9 ns |  4,608.2 ns | 11,200.0 ns |     - |     - |     - |         568 B |

### RedisLockExtension

|       Method |    Mean [ns] |  Error [ns] | StdDev [ns] |  Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------- |-------------:|------------:|------------:|-------------:|------:|------:|------:|--------------:|
| RefreshValue | 365,785.6 ns | 21,622.3 ns | 88,431.8 ns | 359,500.0 ns |     - |     - |     - |        2000 B |
|     Overhead |  19,767.2 ns |  2,950.8 ns | 11,627.3 ns |  15,050.0 ns |     - |     - |     - |        1128 B |
|     Register |  17,637.8 ns |  1,661.6 ns |  6,508.3 ns |  16,400.0 ns |     - |     - |     - |        1128 B |