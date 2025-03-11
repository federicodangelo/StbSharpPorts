using BenchmarkDotNet.Attributes;

using StbSharp;

public class FontBenchmark_DrawText_WithBackground : FontBenchmark
{
    [Benchmark]
    public void DrawTextWithBackground()
    {
        styleRanges[0].text_color = StbGui.STBG_COLOR_WHITE;
        styleRanges[0].background_color = StbGui.STBG_COLOR_RED;
        var bounds = StbGui.stbg_build_rect_infinite();
        var parameters = new StbGui.stbg_render_text_parameters()
        {
            text = FullText.AsMemory().Slice(0, TextLength),
            font_id = 0,
            font_size = 13,
            style_ranges = styleRanges.AsMemory(),
            horizontal_alignment = -1,
            vertical_alignment = -1,
        };

        StbGuiTextHelper.draw_text(parameters, bounds, font, render_adapter);
    }
}
