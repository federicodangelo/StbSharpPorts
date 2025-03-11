using StbSharp;

public class DummyRenderAdapter : StbGuiRenderAdapter
{
    public nint create_texture(int width, int height, byte[] pixels, int bytes_per_pixel, StbGuiRenderAdapter.CreateTextureOptions options = default)
    {
        return 1;
    }

    public void destroy_texture(nint texture_id)
    {
    }

    public void draw_texture_rects(StbGuiRenderAdapter.Rect[] rects, int count, nint texture_id)
    {
    }

    public string get_render_backend()
    {
        return "";
    }

    public void pop_clip_rect()
    {
    }

    public void process_render_command(StbGui.stbg_render_command cmd)
    {
    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {
    }

    public void register_font(int font_id, StbGuiFont font)
    {
    }

    public void register_image(int image_id, byte[] pixels, int width, int height, int bytes_per_pixel)
    {
        throw new NotImplementedException();
    }
}
