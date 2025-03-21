#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using System.Diagnostics;

using font_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{
    private struct stbg__nodes_container_properties
    {

    }

    private static stbg__marshal_info<stbg__nodes_container_properties> stbg__nodes_container_properties_marshal_info = new();

    private static void stbg__nodes_container_init_default_theme()
    {
        // NODES CONTAINER
        var nodesContainerLineSpacing = 64;
        var nodesContainerLineWidth = 1;
        stbg_set_widget_style(STBG_WIDGET_STYLE.NODES_CONTAINER_BACKGROUND_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.NODES_CONTAINER_GRID_LINE_COLOR, rgb(52, 152, 219));
        stbg_set_widget_style(STBG_WIDGET_STYLE.NODES_CONTAINER_GRID_LINE_WIDTH, nodesContainerLineWidth);
        stbg_set_widget_style(STBG_WIDGET_STYLE.NODES_CONTAINER_GRID_LINE_SPACING, nodesContainerLineSpacing);
    }

    private static ref stbg_widget stbg__nodes_container_create(ReadOnlySpan<char> identifier)
    {
        ref var nodes_container = ref stbg__add_widget(STBG_WIDGET_TYPE.NODES_CONTAINER, identifier, out var is_new);
        ref var nodes_container_props = ref stbg__add_widget_custom_properties_by_id_internal(stbg__nodes_container_properties_marshal_info, nodes_container.id, is_new);

        ref var layout = ref nodes_container.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.constrains.max.width = 16384;
        layout.constrains.max.height = 16384;
        layout.intrinsic_size.expand_width = true;
        layout.intrinsic_size.expand_height = true;

        return ref nodes_container;
    }

    private static bool stbg__nodes_container_update_input(ref stbg_widget nodes_container)
    {
        if (context.input_feedback.hovered_widget_id != nodes_container.id)
            return false;

        ref var nodes_container_props = ref stbg__get_widget_custom_properties_by_id_internal<stbg__nodes_container_properties>(nodes_container.id);

        var mouse_position = context.input.mouse_position;
        var bounds = nodes_container.properties.computed_bounds.global_rect;

        // Dragging with the mouse scrolls the parent container
        if (context.input_feedback.pressed_widget_id == nodes_container.id)
        {
            if (context.input.mouse_button_1)
            {
                context.input_feedback.pressed_widget_id = nodes_container.id;
                context.input_feedback.hovered_widget_id = nodes_container.id;
                context.input_feedback.dragged_widget_id = nodes_container.id;
            }
            else
            {
                if (context.input_feedback.dragged_widget_id == nodes_container.id)
                {
                    context.input_feedback.dragged_widget_id = STBG_WIDGET_ID_NULL;
                }
                context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
            }
        }
        else if (context.input.mouse_button_1_down)
        {
            context.input_feedback.pressed_widget_id = nodes_container.id;
            context.input_feedback.drag_from_widget_x = mouse_position.x;
            context.input_feedback.drag_from_widget_y = mouse_position.y;
        }

        if (context.input_feedback.dragged_widget_id == nodes_container.id)
        {
            var drag_delta_x = mouse_position.x - context.input_feedback.drag_from_widget_x;
            var drag_delta_y = mouse_position.y - context.input_feedback.drag_from_widget_y;

            if (MathF.Abs(drag_delta_x) > 0.1f || MathF.Abs(drag_delta_y) > 0.1f)
            {
                // Scroll the parent container
                ref var parent = ref stbg__get_widget_by_id_internal(nodes_container.hierarchy.parent_id);
                parent.properties.layout.children_offset.x += drag_delta_x;
                parent.properties.layout.children_offset.y += drag_delta_y;

                // Update the drag position
                context.input_feedback.drag_from_widget_x = mouse_position.x;
                context.input_feedback.drag_from_widget_y = mouse_position.y;
            }

        }

        return
            context.input.mouse_button_1_down ||
            context.input.mouse_button_1_up ||
            context.input.mouse_button_1 ||
            context.input.mouse_wheel_scroll_amount.x != 0 ||
            context.input.mouse_wheel_scroll_amount.y != 0;
    }

    private static void stbg__nodes_container_render(ref stbg_widget nodes_container)
    {
        var size = nodes_container.properties.computed_bounds.size;
        ref var nodes_container_props = ref stbg__get_widget_custom_properties_by_id_internal<stbg__nodes_container_properties>(nodes_container.id);

        var background_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.NODES_CONTAINER_BACKGROUND_COLOR);
        var line_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.NODES_CONTAINER_GRID_LINE_COLOR);
        var line_spacing = stbg_get_widget_style(STBG_WIDGET_STYLE.NODES_CONTAINER_GRID_LINE_SPACING);
        var line_width = stbg_get_widget_style(STBG_WIDGET_STYLE.NODES_CONTAINER_GRID_LINE_WIDTH);

        var visible_bounds = stbg__rc_get_visible_local_bounds();

        // Draw background

        stbg__rc_draw_rectangle(
            stbg_build_rect(0, 0, size.width, size.height),
            background_color
        );

        // Draw grid lines

        var fromX = MathF.Floor(visible_bounds.x0 / line_spacing - 1) * line_spacing;
        var toX = MathF.Ceiling(visible_bounds.x1 / line_spacing + 1) * line_spacing;
        var fromY = MathF.Floor(visible_bounds.y0 / line_spacing - 1) * line_spacing;
        var toY = MathF.Ceiling(visible_bounds.y1 / line_spacing + 1) * line_spacing;

        // Vertical lines
        for (var x = fromX; x < toX; x += line_spacing)
        {
            stbg__rc_draw_line(
                stbg_build_position(x, visible_bounds.y0),
                stbg_build_position(x, visible_bounds.y1),
                line_color,
                line_width
            );
        }
        // Horizontal lines
        for (var y = fromY; y < toY; y += line_spacing)
        {
            stbg__rc_draw_line(
                stbg_build_position(visible_bounds.x0, y),
                stbg_build_position(visible_bounds.x1, y),
                line_color,
                line_width
            );
        }
    }
}
