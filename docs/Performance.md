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

|                  Method |        Mean [ns] |     Error [ns] |    StdDev [ns] |     Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |-----------------:|---------------:|---------------:|----------:|------:|------:|--------------:|
|                 GetMiss |     782,938.2 ns |    40,287.0 ns |   111,635.0 ns |         - |     - |     - |       13976 B |
|                  GetHit |   3,354,977.2 ns |   153,793.6 ns |   433,777.6 ns |         - |     - |     - |       49712 B |
|      GetHitSimultaneous |   3,974,364.6 ns |   234,284.9 ns |   675,965.5 ns |         - |     - |     - |       58160 B |
|                  SetNew |   2,382,111.7 ns |   104,956.3 ns |   299,446.3 ns |         - |     - |     - |       40856 B |
|             SetExisting |   3,389,554.7 ns |   206,822.2 ns |   562,675.1 ns |         - |     - |     - |       53416 B |
| SetExistingSimultaneous |   4,133,131.6 ns |   208,807.6 ns |   599,108.0 ns |         - |     - |     - |       67752 B |
|                 SetMany | 146,527,917.1 ns | 2,899,614.5 ns | 4,764,149.8 ns | 1000.0000 |     - |     - |      598272 B |