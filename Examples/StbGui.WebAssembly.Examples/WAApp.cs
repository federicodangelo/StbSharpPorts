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
    }
}
