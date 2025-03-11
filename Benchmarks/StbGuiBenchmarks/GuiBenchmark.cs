using BenchmarkDotNet.Attributes;

using StbSharp;

public class GuiBenchmark
{
    protected readonly DummyRenderAdapter render_adapter = new DummyRenderAdapter();
    protected readonly StbGuiFont font;
    protected readonly StbGui.stbg_render_text_style_range[] styleRanges = new StbGui.stbg_render_text_style_range[1];

    public GuiBenchmark()
    {
        font = new StbGuiFont("ProggyClean", "Fonts/ProggyClean.ttf", 13, 1, false, render_adapter);
    }

    protected readonly StbGuiStringMemoryPool stringMemoryPool = new StbGuiStringMemoryPool();

    private StbGui.stbg_external_dependencies BuildExternalDependencies()
    {
        return new()
        {
            measure_text = (text, font, style, options) => new StbGui.stbg_size() { width = text.Length * style.size, height = style.size },
            render = (commands) =>
            {
            },
            copy_text_to_clipboard = (text) => { },
            get_clipboard_text = () => "",
            get_character_position_in_text = (text, font, style, options, character_index) =>
            {
                int x = 0;
                int y = 0;
                var single_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE) != 0;

                for (int i = 0; i < character_index; i++)
                {
                    char c = text[i];
                    if (i == character_index)
                    {
                        break;
                    }
                    if (c == '\n')
                    {
                        if (single_line)
                        {
                            c = ' ';
                        }
                        else
                        {
                            y += 1;
                            x = 0;
                            continue;
                        }
                    }
                    x++;
                }

                return StbGui.stbg_build_position(x, y);
            },
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

        var font_id = StbGui.stbg_add_font(font.name);
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
