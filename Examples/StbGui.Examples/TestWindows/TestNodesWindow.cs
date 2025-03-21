using StbSharp;

public class TestNodesWindow : TestWindow
{
    public TestNodesWindow(StbGuiAppBase appBase, StbGuiStringMemoryPool mp) : base("Test Nodes", appBase, mp)
    {
        open = true;
    }

    public override void Render()
    {
        if (StbGui.stbg_begin_window(title, ref open, StbGui.STBG_WINDOW_OPTIONS.NO_CHILDREN_PADDING))
        {
            StbGui.stbg_set_last_widget_size_if_new(800, 600);
            StbGui.stbg_set_last_widget_position_if_new(100, 100);

            StbGui.stbg_begin_nodes_container("nodes_container");
            {
                StbGui.stbg_end_nodes_container();
            }

            StbGui.stbg_end_window();
        }
    }
}
