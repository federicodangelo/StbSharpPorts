
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

        Assert.Equal(
            [
                new StbGui.stbg_render_command() { type = StbGui.STBG_RENDER_COMMAND_TYPE.BORDER, size = 1, bounds = { top_left = { x = 0, y = 0}, bottom_right = { x = 12, y = 5}}, color = StbGui.STBG_COLOR_BLUE, background_color = StbGui.STBG_COLOR_CYAN },
                new StbGui.stbg_render_command() { type = StbGui.STBG_RENDER_COMMAND_TYPE.TEXT, bounds = { top_left = { x = 2, y = 2}, bottom_right = { x = 10, y = 3}}, text = { text = "Button 1".AsMemory(), font_id = 1, style = { size = 1, color = StbGui.STBG_COLOR_WHITE} } },
            ],
            render_commands
        );

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
}
