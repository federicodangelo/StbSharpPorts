using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using StbSharp;

//[MemoryDiagnoser]
//[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions, HardwareCounter.CacheMisses)]
public class FontBenchmark
{
    private readonly DummyRenderAdapter render_adapter = new DummyRenderAdapter();
    private StbGuiFont font = new StbGuiFont("ProggyClean", "Fonts/ProggyClean.ttf", 13, 1, false, new DummyRenderAdapter());
    private StbGui.stbg_render_text_style_range[] styleRanges = new StbGui.stbg_render_text_style_range[1];

    public string FullText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sit amet nisl a urna bibendum tincidunt. Fusce rhoncus, ex eu lobortis tempor, ligula orci porta lectus, at ultricies dui lorem in lectus. Morbi lacinia odio fringilla metus molestie rhoncus. Vestibulum condimentum arcu orci. Nullam maximus ultricies dictum. Etiam nec accumsan quam. Maecenas ultrices tellus quis nunc volutpat cursus. Aenean pellentesque ipsum nec fermentum mattis. Curabitur varius feugiat odio, at tristique neque porttitor quis.
Nulla quis tincidunt sapien. Ut ut purus justo. Suspendisse et nulla sed lorem blandit mollis. Integer placerat mi justo, vel sodales erat gravida vitae. Etiam nec feugiat mi. Aenean vel suscipit orci, in convallis dolor. Ut vel egestas odio. Integer quis ante id dolor laoreet egestas. Phasellus fermentum tincidunt laoreet. Curabitur felis magna, tristique eget molestie tincidunt, posuere sed quam. Integer nulla sapien, volutpat placerat tellus quis, feugiat dictum neque. Quisque eu turpis eleifend, mollis sem et, mollis nunc. Fusce ornare velit eu tincidunt blandit.
Sed scelerisque tellus nec pharetra facilisis. Mauris ac magna placerat dolor faucibus pretium. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam id justo sed ipsum venenatis pulvinar ac at dui. Nulla eget urna nec arcu rhoncus elementum. Aliquam accumsan dolor at purus scelerisque commodo. Vivamus eleifend eros ut diam elementum, in pellentesque justo congue. Curabitur condimentum ipsum a enim consequat, at iaculis quam pharetra.
Ut tristique sem mi. Proin non ullamcorper lectus, at tempor quam. Morbi auctor ex at pellentesque rhoncus. Nulla porttitor pellentesque ipsum. Quisque ultrices, diam non malesuada egestas, urna nulla aliquet velit, sit amet dapibus orci leo euismod felis. Sed gravida sem sit amet tellus porttitor venenatis. Sed porttitor scelerisque nibh vel volutpat. Nunc eget tortor ac orci luctus aliquet non a libero. Fusce convallis ante eu turpis pretium elementum. Nam vitae volutpat urna, ac tempus tortor.
Ut egestas sagittis libero in convallis. Aliquam sed ex et sapien iaculis aliquam consectetur quis lectus. In consectetur et orci non laoreet. Nullam in congue orci, quis condimentum nisl. In consequat nisi ac rhoncus vestibulum. Suspendisse commodo dui sit amet volutpat facilisis. Curabitur sollicitudin mi a odio sodales tincidunt. Proin iaculis urna nec blandit mollis. Fusce pulvinar aliquam lectus et mollis. Curabitur eu est felis.";

    [Params(1, 10, 100, 1000)]
    public int TextLength;

    [Benchmark]
    public void MeasureText()
    {
        styleRanges[0].text_color = StbGui.STBG_COLOR_WHITE;
        styleRanges[0].background_color = StbGui.STBG_COLOR_TRANSPARENT;
        StbGuiTextHelper.measure_text(FullText.AsSpan().Slice(0, TextLength), font, 13, styleRanges.AsSpan(), StbGui.STBG_MEASURE_TEXT_OPTIONS.NONE);
    }

    [Benchmark]
    public void DrawTextNoBackground()
    {
        styleRanges[0].text_color = StbGui.STBG_COLOR_WHITE;
        styleRanges[0].background_color = StbGui.STBG_COLOR_TRANSPARENT;
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

