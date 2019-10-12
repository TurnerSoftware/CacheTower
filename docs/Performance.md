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

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] |     Median [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|----------------:|------:|------:|------:|--------------:|
|                 GetMiss |  2,350,511.9 ns |    81,324.5 ns |   338,068.1 ns |  2,294,400.0 ns |     - |     - |     - |       29136 B |
|                  GetHit |  4,310,867.2 ns |   129,408.3 ns |   532,173.3 ns |  4,238,300.0 ns |     - |     - |     - |       39584 B |
|      GetHitSimultaneous |  4,758,606.8 ns |   158,378.9 ns |   656,623.7 ns |  4,691,050.0 ns |     - |     - |     - |       47256 B |
|                  SetNew |  3,390,805.3 ns |    92,245.6 ns |   380,381.2 ns |  3,440,550.0 ns |     - |     - |     - |       44728 B |
|             SetExisting |  4,145,332.4 ns |   107,578.7 ns |   437,544.1 ns |  4,159,300.0 ns |     - |     - |     - |       57488 B |
| SetExistingSimultaneous |  4,845,607.2 ns |   142,741.4 ns |   594,963.1 ns |  4,774,300.0 ns |     - |     - |     - |       68944 B |
|                 SetMany | 83,998,683.8 ns | 1,669,175.5 ns | 4,895,402.8 ns | 82,015,500.0 ns |     - |     - |     - |     1566224 B |