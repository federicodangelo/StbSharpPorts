namespace StbSharp.Tests;

public class StbGuiLabelTests : StbGuiTestsBase
{
    [Fact]
    public void TestRenderLabel()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_label("Label 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""Label 1""", "CW7"],
        ]);
    }
}