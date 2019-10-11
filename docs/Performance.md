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

|                  Method |   Mean [ns] | Error [ns] | StdDev [ns] |   Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |------------:|-----------:|------------:|--------:|------:|------:|--------------:|
|                 GetMiss |    200.2 ns |   3.728 ns |    3.487 ns |  0.2499 |     - |     - |         784 B |
|                  GetHit |    426.2 ns |   3.121 ns |    2.919 ns |  0.3057 |     - |     - |         960 B |
|      GetHitSimultaneous |    499.3 ns |   9.374 ns |    8.310 ns |  0.3290 |     - |     - |        1032 B |
|                  SetNew |    359.5 ns |   6.540 ns |    6.118 ns |  0.2828 |     - |     - |         888 B |
|             SetExisting |    603.5 ns |  11.706 ns |   10.377 ns |  0.3386 |     - |     - |        1064 B |
| SetExistingSimultaneous |    865.6 ns |  14.221 ns |   13.302 ns |  0.3948 |     - |     - |        1240 B |
|                 SetMany | 36,441.5 ns | 723.026 ns |  676.319 ns | 15.6860 |     - |     - |       49304 B |

## JsonFileCacheLayer

|                  Method |        Mean [ns] |     Error [ns] |    StdDev [ns] |      Median [ns] |     Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |-----------------:|---------------:|---------------:|-----------------:|----------:|------:|------:|--------------:|
|                 GetMiss |     803,555.4 ns |    59,882.9 ns |   168,900.8 ns |     752,600.0 ns |         - |     - |     - |       13912 B |
|                  GetHit |   3,335,938.3 ns |   158,111.7 ns |   451,101.7 ns |   3,362,500.0 ns |         - |     - |     - |       49776 B |
|      GetHitSimultaneous |   3,753,193.8 ns |   189,356.3 ns |   546,336.1 ns |   3,676,750.0 ns |         - |     - |     - |       58448 B |
|                  SetNew |   2,430,415.6 ns |   134,359.2 ns |   374,539.8 ns |   2,466,900.0 ns |         - |     - |     - |       40920 B |
|             SetExisting |   3,366,032.6 ns |   189,592.3 ns |   543,975.9 ns |   3,297,500.0 ns |         - |     - |     - |       54368 B |
| SetExistingSimultaneous |   4,115,435.1 ns |   226,482.9 ns |   646,168.6 ns |   3,995,450.0 ns |         - |     - |     - |       67816 B |
|                 SetMany | 145,527,648.3 ns | 2,967,779.9 ns | 4,350,133.3 ns | 144,857,300.0 ns | 1000.0000 |     - |     - |      603520 B |