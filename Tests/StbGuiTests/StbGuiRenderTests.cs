
namespace StbSharp.Tests;

public class StbGuiRenderTests : StbGuiTestsBase
{
    [Fact]
    public void TestRenderEmpty()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        Assert.Equal(
            [
                new StbGui.stbg_render_command() { type = StbGui.STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME, bounds ={ bottom_right = { x = ScreenSizeWidth, y = ScreenSizeHeight }}, background_color = StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR)},
                new StbGui.stbg_render_command() { type = StbGui.STBG_RENDER_COMMAND_TYPE.END_FRAME}
            ],
            render_commands_all
        );

        RenderCommandsToTestScreen();

        Assert.Equal(GetTestRenderScreenPixel(0, 0).background_color, StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR));
        Assert.Equal(GetTestRenderScreenPixel(ScreenSizeWidth - 1, ScreenSizeHeight - 1).background_color, StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR));
    }

    [Fact]
    public void TestRenderButton()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            """/----------\""",
            """|          |""",
            """| Button 1 |""",
            """|          |""",
            """\----------/""",
        ], [
            [[COLOR_CYAN, COLOR_BLUE, 12]],
            [[COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 10],[COLOR_CYAN, COLOR_BLUE, 1]],
            [[COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_WHITE, 8], [COLOR_CYAN, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1]],
            [[COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 10],[COLOR_CYAN, COLOR_BLUE, 1]],
            [[COLOR_CYAN, COLOR_BLUE, 12]],
        ]);
    }

    [Fact]
    public void TestRenderWindow()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 4);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 14);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            """/----------------\""",
            """|Window 1        |""",
            """\----------------/""",
            """|                |""",
            """|                |""",
            """|                |""",
            """|                |""",
            """|                |""",
            """|                |""",
            """|                |""",
            """\----------------/""",
        ], [
            [[COLOR_MAGENTA, COLOR_WHITE, 18]],
            [[COLOR_MAGENTA, COLOR_WHITE, 9], [COLOR_MAGENTA, COLOR_TRANSPARENT, 8], [COLOR_MAGENTA, COLOR_WHITE, 1]],
            [[COLOR_MAGENTA, COLOR_WHITE, 18]],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1]],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1]],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1]],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1]],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1]],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1]],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1]],
            [[COLOR_BLUE, COLOR_WHITE, 18]],
        ]);
    }

    [Fact]
    public void TestRenderWindowTwoWindows()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 7);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 14);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
            }
            StbGui.stbg_end_window();
            StbGui.stbg_begin_window("Window 2");
            {
            }
            StbGui.stbg_end_window();

            StbGui.stbg_move_window(StbGui.stbg_get_last_widget_id(), 20, 5);

        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            """/----------------\""",
            """|Window 1        |""",
            """\----------------/""",
            """|                |""",
            """|                |""",
            """|                |  /----------------\""",
            """|                |  |Window 2        |""",
            """|                |  \----------------/""",
            """|                |  |                |""",
            """|                |  |                |""",
            """|                |  |                |""",
            """|                |  |                |""",
            """|                |  |                |""",
            """\----------------/  |                |""",
            """                    |                |""",
            """                    |                |""",
            """                    |                |""",
            """                    |                |""",
            """                    \----------------/""",
        ], [
             [[COLOR_MAGENTA, COLOR_WHITE, 18],],
            [[COLOR_MAGENTA, COLOR_WHITE, 9], [COLOR_MAGENTA, COLOR_TRANSPARENT, 8], [COLOR_MAGENTA, COLOR_WHITE, 1],],
            [[COLOR_MAGENTA, COLOR_WHITE, 18],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_MAGENTA, COLOR_WHITE, 18],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_MAGENTA, COLOR_WHITE, 9], [COLOR_MAGENTA, COLOR_TRANSPARENT, 8], [COLOR_MAGENTA, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_MAGENTA, COLOR_WHITE, 18],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 18], [COLOR_CYAN, COLOR_TRANSPARENT, 2], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_CYAN, COLOR_TRANSPARENT, 20], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_CYAN, COLOR_TRANSPARENT, 20], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_CYAN, COLOR_TRANSPARENT, 20], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_CYAN, COLOR_TRANSPARENT, 20], [COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 16], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_CYAN, COLOR_TRANSPARENT, 20], [COLOR_BLUE, COLOR_WHITE, 18],],
        ]);
    }

    [Fact]
    public void TestRenderWindowWithTwoButtons()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 10);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 20);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_button("Button 1");
                StbGui.stbg_button("Button 2");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            """/----------------------\""",
            """|Window 1              |""",
            """\----------------------/""",
            """|                      |""",
            """|                      |""",
            """| /----------\         |""",
            """| |          |         |""",
            """| | Button 1 |         |""",
            """| |          |         |""",
            """| \----------/         |""",
            """| /----------\         |""",
            """| |          |         |""",
            """| | Button 2 |         |""",
            """| |          |         |""",
            """| \----------/         |""",
            """|                      |""",
            """\----------------------/""",
        ], [
            [[COLOR_MAGENTA, COLOR_WHITE, 24],],
            [[COLOR_MAGENTA, COLOR_WHITE, 9], [COLOR_MAGENTA, COLOR_TRANSPARENT, 14], [COLOR_MAGENTA, COLOR_WHITE, 1],],
            [[COLOR_MAGENTA, COLOR_WHITE, 24],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 22], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 22], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 12], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 10], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_WHITE, 8], [COLOR_CYAN, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 10], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 12], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 12], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 10], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_WHITE, 8], [COLOR_CYAN, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_CYAN, COLOR_TRANSPARENT, 10], [COLOR_CYAN, COLOR_BLUE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 1], [COLOR_CYAN, COLOR_BLUE, 12], [COLOR_BLUE, COLOR_TRANSPARENT, 9], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 1], [COLOR_BLUE, COLOR_TRANSPARENT, 22], [COLOR_BLUE, COLOR_WHITE, 1],],
            [[COLOR_BLUE, COLOR_WHITE, 24],],
        ]);
    }
}
