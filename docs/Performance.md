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

|                    Method | WorkIterations |       Mean [ns] |    Error [ns] |   StdDev [ns] |  Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|-------------------------- |--------------- |----------------:|--------------:|--------------:|-------:|------:|------:|--------------:|
|          SetupAndTeardown |              1 |        351.6 ns |       2.94 ns |       2.75 ns | 0.2933 |     - |     - |         920 B |
|                       Set |              1 |        726.5 ns |       6.33 ns |       5.61 ns | 0.3185 |     - |     - |        1000 B |
|             Set_TwoLayers |              1 |      1,003.6 ns |       7.58 ns |       7.09 ns | 0.5627 |     - |     - |        1768 B |
|                     Evict |              1 |        846.2 ns |       6.08 ns |       5.69 ns | 0.3185 |     - |     - |        1000 B |
|           Evict_TwoLayers |              1 |      1,185.8 ns |      10.07 ns |       8.93 ns | 0.5627 |     - |     - |        1768 B |
|                   Cleanup |              1 |      1,231.8 ns |      15.87 ns |      14.07 ns | 0.3586 |     - |     - |        1128 B |
|         Cleanup_TwoLayers |              1 |      1,675.8 ns |      16.53 ns |      15.46 ns | 0.6027 |     - |     - |        1896 B |
|                   GetMiss |              1 |        450.3 ns |       3.77 ns |       3.53 ns | 0.2933 |     - |     - |         920 B |
|                    GetHit |              1 |        847.1 ns |       4.87 ns |       4.31 ns | 0.3185 |     - |     - |        1000 B |
|       GetOrSet_NeverStale |              1 |      1,535.0 ns |      13.80 ns |      12.91 ns | 0.4215 |     - |     - |        1328 B |
|      GetOrSet_AlwaysStale |              1 |      1,520.2 ns |      15.95 ns |      14.92 ns | 0.4215 |     - |     - |        1328 B |
|  GetOrSet_TwoSimultaneous |              1 | 31,351,330.8 ns | 355,621.61 ns | 332,648.68 ns |      - |     - |     - |        2456 B |
| GetOrSet_FourSimultaneous |              1 | 31,516,912.2 ns | 623,711.44 ns | 765,973.66 ns |      - |     - |     - |        2600 B |
|          SetupAndTeardown |            100 |        358.5 ns |       1.92 ns |       1.60 ns | 0.2933 |     - |     - |         920 B |
|                       Set |            100 |     34,208.9 ns |     378.14 ns |     353.71 ns | 1.2817 |     - |     - |        4168 B |
|             Set_TwoLayers |            100 |     41,915.1 ns |     369.91 ns |     346.01 ns | 1.5259 |     - |     - |        4936 B |
|                     Evict |            100 |     45,399.9 ns |     415.31 ns |     388.48 ns | 2.8076 |     - |     - |        8920 B |
|           Evict_TwoLayers |            100 |     59,663.5 ns |     560.85 ns |     524.62 ns | 4.5776 |     - |     - |       14440 B |
|                   Cleanup |            100 |     59,699.5 ns |     523.00 ns |     436.73 ns | 8.9111 |     - |     - |       28112 B |
|         Cleanup_TwoLayers |            100 |    102,450.6 ns |   1,011.53 ns |     946.18 ns | 5.6152 |     - |     - |       17736 B |
|                   GetMiss |            100 |      7,621.4 ns |      84.16 ns |      78.72 ns | 0.2899 |     - |     - |         920 B |
|                    GetHit |            100 |      8,347.6 ns |      55.77 ns |      49.44 ns | 0.3052 |     - |     - |        1000 B |
|       GetOrSet_NeverStale |            100 |     28,985.6 ns |     142.29 ns |     133.10 ns | 0.3967 |     - |     - |        1328 B |
|      GetOrSet_AlwaysStale |            100 |    112,913.5 ns |     965.52 ns |     855.91 ns | 7.4463 |     - |     - |       23504 B |
|  GetOrSet_TwoSimultaneous |            100 | 31,343,604.2 ns | 439,636.97 ns | 389,726.73 ns |      - |     - |     - |       17080 B |
| GetOrSet_FourSimultaneous |            100 | 31,423,357.8 ns | 588,223.86 ns | 577,714.48 ns |      - |     - |     - |       28312 B |

## Cache Layer Comparison Benchmark

|                 Method | WorkIterations |           Mean |        Error |       StdDev |    Ratio | RatioSD |       Gen 0 |  Gen 1 | Gen 2 |    Allocated |
|----------------------- |--------------- |---------------:|-------------:|-------------:|---------:|--------:|------------:|-------:|------:|-------------:|
|       MemoryCacheLayer |              1 |       136.9 us |      2.71 us |      2.40 us |     1.00 |    0.00 |     41.2598 |      - |     - |    126.83 KB |
|        RedisCacheLayer |              1 |    49,761.2 us |  1,029.10 us |  1,373.82 us |   366.25 |   14.21 |           - |      - |     - |    317.11 KB |
| ProtobufFileCacheLayer |              1 |   227,291.1 us |  4,533.02 us |  6,354.65 us | 1,662.04 |   48.53 |           - |      - |     - |    1575.7 KB |
|     JsonFileCacheLayer |              1 |   276,206.6 us |  5,463.95 us |  5,846.36 us | 2,023.36 |   60.97 |   1000.0000 |      - |     - |   3320.27 KB |
|      MongoDbCacheLayer |              1 |   457,461.3 us |  9,054.99 us | 17,873.66 us | 3,385.67 |  125.30 |  10000.0000 |      - |     - |  31073.59 KB |
|                        |                |                |              |              |          |         |             |        |       |              |
|       MemoryCacheLayer |             10 |     1,107.1 us |     14.27 us |     12.65 us |     1.00 |    0.00 |    261.7188 | 1.9531 |     - |    805.28 KB |
|        RedisCacheLayer |             10 |   470,178.5 us |  2,899.91 us |  2,570.70 us |   424.72 |    4.44 |   1000.0000 |      - |     - |   3129.15 KB |
| ProtobufFileCacheLayer |             10 | 1,903,920.6 us | 18,260.78 us | 17,081.15 us | 1,720.54 |   25.20 |   4000.0000 |      - |     - |  14579.13 KB |
|     JsonFileCacheLayer |             10 | 2,350,582.6 us | 19,487.75 us | 18,228.85 us | 2,122.86 |   33.53 |  10000.0000 |      - |     - |  31642.21 KB |
|      MongoDbCacheLayer |             10 | 3,692,059.0 us | 71,407.86 us | 79,369.65 us | 3,319.53 |   74.62 | 100000.0000 |      - |     - | 308501.52 KB |

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