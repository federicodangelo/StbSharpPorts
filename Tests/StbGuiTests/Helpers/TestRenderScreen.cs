namespace StbSharp.Tests;

public record struct TestRenderScreenPixel
{
    public char character;
    public StbGui.stbg_color character_color;
    public StbGui.stbg_color background_color;
}

public class TestRenderScreen
{
    public readonly int Width;
    public readonly int Height;

    protected TestRenderScreenPixel[][] test_render_screen;

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
        test_render_screen[y][x] = pixel;
    }

    protected void SetTestRenderScreenPixelCharacter(int x, int y, char c)
    {
        test_render_screen[y][x].character = c;
    }

    protected void SetTestRenderScreenPixelCharacterAndColor(int x, int y, char c, StbGui.stbg_color color)
    {
        test_render_screen[y][x].character = c;
        test_render_screen[y][x].character_color = color;
    }

    public TestRenderScreenPixel GetTestRenderScreenPixel(int x, int y)
    {
        return test_render_screen[y][x];
    }

    public TestRenderScreenPixel this[int x, int y] => test_render_screen[y][x];

    public void Clear()
    {
        test_render_screen.ToList().ForEach(l => Array.Clear(l));
    }

    public void ProcessRenderCommand(StbGui.stbg_render_command cmd)
    {
        switch (cmd.type)
        {
            case StbGui.STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME:
                {
                    var bounds = cmd.bounds;
                    var clear_pixel = new TestRenderScreenPixel() { character = ' ', background_color = cmd.background_color };
                    for (int y = (int)bounds.top_left.y; y < (int)bounds.bottom_right.y; y++)
                    {
                        for (int x = (int)bounds.top_left.x; x < (int)bounds.bottom_right.x; x++)
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

                    for (int y = (int)bounds.top_left.y; y < (int)bounds.bottom_right.y; y++)
                    {
                        for (int x = (int)bounds.top_left.x; x < (int)bounds.bottom_right.x; x++)
                        {
                            char character =
                                x == (int)bounds.top_left.x || x == (int)bounds.bottom_right.x - 1 ? '|' :
                                y == (int)bounds.top_left.y || y == (int)bounds.bottom_right.y - 1 ? '-' :
                                ' ';

                            SetTestRenderScreenPixel(x, y, new() { character = character, background_color = background_color, character_color = character != ' ' ? color : GetTestRenderScreenPixel(x, y).character_color });
                        }
                    }
                    SetTestRenderScreenPixelCharacter((int)bounds.top_left.x, (int)bounds.top_left.y, '/');
                    SetTestRenderScreenPixelCharacter((int)bounds.bottom_right.x - 1, (int)bounds.top_left.y, '\\');
                    SetTestRenderScreenPixelCharacter((int)bounds.bottom_right.x - 1, (int)bounds.bottom_right.y - 1, '/');
                    SetTestRenderScreenPixelCharacter((int)bounds.top_left.x, (int)bounds.bottom_right.y - 1, '\\');
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.RECTANGLE:
                {
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;

                    for (int y = (int)bounds.top_left.y; y < (int)bounds.bottom_right.y; y++)
                    {
                        for (int x = (int)bounds.top_left.x; x < (int)bounds.bottom_right.x; x++)
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

                    for (int y = (int)bounds.top_left.y; y < (int)bounds.bottom_right.y && text_index < text.Length; y++)
                    {
                        for (int x = (int)bounds.top_left.x; x < (int)bounds.bottom_right.x && text_index < text.Length; x++)
                        {
                            char character = text[text_index++];
                            SetTestRenderScreenPixelCharacterAndColor(x, y, character, color);
                        }
                    }
                    break;
                }
        }
    }

    static public string[] ConvertPixelsToStrings(TestRenderScreenPixel[][] pixels,  bool color)
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