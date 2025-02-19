#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static void stbg__scrollbar_init_default_theme()
    {
        var font_style = context.theme.default_font_style;

        // SCROLLBAR
        var scrollbarSize = MathF.Ceiling(font_style.size);
        var scrollbarButtonSize = MathF.Ceiling(font_style.size);
        var scrollbarThumbSize = MathF.Ceiling(font_style.size);

        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_SIZE, scrollbarSize);
        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE, scrollbarButtonSize);
        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE, scrollbarThumbSize);

        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_BACKGROUND_COLOR, rgb(44, 62, 80));

        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_COLOR, rgb(127, 140, 141));
        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_BACKGROUND_COLOR, rgb(127, 140, 141));
        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_COLOR, rgb(189, 195, 199));

        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_HOVERED_COLOR, rgb(149, 165, 166));
        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_HOVERED_BACKGROUND_COLOR, rgb(189, 195, 199));
        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_HOVERED_COLOR, rgb(236, 240, 241));

        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_PRESSED_COLOR, rgb(149, 165, 166));
        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_PRESSED_BACKGROUND_COLOR, rgb(189, 195, 199));
        stbg_set_widget_style(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_PRESSED_COLOR, rgb(236, 240, 241));
    }

    private static ref stbg_widget stbg__scrollbar_create(string identifier, STBG_SCROLLBAR_DIRECTION direction, ref float value, float min_value, float max_value)
    {
        ref var scrollbar = ref stbg__add_widget(STBG_WIDGET_TYPE.SCROLLBAR, identifier, out var is_new);

        scrollbar.properties.min_value.f = min_value;
        scrollbar.properties.max_value.f = max_value;

        if (is_new || (scrollbar.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED) == 0)
        {
            scrollbar.properties.value.f = Math.Clamp(value, min_value, max_value);
        }
        else
        {
            value = Math.Clamp(scrollbar.properties.value.f, min_value, max_value);
            scrollbar.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }

        ref var layout = ref scrollbar.properties.layout;

        if (direction == STBG_SCROLLBAR_DIRECTION.VERTICAL)
        {
            layout.constrains = stbg_build_constrains(
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE),
                float.MaxValue, //stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE, STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE, STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE),
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE),
                float.MaxValue
            );
        }
        else if (direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL)
        {
            layout.constrains = stbg_build_constrains(
                float.MaxValue,// stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE, STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE, STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE),
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE),
                float.MaxValue,
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE)
            );
        }

        layout.inner_padding = new stbg_padding();

        return ref scrollbar;
    }

    private static void stbg__scrollbar_update_input(ref stbg_widget scrollbar)
    {
        const int SUB_WIDGET_PART_NONE = 0;
        const int SUB_WIDGET_PART_MIN_BUTTON = 1;
        const int SUB_WIDGET_PART_THUMB_BUTTON = 2;
        const int SUB_WIDGET_PART_MAX_BUTTON = 3;

        if (context.input_feedback.hovered_widget_id != scrollbar.id)
            return;

        var bounds = scrollbar.properties.computed_bounds.global_rect;

        stbg__scrollbar_get_parts(scrollbar, bounds, out stbg_rect min_button_rect, out stbg_rect max_button_rect, out stbg_rect thumb_rect, out var direction);

        var sub_widget_part =
            stbg_rect_is_position_inside(min_button_rect, context.input.mouse_position) ?
                SUB_WIDGET_PART_MIN_BUTTON :
            stbg_rect_is_position_inside(max_button_rect, context.input.mouse_position) ?
                SUB_WIDGET_PART_MAX_BUTTON :
            stbg_rect_is_position_inside(thumb_rect, context.input.mouse_position) ?
                SUB_WIDGET_PART_THUMB_BUTTON :
            SUB_WIDGET_PART_NONE;

        if (context.input.mouse_wheel_scroll_amount.x != 0 || context.input.mouse_wheel_scroll_amount.y != 0)
        {
            if (context.input_feedback.dragged_widget_id == scrollbar.id)
            {
                // Cancel any dragging operation realted to this widget
                context.input_feedback.dragged_widget_id = STBG_WIDGET_ID_NULL;
            }

            var delta = -(context.input.mouse_wheel_scroll_amount.y != 0 ? context.input.mouse_wheel_scroll_amount.y : context.input.mouse_wheel_scroll_amount.x);

            var range = scrollbar.properties.max_value.f - scrollbar.properties.min_value.f;

            stbg__scrollbar_update_value(ref scrollbar, scrollbar.properties.value.f + delta * range / 10);
        }

        if (context.input.mouse_button_1_down)
        {
            context.input_feedback.pressed_widget_id = scrollbar.id;
            context.input_feedback.pressed_sub_widget_part = sub_widget_part;

            if (sub_widget_part == SUB_WIDGET_PART_THUMB_BUTTON)
            {
                context.input_feedback.dragged_widget_id = scrollbar.id;
                context.input_feedback.drag_from_widget_x = context.input.mouse_position.x - thumb_rect.x0;
                context.input_feedback.drag_from_widget_y = context.input.mouse_position.y - thumb_rect.y0;
            }
        }

        if (context.input_feedback.dragged_widget_id == scrollbar.id)
        {
            // Dragging thumb!! Update value
            var new_value = stbg__scrollbar_get_value_from_thumb_position(
                scrollbar,
                bounds,
                direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ?
                    context.input.mouse_position.x - bounds.x0 - context.input_feedback.drag_from_widget_x :
                    context.input.mouse_position.y - bounds.y0 - context.input_feedback.drag_from_widget_y
             );

            stbg__scrollbar_update_value(ref scrollbar, new_value);

            if (context.input.mouse_button_1_up)
            {
                // Finish dragging
                context.input_feedback.dragged_widget_id = STBG_WIDGET_ID_NULL;
            }
        }

        if (context.input_feedback.dragged_widget_id != scrollbar.id)
        {
            context.input_feedback.hovered_sub_widget_part = sub_widget_part;
        }

        if (context.input.mouse_button_1_up &&
            context.input_feedback.pressed_widget_id == scrollbar.id)
        {
            var range = scrollbar.properties.max_value.f - scrollbar.properties.min_value.f;

            float delta = 0;

            switch (sub_widget_part)
            {
                case SUB_WIDGET_PART_MIN_BUTTON:
                    delta = range * -0.1f;
                    break;
                case SUB_WIDGET_PART_MAX_BUTTON:
                    delta = range * 0.1f;
                    break;
            }

            stbg__scrollbar_update_value(ref scrollbar, scrollbar.properties.value.f + delta);

            context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
        }
    }

    private static void stbg__scrollbar_update_value(ref stbg_widget scrollbar, float new_value)
    {
        var old_value = scrollbar.properties.value.f;
        new_value = Math.Clamp(new_value, scrollbar.properties.min_value.f, scrollbar.properties.max_value.f);

        if (new_value != old_value)
        {
            scrollbar.properties.value.f = new_value;
            scrollbar.properties.input_flags |= STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }
    }

    private static void stbg__scrollbar_render(ref stbg_widget scrollbar, ref stbg_render_context render_context)
    {
        const int SUB_WIDGET_PART_MIN_BUTTON = 1;
        const int SUB_WIDGET_PART_THUMB_BUTTON = 2;
        const int SUB_WIDGET_PART_MAX_BUTTON = 3;

        var bounds = stbg_build_rect(0, 0, scrollbar.properties.computed_bounds.size.width, scrollbar.properties.computed_bounds.size.height);

        stbg__scrollbar_get_parts(scrollbar, bounds, out stbg_rect min_button_rect, out stbg_rect max_button_rect, out stbg_rect thumb_rect, out var direction);

        render_context.draw_rectangle(bounds, stbg_get_widget_style_color(STBG_WIDGET_STYLE.SCROLLBAR_BACKGROUND_COLOR));

        for (int p = SUB_WIDGET_PART_MIN_BUTTON; p <= SUB_WIDGET_PART_MAX_BUTTON; p++)
        {
            bool hovered = context.input_feedback.hovered_widget_id == scrollbar.id && context.input_feedback.hovered_sub_widget_part == p;
            bool pressed = context.input_feedback.pressed_widget_id == scrollbar.id && context.input_feedback.pressed_sub_widget_part == p;

            var background_color =
                p == SUB_WIDGET_PART_THUMB_BUTTON ?
                    stbg_get_widget_style_color_normal_hovered_pressed(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_COLOR, STBG_WIDGET_STYLE.SCROLLBAR_THUMB_HOVERED_COLOR, STBG_WIDGET_STYLE.SCROLLBAR_THUMB_PRESSED_COLOR, hovered, pressed) :
                    stbg_get_widget_style_color_normal_hovered_pressed(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_BACKGROUND_COLOR, STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_HOVERED_BACKGROUND_COLOR, STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_PRESSED_BACKGROUND_COLOR, hovered, pressed);

            var color =
                stbg_get_widget_style_color_normal_hovered_pressed(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_COLOR, STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_HOVERED_COLOR, STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_PRESSED_COLOR, hovered, pressed);

            stbg_rect rect =
                p == SUB_WIDGET_PART_MIN_BUTTON ? min_button_rect :
                p == SUB_WIDGET_PART_MAX_BUTTON ? max_button_rect :
                thumb_rect;

            render_context.draw_rectangle(rect, background_color);

            if (p == SUB_WIDGET_PART_MIN_BUTTON)
            {
                if (direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL)
                    render_context.draw_text(rect, stbg__build_text("<".AsMemory(), color), 0, 0, STBG_RENDER_TEXT_OPTIONS.IGNORE_METRICS);
                else
                    render_context.draw_text(rect, stbg__build_text("^".AsMemory(), color), 0, 0, STBG_RENDER_TEXT_OPTIONS.IGNORE_METRICS);
            }
            else if (p == SUB_WIDGET_PART_MAX_BUTTON)
            {
                if (direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL)
                    render_context.draw_text(rect, stbg__build_text(">".AsMemory(), color), 0, 0, STBG_RENDER_TEXT_OPTIONS.IGNORE_METRICS);
                else
                    render_context.draw_text(rect, stbg__build_text("v".AsMemory(), color), 0, 0, STBG_RENDER_TEXT_OPTIONS.IGNORE_METRICS);
            }
        }
    }

    private static void stbg__scrollbar_get_parts(stbg_widget scrollbar, stbg_rect bounds, out stbg_rect min_button_rect, out stbg_rect max_button_rect, out stbg_rect thumb_rect, out STBG_SCROLLBAR_DIRECTION direction)
    {
        var size = stbg_build_size(bounds.x1 - bounds.x0, bounds.y1 - bounds.y0);

        direction = scrollbar.properties.layout.constrains.max.width == float.MaxValue ? STBG_SCROLLBAR_DIRECTION.HORIZONTAL : STBG_SCROLLBAR_DIRECTION.VERTICAL;

        var button_size = direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? size.height : size.width;

        min_button_rect = stbg_build_rect(bounds.x0, bounds.y0, bounds.x0 + button_size, bounds.y0 + button_size);
        max_button_rect = direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ?
                stbg_build_rect(bounds.x1 - button_size, bounds.y0, bounds.x1, bounds.y1) :
                stbg_build_rect(bounds.x0, bounds.y1 - button_size, bounds.x1, bounds.y1);
        var min_thumb_size = stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE);

        var scrolling_size = (direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? size.width : size.height) - button_size * 2 - min_thumb_size;

        var scrolling_min_value = scrollbar.properties.min_value.f;
        var scrolling_max_value = scrollbar.properties.max_value.f;
        var scrolling_range_value = scrolling_max_value - scrolling_min_value;

        var scrolling_value = scrollbar.properties.value.f - scrolling_min_value;

        var scrolling_percent = scrolling_range_value != 0 ? scrolling_value / scrolling_range_value : 0;

        var scrolling_position = (direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? bounds.x0 : bounds.y0) + scrolling_percent * scrolling_size + button_size;

        thumb_rect = direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ?
            stbg_build_rect(scrolling_position, bounds.y0, scrolling_position + min_thumb_size, bounds.y1) :
            stbg_build_rect(bounds.x0, scrolling_position, bounds.x1, scrolling_position + min_thumb_size);
    }


    private static float stbg__scrollbar_get_value_from_thumb_position(stbg_widget scrollbar, stbg_rect bounds, float thumb_position)
    {
        var size = stbg_build_size(bounds.x1 - bounds.x0, bounds.y1 - bounds.y0);

        var direction = scrollbar.properties.layout.constrains.max.width == float.MaxValue ? STBG_SCROLLBAR_DIRECTION.HORIZONTAL : STBG_SCROLLBAR_DIRECTION.VERTICAL;

        var button_size = direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? size.height : size.width;

        var min_thumb_size = stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE);

        var scrolling_size = (direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? size.width : size.height) - button_size * 2 - min_thumb_size;

        var scrolling_min_value = scrollbar.properties.min_value.f;
        var scrolling_max_value = scrollbar.properties.max_value.f;
        var scrolling_range_value = scrolling_max_value - scrolling_min_value;

        var scrolling_percent = Math.Clamp((thumb_position - button_size) / scrolling_size, 0, 1);

        return scrolling_percent * scrolling_range_value;
    }
}