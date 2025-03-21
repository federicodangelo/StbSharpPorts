#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using System.Diagnostics;

using font_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{
    private struct stbg__scrollbar_properties
    {
        public STBG_SCROLLBAR_DIRECTION direction;
        public bool integer;
        public float step_size;
        public float min_value;
        public float max_value;
        public float value;
    }

    private static stbg__marshal_info<stbg__scrollbar_properties> stbg__scrollbar_properties_marshal_info = new();

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

    private static ref stbg_widget stbg__scrollbar_create(ReadOnlySpan<char> identifier, STBG_SCROLLBAR_DIRECTION direction, ref float value, float min_value, float max_value, float step_size, bool integer)
    {
        ref var scrollbar = ref stbg__add_widget(STBG_WIDGET_TYPE.SCROLLBAR, identifier, out var is_new);
        ref var scrollbar_properties = ref stbg__add_widget_custom_properties_by_id_internal(stbg__scrollbar_properties_marshal_info, scrollbar.id, is_new);

        scrollbar_properties.min_value = min_value;
        scrollbar_properties.max_value = max_value;
        scrollbar_properties.step_size = step_size;
        scrollbar_properties.integer = integer;
        scrollbar_properties.direction = direction;

        ref var layout = ref scrollbar.properties.layout;

        if (direction == STBG_SCROLLBAR_DIRECTION.VERTICAL)
        {
            layout.constrains = stbg_build_constrains(
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE),
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE, STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE, STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE),
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE),
                float.MaxValue
            );
        }
        else if (direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL)
        {
            layout.constrains = stbg_build_constrains(
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE, STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE, STBG_WIDGET_STYLE.SCROLLBAR_BUTTON_SIZE),
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE),
                float.MaxValue,
                stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE)
            );
        }

        layout.inner_padding = new stbg_padding();

        if (is_new || (scrollbar.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED) == 0)
        {
            stbg__scrollbar_update_value(ref scrollbar, ref scrollbar_properties, value, false);
        }
        else
        {
            value = stbg_clamp(scrollbar_properties.value, min_value, max_value);
            scrollbar.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }

        return ref scrollbar;
    }

    private static bool stbg__scrollbar_update_input(ref stbg_widget scrollbar)
    {
        const int SUB_WIDGET_PART_NONE = 0;
        const int SUB_WIDGET_PART_MIN_BUTTON = 1;
        const int SUB_WIDGET_PART_THUMB_BUTTON = 2;
        const int SUB_WIDGET_PART_MAX_BUTTON = 3;

        if (context.input_feedback.hovered_widget_id != scrollbar.id)
            return false;

        var bounds = scrollbar.properties.computed_bounds.global_rect;

        ref var scrollbar_properties = ref stbg__get_widget_custom_properties_by_id_internal<stbg__scrollbar_properties>(scrollbar.id);

        stbg__scrollbar_get_parts(scrollbar, scrollbar_properties, bounds, out stbg_rect min_button_rect, out stbg_rect max_button_rect, out stbg_rect thumb_rect);

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
                // Cancel any dragging operation related to this widget
                context.input_feedback.dragged_widget_id = STBG_WIDGET_ID_NULL;
            }

            var delta = (context.input.mouse_wheel_scroll_amount.y != 0 ? -context.input.mouse_wheel_scroll_amount.y : context.input.mouse_wheel_scroll_amount.x);

            stbg__scrollbar_update_value(ref scrollbar, ref scrollbar_properties, scrollbar_properties.value + delta * scrollbar_properties.step_size, true);
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
                scrollbar_properties,
                bounds,
                scrollbar_properties.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ?
                    context.input.mouse_position.x - bounds.x0 - context.input_feedback.drag_from_widget_x :
                    context.input.mouse_position.y - bounds.y0 - context.input_feedback.drag_from_widget_y
             );

            stbg__scrollbar_update_value(ref scrollbar, ref scrollbar_properties, new_value, true);

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
            float delta = 0;

            switch (sub_widget_part)
            {
                case SUB_WIDGET_PART_MIN_BUTTON:
                    delta = -scrollbar_properties.step_size;
                    break;
                case SUB_WIDGET_PART_MAX_BUTTON:
                    delta = scrollbar_properties.step_size;
                    break;
            }

            stbg__scrollbar_update_value(ref scrollbar, ref scrollbar_properties, scrollbar_properties.value + delta, true);

            context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
        }

        return
            context.input.mouse_button_1_down ||
            context.input.mouse_button_1_up ||
            context.input.mouse_button_1 ||
            context.input.mouse_wheel_scroll_amount.x != 0 ||
            context.input.mouse_wheel_scroll_amount.y != 0;
    }

    private static void stbg__scrollbar_update_value(ref stbg_widget scrollbar, ref stbg__scrollbar_properties properties, float new_value, bool set_updated)
    {
        var old_value = properties.value;
        new_value = stbg_clamp(new_value, properties.min_value, properties.max_value);
        if (properties.integer)
            new_value = (int)new_value;

        if (new_value != old_value)
        {
            properties.value = new_value;
            if (set_updated)
                scrollbar.properties.input_flags |= STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }
    }

    private static void stbg__scrollbar_render(ref stbg_widget scrollbar)
    {
        const int SUB_WIDGET_PART_MIN_BUTTON = 1;
        const int SUB_WIDGET_PART_THUMB_BUTTON = 2;
        const int SUB_WIDGET_PART_MAX_BUTTON = 3;

        var bounds = stbg_build_rect(0, 0, scrollbar.properties.computed_bounds.size.width, scrollbar.properties.computed_bounds.size.height);

        ref var scrollbar_properties = ref stbg__get_widget_custom_properties_by_id_internal<stbg__scrollbar_properties>(scrollbar.id);

        stbg__scrollbar_get_parts(scrollbar, scrollbar_properties, bounds, out stbg_rect min_button_rect, out stbg_rect max_button_rect, out stbg_rect thumb_rect);

        stbg__rc_draw_rectangle(bounds, stbg_get_widget_style_color(STBG_WIDGET_STYLE.SCROLLBAR_BACKGROUND_COLOR));

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

            stbg__rc_draw_rectangle(rect, background_color);

            if (p == SUB_WIDGET_PART_MIN_BUTTON)
            {
                if (scrollbar_properties.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL)
                    stbg__rc_draw_text(rect, stbg__build_text(STBG__SCROLLBAR_ARROW_LEFT, color), 0, 0, STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS);
                else
                    stbg__rc_draw_text(rect, stbg__build_text(STBG__SCROLLBAR_ARROW_UP, color), 0, 0, STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS);
            }
            else if (p == SUB_WIDGET_PART_MAX_BUTTON)
            {
                if (scrollbar_properties.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL)
                    stbg__rc_draw_text(rect, stbg__build_text(STBG__SCROLLBAR_ARROW_RIGHT, color), 0, 0, STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS);
                else
                    stbg__rc_draw_text(rect, stbg__build_text(STBG__SCROLLBAR_ARROW_DOWN, color), 0, 0, STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS);
            }
        }
    }

    private static readonly ReadOnlyMemory<char> STBG__SCROLLBAR_ARROW_LEFT = "<".AsMemory();
    private static readonly ReadOnlyMemory<char> STBG__SCROLLBAR_ARROW_RIGHT = ">".AsMemory();
    private static readonly ReadOnlyMemory<char> STBG__SCROLLBAR_ARROW_UP = "^".AsMemory();
    private static readonly ReadOnlyMemory<char> STBG__SCROLLBAR_ARROW_DOWN = "v".AsMemory();

    private static void stbg__scrollbar_get_parts(stbg_widget scrollbar, stbg__scrollbar_properties properties, stbg_rect bounds, out stbg_rect min_button_rect, out stbg_rect max_button_rect, out stbg_rect thumb_rect)
    {
        var size = stbg_build_size(bounds.x1 - bounds.x0, bounds.y1 - bounds.y0);

        var button_size = properties.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? size.height : size.width;

        min_button_rect = stbg_build_rect(bounds.x0, bounds.y0, bounds.x0 + button_size, bounds.y0 + button_size);
        max_button_rect = properties.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ?
                stbg_build_rect(bounds.x1 - button_size, bounds.y0, bounds.x1, bounds.y1) :
                stbg_build_rect(bounds.x0, bounds.y1 - button_size, bounds.x1, bounds.y1);
        var min_thumb_size = stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE);

        var scrolling_size_available = (properties.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? size.width : size.height) - button_size * 2;

        var scrolling_range_value = properties.max_value - properties.min_value;

        var scrolling_value = properties.value - properties.min_value;

        var scrolling_percent = scrolling_range_value != 0 ? scrolling_value / scrolling_range_value : 1;

        var thumb_size = properties.integer ? stbg_clamp(scrolling_size_available - scrolling_range_value, min_thumb_size, scrolling_size_available) : min_thumb_size;

        var scrolling_size = scrolling_size_available - thumb_size;

        var scrolling_position = (properties.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? bounds.x0 : bounds.y0) + scrolling_percent * scrolling_size + button_size;

        thumb_rect = properties.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ?
            stbg_build_rect(scrolling_position, bounds.y0, scrolling_position + thumb_size, bounds.y1) :
            stbg_build_rect(bounds.x0, scrolling_position, bounds.x1, scrolling_position + thumb_size);
    }


    private static float stbg__scrollbar_get_value_from_thumb_position(stbg_widget scrollbar, stbg__scrollbar_properties parameters, stbg_rect bounds, float thumb_position)
    {
        var size = stbg_build_size(bounds.x1 - bounds.x0, bounds.y1 - bounds.y0);

        var button_size = parameters.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? size.height : size.width;

        var min_thumb_size = stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_THUMB_SIZE);

        var scrolling_size_available = (parameters.direction == STBG_SCROLLBAR_DIRECTION.HORIZONTAL ? size.width : size.height) - button_size * 2;

        var scrolling_range_value = parameters.max_value - parameters.min_value;

        var thumb_size = parameters.integer ? stbg_clamp(scrolling_size_available - scrolling_range_value, min_thumb_size, scrolling_size_available) : min_thumb_size;

        var scrolling_size = scrolling_size_available - thumb_size;

        var scrolling_percent = scrolling_size != 0 ? stbg_clamp((thumb_position - button_size) / scrolling_size, 0, 1) : 1;

        return scrolling_percent * scrolling_range_value;
    }
}
