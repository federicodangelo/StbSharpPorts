namespace StbSharp.Examples;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SDL3;
using StbSharp;

public class SDLAppBase : IDisposable
{
    private const int BACKGROUND_FPS = 30;
    private const int MAX_FPS = int.MaxValue;

    private SDLFont mainFont;
    private nint renderer;
    private nint window;

    private bool use_fake_vsync;

    public struct MetricsInfo
    {
        public int Fps;
        public long TotalAllocatedBytes;
        public long LastFrameAllocatedBytes;
        public long TotalGarbageCollectionsPerformed;
    }

    public MetricsInfo Metrics { get; private set; } = new();

    public class SdlAppOptions
    {
        public string WindowName = "StbGui SDL App";
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

    public SDLAppBase(SdlAppOptions options)
    {
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
            throw new Exception(SDL.GetError());
        }

        if (!SDL.CreateWindowAndRenderer(options.WindowName, options.DefaultWindowWidth, options.DefaultWindowHeight, 0, out window, out renderer))
        {
            SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
            throw new Exception(SDL.GetError());
        }

        if (!SDL.SetRenderVSync(renderer, 1))
        {
            SDL.LogError(SDL.LogCategory.Application, $"Failed to enable renderer v-sync: {SDL.GetError()}, using fake v-sync");
            use_fake_vsync = true;
        }

        SDL.SetWindowResizable(window, true);
        SDL.SetWindowBordered(window, true);
        SDL.SetWindowMinimumSize(window, options.MinWindowWidth, options.MinWindowHeight);
        SDL.EnableScreenSaver(); //Re-enable the screensaver that is disabled by default

        mainFont = new SDLFont(options.DefaultFontName, options.DefaultFontPath, options.DefaultFontSize, options.FontRenderingOversampling, options.FontRenderingBilinear, renderer);

        InitStbGui();
    }

    public void MainLoop()
    {
        var frames_count_ticks = SDL.GetTicks();
        var frames_count = 0;

        while (true)
        {
            ulong frame_start_ns = SDL.GetTicksNS();

            var quit = ProcessSDLEvents();

            if (quit) break;

            // Screen clearing is handled by STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME

            StbGui.stbg_begin_frame();
            {
                OnRenderStbGui();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();

            UpdateActiveCursor();

            UpdateMetrics(ref frames_count_ticks, ref frames_count);

            SDL.RenderPresent(renderer);

            ulong frame_end_ns = SDL.GetTicksNS();

            var frame_ns = frame_end_ns - frame_start_ns;

            FrameDelay(frame_ns);
        }
    }

    private SDL.SystemCursor last_active_cursor = SDL.SystemCursor.Default;

    private void UpdateActiveCursor()
    {
        SDL.SystemCursor cursor = SDL.SystemCursor.Default;

        switch (StbGui.stbg_get_cursor())
        {
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.DEFAULT:
                cursor = SDL.SystemCursor.Default;
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_N:
                cursor = SDL.SystemCursor.NResize;
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_NE:
                cursor = SDL.SystemCursor.NEResize;
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_NW:
                cursor = SDL.SystemCursor.NWResize;
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_E:
                cursor = SDL.SystemCursor.EResize;
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_W:
                cursor = SDL.SystemCursor.WResize;
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_S:
                cursor = SDL.SystemCursor.SResize;
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_SE:
                cursor = SDL.SystemCursor.SEResize;
                break;
            case StbGui.STBG_ACTIVE_CURSOR_TYPE.RESIZE_SW:
                cursor = SDL.SystemCursor.SWResize;
                break;
        }

        if (cursor != last_active_cursor)
        {
            last_active_cursor = cursor;

            var system_cursor = SDL.CreateSystemCursor(cursor);

            if (system_cursor != 0)
                SDL.SetCursor(system_cursor);
        }
    }

    private void UpdateMetrics(ref ulong frames_count_ticks, ref int frames_count)
    {
        var updatedMetrics = Metrics;

        // Update FPS
        frames_count++;
        if (SDL.GetTicks() - frames_count_ticks > 1000)
        {
            updatedMetrics.Fps = (int)Math.Round(frames_count / ((SDL.GetTicks() - frames_count_ticks) / 1000.0f));

            frames_count = 0;
            frames_count_ticks = SDL.GetTicks();
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

    private void FrameDelay(ulong frame_ns)
    {
        var window_flags = SDL.GetWindowFlags(window);

        var background_window = !((window_flags & SDL.WindowFlags.InputFocus) != 0);

        if (use_fake_vsync || background_window)
        {
            var refresh_rate = 60;

            if (!background_window)
            {
                var display = SDL.GetDisplayForWindow(window);

                var displayMode = SDL.GetCurrentDisplayMode(display);

                if (displayMode.HasValue)
                {
                    refresh_rate = (int)Math.Round(displayMode.Value.RefreshRate);
                }

                refresh_rate = Math.Clamp(refresh_rate, 1, MAX_FPS);
            }

            if (background_window)
                refresh_rate = Math.Min(refresh_rate, BACKGROUND_FPS);

            ulong frame_delay_ns = 1_000_000_000 / (ulong)refresh_rate;

            if (frame_ns < frame_delay_ns)
            {
                SDL.DelayNS(frame_delay_ns - frame_ns);
            }
        }
    }

    static private Dictionary<SDL.Scancode, StbGui.STBG_KEYBOARD_KEY> SDL_KEY_MAPPINGS = new() {
        // Basic arrows
        { SDL.Scancode.Left, StbGui.STBG_KEYBOARD_KEY.LEFT },
        { SDL.Scancode.Right, StbGui.STBG_KEYBOARD_KEY.RIGHT },
        { SDL.Scancode.Up, StbGui.STBG_KEYBOARD_KEY.UP },
        { SDL.Scancode.Down, StbGui.STBG_KEYBOARD_KEY.DOWN },
        
        // Backspace
        { SDL.Scancode.Backspace, StbGui.STBG_KEYBOARD_KEY.BACKSPACE },

        // Backspace
        { SDL.Scancode.Delete, StbGui.STBG_KEYBOARD_KEY.DELETE },

        // Enter
        { SDL.Scancode.Return, StbGui.STBG_KEYBOARD_KEY.RETURN },
        { SDL.Scancode.Return2, StbGui.STBG_KEYBOARD_KEY.RETURN },
        { SDL.Scancode.KpEnter, StbGui.STBG_KEYBOARD_KEY.RETURN },

        // Control
        { SDL.Scancode.RCtrl, StbGui.STBG_KEYBOARD_KEY.CONTROL_RIGHT },
        { SDL.Scancode.LCtrl, StbGui.STBG_KEYBOARD_KEY.CONTROL_LEFT },

        // Shift
        { SDL.Scancode.LShift, StbGui.STBG_KEYBOARD_KEY.SHIFT_LEFT },
        { SDL.Scancode.RShift, StbGui.STBG_KEYBOARD_KEY.SHIFT_RIGHT },

        // Alt
        { SDL.Scancode.LAlt, StbGui.STBG_KEYBOARD_KEY.ALT_LEFT },
        { SDL.Scancode.RAlt, StbGui.STBG_KEYBOARD_KEY.ALT_RIGHT },

        // Text navigation
        { SDL.Scancode.Home, StbGui.STBG_KEYBOARD_KEY.HOME },
        { SDL.Scancode.End, StbGui.STBG_KEYBOARD_KEY.END },
        { SDL.Scancode.Pageup, StbGui.STBG_KEYBOARD_KEY.PAGE_UP },
        { SDL.Scancode.Pagedown, StbGui.STBG_KEYBOARD_KEY.PAGE_DOWN },
    };

    private bool ProcessSDLEvents()
    {
        bool quit = false;

        Span<byte> tmp_bytes = stackalloc byte[64];
        Span<char> tmp_chars = stackalloc char[64];
        int tmp_chars_length = 0;

        while (SDL.PollEvent(out var e) && !quit)
        {
            switch ((SDL.EventType)e.Type)
            {
                case SDL.EventType.Quit:
                    quit = true;
                    break;

                case SDL.EventType.MouseMotion:
                    StbGui.stbg_add_user_input_event_mouse_position(e.Motion.X, e.Motion.Y);
                    break;

                case SDL.EventType.MouseButtonDown:
                    StbGui.stbg_add_user_input_event_mouse_position(e.Button.X, e.Button.Y, true);
                    StbGui.stbg_add_user_input_event_mouse_button(e.Button.Button, true);
                    break;

                case SDL.EventType.MouseButtonUp:
                    StbGui.stbg_add_user_input_event_mouse_position(e.Button.X, e.Button.Y);
                    StbGui.stbg_add_user_input_event_mouse_button(e.Button.Button, false);
                    break;

                case SDL.EventType.MouseWheel:
                    StbGui.stbg_add_user_input_event_mouse_position(e.Wheel.MouseX, e.Wheel.MouseY);
                    StbGui.stbg_add_user_input_event_mouse_wheel(e.Wheel.X, e.Wheel.Y);
                    break;

                case SDL.EventType.WindowMouseLeave:
                    StbGui.stbg_add_user_input_event_mouse_position(0, 0, false);
                    break;

                case SDL.EventType.TextInput:
                    if (e.Text.Text != 0)
                    {
                        var len = 0;
                        for (int i = 0; i < tmp_bytes.Length; i++)
                        {
                            var b = Marshal.ReadByte(e.Text.Text, i);
                            if (b == 0)
                                break;
                            tmp_bytes[i] = b;
                            len++;
                        }

                        if (len == tmp_bytes.Length)
                        {
                            // We ran out of buffer, allocate manually..
                            var text = Marshal.PtrToStringUTF8(e.Text.Text);
                            if (text != null)
                            {
                                foreach (var c in text)
                                {
                                    StbGui.stbg_add_user_input_event_keyboard_key_character(c, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
                                    StbGui.stbg_add_user_input_event_keyboard_key_character(c, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, false);
                                }
                            }
                        }
                        else if (System.Text.Encoding.UTF8.TryGetChars(tmp_bytes.Slice(0, len), tmp_chars, out tmp_chars_length))
                        {
                            for (var i = 0; i < tmp_chars_length; i++)
                            {
                                var c = tmp_chars[i];
                                StbGui.stbg_add_user_input_event_keyboard_key_character(c, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
                                StbGui.stbg_add_user_input_event_keyboard_key_character(c, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, false);
                            }
                        }
                    }
                    break;

                case SDL.EventType.KeyDown:
                case SDL.EventType.KeyUp:
                    {
                        var modifiers = StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE;
                        if ((e.Key.Mod & SDL.Keymod.Ctrl) != 0)
                            modifiers |= StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.CONTROL;
                        if ((e.Key.Mod & SDL.Keymod.Shift) != 0)
                            modifiers |= StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.SHIFT;
                        if ((e.Key.Mod & SDL.Keymod.Alt) != 0)
                            modifiers |= StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.ALT;
                        if ((e.Key.Mod & SDL.Keymod.GUI) != 0)
                            modifiers |= StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.SUPER;

                        var down = e.Key.Down;

                        // Some special keyboard keys are also valid unicode character, when we find one of those (Enter, Escape, Backspace, etc..)
                        // we handle it as a special key and NOT as a character
                        if (SDL_KEY_MAPPINGS.TryGetValue(e.Key.Scancode, out var mapped_key))
                        {
                            StbGui.stbg_add_user_input_event_keyboard_key(mapped_key, modifiers, down);
                        }
                        else
                        {

                            var key = e.Key.Key;
                            bool is_extended = (key & SDL.Keycode.ExtendedMask) != 0;
                            bool is_scancode = (key & SDL.Keycode.ScanCodeMask) != 0;

                            if (!is_scancode && !is_extended && key != SDL.Keycode.Unknown)
                            {
                                char c = (char)key;
                                if ((modifiers & StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.CONTROL) != 0 ||
                                    (modifiers & StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.ALT) != 0)
                                {
                                    // keys pressed in combination with CONTROL or ALT are not received as TextInput texts, 
                                    // since they represent possible keyboard shortcuts for other actions, so we deliver them
                                    // here manually
                                    StbGui.stbg_add_user_input_event_keyboard_key_character(c, modifiers, e.Key.Down);
                                }
                                else
                                {
                                    //Normal characters are handled as TextInput in case SDL.EventType.TextInput
                                }
                            }
                        }
                        break;
                    }

                case SDL.EventType.WindowResized:
                    if (e.Window.WindowID == SDL.GetWindowID(window))
                    {
                        StbGui.stbg_set_screen_size(e.Window.Data1, e.Window.Data2);
                    }
                    break;
            }
        }

        return quit;
    }

    public void Dispose()
    {
        mainFont.Dispose();

        if (renderer != 0)
            SDL.DestroyRenderer(renderer);

        if (window != 0)
            SDL.DestroyWindow(window);

        SDL.Quit();
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
                }
                else
                {
                    SDL.StopTextInput(window);
                }
            },
            copy_text_to_clipboard = (text) =>
            {
                SDL.SetClipboardText(text.ToString());
            },
            get_clipboard_text = () =>
            {
                return SDL.GetClipboardText();
            },
            get_time_milliseconds = () =>
            {
                return (long) SDL.GetTicks();
            },
            get_performance_counter = () =>
            {
                return (long) SDL.GetPerformanceCounter();
            },
            get_performance_counter_frequency = () =>
            {
                return (long) SDL.GetPerformanceFrequency();
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
                    SDL.SetRenderDrawColor(renderer, background_color.r, background_color.g, background_color.b, background_color.a);
                    SDL.RenderClear(renderer);
                    SDL.SetRenderDrawBlendMode(renderer, SDL.BlendMode.Blend);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.END_FRAME:
                Debug.Assert(SDLHelper.HasClipping() == false);
                break;


            case StbGui.STBG_RENDER_COMMAND_TYPE.BORDER:
                {
                    var color = cmd.color;
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;
                    var border_size = (int)Math.Ceiling(cmd.size);

                    SDL.SetRenderDrawColor(renderer, background_color.r, background_color.g, background_color.b, background_color.a);
                    SDL.RenderFillRect(renderer, new SDL.FRect() { X = bounds.x0, Y = bounds.y0, W = bounds.x1 - bounds.x0, H = bounds.y1 - bounds.y0 });
                    SDL.SetRenderDrawColor(renderer, color.r, color.g, color.b, color.a);
                    for (int i = 0; i < border_size; i++)
                        SDL.RenderRect(renderer, new SDL.FRect() { X = bounds.x0 + i, Y = bounds.y0 + i, W = bounds.x1 - bounds.x0 - i * 2, H = bounds.y1 - bounds.y0 - i * 2 });
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.RECTANGLE:
                {
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;

                    SDL.SetRenderDrawColor(renderer, background_color.r, background_color.g, background_color.b, background_color.a);
                    SDL.RenderFillRect(renderer, new SDL.FRect() { X = bounds.x0, Y = bounds.y0, W = bounds.x1 - bounds.x0, H = bounds.y1 - bounds.y0 });
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.TEXT:
                {
                    var bounds = cmd.bounds;
                    var text = cmd.text;

                    DrawText(cmd.text, bounds);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.PUSH_CLIPPING_RECT:
                SDLHelper.PushClipRect(renderer, cmd.bounds);
                break;

            case StbGui.STBG_RENDER_COMMAND_TYPE.POP_CLIPPING_RECT:
                SDLHelper.PopClipRect(renderer);
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

        return mainFont.MeasureText(text, style.size, tmp_styles, options);
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

        return mainFont.GetCharacterPositionInText(text, style.size, tmp_styles, options, character_index);
    }

    private void DrawText(StbGui.stbg_render_text_parameters text, StbGui.stbg_rect bounds)
    {
        var font = StbGui.stbg_get_font_by_id(text.font_id);
        mainFont.DrawText(text, bounds);
    }

    private void InitStbGui()
    {
        SDL.GetWindowSize(window, out var screenWidth, out var screenHeight);

        StbGui.stbg_init(BuildExternalDependencies(), new());
        StbGui.stbg_set_screen_size(screenWidth, screenHeight);

        int fontId = StbGui.stbg_add_font(mainFont.Name);

        StbGui.stbg_init_default_theme(
            fontId,
            new() { size = mainFont.Size, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE, color = StbGui.STBG_COLOR_WHITE }
        );
    }

    protected virtual void OnRenderStbGui()
    {

    }
}

