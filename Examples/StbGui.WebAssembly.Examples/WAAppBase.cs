namespace StbSharp.Examples;

using System;
using System.Diagnostics;
using Microsoft.Extensions.FileProviders;
using StbSharp;

public abstract class WAAppBase : StbGuiAppBase
{
    private readonly EmbeddedFileProvider embeddedFileProvider = new EmbeddedFileProvider(typeof(WAAppBase).Assembly);

    private readonly Stopwatch sw = Stopwatch.StartNew();


    public WAAppBase(StbGuiAppOptions options) : base(options)
    {
    }

    protected override StbGuiRenderAdapter build_render_adapter(StbGuiAppOptions options)
    {
        CanvasInterop.Init();

        CanvasInterop.SetTitle(options.WindowName);

        return new WARenderAdapter();
    }

    protected override void present_frame(long frame_ms)
    {
        // Nothing to do
    }

    private string last_active_cursor = "";

    protected override void update_active_cursor()
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

    private enum WAEventType
    {
        MouseDown = 1,
        MouseUp = 2,
        MouseMove = 3,
        MouseWheel = 4,

        KeyDown = 10,
        KeyUp = 11,
    }

    protected override void process_input_events()
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

    protected override void set_input_method_editor(StbGui.stbg_input_method_editor_info options)
    {
        // Not implemented
    }

    protected override void copy_text_to_clipboard(ReadOnlySpan<char> text)
    {
        // Not implemented
    }

    protected override ReadOnlySpan<char> get_clipboard_text()
    {
        return ""; // Not implemented
    }

    protected override long get_time_milliseconds()
    {
        return sw.ElapsedMilliseconds;
    }

    protected override long get_performance_counter()
    {
        return (long) sw.Elapsed.TotalMicroseconds;
    }

    protected override long get_performance_counter_frequency()
    {
        return 1_000_000; // 1 second = 1.000.000 microseconds
    }

    protected override StbGui.stbg_size get_screen_size()
    {
        int screenWidth = CanvasInterop.GetWidth();
        int screenHeight = CanvasInterop.GetHeight();

        return StbGui.stbg_build_size(screenWidth, screenHeight);
    }

    protected override byte[] get_file_bytes(string path)
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
}
