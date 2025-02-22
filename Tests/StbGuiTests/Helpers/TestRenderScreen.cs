using System.Diagnostics.CodeAnalysis;

namespace StbSharp.Tests;

public record struct TestRenderScreenPixel
{
    public char character;
    public StbGui.stbg_color character_color;
    public StbGui.stbg_color background_color;
}

[ExcludeFromCodeCoverage]
public class TestRenderScreen
{
    public readonly int Width;
    public readonly int Height;

    protected TestRenderScreenPixel[][] test_render_screen;

    private Stack<StbGui.stbg_rect> clipping_rects = new();

    public TestRenderScreen(int width, int height)
    {
        Width = width;
        Height = height;

        test_render_screen = new TestRenderScreenPixel[height][];
        for (int i = 0; i < test_render_screen.Length; i++)
            test_render_screen[i] = new TestRenderScreenPixel[width];
    }

    protected void SetTestRenderScreenPixel(int x, int y, TestRenderScreenPixel pixel)
    {
        if (IsClipped(x, y)) return;
        test_render_screen[y][x] = pixel;
    }

    protected void SetTestRenderScreenPixelCharacter(int x, int y, char c)
    {
        if (IsClipped(x, y)) return;
        test_render_screen[y][x].character = c;
    }

    protected void SetTestRenderScreenPixelCharacterAndColor(int x, int y, char c, StbGui.stbg_color color)
    {
        if (IsClipped(x, y)) return;
        test_render_screen[y][x].character = c;
        test_render_screen[y][x].character_color = color;
    }

    private bool IsClipped(int x, int y)
    {
        if (clipping_rects.Count == 0)
            return false;

        var clip = clipping_rects.Peek();

        return !StbGui.stbg_rect_is_position_inside(clip, x, y);
    }

    public TestRenderScreenPixel GetTestRenderScreenPixel(int x, int y)
    {
        return test_render_screen[y][x];
    }

    public TestRenderScreenPixel this[int x, int y] => test_render_screen[y][x];

    public void Clear()
    {
        test_render_screen.ToList().ForEach(l => Array.Clear(l));
        clipping_rects.Clear();
    }

    public void ProcessRenderCommand(StbGui.stbg_render_command cmd)
    {
        switch (cmd.type)
        {
            case StbGui.STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME:
                {
                    var bounds = cmd.bounds;
                    var clear_pixel = new TestRenderScreenPixel() { character = ' ', background_color = cmd.background_color };
                    for (int y = (int)bounds.y0; y < (int)bounds.y1; y++)
                    {
                        for (int x = (int)bounds.x0; x < (int)bounds.x1; x++)
                        {
                            SetTestRenderScreenPixel(x, y, clear_pixel);
                        }
                    }
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.BORDER:
                {
                    var color = cmd.color;
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;

                    for (int y = (int)bounds.y0; y < (int)bounds.y1; y++)
                    {
                        for (int x = (int)bounds.x0; x < (int)bounds.x1; x++)
                        {
                            char character =
                                x == (int)bounds.x0 || x == (int)bounds.x1 - 1 ? '|' :
                                y == (int)bounds.y0 || y == (int)bounds.y1 - 1 ? '-' :
                                ' ';

                            if (!IsClipped(x, y))
                                SetTestRenderScreenPixel(x, y, new() { character = character, background_color = background_color, character_color = character != ' ' ? color : GetTestRenderScreenPixel(x, y).character_color });
                        }
                    }
                    SetTestRenderScreenPixelCharacter((int)bounds.x0, (int)bounds.y0, '/');
                    SetTestRenderScreenPixelCharacter((int)bounds.x1 - 1, (int)bounds.y0, '\\');
                    SetTestRenderScreenPixelCharacter((int)bounds.x1 - 1, (int)bounds.y1 - 1, '/');
                    SetTestRenderScreenPixelCharacter((int)bounds.x0, (int)bounds.y1 - 1, '\\');
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.RECTANGLE:
                {
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;

                    for (int y = (int)bounds.y0; y < (int)bounds.y1; y++)
                    {
                        for (int x = (int)bounds.x0; x < (int)bounds.x1; x++)
                        {
                            char character = ' ';
                            SetTestRenderScreenPixel(x, y, new() { character = character, background_color = background_color, character_color = GetTestRenderScreenPixel(x, y).character_color });
                        }
                    }
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.TEXT:
                {
                    var bounds = cmd.bounds;
                    var text = cmd.text.text.Span;
                    var color = cmd.text.style.color;
                    int text_index = 0;

                    for (int y = (int)bounds.y0; y < (int)bounds.y1 && text_index < text.Length; y++)
                    {
                        for (int x = (int)bounds.x0; x < (int)bounds.x1 && text_index < text.Length; x++)
                        {
                            char character = text[text_index++];
                            SetTestRenderScreenPixelCharacterAndColor(x, y, character, color);
                        }
                    }
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.PUSH_CLIPPING_RECT:
                PushClippingRect(cmd.bounds);
                break;


            case StbGui.STBG_RENDER_COMMAND_TYPE.POP_CLIPPING_RECT:
                PopClippingRect();
                break;
        }
    }

    private void PushClippingRect(StbGui.stbg_rect rect)
    {
        if (clipping_rects.Count > 0)
        {
            clipping_rects.Push(StbGui.stbg_clamp_rect(rect, clipping_rects.Peek()));
        }
        else
        {
            clipping_rects.Push(rect);
        }
    }

    private void PopClippingRect()
    {
        clipping_rects.Pop();
    }

    static public string[] ConvertPixelsToStrings(TestRenderScreenPixel[][] pixels, bool color)
    {
        string[] lines = new string[pixels.Length];

        for (int y = 0; y < pixels.Length; y++)
        {
            var line = "";
            var pixelsLine = pixels[y];

            for (int x = 0; x < pixelsLine.Length; x++)
            {
                var pixel = pixelsLine[x];

                if (color)
                {
                    line += ColorsHelper.GetAnsiColor(pixel.character_color, pixel.background_color) + pixel.character.ToString() + ColorsHelper.GetAnsiColorReset();
                }
                else
                {
                    line += pixel.character.ToString();
                }
            }

            lines[y] = line;
        }

        return lines;
    }
}