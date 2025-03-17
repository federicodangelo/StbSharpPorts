#pragma warning disable IDE1006 // Naming Styles

using System.Diagnostics;

using SDL3;

using font_id = int;
using image_id = int;

namespace StbSharp.Examples;

public class SDLRenderAdapter : StbGui.stbg_render_adapter
{
    private const int RECTANGLE_VERTEX_COUNT = 4;
    private const int RECTANGLE_INDICES_COUNT = 6;

    private readonly Dictionary<font_id, StbGuiFont> fonts = [];
    private readonly Dictionary<image_id, nint> images = [];

    private static readonly SDL.Vertex[] tmp_vertex = new SDL.Vertex[StbGuiTextHelper.MAX_RECTS_COUNT * RECTANGLE_VERTEX_COUNT];
    private static readonly int[] tmp_indices = new int[StbGuiTextHelper.MAX_RECTS_COUNT * RECTANGLE_INDICES_COUNT];
    private readonly nint renderer;

    private static readonly Stack<StbGui.stbg_rect> clip_rects = new();

    public SDLRenderAdapter(nint renderer)
    {
        this.renderer = renderer;
    }

    public string get_render_backend()
    {
        return "SDL3";
    }

    public void register_font(int font_id, StbGui.stbg_register_font_parameters parameters, byte[] bytes)
    {
        StbGuiFont font = new StbGuiFont(parameters.name, bytes, parameters.size, parameters.oversampling, parameters.bilinear_filtering, this);
        fonts.Add(font_id, font);
    }

    public void register_image(int image_id, StbGui.stbg_register_image_parameters parameters, byte[] pixels, int width, int height, int bytes_per_pixel)
    {
        var format = bytes_per_pixel == 4 ? SDL.PixelFormat.RGBA8888 : SDL.PixelFormat.RGB24;

        var texture_id = SDL.CreateTexture(renderer, format, SDL.TextureAccess.Static, width, height);

        if (texture_id == 0)
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to create texture: {SDL.GetError()}");
            return;
        }

        if (parameters.bilinear_filtering)
            SDL.SetTextureScaleMode(texture_id, SDL.ScaleMode.Linear);
        else
            SDL.SetTextureScaleMode(texture_id, SDL.ScaleMode.Nearest);

        SDL.UpdateTexture(texture_id, new SDL.Rect() { W = width, H = height, }, pixels, width * bytes_per_pixel);

        images.Add(image_id, texture_id);
    }

    public void draw_text(StbGui.stbg_rect bounds, StbGui.stbg_render_text_parameters text_parameters)
    {
        var font = fonts[text_parameters.font_id];
        StbGuiTextHelper.draw_text(text_parameters, bounds, font, this);
    }

    public void draw_image(StbGui.stbg_rect bounds, StbGui.stbg_rect image_rect, StbGui.stbg_color color, int image_id)
    {
        Span<StbGui.stbg_draw_image_rect> tmp_rect = stackalloc StbGui.stbg_draw_image_rect[1];

        tmp_rect[0].bounds = bounds;
        tmp_rect[0].image_rect = image_rect;
        tmp_rect[0].color = color;

        draw_images(tmp_rect, image_id);
    }

    public void draw_images(Span<StbGui.stbg_draw_image_rect> rects, int image_id)
    {
        nint texture_id = image_id != 0 ? images[image_id] : 0;

        var tmp_v = tmp_vertex;
        var tmp_i = tmp_indices;
        int vertex_index = 0;
        int indices_index = 0;

        if (image_id != 0)
        {
            if (!SDL.GetTextureSize(texture_id, out float pixels_width, out float pixels_height))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to get texture size: {SDL.GetError()}");
                return;
            }

            for (int i = 0; i < rects.Length; i++)
            {
                ref var r = ref rects[i];

                var rect = r.bounds;
                var tex_coord_rect = r.image_rect;
                var color = new SDL.FColor() { R = r.color.r / 255.0f, G = r.color.g / 255.0f, B = r.color.b / 255.0f, A = r.color.a / 255.0f };

                SDL.Vertex v0 = new()
                {
                    Position = new SDL.FPoint() { X = rect.x0, Y = rect.y0 },
                    TexCoord = new SDL.FPoint() { X = tex_coord_rect.x0 / pixels_width, Y = tex_coord_rect.y0 / pixels_height },
                    Color = color,
                };
                SDL.Vertex v1 = new()
                {
                    Position = new SDL.FPoint() { X = rect.x1, Y = rect.y0 },
                    TexCoord = new SDL.FPoint() { X = tex_coord_rect.x1 / pixels_width, Y = tex_coord_rect.y0 / pixels_height },
                    Color = color,
                };
                SDL.Vertex v2 = new()
                {
                    Position = new SDL.FPoint() { X = rect.x1, Y = rect.y1 },
                    TexCoord = new SDL.FPoint() { X = tex_coord_rect.x1 / pixels_width, Y = tex_coord_rect.y1 / pixels_height },
                    Color = color,
                };
                SDL.Vertex v3 = new()
                {
                    Position = new SDL.FPoint() { X = rect.x0, Y = rect.y1 },
                    TexCoord = new SDL.FPoint() { X = tex_coord_rect.x0 / pixels_width, Y = tex_coord_rect.y1 / pixels_height },
                    Color = color,
                };

                tmp_v[vertex_index + 0] = v0;
                tmp_v[vertex_index + 1] = v1;
                tmp_v[vertex_index + 2] = v2;
                tmp_v[vertex_index + 3] = v3;

                tmp_i[indices_index + 0] = vertex_index + 0;
                tmp_i[indices_index + 1] = vertex_index + 1;
                tmp_i[indices_index + 2] = vertex_index + 2;
                tmp_i[indices_index + 3] = vertex_index + 0;
                tmp_i[indices_index + 4] = vertex_index + 2;
                tmp_i[indices_index + 5] = vertex_index + 3;

                vertex_index += RECTANGLE_VERTEX_COUNT;
                indices_index += RECTANGLE_INDICES_COUNT;
            }
        }
        else
        {
            for (int i = 0; i < rects.Length; i++)
            {
                ref var r = ref rects[i];

                var rect = r.bounds;
                var color = new SDL.FColor() { R = r.color.r / 255.0f, G = r.color.g / 255.0f, B = r.color.b / 255.0f, A = r.color.a / 255.0f };

                SDL.Vertex v0 = new()
                {
                    Position = new SDL.FPoint() { X = rect.x0, Y = rect.y0 },
                    Color = color,
                };
                SDL.Vertex v1 = new()
                {
                    Position = new SDL.FPoint() { X = rect.x1, Y = rect.y0 },
                    Color = color,
                };
                SDL.Vertex v2 = new()
                {
                    Position = new SDL.FPoint() { X = rect.x1, Y = rect.y1 },
                    Color = color,
                };
                SDL.Vertex v3 = new()
                {
                    Position = new SDL.FPoint() { X = rect.x0, Y = rect.y1 },
                    Color = color,
                };

                tmp_v[vertex_index + 0] = v0;
                tmp_v[vertex_index + 1] = v1;
                tmp_v[vertex_index + 2] = v2;
                tmp_v[vertex_index + 3] = v3;

                tmp_i[indices_index + 0] = vertex_index + 0;
                tmp_i[indices_index + 1] = vertex_index + 1;
                tmp_i[indices_index + 2] = vertex_index + 2;
                tmp_i[indices_index + 3] = vertex_index + 0;
                tmp_i[indices_index + 4] = vertex_index + 2;
                tmp_i[indices_index + 5] = vertex_index + 3;

                vertex_index += RECTANGLE_VERTEX_COUNT;
                indices_index += RECTANGLE_INDICES_COUNT;
            }
        }

        SDL.RenderGeometry(renderer, texture_id, tmp_v, vertex_index, tmp_i, indices_index);
    }

    public void push_clip_rect(StbGui.stbg_rect rect)
    {
        var prev_clip = clip_rects.Count > 0 ? clip_rects.Peek() : StbGui.stbg_build_rect_infinite();

        var rect_clipped = StbGui.stbg_clamp_rect(rect, prev_clip);

        clip_rects.Push(rect_clipped);

        if (!SDL.SetRenderClipRect(renderer, StbgRectToSdlRect(rect_clipped)))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to set clip rect: {SDL.GetError()}");
        }
    }

    public void pop_clip_rect()
    {
        Debug.Assert(clip_rects.Count > 0);

        clip_rects.Pop();

        if (clip_rects.Count > 0)
        {
            if (!SDL.SetRenderClipRect(renderer, StbgRectToSdlRect(clip_rects.Peek())))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to set clip rect: {SDL.GetError()}");
            }
        }
        else
        {
            if (!SDL.SetRenderClipRect(renderer, 0))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to set clip rect: {SDL.GetError()}");
            }
        }
    }

    public static SDL.Rect StbgRectToSdlRect(StbGui.stbg_rect rect)
    {
        return new SDL.Rect { X = (int)rect.x0, Y = (int)rect.y0, W = (int)(rect.x1 - rect.x0), H = (int)(rect.y1 - rect.y0) };
    }

    public void render_begin_frame(StbGui.stbg_color background_color)
    {
        SDL.SetRenderDrawColor(renderer, background_color.r, background_color.g, background_color.b, background_color.a);
        SDL.RenderClear(renderer);
        SDL.SetRenderDrawBlendMode(renderer, SDL.BlendMode.Blend);
        SDL.SetTextureBlendMode(renderer, SDL.BlendMode.Blend);
    }

    public void render_end_frame()
    {
        Debug.Assert(clip_rects.Count == 0);
    }

    public void draw_rectangle(StbGui.stbg_rect bounds, StbGui.stbg_color background_color)
    {
        SDL.SetRenderDrawColor(renderer, background_color.r, background_color.g, background_color.b, background_color.a);
        SDL.RenderFillRect(renderer, new SDL.FRect() { X = bounds.x0, Y = bounds.y0, W = bounds.x1 - bounds.x0, H = bounds.y1 - bounds.y0 });
    }

    public void draw_border(StbGui.stbg_rect bounds, int border_size, StbGui.stbg_color border_color, StbGui.stbg_color background_color)
    {
        SDL.SetRenderDrawColor(renderer, background_color.r, background_color.g, background_color.b, background_color.a);
        SDL.RenderFillRect(renderer, new SDL.FRect() { X = bounds.x0, Y = bounds.y0, W = bounds.x1 - bounds.x0, H = bounds.y1 - bounds.y0 });
        SDL.SetRenderDrawColor(renderer, border_color.r, border_color.g, border_color.b, border_color.a);
        for (int i = 0; i < border_size; i++)
            SDL.RenderRect(renderer, new SDL.FRect() { X = bounds.x0 + i, Y = bounds.y0 + i, W = bounds.x1 - bounds.x0 - i * 2, H = bounds.y1 - bounds.y0 - i * 2 });
    }

    public void draw_line(StbGui.stbg_position from, StbGui.stbg_position to, StbGui.stbg_color color, float thickness)
    {
        //TODO: Use thickness
        SDL.SetRenderDrawColor(renderer, color.r, color.g, color.b, color.a);
        SDL.RenderLine(renderer, from.x, from.y, to.x, to.y);
    }

    public StbGui.stbg_size measure_text(ReadOnlySpan<char> text, StbGui.stbg_font font, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options)
    {
        Span<StbGui.stbg_render_text_style_range> tmp_styles = stackalloc StbGui.stbg_render_text_style_range[1];

        var real_font = fonts[font.id];

        tmp_styles[0] = new StbGui.stbg_render_text_style_range()
        {
            start_index = 0,
            text_color = style.color,
            font_style = style.style
        };

        return StbGuiTextHelper.measure_text(text, real_font, style.size, tmp_styles, options);
    }

    public StbGui.stbg_position get_character_position_in_text(ReadOnlySpan<char> text, StbGui.stbg_font font, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options, int character_index)
    {
        Span<StbGui.stbg_render_text_style_range> tmp_styles = stackalloc StbGui.stbg_render_text_style_range[1];

        var real_font = fonts[font.id];

        tmp_styles[0] = new StbGui.stbg_render_text_style_range()
        {
            start_index = 0,
            text_color = style.color,
            font_style = style.style
        };

        return StbGuiTextHelper.get_character_position_in_text(text, real_font, style.size, tmp_styles, options, character_index);
    }

    public void destroy()
    {
        foreach (var image in images.Values)
        {
            SDL.DestroyTexture(image);
        }

        fonts.Clear();
        images.Clear();
    }
}
