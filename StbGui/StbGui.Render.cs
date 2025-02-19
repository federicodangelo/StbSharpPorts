#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

public partial class StbGui
{
    [Flags]
    public enum STBG_RENDER_TEXT_OPTIONS
    {
        NONE,
        IGNORE_BASELINE = 1 << 0,
        IGNORE_METRICS = 1 << 1,
        SINGLE_LINE = 1 << 2,
    }

    private struct stbg_render_context
    {
        public delegate void set_global_rect_delegate(stbg_rect global_rect);
        public set_global_rect_delegate set_global_rect;

        public delegate void draw_rectangle_delegate(stbg_rect rect, stbg_color color);
        public draw_rectangle_delegate draw_rectangle;

        public delegate void draw_border_delegate(stbg_rect rect, float border_size, stbg_color border_color, stbg_color background_color);
        public draw_border_delegate draw_border;

        public delegate void draw_text_delegate(stbg_rect rect, stbg_text text, float horizontal_alignment = -1, float vertical_alignment = -1, STBG_RENDER_TEXT_OPTIONS options = STBG_RENDER_TEXT_OPTIONS.NONE);
        public draw_text_delegate draw_text;

        public delegate void push_clipping_rect_delegate(stbg_rect rect);
        public push_clipping_rect_delegate push_clipping_rect;

        public delegate void pop_clipping_rect_delegate();
        public pop_clipping_rect_delegate pop_clipping_rect;
    }

    static private void stbg__render()
    {
        var render_commands_queue = context.render_commands_queue;
        var render_commands_queue_index = 0;

        var render = context.external_dependencies.render;
        stbg_rect last_global_rect = new();

        void enqueue_command(stbg_render_command command)
        {
            if (render_commands_queue_index + 1 == render_commands_queue.Length)
                flush_queue();

            if (command.type != STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME &&
                command.type != STBG_RENDER_COMMAND_TYPE.END_FRAME &&
                command.type != STBG_RENDER_COMMAND_TYPE.POP_CLIPPING_RECT)
            {
                ref var bounds = ref command.bounds;

                // Apply global rect offset
                bounds = stbg_translate_rect(bounds, last_global_rect.x0, last_global_rect.y0);

                // Ensure bounds correctly formed
                bounds.x0 = MathF.Max(bounds.x0, 0);
                bounds.y0 = MathF.Max(bounds.y0, 0);
                bounds.x1 = MathF.Max(bounds.x0, bounds.x1);
                bounds.y1 = MathF.Max(bounds.y0, bounds.y1);

                // Clamp to global rect bounds
                //bounds = stbg_clamp_rect(bounds, last_global_rect);
            }

            render_commands_queue[render_commands_queue_index] = command;
            render_commands_queue_index++;
        }

        void flush_queue()
        {
            if (render_commands_queue_index == 0) return;

            render(render_commands_queue.AsSpan(0, render_commands_queue_index));
            render_commands_queue_index = 0;
        }

        stbg_render_context render_context = new stbg_render_context()
        {
            set_global_rect = (rect) => last_global_rect = rect,
            draw_rectangle = (rect, color) => enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.RECTANGLE, bounds = rect, background_color = color }),
            draw_border = (rect, border_size, border_color, background_color) => enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.BORDER, bounds = rect, size = border_size, color = border_color, background_color = background_color }),
            draw_text = (rect, text, ha, va, options) => enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.TEXT, bounds = rect, text = text, text_horizontal_alignment = ha, text_vertical_alignment = va, text_options = options }),
            push_clipping_rect = (rect) => enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.PUSH_CLIPPING_RECT, bounds = rect }),
            pop_clipping_rect = () => enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.POP_CLIPPING_RECT }),
        };

        enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME, bounds = { x1 = context.screen_size.width, y1 = context.screen_size.height }, background_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR) });

        ref var root = ref stbg_get_widget_by_id(context.root_widget_id);

        stbg__render_widget(ref root, ref render_context);

        enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.END_FRAME });

        flush_queue();
    }

    private static void stbg__render_widget(ref stbg_widget widget, ref stbg_render_context render_context)
    {
        if (widget.properties.computed_bounds.size.width == 0 || widget.properties.computed_bounds.size.height == 0)
            return;

        render_context.set_global_rect(widget.properties.computed_bounds.global_rect);

        var widget_render = STBG__WIDGET_RENDER_MAP[(int)widget.type];

        if (widget_render != null)
            widget_render(ref widget, ref render_context);

        // Render children
        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            var needs_clipping =
                ((widget.properties.layout.flags & STBG_WIDGET_LAYOUT_FLAGS.ALLOW_CHILDREN_OVERFLOW) != 0) &&
                 !stbg_size_is_smaller_than(stbg_size_add_padding(widget.properties.computed_bounds.children_size, widget.properties.layout.inner_padding), widget.properties.computed_bounds.size);

            if (needs_clipping)
            {
                render_context.push_clipping_rect(stbg_build_rect(
                    widget.properties.layout.inner_padding.left,
                    widget.properties.layout.inner_padding.top,
                    widget.properties.computed_bounds.size.width - widget.properties.layout.inner_padding.right,
                    widget.properties.computed_bounds.size.height - widget.properties.layout.inner_padding.bottom
                ));
            }

            var children_id = widget.hierarchy.first_children_id;

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                stbg__render_widget(ref children, ref render_context);

                children_id = children.hierarchy.next_sibling_id;

            } while (children_id != STBG_WIDGET_ID_NULL);

            if (needs_clipping)
            {
                render_context.pop_clipping_rect();
            }
        }
    }
}