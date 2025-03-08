using System.Runtime.InteropServices.JavaScript;
using StbSharp.Examples;

// Create a "Main" method. This is required by the tooling.
return;

public static partial class Main
{
    private static WAApp? app;

    [JSExport]
    public static async Task Init()
    {
        // Ensure JS module loaded.
        await JSHost.ImportAsync("canvas-interop", "/canvas-interop.js");

        app = new WAApp();
    }

    [JSExport]
    public static void Render()
    {
        app?.loop_once();
    }
}
