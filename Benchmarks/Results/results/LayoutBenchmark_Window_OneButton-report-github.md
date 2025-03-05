```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3194)
13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.200
  [Host]     : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2


```
| Method          | WidgetsCount | Mean         | Error       | StdDev      |
|---------------- |------------- |-------------:|------------:|------------:|
| **WindowOneButton** | **1**            |     **493.5 ns** |     **3.43 ns** |     **3.21 ns** |
| **WindowOneButton** | **10**           |   **4,584.6 ns** |    **39.10 ns** |    **36.57 ns** |
| **WindowOneButton** | **100**          |  **44,043.4 ns** |   **124.65 ns** |   **116.60 ns** |
| **WindowOneButton** | **1000**         | **454,471.7 ns** | **1,094.05 ns** | **1,023.37 ns** |
