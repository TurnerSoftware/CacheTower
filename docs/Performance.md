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
|                 GetMiss |  2,423,792.5 ns |   113,176.0 ns |   321,061.5 ns |     - |     - |     - |       29432 B |
|                  GetHit |  4,582,400.0 ns |   253,495.1 ns |   731,391.4 ns |     - |     - |     - |       39768 B |
|      GetHitSimultaneous |  4,899,756.2 ns |   233,517.4 ns |   647,075.7 ns |     - |     - |     - |       48440 B |
|                  SetNew |  3,693,362.1 ns |   172,958.0 ns |   496,248.8 ns |     - |     - |     - |       45056 B |
|             SetExisting |  4,518,695.8 ns |   221,226.8 ns |   634,741.1 ns |     - |     - |     - |       57672 B |
| SetExistingSimultaneous |  5,359,984.0 ns |   268,811.9 ns |   779,872.1 ns |     - |     - |     - |       70296 B |
|                 SetMany | 93,359,150.0 ns | 1,836,856.7 ns | 2,115,326.9 ns |     - |     - |     - |     1621800 B |