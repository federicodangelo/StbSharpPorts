using BenchmarkDotNet.Attributes;
using StbSharp;
public class RenderBenchmark : GuiBenchmark
{
    [Params(1, 10, 100, 1_000)]
    public int WidgetsCount;
}