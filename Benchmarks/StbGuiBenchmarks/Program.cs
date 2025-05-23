using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;

var config = ManualConfig.
    CreateMinimumViable()
    .WithArtifactsPath("../Results")
    .AddExporter(MarkdownExporter.GitHub)
    .AddExporter(AsciiDocExporter.Default);

/*
BenchmarkRunner.Run<FontBenchmark_MeasureText>(config);
BenchmarkRunner.Run<FontBenchmark_DrawText_NoBackground>(config);
BenchmarkRunner.Run<FontBenchmark_DrawText_WithBackground>(config);
*/

/*
BenchmarkRunner.Run<LayoutBenchmark_Window_Empty>(config);
BenchmarkRunner.Run<LayoutBenchmark_Window_OneButton>(config);
BenchmarkRunner.Run<LayoutBenchmark_Window_TwoButton>(config);
*/

BenchmarkRunner.Run<RenderBenchmark_Window_Empty>(config);
BenchmarkRunner.Run<RenderBenchmark_Window_OneButton>(config);
BenchmarkRunner.Run<RenderBenchmark_Window_TwoButton>(config);
