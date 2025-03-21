using System.Diagnostics.CodeAnalysis;

namespace StbSharp.Tests;

[Collection("Sequential")]
[ExcludeFromCodeCoverage]
public class StbGuiTestsBase : IDisposable
{
    protected const int ScreenSizeWidth = 120;
    protected const int ScreenSizeHeight = 80;

    protected TestRenderScreen test_render_screen = new TestRenderScreen(ScreenSizeWidth, ScreenSizeHeight);

    public void Dispose()
    {
        AssertHierarchyConsistency();
        DestroyGui();
        test_render_screen.Clear();
    }

    private StbGui.stbg_external_dependencies BuildExternalDependencies()
    {
        return new StbGui.stbg_external_dependencies()
        {
            render_adapter = new TestScreenRenderAdapter(test_render_screen),
            copy_text_to_clipboard = (text) => { },
            get_clipboard_text = () => "",
            set_input_method_editor = (info) => { },
            get_time_milliseconds = () => 0,
            get_performance_counter = () => 0,
            get_performance_counter_frequency = () => 1,
        };
    }

    protected void RenderCommandsToTestScreen()
    {
    }

    private TestRenderScreenPixel[][] ConvertExpectedLinesAndColorsToExpectedPixels(string[][] expectedLinesAndColors)
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

    protected TestRenderScreenPixel[][] GetUsedScreenPixels()
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

    protected void AssertScreenEqual(string[][] expectedLinesAndColors)
    {
        var expectedPixels = ConvertExpectedLinesAndColorsToExpectedPixels(expectedLinesAndColors);
        var actualPixels = GetUsedScreenPixels();

        AssertScreenEqual(expectedPixels, actualPixels);
    }

    protected void AssertScreenEqual(TestRenderScreenPixel[][] expectedPixels, TestRenderScreenPixel[][] actualPixels)
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

        Assert.Fail($"Expected screen doesn't match actual screen\nExpected screen (without colors):\n{joinedExpectedLines}\nActual screen (without colors):\n{joinedActualLines}\nAssert Code:\n{assertCode}");
    }

    private string AddGuideLines(string[] expectedLines, string[] expectedLinesWithColor)
    {
        int height = expectedLines.Length;
        int width = expectedLines[0].Length;
        string paddingLeft = "   ";

        var topGuide0 = paddingLeft;
        var topGuide1 = paddingLeft;
        for (int x = 0; x < width; x++)
        {
            topGuide0 += x % 10 == 0 ? x.ToString()[0] : " ";
            topGuide1 += x.ToString()[x.ToString().Length - 1];
        }


        string finalLines = topGuide0 + "\n" + topGuide1 + "\n";

        for (int y = 0; y < height; y++)
        {
            finalLines += string.Format("{0,2} ", y) + expectedLinesWithColor[y] + "\n";
        }

        return finalLines;
    }

    private string BuildAssertPixelsCode(TestRenderScreenPixel[][] actualPixels)
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

    protected void InitGUI()
    {
        InitGUI(new() { assert_behavior = StbGui.STBG_ASSERT_BEHAVIOR.EXCEPTION, dont_nest_non_window_root_elements_into_debug_window = true });
    }

    protected void InitGUI(StbGui.stbg_init_options options)
    {
        StbGui.stbg_init(BuildExternalDependencies(), options);
        StbGui.stbg_set_screen_size(ScreenSizeWidth, ScreenSizeHeight);

        int fontId = StbGui.stbg_add_font("default_font");

        InitTestTheme(fontId, new() { size = 1, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE, color = StbGui.STBG_COLOR_WHITE });
    }

    void InitTestTheme(int font_id, StbGui.stbg_font_style font_style)
    {
        StbGui.stbg_init_default_theme(
            font_id, font_style
        );

        // ROOT
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR, StbGui.STBG_COLOR_CYAN);

        // BUTTON
        var buttonBorder = 1.0f;
        var buttonPaddingTopBottom = MathF.Ceiling(font_style.size / 2);
        var buttonPaddingLeftRight = MathF.Ceiling(font_style.size / 2);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, buttonBorder);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_TOP, buttonPaddingTopBottom);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM, buttonPaddingTopBottom);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT, buttonPaddingLeftRight);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT, buttonPaddingLeftRight);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_BORDER_COLOR, StbGui.STBG_COLOR_BLUE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_BACKGROUND_COLOR, StbGui.STBG_COLOR_CYAN);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_TEXT_COLOR, StbGui.STBG_COLOR_WHITE);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_HOVERED_BORDER_COLOR, StbGui.STBG_COLOR_WHITE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_HOVERED_BACKGROUND_COLOR, StbGui.STBG_COLOR_BLUE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_HOVERED_TEXT_COLOR, StbGui.STBG_COLOR_BLACK);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PRESSED_BORDER_COLOR, StbGui.STBG_COLOR_WHITE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PRESSED_BACKGROUND_COLOR, StbGui.STBG_COLOR_GREEN);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PRESSED_TEXT_COLOR, StbGui.STBG_COLOR_BLACK);

        // WINDOW
        var windowBorder = 1.0f;
        var windowTitleHeight = MathF.Ceiling(font_style.size);
        var windowTitlePadding = MathF.Ceiling(font_style.size / 4);
        var windowChildrenPadding = MathF.Ceiling(font_style.size / 2);
        var windowDefaultWidth = 34;
        var windowDefaultHeight = 22;
        var windowChildrenSpacing = 0;

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, windowBorder);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, windowTitleHeight);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, windowTitlePadding);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, windowTitlePadding);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT, windowTitlePadding);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT, windowTitlePadding);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP, windowChildrenPadding);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_BOTTOM, windowChildrenPadding);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_LEFT, windowChildrenPadding);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_RIGHT, windowChildrenPadding);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_COLOR, StbGui.STBG_COLOR_WHITE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BACKGROUND_COLOR, StbGui.STBG_COLOR_BLUE);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_TEXT_COLOR, StbGui.STBG_COLOR_WHITE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_BACKGROUND_COLOR, StbGui.STBG_COLOR_MAGENTA);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_TEXT_COLOR, StbGui.STBG_COLOR_MAGENTA);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR, StbGui.STBG_COLOR_WHITE);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, false);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING, windowChildrenSpacing);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CENTER_NEW_WINDOWS, false);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, windowDefaultWidth);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, windowDefaultHeight);

        // DEBUG WINDOW
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_TEXT_COLOR, StbGui.STBG_COLOR_WHITE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_BACKGROUND_COLOR, StbGui.STBG_COLOR_RED);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_TEXT_COLOR, StbGui.STBG_COLOR_WHITE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR, StbGui.STBG_COLOR_RED);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.DEBUG_WINDOW_POSITION_X, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.DEBUG_WINDOW_POSITION_Y, 0);

        // LABEL
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.LABEL_TEXT_COLOR, StbGui.STBG_COLOR_WHITE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.LABEL_BACKGROUND_COLOR, StbGui.STBG_COLOR_CYAN);

        // SCROLLBAR
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.LABEL_TEXT_COLOR, StbGui.STBG_COLOR_WHITE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.LABEL_BACKGROUND_COLOR, StbGui.STBG_COLOR_CYAN);

        // SCROLLBAR
        var scrollbarSize = 1;
        var scrollbarButtonSize = 1;
        var scrollbarThumbSize = 2;

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_SIZE, scrollbarSize);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE, scrollbarButtonSize);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE, scrollbarThumbSize);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_BACKGROUND_COLOR, StbGui.STBG_COLOR_CYAN);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_THUMB_COLOR, StbGui.STBG_COLOR_BLUE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_BACKGROUND_COLOR, StbGui.STBG_COLOR_BLUE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_COLOR, StbGui.STBG_COLOR_WHITE);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_THUMB_HOVERED_COLOR, StbGui.STBG_COLOR_MAGENTA);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_HOVERED_BACKGROUND_COLOR, StbGui.STBG_COLOR_BLUE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_HOVERED_COLOR, StbGui.STBG_COLOR_RED);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_THUMB_PRESSED_COLOR, StbGui.STBG_COLOR_MAGENTA);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_PRESSED_BACKGROUND_COLOR, StbGui.STBG_COLOR_BLUE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_PRESSED_COLOR, StbGui.STBG_COLOR_RED);

        // TEXTBOX
        var textboxPaddingTopBottom = MathF.Ceiling(font_style.size / 4);
        var textboxPaddingLeftRight = MathF.Ceiling(font_style.size / 4);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP, textboxPaddingTopBottom);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM, textboxPaddingTopBottom);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT, textboxPaddingLeftRight);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT, textboxPaddingLeftRight);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_BACKGROUND_COLOR, StbGui.STBG_COLOR_CYAN);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_BORDER_COLOR, StbGui.STBG_COLOR_BLUE);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_BORDER_SIZE, 1);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_TEXT_COLOR, StbGui.STBG_COLOR_BLACK);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_CURSOR_COLOR, StbGui.STBG_COLOR_YELLOW);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_CURSOR_WIDTH, 1);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_CURSOR_HEIGHT, font_style.size);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.TEXTBOX_CURSOR_BLINKING_RATE, 500);
    }

    protected void SetMousePosition(float x, float y)
    {
        StbGui.stbg_add_user_input_event_mouse_position(x, y);
    }

    protected void SetMousePositionInvalid()
    {
        StbGui.stbg_add_user_input_event_mouse_position(0, 0, false);
    }

    protected void SetMouseButton1(bool pressed)
    {
        StbGui.stbg_add_user_input_event_mouse_button(1, pressed);
    }

    protected void SetMouseScrollWheelAmount(float x, float y)
    {
        StbGui.stbg_add_user_input_event_mouse_wheel(x, y);
    }

    protected void PressKey(StbGui.STBG_KEYBOARD_KEY key, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS modifiers, bool pressed)
    {
        StbGui.stbg_add_user_input_event_keyboard_key(key, modifiers, pressed);
    }

    protected void PressKeyCharacter(char c, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS modifiers, bool pressed)
    {
        StbGui.stbg_add_user_input_event_keyboard_key_character(c, modifiers, pressed);
    }

    protected void DestroyGui()
    {
        StbGui.stbg_destroy();
    }

    protected void AssertGuiNotInitialized()
    {
        Assert.Null(StbGui.stbg_get_context().widgets);
    }

    protected void AssertGuiInitialized()
    {
        Assert.NotNull(StbGui.stbg_get_context().widgets);
    }

    protected void AssertHierarchyConsistency()
    {
        var root_widget_id = StbGui.stbg_get_context().root_widget_id;

        if (root_widget_id != StbGui.STBG_WIDGET_ID_NULL)
        {
            ValidateHierarchy(root_widget_id);
        }
    }

    private void ValidateHierarchy(int widget_id)
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

    protected void AssertWidgetSize(int widget_id, float width, float height)
    {
        Assert.Equal(
            new StbGui.stbg_size() { width = width, height = height },
            StbGui.stbg_get_widget_by_id(widget_id).properties.computed_bounds.size
        );
    }

    protected void AssertWidgetPosition(int widget_id, float x, float y)
    {
        Assert.Equal(
            StbGui.stbg_build_position(x, y),
            StbGui.stbg_get_widget_by_id(widget_id).properties.computed_bounds.position
        );
    }

    protected void AssertWidgetGlobalRect(int widget_id, float x0, float y0, float x1, float y1)
    {
        Assert.Equal(
            StbGui.stbg_build_rect(x0, y0, x1, y1),
            StbGui.stbg_get_widget_by_id(widget_id).properties.computed_bounds.global_rect
        );
    }
}
