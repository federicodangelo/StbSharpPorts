#pragma warning disable IDE1006 // Naming Styles

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.VisualBasic;

namespace StbSharp;

using font_id = int;
using image_id = int;
using widget_id = int;

public partial class StbGui
{
    [Flags]
    public enum STBG_RENDER_TEXT_OPTIONS
    {
        NONE,
        DONT_CLIP = 1 << 3,
    }

    public struct stbg_render_context
    {
        public stbg_render_adapter render_adapter;
        public stbg_rect last_global_rect;
        public stbg_rect visible_local_bounds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_set_global_rect(stbg_rect global_rect)
    {
        context.render_context.last_global_rect = global_rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_set_visible_local_bounds(stbg_rect visible_local_bounds)
    {
        context.render_context.visible_local_bounds = visible_local_bounds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static stbg_rect stbg__rc_get_visible_local_bounds()
    {
        return context.render_context.visible_local_bounds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_draw_rectangle(stbg_rect rect, stbg_color color)
    {
        rect = stbg_translate_rect(rect, context.render_context.last_global_rect.x0, context.render_context.last_global_rect.y0);
        context.render_context.render_adapter.draw_rectangle(rect, color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_draw_border(stbg_rect rect, float border_size, stbg_color border_color, stbg_color background_color)
    {
        rect = stbg_translate_rect(rect, context.render_context.last_global_rect.x0, context.render_context.last_global_rect.y0);
        context.render_context.render_adapter.draw_border(rect, (int)border_size, border_color, background_color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_draw_image(stbg_rect rect, stbg_image_info image, stbg_color color)
    {
        rect = stbg_translate_rect(rect, context.render_context.last_global_rect.x0, context.render_context.last_global_rect.y0);
        context.render_context.render_adapter.draw_image(rect, image.rect, color, image.original_image_id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_draw_line(stbg_position from, stbg_position to, stbg_color color, float line_width)
    {
        from = stbg_translate_position(from, context.render_context.last_global_rect.x0, context.render_context.last_global_rect.y0);
        to = stbg_translate_position(to, context.render_context.last_global_rect.x0, context.render_context.last_global_rect.y0);
        context.render_context.render_adapter.draw_line(from, to, color, line_width);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_draw_text(stbg_rect rect, stbg_text text, float horizontal_alignment = -1, float vertical_alignment = -1, STBG_MEASURE_TEXT_OPTIONS measure_options = STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE, STBG_RENDER_TEXT_OPTIONS render_options = STBG_RENDER_TEXT_OPTIONS.NONE)
    {
        rect = stbg_translate_rect(rect, context.render_context.last_global_rect.x0, context.render_context.last_global_rect.y0);
        context.render_context.render_adapter.draw_text(
            rect,
            new stbg_render_text_parameters
            {
                text = text.text,
                font_size = text.style.size,
                font_id = text.font_id,
                horizontal_alignment = horizontal_alignment,
                vertical_alignment = vertical_alignment,
                measure_options = measure_options,
                render_options = render_options,
                single_style = new stbg_render_text_style_range()
                {
                    start_index = 0,
                    text_color = text.style.color,
                    font_style = text.style.style,
                }
            }
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_draw_text(stbg_rect rect, ReadOnlyMemory<char> text, font_id font_id, float font_size, ReadOnlyMemory<stbg_render_text_style_range> style_ranges, float horizontal_alignment = -1, float vertical_alignment = -1, STBG_MEASURE_TEXT_OPTIONS measure_options = STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE, STBG_RENDER_TEXT_OPTIONS render_options = STBG_RENDER_TEXT_OPTIONS.NONE)
    {
        rect = stbg_translate_rect(rect, context.render_context.last_global_rect.x0, context.render_context.last_global_rect.y0);
        context.render_context.render_adapter.draw_text(
            rect,
            new stbg_render_text_parameters
            {
                text = text,
                font_id = font_id,
                font_size = font_size,
                horizontal_alignment = horizontal_alignment,
                vertical_alignment = vertical_alignment,
                measure_options = measure_options,
                render_options = render_options,
                style_ranges = style_ranges,
            }
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_push_clipping_rect(stbg_rect rect)
    {
        rect = stbg_translate_rect(rect, context.render_context.last_global_rect.x0, context.render_context.last_global_rect.y0);
        context.render_context.render_adapter.push_clip_rect(rect);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void stbg__rc_pop_clipping_rect()
    {
        context.render_context.render_adapter.pop_clip_rect();
    }

    private static bool stbg__render()
    {
        var disable_skip_rendering_optimization = (context.render_options & STBG_RENDER_OPTIONS.DISABLE_SKIP_RENDERING_OPTIMIZATION) != 0;

        bool force_render;
        long render_hash;

        if (disable_skip_rendering_optimization)
        {
            force_render = true;
            render_hash = 0;
            stbg__clear_force_render_queue();
        }
        else
        {
            force_render = stbg__process_force_render_queue();
            render_hash = stbg__get_render_hash();
        }

        if (context.last_render_hash == render_hash && !force_render)
        {
            // No changes in input or widgets, skip rendering
            context.frame_stats.render_skipped_due_to_same_hash = true;
            context.frame_stats.performance.render_time_us = 0;
            return false;
        }

        context.last_render_hash = render_hash;

        var start_render_time = stbg__get_performance_counter();

        context.render_context.render_adapter.render_begin_frame(stbg_get_widget_style_color(STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR));

        ref var root = ref stbg_get_widget_by_id(context.root_widget_id);

        stbg__render_widget(ref root, stbg_build_rect_infinite());

        context.render_context.render_adapter.render_end_frame();

        context.frame_stats.render_skipped_due_to_same_hash = false;
        context.frame_stats.performance.render_time_us = ((stbg__get_performance_counter() - start_render_time) * MICROSECONDS) / stbg__get_performance_counter_frequency();

        return true;
    }

    private static bool stbg__process_force_render_queue()
    {
        ref var queue = ref context.force_render_queue;

        if (queue.count == 0)
            return false;

        var current_time = context.current_time_milliseconds;
        var force_render = false;

        for (int i = 0; i < queue.count; i++)
        {
            ref var entry = ref queue.entries[i];

            if (entry.at_time <= current_time)
            {
                ref var widget = ref stbg__get_widget_by_id_internal(entry.widget_id);

                if (widget.hash == entry.widget_hash)
                {
                    widget.flags |= STBG_WIDGET_FLAGS.FORCE_RENDER;
                }

                entry = new stbg_force_render_queue_entry();
                force_render = true;
            }
        }

        if (force_render)
        {
            for (int i = 0; i < queue.count; i++)
            {
                if (queue.entries[i].widget_id == STBG_WIDGET_ID_NULL)
                {
                    // Compact the queue by copying entries after this one to this position
                    queue.entries.AsSpan().Slice(i + 1, queue.count - i - 1).CopyTo(queue.entries.AsSpan(i));

                    queue.count--;
                    i--;
                }
            }
        }

        return force_render;
    }

    private static void stbg__clear_force_render_queue()
    {
        ref var queue = ref context.force_render_queue;

        queue.count = 0;

        return;
    }

    private static void stbg__enqueue_force_render(ref stbg_widget widget, int delay_ms = 0)
    {
        if ((context.render_options & STBG_RENDER_OPTIONS.DISABLE_SKIP_RENDERING_OPTIMIZATION) != 0)
        {
            // No need to enqueue if always render is enabled
            return;
        }

        ref var queue = ref context.force_render_queue;

        for (int i = 0; i < queue.count; i++)
        {
            ref var entry = ref queue.entries[i];
            if (entry.widget_id == widget.id)
            {
                if (entry.widget_hash != widget.hash)
                {
                    entry.at_time = context.current_time_milliseconds + delay_ms;
                }
                else
                {
                    entry.at_time = Math.Min(entry.at_time, context.current_time_milliseconds + delay_ms);
                }
                return;
            }
        }

        stbg__assert(queue.count < queue.entries.Length, "Force render queue is full");

        var new_entry = new stbg_force_render_queue_entry();
        new_entry.widget_id = widget.id;
        new_entry.widget_hash = widget.hash;
        new_entry.at_time = context.current_time_milliseconds + delay_ms;

        queue.entries[queue.count] = new_entry;
        queue.count++;
    }

    private static void stbg__render_widget(ref stbg_widget widget, stbg_rect parent_clip_bounds)
    {
        if ((widget.flags & STBG_WIDGET_FLAGS.IGNORE) != 0)
            return;

        var global_rect = widget.properties.computed_bounds.global_rect;

        var clip_bounds = stbg_clamp_rect(global_rect, parent_clip_bounds);

        if (clip_bounds.x1 == clip_bounds.x0 || clip_bounds.y1 == clip_bounds.y0)
        {
            // Don't draw widgets that are completely outside parent clipping bounds
            return;
        }

        stbg__rc_set_global_rect(global_rect);
        stbg__rc_set_visible_local_bounds(stbg_translate_rect(clip_bounds, -global_rect.x0, -global_rect.y0));

        // Clear force render flag
        if ((widget.flags & STBG_WIDGET_FLAGS.FORCE_RENDER) != 0)
            widget.flags &= ~STBG_WIDGET_FLAGS.FORCE_RENDER;

        // Call widget-specific render function
        var widget_render = STBG__WIDGET_RENDER_MAP[(int)widget.type];

        if (widget_render != null)
            widget_render(ref widget);

        // Render children
        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            var needs_clipping =
                ((widget.properties.layout.flags & STBG_WIDGET_LAYOUT_FLAGS.ALLOW_CHILDREN_OVERFLOW) != 0) &&
                 !stbg_size_is_smaller_than(stbg_size_add_padding(widget.properties.computed_bounds.children_size, widget.properties.layout.inner_padding), widget.properties.computed_bounds.size);

            var clipping_rect = needs_clipping ? stbg_build_rect(
                    widget.properties.layout.inner_padding.left,
                    widget.properties.layout.inner_padding.top,
                    widget.properties.computed_bounds.size.width - widget.properties.layout.inner_padding.right,
                    widget.properties.computed_bounds.size.height - widget.properties.layout.inner_padding.bottom
                ) : stbg_build_rect_zero();

            var clipping_enabled = false;

            var children_id = widget.hierarchy.first_children_id;

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                if ((children.properties.layout.flags & STBG_WIDGET_LAYOUT_FLAGS.PARENT_CONTROLLED) != 0)
                {
                    if (clipping_enabled)
                    {
                        clipping_enabled = false;
                        stbg__rc_pop_clipping_rect();
                    }
                }
                else if (needs_clipping && !clipping_enabled)
                {
                    clipping_enabled = true;
                    stbg__rc_set_global_rect(widget.properties.computed_bounds.global_rect);
                    stbg__rc_push_clipping_rect(clipping_rect);
                }

                stbg__render_widget(ref children, clip_bounds);

                children_id = children.hierarchy.next_sibling_id;

            } while (children_id != STBG_WIDGET_ID_NULL);

            if (clipping_enabled)
            {
                stbg__rc_pop_clipping_rect();
            }
        }
    }
}
