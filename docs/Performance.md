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
|                 GetMiss |  2,125,279.4 ns |    56,695.1 ns |   231,873.9 ns |     - |     - |     - |       28960 B |
|                  GetHit |  4,050,581.6 ns |   114,623.3 ns |   476,492.0 ns |     - |     - |     - |       38464 B |
|      GetHitSimultaneous |  4,408,501.0 ns |   118,136.6 ns |   489,782.9 ns |     - |     - |     - |       46344 B |
|                  SetNew |  3,187,503.2 ns |    80,339.9 ns |   329,483.3 ns |     - |     - |     - |       44096 B |
|             SetExisting |  3,887,191.1 ns |    90,491.4 ns |   375,168.5 ns |     - |     - |     - |       55992 B |
| SetExistingSimultaneous |  4,524,192.9 ns |   127,456.9 ns |   534,072.0 ns |     - |     - |     - |       68152 B |
|                 SetMany | 82,139,034.1 ns | 1,617,084.6 ns | 3,037,276.8 ns |     - |     - |     - |     1501608 B |

## ProtobufFileCacheLayer

|                  Method |       Mean [ns] |     Error [ns] |    StdDev [ns] | Gen 0 | Gen 1 | Gen 2 | Allocated [B] |
|------------------------ |----------------:|---------------:|---------------:|------:|------:|------:|--------------:|
|                 GetMiss |  1,248,374.2 ns |    27,430.2 ns |   110,626.1 ns |     - |     - |     - |        6592 B |
|                  GetHit |  2,738,936.9 ns |    74,074.4 ns |   302,952.4 ns |     - |     - |     - |        9296 B |
|      GetHitSimultaneous |  2,886,168.3 ns |    64,794.1 ns |   266,456.4 ns |     - |     - |     - |        9392 B |
|                  SetNew |  2,309,798.4 ns |    60,317.8 ns |   247,370.3 ns |     - |     - |     - |        9056 B |
|             SetExisting |  2,736,878.2 ns |    68,259.9 ns |   279,941.8 ns |     - |     - |     - |        9056 B |
| SetExistingSimultaneous |  3,155,860.2 ns |    70,524.2 ns |   287,635.6 ns |     - |     - |     - |        9136 B |
|                 SetMany | 55,075,658.5 ns | 1,090,291.8 ns | 2,275,845.0 ns |     - |     - |     - |        9040 B |