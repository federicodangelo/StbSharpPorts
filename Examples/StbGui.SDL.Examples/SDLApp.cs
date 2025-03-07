namespace StbSharp.Examples;

public class SDLApp : SDLAppBase
{
    private readonly App app;

    public SDLApp() : base(App.options)
    {
        app = new App(this);
    }

    protected override void on_render_stbgui()
    {
        app.Render();
    }
}
