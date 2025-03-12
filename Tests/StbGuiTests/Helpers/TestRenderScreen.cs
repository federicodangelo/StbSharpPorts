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

    private readonly Stack<StbGui.stbg_rect> clipping_rects = new();

    public TestRenderScreen(int width, int height)
    {
        Width = width;
        Height = height;

        test_render_screen = new TestRenderScreenPixel[height][];
        for (int i = 0; i < test_render_screen.Length; i++)
            test_render_screen[i] = new TestRenderScreenPixel[width];
    }

    public void SetTestRenderScreenPixel(int x, int y, TestRenderScreenPixel pixel)
    {
        if (IsClipped(x, y)) return;
        test_render_screen[y][x] = pixel;
    }

    public void SetTestRenderScreenPixelCharacter(int x, int y, char c)
    {
        if (IsClipped(x, y)) return;
        test_render_screen[y][x].character = c;
    }

    public void SetTestRenderScreenPixelCharacterAndColor(int x, int y, char c, StbGui.stbg_color color)
    {
        if (IsClipped(x, y)) return;
        test_render_screen[y][x].character = c;
        test_render_screen[y][x].character_color = color;
    }

    public void SetTestRenderScreenPixelCharacterAndColor(int x, int y, char c, StbGui.stbg_color color, StbGui.stbg_color background_color)
    {
        if (IsClipped(x, y)) return;
        test_render_screen[y][x].character = c;
        test_render_screen[y][x].character_color = color;
        test_render_screen[y][x].background_color = background_color;
    }

    public bool IsClipped(int x, int y)
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

    public void PushClippingRect(StbGui.stbg_rect rect)
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

    public void PopClippingRect()
    {
        clipping_rects.Pop();
    }

    public static string[] ConvertPixelsToStrings(TestRenderScreenPixel[][] pixels, bool color)
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
