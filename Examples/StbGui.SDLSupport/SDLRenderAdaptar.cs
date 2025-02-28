using SDL3;

namespace StbSharp.Examples;

public class SDLRenderAdapter : StbGuiRenderAdapter
{
    static private SDL.Vertex[] tmp_vertex = new SDL.Vertex[StbGuiTextHelper.MAX_VERTEX_COUNT];
    private nint renderer;

    public SDLRenderAdapter(nint renderer)
    {
        this.renderer = renderer;
    }

    public nint create_texture(int width, int height, StbGuiRenderAdapter.CreateTextureOptions options = default)
    {
        var texture_id = SDL.CreateTexture(renderer, SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Static, width, height);

        if (texture_id == 0)
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to create texture: {SDL.GetError()}");
            return 0;
        }

        if (options.use_bilinear_filtering)
            SDL.SetTextureScaleMode(texture_id, SDL.ScaleMode.Linear);
        else
            SDL.SetTextureScaleMode(texture_id, SDL.ScaleMode.Nearest);

        return texture_id;
    }

    public void set_texture_pixels(nint texture_id, StbGui.stbg_size size, byte[] pixels)
    {
        if (!SDL.UpdateTexture(texture_id, new SDL.Rect() { W = (int)size.width, H = (int)size.height, }, pixels, (int)size.width * 4))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to update texture: {SDL.GetError()}");
            return;
        }
    }

    public void destroy_texture(nint texture_id)
    {
        SDL.DestroyTexture(texture_id);
    }

    public void draw_vertices(Vertex[] vertices, int count, nint texture_id)
    {
        var tmp = tmp_vertex.AsSpan(0, count);

        if (texture_id != 0)
        {
            if (!SDL.GetTextureSize(texture_id, out float pixels_width, out float pixels_height))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to get texture size: {SDL.GetError()}");
                return;
            }

            for (int i = 0; i < tmp.Length; i++)
            {
                var c = vertices[i].color;
                var t = vertices[i].tex_coord;
                tmp[i].Color = new SDL.FColor() { R = c.r / 255.0f, G = c.g / 255.0f, B = c.b / 255.0f, A = c.a / 255.0f };
                tmp[i].TexCoord = new SDL.FPoint() { X = t.x / pixels_width, Y = t.y / pixels_height };
                tmp[i].Position = new SDL.FPoint() { X = vertices[i].position.x, Y = vertices[i].position.y };
            }
        }
        else
        {
            for (int i = 0; i < tmp.Length; i++)
            {
                var c = vertices[i].color;
                tmp[i].Color = new SDL.FColor() { R = c.r / 255.0f, G = c.g / 255.0f, B = c.b / 255.0f, A = c.a / 255.0f };
                tmp[i].Position = new SDL.FPoint() { X = vertices[i].position.x, Y = vertices[i].position.y };
            }
        }

        SDL.RenderGeometry(renderer, texture_id, tmp_vertex, count, 0, 0);
    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {
        SDLHelper.PushClipRect(renderer, rect);
    }

    public void pop_clip_rect()
    {
        SDLHelper.PopClipRect(renderer);
    }
}
