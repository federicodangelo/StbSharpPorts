using SDL3;

namespace StbSharp.Examples;

public class SDLFont : Font, IDisposable
{
    private nint font_texture;

    static private SDL.Vertex[] tmp_vertex = new SDL.Vertex[MAX_VERTEX_COUNT];

    public SDLFont(string name, string fileName, float fontSize, int oversampling, bool use_bilinear_filtering, nint renderer) : base(name, fileName, fontSize, oversampling)
    {
        font_texture = SDL.CreateTexture(renderer, SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Static, font_pixels_width, font_pixels_height);

        if (use_bilinear_filtering)
            SDL.SetTextureScaleMode(font_texture, SDL.ScaleMode.Linear);
        else
            SDL.SetTextureScaleMode(font_texture, SDL.ScaleMode.Nearest);

        if (font_texture == 0)
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to create texture: {SDL.GetError()}");
            return;
        }

        if (!SDL.UpdateTexture(font_texture, new SDL.Rect() { W = font_pixels_width, H = font_pixels_height, }, font_pixels, font_pixels_width * 4))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to update texture: {SDL.GetError()}");
            return;
        }

        draw_vertices = (vertices, count, use_texture) =>
        {
            var tmp = tmp_vertex.AsSpan(0, count);

            if (use_texture)
            {
                float pixels_width = font_pixels_width;
                float pixels_height = font_pixels_height;

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

            SDL.RenderGeometry(renderer, use_texture ? font_texture : 0, tmp_vertex, count, 0, 0);
        };
        push_clip_rect = (rect) => SDLHelper.PushClipRect(renderer, rect);
        pop_clip_rect = () => SDLHelper.PopClipRect(renderer);
    }

    public void Dispose()
    {
        if (font_texture != 0)
        {
            SDL.DestroyTexture(font_texture);
            font_texture = 0;
        }
    }
}