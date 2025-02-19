using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SDL3;

namespace StbSharp.Examples;

public class SDLHelper
{
    private static readonly Queue<StbGui.stbg_rect> clip_rects = new();

    static public void PushClipRect(nint renderer, StbGui.stbg_rect rect)
    {
        var prev_clip = clip_rects.Count > 0 ? clip_rects.Peek() : StbGui.stbg_build_rect_infinite();

        var rect_clipped = StbGui.stbg_clamp_rect(rect, prev_clip);

        clip_rects.Enqueue(rect_clipped);

        if (!SDL.SetRenderClipRect(renderer, StbgRectToSdlRect(rect_clipped)))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to set clip rect: {SDL.GetError()}");
        }
    }

    static public void PopClipRect(nint renderer)
    {
        Debug.Assert(clip_rects.Count > 0);

        var rect = clip_rects.Dequeue();

        if (clip_rects.Count > 0)
        {
            SDL.SetRenderClipRect(renderer, StbgRectToSdlRect(rect));
        }
        else
        {
            SDL.SetRenderClipRect(renderer, 0);
        }
    }

    static public bool HasClipping()
    {
        return clip_rects.Count > 0;
    }

    static public SDL.Rect StbgRectToSdlRect(StbGui.stbg_rect rect)
    {
        return new SDL.Rect { X = (int)rect.x0, Y = (int)rect.y0, W = (int)(rect.x1 - rect.x0), H = (int)(rect.y1 - rect.y0) };
    }

    static public StbGui.stbg_rect SdlRectToStbgRect(SDL.Rect rect)
    {
        return StbGui.stbg_build_rect(
            rect.X,
            rect.Y,
            rect.X + rect.W,
            rect.Y + rect.H
        );
    }
}
