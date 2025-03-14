using System.Diagnostics;
using System.Runtime.CompilerServices;

using StbSharp;

public class TestBenchmarksWindow : TestWindow
{
    public static StbGui.stbg_textbox_text_to_edit last_benchmark_result = StbGui.stbg_textbox_build_text_to_edit(1024);

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

            if (last_benchmark_result.length != 0)
            {
                StbGui.stbg_label(last_benchmark_result.text.Span.Slice(0, last_benchmark_result.length));
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
            sum += i * 3.1415;
        }

        sw.Stop();

        LogResult($"DOTNET [DOUBLE] - Sum: {sum} Time: {sw.Elapsed.TotalMilliseconds}ms");
    }

    private void RunBenchmarkDotNetFloat()
    {
        Stopwatch sw = Stopwatch.StartNew();

        float sum = 0;
        for (int i = 0; i < 100_000_000; i++)
        {
            sum += i * 3.1415f;
        }

        sw.Stop();

        LogResult($"DOTNET [FLOAT] - Sum: {sum} Time: {sw.Elapsed.TotalMilliseconds}ms");
    }

    private void RunBenchmarkDotNetInt()
    {
        Stopwatch sw = Stopwatch.StartNew();

        int sum = 0;
        for (int i = 0; i < 100_000_000; i++)
        {
            sum += i * 3;
        }

        sw.Stop();

        LogResult($"DOTNET [int] - Sum: {sum} Time: {sw.Elapsed.TotalMilliseconds}ms");
    }

    public static void LogResult(string result)
    {
        Console.WriteLine(result);
        StbGui.stbg_textbox_set_text_to_edit(ref last_benchmark_result, result);
    }
}
