#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;


public abstract class StbGuiAppBase : IDisposable
{
    public struct MetricsInfo
    {
        public int Fps;
        public long TotalAllocatedBytes;
        public long LastFrameAllocatedBytes;
        public long TotalGarbageCollectionsPerformed;
    }

    public MetricsInfo Metrics { get; private set; } = new();

    public class StbGuiAppOptions
    {
        public string WindowName = "StbGui App";
        public int DefaultWindowWidth = 800;
        public int DefaultWindowHeight = 600;
        public int MinWindowWidth = 320;
        public int MinWindowHeight = 100;
        public string DefaultFontPath = "Fonts/ProggyClean.ttf";
        public string DefaultFontName = "ProggyClean";
        public float DefaultFontSize = 13;
        public int FontRenderingOversampling = 1;
        public bool FontRenderingBilinear = false;
    }

    private StbGuiRenderAdapter render_adapter;
    private StbGuiFont mainFont;
    private long frames_count_ms;
    private long frames_count;


    public StbGuiAppBase(StbGuiAppOptions options)
    {
        render_adapter = build_render_adapter(options);

        mainFont = new StbGuiFont(options.DefaultFontName, get_file_bytes(options.DefaultFontPath), options.DefaultFontSize, options.FontRenderingOversampling, options.FontRenderingBilinear, render_adapter);

        init_stb_gui();

        frames_count_ms = get_time_milliseconds();
    }

    private StbGui.stbg_external_dependencies build_external_dependencies()
    {
        return new StbGui.stbg_external_dependencies()
        {
            measure_text = measure_text,
            get_character_position_in_text = get_character_position_in_text,
            render = (commands) =>
            {
                foreach (var cmd in commands)
                    render_adapter.process_render_command(cmd);
            },
            set_input_method_editor = set_input_method_editor,
            copy_text_to_clipboard = copy_text_to_clipboard,
            get_clipboard_text = get_clipboard_text,
            get_time_milliseconds = get_time_milliseconds,
            get_performance_counter = get_performance_counter,
            get_performance_counter_frequency = get_performance_counter_frequency,
        };
    }

    private StbGui.stbg_size measure_text(ReadOnlySpan<char> text, StbGui.stbg_font _, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options)
    {
        Span<StbGui.stbg_render_text_style_range> tmp_styles = stackalloc StbGui.stbg_render_text_style_range[1];

        tmp_styles[0] = new StbGui.stbg_render_text_style_range()
        {
            start_index = 0,
            text_color = style.color,
            font_style = style.style
        };

        return StbGuiTextHelper.measure_text(text, mainFont, style.size, tmp_styles, options);
    }

    private StbGui.stbg_position get_character_position_in_text(ReadOnlySpan<char> text, StbGui.stbg_font _, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options, int character_index)
    {
        Span<StbGui.stbg_render_text_style_range> tmp_styles = stackalloc StbGui.stbg_render_text_style_range[1];

        tmp_styles[0] = new StbGui.stbg_render_text_style_range()
        {
            start_index = 0,
            text_color = style.color,
            font_style = style.style
        };

        return StbGuiTextHelper.get_character_position_in_text(text, mainFont, style.size, tmp_styles, options, character_index);
    }

    private void init_stb_gui()
    {
        var screen_size = get_screen_size();

        StbGui.stbg_init(build_external_dependencies(), new());
        StbGui.stbg_set_screen_size((int)screen_size.width, (int)screen_size.height);

        int fontId = StbGui.stbg_add_font(mainFont.name);

        render_adapter.register_font(fontId, mainFont);

        StbGui.stbg_init_default_theme(
            fontId,
            new() { size = mainFont.size, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE, color = StbGui.STBG_COLOR_WHITE }
        );
    }


    public void loop_once()
    {
        var screen_size = get_screen_size();
        StbGui.stbg_set_screen_size((int)screen_size.width, (int)screen_size.height);

        var frame_start_ms = get_time_milliseconds();

        process_input_events();

        StbGui.stbg_begin_frame();
        {
            on_render_stbgui();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        update_active_cursor();

        update_metrics();

        var frame_end_ms = get_time_milliseconds();

        var frame_ms = frame_end_ms - frame_start_ms;

        present_frame(frame_ms);
    }

    protected void update_metrics()
    {
        var updatedMetrics = Metrics;

        // Update FPS
        var now_ms = get_time_milliseconds();
        frames_count++;
        if (now_ms - frames_count_ms > 1000)
        {
            updatedMetrics.Fps = (int)Math.Round(frames_count / ((now_ms - frames_count_ms) / 1000.0f));

            //Console.WriteLine($"FPS: {updatedMetrics.Fps}");

            frames_count = 0;
            frames_count_ms = now_ms;
        }

        // Update memory usage
        var totalAllocatedBytes = GC.GetTotalAllocatedBytes(true);

        updatedMetrics.LastFrameAllocatedBytes = totalAllocatedBytes - updatedMetrics.TotalAllocatedBytes;
        updatedMetrics.TotalAllocatedBytes = totalAllocatedBytes;

        var collectionCount = 0;

        for (var i = 0; i < GC.MaxGeneration; i++)
            collectionCount += GC.CollectionCount(i);

        updatedMetrics.TotalGarbageCollectionsPerformed = collectionCount;

        Metrics = updatedMetrics;
    }


    // Initialization
    protected abstract StbGuiRenderAdapter build_render_adapter(StbGuiAppOptions options);
    protected abstract StbGui.stbg_size get_screen_size();

    // External dependencies
    protected abstract long get_performance_counter_frequency();
    protected abstract long get_performance_counter();
    protected abstract long get_time_milliseconds();
    protected abstract ReadOnlySpan<char> get_clipboard_text();
    protected abstract void copy_text_to_clipboard(ReadOnlySpan<char> text);
    protected abstract void set_input_method_editor(StbGui.stbg_input_method_editor_info options);
    protected abstract byte[] get_file_bytes(string path);

    // Main Loop
    protected abstract void on_render_stbgui();
    protected abstract void update_active_cursor();
    protected abstract void process_input_events();
    protected abstract void present_frame(long frame_ms);

    public virtual void Dispose()
    {
        mainFont.Dispose();
    }
}
