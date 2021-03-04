# Performance

The performance figures below are as a guide only. Different systems and configurations can drastically change performance results.
Run the benchmark suite on your target machine to best gauge how the performance is for your use case.

Regarding specific tests, it is best to look at the implementations themselves to understand what the test is doing in that time/allocation allotment.

**Test Machine**

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.804 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.200
  [Host]     : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT
  Job-HZFSHP : .NET Core 5.0.3 (CoreCLR 5.0.321.7212, CoreFX 5.0.321.7212), X64 RyuJIT

Runtime=.NET Core 5.0  MaxIterationCount=50
```

## Cache Stack Benchmark

|               Method | WorkIterations |   Mean [ns] | Error [ns] | StdDev [ns] |   Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|--------------------- |--------------- |------------:|-----------:|------------:|--------:|------:|------:|--------------:|
|                  Set |            100 | 24,129.0 ns |   290.8 ns |    257.8 ns |  1.3123 |     - |     - |        4160 B |
|        Set_TwoLayers |            100 | 30,731.2 ns |   388.0 ns |    363.0 ns |  1.5259 |     - |     - |        4920 B |
|                Evict |            100 | 36,629.8 ns |   588.4 ns |    550.4 ns |  2.8076 |     - |     - |        8912 B |
|      Evict_TwoLayers |            100 | 49,364.9 ns |   973.3 ns |    910.4 ns |  4.5776 |     - |     - |       14424 B |
|              Cleanup |            100 | 55,434.8 ns |   822.3 ns |    728.9 ns |  3.3569 |     - |     - |       10560 B |
|    Cleanup_TwoLayers |            100 | 87,934.2 ns | 1,068.4 ns |    999.4 ns |  5.6152 |     - |     - |       17720 B |
|              GetMiss |            100 |  8,930.0 ns |   152.2 ns |    142.3 ns |  0.2899 |     - |     - |         912 B |
|               GetHit |            100 |  9,411.4 ns |   121.6 ns |    113.7 ns |  0.3052 |     - |     - |         992 B |
|  GetOrSet_NeverStale |            100 | 17,346.2 ns |   294.4 ns |    275.4 ns |  0.3967 |     - |     - |        1328 B |
| GetOrSet_AlwaysStale |            100 | 71,033.2 ns |   976.5 ns |    815.4 ns |  7.6904 |     - |     - |       24296 B |
|   GetOrSet_UnderLoad |            100 | 80,199.5 ns | 1,564.1 ns |  2,388.5 ns | 13.6719 |     - |     - |       42506 B |

## Cache Layer Comparison Benchmark

|                 Method | WorkIterations |           Mean |        Error |       StdDev |         Median |    Ratio | RatioSD |      Gen 0 |  Gen 1 | Gen 2 |   Allocated |
|----------------------- |--------------- |---------------:|-------------:|-------------:|---------------:|---------:|--------:|-----------:|-------:|------:|------------:|
|       MemoryCacheLayer |              1 |       119.7 us |      0.88 us |      0.78 us |       119.5 us |     1.00 |    0.00 |    43.7012 | 0.2441 |     - |   134.34 KB |
|        RedisCacheLayer |              1 |    53,373.7 us |    824.23 us |  1,465.07 us |    53,399.2 us |   450.35 |   16.42 |          - |      - |     - |   266.34 KB |
| ProtobufFileCacheLayer |              1 |   187,721.3 us |  3,373.72 us |  2,817.21 us |   188,113.5 us | 1,568.86 |   23.26 |          - |      - |     - |  1474.11 KB |
|     JsonFileCacheLayer |              1 |   201,487.0 us |  2,606.99 us |  2,438.58 us |   201,416.3 us | 1,683.92 |   26.89 |  1000.0000 |      - |     - |  3250.96 KB |
|      MongoDbCacheLayer |              1 |   355,290.8 us |  6,233.94 us |  5,831.23 us |   354,024.5 us | 2,965.19 |   42.63 |  8000.0000 |      - |     - | 27257.54 KB |
|                        |                |                |              |              |                |          |         |            |        |       |             |
|       MemoryCacheLayer |             10 |       912.4 us |     17.27 us |     43.34 us |       891.9 us |     1.00 |    0.00 |   261.7188 | 0.9766 |     - |   801.94 KB |
|        RedisCacheLayer |             10 |   495,324.8 us |  2,694.06 us |  2,520.03 us |   495,735.3 us |   504.72 |   11.26 |          - |      - |     - |  2643.95 KB |
| ProtobufFileCacheLayer |             10 | 1,630,101.8 us | 14,011.98 us | 13,106.81 us | 1,633,028.9 us | 1,661.13 |   42.41 |  4000.0000 |      - |     - | 13651.83 KB |
|     JsonFileCacheLayer |             10 | 1,758,585.6 us |  9,551.04 us |  8,934.05 us | 1,757,511.6 us | 1,792.05 |   44.36 | 10000.0000 |      - |     - | 30750.66 KB |
|      MongoDbCacheLayer |             10 | 3,315,541.0 us | 66,175.49 us | 70,807.03 us | 3,297,892.4 us | 3,430.08 |  167.72 | 89000.0000 |      - |     - | 272440.1 KB |

## In-Memory Benchmarks

### MemoryCacheLayer

|      Method | WorkIterations |   Mean [ns] | Error [ns] | StdDev [ns] |   Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------ |--------------- |------------:|-----------:|------------:|--------:|------:|------:|--------------:|
|    Overhead |              1 |    214.7 ns |    4.28 ns |     4.20 ns |  0.2270 |     - |     - |         712 B |
|     GetMiss |              1 |    262.8 ns |    2.51 ns |     2.22 ns |  0.2270 |     - |     - |         712 B |
|      GetHit |              1 |    522.9 ns |   10.25 ns |     9.08 ns |  0.2518 |     - |     - |         792 B |
| SetExisting |              1 |    733.3 ns |   12.48 ns |    11.06 ns |  0.2623 |     - |     - |         824 B |
|   EvictMiss |              1 |    285.4 ns |    3.38 ns |     3.16 ns |  0.2270 |     - |     - |         712 B |
|    EvictHit |              1 |    550.8 ns |    8.69 ns |     8.13 ns |  0.2518 |     - |     - |         792 B |
|     Cleanup |              1 |    987.2 ns |   11.81 ns |    11.05 ns |  0.2918 |     - |     - |         920 B |
|    Overhead |            100 |    214.5 ns |    3.19 ns |     2.98 ns |  0.2270 |     - |     - |         712 B |
|     GetMiss |            100 |  2,880.7 ns |   35.59 ns |    33.29 ns |  0.2251 |     - |     - |         712 B |
|      GetHit |            100 |  3,582.9 ns |   43.98 ns |    41.14 ns |  0.2518 |     - |     - |         792 B |
| SetExisting |            100 | 24,386.8 ns |  283.87 ns |   265.53 ns |  1.2512 |     - |     - |        3992 B |
|   EvictMiss |            100 |  5,062.7 ns |   57.23 ns |    50.73 ns |  0.2213 |     - |     - |         712 B |
|    EvictHit |            100 | 32,059.8 ns |  492.67 ns |   460.85 ns |  2.7466 |     - |     - |        8712 B |
|     Cleanup |            100 | 53,421.4 ns |  437.98 ns |   409.69 ns | 12.4512 |     - |     - |       39192 B |

## File System Benchmarks

### FileCacheLayerBase (Overhead)

|                  Method | WorkIterations |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |--------------- |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |              1 |  2,411,510.8 ns |    79,179.8 ns |   330,906.8 ns |  2,364,300.0 ns |     - |     - |     - |       12584 B |
| SetExistingSimultaneous |              1 |  2,361,415.3 ns |    77,555.2 ns |   324,972.8 ns |  2,309,950.0 ns |     - |     - |     - |       10768 B |
|                Overhead |              1 |  1,171,119.8 ns |    33,740.9 ns |   139,886.4 ns |  1,148,300.0 ns |     - |     - |     - |        4160 B |
|                 GetMiss |              1 |  1,595,598.5 ns |    48,689.5 ns |   202,943.6 ns |  1,543,150.0 ns |     - |     - |     - |        6568 B |
|                  GetHit |              1 |  2,356,426.7 ns |    76,070.8 ns |   317,913.7 ns |  2,306,400.0 ns |     - |     - |     - |       12488 B |
|             SetExisting |              1 |  2,374,889.3 ns |    77,497.6 ns |   320,433.3 ns |  2,307,050.0 ns |     - |     - |     - |       10376 B |
|               EvictMiss |              1 |  1,558,995.4 ns |    47,816.5 ns |   199,833.7 ns |  1,538,200.0 ns |     - |     - |     - |        6704 B |
|                EvictHit |              1 |  2,422,586.0 ns |    81,473.9 ns |   338,689.3 ns |  2,405,700.0 ns |     - |     - |     - |       10920 B |
|                 Cleanup |              1 |  2,465,906.7 ns |    78,538.2 ns |   326,485.4 ns |  2,463,800.0 ns |     - |     - |     - |       11064 B |
|      GetHitSimultaneous |            100 |  7,711,601.0 ns |   170,516.3 ns |   716,376.5 ns |  7,666,600.0 ns |     - |     - |     - |      317232 B |
| SetExistingSimultaneous |            100 | 15,263,568.3 ns |   303,709.2 ns | 1,011,591.0 ns | 15,004,150.0 ns |     - |     - |     - |      170896 B |
|                Overhead |            100 |  1,100,191.9 ns |    24,063.5 ns |    97,870.8 ns |  1,090,800.0 ns |     - |     - |     - |        4160 B |
|                 GetMiss |            100 |  1,607,973.4 ns |    45,548.1 ns |   188,837.8 ns |  1,586,600.0 ns |     - |     - |     - |        6584 B |
|                  GetHit |            100 | 12,243,533.8 ns |   243,956.4 ns |   598,429.2 ns | 12,263,300.0 ns |     - |     - |     - |      335784 B |
|             SetExisting |            100 | 14,718,018.3 ns |   293,512.7 ns |   778,354.0 ns | 14,651,750.0 ns |     - |     - |     - |      167760 B |
|               EvictMiss |            100 |  1,595,259.2 ns |    49,165.0 ns |   203,285.2 ns |  1,549,500.0 ns |     - |     - |     - |        6416 B |
|                EvictHit |            100 | 69,754,520.0 ns | 1,506,628.4 ns | 2,678,030.5 ns | 69,655,650.0 ns |     - |     - |     - |      397752 B |
|                 Cleanup |            100 | 80,794,718.3 ns | 1,613,254.1 ns | 3,957,339.7 ns | 80,012,500.0 ns |     - |     - |     - |      439808 B |

### JsonFileCacheLayer

|                  Method | WorkIterations |        Mean [ns] |     Error [ns] |    StdDev [ns] |      Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |--------------- |-----------------:|---------------:|---------------:|-----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |              1 |   4,487,601.0 ns |   119,467.8 ns |   495,302.1 ns |   4,452,300.0 ns |     - |     - |     - |       42088 B |
| SetExistingSimultaneous |              1 |   4,710,311.9 ns |   117,480.6 ns |   489,672.9 ns |   4,678,900.0 ns |     - |     - |     - |       42384 B |
|                Overhead |              1 |   1,822,029.7 ns |    58,850.3 ns |   245,945.9 ns |   1,792,600.0 ns |     - |     - |     - |       11992 B |
|                 GetMiss |              1 |   2,847,625.4 ns |    90,456.9 ns |   376,031.6 ns |   2,814,400.0 ns |     - |     - |     - |       22960 B |
|                  GetHit |              1 |   4,460,718.4 ns |   113,935.4 ns |   469,820.6 ns |   4,417,350.0 ns |     - |     - |     - |       41880 B |
|             SetExisting |              1 |   4,708,434.5 ns |   111,166.4 ns |   463,354.8 ns |   4,665,050.0 ns |     - |     - |     - |       42160 B |
|               EvictMiss |              1 |   2,873,014.1 ns |    90,066.0 ns |   379,375.8 ns |   2,858,450.0 ns |     - |     - |     - |       22912 B |
|                EvictHit |              1 |   4,198,945.4 ns |   102,416.5 ns |   426,884.2 ns |   4,137,800.0 ns |     - |     - |     - |       34816 B |
|                 Cleanup |              1 |   4,237,236.9 ns |   122,814.5 ns |   517,318.7 ns |   4,163,500.0 ns |     - |     - |     - |       34856 B |
|      GetHitSimultaneous |            100 |  31,432,010.3 ns |   625,726.0 ns | 2,084,160.9 ns |  30,857,550.0 ns |     - |     - |     - |      932880 B |
| SetExistingSimultaneous |            100 |  61,297,374.6 ns | 1,220,897.1 ns | 2,901,591.1 ns |  60,510,600.0 ns |     - |     - |     - |      967520 B |
|                Overhead |            100 |   1,764,175.5 ns |    52,430.3 ns |   215,022.5 ns |   1,723,200.0 ns |     - |     - |     - |       12072 B |
|                 GetMiss |            100 |   2,900,579.2 ns |    90,309.9 ns |   377,421.6 ns |   2,862,850.0 ns |     - |     - |     - |       22816 B |
|                  GetHit |            100 |  37,378,221.3 ns |   938,767.7 ns | 3,943,969.4 ns |  37,302,800.0 ns |     - |     - |     - |      929760 B |
|             SetExisting |            100 |  62,990,165.2 ns | 1,245,125.3 ns | 1,574,684.9 ns |  63,165,100.0 ns |     - |     - |     - |      945336 B |
|               EvictMiss |            100 |   2,847,230.7 ns |    86,519.7 ns |   358,702.4 ns |   2,831,500.0 ns |     - |     - |     - |       22784 B |
|                EvictHit |            100 | 121,607,519.2 ns | 1,467,431.7 ns | 1,225,371.9 ns | 121,596,550.0 ns |     - |     - |     - |     1167960 B |
|                 Cleanup |            100 | 131,077,141.5 ns | 2,570,760.7 ns | 4,635,611.8 ns | 130,925,300.0 ns |     - |     - |     - |     1220352 B |

### ProtobufFileCacheLayer

|                  Method | WorkIterations |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |--------------- |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|      GetHitSimultaneous |              1 |  3,414,136.0 ns |    96,279.3 ns |   395,934.9 ns |  3,359,100.0 ns |     - |     - |     - |       17240 B |
| SetExistingSimultaneous |              1 |  3,618,728.3 ns |    97,086.7 ns |   401,429.4 ns |  3,587,900.0 ns |     - |     - |     - |       17488 B |
|                Overhead |              1 |  1,242,013.5 ns |    35,113.1 ns |   145,575.5 ns |  1,204,350.0 ns |     - |     - |     - |        4208 B |
|                 GetMiss |              1 |  1,672,533.7 ns |    67,485.1 ns |   280,537.4 ns |  1,579,200.0 ns |     - |     - |     - |        6768 B |
|                  GetHit |              1 |  3,268,305.6 ns |   131,256.3 ns |   548,543.9 ns |  3,066,700.0 ns |     - |     - |     - |       16760 B |
|             SetExisting |              1 |  3,486,736.3 ns |   131,631.5 ns |   553,012.7 ns |  3,319,550.0 ns |     - |     - |     - |       17264 B |
|               EvictMiss |              1 |  1,588,584.7 ns |    56,912.9 ns |   234,684.4 ns |  1,504,700.0 ns |     - |     - |     - |        6744 B |
|                EvictHit |              1 |  2,776,886.6 ns |   100,255.3 ns |   417,876.1 ns |  2,615,750.0 ns |     - |     - |     - |       14368 B |
|                 Cleanup |              1 |  2,815,723.6 ns |   118,242.1 ns |   494,155.3 ns |  2,624,200.0 ns |     - |     - |     - |       13088 B |
|      GetHitSimultaneous |            100 |  8,952,869.5 ns |   178,883.1 ns |   608,092.0 ns |  8,841,500.0 ns |     - |     - |     - |      346168 B |
| SetExistingSimultaneous |            100 | 37,543,790.0 ns |   739,696.9 ns |   851,836.0 ns | 37,450,700.0 ns |     - |     - |     - |      404792 B |
|                Overhead |            100 |  1,170,440.2 ns |    41,094.4 ns |   168,995.0 ns |  1,117,700.0 ns |     - |     - |     - |        4208 B |
|                 GetMiss |            100 |  1,643,025.0 ns |    61,365.5 ns |   251,666.8 ns |  1,562,250.0 ns |     - |     - |     - |        6784 B |
|                  GetHit |            100 | 14,435,842.5 ns |   288,577.2 ns |   936,884.4 ns | 14,265,050.0 ns |     - |     - |     - |      342504 B |
|             SetExisting |            100 | 36,443,065.7 ns |   725,544.4 ns | 1,766,075.4 ns | 36,131,650.0 ns |     - |     - |     - |      366496 B |
|               EvictMiss |            100 |  1,630,512.2 ns |    58,443.3 ns |   237,700.3 ns |  1,557,950.0 ns |     - |     - |     - |        6616 B |
|                EvictHit |            100 | 86,455,742.9 ns |   962,666.6 ns |   853,378.8 ns | 86,765,300.0 ns |     - |     - |     - |      594976 B |
|                 Cleanup |            100 | 95,314,051.3 ns | 1,998,563.2 ns | 3,500,322.2 ns | 94,880,400.0 ns |     - |     - |     - |      636776 B |

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

|          Method |  Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 |  Gen 1 | Gen 2 | Allocated [B] |
|---------------- |-----------:|-----------:|------------:|-------:|-------:|------:|--------------:|
|   OnCacheUpdate | 2,503.0 ns |   50.01 ns |    46.78 ns | 0.1068 | 0.0153 |     - |         423 B |
| OnCacheEviction | 1,459.5 ns |   29.17 ns |    74.78 ns | 0.0572 | 0.0191 |     - |         299 B |
|    OnCacheFlush | 2,176.6 ns |   43.33 ns |    79.23 ns | 0.0801 | 0.0153 |     - |         296 B |

### RedisLockExtension

|      Method |    Mean [ns] | Error [ns] | StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------ |-------------:|-----------:|------------:|-------:|------:|------:|--------------:|
| WithRefresh | 162,511.5 ns | 1,243.2 ns |  1,102.0 ns | 0.2441 |     - |     - |        1208 B |