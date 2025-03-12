#pragma warning disable IDE1006 // Naming Styles

#define USE_DRAW_BUFFER

using System.Diagnostics;

using StbSharp;

using font_id = int;
using image_id = int;

class WARenderAdapter : StbGui.stbg_render_adapter
{
    private int clip_rects_count;

    private readonly Dictionary<font_id, StbGuiFont> fonts = [];
    private readonly Dictionary<image_id, nint> images = [];

#if USE_DRAW_BUFFER
    private const int DRAW_BATCH_BORDER = 1;
    private const int DRAW_BATCH_RECTANGLE = 2;
    private const int DRAW_BATCH_TEXTURE_RECTANGLE = 3;
    private const int DRAW_BATCH_TEXTURE_RECTANGLE_BATCH = 4;
    private const int DRAW_BATCH_PUSH_CLIP_RECT = 5;
    private const int DRAW_BATCH_POP_CLIP_RECT = 6;
    private const int DRAW_BATCH_LINE = 7;

    private readonly double[] draw_batch_buffer = new double[8192];
    private int draw_batch_buffer_index = 0;
#endif

    public string get_render_backend()
    {
        return CanvasInterop.GetRenderBackend();
    }

    public void register_font(int font_id, StbGui.stbg_register_font_parameters parameters, byte[] bytes)
    {
        StbGuiFont font = new StbGuiFont(parameters.name, bytes, parameters.size, parameters.oversampling, parameters.bilinear_filtering, this);
        fonts.Add(font_id, font);
    }


    public void register_image(int image_id, StbGui.stbg_register_image_parameters parameters, byte[] pixels, int width, int height, int bytes_per_pixel)
    {
        var texture_id = CanvasInterop.CreateTexture(width, height, pixels, bytes_per_pixel);

        images.Add(image_id, texture_id);
    }

#if USE_DRAW_BUFFER
    private void flush_draw_buffer_if_doesnt_fit(int size)
    {
        if (draw_batch_buffer_index + size >= draw_batch_buffer.Length)
        {
            flush_draw_buffer();
        }
    }

    private bool draw_buffer_fits(int size)
    {
        return draw_batch_buffer_index + size < draw_batch_buffer.Length;
    }

    private void flush_draw_buffer()
    {
        if (draw_batch_buffer_index > 0)
        {
            CanvasInterop.DrawBatch(draw_batch_buffer.AsSpan(0, draw_batch_buffer_index));
            draw_batch_buffer_index = 0;
        }
    }
#endif

#if !USE_DRAW_BUFFER
    private readonly double[] draw_rects_buffer = new double[1024 * CanvasInterop.DRAW_TEXTURE_RECTANGLE_BATCH_ELEMENT_SIZE];
#endif

    public void draw_text(StbGui.stbg_rect bounds, StbGui.stbg_render_text_parameters text_parameters)
    {
        var font = fonts[text_parameters.font_id];
        StbGuiTextHelper.draw_text(text_parameters, bounds, font, this);
    }

    public void draw_image(StbGui.stbg_rect bounds, StbGui.stbg_rect image_rect, StbGui.stbg_color color, int image_id)
    {
        Span<StbGui.stbg_draw_image_rect> tmp_rect = stackalloc StbGui.stbg_draw_image_rect[1];

        tmp_rect[0].bounds = bounds;
        tmp_rect[0].image_rect = image_rect;
        tmp_rect[0].color = color;

        draw_images(tmp_rect, image_id);
    }

    public void draw_images(Span<StbGui.stbg_draw_image_rect> rects, int image_id)
    {
        int count = rects.Length;
        nint texture_id = image_id != 0 ? images[image_id] : 0;
#if USE_DRAW_BUFFER
        int size = 3 + count * 9;

        flush_draw_buffer_if_doesnt_fit(size);

        int batches;
        int rects_per_batch;

        if (draw_buffer_fits(size))
        {
            // Fits in a single batch
            batches = 1;
            rects_per_batch = count;
        }
        else
        {
            // Doesn't fit in a single batch, so we need to split it into multiple batches
            rects_per_batch = (draw_batch_buffer.Length - 3) / 9;
            batches = count / rects_per_batch + (count % rects_per_batch == 0 ? 0 : 1);
        }

        int rect_index = 0;

        for (int batch = 0; batch < batches; batch++)
        {
            if (batch > 0)
            {
                // Flush the previous batch
                flush_draw_buffer();
            }

            var batch_count = Math.Min(rects_per_batch, count - rect_index);

            draw_batch_buffer[draw_batch_buffer_index++] = DRAW_BATCH_TEXTURE_RECTANGLE_BATCH;
            draw_batch_buffer[draw_batch_buffer_index++] = texture_id;
            draw_batch_buffer[draw_batch_buffer_index++] = batch_count;

            for (var j = 0; j < batch_count; j++)
            {
                var rect = rects[rect_index++];

                var r = rect.image_rect;
                var target = rect.bounds;
                var c = rect.color;

                draw_batch_buffer[draw_batch_buffer_index++] = r.x0;
                draw_batch_buffer[draw_batch_buffer_index++] = r.y0;
                draw_batch_buffer[draw_batch_buffer_index++] = r.x1 - r.x0;
                draw_batch_buffer[draw_batch_buffer_index++] = r.y1 - r.y0;
                draw_batch_buffer[draw_batch_buffer_index++] = target.x0;
                draw_batch_buffer[draw_batch_buffer_index++] = target.y0;
                draw_batch_buffer[draw_batch_buffer_index++] = target.x1 - target.x0;
                draw_batch_buffer[draw_batch_buffer_index++] = target.y1 - target.y0;
                draw_batch_buffer[draw_batch_buffer_index++] = CanvasInterop.BuildRGBA(c);
            }
        }
#else
        if (count < 2)
        {
            for (var i = 0; i < count; i++)
            {
                var rect = rects[i];

                var r = rect.image_rect;
                var target = rect.bounds;
                var c = rect.color;

                CanvasInterop.DrawTextureRectangle((int)texture_id, r.x0, r.y0, r.x1 - r.x0, r.y1 - r.y0, target.x0, target.y0, target.x1 - target.x0, target.y1 - target.y0, CanvasInterop.BuildRGBA(c));
            }
        }
        else
        {
            var index = 0;
            var len = 0;

            for (var i = 0; i < count; i++)
            {
                var rect = rects[i];

                var r = rect.image_rect;
                var target = rect.bounds;
                var c = rect.color;

                draw_rects_buffer[index++] = r.x0;
                draw_rects_buffer[index++] = r.y0;
                draw_rects_buffer[index++] = r.x1 - r.x0;
                draw_rects_buffer[index++] = r.y1 - r.y0;
                draw_rects_buffer[index++] = target.x0;
                draw_rects_buffer[index++] = target.y0;
                draw_rects_buffer[index++] = target.x1 - target.x0;
                draw_rects_buffer[index++] = target.y1 - target.y0;
                draw_rects_buffer[index++] = CanvasInterop.BuildRGBA(c);

                len += CanvasInterop.DRAW_TEXTURE_RECTANGLE_BATCH_ELEMENT_SIZE;

                if (index == draw_rects_buffer.Length)
                {
                    CanvasInterop.DrawTextureRectangleBatch((int)texture_id, draw_rects_buffer.AsSpan(0, len));
                    index = 0;
                    len = 0;
                }
            }

            CanvasInterop.DrawTextureRectangleBatch((int)texture_id, draw_rects_buffer.AsSpan(0, len));
        }
#endif
    }

    public void draw_rectangle(StbGui.stbg_rect bounds, StbGui.stbg_color background_color)
    {
#if USE_DRAW_BUFFER
        flush_draw_buffer_if_doesnt_fit(1 + 5);
        draw_batch_buffer[draw_batch_buffer_index++] = DRAW_BATCH_RECTANGLE;
        draw_batch_buffer[draw_batch_buffer_index++] = CanvasInterop.BuildRGBA(background_color);
        draw_batch_buffer[draw_batch_buffer_index++] = bounds.x0;
        draw_batch_buffer[draw_batch_buffer_index++] = bounds.y0;
        draw_batch_buffer[draw_batch_buffer_index++] = bounds.x1 - bounds.x0;
        draw_batch_buffer[draw_batch_buffer_index++] = bounds.y1 - bounds.y0;
#else
        CanvasInterop.DrawRectangle(
            CanvasInterop.BuildRGBA(background_color),
            bounds.x0, bounds.y0, bounds.x1 - bounds.x0, bounds.y1 - bounds.y0
        );
#endif
    }

    public void draw_border(StbGui.stbg_rect bounds, int border_size, StbGui.stbg_color border_color, StbGui.stbg_color background_color)
    {
#if USE_DRAW_BUFFER
        flush_draw_buffer_if_doesnt_fit(1 + 7);
        draw_batch_buffer[draw_batch_buffer_index++] = DRAW_BATCH_BORDER;
        draw_batch_buffer[draw_batch_buffer_index++] = CanvasInterop.BuildRGBA(background_color);
        draw_batch_buffer[draw_batch_buffer_index++] = CanvasInterop.BuildRGBA(border_color);
        draw_batch_buffer[draw_batch_buffer_index++] = bounds.x0;
        draw_batch_buffer[draw_batch_buffer_index++] = bounds.y0;
        draw_batch_buffer[draw_batch_buffer_index++] = bounds.x1 - bounds.x0;
        draw_batch_buffer[draw_batch_buffer_index++] = bounds.y1 - bounds.y0;
        draw_batch_buffer[draw_batch_buffer_index++] = border_size;
#else
        CanvasInterop.DrawBorder(
            CanvasInterop.BuildRGBA(background_color),
            CanvasInterop.BuildRGBA(border_color),
            bounds.x0, bounds.y0, bounds.x1 - bounds.x0, bounds.y1 - bounds.y0,
            border_size
        );
#endif
    }

    public void pop_clip_rect()
    {
        clip_rects_count--;
#if USE_DRAW_BUFFER
        flush_draw_buffer_if_doesnt_fit(1);
        draw_batch_buffer[draw_batch_buffer_index++] = DRAW_BATCH_POP_CLIP_RECT;
#else
        CanvasInterop.PopClip();
#endif

    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {
        clip_rects_count++;
#if USE_DRAW_BUFFER
        flush_draw_buffer_if_doesnt_fit(1 + 4);
        draw_batch_buffer[draw_batch_buffer_index++] = DRAW_BATCH_PUSH_CLIP_RECT;
        draw_batch_buffer[draw_batch_buffer_index++] = rect.x0;
        draw_batch_buffer[draw_batch_buffer_index++] = rect.y0;
        draw_batch_buffer[draw_batch_buffer_index++] = rect.x1 - rect.x0;
        draw_batch_buffer[draw_batch_buffer_index++] = rect.y1 - rect.y0;
#else
        CanvasInterop.PushClip(rect.x0, rect.y0, rect.x1 - rect.x0, rect.y1 - rect.y0);
#endif
    }

    public void render_begin_frame(StbGui.stbg_color background_color)
    {
        CanvasInterop.Clear(CanvasInterop.BuildRGBA(background_color));
    }

    public void render_end_frame()
    {
#if USE_DRAW_BUFFER
        flush_draw_buffer();
#endif
        Debug.Assert(clip_rects_count == 0);
    }

    public void draw_line(StbGui.stbg_position from, StbGui.stbg_position to, StbGui.stbg_color color, float thickness)
    {
#if USE_DRAW_BUFFER
        flush_draw_buffer_if_doesnt_fit(1 + 6);
        draw_batch_buffer[draw_batch_buffer_index++] = DRAW_BATCH_LINE;
        draw_batch_buffer[draw_batch_buffer_index++] = from.x;
        draw_batch_buffer[draw_batch_buffer_index++] = from.y;
        draw_batch_buffer[draw_batch_buffer_index++] = to.x;
        draw_batch_buffer[draw_batch_buffer_index++] = to.y;
        draw_batch_buffer[draw_batch_buffer_index++] = CanvasInterop.BuildRGBA(color);
        draw_batch_buffer[draw_batch_buffer_index++] = thickness;
#else
        CanvasInterop.DrawLine(from.x, from.y, to.x, to.y, CanvasInterop.BuildRGBA(color), thickness);
#endif
    }

    public StbGui.stbg_size measure_text(ReadOnlySpan<char> text, StbGui.stbg_font font, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options)
    {
        Span<StbGui.stbg_render_text_style_range> tmp_styles = stackalloc StbGui.stbg_render_text_style_range[1];

        var real_font = fonts[font.id];

        tmp_styles[0] = new StbGui.stbg_render_text_style_range()
        {
            start_index = 0,
            text_color = style.color,
            font_style = style.style
        };

        return StbGuiTextHelper.measure_text(text, real_font, style.size, tmp_styles, options);
    }

    public StbGui.stbg_position get_character_position_in_text(ReadOnlySpan<char> text, StbGui.stbg_font font, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options, int character_index)
    {
        Span<StbGui.stbg_render_text_style_range> tmp_styles = stackalloc StbGui.stbg_render_text_style_range[1];

        var real_font = fonts[font.id];

        tmp_styles[0] = new StbGui.stbg_render_text_style_range()
        {
            start_index = 0,
            text_color = style.color,
            font_style = style.style
        };

        return StbGuiTextHelper.get_character_position_in_text(text, real_font, style.size, tmp_styles, options, character_index);
    }

    public void destroy()
    {
        foreach (var image in images.Values)
        {
            CanvasInterop.DestroyTexture((int)image);
        }

        fonts.Clear();
        images.Clear();
    }
}
