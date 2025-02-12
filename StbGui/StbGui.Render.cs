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

            ref var bounds = ref command.bounds;

            // Apply global rect offset
            bounds.top_left.x += last_global_rect.top_left.x;
            bounds.top_left.y += last_global_rect.top_left.y;
            bounds.bottom_right.x += last_global_rect.top_left.x;
            bounds.bottom_right.y += last_global_rect.top_left.y;

            // Clamp to global rect bounds and ensure that bottom_right is bigger than top_left
            bounds.top_left.x = Math.Clamp(bounds.top_left.x, last_global_rect.top_left.x, last_global_rect.bottom_right.x);
            bounds.top_left.y = Math.Clamp(bounds.top_left.y, last_global_rect.top_left.y, last_global_rect.bottom_right.y);
            bounds.bottom_right.x = Math.Max(Math.Clamp(bounds.bottom_right.x, last_global_rect.top_left.x, last_global_rect.bottom_right.x), bounds.top_left.x);
            bounds.bottom_right.y = Math.Max(Math.Clamp(bounds.bottom_right.y, last_global_rect.top_left.y, last_global_rect.bottom_right.y), bounds.top_left.y);

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

        enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME });

        ref var root = ref stbg_get_widget_by_id(context.root_widget_id);

        stbg__render_widget(ref root, ref render_context);

        enqueue_command(new() { type = STBG_RENDER_COMMAND_TYPE.END_FRAME });

        flush_queue();
    }

    private static void stbg__render_widget(ref stbg_widget widget, ref stbg_render_context render_context)
    {
        render_context.set_global_rect(widget.computed_bounds.global_rect);

        var size = widget.computed_bounds.size;

        switch (widget.type)
        {
            case STBG_WIDGET_TYPE.WINDOW:
                // background and border
                render_context.draw_border(
                    stbg_build_rect(0, 0, size.width, size.height),
                    stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_BORDER_COLOR),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_BACKGROUND_COLOR)
                );
                // title background
                render_context.draw_rectangle(
                    stbg_build_rect(
                        stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT),
                        stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP),
                        size.width - stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT),
                        size.height - stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM)
                    ),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_TITLE_BACKGROUND_COLOR)
                );
                // title text
                render_context.draw_text(
                    stbg_build_rect(
                        stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT),
                        stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP),
                        size.width - stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT),
                        size.height - stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM)
                    ),
                    widget.text
                );
                break;

            case STBG_WIDGET_TYPE.BUTTON:
                render_context.draw_border(
                    stbg_build_rect(0, 0, size.width, size.height),
                    stbg_get_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.BUTTON_BORDER_COLOR),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.BUTTON_BACKGROUND_COLOR)
                );
                render_context.draw_text(
                    stbg_build_rect(
                        stbg_get_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT),
                        stbg_get_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_TOP),
                        size.width - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT, STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT),
                        size.height - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_PADDING_TOP, STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM)
                    ),
                    widget.text
                );
                break;
        }

        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            // Render children
            var children_id = widget.hierarchy.first_children_id;

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                stbg__render_widget(ref children, ref render_context);

            } while (children_id != STBG_WIDGET_ID_NULL);
        }
    }
}