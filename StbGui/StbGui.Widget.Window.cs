#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static ref stbg_widget stbg__window_create(string title)
    {
        ref var window = ref stbg__add_widget(STBG_WIDGET_TYPE.WINDOW, title, out var is_new);

        stbg__window_init(ref window, is_new, title);

        return ref window;
    }

    private static void stbg__window_init(ref stbg_widget window, bool is_new, string title)
    {
        window.text = title.AsMemory();

        ref var layout = ref window.layout;

        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_RIGHT),
        };
        layout.constrains = stbg_build_constrains_unconstrained();
        layout.children_layout_direction = STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL;
        layout.children_spacing = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING);

        if (is_new)
        {
            window.layout.intrinsic_position = context.next_new_window_position;
            context.next_new_window_position = stbg_offset_position(context.next_new_window_position, stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS), stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS));
            window.layout.intrinsic_size = stbg__build_intrinsic_size_pixels(stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH), stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT));
        }
    }

    private static void stbg__window_update_input(ref stbg_widget window)
    {
        if (context.input_feedback.hovered_widget_id != window.id)
            return;

        float title_height_total = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP);

        bool mouse_over_title = context.input.mouse_position.y < Math.Min(window.computed_bounds.global_rect.y1, window.computed_bounds.global_rect.y0 + title_height_total);

        if (context.input_feedback.pressed_widget_id == window.id)
        {
            if (context.input.mouse_button_1_down)
            {
                if (mouse_over_title)
                {
                    context.input_feedback.pressed_widget_id = window.id;
                    context.input_feedback.hovered_widget_id = window.id;
                    context.input_feedback.dragged_widget_id = window.id;
                }
            }
            else
            {
                if (context.input_feedback.dragged_widget_id == window.id)
                {
                    context.input_feedback.dragged_widget_id = STBG_WIDGET_ID_NULL;
                }
            }
        }
        else if (context.input.mouse_button_1_down)
        {
            context.input_feedback.pressed_widget_id = window.id;
            context.input_feedback.active_widget_id = window.id;
            window.layout.intrinsic_sorting_index = int.MaxValue;
        }

        if (context.input_feedback.dragged_widget_id == window.id)
        {
            window.layout.intrinsic_position.x += context.input.mouse_delta.x;
            window.layout.intrinsic_position.y += context.input.mouse_delta.y;
        }
    }

    private static void stbg__window_render(ref stbg_widget window, ref stbg_render_context render_context)
    {
        var size = window.computed_bounds.size;

        bool active = context.input_feedback.hovered_widget_id == window.id || context.input_feedback.pressed_widget_id == window.id || context.input_feedback.active_widget_id == window.id;

        var title_background_color = stbg_get_widget_style_color(active ? STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR : STBG_WIDGET_STYLE.WINDOW_TITLE_BACKGROUND_COLOR);
        var title_text_color = stbg_get_widget_style_color(active ? STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_TEXT_COLOR : STBG_WIDGET_STYLE.WINDOW_TITLE_TEXT_COLOR);

        if (window.hash == debug_window_hash)
        {
            title_background_color = stbg_get_widget_style_color(active ? STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR : STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_BACKGROUND_COLOR);
            title_text_color = stbg_get_widget_style_color(active ? STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_TEXT_COLOR : STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_TEXT_COLOR);
        }

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
            stbg__build_text(window.text, title_text_color)
        );
    }
}
