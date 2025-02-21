namespace StbSharp.Examples;

using System.Buffers.Text;
using StbSharp;

public class SDLApp : SDLAppBase
{
    public SDLApp() : base(new SdlAppOptions() { WindowName = "Example App" })
    {

    }

    private bool showButton3 = true;
    private float scrollbar_value = 50;
    private int scrollbar_value_int = 50;

    protected override void OnRenderStbGui()
    {
        StbGui.stbg_label("FPS: " + Metrics.Fps);
        StbGui.stbg_label("Allocated Bytes Delta: " + Metrics.LastFrameAllocatedBytes);

        StbGui.stbg_begin_window("Window 1");
        {
            if (StbGui.stbg_get_last_widget_is_new())
                StbGui.stbg_set_last_widget_position(100, 100);

            StbGui.stbg_button("Button 1");
            if (StbGui.stbg_button("Toggle Button 3"))
                showButton3 = !showButton3;

            if (showButton3)
                StbGui.stbg_button("Button 3");
        }
        StbGui.stbg_end_window();

        StbGui.stbg_begin_window("Window 2 with a REALLY REALLY REALLY REALLY REALLY REALLY REALLY REALLY REALLY REALLY long title");
        {
            if (StbGui.stbg_get_last_widget_is_new())
                StbGui.stbg_set_last_widget_position(200, 150);

            StbGui.stbg_begin_container("concon", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL, StbGui.stbg_build_constrains(0, 0, 400, float.MaxValue));
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
                    StbGui.stbg_label("Scrollbar Value: " + scrollbar_value);

                    StbGui.stbg_label("Scrollbar Value Int: " + scrollbar_value_int);
                }
                StbGui.stbg_end_container();
            }
            StbGui.stbg_end_container();
        }
        StbGui.stbg_end_window();

        StbGui.stbg_begin_window("Window 4");
        {
            if (StbGui.stbg_get_last_widget_is_new())
                StbGui.stbg_set_last_widget_position(300, 250);

            for (int i = 0; i < 20; i++)
            {
                StbGui.stbg_button("Test Button " + i);
            }

            StbGui.stbg_button("Test Button XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
        }
        StbGui.stbg_end_window();

        StbGui.stbg_label("This is the debug window!");
    }
}
