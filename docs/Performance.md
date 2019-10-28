# Performance

The performance figures below are as a guide only. Different systems and configurations can drastically change performance results.
Run the benchmark suite on your target machine to best gauge how the performance is for your use case.

Regarding specific tests, it is best to look at the implementations themselves to understand what the test is doing in that time/allocation allotment.

**Test Machine**

```
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  Job-CQNZSR : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT

Runtime=.NET Core 3.0  MaxIterationCount=200
UnrollFactor=1
```

## Cache Stack Benchmark

|                    Method |       Mean [ns] |    Error [ns] |   StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|-------------------------- |----------------:|--------------:|--------------:|-------:|------:|------:|--------------:|
|          SetupAndTeardown |        269.3 ns |       4.63 ns |       4.11 ns | 0.1326 |     - |     - |         416 B |
|                       Set |        661.3 ns |       6.44 ns |       5.38 ns | 0.1860 |     - |     - |         584 B |
|             Set_TwoLayers |        869.1 ns |      16.48 ns |      17.63 ns | 0.2975 |     - |     - |         936 B |
|                     Evict |        799.2 ns |      14.25 ns |      13.33 ns | 0.1860 |     - |     - |         584 B |
|           Evict_TwoLayers |      1,030.8 ns |      14.00 ns |      13.09 ns | 0.2975 |     - |     - |         936 B |
|                   Cleanup |        958.3 ns |       6.89 ns |       6.11 ns | 0.1850 |     - |     - |         584 B |
|         Cleanup_TwoLayers |      1,421.2 ns |      17.53 ns |      15.54 ns | 0.2975 |     - |     - |         936 B |
|                   GetMiss |        375.4 ns |       3.88 ns |       3.63 ns | 0.1326 |     - |     - |         416 B |
|                    GetHit |        793.1 ns |      12.73 ns |      11.91 ns | 0.1860 |     - |     - |         584 B |
|                  GetOrSet |      1,916.0 ns |      21.73 ns |      18.15 ns | 0.3319 |     - |     - |        1048 B |
|  GetOrSet_TwoSimultaneous | 62,147,289.6 ns | 459,902.87 ns | 430,193.43 ns |      - |     - |     - |        2322 B |
| GetOrSet_FourSimultaneous | 62,142,030.0 ns | 393,933.53 ns | 368,485.67 ns |      - |     - |     - |        2831 B |

## Cache Layer Comparison Benchmark

|                 Method |         Mean |       Error |       StdDev |       Median |    Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|----------------------- |-------------:|------------:|-------------:|-------------:|---------:|--------:|-----------:|------:|------:|------------:|
|       MemoryCacheLayer |     118.7 us |     2.26 us |      2.12 us |     118.4 us |     1.00 |    0.00 |    30.2734 |     - |     - |    92.77 KB |
|        RedisCacheLayer |  48,898.5 us |   970.75 us |  2,488.40 us |  47,959.8 us |   442.92 |   16.99 |          - |     - |     - |   315.05 KB |
| ProtobufFileCacheLayer | 225,156.9 us | 3,912.92 us |  3,468.70 us | 224,579.6 us | 1,896.36 |   47.60 |          - |     - |     - |  1612.24 KB |
|     JsonFileCacheLayer | 279,037.0 us | 5,569.81 us | 11,869.73 us | 275,174.1 us | 2,450.32 |  112.80 |  1000.0000 |     - |     - |  3314.62 KB |
|      MongoDbCacheLayer | 484,883.7 us | 9,534.35 us | 19,901.73 us | 481,217.7 us | 4,177.79 |  176.27 | 10000.0000 |     - |     - | 30962.02 KB |

## In-Memory Benchmarks

### MemoryCacheLayer

|      Method |   Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------ |------------:|-----------:|------------:|-------:|------:|------:|--------------:|
|    Overhead |    135.1 ns |    1.09 ns |     0.97 ns | 0.0663 |     - |     - |         208 B |
|     GetMiss |    212.0 ns |    1.64 ns |     1.53 ns | 0.0663 |     - |     - |         208 B |
|      GetHit |    533.4 ns |   10.20 ns |    10.48 ns | 0.1192 |     - |     - |         376 B |
|      SetNew |    433.4 ns |    6.15 ns |     5.45 ns | 0.1197 |     - |     - |         376 B |
| SetExisting |    745.5 ns |   10.43 ns |     9.24 ns | 0.1297 |     - |     - |         408 B |
|     SetMany | 33,831.8 ns |  483.94 ns |   429.00 ns | 6.7139 |     - |     - |       21120 B |

## File System Benchmarks

### FileCacheLayerBase (Overhead)

|                  Method |       Mean [ns] |   Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  2,239,577.2 ns |  67,345.3 ns |   276,948.1 ns |  2,202,500.0 ns |     - |     - |     - |       16408 B |
| SetExistingSimultaneous |  2,332,610.9 ns |  70,150.4 ns |   290,836.7 ns |  2,264,750.0 ns |     - |     - |     - |       12792 B |
|                Overhead |  1,087,240.7 ns |  37,834.9 ns |   157,280.7 ns |  1,063,050.0 ns |     - |     - |     - |        4560 B |
|                 GetMiss |  1,445,024.1 ns |  45,014.7 ns |   184,102.9 ns |  1,423,500.0 ns |     - |     - |     - |        7400 B |
|                  GetHit |  2,091,793.5 ns |  53,136.4 ns |   219,705.9 ns |  2,050,550.0 ns |     - |     - |     - |       13344 B |
|                  SetNew |  1,918,547.1 ns |  53,247.8 ns |   217,774.8 ns |  1,889,100.0 ns |     - |     - |     - |        9840 B |
|             SetExisting |  2,207,402.1 ns |  73,197.0 ns |   303,467.6 ns |  2,181,050.0 ns |     - |     - |     - |       11520 B |
|                 SetMany | 46,902,491.5 ns | 973,060.0 ns | 1,754,627.9 ns | 46,771,950.0 ns |     - |     - |     - |      254400 B |

### JsonFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  4,487,127.1 ns |   125,167.5 ns |   518,932.3 ns |  4,397,400.0 ns |     - |     - |     - |       51928 B |
| SetExistingSimultaneous |  4,983,330.6 ns |   123,043.3 ns |   511,494.1 ns |  4,892,300.0 ns |     - |     - |     - |       51440 B |
|                Overhead |  1,644,103.4 ns |    48,518.2 ns |   200,610.7 ns |  1,613,850.0 ns |     - |     - |     - |       12488 B |
|                 GetMiss |  2,583,441.9 ns |    78,073.1 ns |   322,812.8 ns |  2,505,800.0 ns |     - |     - |     - |       23656 B |
|                  GetHit |  4,067,251.8 ns |   103,713.9 ns |   428,831.3 ns |  4,003,700.0 ns |     - |     - |     - |       42720 B |
|                  SetNew |  3,569,423.2 ns |    96,998.7 ns |   399,981.1 ns |  3,509,900.0 ns |     - |     - |     - |       33480 B |
|             SetExisting |  4,229,421.4 ns |   109,822.2 ns |   455,312.2 ns |  4,148,700.0 ns |     - |     - |     - |       42888 B |
|                 SetMany | 92,741,550.0 ns | 1,837,211.9 ns | 3,217,728.5 ns | 92,088,850.0 ns |     - |     - |     - |     1081200 B |

### ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |  3,116,055.4 ns |    67,536.2 ns |   275,448.7 ns |  3,052,450.0 ns |     - |     - |     - |       21272 B |
| SetExistingSimultaneous |  3,627,764.3 ns |    84,783.8 ns |   348,661.2 ns |  3,544,450.0 ns |     - |     - |     - |       21208 B |
|                Overhead |  1,183,173.3 ns |    35,858.8 ns |   148,267.1 ns |  1,176,100.0 ns |     - |     - |     - |        4608 B |
|                 GetMiss |  1,562,540.4 ns |    48,793.1 ns |   202,834.2 ns |  1,524,000.0 ns |     - |     - |     - |        7440 B |
|                  GetHit |  3,108,708.2 ns |    95,251.7 ns |   398,074.3 ns |  2,997,200.0 ns |     - |     - |     - |       17744 B |
|                  SetNew |  2,895,570.7 ns |    82,929.6 ns |   340,103.9 ns |  2,814,650.0 ns |     - |     - |     - |       14184 B |
|             SetExisting |  3,224,126.8 ns |    93,260.0 ns |   388,718.7 ns |  3,126,650.0 ns |     - |     - |     - |       17976 B |
|                 SetMany | 62,606,770.0 ns | 1,249,913.1 ns | 2,795,609.8 ns | 62,614,000.0 ns |     - |     - |     - |      477120 B |

## Database Benchmarks

### MongoDbCacheLayer

|                  Method |        Mean [ns] |     Error [ns] |    StdDev [ns] |      Median [ns] |     Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |-----------------:|---------------:|---------------:|-----------------:|----------:|------:|------:|--------------:|
|      GetHitSimultaneous |  72,721,353.5 ns | 1,949,497.6 ns | 8,211,665.2 ns |  71,905,100.0 ns |         - |     - |     - |      261544 B |
| SetExistingSimultaneous |  70,983,916.3 ns | 2,088,962.4 ns | 8,753,201.0 ns |  69,894,400.0 ns |         - |     - |     - |      210224 B |
|                Overhead |       6,833.7 ns |       794.4 ns |     3,026.2 ns |       5,900.0 ns |         - |     - |     - |         424 B |
|                 GetMiss |  67,859,459.3 ns | 1,720,821.5 ns | 7,172,587.1 ns |  66,883,550.0 ns |         - |     - |     - |      121664 B |
|                  GetHit |  69,726,953.5 ns | 1,907,277.3 ns | 8,033,824.9 ns |  68,625,600.0 ns |         - |     - |     - |      182888 B |
|                  SetNew |  69,912,851.0 ns | 1,938,316.5 ns | 8,079,132.0 ns |  68,548,250.0 ns |         - |     - |     - |       95720 B |
|             SetExisting |  68,935,014.2 ns | 1,906,826.9 ns | 8,010,998.5 ns |  67,035,400.0 ns |         - |     - |     - |      152976 B |
|                 SetMany | 125,761,659.1 ns | 2,502,151.4 ns | 7,759,473.3 ns | 124,211,200.0 ns | 1000.0000 |     - |     - |     5867032 B |

## Other

### Redis

|                  Method |       Mean [ns] |   Error [ns] |  StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|-------------:|-------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |    543,613.0 ns |  32,141.7 ns | 133,256.3 ns |    519,700.0 ns |     - |     - |     - |        3168 B |
| SetExistingSimultaneous |    655,500.0 ns |  40,606.6 ns | 169,253.1 ns |    642,500.0 ns |     - |     - |     - |        3184 B |
|                Overhead |      7,708.5 ns |   1,506.6 ns |   5,989.1 ns |      5,300.0 ns |     - |     - |     - |         136 B |
|                 GetMiss |    343,654.7 ns |  18,416.4 ns |  76,352.8 ns |    329,850.0 ns |     - |     - |     - |         840 B |
|                  GetHit |    582,914.5 ns |  23,003.7 ns |  93,821.2 ns |    574,150.0 ns |     - |     - |     - |        2256 B |
|                  SetNew |    387,381.4 ns |  19,223.8 ns |  78,186.9 ns |    382,050.0 ns |     - |     - |     - |        1288 B |
|             SetExisting |    595,099.5 ns |  22,497.1 ns |  90,987.9 ns |    591,600.0 ns |     - |     - |     - |        2256 B |
|                 SetMany | 12,999,862.4 ns | 267,127.9 ns | 886,036.4 ns | 12,924,600.0 ns |     - |     - |     - |       95832 B |

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