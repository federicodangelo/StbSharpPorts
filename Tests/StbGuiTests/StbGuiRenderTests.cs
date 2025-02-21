
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
                new StbGui.stbg_render_command() { type = StbGui.STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME, bounds ={ x1 = ScreenSizeWidth, y1 = ScreenSizeHeight }, background_color = StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR)},
                new StbGui.stbg_render_command() { type = StbGui.STBG_RENDER_COMMAND_TYPE.END_FRAME}
            ],
            render_commands_all
        );

        RenderCommandsToTestScreen();

        Assert.Equal(test_render_screen.GetTestRenderScreenPixel(0, 0).background_color, StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR));
        Assert.Equal(test_render_screen.GetTestRenderScreenPixel(ScreenSizeWidth - 1, ScreenSizeHeight - 1).background_color, StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR));
    }
}
