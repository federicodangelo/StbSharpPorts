using BenchmarkDotNet.Attributes;
using StbSharp;

public class FontBenchmark_MeasureText : FontBenchmark
{
    [Benchmark]
    public void MeasureText()
    {
        styleRanges[0].text_color = StbGui.STBG_COLOR_WHITE;
        styleRanges[0].background_color = StbGui.STBG_COLOR_TRANSPARENT;
        StbGuiTextHelper.measure_text(FullText.AsSpan().Slice(0, TextLength), font, 13, styleRanges.AsSpan(), StbGui.STBG_MEASURE_TEXT_OPTIONS.NONE);
    }
}
