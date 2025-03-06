namespace StbSharp.Examples;

using System;
using System.Diagnostics;
using Microsoft.Extensions.FileProviders;
using StbSharp;

public class WAAppBase : IDisposable
{
    private const int BACKGROUND_FPS = 30;
    private const int MAX_FPS = int.MaxValue;

    private StbGuiFont mainFont;
    private StbGuiRenderAdapter render_adapter;

    public struct MetricsInfo
    {
        public int Fps;
        public long TotalAllocatedBytes;
        public long LastFrameAllocatedBytes;
        public long TotalGarbageCollectionsPerformed;
    }

    public MetricsInfo Metrics { get; private set; } = new();

    public class WaAppOptions
    {
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

    private EmbeddedFileProvider embeddedFileProvider;

    protected byte[] GetFileBytes(string path)
    {
        var file = embeddedFileProvider.GetFileInfo(path);

        if (!file.Exists)
            throw new FileNotFoundException(path);

        using (var memoryStream = new MemoryStream())
        {
            using (var stream = file.CreateReadStream())
            {
                stream.CopyTo(memoryStream);
            }
            return memoryStream.ToArray();
        }

    }

    public WAAppBase(WaAppOptions options)
    {
        embeddedFileProvider = new EmbeddedFileProvider(typeof(WAAppBase).Assembly);

        CanvasInterop.Init();

        render_adapter = new WARenderAdapter();

        mainFont = new StbGuiFont(options.DefaultFontName, GetFileBytes(options.DefaultFontPath), options.DefaultFontSize, options.FontRenderingOversampling, options.FontRenderingBilinear, render_adapter);

        InitStbGui();

        sw = new Stopwatch();
        sw.Start();

        frames_count_ms = sw.ElapsedMilliseconds;
    }

    private Stopwatch sw;

    private long frames_count_ms;
    private long frames_count;

    public void LoopOnce()
    {
        UpdateCanvasSize();

        var frame_start_ms = sw.ElapsedMilliseconds;

        ProcessWAEvents();

        //if (quit) break;

        // Screen clearing is handled by STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME

        StbGui.stbg_begin_frame();
        {
            OnRenderStbGui();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        UpdateActiveCursor();

        UpdateMetrics();

        //SDL.RenderPresent(renderer);

        var frame_end_ms = sw.ElapsedMilliseconds;

        //var frame_ms = frame_end_ms - frame_start_ms;

        //FrameDelay(frame_ns);
    }

    private string last_active_cursor = "";

    private void UpdateActiveCursor()
    {
        string cursor = "";

        switch (StbGui.stbg_get_cursor())
        {
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.DEFAULT:
                cursor = "default";
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_N:
                cursor = "n-resize";
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_NE:
                cursor = "ne-resize";
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_NW:
                cursor = "nw-resize";
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_E:
                cursor = "e-resize";
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_W:
                cursor = "w-resize";
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_S:
                cursor = "s-resize";
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_SE:
                cursor = "se-resize";
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_SW:
                cursor = "sw-resize";
                break;
        }

        if (cursor != last_active_cursor)
        {
            last_active_cursor = cursor;

            CanvasInterop.SetCursor(cursor);
        }
    }

    private void UpdateMetrics()
    {
        var updatedMetrics = Metrics;

        // Update FPS
        var now_ms = sw.ElapsedMilliseconds;
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

    private enum WAEventType
    {
        MouseDown = 1,
        MouseUp = 2,
        MouseMove = 3,
        MouseWheel = 4,

        KeyDown = 10,
        KeyUp = 11,
    }

    private void ProcessWAEvents()
    {
        Span<char> tmp_string = stackalloc char[256];

        var count = CanvasInterop.GetEventsCount();


        for (int i = 0; i < count; i++)
        {
            var e = (WAEventType)CanvasInterop.GetEvent(i);

            switch (e)
            {
                case WAEventType.MouseDown:
                    StbGui.stbg_add_user_input_event_mouse_position(CanvasInterop.GetEventProperty(i, "x"), CanvasInterop.GetEventProperty(i, "y"), true);
                    StbGui.stbg_add_user_input_event_mouse_button(CanvasInterop.GetEventProperty(i, "button") + 1, true);
                    break;
                case WAEventType.MouseUp:
                    StbGui.stbg_add_user_input_event_mouse_position(CanvasInterop.GetEventProperty(i, "x"), CanvasInterop.GetEventProperty(i, "y"), true);
                    StbGui.stbg_add_user_input_event_mouse_button(CanvasInterop.GetEventProperty(i, "button") + 1, false);
                    break;
                case WAEventType.MouseMove:
                    StbGui.stbg_add_user_input_event_mouse_position(CanvasInterop.GetEventProperty(i, "x"), CanvasInterop.GetEventProperty(i, "y"), true);
                    break;
                case WAEventType.MouseWheel:
                    StbGui.stbg_add_user_input_event_mouse_position(CanvasInterop.GetEventProperty(i, "x"), CanvasInterop.GetEventProperty(i, "y"), true);
                    StbGui.stbg_add_user_input_event_mouse_wheel(MathF.Round(CanvasInterop.GetEventProperty(i, "dx") * 0.025f), -MathF.Round(CanvasInterop.GetEventProperty(i, "dy") * 0.025f));
                    break;

                case WAEventType.KeyDown:
                case WAEventType.KeyUp:
                    {
                        var modifiers = StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE;
                        if (CanvasInterop.GetEventProperty(i, "ctrl") != 0)
                            modifiers |= StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.CONTROL;
                        if (CanvasInterop.GetEventProperty(i, "shift") != 0)
                            modifiers |= StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.SHIFT;
                        if (CanvasInterop.GetEventProperty(i, "alt") != 0)
                            modifiers |= StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.ALT;
                        if (CanvasInterop.GetEventProperty(i, "meta") != 0)
                            modifiers |= StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.SUPER;

                        var down = e == WAEventType.KeyDown;

                        Span<char> code = GetEventPropertyString(i, "code", tmp_string);

                        if (SDL_KEY_MAPPINGS_ALTERNATE_LOOKUP.TryGetValue(code, out var mapped_key))
                        {
                            StbGui.stbg_add_user_input_event_keyboard_key(mapped_key, modifiers, down);
                        }
                        else
                        {
                            Span<char> key = GetEventPropertyString(i, "key", tmp_string);
                            if (key.Length == 1)
                                StbGui.stbg_add_user_input_event_keyboard_key_character(key[0], modifiers, down);
                        }
                        break;
                    }
            }

        }

        CanvasInterop.ClearEvents();
    }

    private static Span<char> GetEventPropertyString(int i, string property, Span<char> tmp_string)
    {
        Span<int> tmp_string_bytes = stackalloc int[tmp_string.Length];

        var len = CanvasInterop.GetEventPropertyString(i, property, tmp_string_bytes);
        for (int c = 0; c < len; c++)
            tmp_string[c] = (char)tmp_string_bytes[c];
        var code = tmp_string.Slice(0, len);
        return code;
    }

    static private Dictionary<string, StbGui.STBG_KEYBOARD_KEY> SDL_KEY_MAPPINGS = new() {
        // Basic arrows
        { "ArrowLeft", StbGui.STBG_KEYBOARD_KEY.LEFT },
        { "ArrowRight", StbGui.STBG_KEYBOARD_KEY.RIGHT },
        { "ArrowUp", StbGui.STBG_KEYBOARD_KEY.UP },
        { "ArrowDown", StbGui.STBG_KEYBOARD_KEY.DOWN },
        
        // Backspace
        { "Backspace", StbGui.STBG_KEYBOARD_KEY.BACKSPACE },

        // Delete
        { "Delete", StbGui.STBG_KEYBOARD_KEY.DELETE },

        // Enter
        { "Enter", StbGui.STBG_KEYBOARD_KEY.RETURN },

        // Control
        { "ControlLeft", StbGui.STBG_KEYBOARD_KEY.CONTROL_LEFT },
        { "ControlRight", StbGui.STBG_KEYBOARD_KEY.CONTROL_RIGHT },

        // Shift
        { "ShiftLeft", StbGui.STBG_KEYBOARD_KEY.SHIFT_LEFT },
        { "ShiftRight", StbGui.STBG_KEYBOARD_KEY.SHIFT_RIGHT },

        // Alt
        { "AltLeft", StbGui.STBG_KEYBOARD_KEY.ALT_LEFT },
        { "AltRight", StbGui.STBG_KEYBOARD_KEY.ALT_RIGHT },

        // Text navigation
        { "Home", StbGui.STBG_KEYBOARD_KEY.HOME },
        { "End", StbGui.STBG_KEYBOARD_KEY.END },
        { "Pageup", StbGui.STBG_KEYBOARD_KEY.PAGE_UP },
        { "PageDown", StbGui.STBG_KEYBOARD_KEY.PAGE_DOWN },
    };

    static private Dictionary<string, StbGui.STBG_KEYBOARD_KEY>.AlternateLookup<ReadOnlySpan<char>> SDL_KEY_MAPPINGS_ALTERNATE_LOOKUP = SDL_KEY_MAPPINGS.GetAlternateLookup<ReadOnlySpan<char>>();

    public void Dispose()
    {
        mainFont.Dispose();
    }

    private StbGui.stbg_external_dependencies BuildExternalDependencies()
    {
        return new StbGui.stbg_external_dependencies()
        {
            measure_text = (text, font, style, options) => MeasureText(text, font, style, options),
            get_character_position_in_text = (text, font, style, options, character_index) => GetCharacterPositionInText(text, font, style, options, character_index),
            render = (commands) =>
            {
                foreach (var cmd in commands)
                    ProcessRenderCommand(cmd);
            },
            set_input_method_editor = (options) =>
            {
                if (options.enable)
                {
                    /*
                    SDL.SetTextInputArea(window,
                        new SDL.Rect()
                        {
                            X = (int)options.editing_global_rect.x0,
                            Y = (int)options.editing_global_rect.y0,
                            W = (int)(options.editing_global_rect.x1 - options.editing_global_rect.x0),
                            H = (int)(options.editing_global_rect.y1 - options.editing_global_rect.y0)
                        },
                        (int)options.editing_cursor_global_x
                    );

                    SDL.StartTextInput(window);
                    */
                }
                else
                {
                    //SDL.StopTextInput(window);
                }
            },
            copy_text_to_clipboard = (text) =>
            {
                //SDL.SetClipboardText(text.ToString());
            },
            get_clipboard_text = () =>
            {
                return ""; //SDL.GetClipboardText();
            },
            get_time_milliseconds = () =>
            {
                return sw.ElapsedMilliseconds;
            },
            get_performance_counter = () =>
            {
                return sw.ElapsedMilliseconds;
            },
            get_performance_counter_frequency = () =>
            {
                return 1000;
            },
        };
    }

    private void ProcessRenderCommand(StbGui.stbg_render_command cmd)
    {
        switch (cmd.type)
        {
            case StbGui.STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME:
                {
                    var background_color = cmd.background_color;
                    CanvasInterop.Clear(background_color.r, background_color.g, background_color.b, background_color.a);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.END_FRAME:
                Debug.Assert(WAHelper.HasClipping() == false);
                break;


            case StbGui.STBG_RENDER_COMMAND_TYPE.BORDER:
                {
                    var color = cmd.color;
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;
                    var border_size = (int)Math.Ceiling(cmd.size);

                    CanvasInterop.DrawBorder(
                        background_color.r, background_color.g, background_color.b, background_color.a,
                        color.r, color.g, color.b, color.a,
                        bounds.x0, bounds.y0, bounds.x1 - bounds.x0, bounds.y1 - bounds.y0,
                        border_size
                    );
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.RECTANGLE:
                {
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;

                    CanvasInterop.DrawRectangle(
                        background_color.r, background_color.g, background_color.b, background_color.a,
                        bounds.x0, bounds.y0, bounds.x1 - bounds.x0, bounds.y1 - bounds.y0
                    );
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.TEXT:
                {
                    var bounds = cmd.bounds;
                    var text = cmd.text;

                    DrawText(text, bounds);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.PUSH_CLIPPING_RECT:
                WAHelper.PushClipRect(cmd.bounds);
                break;

            case StbGui.STBG_RENDER_COMMAND_TYPE.POP_CLIPPING_RECT:
                WAHelper.PopClipRect();
                break;
        }
    }

    private StbGui.stbg_size MeasureText(ReadOnlySpan<char> text, StbGui.stbg_font _, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options)
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

    private StbGui.stbg_position GetCharacterPositionInText(ReadOnlySpan<char> text, StbGui.stbg_font _, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options, int character_index)
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

    private void DrawText(StbGui.stbg_render_text_parameters text, StbGui.stbg_rect bounds)
    {
        var font = StbGui.stbg_get_font_by_id(text.font_id);

        StbGuiTextHelper.draw_text(text, bounds, mainFont, render_adapter);
    }

    private void InitStbGui()
    {
        StbGui.stbg_init(BuildExternalDependencies(), new());

        UpdateCanvasSize();

        int fontId = StbGui.stbg_add_font(mainFont.name);

        StbGui.stbg_init_default_theme(
            fontId,
            new() { size = mainFont.size, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE, color = StbGui.STBG_COLOR_WHITE }
        );
    }

    private static void UpdateCanvasSize()
    {
        int screenWidth = CanvasInterop.GetWidth();
        int screenHeight = CanvasInterop.GetHeight();
        StbGui.stbg_set_screen_size(screenWidth, screenHeight);
    }

    protected virtual void OnRenderStbGui()
    {

    }
}

