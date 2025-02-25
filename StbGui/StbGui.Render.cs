#pragma warning disable IDE1006 // Naming Styles

using System.Runtime.CompilerServices;

namespace StbSharp;

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
        public stbg_render_command[] render_commands_queue;
        public int render_commands_queue_index;
        public stbg_rect last_global_rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void stbg__rc_set_global_rect(stbg_rect global_rect)
    {
        context.render_context.last_global_rect = global_rect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void stbg__rc_draw_rectangle(stbg_rect rect, stbg_color color)
    {
        stbg__rc_enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.RECTANGLE, bounds = rect, background_color = color });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void stbg__rc_draw_border(stbg_rect rect, float border_size, stbg_color border_color, stbg_color background_color)
    {
        stbg__rc_enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.BORDER, bounds = rect, size = border_size, color = border_color, background_color = background_color });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void stbg__rc_draw_text(stbg_rect rect, stbg_text text, float horizontal_alignment = -1, float vertical_alignment = -1, STBG_MEASURE_TEXT_OPTIONS measure_options = STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE, STBG_RENDER_TEXT_OPTIONS render_options = STBG_RENDER_TEXT_OPTIONS.NONE)
    {
        stbg__rc_enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.TEXT, bounds = rect, text = text, text_horizontal_alignment = horizontal_alignment, text_vertical_alignment = vertical_alignment, text_measure_options = measure_options, text_render_options = render_options });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void stbg__rc_push_clipping_rect(stbg_rect rect)
    {
        stbg__rc_enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.PUSH_CLIPPING_RECT, bounds = rect });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void stbg__rc_pop_clipping_rect()
    {
        stbg__rc_enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.POP_CLIPPING_RECT });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private void stbg__rc_enqueue_command(stbg_render_command command)
    {
        ref var render_context = ref context.render_context;

        if (command.type != STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME &&
            command.type != STBG_RENDER_COMMAND_TYPE.END_FRAME &&
            command.type != STBG_RENDER_COMMAND_TYPE.POP_CLIPPING_RECT)
        {
            ref var bounds = ref command.bounds;

            // Apply global rect offset
            bounds = stbg_translate_rect(bounds, render_context.last_global_rect.x0, render_context.last_global_rect.y0);
        }

        render_context.render_commands_queue[render_context.render_commands_queue_index] = command;
        render_context.render_commands_queue_index++;

        if (render_context.render_commands_queue_index == render_context.render_commands_queue.Length)
            stbg__rc_flush_queue();
    }

    static private void stbg__rc_flush_queue()
    {
        ref var render_context = ref context.render_context;

        if (render_context.render_commands_queue_index == 0) return;

        context.external_dependencies.render(render_context.render_commands_queue.AsSpan(0, render_context.render_commands_queue_index));
        render_context.render_commands_queue_index = 0;
    }

    static private void stbg__render()
    {
        stbg__assert_internal(context.render_context.render_commands_queue_index == 0, "Pending render commands left in render queue from a previous frame");

        stbg__rc_enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME, bounds = { x1 = context.screen_size.width, y1 = context.screen_size.height }, background_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR) });

        ref var root = ref stbg_get_widget_by_id(context.root_widget_id);

        stbg__render_widget(ref root, stbg_build_rect_infinite());

        stbg__rc_enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.END_FRAME });

        stbg__rc_flush_queue();
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