using System.Diagnostics;
using StbSharp;
using StbSharp.Examples;

class WARenderAdapter : StbGuiRenderAdapterBase
{
    private int clip_rects_count;

    public override nint create_texture(int width, int height, StbGuiRenderAdapter.CreateTextureOptions options = default)
    {
        return CanvasInterop.CreateCanvas(width, height);
    }

    public override void destroy_texture(nint texture_id)
    {
        CanvasInterop.DestroyCanvas((int)texture_id);
    }

    private int[] draw_rects_buffer = new int[1024 * CanvasInterop.COPY_CANVAS_PIXELS_BATCH_ELEMENT_SIZE];

    public override void draw_rects(StbGuiRenderAdapter.Rect[] rects, int count, nint texture_id)
    {
        if (count < 2)
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
        else
        {
            var index = 0;
            var len = 0;

            for (var i = 0; i < count; i++)
            {
                var rect = rects[i];

                var r = rect.tex_coord_rect;
                var target = rect.rect;
                var c = rect.color;

                draw_rects_buffer[index++] = (int)r.x0;
                draw_rects_buffer[index++] = (int)r.y0;
                draw_rects_buffer[index++] = (int)(r.x1 - r.x0);
                draw_rects_buffer[index++] = (int)(r.y1 - r.y0);
                draw_rects_buffer[index++] = (int)target.x0;
                draw_rects_buffer[index++] = (int)target.y0;
                draw_rects_buffer[index++] = (int)(target.x1 - target.x0);
                draw_rects_buffer[index++] = (int)(target.y1 - target.y0);
                draw_rects_buffer[index++] = c.r;
                draw_rects_buffer[index++] = c.g;
                draw_rects_buffer[index++] = c.b;
                draw_rects_buffer[index++] = c.a;

                len += CanvasInterop.COPY_CANVAS_PIXELS_BATCH_ELEMENT_SIZE;

                if (index == draw_rects_buffer.Length)
                {
                    CanvasInterop.CopyCanvasPixelsBatch((int)texture_id, draw_rects_buffer.AsSpan(0, len));
                    index = 0;
                    len = 0;
                }
            }

            CanvasInterop.CopyCanvasPixelsBatch((int)texture_id, draw_rects_buffer.AsSpan(0, len));
        }
    }

    public override void draw_vertices(StbGuiRenderAdapter.Vertex[] vertices, int count, nint texture_id)
    {
        //throw new NotImplementedException();
    }

    public override void pop_clip_rect()
    {
        clip_rects_count--;
        CanvasInterop.PopClip();
    }

    public override void push_clip_rect(StbGui.stbg_rect rect)
    {
        clip_rects_count++;
        CanvasInterop.PushClip(rect.x0, rect.y0, rect.x1 - rect.x0, rect.y1 - rect.y0);
    }

    public override void set_texture_pixels(nint texture_id, StbGui.stbg_size size, byte[] pixels)
    {
        CanvasInterop.SetCanvasPixels((int)texture_id, (int)size.width, (int)size.height, pixels);
    }

    protected override void render_begin_frame(StbGui.stbg_color background_color)
    {
        CanvasInterop.Clear(background_color.r, background_color.g, background_color.b, background_color.a);
    }

    protected override void render_end_frame()
    {
        Debug.Assert(clip_rects_count == 0);
    }

    protected override void render_draw_rectangle(StbGui.stbg_rect bounds, StbGui.stbg_color background_color)
    {
        CanvasInterop.DrawRectangle(
            background_color.r, background_color.g, background_color.b, background_color.a,
            bounds.x0, bounds.y0, bounds.x1 - bounds.x0, bounds.y1 - bounds.y0
        );
    }

    protected override void render_draw_border(StbGui.stbg_rect bounds, int border_size, StbGui.stbg_color background_color, StbGui.stbg_color color)
    {
        CanvasInterop.DrawBorder(
            background_color.r, background_color.g, background_color.b, background_color.a,
            color.r, color.g, color.b, color.a,
            bounds.x0, bounds.y0, bounds.x1 - bounds.x0, bounds.y1 - bounds.y0,
            border_size
        );
    }
}