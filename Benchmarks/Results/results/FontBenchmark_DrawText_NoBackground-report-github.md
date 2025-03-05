```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3194)
13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.200
  [Host]     : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2


```
| Method               | TextLength | Mean         | Error      | StdDev     |
|--------------------- |----------- |-------------:|-----------:|-----------:|
| **DrawTextNoBackground** | **1**          |     **40.30 ns** |   **0.121 ns** |   **0.113 ns** |
| **DrawTextNoBackground** | **10**         |    **122.85 ns** |   **0.269 ns** |   **0.238 ns** |
| **DrawTextNoBackground** | **100**        |    **993.69 ns** |   **2.501 ns** |   **2.217 ns** |
| **DrawTextNoBackground** | **1000**       |  **9,645.91 ns** |  **33.398 ns** |  **31.241 ns** |
| **DrawTextNoBackground** | **10000**      | **95,042.28 ns** | **310.302 ns** | **290.257 ns** |
