using System.Runtime.InteropServices.JavaScript;
using StbSharp;

public partial class CanvasInterop
{
    static public ulong BuildRGBA(StbGui.stbg_color color)
    {
        return ((ulong) color.a) << 24 |
               ((ulong) color.b) << 16 |
               ((ulong) color.g) << 8 |
               ((ulong) color.r);
    }

    [JSImport("init", "canvas-interop")]
    public static partial void Init();

    [JSImport("getWidth", "canvas-interop")]
    public static partial int GetWidth();

    [JSImport("getHeight", "canvas-interop")]
    public static partial int GetHeight();

    [JSImport("clear", "canvas-interop")]
    public static partial void Clear(double color);

    [JSImport("drawBorder", "canvas-interop")]
    public static partial void DrawBorder(double color1, double color2, double x, double y, double w, double h, int border_size);

    [JSImport("drawRectangle", "canvas-interop")]
    public static partial void DrawRectangle(double color, double x, double y, double w, double h);

    [JSImport("pushClip", "canvas-interop")]
    public static partial void PushClip(double x, double y, double w, double h);

    [JSImport("popClip", "canvas-interop")]
    public static partial void PopClip();

    [JSImport("createTexture", "canvas-interop")]
    public static partial int CreateTexture(int width, int height, [JSMarshalAs<JSType.MemoryView>] Span<byte> pixels);

    [JSImport("destroyTexture", "canvas-interop")]
    public static partial void DestroyTexture(int id);

    [JSImport("drawTextureRectangle", "canvas-interop")]
    public static partial void DrawTextureRectangle(int id, double fromX, double fromY, double fromWidth, double fromHeight, double toX, double toY, double toWidth, double toHeight, double color);

    public const int DRAW_TEXTURE_RECTANGLE_BATCH_ELEMENT_SIZE = 9;

    [JSImport("drawTextureRectangleBatch", "canvas-interop")]
    public static partial void DrawTextureRectangleBatch(int id, [JSMarshalAs<JSType.MemoryView>] Span<double> buffer);

    [JSImport("drawBatch", "canvas-interop")]
    public static partial void DrawBatch([JSMarshalAs<JSType.MemoryView>] Span<double> buffer);

    [JSImport("presentFrame", "canvas-interop")]
    public static partial void PresentFrame();

   [JSImport("getInputEventsCount", "canvas-interop")]
    public static partial int GetInputEventsCount();

    [JSImport("getInputEvent", "canvas-interop")]
    public static partial int GetInputEvent(int index);

    [JSImport("getInputEventProperty", "canvas-interop")]
    public static partial int GetInputEventProperty(int index, string property);

    [JSImport("getInputEventPropertyString", "canvas-interop")]
    public static partial int GetInputEventPropertyString(int index, string property, [JSMarshalAs<JSType.MemoryView>] Span<int> buffer); //Returns string length

    [JSImport("clearInputEvents", "canvas-interop")]
    public static partial void ClearInputEvents();

    [JSImport("setCursor", "canvas-interop")]
    public static partial void SetCursor(string cursor);

    [JSImport("setTitle", "canvas-interop")]
    public static partial void SetTitle(string title);

    [JSImport("copyToClipboard", "canvas-interop")]
    public static partial void CopyToClipboard(string text);
    
    [JSImport("getFromClipboard", "canvas-interop")]
    public static partial string GetFromClipboard();
}