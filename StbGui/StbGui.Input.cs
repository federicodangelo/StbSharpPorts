#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static void stbg__process_input()
    {
        if (context.root_widget_id == STBG_WIDGET_ID_NULL)
            return;

        stbg__derive_new_input_from_user_input();

        if (!context.input.mouse_position_valid)
        {
            context.input.mouse_position = stbg_build_position(-99999, -99999);
        }

        if (context.input_feedback.dragged_widget_id != STBG_WIDGET_ID_NULL)
        {
            context.input_feedback.hovered_widget_id = context.input_feedback.dragged_widget_id;
            context.input_feedback.pressed_widget_id = context.input_feedback.dragged_widget_id;
        }
        else
        {
            var new_hover = context.input.mouse_position_valid ?
                stbg__get_widget_id_at_position(ref stbg_get_widget_by_id(context.root_widget_id), context.input.mouse_position, stbg_build_rect_infinite()) :
                STBG_WIDGET_ID_NULL;

            if (new_hover != context.input_feedback.hovered_widget_id)
            {
                context.input_feedback.hovered_widget_id = new_hover;
                context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
            }
        }

        if (context.input.mouse_button_1_down && context.input_feedback.hovered_widget_id != STBG_WIDGET_ID_NULL)
        {
            // Try to bring the window related to the widget (if there is any) to the top
            if (stbg__find_widget_parent_by_type(context.input_feedback.hovered_widget_id, STBG_WIDGET_TYPE.WINDOW, out var parent_window))
            {
                stbg_get_widget_by_id(parent_window).properties.layout.intrinsic_sorting_index = int.MaxValue;
            }
        }

        stbg__process_widget_input(context.root_widget_id);
    }

    private static void stbg__derive_new_input_from_user_input()
    {
        var user_input = context.user_input;

        ref var input = ref context.input;

        input.mouse_position = user_input.mouse_position;
        input.mouse_position_valid = user_input.mouse_position_valid;
        input.mouse_wheel_scroll_amount = user_input.mouse_wheel_scroll_amount;

        stgb_update_input_button(user_input.mouse_button_1, ref input.mouse_button_1, ref input.mouse_button_1_down, ref input.mouse_button_1_up);
        stgb_update_input_button(user_input.mouse_button_2, ref input.mouse_button_2, ref input.mouse_button_2_down, ref input.mouse_button_2_up);
    }

    private static void stgb_update_input_button(bool user_pressed, ref bool pressed, ref bool down, ref bool up)
    {
        if (user_pressed)
        {
            up = false;
            if (!pressed)
            {
                pressed = true;
                down = true;
            }
            else
            {
                down = false;
            }
        }
        else
        {
            down = false;
            if (pressed)
            {
                pressed = false;
                up = true;
            }
            else
            {
                up = false;
            }
        }
    }

    private static bool stbg__process_widget_input(widget_id widget_id)
    {
        ref var widget = ref stbg_get_widget_by_id(widget_id);

        var children_id = widget.hierarchy.first_children_id;

        while (children_id != STBG_WIDGET_ID_NULL)
        {
            if (stbg__process_widget_input(children_id))
                return true;

            children_id = stbg_get_widget_by_id(children_id).hierarchy.next_sibling_id;
        }

        var widget_update_input = STBG__WIDGET_UPDATE_INPUT_MAP[(int)widget.type];

        if (widget_update_input != null)
        {
            if (widget_update_input(ref widget))
            {
                return true;
            }
        }

        return false;
    }

    private static widget_id stbg__get_widget_id_at_position(ref stbg_widget widget, stbg_position position, stbg_rect parent_global_rect)
    {
        var mouse_tolerance = widget.properties.mouse_tolerance;

        var global_rect = stbg_clamp_rect(widget.properties.computed_bounds.global_rect, parent_global_rect);

        if (position.x < global_rect.x0 - mouse_tolerance ||
            position.y < global_rect.y0 - mouse_tolerance ||
            position.x >= global_rect.x1 + mouse_tolerance ||
            position.y >= global_rect.y1 + mouse_tolerance)
        {
            return STBG_WIDGET_ID_NULL;
        }

        var children_id = widget.hierarchy.last_children_id;

        while (children_id != STBG_WIDGET_ID_NULL)
        {
            ref var children = ref stbg_get_widget_by_id(children_id);
            var childrean_at_position = stbg__get_widget_id_at_position(ref children, position, global_rect);

            if (childrean_at_position != STBG_WIDGET_ID_NULL)
                return childrean_at_position;

            children_id = children.hierarchy.prev_sibling_id;
        }

        return widget.id;
    }
}