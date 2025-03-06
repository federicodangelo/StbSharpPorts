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

    [JSImport("createCanvas", "canvas-interop")]
    public static partial int CreateCanvas(int width, int height);

    [JSImport("destroyCanvas", "canvas-interop")]
    public static partial void DestroyCanvas(int id);

    [JSImport("setCanvasPixels", "canvas-interop")]
    public static partial void SetCanvasPixels(int id, int width, int height, [JSMarshalAs<JSType.MemoryView>] Span<byte> pixels);

    [JSImport("copyCanvasPixels", "canvas-interop")]
    public static partial void CopyCanvasPixels(int id, int fromX, int fromY, int fromWidth, int fromHeight, int toX, int toY, int toWidth, int toHeight, byte r, byte g, byte b, byte a);

    [JSImport("getEventsCount", "canvas-interop")]
    public static partial int GetEventsCount();

    [JSImport("getEvent", "canvas-interop")]
    public static partial int GetEvent(int index);

    [JSImport("getEventProperty", "canvas-interop")]
    public static partial int GetEventProperty(int index, string property);

    [JSImport("getEventPropertyString", "canvas-interop")]
    public static partial int GetEventPropertyString(int index, string property, [JSMarshalAs<JSType.MemoryView>] Span<int> buffer); //Returns string length

    [JSImport("clearEvents", "canvas-interop")]
    public static partial void ClearEvents();

    [JSImport("setCursor", "canvas-interop")]
    public static partial void SetCursor(string cursor);
}