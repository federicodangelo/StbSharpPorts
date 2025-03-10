namespace StbSharp.Examples;

using System.Reflection;
using StbSharp;

public class App
{
    // Available fonts: 
    // DefaultFontPath = "Fonts/ProggyClean.ttf", DefaultFontSize = 13
    // DefaultFontPath = "Fonts/ProggyTiny.ttf", DefaultFontSize = 10

    static public StbGuiAppBase.StbGuiAppOptions options = new() { DefaultFontName = "Font", DefaultFontPath = "Fonts/ProggyClean.ttf", DefaultFontSize = 13 };

    private readonly StbGuiAppBase appBase;

    private int test_image_id;
    private int[] test_sub_images = new int[4];

    private byte[] GetResourceFileBytes(string fileName)
    {
        var resourceName = "StbGui.Examples.Resources." + fileName.Replace("\\", ".").Replace("/", ".");

        using (var memoryStream = new MemoryStream())
        {
            using (var stream = typeof(App).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Resource file not found.");

                stream.CopyTo(memoryStream);
            }
            return memoryStream.ToArray();
        }
    }

    public App(StbGuiAppBase appBase)
    {
        this.appBase = appBase;

        StbGui.stbg_textbox_set_text_to_edit(ref text_to_edit, "Hello World THIS IS A VERY LONG TEXT TO EDITTTT");
        StbGui.stbg_textbox_set_text_to_edit(ref text_to_edit2, "Hello World THIS IS MULTILINE!\nYESSS!!!");

        test_image_id = appBase.add_image(GetResourceFileBytes("test.png"), false);
        test_sub_images[0] = appBase.add_sub_image(test_image_id, 0, 0, 128, 128);
        test_sub_images[1] = appBase.add_sub_image(test_image_id, 128, 0, 128, 128);
        test_sub_images[2] = appBase.add_sub_image(test_image_id, 0, 128, 128, 128);
        test_sub_images[3] = appBase.add_sub_image(test_image_id, 128, 128, 128, 128);
    }

    private bool showButton3 = true;
    private float scrollbar_value = 50;
    private int scrollbar_value_int = 50;

    private readonly StbGuiStringMemoryPool mp = new StbGuiStringMemoryPool();

    private bool show_title = true;
    private bool resizable = true;
    private bool show_scrollbars = true;
    private bool movable = true;

    private bool window2_open = true;
    private bool window5_open = true;

    private StbGui.stbg_textbox_text_to_edit text_to_edit = StbGui.stbg_textbox_build_text_to_edit(1024);
    private StbGui.stbg_textbox_text_to_edit text_to_edit2 = StbGui.stbg_textbox_build_text_to_edit(1024);

    private bool show_subimages = true;

    public void Render()
    {
        mp.ResetPool();

        var metrics = appBase.Metrics;

        StbGui.stbg_label(mp.Concat(mp.Concat(mp.Concat("FPS: ", appBase.Metrics.Fps), " Skipped Frames: "), appBase.Metrics.SkippedFrames));
        StbGui.stbg_label(mp.Concat("Render Backend: ", appBase.RenderBackend));
        StbGui.stbg_label(mp.Concat("Allocated Bytes Delta: ", metrics.LastSecondAllocatedBytes));
        StbGui.stbg_label(mp.Concat(mp.Concat("Process input time : ", metrics.average_performance_metrics.process_input_time_us / 1000.0f, 3), " ms"));
        StbGui.stbg_label(mp.Concat(mp.Concat("Layout widgets time: ", metrics.average_performance_metrics.layout_widgets_time_us / 1000.0f, 3), " ms"));
        StbGui.stbg_label(mp.Concat(mp.Concat("Render time        : ", metrics.average_performance_metrics.render_time_us / 1000.0f, 3), " ms"));
        //Console.WriteLine(appBase.Metrics.LastFrameAllocatedBytes);
        StbGui.stbg_label(mp.Concat("Total GC Performed: ", metrics.TotalGarbageCollectionsPerformed));
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

        if (StbGui.stbg_begin_window("Window 2", ref window2_open))
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

            StbGui.stbg_textbox("textbox1", ref text_to_edit);
            StbGui.stbg_textbox("textbox2", ref text_to_edit2, 3);

            for (int i = 0; i < 20; i++)
            {
                StbGui.stbg_button(mp.Concat("Test Button ", i));
            }

            StbGui.stbg_button("Test Button XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

            StbGui.stbg_end_window();
        }

        if (StbGui.stbg_begin_window("Window 5", ref window5_open))
        {
            if (StbGui.stbg_get_last_widget_is_new())
            {
                StbGui.stbg_set_last_widget_position(400, 350);
                StbGui.stbg_set_last_widget_size(400, 300);
            }

            StbGui.stbg_begin_container("hor", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL);
            {
                StbGui.stbg_image("image", test_image_id, 0.5f);

                if (show_subimages)
                {
                    StbGui.stbg_begin_container("subV1", StbGui.STBG_CHILDREN_LAYOUT.VERTICAL);
                    {
                        StbGui.stbg_image(mp.Concat("subimage", 0), test_sub_images[0], 0.5f);
                        StbGui.stbg_image(mp.Concat("subimage", 2), test_sub_images[2], 0.5f);
                    }
                    StbGui.stbg_end_container();

                    StbGui.stbg_begin_container("subV2", StbGui.STBG_CHILDREN_LAYOUT.VERTICAL);
                    {
                        StbGui.stbg_image(mp.Concat("subimage", 1), test_sub_images[1], 0.5f);
                        StbGui.stbg_image(mp.Concat("subimage", 3), test_sub_images[3], 0.5f);
                    }
                    StbGui.stbg_end_container();
                }
            }
            StbGui.stbg_end_container();

            StbGui.stbg_label("The image below is a button!");
            if (StbGui.stbg_button("imagebutton", test_sub_images[0], test_sub_images[1], test_sub_images[2], true, 0.25f))
                show_subimages = !show_subimages;

            StbGui.stbg_end_window();
        }

        StbGui.stbg_label("This is the debug window!");

        if (!window2_open && StbGui.stbg_button("Re-open Window 2"))
            window2_open = true;

        if (!window5_open && StbGui.stbg_button("Re-open Window 5"))
            window5_open = true;
    }
}
