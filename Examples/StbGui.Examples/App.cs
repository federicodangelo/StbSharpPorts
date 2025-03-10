namespace StbSharp.Examples;

using StbSharp;

public class App
{
    // Available fonts: 
    // DefaultFontPath = "Fonts/ProggyClean.ttf", DefaultFontSize = 13
    // DefaultFontPath = "Fonts/ProggyTiny.ttf", DefaultFontSize = 10

    static public StbGuiAppBase.StbGuiAppOptions options = new() { DefaultFontName = "Font", DefaultFontPath = "Fonts/ProggyClean.ttf", DefaultFontSize = 13 };

    private readonly StbGuiAppBase appBase;
    public App(StbGuiAppBase appBase)
    {
        this.appBase = appBase;
    }

    private readonly StbGuiStringMemoryPool mp = new StbGuiStringMemoryPool();


    public void Render()
    {
        mp.ResetPool();

        DebugWindow.Render(appBase, mp);

        TestWindows.Render(appBase, mp);
    }
}
