
using System.Diagnostics;

namespace StbSharp.Examples;

public class WAApp : WAAppBase
{
    private readonly App app;

    public WAApp() : base(App.options)
    {
        app = new App(this);
    }

    protected override void on_render_stbgui()
    {
        app.Render();

        if (StbGui.stbg_begin_window(TestBenchmarksWindow.TITLE))
        {
            StbGui.stbg_label("Don't run benchmarks with the console open, they run slower!!");
         
            if (StbGui.stbg_button("Benchmark Javascript"))
            {
                TestBenchmarksWindow.LogResult(CanvasInterop.RunBenchmarkJavascript());
            }

            StbGui.stbg_end_window();
        }
    }
}
