namespace StbSharp.Examples;

public class TestWindowsWindow : TestWindow
{
    public TestWindowsWindow(StbGuiAppBase appBase, StbGuiStringMemoryPool mp) : base("Test Window", appBase, mp)
    {
        open = true;
    }

    private bool show_title = true;
    private bool resizable = true;
    private bool show_scrollbars = true;
    private bool movable = true;

    public override void Render()
    {
        if (StbGui.stbg_begin_window(title, ref open,
                (show_title ? 0 : StbGui.STBG_WINDOW_OPTIONS.NO_TITLE) |
                (resizable ? 0 : StbGui.STBG_WINDOW_OPTIONS.NO_RESIZE) |
                (show_scrollbars ? 0 : StbGui.STBG_WINDOW_OPTIONS.NO_SCROLLBAR) |
                (movable ? 0 : StbGui.STBG_WINDOW_OPTIONS.NO_MOVE))
            )
        {
            StbGui.stbg_checkbox("Title", ref show_title);

            StbGui.stbg_checkbox("Resizable", ref resizable);

            StbGui.stbg_checkbox("Scrollbars", ref show_scrollbars);

            StbGui.stbg_checkbox("Movable", ref movable);

            for (int i = 0; i < 20; i++)
            {
                StbGui.stbg_button(mp.Build("Test Button ") + i);
            }

            StbGui.stbg_button("Test Button XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

            StbGui.stbg_end_window();
        }
    }
}