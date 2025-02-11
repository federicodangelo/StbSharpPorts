namespace StbSharp.Tests;

public class StbGuiWindowTests : StbGuiTestsBase
{
    [Fact]
    public void TestBeginAndEndWindow()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestUnbalancedBeginAndEndWindowThrowsException()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
        }
        Assert.Throws<StbGui.StbgAssertException>(() => StbGui.stbg_end_frame());
    }

    [Fact]
    public void TestDuplicatedButtonIdsInDifferentWindows()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            if (StbGui.stbg_begin_window("Window 1"))
            {
                StbGui.stbg_button("Button");
            }
            StbGui.stbg_end_window();

            if (StbGui.stbg_begin_window("Window 2"))
            {
                StbGui.stbg_button("Button");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestWindowDissapearsInSecondFrame()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_button("Button");
            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {
                StbGui.stbg_button("Button");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_begin_frame();
        {
            // Skip Window 1
            StbGui.stbg_begin_window("Window 2");
            {
                StbGui.stbg_button("Button");
                StbGui.stbg_end_window();
            }
        }
        StbGui.stbg_end_frame();

        // Both Window1 and the contained button should have been destroyed
        Assert.Equal(2, StbGui.stbg_get_context().frame_stats.destroyed_widgets);
    }
}