namespace StbSharp.Examples;

public class WAHelper
{
    static private int count;

    static public void PushClipRect(StbGui.stbg_rect rect)
    {
        count++;
        CanvasInterop.PushClip(rect.x0, rect.y0, rect.x1 - rect.x0, rect.y1 - rect.y0);
    }

    static public void PopClipRect()
    {
        count--;
        CanvasInterop.PopClip();
    }

    static public bool HasClipping()
    {
        return count > 0;
    }
}
