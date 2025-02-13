using System.Buffers;

namespace StbSharp.Tests;

[Collection("Sequential")]
public class StbGuiTestsBase : IDisposable
{
    protected const int ScreenSizeWidth = 120;
    protected const int ScreenSizeHeight = 80;

    static protected TestRenderScreen test_render_screen = new TestRenderScreen(ScreenSizeWidth, ScreenSizeHeight);

    static protected List<StbGui.stbg_render_command> render_commands_all = []; // All render commands
    static protected List<StbGui.stbg_render_command> render_commands = []; // Exclude begin and end frame commands

    public void Dispose()
    {
        AssertHierarchyConsistency();
        DestroyGui();
        render_commands_all = [];
        render_commands = [];
        test_render_screen.Clear();
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
            test_render_screen.ProcessRenderCommand(cmd);
        }
    }

    static protected readonly int COLOR_RED = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_RED);
    static protected readonly int COLOR_GREEN = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_GREEN);
    static protected readonly int COLOR_BLUE = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_BLUE);
    static protected readonly int COLOR_YELLOW = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_YELLOW);
    static protected readonly int COLOR_CYAN = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_CYAN);
    static protected readonly int COLOR_MAGENTA = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_MAGENTA);
    static protected readonly int COLOR_WHITE = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_WHITE);
    static protected readonly int COLOR_BLACK = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_BLACK);
    static protected readonly int COLOR_TRANSPARENT = (int)StbGui.stbg_color_to_uint(StbGui.STBG_COLOR_TRANSPARENT);

    static private TestRenderScreenPixel[][] ConvertExpectedLinesAndColorsToExpectedPixels(string[][] expectedLinesAndColors)
    {
        TestRenderScreenPixel[][] expectedPixels = new TestRenderScreenPixel[expectedLinesAndColors.Length][];
        for (int y = 0; y < expectedLinesAndColors.Length; y++)
        {
            var line = expectedLinesAndColors[y][0];
            var colors = expectedLinesAndColors[y][1];

            var pixels = new TestRenderScreenPixel[line.Length];

            for (int x = 0; x < line.Length; x++)
            {
                pixels[x].character = line[x];

                int acc_len = 0;
                int n = 0;
                while (n < colors.Length)
                {
                    var background_color_character = colors[n];
                    var color_character = colors[n + 1];
                    var lenDigits = 0;

                    while (
                        n + 2 + lenDigits < colors.Length &&
                        colors[n + 2 + lenDigits] >= '0' &&
                        colors[n + 2 + lenDigits] <= '9')
                    {
                        lenDigits++;
                    }

                    var len = int.Parse(colors.AsSpan().Slice(n + 2, lenDigits));

                    if (x < len + acc_len)
                    {
                        pixels[x].character_color = ColorsHelper.GetColorFromSingleCharacterNiceColorName(color_character);
                        pixels[x].background_color = ColorsHelper.GetColorFromSingleCharacterNiceColorName(background_color_character);
                        break;
                    }

                    acc_len += len;

                    n += 2 + lenDigits;
                }
            }

            expectedPixels[y] = pixels;
        }

        return expectedPixels;
    }

    static protected TestRenderScreenPixel[][] GetUsedScreenPixels()
    {
        // Find max X and Y that have characters
        int maxX = 0;
        int maxY = 0;

        for (int y = 0; y < test_render_screen.Height; y++)
        {
            for (int x = 0; x < test_render_screen.Width; x++)
            {
                if (test_render_screen.GetTestRenderScreenPixel(x, y).character != ' ')
                {
                    maxX = Math.Max(maxX, x + 1);
                    maxY = Math.Max(maxY, y + 1);
                }
            }
        }

        var usedScreenPixels = new TestRenderScreenPixel[maxY][];
        for (int y = 0; y < maxY; y++)
        {
            usedScreenPixels[y] = new TestRenderScreenPixel[maxX];

            for (int x = 0; x < maxX; x++)
            {
                usedScreenPixels[y][x] = test_render_screen.GetTestRenderScreenPixel(x, y);
            }
        }

        return usedScreenPixels;
    }

    static protected void AssertScreenEqual(string[][] expectedLinesAndColors)
    {
        var expectedPixels = ConvertExpectedLinesAndColorsToExpectedPixels(expectedLinesAndColors);
        var actualPixels = GetUsedScreenPixels();

        AssertScreenEqual(expectedPixels, actualPixels);
    }

    static protected void AssertScreenEqual(TestRenderScreenPixel[][] expectedPixels, TestRenderScreenPixel[][] actualPixels)
    {
        int maxY = Math.Max(actualPixels.Length, expectedPixels.Length);
        int maxX = Math.Max(actualPixels[0].Length, expectedPixels[0].Length);

        bool valid = true;
        int maxErrors = 5;

        for (int y = 0; y < maxY; y++)
        {
            for (int x = 0; x < maxX; x++)
            {
                var expectedPixel = y < expectedPixels.Length && x < expectedPixels[0].Length ? expectedPixels[y][x] : new TestRenderScreenPixel();
                var actualPixel = y < actualPixels.Length && x < actualPixels[0].Length ? actualPixels[y][x] : new TestRenderScreenPixel();

                if (expectedPixel != actualPixel)
                {
                    if (maxErrors > 0)
                    {
                        if (expectedPixel.character != actualPixel.character)
                            Console.Error.WriteLine($"[{x},{y}] Expected: '{expectedPixel.character}' Actual: '{actualPixel.character}'");
                        else if (expectedPixel.character_color != actualPixel.character_color)
                            Console.Error.WriteLine($"[{x},{y}] Expected: color '{ColorsHelper.GetNiceColorName(expectedPixel.character_color)}' Actual: color '{ColorsHelper.GetNiceColorName(actualPixel.character_color)}'");
                        else
                            Console.Error.WriteLine($"[{x},{y}] Expected: background color '{ColorsHelper.GetNiceColorName(expectedPixel.background_color)}' Actual: background color '{ColorsHelper.GetNiceColorName(actualPixel.background_color)}'");
                        maxErrors--;
                    }
                    else if (maxErrors == 0)
                    {
                        Console.Error.WriteLine("Too many pixel errors, truncated output");
                        maxErrors--;
                    }

                    valid = false;
                }
            }
        }

        if (valid)
        {
            return;
        }

        string[] expectedLines = TestRenderScreen.ConvertPixelsToStrings(expectedPixels, false);
        string[] expectedLinesWithColor = TestRenderScreen.ConvertPixelsToStrings(expectedPixels, true);
        string[] actualLines = TestRenderScreen.ConvertPixelsToStrings(actualPixels, false);
        string[] actualLinesWithColor = TestRenderScreen.ConvertPixelsToStrings(actualPixels, true);

        string joinedExpectedLines = string.Join("\n", expectedLines);
        string joinedActualLines = string.Join("\n", actualLines);

        Console.Error.WriteLine($"Expected screen: {ColorsHelper.GetAnsiColor(StbGui.STBG_COLOR_RED, StbGui.STBG_COLOR_TRANSPARENT)}[DON'T TRUST VISUAL STUDIO INTEGRATED TERMINAL COLORS{ColorsHelper.GetAnsiColorReset()}]");
        Console.Error.WriteLine(AddGuideLines(expectedLines, expectedLinesWithColor));
        Console.Error.WriteLine("Actual screen:");
        Console.Error.WriteLine(AddGuideLines(actualLines, actualLinesWithColor));

        string assertCode = BuildAssertPixelsCode(actualPixels);

        Console.Error.WriteLine($"{assertCode}");

        Assert.Fail($"Expected screen doesn't match actual screen\nExpected screen (without colors):\n{joinedExpectedLines}\nActual screen (without colors):\n{joinedActualLines}");
    }

    private static string AddGuideLines(string[] expectedLines, string[] expectedLinesWithColor)
    {
        int height = expectedLines.Length;
        int width = expectedLines[0].Length;
        string paddingLeft = "   ";

        var topGuide = paddingLeft;
        for (int x = 0; x < width; x++)
            topGuide += x.ToString()[x.ToString().Length - 1];


        string finalLines = topGuide + "\n";
        
        for (int y = 0; y < height; y++)
        {
            finalLines += string.Format("{0,-3}", y) + expectedLinesWithColor[y] + "\n";
        }

        return finalLines;
    }

    private static string BuildAssertPixelsCode(TestRenderScreenPixel[][] actualPixels)
    {
        string assert_actual_colors_code = "";
        string[] actualLines = TestRenderScreen.ConvertPixelsToStrings(actualPixels, false);

        for (int y = 0; y < actualPixels.Length; y++)
        {
            string last_background_color = "";
            string last_character_color = "";
            int last_count = 0;

            string line = "    [\"\"\"" + actualLines[y] + "\"\"\", \"";

            for (int x = 0; x < actualPixels[y].Length; x++)
            {
                string background_color = ColorsHelper.GetNiceColorNameSingleCharacter(actualPixels[y][x].background_color).ToString();
                string character_color = ColorsHelper.GetNiceColorNameSingleCharacter(actualPixels[y][x].character_color).ToString();

                if (background_color != last_background_color || character_color != last_character_color)
                {
                    if (last_count > 0)
                    {
                        line += $"{last_background_color}{last_character_color}{last_count}";
                    }
                    last_character_color = character_color;
                    last_background_color = background_color;
                    last_count = 1;
                }
                else
                {
                    last_count++;
                }
            }

            if (last_count > 0)
            {
                line += $"{last_background_color}{last_character_color}{last_count}";
            }

            line += "\"],\n";

            assert_actual_colors_code += line;
        }


        string assertCode =
            "--------------------------------------\n" +
            "-----------  ASSERTION CODE ----------\n" +
            "--------------------------------------\n" +
            "AssertScreenEqual([\n" +
            assert_actual_colors_code +
            "]);\n" +
            "--------------------------------------\n" +
            "--------------------------------------\n" +
            "--------------------------------------\n";
        return assertCode;
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