namespace StbSharp.Tests;

public class StbGuiTests : StbGuiTestsBase, IDisposable
{
    public void Dispose()
    {
        AssertHierarchyConsistency();
        DestroyGui();
    }

    [Fact]
    public void TestInit()
    {
        AssertGuiNotInitialized();

        InitGUI();

        AssertGuiInitialized();
    }

    [Fact]
    public void TestDestroy()
    {
        AssertGuiNotInitialized();

        InitGUI();

        AssertGuiInitialized();

        DestroyGui();

        AssertGuiNotInitialized();
    }

    [Fact]
    public void TestChainOfFreeWidgets()
    {
        InitGUI();

        int freeChildrenId = StbGui.stbg_get_context().first_free_widget_id;

        var widgets = StbGui.stbg_get_context().widgets;

        int freeChildrenCount = 0;

        while (freeChildrenId != StbGui.STBG_WIDGET_ID_NULL)
        {
            freeChildrenId = widgets[freeChildrenId].hierarchy.next_sibling_id;
            freeChildrenCount++;
        }

        // We lose 1 children because we don't use the slot 0
        Assert.Equal(widgets.Length - 1, freeChildrenCount);
    }

    [Fact]
    public void TestBeginAndEndFrame()
    {
        InitGUI();

        StbGui.stbg_begin_frame();

        Assert.True(StbGui.stbg_get_context().inside_frame);

        StbGui.stbg_end_frame();

        Assert.False(StbGui.stbg_get_context().inside_frame);
    }

    [Fact]
    public void TestMultipleBeginAndEndFrame()
    {
        InitGUI();

        for (int i = 0; i < 10; i++)
        {
            StbGui.stbg_begin_frame();

            StbGui.stbg_end_frame();
        }
    }

    [Fact]
    public void TestNewAndReusedWidgets()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        StbGui.stbg_end_frame();

        Assert.True(StbGui.stbg_get_context().frame_stats.reused_widgets == 0, "There should be no reused widgets in the first frame");
        Assert.True(StbGui.stbg_get_context().frame_stats.new_widgets > 0, "There should be at least one new widget in the first frame (the root container)");

        StbGui.stbg_begin_frame();
        StbGui.stbg_end_frame();

        Assert.True(StbGui.stbg_get_context().frame_stats.new_widgets == 0, "There should be no new widgets after the first frame");
        Assert.True(StbGui.stbg_get_context().frame_stats.reused_widgets > 0, "There should be at least one reused widget after the first frame (the root container)");
    }

    [Fact]
    public void TestAddSingleButton()
    {
        InitGUI();

        StbGui.stbg_begin_frame();

        StbGui.stbg_button("Hello");

        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestAddSameButtonTwiceInSameFrameShouldThrowAnException()
    {
        InitGUI();

        StbGui.stbg_begin_frame();

        StbGui.stbg_button("Hello");

        Assert.Throws<Exception>(() => StbGui.stbg_button("Hello"));

        Assert.Equal(1, StbGui.stbg_get_context().frame_stats.duplicated_widgets_ids);

        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestAddThreeDifferentButtons()
    {
        InitGUI();

        StbGui.stbg_begin_frame();

        StbGui.stbg_button("Hello1");
        StbGui.stbg_button("Hello2");
        StbGui.stbg_button("Hello3");

        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestThreeDifferentButtonsShouldBeReusedInDifferentOrder()
    {
        InitGUI();

        StbGui.stbg_begin_frame();

        StbGui.stbg_button("Hello1");
        StbGui.stbg_button("Hello2");
        StbGui.stbg_button("Hello3");

        StbGui.stbg_end_frame();

        int newWidgetsCount = StbGui.stbg_get_context().frame_stats.new_widgets;
        Assert.True(StbGui.stbg_get_context().frame_stats.new_widgets > 0);

        StbGui.stbg_begin_frame();

        StbGui.stbg_button("Hello2");
        StbGui.stbg_button("Hello1");
        StbGui.stbg_button("Hello3");

        StbGui.stbg_end_frame();
        
        Assert.Equal(0, StbGui.stbg_get_context().frame_stats.new_widgets);
        Assert.Equal(newWidgetsCount, StbGui.stbg_get_context().frame_stats.reused_widgets);
    }

    [Fact]
    public void TestNotReusedButtonsShouldBeDestroyed()
    {
        InitGUI();

        StbGui.stbg_begin_frame();

        StbGui.stbg_button("Hello1");
        StbGui.stbg_button("Hello2");
        StbGui.stbg_button("Hello3");

        StbGui.stbg_end_frame();

        StbGui.stbg_begin_frame();

        // Don't create buttons Hello1 / Hello2
        StbGui.stbg_button("Hello3");

        StbGui.stbg_end_frame();
        
        Assert.Equal(0, StbGui.stbg_get_context().frame_stats.new_widgets);
        Assert.Equal(2, StbGui.stbg_get_context().frame_stats.destroyed_widgets);
    }
}