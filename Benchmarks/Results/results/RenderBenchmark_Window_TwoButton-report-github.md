```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3194)
13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.200
  [Host]     : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2


```
| Method          | WidgetsCount | Mean         | Error     | StdDev    |
|---------------- |------------- |-------------:|----------:|----------:|
| **WindowTwoButton** | **1**            |     **197.4 ns** |   **0.79 ns** |   **0.70 ns** |
| **WindowTwoButton** | **10**           |   **1,528.3 ns** |   **9.48 ns** |   **8.86 ns** |
| **WindowTwoButton** | **100**          |  **15,830.4 ns** |  **52.94 ns** |  **49.52 ns** |
| **WindowTwoButton** | **1000**         | **154,776.4 ns** | **808.36 ns** | **756.14 ns** |
