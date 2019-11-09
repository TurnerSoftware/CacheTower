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

|                 Method | WorkIterations |           Mean |        Error |        StdDev |    Ratio | RatioSD |       Gen 0 | Gen 1 | Gen 2 |    Allocated |
|----------------------- |--------------- |---------------:|-------------:|--------------:|---------:|--------:|------------:|------:|------:|-------------:|
|       MemoryCacheLayer |              1 |       114.3 us |      0.75 us |       0.70 us |     1.00 |    0.00 |     30.2734 |     - |     - |     92.77 KB |
|        RedisCacheLayer |              1 |    51,644.1 us |  1,054.98 us |   2,547.89 us |   478.61 |   25.23 |           - |     - |     - |    316.73 KB |
| ProtobufFileCacheLayer |              1 |   231,220.3 us |  4,500.69 us |   7,267.77 us | 2,029.55 |   73.36 |           - |     - |     - |   1579.58 KB |
|     JsonFileCacheLayer |              1 |   277,886.8 us |  5,338.60 us |   7,126.87 us | 2,439.41 |   62.02 |   1000.0000 |     - |     - |   3338.56 KB |
|      MongoDbCacheLayer |              1 |   462,855.6 us |  9,240.89 us |  18,455.06 us | 4,037.88 |  203.11 |  10000.0000 |     - |     - |  30961.79 KB |
|                        |                |                |              |               |          |         |             |       |       |              |
|       MemoryCacheLayer |             10 |     1,126.8 us |      8.65 us |       7.67 us |     1.00 |    0.00 |    238.2813 |     - |     - |    730.51 KB |
|        RedisCacheLayer |             10 |   466,286.4 us |  2,793.72 us |   2,613.25 us |   413.56 |    4.11 |   1000.0000 |     - |     - |      3142 KB |
| ProtobufFileCacheLayer |             10 | 1,970,911.9 us | 27,300.10 us |  25,536.53 us | 1,747.54 |   23.08 |   4000.0000 |     - |     - |   14632.1 KB |
|     JsonFileCacheLayer |             10 | 2,414,588.6 us | 10,208.55 us |   8,524.61 us | 2,141.78 |   16.28 |  10000.0000 |     - |     - |  31644.45 KB |
|      MongoDbCacheLayer |             10 | 3,635,721.9 us | 71,347.02 us | 100,018.56 us | 3,249.76 |  109.10 | 101000.0000 |     - |     - | 309633.16 KB |

## In-Memory Benchmarks

### MemoryCacheLayer

|      Method | WorkIterations |   Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------ |--------------- |------------:|-----------:|------------:|-------:|------:|------:|--------------:|
|    Overhead |              1 |    134.0 ns |    1.91 ns |     1.78 ns | 0.0663 |     - |     - |         208 B |
|     GetMiss |              1 |    215.7 ns |    1.77 ns |     1.66 ns | 0.0663 |     - |     - |         208 B |
|      GetHit |              1 |    528.3 ns |    5.43 ns |     5.08 ns | 0.1192 |     - |     - |         376 B |
| SetExisting |              1 |    686.8 ns |    6.79 ns |     6.02 ns | 0.1297 |     - |     - |         408 B |
|   EvictMiss |              1 |    210.4 ns |    3.09 ns |     2.89 ns | 0.0663 |     - |     - |         208 B |
|    EvictHit |              1 |    515.4 ns |   10.57 ns |    11.31 ns | 0.1192 |     - |     - |         376 B |
|     Cleanup |              1 |    902.2 ns |   12.96 ns |    12.13 ns | 0.1392 |     - |     - |         440 B |
|    Overhead |            100 |    136.4 ns |    1.71 ns |     1.60 ns | 0.0663 |     - |     - |         208 B |
|     GetMiss |            100 |  5,712.4 ns |  112.45 ns |   105.18 ns | 0.0610 |     - |     - |         208 B |
|      GetHit |            100 |  8,007.4 ns |   98.16 ns |    91.82 ns | 0.1068 |     - |     - |         376 B |
| SetExisting |            100 | 26,469.3 ns |  357.36 ns |   316.79 ns | 1.1292 |     - |     - |        3576 B |
|   EvictMiss |            100 |  4,969.0 ns |   57.91 ns |    54.17 ns | 0.0610 |     - |     - |         208 B |
|    EvictHit |            100 | 31,773.8 ns |  590.73 ns |   552.57 ns | 1.0986 |     - |     - |        3544 B |
|     Cleanup |            100 | 36,293.3 ns |  502.66 ns |   470.19 ns | 6.5308 |     - |     - |       20640 B |

## File System Benchmarks

### FileCacheLayerBase (Overhead)

|                  Method | WorkIterations |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |--------------- |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |              1 |  2,386,350.0 ns |    57,245.1 ns |   234,768.8 ns |  2,343,400.0 ns |     - |     - |     - |       13408 B |
| SetExistingSimultaneous |              1 |  2,376,932.1 ns |    60,584.4 ns |   249,824.2 ns |  2,334,250.0 ns |     - |     - |     - |       11272 B |
|                Overhead |              1 |  1,151,229.3 ns |    31,898.6 ns |   130,819.7 ns |  1,138,550.0 ns |     - |     - |     - |        4560 B |
|                 GetMiss |              1 |  1,655,499.0 ns |    46,796.1 ns |   196,085.7 ns |  1,620,800.0 ns |     - |     - |     - |        6840 B |
|                  GetHit |              1 |  2,365,561.6 ns |    53,496.4 ns |   217,580.4 ns |  2,298,400.0 ns |     - |     - |     - |       13344 B |
|             SetExisting |              1 |  2,363,501.0 ns |    59,796.9 ns |   249,240.7 ns |  2,320,200.0 ns |     - |     - |     - |       11040 B |
|               EvictMiss |              1 |  1,601,975.9 ns |    43,848.7 ns |   183,251.5 ns |  1,572,800.0 ns |     - |     - |     - |        7224 B |
|                EvictHit |              1 |  2,467,259.8 ns |    56,406.5 ns |   231,963.8 ns |  2,418,000.0 ns |     - |     - |     - |       11424 B |
|                 Cleanup |              1 |  2,430,859.8 ns |    55,151.7 ns |   223,685.8 ns |  2,413,150.0 ns |     - |     - |     - |       11568 B |
|      GetHitSimultaneous |            100 |  7,503,542.9 ns |   149,365.9 ns |   581,510.2 ns |  7,436,800.0 ns |     - |     - |     - |      317544 B |
| SetExistingSimultaneous |            100 | 15,238,141.7 ns |   303,863.3 ns | 1,200,881.7 ns | 14,910,600.0 ns |     - |     - |     - |      171544 B |
|                Overhead |            100 |  1,167,856.5 ns |    29,900.8 ns |   123,632.3 ns |  1,156,700.0 ns |     - |     - |     - |        4560 B |
|                 GetMiss |            100 |  1,615,889.1 ns |    40,538.1 ns |   168,067.2 ns |  1,587,000.0 ns |     - |     - |     - |        7088 B |
|                  GetHit |            100 | 11,978,487.9 ns |   237,938.5 ns |   837,421.7 ns | 11,902,750.0 ns |     - |     - |     - |      314216 B |
|             SetExisting |            100 | 15,196,073.1 ns |   303,766.9 ns |   784,119.3 ns | 15,091,900.0 ns |     - |     - |     - |      168408 B |
|               EvictMiss |            100 |  1,638,848.6 ns |    33,295.8 ns |   134,662.7 ns |  1,621,000.0 ns |     - |     - |     - |        7080 B |
|                EvictHit |            100 | 70,450,330.0 ns | 1,338,300.3 ns | 1,541,188.7 ns | 70,353,650.0 ns |     - |     - |     - |      398800 B |
|                 Cleanup |            100 | 82,091,173.5 ns | 1,625,360.2 ns | 3,894,259.3 ns | 81,863,850.0 ns |     - |     - |     - |      440312 B |

### JsonFileCacheLayer

|                  Method | WorkIterations |        Mean [ns] |     Error [ns] |    StdDev [ns] |      Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |--------------- |-----------------:|---------------:|---------------:|-----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |              1 |   4,575,119.3 ns |    98,421.8 ns |   408,047.3 ns |   4,516,900.0 ns |     - |     - |     - |       42784 B |
| SetExistingSimultaneous |              1 |   4,697,320.6 ns |    97,572.9 ns |   406,695.3 ns |   4,664,900.0 ns |     - |     - |     - |       42776 B |
|                Overhead |              1 |   1,796,057.4 ns |    49,549.1 ns |   203,206.7 ns |   1,743,800.0 ns |     - |     - |     - |       12184 B |
|                 GetMiss |              1 |   2,740,641.9 ns |    59,781.5 ns |   243,820.9 ns |   2,674,550.0 ns |     - |     - |     - |       23672 B |
|                  GetHit |              1 |   4,448,928.3 ns |    91,885.2 ns |   379,922.6 ns |   4,385,400.0 ns |     - |     - |     - |       42416 B |
|             SetExisting |              1 |   4,616,570.5 ns |    92,121.1 ns |   372,577.3 ns |   4,615,500.0 ns |     - |     - |     - |       42888 B |
|               EvictMiss |              1 |   2,738,647.1 ns |    57,803.4 ns |   236,406.5 ns |   2,692,900.0 ns |     - |     - |     - |       23624 B |
|                EvictHit |              1 |   4,254,638.7 ns |    99,689.3 ns |   415,516.6 ns |   4,171,400.0 ns |     - |     - |     - |       35544 B |
|                 Cleanup |              1 |   4,279,397.9 ns |   100,670.7 ns |   416,248.6 ns |   4,211,100.0 ns |     - |     - |     - |       35696 B |
|      GetHitSimultaneous |            100 |  29,967,106.6 ns |   597,300.7 ns | 2,186,683.5 ns |  29,419,400.0 ns |     - |     - |     - |      933560 B |
| SetExistingSimultaneous |            100 |  59,536,437.2 ns | 1,186,756.3 ns | 2,199,728.9 ns |  58,986,300.0 ns |     - |     - |     - |      968432 B |
|                Overhead |            100 |   1,769,533.2 ns |    52,125.2 ns |   218,415.6 ns |   1,694,500.0 ns |     - |     - |     - |       12184 B |
|                 GetMiss |            100 |   2,793,093.2 ns |    58,236.5 ns |   240,142.5 ns |   2,747,550.0 ns |     - |     - |     - |       23512 B |
|                  GetHit |            100 |  34,255,075.0 ns |   904,309.0 ns | 3,789,249.0 ns |  33,642,200.0 ns |     - |     - |     - |      930440 B |
|             SetExisting |            100 |  59,826,470.0 ns | 1,161,280.4 ns | 1,738,149.8 ns |  59,316,300.0 ns |     - |     - |     - |      946032 B |
|               EvictMiss |            100 |   2,794,383.9 ns |    68,044.5 ns |   282,862.7 ns |   2,700,600.0 ns |     - |     - |     - |       23480 B |
|                EvictHit |            100 | 122,832,112.5 ns | 2,411,872.1 ns | 4,287,100.3 ns | 122,246,350.0 ns |     - |     - |     - |     1168256 B |
|                 Cleanup |            100 | 127,748,566.7 ns | 2,503,315.8 ns | 3,509,299.2 ns | 127,791,200.0 ns |     - |     - |     - |     1200032 B |

### ProtobufFileCacheLayer

|                  Method | WorkIterations |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |--------------- |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |              1 |  3,441,835.1 ns |    90,072.0 ns |   372,425.3 ns |  3,345,800.0 ns |     - |     - |     - |       17952 B |
| SetExistingSimultaneous |              1 |  3,592,881.6 ns |    78,639.8 ns |   324,276.8 ns |  3,514,950.0 ns |     - |     - |     - |       17704 B |
|                Overhead |              1 |  1,291,644.4 ns |    41,138.0 ns |   172,377.0 ns |  1,233,900.0 ns |     - |     - |     - |        4608 B |
|                 GetMiss |              1 |  1,732,844.7 ns |    46,706.6 ns |   192,598.0 ns |  1,684,700.0 ns |     - |     - |     - |        7448 B |
|                  GetHit |              1 |  3,289,738.8 ns |    83,234.7 ns |   341,355.0 ns |  3,221,050.0 ns |     - |     - |     - |       17744 B |
|             SetExisting |              1 |  3,563,944.8 ns |    92,861.1 ns |   384,993.3 ns |  3,467,900.0 ns |     - |     - |     - |       17976 B |
|               EvictMiss |              1 |  1,740,280.7 ns |    48,974.3 ns |   203,042.8 ns |  1,698,150.0 ns |     - |     - |     - |        7424 B |
|                EvictHit |              1 |  2,902,818.1 ns |    87,463.0 ns |   363,586.0 ns |  2,836,300.0 ns |     - |     - |     - |       15008 B |
|                 Cleanup |              1 |  2,911,368.1 ns |    74,323.4 ns |   304,808.8 ns |  2,866,150.0 ns |     - |     - |     - |       13512 B |
|      GetHitSimultaneous |            100 |  9,078,847.9 ns |   181,402.8 ns |   444,984.2 ns |  9,065,900.0 ns |     - |     - |     - |      370120 B |
| SetExistingSimultaneous |            100 | 36,237,618.6 ns |   719,157.5 ns | 1,333,004.6 ns | 36,471,900.0 ns |     - |     - |     - |      389960 B |
|                Overhead |            100 |  1,278,807.7 ns |    40,118.9 ns |   167,220.5 ns |  1,233,200.0 ns |     - |     - |     - |        4608 B |
|                 GetMiss |            100 |  1,785,517.2 ns |    48,128.6 ns |   199,536.4 ns |  1,755,350.0 ns |     - |     - |     - |        7288 B |
|                  GetHit |            100 | 15,163,948.0 ns |   302,418.2 ns |   891,687.0 ns | 15,032,250.0 ns |     - |     - |     - |      343584 B |
|             SetExisting |            100 | 36,935,015.6 ns |   737,911.4 ns | 1,710,222.0 ns | 36,982,200.0 ns |     - |     - |     - |      367160 B |
|               EvictMiss |            100 |  1,808,394.4 ns |    51,824.2 ns |   216,582.6 ns |  1,765,600.0 ns |     - |     - |     - |        7280 B |
|                EvictHit |            100 | 89,935,131.2 ns | 1,679,694.3 ns | 1,649,684.4 ns | 90,064,550.0 ns |     - |     - |     - |      595640 B |
|                 Cleanup |            100 | 96,510,047.8 ns | 1,909,998.8 ns | 3,679,919.3 ns | 95,782,250.0 ns |     - |     - |     - |      625152 B |

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

|                  Method | WorkIterations |       Mean [ns] |   Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |--------------- |----------------:|-------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |              1 |    776,106.7 ns |  47,894.0 ns |   199,628.0 ns |    731,550.0 ns |     - |     - |     - |        2472 B |
| SetExistingSimultaneous |              1 |    761,793.5 ns |  43,919.9 ns |   182,576.2 ns |    734,550.0 ns |     - |     - |     - |        2576 B |
|                Overhead |              1 |      6,839.1 ns |     783.0 ns |     2,963.5 ns |      5,900.0 ns |     - |     - |     - |         144 B |
|                 GetMiss |              1 |    369,215.4 ns |  19,502.8 ns |    79,983.4 ns |    361,050.0 ns |     - |     - |     - |         864 B |
|                  GetHit |              1 |    659,281.4 ns |  28,088.7 ns |   114,242.4 ns |    653,950.0 ns |     - |     - |     - |        2200 B |
|             SetExisting |              1 |    608,181.6 ns |  31,405.1 ns |   127,730.9 ns |    607,200.0 ns |     - |     - |     - |        2264 B |
|               EvictMiss |              1 |    367,308.0 ns |  20,143.2 ns |    82,382.4 ns |    351,200.0 ns |     - |     - |     - |         616 B |
|                EvictHit |              1 |    596,309.2 ns |  28,546.7 ns |   115,780.6 ns |    590,850.0 ns |     - |     - |     - |        1568 B |
|                 Cleanup |              1 |     22,283.4 ns |   3,690.8 ns |    15,094.8 ns |     14,800.0 ns |     - |     - |     - |         336 B |
|      GetHitSimultaneous |            100 |  1,095,998.2 ns |  36,059.5 ns |   151,493.8 ns |  1,086,850.0 ns |     - |     - |     - |       89304 B |
| SetExistingSimultaneous |            100 |  2,013,256.8 ns |  68,469.9 ns |   289,158.2 ns |  2,015,400.0 ns |     - |     - |     - |       97568 B |
|                Overhead |            100 |      6,132.3 ns |     695.3 ns |     2,606.2 ns |      5,300.0 ns |     - |     - |     - |         144 B |
|                 GetMiss |            100 | 13,433,628.8 ns | 319,422.0 ns | 1,313,577.5 ns | 13,134,250.0 ns |     - |     - |     - |       43448 B |
|                  GetHit |            100 | 13,995,872.5 ns | 337,780.3 ns | 1,389,073.2 ns | 13,552,600.0 ns |     - |     - |     - |       82752 B |
|             SetExisting |            100 | 13,763,840.4 ns | 274,609.1 ns |   987,860.7 ns | 13,635,850.0 ns |     - |     - |     - |       89120 B |
|               EvictMiss |            100 | 13,538,958.1 ns | 300,680.3 ns | 1,266,524.2 ns | 13,434,750.0 ns |     - |     - |     - |       25840 B |
|                EvictHit |            100 | 26,777,372.2 ns | 514,088.0 ns |   550,068.4 ns | 26,812,150.0 ns |     - |     - |     - |      121160 B |
|                 Cleanup |            100 |     58,360.5 ns |   4,923.2 ns |    20,301.1 ns |     52,550.0 ns |     - |     - |     - |       10488 B |

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