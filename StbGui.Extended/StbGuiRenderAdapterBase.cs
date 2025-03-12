#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

public abstract class StbGuiRenderAdapterBase : StbGuiRenderAdapter
{
    private readonly Dictionary<int, StbGuiFont> fonts = new();

    private readonly Dictionary<int, nint> images = new();

    private readonly StbGuiRenderAdapter.Rect[] tmp_rect = new StbGuiRenderAdapter.Rect[1];

    public void register_font(int font_id, StbGuiFont font)
    {
        fonts[font_id] = font;
    }


    public void register_image(int image_id, byte[] pixels, int width, int height, int bytes_per_pixel)
    {
        nint texture_id = create_texture(width, height, pixels, bytes_per_pixel, new() { use_bilinear_filtering = false });
        images[image_id] = texture_id;
    }

    public void process_render_command(StbGui.stbg_render_command cmd)
    {
        switch (cmd.type)
        {
            case StbGui.STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME:
                {
                    var background_color = cmd.background_color;
                    render_begin_frame(background_color);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.END_FRAME:
                render_end_frame();
                break;


            case StbGui.STBG_RENDER_COMMAND_TYPE.BORDER:
                {
                    var color = cmd.color;
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;
                    var border_size = (int)Math.Ceiling(cmd.size);

                    draw_border(bounds, border_size, background_color, color);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.RECTANGLE:
                {
                    var background_color = cmd.background_color;
                    var bounds = cmd.bounds;

                    draw_rectangle(bounds, background_color);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.LINE:
                {
                    var from = StbGui.stbg_build_position(cmd.bounds.x0, cmd.bounds.y0);
                    var to = StbGui.stbg_build_position(cmd.bounds.x1, cmd.bounds.y1);
                    var color = cmd.color;
                    var thickness = cmd.size;

                    draw_line(from, to, color, thickness);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.TEXT:
                {
                    var bounds = cmd.bounds;
                    var text = cmd.text;

                    var font = fonts[text.font_id];

                    StbGuiTextHelper.draw_text(text, bounds, font, this);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.IMAGE:
                {
                    var image_id = cmd.image_id;
                    var color = cmd.color;
                    var bounds = cmd.bounds;
                    var source_rect = cmd.source_rect;
                    var texture_id = images[image_id];

                    tmp_rect[0] = new StbGuiRenderAdapter.Rect()
                    {
                        rect = bounds,
                        tex_coord_rect = source_rect,
                        color = color
                    };

                    draw_texture_rects(tmp_rect, 1, texture_id);
                    break;
                }

            case StbGui.STBG_RENDER_COMMAND_TYPE.PUSH_CLIPPING_RECT:
                push_clip_rect(cmd.bounds);
                break;

            case StbGui.STBG_RENDER_COMMAND_TYPE.POP_CLIPPING_RECT:
                pop_clip_rect();
                break;
        }
    }

    protected abstract void render_begin_frame(StbGui.stbg_color background_color);

    protected abstract void render_end_frame();

    protected abstract void draw_rectangle(StbGui.stbg_rect bounds, StbGui.stbg_color background_color);

    protected abstract void draw_border(StbGui.stbg_rect bounds, int border_size, StbGui.stbg_color background_color, StbGui.stbg_color color);

    public abstract void draw_texture_rects(StbGuiRenderAdapter.Rect[] rects, int count, nint texture_id);

    protected abstract void draw_line(StbGui.stbg_position from, StbGui.stbg_position to, StbGui.stbg_color color, float thickness);

    public abstract void push_clip_rect(StbGui.stbg_rect rect);

    public abstract void pop_clip_rect();

    public abstract nint create_texture(int width, int height, byte[] pixels, int bytes_per_pixel, StbGuiRenderAdapter.CreateTextureOptions options);

    public abstract void destroy_texture(nint texture_id);

    public abstract string get_render_backend();
}
