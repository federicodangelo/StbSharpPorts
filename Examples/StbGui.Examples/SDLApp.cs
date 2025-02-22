namespace StbSharp.Examples;

using System.Buffers.Text;
using StbSharp;

public class SDLApp : SDLAppBase
{

    // Available fonts: 
    // DefaultFontPath = "Fonts/ProggyClean.ttf", DefaultFontSize = 13
    // DefaultFontPath = "Fonts/ProggyTiny.ttf", DefaultFontSize = 10
    // DefaultFontPath = "Fonts/DroidSans.ttf", DefaultFontSize = 24
    // DefaultFontPath = "Fonts/Karla-Regular.ttf", DefaultFontSize = 16
    // DefaultFontPath = "Fonts/Roboto-Medium.ttf", DefaultFontSize = 16

    public SDLApp() : base(new SdlAppOptions() { WindowName = "Example App", DefaultFontName = "Font", DefaultFontPath = "Fonts/ProggyClean.ttf", DefaultFontSize = 13 })
    {
        var txt = "Hello World";

        txt.AsSpan().CopyTo(text_to_edit.Span);
        text_to_edit_length = txt.Length;
    }

    private bool showButton3 = true;
    private float scrollbar_value = 50;
    private int scrollbar_value_int = 50;

    private readonly StringMemoryPool mp = new StringMemoryPool();

    private bool show_title = true;
    private bool resizable = true;
    private bool show_scrollbars = true;
    private bool movable = true;

    private bool window_open = true;

    private Memory<char> text_to_edit = new Memory<char>(new char[1024]);
    private int text_to_edit_length;

    protected override void OnRenderStbGui()
    {
        mp.ResetPool();

        StbGui.stbg_label(mp.Concat("FPS: ", Metrics.Fps));
        StbGui.stbg_label(mp.Concat("Allocated Bytes Delta: ", Metrics.LastFrameAllocatedBytes));
        StbGui.stbg_label(mp.Concat("Total GC Performed: ", Metrics.TotalGarbageCollectionsPerformed));
        StbGui.stbg_label(mp.Concat("String Memory Pool Used Characters: ", StbGui.stbg_get_frame_stats().string_memory_pool_used_characters));
        StbGui.stbg_label(mp.Concat("String Memory Pool Overflown Characters: ", StbGui.stbg_get_frame_stats().string_memory_pool_overflowed_characters));

        if (StbGui.stbg_begin_window("Window 1",
                (show_title ? 0 : StbGui.STBG_WINDOW_OPTIONS.NO_TITLE) |
                (resizable ? 0 : StbGui.STBG_WINDOW_OPTIONS.NO_RESIZE) |
                (show_scrollbars ? 0 : StbGui.STBG_WINDOW_OPTIONS.NO_SCROLLBAR) |
                (movable ? 0 : StbGui.STBG_WINDOW_OPTIONS.NO_MOVE))
            )
        {
            if (StbGui.stbg_get_last_widget_is_new())
                StbGui.stbg_set_last_widget_position(100, 100);

            if (StbGui.stbg_button(mp.Concat("Toggle Title: ", show_title)))
                show_title = !show_title;

            if (StbGui.stbg_button(mp.Concat("Toggle Resizable: ", resizable)))
                resizable = !resizable;

            if (StbGui.stbg_button(mp.Concat("Toggle Scrollbars: ", show_scrollbars)))
                show_scrollbars = !show_scrollbars;

            if (StbGui.stbg_button(mp.Concat("Toggle Movable: ", movable)))
                movable = !movable;

            StbGui.stbg_button("Button 1");
            if (StbGui.stbg_button("Toggle Button 3"))
                showButton3 = !showButton3;

            if (showButton3)
                StbGui.stbg_button("Button 3");

            StbGui.stbg_end_window();
        }

        if (StbGui.stbg_begin_window("Window 2", ref window_open))
        {
            if (StbGui.stbg_get_last_widget_is_new())
                StbGui.stbg_set_last_widget_position(200, 150);

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
                    StbGui.stbg_label(mp.Concat("Scrollbar Value: ", scrollbar_value));

                    StbGui.stbg_label(mp.Concat("Scrollbar Value Int: ", scrollbar_value_int));
                }
                StbGui.stbg_end_container();
            }
            StbGui.stbg_end_container();

            StbGui.stbg_end_window();
        }

        if (StbGui.stbg_begin_window("Window 4"))
        {
            if (StbGui.stbg_get_last_widget_is_new())
                StbGui.stbg_set_last_widget_position(300, 250);

            StbGui.stbg_textfield("textfield1", text_to_edit, ref text_to_edit_length);

            for (int i = 0; i < 20; i++)
            {
                StbGui.stbg_button(mp.Concat("Test Button ", i));
            }

            StbGui.stbg_button("Test Button XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

            StbGui.stbg_end_window();
        }

        StbGui.stbg_label("This is the debug window!");

        if (!window_open && StbGui.stbg_button("Re-open Window 2"))
            window_open = true;
    }
}
