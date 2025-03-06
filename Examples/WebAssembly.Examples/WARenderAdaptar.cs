using StbSharp;

class WARenderAdapter : StbGuiRenderAdapter
{
    public nint create_texture(int width, int height, StbGuiRenderAdapter.CreateTextureOptions options = default)
    {
        return CanvasInterop.CreateCanvas(width, height);
    }

    public void destroy_texture(nint texture_id)
    {
        CanvasInterop.DestroyCanvas((int)texture_id);
    }

    public void draw_rects(StbGuiRenderAdapter.Rect[] rects, int count, nint texture_id)
    {
        for (var i = 0; i < count; i++)
        {
            var rect = rects[i];

            var r = rect.tex_coord_rect;
            var target = rect.rect;
            var c = rect.color;

            CanvasInterop.CopyCanvasPixels((int)texture_id, (int)r.x0, (int)r.y0, (int)(r.x1 - r.x0), (int)(r.y1 - r.y0), (int)target.x0, (int)target.y0, (int)(target.x1 - target.x0), (int)(target.y1 - target.y0), c.r, c.g, c.b, c.a);
        }
    }

    public void draw_vertices(StbGuiRenderAdapter.Vertex[] vertices, int count, nint texture_id)
    {
        //throw new NotImplementedException();
    }

    public void pop_clip_rect()
    {
        CanvasInterop.PopClip();
    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {
        CanvasInterop.PushClip(rect.x0, rect.y0, rect.x1 - rect.x0, rect.y1 - rect.y0);
    }

    public void set_texture_pixels(nint texture_id, StbGui.stbg_size size, byte[] pixels)
    {
        CanvasInterop.SetCanvasPixels((int)texture_id, (int)size.width, (int)size.height, pixels);
    }
}