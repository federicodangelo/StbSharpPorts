using System.Reflection.Metadata;
using StbSharp;

class WARenderAdapter : StbGuiRenderAdapter
{
    public nint create_texture(int width, int height, StbGuiRenderAdapter.CreateTextureOptions options = default)
    {
        //throw new NotImplementedException();
        return 0;
    }

    public void destroy_texture(nint texture_id)
    {
        //throw new NotImplementedException();
    }

    public void draw_rects(StbGuiRenderAdapter.Rect[] rects, int count, nint texture_id)
    {
        //throw new NotImplementedException();
    }

    public void draw_vertices(StbGuiRenderAdapter.Vertex[] vertices, int count, nint texture_id)
    {
        //throw new NotImplementedException();
    }

    public void pop_clip_rect()
    {
        //throw new NotImplementedException();
    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {
        //throw new NotImplementedException();
    }

    public void set_texture_pixels(nint texture_id, StbGui.stbg_size size, byte[] pixels)
    {
        //throw new NotImplementedException();
    }
}