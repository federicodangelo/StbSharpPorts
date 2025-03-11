using BenchmarkDotNet.Attributes;

using StbSharp;

public class RenderBenchmark_Window_OneButton : RenderBenchmark
{
    protected override void SetupContent()
    {
        StbGui.stbg_begin_frame();

        for (int i = 0; i < WidgetsCount; i++)
        {
            StbGui.stbg_begin_window(stringMemoryPool.Concat("Window", i));
            {
                StbGui.stbg_set_last_widget_position(0, 0);
                StbGui.stbg_button("Button1");
            }
            StbGui.stbg_end_window();
        }

        StbGui.stbg_end_frame();
    }


    [Benchmark]
    public void WindowOneButton()
    {
        StbGui.stbg_render();
    }
}
