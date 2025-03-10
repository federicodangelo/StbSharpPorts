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

static public class TestWindows
{
    static public bool open = true;
    static public bool initialized = false;
    static private TestWindow[] windows;

    static public void Render(StbGuiAppBase appBase, StbGuiStringMemoryPool mp)
    {
        if (!initialized)
        {
            initialized = true;
            windows = [
                new TestWindowsWindow(appBase, mp),
                new TestScrollbarsWindow(appBase, mp),
                new TestImagesWindow(appBase, mp),
                new TestTextboxesWindow(appBase, mp),
            ];
        }

        if (StbGui.stbg_begin_window("Test Windows", ref open))
        {
            StbGui.stbg_set_last_widget_position_if_new(5, 205);

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

    static public void WindowButton(TestWindow window, StbGuiStringMemoryPool mp)
    {
        if (StbGui.stbg_button(mp.Build(window.open ? "Close " : "Open ") + window.title))
            window.open = !window.open;
    }
}
