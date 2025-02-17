namespace StbSharp.Examples;

using System;
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

    public int Fps { private set; get; }

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

        mainFont = new SDLFont(options.DefaultFontName, options.DefaultFontPath, options.DefaultFontSize, renderer);

        InitStbGui();
    }

    public void MainLoop()
    {
        var frames_ticks = SDL.GetTicks();
        var frames_count = 0;

        var input = new StbGui.stbg_input();

        while (true)
        {
            ulong frame_start_ns = SDL.GetTicksNS();

            var quit = ProcessSDLEvents(ref input);

            if (quit) break;

            // Screen clearing is handled by STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME

            StbGui.stbg_set_input(input);

            StbGui.stbg_begin_frame();
            {
                OnRenderStbGui();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();

            UpdateActiveCursor();

            UpdateFps(ref frames_ticks, ref frames_count);

            // Screen presenting is handled by STBG_RENDER_COMMAND_TYPE.END_FRAME

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

    private void UpdateFps(ref ulong frames_ticks, ref int frames_count)
    {
        frames_count++;
        if (SDL.GetTicks() - frames_ticks > 1000)
        {
            Fps = (int)Math.Round(frames_count / ((SDL.GetTicks() - frames_ticks) / 1000.0f));

            frames_count = 0;
            frames_ticks = SDL.GetTicks();
        }
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

    private bool ProcessSDLEvents(ref StbGui.stbg_input input)
    {
        bool quit = false;

        input.mouse_delta = StbGui.stbg_build_position(0, 0);
        input.mouse_button_1_down = false;
        input.mouse_button_2_down = false;
        input.mouse_button_1_released = false;
        input.mouse_button_2_released = false;

        while (SDL.PollEvent(out var e) && !quit)
        {
            switch ((SDL.EventType)e.Type)
            {
                case SDL.EventType.Quit:
                    quit = true;
                    break;

                case SDL.EventType.MouseMotion:
                    input.mouse_position.x = e.Motion.X;
                    input.mouse_position.y = e.Motion.Y;
                    input.mouse_delta.x += e.Motion.XRel;
                    input.mouse_delta.y += e.Motion.YRel;
                    input.mouse_position_valid = true;
                    break;

                case SDL.EventType.MouseButtonDown:
                    if (e.Button.Button == 1)
                    {
                        input.mouse_button_1_down = true;
                        input.mouse_button_1_pressed = true;
                    }
                    else if (e.Button.Button == 2)
                    {
                        input.mouse_button_2_down = true;
                        input.mouse_button_2_pressed = true;
                    }
                    input.mouse_position.x = e.Button.X;
                    input.mouse_position.y = e.Button.Y;
                    input.mouse_position_valid = true;
                    break;

                case SDL.EventType.MouseButtonUp:
                    if (e.Button.Button == 1)
                    {
                        input.mouse_button_1_pressed = false;
                        input.mouse_button_1_released = true;
                    }
                    else if (e.Button.Button == 2)
                    {
                        input.mouse_button_2_pressed = false;
                        input.mouse_button_2_released = true;
                    }
                    input.mouse_position.x = e.Button.X;
                    input.mouse_position.y = e.Button.Y;
                    input.mouse_position_valid = true;
                    break;

                case SDL.EventType.WindowMouseLeave:
                    input.mouse_position_valid = false;
                    break;

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
            measure_text = (text, font, style) => MeasureText(text, font, style),
            render = (commands) =>
            {
                foreach (var cmd in commands)
                    ProcessRenderCommand(cmd);
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
                SDL.RenderPresent(renderer);
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
                    var text = cmd.text.text.Span;

                    DrawText(text, StbGui.stbg_get_font_by_id(cmd.text.font_id), cmd.text.style, bounds);
                    break;
                }
        }
    }

    private StbGui.stbg_size MeasureText(ReadOnlySpan<char> text, StbGui.stbg_font _font, StbGui.stbg_font_style style)
    {
        return mainFont.MeasureText(text, style);
    }

    private void DrawText(ReadOnlySpan<char> text, StbGui.stbg_font _font, StbGui.stbg_font_style style, StbGui.stbg_rect bounds)
    {
        mainFont.DrawText(text, style, bounds);
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

