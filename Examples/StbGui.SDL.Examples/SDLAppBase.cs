namespace StbSharp.Examples;

using System;
using System.Runtime.InteropServices;
using SDL3;
using StbSharp;

public abstract class SDLAppBase : StbGuiAppBase
{
    private const int BACKGROUND_FPS = 30;
    private const int MAX_FPS = int.MaxValue;

    private nint renderer;
    private nint window;
    private bool use_fake_vsync;
    private bool quit;

    public bool Quit => quit;

    public SDLAppBase(StbGuiAppOptions options) : base(options)
    {
    }

    protected override StbGuiRenderAdapter build_render_adapter(StbGuiAppOptions options)
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

        return new SDLRenderAdapter(renderer);
    }

    private SDL.SystemCursor last_active_cursor = SDL.SystemCursor.Default;

    protected override void update_active_cursor()
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

    private uint displayModeDisplay = uint.MaxValue;
    private SDL.DisplayMode? displayMode = null;

    protected override void present_frame(long frame_ms, bool rendered)
    {
        if (rendered)        
        {
            SDL.RenderPresent(renderer);
        }

        var window_flags = SDL.GetWindowFlags(window);

        var background_window = !((window_flags & SDL.WindowFlags.InputFocus) != 0);

        if (use_fake_vsync || background_window || !rendered)
        {
            var refresh_rate = 60;

            if (!background_window)
            {
                var display = SDL.GetDisplayForWindow(window);

                if (display != displayModeDisplay)
                {
                    displayModeDisplay = display;
                    displayMode = SDL.GetCurrentDisplayMode(display);
                }

                if (displayMode.HasValue)
                {
                    refresh_rate = (int)Math.Round(displayMode.Value.RefreshRate);
                }

                refresh_rate = Math.Clamp(refresh_rate, 1, MAX_FPS);
            }

            if (background_window)
                refresh_rate = Math.Min(refresh_rate, BACKGROUND_FPS);

            long frame_delay_ms = 1_000 / (long)refresh_rate;

            if (frame_ms < frame_delay_ms)
            {
                SDL.Delay((uint)(frame_delay_ms - frame_ms + 1));
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

    protected override void process_input_events()
    {
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
    }

    public override void Dispose()
    {
        base.Dispose();

        if (renderer != 0)
            SDL.DestroyRenderer(renderer);

        if (window != 0)
            SDL.DestroyWindow(window);

        SDL.Quit();
    }

    protected override StbGui.stbg_size get_screen_size()
    {
        SDL.GetWindowSize(window, out var screenWidth, out var screenHeight);

        return StbGui.stbg_build_size(screenWidth, screenHeight);
    }

    protected override long get_performance_counter_frequency()
    {
        return (long)SDL.GetPerformanceFrequency();
    }

    protected override long get_performance_counter()
    {
        return (long)SDL.GetPerformanceCounter();
    }

    protected override long get_time_milliseconds()
    {
        return (long)SDL.GetTicks();
    }

    protected override ReadOnlySpan<char> get_clipboard_text()
    {
        return SDL.GetClipboardText();
    }

    protected override void copy_text_to_clipboard(ReadOnlySpan<char> text)
    {
        SDL.SetClipboardText(text.ToString());
    }

    protected override void set_input_method_editor(StbGui.stbg_input_method_editor_info options)
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
    }
}
