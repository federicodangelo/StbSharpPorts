
namespace StbSharp.Tests;

public class StbGuiScrollbarTests : StbGuiTestsBase
{
    [Fact]
    public void TestRenderScrollbarHorizontal()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            float value = 0;
            StbGui.stbg_scrollbar("SB1", StbGui.STBG_SCROLLBAR_DIRECTION.HORIZONTAL, ref value, 0, 100);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""<        >""", "BW1BT2CT6BW1"],
        ]);
    }

    [Fact]
    public void TestRenderScrollbarHorizontalHalfValue()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            float value = 50;
            StbGui.stbg_scrollbar("SB1", StbGui.STBG_SCROLLBAR_DIRECTION.HORIZONTAL, ref value, 0, 100);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""<        >""", "BW1CT3BT2CT3BW1"],
        ]);
    }

    [Fact]
    public void TestRenderScrollbarHorizontalFullValue()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            float value = 100;
            StbGui.stbg_scrollbar("SB1", StbGui.STBG_SCROLLBAR_DIRECTION.HORIZONTAL, ref value, 0, 100);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""<        >""", "BW1CT6BT2BW1"],
        ]);
    }

    [Fact]
    public void TestRenderScrollbarVertical()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            float value = 0;
            StbGui.stbg_scrollbar("SB1", StbGui.STBG_SCROLLBAR_DIRECTION.VERTICAL, ref value, 0, 100);
            StbGui.stbg_set_last_widget_size(0, 10);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""^""", "BW1"],
            [""" """, "BT1"],
            [""" """, "BT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            ["""v""", "BW1"],
        ]);
    }

    [Fact]
    public void TestRenderScrollbarVerticalHalfValue()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            float value = 50;
            StbGui.stbg_scrollbar("SB1", StbGui.STBG_SCROLLBAR_DIRECTION.VERTICAL, ref value, 0, 100);
            StbGui.stbg_set_last_widget_size(0, 10);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
             ["""^""", "BW1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "BT1"],
            [""" """, "BT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            ["""v""", "BW1"],
        ]);
    }

    [Fact]
    public void TestRenderScrollbarVerticalFullValue()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            float value = 100;
            StbGui.stbg_scrollbar("SB1", StbGui.STBG_SCROLLBAR_DIRECTION.VERTICAL, ref value, 0, 100);
            StbGui.stbg_set_last_widget_size(0, 10);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""^""", "BW1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "CT1"],
            [""" """, "BT1"],
            [""" """, "BT1"],
            ["""v""", "BW1"],
        ]);
    }
}
