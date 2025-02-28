using System.Diagnostics;
using SDL3;

namespace StbSharp.Examples;

public class SDLHelper
{
    private static readonly Stack<StbGui.stbg_rect> clip_rects = new();

    static public void PushClipRect(nint renderer, StbGui.stbg_rect rect)
    {
        var prev_clip = clip_rects.Count > 0 ? clip_rects.Peek() : StbGui.stbg_build_rect_infinite();

        var rect_clipped = StbGui.stbg_clamp_rect(rect, prev_clip);

        clip_rects.Push(rect_clipped);

        if (!SDL.SetRenderClipRect(renderer, StbgRectToSdlRect(rect_clipped)))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to set clip rect: {SDL.GetError()}");
        }
    }

    static public void PopClipRect(nint renderer)
    {
        Debug.Assert(clip_rects.Count > 0);

        clip_rects.Pop();

        if (clip_rects.Count > 0)
        {
            if (!SDL.SetRenderClipRect(renderer, StbgRectToSdlRect(clip_rects.Peek())))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to set clip rect: {SDL.GetError()}");
            }
        }
        else
        {
            if (!SDL.SetRenderClipRect(renderer, 0))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to set clip rect: {SDL.GetError()}");
            }
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
}
