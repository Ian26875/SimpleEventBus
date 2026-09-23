```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26200.9457)
AMD Ryzen 7 250 w/ Radeon 780M Graphics 3.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.401
  [Host]   : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  ShortRun : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                          | Mean         | Error        | StdDev      | Gen0   | Allocated |
|-------------------------------- |-------------:|-------------:|------------:|-------:|----------:|
| Publish_CorePipeline            |   308.157 ns |    25.582 ns |   1.4022 ns | 0.0734 |     616 B |
| Dispatch_SingleInterfaceHandler |   141.449 ns |   138.204 ns |   7.5754 ns | 0.0582 |     488 B |
| Dispatch_DelegateHandler        |   727.619 ns |   108.237 ns |   5.9328 ns | 0.0525 |     440 B |
| Dispatch_ThreeHandlers          | 2,246.990 ns | 3,829.522 ns | 209.9090 ns | 0.2289 |    1938 B |
| Registry_GetEventName           |     3.516 ns |     1.724 ns |   0.0945 ns |      - |         - |
