using StbSharp;

public class DummyRenderAdapter : StbGuiRenderAdapter
{
    public nint create_texture(int width, int height, StbGuiRenderAdapter.CreateTextureOptions options = default)
    {
        return 1;
    }

    public void destroy_texture(nint texture_id)
    {

    }

    public void draw_vertices(Vertex[] vertices, int count, nint texture_id)
    {

    }

    public void pop_clip_rect()
    {

    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {

    }

    public void set_texture_pixels(nint texture_id, StbGui.stbg_size size, byte[] pixels)
    {

    }
}