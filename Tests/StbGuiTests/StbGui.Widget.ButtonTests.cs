namespace StbSharp.Tests;

public class StbGuiButtonTests : StbGuiTestsBase
{

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
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);
    }

    [Fact]
    public void TestButtonHover()
    {
        InitGUI();

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);

        // Hover
        SetMousePosition(2, 2);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "BW12"],
            ["""|          |""", "BW1BT10BW1"],
            ["""| Button 1 |""", "BW1BT1BK8BT1BW1"],
            ["""|          |""", "BW1BT10BW1"],
            ["""\----------/""", "BW12"],
        ]);
    }

    [Fact]
    public void TestButtonHoverOff()
    {
        InitGUI();

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);

        // Hover
        SetMousePosition(2, 2);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "BW12"],
            ["""|          |""", "BW1BT10BW1"],
            ["""| Button 1 |""", "BW1BT1BK8BT1BW1"],
            ["""|          |""", "BW1BT10BW1"],
            ["""\----------/""", "BW12"],
        ]);

        // Hover off
        SetMousePositionInvalid();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);
    }

    [Fact]
    public void TestButtonPressed()
    {
        InitGUI();

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);

        // Press
        SetMousePosition(2, 2);
        SetMouseButton1(true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "GW12"],
            ["""|          |""", "GW1GT10GW1"],
            ["""| Button 1 |""", "GW1GT1GK8GT1GW1"],
            ["""|          |""", "GW1GT10GW1"],
            ["""\----------/""", "GW12"],
        ]);
    }

    [Fact]
    public void TestButtonClick()
    {
        InitGUI();

        // Normal render
        StbGui.stbg_begin_frame();
        {
            Assert.False(StbGui.stbg_button("Button 1"));
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);

        // Press
        SetMousePosition(2, 2);
        SetMouseButton1(true);

        StbGui.stbg_begin_frame();
        {
            Assert.False(StbGui.stbg_button("Button 1"));
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "GW12"],
            ["""|          |""", "GW1GT10GW1"],
            ["""| Button 1 |""", "GW1GT1GK8GT1GW1"],
            ["""|          |""", "GW1GT10GW1"],
            ["""\----------/""", "GW12"],
        ]);

        // Release (triggers click)
        SetMouseButton1(false);

        StbGui.stbg_begin_frame();
        {
            Assert.True(StbGui.stbg_button("Button 1"));
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "BW12"],
            ["""|          |""", "BW1BT10BW1"],
            ["""| Button 1 |""", "BW1BT1BK8BT1BW1"],
            ["""|          |""", "BW1BT10BW1"],
            ["""\----------/""", "BW12"],
        ]);


        // Frame after click (should not triggers click anymore)
        StbGui.stbg_begin_frame();
        {
            Assert.False(StbGui.stbg_button("Button 1"));
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "BW12"],
            ["""|          |""", "BW1BT10BW1"],
            ["""| Button 1 |""", "BW1BT1BK8BT1BW1"],
            ["""|          |""", "BW1BT10BW1"],
            ["""\----------/""", "BW12"],
        ]);
    }
}
