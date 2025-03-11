
using StbSharp;

public class TestScrollbarsWindow : TestWindow
{
    public TestScrollbarsWindow(StbGuiAppBase appBase, StbGuiStringMemoryPool mp) : base("Test Scrollbars", appBase, mp)
    {
    }

    private float scrollbar_value = 50;
    private int scrollbar_value_int = 50;

    public override void Render()
    {
        if (StbGui.stbg_begin_window(title, ref open))
        {
            StbGui.stbg_begin_container("con4444", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL, StbGui.stbg_build_constrains(0, 0, 400, float.MaxValue));
            {
                StbGui.stbg_scrollbar("horizontal-sb", StbGui.STBG_SCROLLBAR_DIRECTION.HORIZONTAL, ref scrollbar_value, 0, 100);
                StbGui.stbg_set_last_widget_size(200, 0);
                StbGui.stbg_scrollbar("horizontal-sb2", StbGui.STBG_SCROLLBAR_DIRECTION.HORIZONTAL, ref scrollbar_value, 0, 100);
                StbGui.stbg_set_last_widget_size(200, 0);
            }
            StbGui.stbg_end_container();

            StbGui.stbg_begin_container("con3332", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL, StbGui.stbg_build_constrains(0, 0, float.MaxValue, 200));
            {
                StbGui.stbg_scrollbar("vertical-sb", StbGui.STBG_SCROLLBAR_DIRECTION.VERTICAL, ref scrollbar_value_int, 0, 100);
                StbGui.stbg_set_last_widget_size(0, 200);

                StbGui.stbg_begin_container("con3332", StbGui.STBG_CHILDREN_LAYOUT.VERTICAL);
                {
                    StbGui.stbg_label(mp.Build("Scrollbar Value: ") + scrollbar_value);

                    StbGui.stbg_label(mp.Build("Scrollbar Value Int: ") + scrollbar_value_int);
                }
                StbGui.stbg_end_container();
            }
            StbGui.stbg_end_container();

            StbGui.stbg_end_window();
        }
    }
}
