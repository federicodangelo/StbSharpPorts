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
        render_context.set_global_rect(widget.computed_bounds.global_rect);

        var size = widget.computed_bounds.size;

        bool hovered = (widget.flags & STBG_WIDGET_FLAGS.HOVERED) != 0;
        bool pressed = (widget.flags & STBG_WIDGET_FLAGS.PRESSED) != 0;

        switch (widget.type)
        {
            case STBG_WIDGET_TYPE.WINDOW:
            {
                var title_background_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.WINDOW_TITLE_PRESSED_BACKGROUND_COLOR : hovered ? STBG_WIDGET_STYLE.WINDOW_TITLE_HOVERED_BACKGROUND_COLOR : STBG_WIDGET_STYLE.WINDOW_TITLE_BACKGROUND_COLOR);
                var title_text_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.WINDOW_TITLE_PRESSED_TEXT_COLOR : hovered ? STBG_WIDGET_STYLE.WINDOW_TITLE_HOVERED_TEXT_COLOR : STBG_WIDGET_STYLE.WINDOW_TITLE_TEXT_COLOR);

                // background and border
                render_context.draw_border(
                    stbg_build_rect(0, 0, size.width, size.height),
                    stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_BORDER_COLOR),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_BACKGROUND_COLOR)
                );
                // title background
                render_context.draw_border(
                    stbg_build_rect(
                        0,
                        0,
                        size.width,
                        stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM)
                    ),
                    stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_BORDER_COLOR),
                    title_background_color
                );
                // title text
                render_context.draw_text(
                    stbg_build_rect(
                        stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT),
                        stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP),
                        size.width - stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT),
                        stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT)
                    ),
                    stbg__build_text(widget.text, title_text_color)
                );
                break;
            }

            case STBG_WIDGET_TYPE.BUTTON:
                {
                    var border_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_BORDER_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_BORDER_COLOR : STBG_WIDGET_STYLE.BUTTON_BORDER_COLOR);
                    var background_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_BACKGROUND_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_BACKGROUND_COLOR : STBG_WIDGET_STYLE.BUTTON_BACKGROUND_COLOR);
                    var text_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_TEXT_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_TEXT_COLOR : STBG_WIDGET_STYLE.BUTTON_TEXT_COLOR);

                    render_context.draw_border(
                        stbg_build_rect(0, 0, size.width, size.height),
                        stbg_get_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
                        border_color,
                        background_color
                    );
                    render_context.draw_text(
                        stbg_build_rect(
                            stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT),
                            stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_TOP),
                            size.width - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT),
                            size.height - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM)
                        ),
                        stbg__build_text(widget.text, text_color)
                    );
                    break;
                }
        }

        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            // Render children
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