#pragma warning disable IDE1006 // Naming Styles

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace StbSharp;

public class StbGuiTextHelper
{
    public const int MAX_RECTS_COUNT = 1024;

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
        public float previous_line_width;
        public bool new_line;
        public bool final;
        public StbGui.stbg_color text_color;
        public StbGui.stbg_color background_color;
    }

    private delegate bool IterateTextInternalDelegate<T>(ref TextIterationData data, ref T user_data);

    static private void iterate_text_internal<T>(
        ReadOnlySpan<char> text,
        StbGuiFont font,
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
                c_data = ref font.font_char_data[(int)' '],
                final = true
            };
            callback(ref empty, ref user_data);
            return;
        }

        Debug.Assert(style_ranges.Length > 0);

        var font_char_data = font.font_char_data;

        TextIterationData iteration_data = new TextIterationData();
        iteration_data.scale = font_size / font.size;

        float current_line_height = 0;

        var ignore_metrics = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS) != 0;
        var use_only_baseline_for_first_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE) != 0;
        var single_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE) != 0;

        float get_line_height(ref TextIterationData data)
        {
            return (ignore_metrics && current_line_height != 0) ? current_line_height :
                (use_only_baseline_for_first_line && data.line_number == 0) ? font.baseline * data.scale :
                font.line_height * data.scale;
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
                    current_line_height = Math.Max(current_line_height, (iteration_data.c_data.y1 - iteration_data.c_data.y0) * iteration_data.scale * font.oversampling_scale);
                    iteration_data.dx = (iteration_data.c_data.x1 - iteration_data.c_data.x0) * iteration_data.scale * font.oversampling_scale;
                }
                else
                {
                    iteration_data.dx = iteration_data.c_data.xadvance * iteration_data.scale;
                    if (iteration_data.c_index + 1 < text.Length)
                        iteration_data.dx += font.font_scale * font.GetCodepointKernAdvance(iteration_data.c, text[iteration_data.c_index + 1]) * iteration_data.scale;
                }
            }

            if (iteration_data.new_line)
            {
                iteration_data.previous_line_width = iteration_data.x;

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

    static private bool measure_text_callback(ref TextIterationData data, ref StbGui.stbg_size size)
    {
        if (data.new_line || data.final)
        {
            size.width = Math.Max(Math.Max(size.width, data.previous_line_width), data.x);
            size.height += data.dy;
        }

        return false;
    }


    static public StbGui.stbg_size measure_text(ReadOnlySpan<char> text, StbGuiFont font, float font_size, ReadOnlySpan<StbGui.stbg_render_text_style_range> style_ranges, StbGui.STBG_MEASURE_TEXT_OPTIONS options)
    {
        StbGui.stbg_size size = new StbGui.stbg_size();

        iterate_text_internal(text, font, font_size, style_ranges, options, ref size, measure_text_callback);

        return StbGui.stbg_build_size(size.width, size.height);
    }

    private struct GetCharacterPositionInTextCallbackData
    {
        public int character_index;
        public StbGui.stbg_position position;
    }

    static private bool get_character_position_in_text_callback(ref TextIterationData data, ref GetCharacterPositionInTextCallbackData callback_data)
    {
        if (data.c_index + 1 == callback_data.character_index)
        {
            callback_data.position.x = data.x + data.dx;
            callback_data.position.y = data.y;
            return true;
        }

        return false;
    }

    static public StbGui.stbg_position get_character_position_in_text(ReadOnlySpan<char> text, StbGuiFont font, float font_size, ReadOnlySpan<StbGui.stbg_render_text_style_range> style_ranges, StbGui.STBG_MEASURE_TEXT_OPTIONS options, int character_index)
    {
        GetCharacterPositionInTextCallbackData callback_data = new()
        {
            character_index = character_index,
            position = new StbGui.stbg_position()
        };

        iterate_text_internal(text, font, font_size, style_ranges, options, ref callback_data, get_character_position_in_text_callback);

        return callback_data.position;
    }

    private struct RectsBuffer
    {
        public StbGuiRenderAdapter.Rect[] buffer;
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
        public nint font_texture_id;
        public RectsBuffer text_rect_buffer;
        public RectsBuffer background_rect_buffer;
        public StbGuiRenderAdapter render_adapter;
    }

    private static readonly StbGuiRenderAdapter.Rect[] draw_text_rects_buffer = new StbGuiRenderAdapter.Rect[MAX_RECTS_COUNT];
    private static readonly StbGuiRenderAdapter.Rect[] draw_text_rects_buffer2 = new StbGuiRenderAdapter.Rect[MAX_RECTS_COUNT];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void add_draw_texture_to_rect_buffer(ref RectsBuffer rect_buffer, StbGui.stbg_rect tex_coords_rect, StbGui.stbg_rect rect, StbGui.stbg_color color)
    {
        rect_buffer.buffer[rect_buffer.index++] = new StbGuiRenderAdapter.Rect()
        {
            rect = rect,
            tex_coord_rect = tex_coords_rect,
            color = color
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void add_draw_rect_to_rect_buffer(ref RectsBuffer rect_buffer, StbGui.stbg_rect rect, StbGui.stbg_color color)
    {
        rect_buffer.buffer[rect_buffer.index++] = new StbGuiRenderAdapter.Rect()
        {
            rect = rect,
            color = color
        };
    }

    static private void flush_render_buffer(ref RectsBuffer rects_buffer, nint texture_id, StbGuiRenderAdapter render_adapter)
    {
        if (rects_buffer.index > 0)
        {
            render_adapter.draw_texture_rects(rects_buffer.buffer, rects_buffer.index, texture_id);
            rects_buffer.index = 0;
        }
    }

    static private bool draw_text_callback(ref TextIterationData data, ref DrawTextCallbackData callback_data)
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

            if (callback_data.text_rect_buffer.index + 1 == callback_data.text_rect_buffer.buffer.Length)
            {
                flush_render_buffer(ref callback_data.background_rect_buffer, 0, callback_data.render_adapter);
                flush_render_buffer(ref callback_data.text_rect_buffer, callback_data.font_texture_id, callback_data.render_adapter);
            }

            add_draw_texture_to_rect_buffer(ref callback_data.text_rect_buffer, fromRect, toRect, data.text_color);
            if (data.background_color.a > 0)
            {
                var background_rect = StbGui.stbg_build_rect(xpos, ypos - 1, xpos + data.dx, ypos + callback_data.line_height + 1);

                add_draw_rect_to_rect_buffer(ref callback_data.background_rect_buffer, background_rect, data.background_color);
            }
        }
        else if (data.final)
        {
            flush_render_buffer(ref callback_data.background_rect_buffer, 0, callback_data.render_adapter);
            flush_render_buffer(ref callback_data.text_rect_buffer, callback_data.font_texture_id, callback_data.render_adapter);
        }

        return false;
    }

    static public void draw_text(StbGui.stbg_render_text_parameters parameters, StbGui.stbg_rect bounds, StbGuiFont font, StbGuiRenderAdapter render_adapter)
    {
        float scale = parameters.font_size / font.size;

        var bounds_width = bounds.x1 - bounds.x0;
        var bounds_height = bounds.y1 - bounds.y0;

        if (bounds_width <= 0 || bounds_height <= 0)
            return;

        ReadOnlySpan<StbGui.stbg_render_text_style_range> style_ranges =
            parameters.style_ranges.Length > 0 ?
                parameters.style_ranges.Span :
                stackalloc StbGui.stbg_render_text_style_range[1] { parameters.single_style };

        var full_text_bounds = measure_text(parameters.text.Span, font, parameters.font_size, style_ranges, parameters.measure_options);

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
            render_adapter.push_clip_rect(bounds);
        }

        DrawTextCallbackData callback_data = new()
        {
            bounds = bounds,
            horizontal_alignment = parameters.horizontal_alignment,
            vertical_alignment = parameters.vertical_alignment,
            position = new StbGui.stbg_position(),
            ignore_metrics = ignore_metrics,
            scale = scale,
            baseline = font.baseline,
            center_x_offset = center_x_offset,
            center_y_offset = center_y_offset,
            oversampling_scale = font.oversampling_scale,
            line_height = parameters.font_size,
            text_rect_buffer = new RectsBuffer()
            {
                buffer = draw_text_rects_buffer,
                index = 0
            },
            background_rect_buffer = new RectsBuffer()
            {
                buffer = draw_text_rects_buffer2,
                index = 0
            },
            font_texture_id = font.texture_id,
            render_adapter = render_adapter,
        };

        iterate_text_internal(parameters.text.Span, font, parameters.font_size, style_ranges, parameters.measure_options, ref callback_data, draw_text_callback);

        if (use_clipping)
        {
            render_adapter.pop_clip_rect();
        }
    }
}
