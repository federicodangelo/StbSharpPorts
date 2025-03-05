```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3194)
13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.200
  [Host]     : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2


```
| Method          | WidgetsCount | Mean         | Error     | StdDev    |
|---------------- |------------- |-------------:|----------:|----------:|
| **WindowOneButton** | **1**            |     **139.9 ns** |   **0.37 ns** |   **0.35 ns** |
| **WindowOneButton** | **10**           |   **1,065.5 ns** |   **8.38 ns** |   **7.84 ns** |
| **WindowOneButton** | **100**          |  **10,412.8 ns** |  **65.35 ns** |  **61.13 ns** |
| **WindowOneButton** | **1000**         | **101,739.6 ns** | **377.98 ns** | **353.56 ns** |
