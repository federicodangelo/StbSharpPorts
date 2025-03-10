#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;


public interface StbGuiRenderAdapter
{
    public struct Vertex
    {
        public StbGui.stbg_position position;
        public StbGui.stbg_position tex_coord;
        public StbGui.stbg_color color;
    }

    public struct Rect
    {
        public StbGui.stbg_rect rect;
        public StbGui.stbg_rect tex_coord_rect;
        public StbGui.stbg_color color;
    }

    public void draw_texture_rects(Rect[] rects, int count, nint texture_id);

    public void push_clip_rect(StbGui.stbg_rect rect);

    public void pop_clip_rect();

    public struct CreateTextureOptions
    {
        public bool use_bilinear_filtering;
    }

    public nint create_texture(int width, int height, byte[] pixels, int bytes_per_pixel, CreateTextureOptions options = default);

    public void destroy_texture(nint texture_id);

    public void register_font(int font_id, StbGuiFont font);

    public void process_render_command(StbGui.stbg_render_command cmd);

    public void register_image(int image_id, byte[] pixels, int width, int height, int bytes_per_pixel);

    public string get_render_backend();
}