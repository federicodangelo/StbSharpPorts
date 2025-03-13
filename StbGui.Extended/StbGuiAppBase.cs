#pragma warning disable IDE1006 // Naming Styles

using System.Diagnostics;
using System.Reflection;

namespace StbSharp;


public abstract class StbGuiAppBase : IDisposable
{
    public struct MetricsInfo
    {
        public int Fps;
        public int SkippedFrames;
        public long TotalAllocatedBytes;
        public long LastSecondAllocatedBytes;
        public long TotalGarbageCollectionsPerformed;
        public StbGui.stbg_performance_metrics average_performance_metrics;
    }

    public MetricsInfo Metrics { get; private set; } = new();

    public class StbGuiAppOptions
    {
        public string WindowName = "StbGui App";
        public int DefaultWindowWidth = 1024;
        public int DefaultWindowHeight = 768;
        public int MinWindowWidth = 320;
        public int MinWindowHeight = 100;
        public string DefaultFontPath = "Fonts/ProggyClean.ttf";
        public string DefaultFontName = "ProggyClean";
        public float DefaultFontSize = 13;
        public int FontRenderingOversampling = 1;
        public bool FontRenderingBilinear = false;
        public bool ForceAlwaysRender = false;
    }

    private readonly StbGui.stbg_render_adapter render_adapter;
    private long frames_count_ms;
    private int frames_count;
    private int skipped_frame_count;

    public string RenderBackend { get; private set; }

    protected byte[] GetResourceFileBytes(string fileName)
    {
        var resourceName = "StbGui.Extended." + fileName.Replace("\\", ".").Replace("/", ".");

        using (var memoryStream = new MemoryStream())
        {
            using (var stream = typeof(StbGuiAppBase).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Resource file not found.");

                stream.CopyTo(memoryStream);
            }
            return memoryStream.ToArray();
        }
    }


    public StbGuiAppBase(StbGuiAppOptions options)
    {
        render_adapter = build_render_adapter(options);

        RenderBackend = render_adapter.get_render_backend();

        init_stb_gui(options);

        frames_count_ms = get_time_milliseconds();
    }

    private StbGui.stbg_external_dependencies build_external_dependencies()
    {
        return new StbGui.stbg_external_dependencies()
        {
            render_adapter = render_adapter,
            set_input_method_editor = set_input_method_editor,
            copy_text_to_clipboard = copy_text_to_clipboard,
            get_clipboard_text = get_clipboard_text,
            get_time_milliseconds = get_time_milliseconds,
            get_performance_counter = get_performance_counter,
            get_performance_counter_frequency = get_performance_counter_frequency,
        };
    }

    public int add_font(string name, byte[] bytes, float size, int oversampling = 1, bool bilinear = false)
    {
        Debug.Assert(bytes != null);

        var font_id = StbGui.stbg_add_font(name);
        render_adapter.register_font(font_id, new() { name = name, size = size, oversampling = oversampling, bilinear_filtering = bilinear }, bytes);

        return font_id;
    }

    public int add_image(byte[] bytes, bool alpha)
    {
        Debug.Assert(bytes != null);

        var ptr = StbImage.stbi_load_from_memory(bytes, bytes.Length, out int width, out int height, out _, alpha ? StbImage.STBI_CHANNELS.rgb_alpha : StbImage.STBI_CHANNELS.rgb);
        Debug.Assert(!ptr.IsNull);

        var bytes_per_pixel = alpha ? 4 : 3;

        var pixels = ptr.Array;

        Debug.Assert(pixels != null);
        Debug.Assert(pixels.Length == width * height * bytes_per_pixel);

        return add_image_raw(pixels, width, height, bytes_per_pixel);
    }

    public int add_sub_image(int image_id, int x, int y, int width, int height)
    {
        return StbGui.stbg_add_sub_image(image_id, x, y, width, height);
    }

    public int add_image_raw(byte[] pixels, int width, int height, int bytes_per_pixel)
    {
        Debug.Assert(pixels != null);
        Debug.Assert(bytes_per_pixel == 3 || bytes_per_pixel == 4);

        Debug.Assert(pixels.Length == width * height * bytes_per_pixel);

        var image_id = StbGui.stbg_add_image(width, height);
        render_adapter.register_image(image_id, new() { }, pixels, width, height, bytes_per_pixel);

        return image_id;
    }

    private void init_stb_gui(StbGuiAppOptions options)
    {
        var screen_size = get_screen_size();

        StbGui.stbg_init(build_external_dependencies(), new()
        {
            render_options =
                StbGui.STBG_RENDER_OPTIONS.NONE &
                (options.ForceAlwaysRender ? StbGui.STBG_RENDER_OPTIONS.DISABLE_SKIP_RENDERING_OPTIMIZATION : StbGui.STBG_RENDER_OPTIONS.NONE)
        });
        StbGui.stbg_set_screen_size((int)screen_size.width, (int)screen_size.height);

        int default_font_id = add_font(options.DefaultFontName, GetResourceFileBytes(options.DefaultFontPath), options.DefaultFontSize, options.FontRenderingOversampling, options.FontRenderingBilinear);

        StbGui.stbg_init_default_theme(
            default_font_id,
            new() { size = options.DefaultFontSize, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE, color = StbGui.STBG_COLOR_WHITE }
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

        var rendered = StbGui.stbg_render();

        update_active_cursor();

        update_metrics(rendered);

        var frame_end_ms = get_time_milliseconds();

        var frame_ms = frame_end_ms - frame_start_ms;

        present_frame(frame_ms, rendered);
    }

    protected void update_metrics(bool rendered)
    {
        // Update FPS
        var now_ms = get_time_milliseconds();
        frames_count++;
        if (!rendered)
            skipped_frame_count++;
        if (now_ms - frames_count_ms > 1000)
        {
            var updatedMetrics = Metrics;

            // Update FPS
            updatedMetrics.Fps = (int)Math.Round(frames_count / ((now_ms - frames_count_ms) / 1000.0f));
            updatedMetrics.SkippedFrames = skipped_frame_count;

            frames_count = 0;
            frames_count_ms = now_ms;
            skipped_frame_count = 0;

            // Update memory usage
            var totalAllocatedBytes = GC.GetTotalAllocatedBytes(true);
            updatedMetrics.LastSecondAllocatedBytes = totalAllocatedBytes - updatedMetrics.TotalAllocatedBytes;
            updatedMetrics.TotalAllocatedBytes = totalAllocatedBytes;

            // Update garbage collection count
            var collectionCount = 0;
            for (var i = 0; i < GC.MaxGeneration; i++)
                collectionCount += GC.CollectionCount(i);
            updatedMetrics.TotalGarbageCollectionsPerformed = collectionCount;

            // Update average metrics
            updatedMetrics.average_performance_metrics = StbGui.stbg_get_average_performance_metrics();

            Metrics = updatedMetrics;
        }
    }


    // Initialization
    protected abstract StbGui.stbg_render_adapter build_render_adapter(StbGuiAppOptions options);
    protected abstract StbGui.stbg_size get_screen_size();

    // External dependencies
    protected abstract long get_performance_counter_frequency();
    protected abstract long get_performance_counter();
    protected abstract long get_time_milliseconds();
    protected abstract ReadOnlySpan<char> get_clipboard_text();
    protected abstract void copy_text_to_clipboard(ReadOnlySpan<char> text);
    protected abstract void set_input_method_editor(StbGui.stbg_input_method_editor_info options);

    // Main Loop
    protected abstract void on_render_stbgui();
    protected abstract void update_active_cursor();
    protected abstract void process_input_events();
    protected abstract void present_frame(long frame_ms, bool rendered);

    public virtual void Dispose()
    {
        render_adapter.destroy();
    }
}
