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
|                 GetMiss |  2,285,449.7 ns |    75,362.0 ns |   313,281.9 ns |  2,213,700.0 ns |     - |     - |     - |       29280 B |
|                  GetHit |  4,294,189.1 ns |   128,181.0 ns |   531,425.9 ns |  4,300,450.0 ns |     - |     - |     - |       39264 B |
|      GetHitSimultaneous |  4,680,035.4 ns |   137,342.1 ns |   569,407.3 ns |  4,671,100.0 ns |     - |     - |     - |       47328 B |
|                  SetNew |  3,369,324.3 ns |    96,460.4 ns |   396,679.7 ns |  3,384,100.0 ns |     - |     - |     - |       44920 B |
|             SetExisting |  4,209,416.3 ns |   119,664.6 ns |   493,445.4 ns |  4,207,800.0 ns |     - |     - |     - |       56824 B |
| SetExistingSimultaneous |  4,882,134.5 ns |   135,897.2 ns |   566,435.6 ns |  4,837,100.0 ns |     - |     - |     - |       69136 B |
|                 SetMany | 83,035,035.4 ns | 1,633,726.1 ns | 3,224,815.9 ns | 82,019,250.0 ns |     - |     - |     - |     1591408 B |