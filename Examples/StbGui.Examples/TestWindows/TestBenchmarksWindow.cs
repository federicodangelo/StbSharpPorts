using System.Diagnostics;
using System.Runtime.CompilerServices;
using StbSharp;

public class TestBenchmarksWindow : TestWindow
{
    public const string TITLE = "Test Benchmarks";

    public TestBenchmarksWindow(StbGuiAppBase appBase, StbGuiStringMemoryPool mp) : base(TITLE, appBase, mp)
    {
    }

    public override void Render()
    {
        if (StbGui.stbg_begin_window(title, ref open))
        {
            if (RuntimeFeature.IsDynamicCodeSupported)
            {
                StbGui.stbg_label("Non AOT version, benchmark results might not be accurate");
            }

            if (StbGui.stbg_button("Benchmark DotNet [DOUBLE]"))
            {
                RunBenchmarkDotNetDouble();
            }

            if (StbGui.stbg_button("Benchmark DotNet [FLOAT]"))
            {
                RunBenchmarkDotNetFloat();
            }

            if (StbGui.stbg_button("Benchmark DotNet [int]"))
            {
                RunBenchmarkDotNetInt();
            }

            StbGui.stbg_end_window();
        }
    }

    private void RunBenchmarkDotNetDouble()
    {
        Stopwatch sw = Stopwatch.StartNew();

        double sum = 0;
        for (int i = 0; i < 100_000_000; i++)
        {
            sum += Math.Sqrt(i);
        }

        sw.Stop();

        Console.WriteLine($"DOTNET [DOUBLE] - Sum: {sum} Time: {sw.Elapsed.TotalMilliseconds}ms");
    }

    private void RunBenchmarkDotNetFloat()
    {
        Stopwatch sw = Stopwatch.StartNew();

        float sum = 0;
        for (int i = 0; i < 100_000_000; i++)
        {
            sum += MathF.Sqrt(i);
        }

        sw.Stop();

        Console.WriteLine($"DOTNET [FLOAT] - Sum: {sum} Time: {sw.Elapsed.TotalMilliseconds}ms");
    }

    private void RunBenchmarkDotNetInt()
    {
        Stopwatch sw = Stopwatch.StartNew();

        int sum = 0;
        for (int i = 0; i < 100_000_000; i++)
        {
            sum += (int)MathF.Sqrt(i);
        }

        sw.Stop();

        Console.WriteLine($"DOTNET [int] - Sum: {sum} Time: {sw.Elapsed.TotalMilliseconds}ms");
    }
}
