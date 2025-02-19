namespace StbSharp.Examples;

using StbSharp;

public class SDLApp : SDLAppBase
{
    private bool showButton3 = true;

    public SDLApp() : base(new SdlAppOptions() { WindowName = "Example App" })
    {

    }

    private float scrollbar_value = 50;

    protected override void OnRenderStbGui()
    {
        StbGui.stbg_label("FPS: " + Fps);

        StbGui.stbg_begin_window("Window 1");
        {
            if (StbGui.stbg_get_last_widget_is_new())
                StbGui.stbg_set_widget_position(StbGui.stbg_get_last_widget_id(), 100, 50);

            StbGui.stbg_button("Button 1");
            if (StbGui.stbg_button("Toggle Button 3"))
                showButton3 = !showButton3;

            if (showButton3)
                StbGui.stbg_button("Button 3");
        }
        StbGui.stbg_end_window();

        StbGui.stbg_begin_window("Window 2");
        {
            if (StbGui.stbg_get_last_widget_is_new())
                StbGui.stbg_set_widget_position(StbGui.stbg_get_last_widget_id(), 200, 150);

            StbGui.stbg_begin_container("concon", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL, StbGui.stbg_build_constrains(0, 0, 400, float.MaxValue));
            {
                StbGui.stbg_begin_container("con", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL, StbGui.stbg_build_constrains(0, 0, 200, float.MaxValue));
                {
                    StbGui.stbg_scrollbar("horizontal-sb", StbGui.STBG_SCROLLBAR_DIRECTION.HORIZONTAL, ref scrollbar_value, 0, 100);
                }
                StbGui.stbg_end_container();
                StbGui.stbg_begin_container("con2", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL, StbGui.stbg_build_constrains(0, 0, 200, float.MaxValue));
                {
                    StbGui.stbg_scrollbar("horizontal-sb2", StbGui.STBG_SCROLLBAR_DIRECTION.HORIZONTAL, ref scrollbar_value, 0, 100);
                }
                StbGui.stbg_end_container();
            }
            StbGui.stbg_end_container();

            StbGui.stbg_begin_container("con3332", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL, StbGui.stbg_build_constrains(0, 0, float.MaxValue, 200));
            {
                StbGui.stbg_scrollbar("vertical-sb", StbGui.STBG_SCROLLBAR_DIRECTION.VERTICAL, ref scrollbar_value, 0, 100);

                StbGui.stbg_label($"Scrollbar Value: {scrollbar_value}");
            }
            StbGui.stbg_end_container();

            

        }
        StbGui.stbg_end_window();

        StbGui.stbg_label("This is the debug window!");
    }
}
