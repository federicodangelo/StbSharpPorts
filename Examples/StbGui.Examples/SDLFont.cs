using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SDL3;

namespace StbSharp.Examples;

public class SDLFont : IDisposable
{
    public readonly string Name;
    public readonly float Size = 13;

    private StbTrueType.stbtt_fontinfo font;
    private float fontScale;

    public readonly float Ascent;
    public readonly float Descent;
    public readonly float LineGap;
    public readonly float LineHeight;
    public readonly float Baseline;

    private bool use_bilinear_filtering = false;
    private int oversampling = 1;
    private float oversampling_scale = 1.0f;

    const int FONT_BITMAP_SIZE = 512;
    private nint fontTexture;
    private StbTrueType.stbtt_packedchar[] fontCharData;

    private nint renderer;

    public SDLFont(string name, string fileName, float fontSize, int oversampling, bool use_bilinear_filtering, nint renderer)
    {
        Name = name;
        Size = fontSize;
        this.renderer = renderer;

        this.oversampling = oversampling;
        this.oversampling_scale = 1.0f / (float)oversampling;
        this.use_bilinear_filtering = use_bilinear_filtering;

        byte[] fontBytes = File.ReadAllBytes(fileName);
        StbTrueType.stbtt_InitFont(out font, fontBytes, 0);

        fontScale = StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize);
        StbTrueType.stbtt_GetFontVMetrics(ref font, out int ascent, out int descent, out int lineGap);

        Debug.WriteLine($"Font: {fileName}, Size: {fontSize}, Scale: {fontScale} Ascent: {ascent}, Descent: {descent}, LineGap: {lineGap}");

        Ascent = ascent * fontScale;
        Descent = descent * fontScale;
        LineGap = lineGap * fontScale;
        LineHeight = (ascent - descent + lineGap) * fontScale;

        Baseline = (int)Ascent;

        InitFontTexture(fontBytes);
    }

    [MemberNotNull(nameof(fontCharData))]
    private void InitFontTexture(byte[] fontBytes)
    {
        var fontBitmap = new byte[FONT_BITMAP_SIZE * FONT_BITMAP_SIZE];

        int rangeFrom = 0;
        int rangeTo = 255;

        int rangeSize = rangeTo - rangeFrom;

        StbTrueType.stbtt_pack_range[] packRanges = new StbTrueType.stbtt_pack_range[1];
        packRanges[0].font_size = Size;
        packRanges[0].first_unicode_codepoint_in_range = rangeFrom;
        packRanges[0].num_chars = rangeSize;
        packRanges[0].chardata_for_range = new StbTrueType.stbtt_packedchar[rangeSize];

        StbTrueType.stbtt_PackBegin(out var spc, fontBitmap, FONT_BITMAP_SIZE, FONT_BITMAP_SIZE, 0, 1);

        StbTrueType.stbtt_PackSetOversampling(ref spc, (uint)oversampling, (uint)oversampling);
        StbTrueType.stbtt_PackSetSkipMissingCodepoints(ref spc, true);

        StbTrueType.stbtt_PackFontRanges(ref spc, fontBytes, 0, packRanges, 1);

        StbTrueType.stbtt_PackEnd(ref spc);

        fontCharData = packRanges[0].chardata_for_range;

        byte[] fontPixels = new byte[FONT_BITMAP_SIZE * FONT_BITMAP_SIZE * 4];

        int fontBitmapOffset = 0;
        int fontTextureOffset = 0;

        for (int y = 0; y < FONT_BITMAP_SIZE; y++)
        {
            for (int x = 0; x < FONT_BITMAP_SIZE; x++)
            {
                byte pixel = fontBitmap[fontBitmapOffset];

                fontPixels[fontTextureOffset + 0] = pixel;
                fontPixels[fontTextureOffset + 1] = pixel;
                fontPixels[fontTextureOffset + 2] = pixel;
                fontPixels[fontTextureOffset + 3] = pixel;

                fontBitmapOffset++;
                fontTextureOffset += 4;
            }
        }


        fontTexture = SDL.CreateTexture(renderer, SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Static, FONT_BITMAP_SIZE, FONT_BITMAP_SIZE);

        if (use_bilinear_filtering)
            SDL.SetTextureScaleMode(fontTexture, SDL.ScaleMode.Linear);
        else
            SDL.SetTextureScaleMode(fontTexture, SDL.ScaleMode.Nearest);

        if (fontTexture == 0)
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to create texture: {SDL.GetError()}");
            return;
        }

        if (!SDL.UpdateTexture(fontTexture, new SDL.Rect() { W = FONT_BITMAP_SIZE, H = FONT_BITMAP_SIZE, }, fontPixels, FONT_BITMAP_SIZE * 4))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to update texture: {SDL.GetError()}");
            return;
        }
    }

    private ref struct TextIterationData
    {
        public float scale;
        public char c;
        public int c_index;
        public ref StbTrueType.stbtt_packedchar c_data;
        public float x;
        public float y;
        public float dx;
        public float dy;
        public int line_number;
        public bool new_line;
        public bool final;
        public StbGui.stbg_color text_color;
        public StbGui.stbg_color background_color;
    }

    private delegate bool IterateTextInternalDelegate<T>(ref TextIterationData data, ref T user_data);

    private void IterateTextInternal<T>(
        ReadOnlySpan<char> text, 
        float font_size,
        ReadOnlySpan<StbGui.stbg_render_text_style_range> style_ranges,
        StbGui.STBG_MEASURE_TEXT_OPTIONS options,
        ref T user_data,
        IterateTextInternalDelegate<T> callback)
    {
        if (text.Length == 0)
        {
            var empty = new TextIterationData
            {
                c_data = ref fontCharData[(int) ' '],
                final = true
            };
            callback(ref empty, ref user_data);
            return;
        }            

        Debug.Assert(style_ranges.Length > 0);

        TextIterationData iteration_data = new TextIterationData();
        iteration_data.scale = font_size / Size;

        float current_line_height = 0;

        var ignore_metrics = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS) != 0;
        var use_only_baseline_for_first_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE) != 0;
        var single_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE) != 0;

        float get_line_height(ref TextIterationData data)
        {
            return (ignore_metrics && current_line_height != 0) ? current_line_height :
                (use_only_baseline_for_first_line && data.line_number == 0) ? Baseline * data.scale :
                LineHeight * data.scale;
        }

        bool stop = false;

        int style_index = 0;
        var style = style_ranges[style_index];
        var next_style_start_index = style_index + 1 < style_ranges.Length ? style_ranges[style_index + 1].start_index : int.MaxValue;
        iteration_data.text_color = style.text_color;
        iteration_data.background_color = style.background_color;

        while (iteration_data.c_index < text.Length && !stop)
        {
            iteration_data.c = text[iteration_data.c_index];
            iteration_data.new_line = false;
            iteration_data.dy = 0;
            iteration_data.dx = 0;

            if (iteration_data.c_index == next_style_start_index)
            {
                style_index++;
                style = style_ranges[style_index];
                next_style_start_index = style_index + 1 < style_ranges.Length ? style_ranges[style_index + 1].start_index : int.MaxValue;
                iteration_data.text_color = style.text_color;
                iteration_data.background_color = style.background_color;
            }

            if (iteration_data.c < 0 || iteration_data.c > fontCharData.Length)
            {
                // Replace characters in unknown range with space character
                iteration_data.c = ' ';
            }

            iteration_data.c_data = ref fontCharData[iteration_data.c];

            if (iteration_data.c == '\n')
            {
                if (!single_line)
                {
                    iteration_data.new_line = true;
                    iteration_data.dy = get_line_height(ref iteration_data);
                }
                else
                {
                    // ignore the new line if we are in a single line scenario
                }
            }
            else
            {
                if (ignore_metrics)
                {
                    current_line_height = Math.Max(current_line_height, (iteration_data.c_data.y1 - iteration_data.c_data.y0) * iteration_data.scale * oversampling_scale);
                    iteration_data.dx = ((iteration_data.c_data.x1 - iteration_data.c_data.x0) + 1) * iteration_data.scale * oversampling_scale;
                }
                else
                {
                    iteration_data.dx = iteration_data.c_data.xadvance * iteration_data.scale;
                    if (iteration_data.c_index + 1 < text.Length)
                        iteration_data.dx += fontScale * StbTrueType.stbtt_GetCodepointKernAdvance(ref font, iteration_data.c, text[iteration_data.c_index + 1]) * iteration_data.scale;
                }
            }

            if (iteration_data.new_line)
            {
                iteration_data.x = 0;

                current_line_height = 0;

                iteration_data.line_number++;

                iteration_data.y += iteration_data.dy;
            }

            stop = callback(ref iteration_data, ref user_data);

            iteration_data.x += iteration_data.dx;

            iteration_data.c_index++;
        }

        if (!stop)
        {
            // Call callback one last time with the final X/Y position 
            var last_line_height = get_line_height(ref iteration_data);
            iteration_data.final = true;
            iteration_data.y += last_line_height;
            iteration_data.dy = last_line_height;
            iteration_data.dx = 0;
            iteration_data.new_line = false;

            callback(ref iteration_data, ref user_data);
        }
    }

    static private bool MeasureTextCallback(ref TextIterationData data, ref StbGui.stbg_size size)
    {
        if (data.new_line || data.final)
        {
            size.width = Math.Max(size.width, data.x);
            size.height += data.dy;
        }

        return false;
    }


    public StbGui.stbg_size MeasureText(ReadOnlySpan<char> text, float font_size, ReadOnlySpan<StbGui.stbg_render_text_style_range> style_ranges, StbGui.STBG_MEASURE_TEXT_OPTIONS options)
    {
        StbGui.stbg_size size = new StbGui.stbg_size();

        IterateTextInternal(text, font_size, style_ranges, options, ref size, MeasureTextCallback);

        return StbGui.stbg_build_size(size.width, size.height);
    }

    private struct GetCharacterPositionInTextCallbackData
    {
        public int character_index;
        public StbGui.stbg_position position;
    }

    static private bool GetCharacterPositionInTextCallback(ref TextIterationData data, ref GetCharacterPositionInTextCallbackData callback_data)
    {
        if (data.c_index + 1 == callback_data.character_index)
        {
            callback_data.position.x = data.x + data.dx;
            callback_data.position.y = data.y;
            return true;
        }

        return false;
    }

    public StbGui.stbg_position GetCharacterPositionInText(ReadOnlySpan<char> text, float font_size, ReadOnlySpan<StbGui.stbg_render_text_style_range> style_ranges, StbGui.STBG_MEASURE_TEXT_OPTIONS options, int character_index)
    {
        GetCharacterPositionInTextCallbackData callback_data = new()
        {
            character_index = character_index,
            position = new StbGui.stbg_position()
        };

        IterateTextInternal(text, font_size, style_ranges, options, ref callback_data, GetCharacterPositionInTextCallback);

        return callback_data.position;
    }

    private struct VertexBuffer
    {
        public SDL.Vertex[] buffer;
        public int index;
    }

    private struct DrawTextCallbackData
    {
        public StbGui.stbg_rect bounds;
        public float horizontal_alignment;
        public float vertical_alignment;
        public StbGui.stbg_position position;
        public bool ignore_metrics;
        public float scale;
        public float line_height;
        public float baseline;
        public float center_x_offset;
        public float center_y_offset;
        public float oversampling_scale;
        public nint renderer;
        public nint font_texture;
        public VertexBuffer text_vertex_buffer;
        public VertexBuffer background_vertex_buffer;
    }

    static private SDL.Vertex[] draw_text_vertices_buffer = new SDL.Vertex[8192];
    static private SDL.Vertex[] draw_text_vertices_buffer2 = new SDL.Vertex[8192];
    

    private const int RECTANGLE_VERTEX_COUNT = 6;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void AddVerticesToVertexBuffer(ref VertexBuffer vertex_buffer, SDL.Vertex v0, SDL.Vertex v1, SDL.Vertex v2, SDL.Vertex v3)
    {
        vertex_buffer.buffer[vertex_buffer.index++] = v0;
        vertex_buffer.buffer[vertex_buffer.index++] = v1;
        vertex_buffer.buffer[vertex_buffer.index++] = v2;
        vertex_buffer.buffer[vertex_buffer.index++] = v0;
        vertex_buffer.buffer[vertex_buffer.index++] = v2;
        vertex_buffer.buffer[vertex_buffer.index++] = v3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void AddDrawTextureToVertexBuffer(ref VertexBuffer vertex_buffer, SDL.FRect fromRect, SDL.FRect toRect, SDL.FColor color)
    {
        SDL.Vertex v0 =
        new()
        {
            Position = new SDL.FPoint() { X = toRect.X, Y = toRect.Y },
            TexCoord = new SDL.FPoint() { X = fromRect.X / FONT_BITMAP_SIZE, Y = fromRect.Y / FONT_BITMAP_SIZE },
            Color = color
        };
        SDL.Vertex v1 =
        new()
        {
            Position = new SDL.FPoint() { X = toRect.X + toRect.W, Y = toRect.Y },
            TexCoord = new SDL.FPoint() { X = (fromRect.X + fromRect.W) / FONT_BITMAP_SIZE, Y = fromRect.Y / FONT_BITMAP_SIZE },
            Color = color
        };
        SDL.Vertex v2 = new()
        {
            Position = new SDL.FPoint() { X = toRect.X + toRect.W, Y = toRect.Y + toRect.H },
            TexCoord = new SDL.FPoint() { X = (fromRect.X + fromRect.W) / FONT_BITMAP_SIZE, Y = (fromRect.Y + fromRect.H) / FONT_BITMAP_SIZE },
            Color = color
        };
        SDL.Vertex v3 = new()
        {
            Position = new SDL.FPoint() { X = toRect.X, Y = toRect.Y + toRect.H },
            TexCoord = new SDL.FPoint() { X = fromRect.X / FONT_BITMAP_SIZE, Y = (fromRect.Y + fromRect.H) / FONT_BITMAP_SIZE },
            Color = color
        };

        AddVerticesToVertexBuffer(ref vertex_buffer, v0, v1, v2, v3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void AddDrawRectToVertexBuffer(ref VertexBuffer vertex_buffer, SDL.FRect toRect, SDL.FColor color)
    {
        SDL.Vertex v0 =
        new()
        {
            Position = new SDL.FPoint() { X = toRect.X, Y = toRect.Y },
            Color = color
        };
        SDL.Vertex v1 =
        new()
        {
            Position = new SDL.FPoint() { X = toRect.X + toRect.W, Y = toRect.Y },
            Color = color
        };
        SDL.Vertex v2 = new()
        {
            Position = new SDL.FPoint() { X = toRect.X + toRect.W, Y = toRect.Y + toRect.H },
            Color = color
        };
        SDL.Vertex v3 = new()
        {
            Position = new SDL.FPoint() { X = toRect.X, Y = toRect.Y + toRect.H },
            Color = color
        };

        AddVerticesToVertexBuffer(ref vertex_buffer, v0, v1, v2, v3);
    }
    static private void FlushRenderBuffer(ref VertexBuffer vertex_buffer, nint renderer, nint texture)
    {
        if (vertex_buffer.index > 0)
        {
            if (!SDL.RenderGeometry(renderer, texture, vertex_buffer.buffer, vertex_buffer.index, 0, 0))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to render geometry: {SDL.GetError()}");
            }
            vertex_buffer.index = 0;
        }
    }

    static private bool DrawTextCallback(ref TextIterationData data, ref DrawTextCallbackData callback_data)
    {
        if (!data.new_line && !data.final)
        {
            ref var char_data = ref data.c_data;

            var metricsX = (callback_data.ignore_metrics ? 0 : char_data.xoff) * callback_data.scale;
            var metricsY = (callback_data.ignore_metrics ? 0 : (callback_data.baseline + char_data.yoff)) * callback_data.scale;

            float xpos = callback_data.bounds.x0 + callback_data.center_x_offset + data.x;
            float ypos = callback_data.bounds.y0 + callback_data.center_y_offset + data.y;

            var fromRect = new SDL.FRect() { X = char_data.x0, Y = char_data.y0, W = char_data.x1 - char_data.x0, H = char_data.y1 - char_data.y0 };
            var toRect = new SDL.FRect() { X = xpos + metricsX, Y = ypos + metricsY, W = (char_data.x1 - char_data.x0) * callback_data.scale * callback_data.oversampling_scale, H = (char_data.y1 - char_data.y0) * callback_data.scale * callback_data.oversampling_scale };

            if (callback_data.text_vertex_buffer.index + RECTANGLE_VERTEX_COUNT > callback_data.text_vertex_buffer.buffer.Length)
            {
                FlushRenderBuffer(ref callback_data.background_vertex_buffer, callback_data.renderer, 0);
                FlushRenderBuffer(ref callback_data.text_vertex_buffer, callback_data.renderer, callback_data.font_texture);
            }

            AddDrawTextureToVertexBuffer(ref callback_data.text_vertex_buffer, fromRect, toRect, new SDL.FColor() { R = data.text_color.r / 255.0f, G = data.text_color.g / 255.0f, B = data.text_color.b / 255.0f, A = data.text_color.a / 255.0f });
            if (data.background_color.a > 0)
            {
                var background_rect = new SDL.FRect() { X = xpos, Y = ypos - 1, W = data.dx, H = callback_data.line_height + 2 };

                AddDrawRectToVertexBuffer(ref callback_data.background_vertex_buffer, background_rect, new SDL.FColor() { R = data.background_color.r / 255.0f, G = data.background_color.g / 255.0f, B = data.background_color.b / 255.0f, A = data.background_color.a / 255.0f });
            }
        }
        else if (data.final)
        {
            FlushRenderBuffer(ref callback_data.background_vertex_buffer, callback_data.renderer, 0);
            FlushRenderBuffer(ref callback_data.text_vertex_buffer, callback_data.renderer, callback_data.font_texture);
        }

        return false;
    }

    public void DrawText(StbGui.stbg_render_text_parameters parameters, StbGui.stbg_rect bounds)
    {
        float scale = parameters.font_size / Size;

        var bounds_width = bounds.x1 - bounds.x0;
        var bounds_height = bounds.y1 - bounds.y0;

        if (bounds_width <= 0 || bounds_height <= 0)
            return;

        ReadOnlySpan<StbGui.stbg_render_text_style_range> style_ranges = 
            parameters.style_ranges.Length > 0 ? 
                parameters.style_ranges.Span : 
                stackalloc StbGui.stbg_render_text_style_range[1] { parameters.single_style };

        var full_text_bounds = MeasureText(parameters.text.Span, parameters.font_size, style_ranges, parameters.measure_options);

        bool dont_clip = (parameters.render_options & StbGui.STBG_RENDER_TEXT_OPTIONS.DONT_CLIP) != 0;

        var center_y_offset = full_text_bounds.height < bounds_height ?
            MathF.Floor(((bounds_height - full_text_bounds.height) / 2) * (1 + parameters.vertical_alignment)) :
            0;

        var center_x_offset = full_text_bounds.width < bounds_width ?
            MathF.Floor(((bounds_width - full_text_bounds.width) / 2) * (1 + parameters.horizontal_alignment)) :
            0;

        var ignore_metrics = (parameters.measure_options & StbGui.STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS) != 0;
        var use_clipping = !dont_clip && (full_text_bounds.height > bounds_height || full_text_bounds.width > bounds_width);

        if (use_clipping)
        {
            SDLHelper.PushClipRect(renderer, bounds);
        }

        DrawTextCallbackData callback_data = new()
        {
            bounds = bounds,
            horizontal_alignment = parameters.horizontal_alignment,
            vertical_alignment = parameters.vertical_alignment,
            position = new StbGui.stbg_position(),
            ignore_metrics = ignore_metrics,
            scale = scale,
            baseline = Baseline,
            center_x_offset = center_x_offset,
            center_y_offset = center_y_offset,
            oversampling_scale = oversampling_scale,
            renderer = renderer,
            font_texture = fontTexture,
            line_height = parameters.font_size,
            text_vertex_buffer = new VertexBuffer()
            {
                buffer = draw_text_vertices_buffer,
                index = 0
            },
            background_vertex_buffer = new VertexBuffer()
            {
                buffer = draw_text_vertices_buffer2,
                index = 0
            },
        };

        IterateTextInternal(parameters.text.Span, parameters.font_size, style_ranges, parameters.measure_options, ref callback_data, DrawTextCallback);

        if (use_clipping)
        {
            SDLHelper.PopClipRect(renderer);
        }
    }

    public void Dispose()
    {
        if (fontTexture != 0)
        {
            SDL.DestroyTexture(fontTexture);
            fontTexture = 0;
        }
    }
}