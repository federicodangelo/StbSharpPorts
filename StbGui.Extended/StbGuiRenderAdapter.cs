namespace StbSharp;

public struct Vertex
{
    public StbGui.stbg_position position;
    public StbGui.stbg_position tex_coord;
    public StbGui.stbg_color color;
}

public interface StbGuiRenderAdapter
{

    public void draw_vertices(Vertex[] vertices, int count, nint texture_id);

    public void push_clip_rect(StbGui.stbg_rect rect);

    public void pop_clip_rect();


    public struct CreateTextureOptions
    {
        public bool use_bilinear_filtering;
    }

    public nint create_texture(int width, int height, CreateTextureOptions options = default);

    public void set_texture_pixels(nint texture_id, StbGui.stbg_size size, byte[] pixels);

    public void destroy_texture(nint texture_id);
}