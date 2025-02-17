#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

public partial class StbGui
{
    private struct stbg_render_context
    {
        public delegate void set_global_rect_delegate(stbg_rect global_rect);
        public set_global_rect_delegate set_global_rect;

        public delegate void draw_rectangle_delegate(stbg_rect rect, stbg_color color);
        public draw_rectangle_delegate draw_rectangle;

        public delegate void draw_border_delegate(stbg_rect rect, float border_size, stbg_color border_color, stbg_color background_color);
        public draw_border_delegate draw_border;

        public delegate void draw_text_delegate(stbg_rect rect, stbg_text text);
        public draw_text_delegate draw_text;
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

            if (command.type != STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME && command.type != STBG_RENDER_COMMAND_TYPE.END_FRAME)
            {
                ref var bounds = ref command.bounds;

                // Ensure bounds correctly formed
                bounds.x0 = MathF.Max(bounds.x0, 0);
                bounds.y0 = MathF.Max(bounds.y0, 0);
                bounds.x1 = MathF.Max(bounds.x0, bounds.x1);
                bounds.y1 = MathF.Max(bounds.y0, bounds.y1);

                // Apply global rect offset
                bounds = stbg_translate_rect(bounds, last_global_rect.x0, last_global_rect.y0);

                // Clamp to global rect bounds
                bounds = stbg_clamp_rect(bounds, last_global_rect);
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
            draw_text = (rect, text) => enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.TEXT, bounds = rect, text = text }),
        };

        enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME, bounds = { x1 = context.screen_size.width, y1 = context.screen_size.height }, background_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR) });

        ref var root = ref stbg_get_widget_by_id(context.root_widget_id);

        stbg__render_widget(ref root, ref render_context);

        enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.END_FRAME });

        flush_queue();
    }

    private static void stbg__render_widget(ref stbg_widget widget, ref stbg_render_context render_context)
    {
        render_context.set_global_rect(widget.properties.computed_bounds.global_rect);

        var widget_render = STBG__WIDGET_RENDER_MAP[(int)widget.type];

        if (widget_render != null)
            widget_render(ref widget, ref render_context);

        // Render children
        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            var children_id = widget.hierarchy.first_children_id;

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                stbg__render_widget(ref children, ref render_context);

                children_id = children.hierarchy.next_sibling_id;

            } while (children_id != STBG_WIDGET_ID_NULL);
        }
    }
}