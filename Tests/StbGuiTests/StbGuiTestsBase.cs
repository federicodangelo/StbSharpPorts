using System.Buffers;
using System.Diagnostics;
using StbSharp;

namespace StbSharp.Tests;

[Collection("Sequential")]
public class StbGuiTestsBase : IDisposable
{
    protected const int ScreenSizeWidth = 120;
    protected const int ScreenSizeHeight = 80;

    protected record struct TestRenderScreenPixel
    {
        public char character;
        public StbGui.stbg_color character_color;
        public StbGui.stbg_color background_color;
    }

    static protected TestRenderScreenPixel[][] test_render_screen;

    static protected List<StbGui.stbg_render_command> render_commands_all = []; // All render commands
    static protected List<StbGui.stbg_render_command> render_commands = []; // Exclude begin and end frame commands

    static StbGuiTestsBase()
    {
        test_render_screen = new TestRenderScreenPixel[ScreenSizeHeight][];
        for (int i = 0; i < test_render_screen.Length; i++)
            test_render_screen[i] = new TestRenderScreenPixel[ScreenSizeWidth];
    }

    static private void SetTestRenderScreenPixel(int x, int y, TestRenderScreenPixel pixel)
    {
        test_render_screen[y][x] = pixel;
    }

    static private void SetTestRenderScreenPixelCharacter(int x, int y, char c)
    {
        test_render_screen[y][x].character = c;
    }

    static private void SetTestRenderScreenPixelCharacterAndColor(int x, int y, char c, StbGui.stbg_color color)
    {
        test_render_screen[y][x].character = c;
        test_render_screen[y][x].character_color = color;
    }

    static protected TestRenderScreenPixel GetTestRenderScreenPixel(int x, int y)
    {
        return test_render_screen[y][x];
    }

    public void Dispose()
    {
        AssertHierarchyConsistency();
        DestroyGui();
        render_commands_all = [];
        render_commands = [];
        test_render_screen.ToList().ForEach(l => Array.Clear(l));
    }

    static private StbGui.stbg_external_dependencies BuildExternalDependencies()
    {
        return new StbGui.stbg_external_dependencies()
        {
            measure_text = (text, font, style) => new StbGui.stbg_size() { width = text.Length * style.size, height = style.size },
            render = (commands) =>
            {
                render_commands_all.AddRange(commands);
                render_commands.AddRange(commands.ToArray().Where(c => c.type != StbGui.STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME && c.type != StbGui.STBG_RENDER_COMMAND_TYPE.END_FRAME));
            },
        };
    }

    static protected void RenderCommandsToTestScreen()
    {
        foreach (var cmd in render_commands_all)
        {
            ProcessRenderCommand(cmd);
        }
    }

    private static void ProcessRenderCommand(StbGui.stbg_render_command cmd)
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

    protected readonly int COLOR_RED = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_RED);
    protected readonly int COLOR_GREEN = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_GREEN);
    protected readonly int COLOR_BLUE = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_BLUE);
    protected readonly int COLOR_YELLOW = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_YELLOW);
    protected readonly int COLOR_CYAN = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_CYAN);
    protected readonly int COLOR_MAGENTA = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_MAGENTA);
    protected readonly int COLOR_WHITE = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_WHITE);
    protected readonly int COLOR_BLACK = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_BLACK);
    protected readonly int COLOR_TRANSPARENT = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_TRANSPARENT);

    static protected void AssertScreenEqual(string[] expectedLines, int[][][] expectedColors)
    {
        Assert.True(expectedLines.Length == expectedColors.Length, "Wrong expected colors");

        bool valid = true;

        TestRenderScreenPixel[][] expectedPixels = new TestRenderScreenPixel[expectedLines.Length][];
        for (int y = 0; y < expectedLines.Length; y++)
        {
            var line = expectedLines[y];
            var colors = expectedColors[y];

            var pixels = new TestRenderScreenPixel[line.Length];

            for (int x = 0; x < line.Length; x++)
            {
                pixels[x].character = line[x];

                int acc_len = 0;
                for (int n = 0; n < colors.Length; n++)
                {
                    var info = colors[n];

                    if (info.Length != 3)
                        Assert.Fail("Wrong color information in row " + y);

                    int backgroundColor = info[0];
                    int color = info[1];
                    int len = info[2];

                    if (len == 0)
                        Assert.Fail("Wrong color information in row " + y);

                    if (x < len + acc_len)
                    {
                        pixels[x].character_color = StbGui.stbg_uint_to_color((uint)color);
                        pixels[x].background_color = StbGui.stbg_uint_to_color((uint)backgroundColor); ;
                        break;
                    }

                    acc_len += len;
                }
            }

            expectedPixels[y] = pixels;
        }

        for (int y = 0; y < expectedPixels.Length; y++)
        {
            for (int x = 0; x < expectedPixels[y].Length; x++)
            {
                var expectedPixel = expectedPixels[y][x];
                var actualPixel = GetTestRenderScreenPixel(x, y);

                if (expectedPixel != actualPixel)
                {
                    if (expectedPixel.character != actualPixel.character)
                        Console.Error.WriteLine($"[{x},{y}] Expected: '{expectedPixel.character}' Actual: '{actualPixel.character}'");
                    else
                        Console.Error.WriteLine($"[{x},{y}] Expected: '{expectedPixel.character}' Actual: '{actualPixel.character}'");

                    valid = false;
                }
            }
        }

        if (!valid)
        {
            const string ANSI_COLOR_RESET = "\x1b[0m";

            const string ANSI_COLOR_PREFIX = "\x1b[1;";
            const string ANSI_COLOR_SEPARATOR = ";";
            const string ANSI_COLOR_SUFFIX = "m";

            const int ANSI_COLOR_BLACK = 30;
            const int ANSI_COLOR_RED = 31;
            const int ANSI_COLOR_GREEN = 32;
            const int ANSI_COLOR_YELLOW = 33;
            const int ANSI_COLOR_BLUE = 34;
            const int ANSI_COLOR_MAGENTA = 35;
            const int ANSI_COLOR_CYAN = 36;
            const int ANSI_COLOR_WHITE = 37;
            const int ANSI_COLOR_DEFAULT = 39;

            const int ANSI_COLOR_BACKGROUND_BLACK = 40;
            const int ANSI_COLOR_BACKGROUND_RED = 41;
            const int ANSI_COLOR_BACKGROUND_GREEN = 42;
            const int ANSI_COLOR_BACKGROUND_YELLOW = 43;
            const int ANSI_COLOR_BACKGROUND_BLUE = 44;
            const int ANSI_COLOR_BACKGROUND_MAGENTA = 45;
            const int ANSI_COLOR_BACKGROUND_CYAN = 46;
            const int ANSI_COLOR_BACKGROUND_WHITE = 47;
            const int ANSI_COLOR_BACKGROUND_DEFAULT = 49;

            string GetAnsiColor(StbGui.stbg_color color, StbGui.stbg_color backgroundColor)
            {
                if (color.a == 0 && backgroundColor.a == 0)
                    return "";

                string c = ANSI_COLOR_PREFIX;

                if (color == StbGui.STBG_COLOR_BLACK)
                    c += ANSI_COLOR_BLACK;
                else if (color == StbGui.STBG_COLOR_RED)
                    c += ANSI_COLOR_RED;
                else if (color == StbGui.STBG_COLOR_GREEN)
                    c += ANSI_COLOR_GREEN;
                else if (color == StbGui.STBG_COLOR_YELLOW)
                    c += ANSI_COLOR_YELLOW;
                else if (color == StbGui.STBG_COLOR_BLUE)
                    c += ANSI_COLOR_BLUE;
                else if (color == StbGui.STBG_COLOR_MAGENTA)
                    c += ANSI_COLOR_MAGENTA;
                else if (color == StbGui.STBG_COLOR_CYAN)
                    c += ANSI_COLOR_CYAN;
                else if (color == StbGui.STBG_COLOR_WHITE)
                    c += ANSI_COLOR_WHITE;
                else
                    c += ANSI_COLOR_DEFAULT;

                c += ANSI_COLOR_SEPARATOR;

                if (backgroundColor == StbGui.STBG_COLOR_BLACK)
                    c += ANSI_COLOR_BACKGROUND_BLACK;
                else if (backgroundColor == StbGui.STBG_COLOR_RED)
                    c += ANSI_COLOR_BACKGROUND_RED;
                else if (backgroundColor == StbGui.STBG_COLOR_GREEN)
                    c += ANSI_COLOR_BACKGROUND_GREEN;
                else if (backgroundColor == StbGui.STBG_COLOR_YELLOW)
                    c += ANSI_COLOR_BACKGROUND_YELLOW;
                else if (backgroundColor == StbGui.STBG_COLOR_BLUE)
                    c += ANSI_COLOR_BACKGROUND_BLUE;
                else if (backgroundColor == StbGui.STBG_COLOR_MAGENTA)
                    c += ANSI_COLOR_BACKGROUND_MAGENTA;
                else if (backgroundColor == StbGui.STBG_COLOR_CYAN)
                    c += ANSI_COLOR_BACKGROUND_CYAN;
                else if (backgroundColor == StbGui.STBG_COLOR_WHITE)
                    c += ANSI_COLOR_BACKGROUND_WHITE;
                else
                    c += ANSI_COLOR_BACKGROUND_DEFAULT;

                c += ANSI_COLOR_SUFFIX;

                return c;
            }

            // Interleave ASCII colors into the expected lines
            for (int y = 0; y < expectedLines.Length; y++)
            {
                var line = expectedLines[y];
                var lineWithColors = "";
                for (int x = 0; x < line.Length; x++)
                {
                    lineWithColors += GetAnsiColor(expectedPixels[y][x].character_color, expectedPixels[y][x].background_color) + line[x] + ANSI_COLOR_RESET;
                }
                expectedLines[y] = lineWithColors;
            }

            string joinedExpectedLines = string.Join("\n", expectedLines);

            string[] actualLines = test_render_screen.Select(l => string.Join("", l.Select(c => c.character.ToString())).TrimEnd()).ToArray();

            int emptyLinesAtBottom = 0;
            for (int i = actualLines.Length - 1; i >= 0; i--)
            {
                if (actualLines[i] != "")
                    break;
                emptyLinesAtBottom++;
            }

            actualLines = actualLines.AsSpan().Slice(0, actualLines.Length - emptyLinesAtBottom).ToArray();

            // Interleave ASCII colors into the actual lines
            for (int y = 0; y < actualLines.Length; y++)
            {
                var line = actualLines[y];
                var lineWithColors = "";
                for (int x = 0; x < line.Length; x++)
                {
                    lineWithColors += GetAnsiColor(GetTestRenderScreenPixel(x, y).character_color, GetTestRenderScreenPixel(x, y).background_color) + line[x] + ANSI_COLOR_RESET;
                }
                actualLines[y] = lineWithColors;
            }

            string joinedActualLines = string.Join("\n", actualLines) + ANSI_COLOR_RESET;

            Console.Error.WriteLine("Expected screen:");
            Console.Error.WriteLine(joinedExpectedLines);
            Console.Error.WriteLine("Actual screen:");
            Console.Error.WriteLine(joinedActualLines);

            Assert.Fail($"Expected screen doesn't match actual screen\nExpected screen (without colors):\n{joinedExpectedLines}\nActual screen (without colors):\n{joinedActualLines}");
        }
    }

    static protected void InitGUI()
    {
        InitGUI(new() { assert_behaviour = StbGui.STBG_ASSERT_BEHAVIOUR.EXCEPTION });
    }

    static protected void InitGUI(StbGui.stbg_init_options options)
    {
        StbGui.stbg_init(BuildExternalDependencies(), options);
        StbGui.stbg_set_screen_size(ScreenSizeWidth, ScreenSizeHeight);

        int fontId = StbGui.stbg_add_font("default_font");

        StbGui.stbg_init_default_theme(
            fontId,
            new() { size = 1, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE, color = StbGui.STBG_COLOR_WHITE }
        );
    }

    static protected void DestroyGui()
    {
        StbGui.stbg_destroy();
    }

    static protected void AssertGuiNotInitialized()
    {
        Assert.Null(StbGui.stbg_get_context().widgets);
    }

    static protected void AssertGuiInitialized()
    {
        Assert.NotNull(StbGui.stbg_get_context().widgets);
    }

    static protected void AssertHierarchyConsistency()
    {
        var root_widget_id = StbGui.stbg_get_context().root_widget_id;

        if (root_widget_id != StbGui.STBG_WIDGET_ID_NULL)
        {
            ValidateHierarchy(root_widget_id);
        }
    }

    static void ValidateHierarchy(int widget_id)
    {
        var widget = StbGui.stbg_get_widget_by_id(widget_id);

        // Validate that all next siblings point to the same parent, and that the next <-> previous chain is ok
        {
            var nextSiblingId = widget.hierarchy.next_sibling_id;
            var previousSiblingId = widget.id;

            while (nextSiblingId != StbGui.STBG_WIDGET_ID_NULL)
            {
                var nextSibling = StbGui.stbg_get_widget_by_id(nextSiblingId);

                Assert.Equal(widget.hierarchy.parent_id, nextSibling.hierarchy.parent_id);
                Assert.Equal(previousSiblingId, nextSibling.hierarchy.prev_sibling_id);

                nextSiblingId = nextSibling.hierarchy.next_sibling_id;
                previousSiblingId = nextSibling.id;
            }
        }

        // Validate that all previous siblings point to the same parent, and that the next <-> previous chain is ok
        {
            var nextSiblingId = widget.id;
            var previousSiblingId = widget.hierarchy.prev_sibling_id;

            while (previousSiblingId != StbGui.STBG_WIDGET_ID_NULL)
            {
                var previousSibling = StbGui.stbg_get_widget_by_id(previousSiblingId);

                Assert.Equal(widget.hierarchy.parent_id, previousSibling.hierarchy.parent_id);
                Assert.Equal(nextSiblingId, previousSibling.hierarchy.next_sibling_id);

                nextSiblingId = previousSibling.id;
                previousSiblingId = previousSibling.hierarchy.prev_sibling_id;
            }
        }

        // Validate all children
        {
            var childrenId = widget.hierarchy.first_children_id;
            while (childrenId != StbGui.STBG_WIDGET_ID_NULL)
            {
                var children = StbGui.stbg_get_widget_by_id(childrenId);

                Assert.Equal(widget.id, children.hierarchy.parent_id);

                ValidateHierarchy(childrenId);

                childrenId = children.hierarchy.next_sibling_id;
            }
        }
    }

    static protected void AssertWidgetSize(int widget_id, float width, float height)
    {
        Assert.Equal(
            new StbGui.stbg_size() { width = width, height = height },
            StbGui.stbg_get_widget_by_id(widget_id).computed_bounds.size
        );
    }

    static protected void AssertWidgetPosition(int widget_id, float x, float y)
    {
        Assert.Equal(
            new StbGui.stbg_position() { x = x, y = y },
            StbGui.stbg_get_widget_by_id(widget_id).computed_bounds.relative_position
        );
    }

    static protected void AssertWidgetGlobalRect(int widget_id, float x0, float y0, float x1, float y1)
    {
        Assert.Equal(
            new StbGui.stbg_rect() { top_left = new StbGui.stbg_position() { x = x0, y = y0 }, bottom_right = new StbGui.stbg_position() { x = x1, y = y1 } },
            StbGui.stbg_get_widget_by_id(widget_id).computed_bounds.global_rect
        );
    }
}