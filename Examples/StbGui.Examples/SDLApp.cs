namespace StbSharp.Examples;

using System;
using SDL3;
using StbSharp;

public class SDLApp : IDisposable
{
    private const int BACKGROUND_FPS = 30;
    private const int MAX_FPS = int.MaxValue;

    private SDLFont mainFont;
    private nint renderer;
    private nint window;

    private bool use_fake_vsync;

    public SDLApp()
    {
        int screenWidth = 800;
        int screenHeight = 600;

        if (!SDL.Init(SDL.InitFlags.Video))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
            throw new Exception(SDL.GetError());
        }

        if (!SDL.CreateWindowAndRenderer("StbGui Example App", screenWidth, screenHeight, 0, out window, out renderer))
        {
            SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
            throw new Exception(SDL.GetError());
        }

        SDL.SetWindowResizable(window, true);
        SDL.SetWindowBordered(window, true);
        SDL.SetWindowMinimumSize(window, 320, 100);
        if (!SDL.SetWindowSurfaceVSync(window, 1))
        {
            SDL.LogError(SDL.LogCategory.Application, $"Failed to enable window v-sync: {SDL.GetError()}, using fake v-sync");
            use_fake_vsync = true;
        }

        mainFont = new SDLFont("ProggyClean", "Fonts/ProggyClean.ttf", 13, renderer);

        InitStbGui(screenWidth, screenHeight);
    }

    private int fps = 0;

    public void MainLoop()
    {
        var loop = true;

        var frames_ticks = SDL.GetTicks();
        var frames_count = 0;

        var stbg_io = new StbGui.stbg_io();

        while (loop)
        {
            ulong frame_start_ns = SDL.GetTicksNS();

            loop = ProcessSDLEvents(ref stbg_io);

            if (!loop) break;

            // Screen clearing is handled by STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME
            //SDL.SetRenderDrawColor(renderer, 100, 149, 237, 0);
            //SDL.RenderClear(renderer);

            StbGui.stbg_set_io(stbg_io);

            RenderStbGui();

            frames_count++;
            if (SDL.GetTicks() - frames_ticks > 1000)
            {
                fps = (int)Math.Round(frames_count / ((SDL.GetTicks() - frames_ticks) / 1000.0f));

                frames_count = 0;
                frames_ticks = SDL.GetTicks();
            }

            // Screen presenting is handled by STBG_RENDER_COMMAND_TYPE.END_FRAME
            //SDL.RenderPresent(renderer);

            ulong frame_end_ns = SDL.GetTicksNS();

            var window_flags = SDL.GetWindowFlags(window);

            var background_window = !((window_flags & SDL.WindowFlags.InputFocus) != 0);

            if (use_fake_vsync || background_window)
            {
                var display = SDL.GetDisplayForWindow(window);

                var displayMode = SDL.GetCurrentDisplayMode(display);

                var refresh_rate = 60;

                if (displayMode.HasValue)
                {
                    refresh_rate = (int)Math.Round(displayMode.Value.RefreshRate);
                }

                refresh_rate = Math.Clamp(refresh_rate, 1, MAX_FPS);

                if (background_window)
                    refresh_rate = Math.Min(refresh_rate, BACKGROUND_FPS);

                ulong frame_delay_ns = 1_000_000_000 / (ulong)refresh_rate;

                var frame_ns = frame_end_ns - frame_start_ns;

                if (frame_ns < frame_delay_ns)
                {
                    SDL.DelayNS(frame_delay_ns - frame_ns);
                }
            }
        }
    }

    private bool ProcessSDLEvents(ref StbGui.stbg_io stbg_io)
    {
        bool loop = true;

        stbg_io.mouse_delta = StbGui.stbg_build_position(0, 0);

        while (SDL.PollEvent(out var e) && loop)
        {
            switch ((SDL.EventType)e.Type)
            {
                case SDL.EventType.Quit:
                    loop = false;
                    break;

                case SDL.EventType.MouseMotion:
                    stbg_io.mouse_position.x = e.Motion.X;
                    stbg_io.mouse_position.y = e.Motion.Y;
                    stbg_io.mouse_delta.x += e.Motion.XRel;
                    stbg_io.mouse_delta.y += e.Motion.YRel;
                    stbg_io.mouse_position_valid = true;
                    break;

                case SDL.EventType.MouseButtonDown:
                    if (e.Button.Button == 1)
                        stbg_io.mouse_button_1_down = true;
                    else if (e.Button.Button == 2)
                        stbg_io.mouse_button_2_down = true;
                    stbg_io.mouse_position.x = e.Button.X;
                    stbg_io.mouse_position.y = e.Button.Y;
                    stbg_io.mouse_position_valid = true;
                    break;

                case SDL.EventType.MouseButtonUp:
                    if (e.Button.Button == 1)
                        stbg_io.mouse_button_1_down = false;
                    else if (e.Button.Button == 2)
                        stbg_io.mouse_button_2_down = false;
                    stbg_io.mouse_position.x = e.Button.X;
                    stbg_io.mouse_position.y = e.Button.Y;
                    stbg_io.mouse_position_valid = true;
                    break;

                case SDL.EventType.WindowMouseLeave:
                    stbg_io.mouse_position_valid = false;
                    break;

                case SDL.EventType.WindowResized:
                    if (e.Window.WindowID == SDL.GetWindowID(window))
                    {
                        StbGui.stbg_set_screen_size(e.Window.Data1, e.Window.Data2);
                    }
                    break;
            }
        }

        return loop;
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

    private void InitStbGui(int screenWidth, int screenHeight)
    {
        StbGui.stbg_init(BuildExternalDependencies(), new());
        StbGui.stbg_set_screen_size(screenWidth, screenHeight);

        int fontId = StbGui.stbg_add_font(mainFont.Name);

        StbGui.stbg_init_default_theme(
            fontId,
            new() { size = mainFont.Size, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE, color = StbGui.STBG_COLOR_WHITE }
        );
    }

    private bool showButton3 = true;

    private void RenderStbGui()
    {
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_label("FPS: " + fps);

            StbGui.stbg_begin_window("Window 1");
            {
                if (StbGui.stbg_get_last_widget_is_new())
                    StbGui.stbg_move_window(StbGui.stbg_get_last_widget_id(), 100, 50);

                StbGui.stbg_button("Button 1");
                if (StbGui.stbg_button("Toggle Button 3"))
                    showButton3 = !showButton3;

                if (showButton3)
                    StbGui.stbg_button("Button 3");
            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2 with a REALLY REALLY REALLY REALLY REALLY REALLY REALLY REALLY REALLY long title");
            {
                if (StbGui.stbg_get_last_widget_is_new())
                    StbGui.stbg_move_window(StbGui.stbg_get_last_widget_id(), 200, 150);

                StbGui.stbg_button("Button 1");
                StbGui.stbg_button("Button 3");
            }
            StbGui.stbg_end_window();

            StbGui.stbg_label("This is the debug window!");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();
    }
}

