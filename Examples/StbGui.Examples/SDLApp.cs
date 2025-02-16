namespace StbSharp.Examples;

using System;
using SDL3;
using StbSharp;

public class SDLApp : IDisposable
{
    private SDLFont mainFont;
    private nint renderer;

    private nint window;

    public SDLApp()
    {
        int screenWidth = 800;
        int screenHeight = 600;

        if (!SDL.Init(SDL.InitFlags.Video))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
            return;
        }

        if (!SDL.CreateWindowAndRenderer("SDL3 Create Window", screenWidth, screenHeight, 0, out window, out renderer))
        {
            SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
            return;
        }

        mainFont = new SDLFont("ProggyClean", "Fonts/ProggyClean.ttf", 13, renderer);

        InitStbGui(screenWidth, screenHeight);

    }    

    private StbGui.stbg_io stbg_io;

    public void MainLoop()
    {
        var loop = true;

        while (loop)
        {
            stbg_io.mouse_delta = StbGui.stbg_build_position(0, 0);

            while (SDL.PollEvent(out var e) && loop)
            {
                switch((SDL.EventType) e.Type)
                {
                    case SDL.EventType.Quit:
                        loop = false;
                        break;

                    case SDL.EventType.MouseMotion:
                        stbg_io.mouse_position.x = e.Motion.X;
                        stbg_io.mouse_position.y = e.Motion.Y;
                        stbg_io.mouse_delta.x += e.Motion.XRel;
                        stbg_io.mouse_delta.y += e.Motion.YRel;
                        break;

                    case SDL.EventType.MouseButtonDown:
                        if (e.Button.Button == 1)
                            stbg_io.mouse_button_1_down = true;
                        else if (e.Button.Button == 2)
                            stbg_io.mouse_button_2_down = true;
                        stbg_io.mouse_position.x = e.Button.X;
                        stbg_io.mouse_position.y = e.Button.Y;
                        break;

                    case SDL.EventType.MouseButtonUp:
                        if (e.Button.Button == 1)
                            stbg_io.mouse_button_1_down = false;
                        else if (e.Button.Button == 2)
                            stbg_io.mouse_button_2_down = false;
                        stbg_io.mouse_position.x = e.Button.X;
                        stbg_io.mouse_position.y = e.Button.Y;
                        break;

                }
            }

            if (!loop) continue;

            // Screen clearing is handled by STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME
            //SDL.SetRenderDrawColor(renderer, 100, 149, 237, 0);
            //SDL.RenderClear(renderer);

            StbGui.stbg_set_io(stbg_io);

            RenderStbGui();

            // Screen presenting is handled by STBG_RENDER_COMMAND_TYPE.END_FRAME
            //SDL.RenderPresent(renderer);
        }
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
                    var border_size = (int) Math.Ceiling(cmd.size);

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

                    /*int text_index = 0;

                    for (int y = (int)bounds.y0; y < (int)bounds.y1 && text_index < text.Length; y++)
                    {
                        for (int x = (int)bounds.x0; x < (int)bounds.x1 && text_index < text.Length; x++)
                        {
                            char character = text[text_index++];
                            SetTestRenderScreenPixelCharacterAndColor(x, y, character, color);
                        }
                    }*/
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
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_button("Button 1");
                if (StbGui.stbg_button("Toggle Button 3"))
                    showButton3 = !showButton3;
                
                if (showButton3)
                    StbGui.stbg_button("Button 3");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();
    }
}

