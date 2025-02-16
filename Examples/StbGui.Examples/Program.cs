namespace StbSharp.Examples;

public class Program
{
    public static void Main(string[] args)
    {
        SDLApp app = new SDLApp();

        app.MainLoop();

        app.Dispose();
    }
}

