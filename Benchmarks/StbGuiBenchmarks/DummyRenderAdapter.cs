
using StbSharp;

public class DummyRenderAdapter : StbGui.stbg_render_adapter
{
    public void destroy()
    {
    }

    public void draw_border(StbGui.stbg_rect bounds, int border_size, StbGui.stbg_color border_color, StbGui.stbg_color background_color)
    {
    }

    public void draw_image(StbGui.stbg_rect bounds, StbGui.stbg_rect image_rect, StbGui.stbg_color color, int image_id)
    {
    }

    public void draw_images(Span<StbGui.stbg_draw_image_rect> rects, int image_id)
    {
    }

    public void draw_line(StbGui.stbg_position from, StbGui.stbg_position to, StbGui.stbg_color color, float thickness)
    {
    }

    public void draw_rectangle(StbGui.stbg_rect bounds, StbGui.stbg_color background_color)
    {
    }

    public void draw_text(StbGui.stbg_rect bounds, StbGui.stbg_render_text_parameters text_parameters)
    {
    }

    public StbGui.stbg_position get_character_position_in_text(ReadOnlySpan<char> text, StbGui.stbg_font font, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options, int character_index)
    {
        int x = 0;
        int y = 0;
        var single_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE) != 0;

        for (int i = 0; i < character_index; i++)
        {
            char c = text[i];
            if (i == character_index)
            {
                break;
            }
            if (c == '\n')
            {
                if (single_line)
                {
                    c = ' ';
                }
                else
                {
                    y += 1;
                    x = 0;
                    continue;
                }
            }
            x++;
        }

        return StbGui.stbg_build_position(x, y);
    }

    public string get_render_backend()
    {
        return "dummy";
    }

    public StbGui.stbg_size measure_text(ReadOnlySpan<char> text, StbGui.stbg_font font, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS measure_options)
    {
        return new StbGui.stbg_size() { width = text.Length * style.size, height = style.size };
    }

    public void pop_clip_rect()
    {
    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {
    }

    public void register_font(int font_id, StbGui.stbg_register_font_parameters parameters, byte[] bytes)
    {
    }

    public void register_image(int image_id, StbGui.stbg_register_image_parameters parameters, byte[] pixels, int width, int height, int bytes_per_pixel)
    {
    }

    public void render_begin_frame(StbGui.stbg_color background_color)
    {
    }

    public void render_end_frame()
    {
    }
}
