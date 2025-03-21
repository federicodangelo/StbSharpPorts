using StbSharp;
using StbSharp.Examples;

public abstract class TestWindow
{
    public bool open;
    public readonly string title;
    protected readonly StbGuiAppBase appBase;
    protected readonly StbGuiStringMemoryPool mp;

    public TestWindow(string title, StbGuiAppBase appBase, StbGuiStringMemoryPool mp)
    {
        this.title = title;
        this.appBase = appBase;
        this.mp = mp;
    }

    public abstract void Render();
}

public static class TestWindows
{
    public static bool open = true;
    public static bool initialized = false;
    private static TestWindow[] windows = Array.Empty<TestWindow>();
    private static bool disable_skip_rendering_optimization = false;

    public static void Render(StbGuiAppBase appBase, StbGuiStringMemoryPool mp)
    {
        if (!initialized)
        {
            initialized = true;
            windows = [
                new TestWindowsWindow(appBase, mp),
                new TestScrollbarsWindow(appBase, mp),
                new TestImagesWindow(appBase, mp),
                new TestTextboxesWindow(appBase, mp),
                new TestBenchmarksWindow(appBase, mp),
                new TestNodesWindow(appBase, mp),
            ];
            StbGui.stbg_set_render_options_flag(StbGui.STBG_RENDER_OPTIONS.DISABLE_SKIP_RENDERING_OPTIMIZATION, disable_skip_rendering_optimization);
        }

        if (StbGui.stbg_begin_window("Test Windows", ref open))
        {
            StbGui.stbg_set_last_widget_position_if_new(5, 205);

            if (StbGui.stbg_checkbox("Disable skip rendering optimization", ref disable_skip_rendering_optimization))
            {
                StbGui.stbg_set_render_options_flag(StbGui.STBG_RENDER_OPTIONS.DISABLE_SKIP_RENDERING_OPTIMIZATION, disable_skip_rendering_optimization);
            }

            for (var i = 0; i < windows.Length; i++)
            {
                WindowButton(windows[i], mp);
            }

            StbGui.stbg_end_window();
        }

        for (var i = 0; i < windows.Length; i++)
            windows[i].Render();

        if (!open)
            if (StbGui.stbg_button("Open Test Windows"))
                open = true;
    }

    public static void WindowButton(TestWindow window, StbGuiStringMemoryPool mp)
    {
        if (StbGui.stbg_button(mp.Build(window.open ? "Close " : "Open ") + window.title))
            window.open = !window.open;
    }
}
