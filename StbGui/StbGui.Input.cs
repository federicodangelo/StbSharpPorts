#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static void stbg__add_user_input_event(stbg_user_input_input_event user_input_event)
    {
        stbg__assert(!context.inside_frame);
        stbg__assert(context.user_input_events_queue_offset + 1 < context.user_input_events_queue.Length);

        context.user_input_events_queue[context.user_input_events_queue_offset++] = user_input_event;
    }

    private static void stbg__process_input()
    {
        if (context.root_widget_id == STBG_WIDGET_ID_NULL)
            return;

        stbg__derive_new_input_from_user_input();

        var last_ime_info = context.input_feedback.ime_info;

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
                context.input_feedback.active_window_id = parent_window;
                stbg_get_widget_by_id(parent_window).properties.layout.intrinsic_sorting_index = int.MaxValue;
            }
        }

        stbg__process_widget_input(ref stbg_get_widget_by_id(context.root_widget_id));

        context.user_input_events_queue_offset = 0;

        if (last_ime_info.widget_id != context.input_feedback.ime_info.widget_id)
        {
            context.external_dependencies.set_input_method_editor(context.input_feedback.ime_info);
        }
    }

    private static void stbg__derive_new_input_from_user_input()
    {
        ref var input = ref context.input;

        input.mouse_wheel_scroll_amount = stbg_build_position_zero();

        input.mouse_button_1_down = false;
        input.mouse_button_1_up = false;
        input.mouse_button_2_down = false;
        input.mouse_button_2_up = false;

        for (var i = 0; i < context.user_input_events_queue_offset; i++)
        {
            var user_input = context.user_input_events_queue[i];

            switch (user_input.type)
            {
                case STBG_INPUT_EVENT_TYPE.MOUSE_POSITION:
                    input.mouse_position = user_input.mouse_position;
                    input.mouse_position_valid = user_input.mouse_position_valid;
                    break;

                case STBG_INPUT_EVENT_TYPE.MOUSE_SCROLL_WHEEL:
                    input.mouse_wheel_scroll_amount.x += user_input.mouse_scroll_wheel.x;
                    input.mouse_wheel_scroll_amount.y += user_input.mouse_scroll_wheel.y;
                    break;

                case STBG_INPUT_EVENT_TYPE.MOUSE_BUTTON:
                    if (user_input.mouse_button == 1)
                        stbg_update_input_button(user_input.mouse_button_pressed, ref input.mouse_button_1, ref input.mouse_button_1_down, ref input.mouse_button_1_up);
                    if (user_input.mouse_button == 2)
                        stbg_update_input_button(user_input.mouse_button_pressed, ref input.mouse_button_2, ref input.mouse_button_2_down, ref input.mouse_button_2_up);
                    break;

                case STBG_INPUT_EVENT_TYPE.KEYBOARD_KEY:
                    // We don't derive anything from keyboard events
                    break;
            }
        }
    }

    private static void stbg_update_input_button(bool user_pressed, ref bool pressed, ref bool down, ref bool up)
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

    private static bool stbg__process_widget_input(ref stbg_widget widget)
    {
        if ((widget.flags & STBG_WIDGET_FLAGS.IGNORE) != 0)
            return false;

        var children_id = widget.hierarchy.first_children_id;

        while (children_id != STBG_WIDGET_ID_NULL)
        {
            ref var children = ref stbg_get_widget_by_id(children_id);

            if (stbg__process_widget_input(ref children))
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
        if ((widget.flags & STBG_WIDGET_FLAGS.IGNORE) != 0)
            return STBG_WIDGET_ID_NULL;

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
            var children_at_position = stbg__get_widget_id_at_position(ref children, position, global_rect);

            if (children_at_position != STBG_WIDGET_ID_NULL)
                return children_at_position;

            children_id = children.hierarchy.prev_sibling_id;
        }

        return widget.id;
    }
}