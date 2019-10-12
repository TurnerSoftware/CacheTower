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

## MemoryCacheLayer

|                  Method |   Mean [ns] | Error [ns] |  StdDev [ns] |   Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |------------:|-----------:|-------------:|--------:|------:|------:|--------------:|
|                 GetMiss |    263.8 ns |   5.107 ns |     7.160 ns |  0.2499 |     - |     - |         784 B |
|                  GetHit |    500.5 ns |   9.462 ns |     8.850 ns |  0.3052 |     - |     - |         960 B |
|      GetHitSimultaneous |    597.5 ns |  11.746 ns |    17.217 ns |  0.3290 |     - |     - |        1032 B |
|                  SetNew |    461.4 ns |  11.080 ns |    32.670 ns |  0.2828 |     - |     - |         888 B |
|             SetExisting |    668.6 ns |  13.347 ns |    12.485 ns |  0.3386 |     - |     - |        1064 B |
| SetExistingSimultaneous |    976.5 ns |  20.851 ns |    31.209 ns |  0.3948 |     - |     - |        1240 B |
|                 SetMany | 37,740.0 ns | 747.389 ns | 1,790.697 ns | 12.0239 |     - |     - |       37776 B |

## JsonFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|                 GetMiss |  2,319,180.5 ns |    78,039.7 ns |   321,802.1 ns |     - |     - |     - |       29136 B |
|                  GetHit |  4,444,612.5 ns |   132,159.2 ns |   547,919.1 ns |     - |     - |     - |       38344 B |
|      GetHitSimultaneous |  4,764,031.4 ns |   129,717.4 ns |   540,677.6 ns |     - |     - |     - |       46472 B |
|                  SetNew |  3,378,316.8 ns |   103,556.3 ns |   428,179.7 ns |     - |     - |     - |       44064 B |
|             SetExisting |  4,237,861.2 ns |   125,704.5 ns |   526,728.7 ns |     - |     - |     - |       55912 B |
| SetExistingSimultaneous |  4,844,010.3 ns |   145,462.3 ns |   606,304.1 ns |     - |     - |     - |       68280 B |
|                 SetMany | 82,661,490.9 ns | 1,598,303.2 ns | 2,535,078.2 ns |     - |     - |     - |     1525360 B |