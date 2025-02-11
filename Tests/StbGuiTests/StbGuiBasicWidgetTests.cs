namespace StbSharp.Tests;

public class StbGuiBasicWidgetTests : StbGuiTestsBase
{
    [Fact]
    public void TestAddSingleButton()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello");
        }
        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestAddSameButtonTwiceInSameFrameShouldThrowAnException()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello");

            Assert.Throws<StbGui.StbgAssertException>(() => StbGui.stbg_button("Hello"));
        }
        StbGui.stbg_end_frame();

        Assert.Equal(1, StbGui.stbg_get_context().frame_stats.duplicated_widgets_ids);
    }

    [Fact]
    public void TestAddThreeDifferentButtons()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello1");
            StbGui.stbg_button("Hello2");
            StbGui.stbg_button("Hello3");
        }
        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestThreeDifferentButtonsShouldBeReusedInDifferentOrder()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello1");
            StbGui.stbg_button("Hello2");
            StbGui.stbg_button("Hello3");
        }
        StbGui.stbg_end_frame();

        int newWidgetsCount = StbGui.stbg_get_context().frame_stats.new_widgets;
        Assert.True(StbGui.stbg_get_context().frame_stats.new_widgets > 0);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello2");
            StbGui.stbg_button("Hello1");
            StbGui.stbg_button("Hello3");
        }
        StbGui.stbg_end_frame();

        Assert.Equal(0, StbGui.stbg_get_context().frame_stats.new_widgets);
        Assert.Equal(newWidgetsCount, StbGui.stbg_get_context().frame_stats.reused_widgets);
    }

    [Fact]
    public void TestNotReusedButtonsShouldBeDestroyed()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello1");
            StbGui.stbg_button("Hello2");
            StbGui.stbg_button("Hello3");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_begin_frame();
        {
            // Don't create buttons Hello1 / Hello2
            StbGui.stbg_button("Hello3");
        }
        StbGui.stbg_end_frame();

        Assert.Equal(0, StbGui.stbg_get_context().frame_stats.new_widgets);
        Assert.Equal(2, StbGui.stbg_get_context().frame_stats.destroyed_widgets);
    }

    [Fact]
    public void TestAddThreeDifferentButtons_HashCollisions()
    {
        // Force hash table size to 1 to ensure hash bucket collisions
        InitGUI(new() { hash_table_size = 1 });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello1");
            StbGui.stbg_button("Hello2");
            StbGui.stbg_button("Hello3");
        }
        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestNotReusedButtonsShouldBeDestroyed_HashCollisions()
    {
        // Force hash table size to 1 to ensure hash bucket collisions
        InitGUI(new() { hash_table_size = 1 });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello1");
            StbGui.stbg_button("Hello2");
            StbGui.stbg_button("Hello3");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_begin_frame();
        {
            // Don't create buttons Hello1 / Hello2
            StbGui.stbg_button("Hello3");
        }
        StbGui.stbg_end_frame();

        Assert.Equal(0, StbGui.stbg_get_context().frame_stats.new_widgets);
        Assert.Equal(2, StbGui.stbg_get_context().frame_stats.destroyed_widgets);
    }
}