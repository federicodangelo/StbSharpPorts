namespace StbSharp.Tests;

public class StbGuiBasicLoopTests : StbGuiTestsBase
{
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
}
