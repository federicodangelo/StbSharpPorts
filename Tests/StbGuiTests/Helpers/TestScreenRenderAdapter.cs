
namespace StbSharp.Tests;

public class TestScreenRenderAdapter : StbGui.stbg_render_adapter
{
    private readonly TestRenderScreen screen;

    public TestScreenRenderAdapter(TestRenderScreen screen)
    {
        this.screen = screen;
    }

    public void destroy()
    {
    }

    public void draw_border(StbGui.stbg_rect bounds, int border_size, StbGui.stbg_color border_color, StbGui.stbg_color background_color)
    {
        for (int y = (int)bounds.y0; y < (int)bounds.y1; y++)
        {
            for (int x = (int)bounds.x0; x < (int)bounds.x1; x++)
            {
                char character =
                    x == (int)bounds.x0 || x == (int)bounds.x1 - 1 ? '|' :
                    y == (int)bounds.y0 || y == (int)bounds.y1 - 1 ? '-' :
                    ' ';

                if (!screen.IsClipped(x, y))
                    screen.SetTestRenderScreenPixel(x, y, new() { character = character, background_color = background_color, character_color = character != ' ' ? border_color : screen.GetTestRenderScreenPixel(x, y).character_color });
            }
        }
        screen.SetTestRenderScreenPixelCharacter((int)bounds.x0, (int)bounds.y0, '/');
        screen.SetTestRenderScreenPixelCharacter((int)bounds.x1 - 1, (int)bounds.y0, '\\');
        screen.SetTestRenderScreenPixelCharacter((int)bounds.x1 - 1, (int)bounds.y1 - 1, '/');
        screen.SetTestRenderScreenPixelCharacter((int)bounds.x0, (int)bounds.y1 - 1, '\\');
    }

    public void draw_image(StbGui.stbg_rect bounds, StbGui.stbg_rect image_rect, StbGui.stbg_color color, int image_id)
    {
        throw new NotImplementedException();
    }

    public void draw_images(Span<StbGui.stbg_draw_image_rect> rects, int image_id)
    {
        throw new NotImplementedException();
    }

    public void draw_line(StbGui.stbg_position from, StbGui.stbg_position to, StbGui.stbg_color color, float thickness)
    {
        throw new NotImplementedException();
    }

    public void draw_rectangle(StbGui.stbg_rect bounds, StbGui.stbg_color background_color)
    {
        for (int y = (int)bounds.y0; y < (int)bounds.y1; y++)
        {
            for (int x = (int)bounds.x0; x < (int)bounds.x1; x++)
            {
                char character = ' ';
                screen.SetTestRenderScreenPixel(x, y, new() { character = character, background_color = background_color, character_color = screen.GetTestRenderScreenPixel(x, y).character_color });
            }
        }
    }

    public void draw_text(StbGui.stbg_rect bounds, StbGui.stbg_render_text_parameters text_parameters)
    {
        var text = text_parameters.text.Span;
        var measure_options = text_parameters.measure_options;
        var style_ranges = text_parameters.style_ranges.Length > 0 ? text_parameters.style_ranges.Span : [text_parameters.single_style];
        bool single_line = (measure_options & StbGui.STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE) != 0;

        int style_index = 0;
        var style = style_ranges[style_index];
        var next_style_start_index = style_index + 1 < style_ranges.Length ? style_ranges[style_index + 1].start_index : int.MaxValue;

        var x = (int)bounds.x0;
        var y = (int)bounds.y0;

        for (var text_index = 0; text_index < text.Length; text_index++)
        {
            char character = text[text_index];

            if (text_index == next_style_start_index)
            {
                style_index++;
                style = style_ranges[style_index];
                next_style_start_index = style_index + 1 < style_ranges.Length ? style_ranges[style_index + 1].start_index : int.MaxValue;
            }

            if (character == '\n')
            {
                if (single_line)
                {
                    character = ' ';
                }
                else
                {
                    x = (int)bounds.x0;
                    y++;
                    continue;
                }
            }

            if (x < bounds.x1 && y < bounds.y1)
            {
                if (style.background_color != StbGui.STBG_COLOR_TRANSPARENT)
                    screen.SetTestRenderScreenPixelCharacterAndColor(x, y, character, style.text_color, style.background_color);
                else
                    screen.SetTestRenderScreenPixelCharacterAndColor(x, y, character, style.text_color);
            }
            x++;
        }
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
        return "test screen";
    }

    public StbGui.stbg_size measure_text(ReadOnlySpan<char> text, StbGui.stbg_font font, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS measure_options)
    {
        return new StbGui.stbg_size() { width = text.Length * style.size, height = style.size };
    }

    public void pop_clip_rect()
    {
        screen.PopClippingRect();
    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {
        screen.PushClippingRect(rect);
    }

    public void register_font(int font_id, StbGui.stbg_register_font_parameters parameters, byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public void register_image(int image_id, StbGui.stbg_register_image_parameters parameters, byte[] pixels, int width, int height, int bytes_per_pixel)
    {
        throw new NotImplementedException();
    }

    public void render_begin_frame(StbGui.stbg_color background_color)
    {
        var clear_pixel = new TestRenderScreenPixel() { character = ' ', background_color = background_color };
        for (int y = 0; y < screen.Height; y++)
        {
            for (int x = 0; x < screen.Width; x++)
            {
                screen.SetTestRenderScreenPixel(x, y, clear_pixel);
            }
        }
    }

    public void render_end_frame()
    {
    }
}