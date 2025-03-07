namespace StbSharp.Examples;

public class Program
{
    public static void Main(string[] args)
    {
        SDLApp app = new SDLApp();

        while(!app.Quit)
        {
            app.loop_once();
        }

        app.Dispose();
    }
}

