using System.Runtime.InteropServices.JavaScript;

public partial class CanvasInterop
{
    [JSImport("init", "canvas-interop")]
    public static partial void Init();

    [JSImport("getWidth", "canvas-interop")]
    public static partial int GetWidth();

    [JSImport("getHeight", "canvas-interop")]
    public static partial int GetHeight();

    [JSImport("clear", "canvas-interop")]
    public static partial void Clear(byte r, byte g, byte b, byte a);

    [JSImport("drawBorder", "canvas-interop")]
    public static partial void DrawBorder(byte r1, byte g1, byte b1, byte a1, byte r2, byte g2, byte b2, byte a2, float x, float y, float w, float h, int border_size);

    [JSImport("drawRectangle", "canvas-interop")]
    public static partial void DrawRectangle(byte r, byte g, byte b, byte a, float x, float y, float w, float h);

    [JSImport("pushClip", "canvas-interop")]
    public static partial void PushClip(float x, float y, float w, float h);
    
    [JSImport("popClip", "canvas-interop")]
    public static partial void PopClip();
}