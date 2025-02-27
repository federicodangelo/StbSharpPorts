using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace StbSharp.Examples;

public class Font
{
    protected const int MAX_VERTEX_COUNT = 8192;

    public readonly string Name;
    public readonly float Size = 13;

    public readonly float Ascent;
    public readonly float Descent;
    public readonly float LineGap;
    public readonly float LineHeight;
    public readonly float Baseline;

    private StbTrueType.stbtt_fontinfo font;
    private float font_scale;
    private int oversampling = 1;
    private float oversampling_scale = 1.0f;
    private StbTrueType.stbtt_packedchar[] font_char_data;

    protected int font_pixels_width;
    protected int font_pixels_height;
    protected byte[] font_pixels;

    protected delegate void DrawVerticesDelegate(Vertex[] vertices, int count, bool use_texture);
    protected DrawVerticesDelegate draw_vertices = (_, _, _) => { };

    protected delegate void PushClipRectDelegate(StbGui.stbg_rect rect);
    protected PushClipRectDelegate push_clip_rect = _ => { };

    protected delegate void PopClipRectDelegate();
    protected PopClipRectDelegate pop_clip_rect = () => { };

    public Font(string name, string fileName, float fontSize, int oversampling)
    {
        Name = name;
        Size = fontSize;

        this.oversampling = oversampling;
        this.oversampling_scale = 1.0f / (float)oversampling;

        byte[] fontBytes = File.ReadAllBytes(fileName);
        StbTrueType.stbtt_InitFont(out font, fontBytes, 0);

        font_scale = StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize);
        StbTrueType.stbtt_GetFontVMetrics(ref font, out int ascent, out int descent, out int lineGap);

        Ascent = ascent * font_scale;
        Descent = descent * font_scale;
        LineGap = lineGap * font_scale;
        LineHeight = (ascent - descent + lineGap) * font_scale;

        Baseline = (int)Ascent;

        InitFontTexture(fontBytes);
    }

    [MemberNotNull(nameof(font_char_data), nameof(font_pixels))]
    private void InitFontTexture(byte[] fontBytes)
    {
        var width = 512;
        var height = 512;

        var fontBitmap = new byte[width * height];

        int rangeFrom = 0;
        int rangeTo = 255;

        int rangeSize = rangeTo - rangeFrom;

        StbTrueType.stbtt_pack_range[] packRanges = new StbTrueType.stbtt_pack_range[1];
        packRanges[0].font_size = Size;
        packRanges[0].first_unicode_codepoint_in_range = rangeFrom;
        packRanges[0].num_chars = rangeSize;
        packRanges[0].chardata_for_range = new StbTrueType.stbtt_packedchar[rangeSize];

        StbTrueType.stbtt_PackBegin(out var spc, fontBitmap, width, height, 0, 1);

        StbTrueType.stbtt_PackSetOversampling(ref spc, (uint)oversampling, (uint)oversampling);
        StbTrueType.stbtt_PackSetSkipMissingCodepoints(ref spc, true);

        StbTrueType.stbtt_PackFontRanges(ref spc, fontBytes, 0, packRanges, 1);

        StbTrueType.stbtt_PackEnd(ref spc);

        font_char_data = packRanges[0].chardata_for_range;

        var pixels = new byte[width * height * 4];

        int fontBitmapOffset = 0;
        int fontTextureOffset = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte pixel = fontBitmap[fontBitmapOffset];

                pixels[fontTextureOffset + 0] = pixel;
                pixels[fontTextureOffset + 1] = pixel;
                pixels[fontTextureOffset + 2] = pixel;
                pixels[fontTextureOffset + 3] = pixel;

                fontBitmapOffset++;
                fontTextureOffset += 4;
            }
        }

        font_pixels = pixels;
        font_pixels_width = width;
        font_pixels_height = height;
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
                c_data = ref font_char_data[(int)' '],
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

            if (iteration_data.c < 0 || iteration_data.c > font_char_data.Length)
            {
                // Replace characters in unknown range with space character
                iteration_data.c = ' ';
            }

            iteration_data.c_data = ref font_char_data[iteration_data.c];

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
                        iteration_data.dx += font_scale * StbTrueType.stbtt_GetCodepointKernAdvance(ref font, iteration_data.c, text[iteration_data.c_index + 1]) * iteration_data.scale;
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

    protected struct Vertex
    {
        public StbGui.stbg_position position;
        public StbGui.stbg_position tex_coord;
        public StbGui.stbg_color color;
    }

    protected struct VertexBuffer
    {
        public Vertex[] buffer;
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
        public VertexBuffer text_vertex_buffer;
        public VertexBuffer background_vertex_buffer;
        public DrawVerticesDelegate draw_vertices;
    }

    static private Vertex[] draw_text_vertices_buffer = new Vertex[MAX_VERTEX_COUNT];
    static private Vertex[] draw_text_vertices_buffer2 = new Vertex[MAX_VERTEX_COUNT];

    private const int RECTANGLE_VERTEX_COUNT = 6;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void AddVerticesToVertexBuffer(ref VertexBuffer vertex_buffer, Vertex v0, Vertex v1, Vertex v2, Vertex v3)
    {
        vertex_buffer.buffer[vertex_buffer.index++] = v0;
        vertex_buffer.buffer[vertex_buffer.index++] = v1;
        vertex_buffer.buffer[vertex_buffer.index++] = v2;
        vertex_buffer.buffer[vertex_buffer.index++] = v0;
        vertex_buffer.buffer[vertex_buffer.index++] = v2;
        vertex_buffer.buffer[vertex_buffer.index++] = v3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void AddDrawTextureToVertexBuffer(ref VertexBuffer vertex_buffer, StbGui.stbg_rect fromRect, StbGui.stbg_rect toRect, StbGui.stbg_color color)
    {
        Vertex v0 = new()
        {
            position = StbGui.stbg_build_position(toRect.x0, toRect.y0),
            tex_coord = StbGui.stbg_build_position(fromRect.x0, fromRect.y0),
            color = color,
        };
        Vertex v1 = new()
        {
            position = StbGui.stbg_build_position(toRect.x1, toRect.y0),
            tex_coord = StbGui.stbg_build_position(fromRect.x1, fromRect.y0),
            color = color,
        };
        Vertex v2 = new()
        {
            position = StbGui.stbg_build_position(toRect.x1, toRect.y1),
            tex_coord = StbGui.stbg_build_position(fromRect.x1, fromRect.y1),
            color = color,
        };
        Vertex v3 = new()
        {
            position = StbGui.stbg_build_position(toRect.x0, toRect.y1),
            tex_coord = StbGui.stbg_build_position(fromRect.x0, fromRect.y1),
            color = color,
        };


        AddVerticesToVertexBuffer(ref vertex_buffer, v0, v1, v2, v3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void AddDrawRectToVertexBuffer(ref VertexBuffer vertex_buffer, StbGui.stbg_rect toRect, StbGui.stbg_color color)
    {
        Vertex v0 = new()
        {
            position = StbGui.stbg_build_position(toRect.x0, toRect.y0),
            color = color,
        };
        Vertex v1 = new()
        {
            position = StbGui.stbg_build_position(toRect.x1, toRect.y0),
            color = color,
        };
        Vertex v2 = new()
        {
            position = StbGui.stbg_build_position(toRect.x1, toRect.y1),
            color = color,
        };
        Vertex v3 = new()
        {
            position = StbGui.stbg_build_position(toRect.x0, toRect.y1),
            color = color,
        };

        AddVerticesToVertexBuffer(ref vertex_buffer, v0, v1, v2, v3);
    }



    static private void FlushRenderBuffer(ref VertexBuffer vertex_buffer, bool use_texture, DrawVerticesDelegate draw_vertices)
    {
        if (vertex_buffer.index > 0)
        {
            draw_vertices(vertex_buffer.buffer, vertex_buffer.index, use_texture);
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

            var fromRect = StbGui.stbg_build_rect(char_data.x0, char_data.y0, char_data.x1, char_data.y1);
            var toRect = StbGui.stbg_build_rect(xpos + metricsX, ypos + metricsY, xpos + metricsX + (char_data.x1 - char_data.x0) * callback_data.scale * callback_data.oversampling_scale, ypos + metricsY + (char_data.y1 - char_data.y0) * callback_data.scale * callback_data.oversampling_scale);

            if (callback_data.text_vertex_buffer.index + RECTANGLE_VERTEX_COUNT > callback_data.text_vertex_buffer.buffer.Length)
            {
                FlushRenderBuffer(ref callback_data.background_vertex_buffer, false, callback_data.draw_vertices);
                FlushRenderBuffer(ref callback_data.text_vertex_buffer, true, callback_data.draw_vertices);
            }

            AddDrawTextureToVertexBuffer(ref callback_data.text_vertex_buffer, fromRect, toRect, data.text_color);
            if (data.background_color.a > 0)
            {
                var background_rect = StbGui.stbg_build_rect(xpos, ypos - 1, xpos + data.dx, ypos + callback_data.line_height + 1);

                AddDrawRectToVertexBuffer(ref callback_data.background_vertex_buffer, background_rect, data.background_color);
            }
        }
        else if (data.final)
        {
            FlushRenderBuffer(ref callback_data.background_vertex_buffer, false, callback_data.draw_vertices);
            FlushRenderBuffer(ref callback_data.text_vertex_buffer, true, callback_data.draw_vertices);
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
            push_clip_rect(bounds);
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
            draw_vertices = draw_vertices,
        };

        IterateTextInternal(parameters.text.Span, parameters.font_size, style_ranges, parameters.measure_options, ref callback_data, DrawTextCallback);

        if (use_clipping)
        {
            pop_clip_rect();
        }
    }
}