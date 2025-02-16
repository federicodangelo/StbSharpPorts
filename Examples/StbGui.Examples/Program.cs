namespace StbSharp.Examples;

using System;
using SDL3;
using StbSharp;

public class Program
{
    static private nint renderer;
    static private SDLFont mainFont;

    static private StbGui.stbg_external_dependencies BuildExternalDependencies()
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

    private static void ProcessRenderCommand(StbGui.stbg_render_command cmd)
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

    private static StbGui.stbg_size MeasureText(ReadOnlySpan<char> text, StbGui.stbg_font _font, StbGui.stbg_font_style style)
    {
        return mainFont.MeasureText(text, style);
    }

    private static void DrawText(ReadOnlySpan<char> text, StbGui.stbg_font _font, StbGui.stbg_font_style style, StbGui.stbg_rect bounds)
    {
        mainFont.DrawText(text, style, bounds);
    }

    static private void InitStbGui(int screenWidth, int screenHeight)
    {
        StbGui.stbg_init(BuildExternalDependencies(), new());
        StbGui.stbg_set_screen_size(screenWidth, screenHeight);

        int fontId = StbGui.stbg_add_font(mainFont.Name);

        StbGui.stbg_init_default_theme(
            fontId,
            new() { size = mainFont.FontSize, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE, color = StbGui.STBG_COLOR_WHITE }
        );

    }

    public static void Main(string[] args)
    {
        int screenWidth = 800;
        int screenHeight = 600;

        if (!SDL.Init(SDL.InitFlags.Video))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
            return;
        }

        if (!SDL.CreateWindowAndRenderer("SDL3 Create Window", screenWidth, screenHeight, 0, out var window, out renderer))
        {
            SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
            return;
        }

        mainFont = new SDLFont("ProggyClean", "Fonts/ProggyClean.ttf", 13, renderer);

        InitStbGui(screenWidth, screenHeight);

        var loop = true;

        while (loop)
        {
            while (SDL.PollEvent(out var e))
            {
                if ((SDL.EventType)e.Type == SDL.EventType.Quit)
                {
                    loop = false;
                }
            }

            // Screen clearing is handled by STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME
            //SDL.SetRenderDrawColor(renderer, 100, 149, 237, 0);
            //SDL.RenderClear(renderer);

            RenderStbGui();

            // Screen presenting is handled by STBG_RENDER_COMMAND_TYPE.END_FRAME
            //SDL.RenderPresent(renderer);
        }

        SDL.DestroyRenderer(renderer);
        SDL.DestroyWindow(window);

        SDL.Quit();
    }

    private static void RenderStbGui()
    {
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_button("Button 1");
                StbGui.stbg_button("Button 2");
                StbGui.stbg_button("Button 3");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();
    }
}

