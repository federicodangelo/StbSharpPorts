using BenchmarkDotNet.Attributes;

using StbSharp;

public class GuiBenchmark
{
    protected readonly DummyRenderAdapter render_adapter = new DummyRenderAdapter();
    protected StbGuiFont font;
    protected readonly StbGui.stbg_render_text_style_range[] styleRanges = new StbGui.stbg_render_text_style_range[1];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public GuiBenchmark()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        
    }

    protected readonly StbGuiStringMemoryPool stringMemoryPool = new StbGuiStringMemoryPool();

    private StbGui.stbg_external_dependencies BuildExternalDependencies()
    {
        return new()
        {
            render_adapter = render_adapter,
            copy_text_to_clipboard = (text) => { },
            get_clipboard_text = () => "",
            set_input_method_editor = (info) => { },
            get_time_milliseconds = () => 0,
            get_performance_counter = () => 0,
            get_performance_counter_frequency = () => 1,
        };

    }

    [GlobalSetup]
    public void SetupImGui()
    {
        StbGui.stbg_init(BuildExternalDependencies(),
            new()
            {
                assert_behavior = StbGui.STBG_ASSERT_BEHAVIOR.EXCEPTION
            }
        );

        var font_id = StbGui.stbg_add_font("ProggyClean");
        font = new StbGuiFont("ProggyClean", "Fonts/ProggyClean.ttf", 13, 1, false, render_adapter);
        StbGui.stbg_init_default_theme(font_id, new() { color = StbGui.STBG_COLOR_BLACK, size = font.size, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE });
        StbGui.stbg_set_screen_size(1920, 1080);

        SetupContent();
    }

    protected virtual void SetupContent()
    {

    }

    [GlobalCleanup]
    public void DestroyImGui()
    {
        StbGui.stbg_destroy();

        stringMemoryPool.ResetPool();
    }
}
