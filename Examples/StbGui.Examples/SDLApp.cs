namespace StbSharp.Examples;

using StbSharp;

public class SDLApp : SDLAppBase
{
    private bool showButton3 = true;

    public SDLApp() : base(new SdlAppOptions() { WindowName = "Example App" })
    {

    }

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

            StbGui.stbg_button("Button 1");
            StbGui.stbg_button("Button 3");
        }
        StbGui.stbg_end_window();

        StbGui.stbg_label("This is the debug window!");
    }
}
