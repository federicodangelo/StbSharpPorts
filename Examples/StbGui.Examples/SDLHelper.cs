using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SDL3;

namespace StbSharp.Examples;

public class SDLHelper
{
    private struct ClipRectInfo
    {
        public bool enabled;
        public SDL.Rect rect;
    }

    private static readonly Queue<ClipRectInfo> clipRectInfos = new();

    static public void PushClipRect(nint renderer, StbGui.stbg_rect rect)
    {
        var lastClip = new ClipRectInfo();
        lastClip.enabled = SDL.RenderClipEnabled(renderer);
        if (lastClip.enabled)
            SDL.GetRenderClipRect(renderer, ref lastClip.rect);

        clipRectInfos.Enqueue(lastClip);

        if (!SDL.SetRenderClipRect(renderer, new SDL.Rect { X = (int)rect.x0, Y = (int)rect.y0, W = (int)(rect.x1 - rect.x0), H = (int)(rect.y1 - rect.y0) }))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to set clip rect: {SDL.GetError()}");
        }
    }

    static public void PopClipRect(nint renderer)
    {
        Debug.Assert(clipRectInfos.Count > 0);

        var clip = clipRectInfos.Dequeue();

        if (clip.enabled)
        {
            SDL.SetRenderClipRect(renderer, clip.rect);
        }
        else
        {
            SDL.SetRenderClipRect(renderer, 0);
        }
    }


}
