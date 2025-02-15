#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;
using System.Runtime.InteropServices;

public partial class StbGui
{
    private static void stbg__layout_widgets()
    {
        // Ideas from:
        // - Originally using https://www.rfleury.com/p/ui-part-2-build-it-every-frame-immediate but it was waaay to complex
        // - Switched to https://docs.flutter.dev/ui/layout/constraints (let's see how it goes..)

        // - Constrains go down
        // - Sizes go up
        // - Parent sets position

        stbg_widget_constrains constrains = stbg_build_constrains_unconstrained();

        stbg__layout_widget(constrains, ref stbg_get_widget_by_id(context.root_widget_id));

        stbg__update_global_rect(ref stbg_get_widget_by_id(context.root_widget_id), new stbg_rect());

    }

    private static void stbg__update_global_rect(ref stbg_widget widget, stbg_rect parent_global_rect)
    {
        // Update self bounds
        ref var widget_computed_bounds = ref widget.computed_bounds;
        ref var widget_global_rect = ref widget.computed_bounds.global_rect;

        widget_global_rect = stbg_build_rect(
            widget_computed_bounds.position.x, widget_computed_bounds.position.y,
            widget_computed_bounds.position.x + widget_computed_bounds.size.width, widget_computed_bounds.position.y + widget_computed_bounds.size.height);

        widget_global_rect = stbg_translate_rect(widget_global_rect, parent_global_rect.x0, parent_global_rect.y0);

        // Update children bounds
        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            // Iterate children
            var children_id = widget.hierarchy.first_children_id;

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                stbg__update_global_rect(ref children, widget_global_rect);

                children_id = children.hierarchy.next_sibling_id;
            } while (children_id != STBG_WIDGET_ID_NULL);
        }
    }
    private static stbg_size stbg__layout_widget(stbg_widget_constrains parent_constrains, ref stbg_widget widget)
    {
        // - Constrains go down
        // - Sizes go up
        // - Parent sets position
        var widget_layout = widget.layout;
        var widget_intrinsic_size = widget_layout.intrinsic_size;
        var widget_constrains = widget_layout.constrains;
        var widget_inner_padding = widget.layout.inner_padding;

        // Build current contrains by merging the widget constrains with the parent constrains
        var constrains = stbg_merge_constrains(parent_constrains, widget_constrains);

        stbg_size intrinsic_size = stbg__get_instrinsic_size(widget, widget_intrinsic_size);

        var accumulated_children_size = stbg_build_size_zero();

        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            // Widget has children
            var widget_children_spacing = widget.layout.children_spacing;
            var widget_children_layout_direction = widget.layout.children_layout_direction;

            // Build children constrains by removing padding
            var children_constrains = stbg_constrains_remove_padding(constrains, widget_inner_padding);

            // Iterate children
            var children_id = widget.hierarchy.first_children_id;

            // TODO:
            // - add "expand_width" and "expand_height" properties, used to resize children to fill all available space 
            //   (they would modify min.width and min.height to match the expected size)

            var next_children_top_left = stbg_build_position(widget_inner_padding.left, widget_inner_padding.top);

            var first_chilren = true;

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                if (!first_chilren && widget_children_spacing != 0)
                {
                    switch (widget_children_layout_direction)
                    {
                        case STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL:
                            {
                                var available_spacing = Math.Min(widget_children_spacing, children_constrains.max.height);
                                next_children_top_left.y += available_spacing;
                                children_constrains.max.height -= available_spacing;
                                accumulated_children_size.height += available_spacing;
                                break;
                            }

                        case STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL:
                            {
                                var available_spacing = Math.Min(widget_children_spacing, children_constrains.max.width);
                                next_children_top_left.x += available_spacing;
                                children_constrains.max.width -= available_spacing;
                                accumulated_children_size.width += available_spacing;
                                break;
                            }
                    }
                }

                var children_size = stbg__layout_widget(children_constrains, ref children);

                stbg__assert_internal(children_size.width <= children_constrains.max.width);
                stbg__assert_internal(children_size.width >= children_constrains.min.width);
                stbg__assert_internal(children_size.height <= children_constrains.max.height);
                stbg__assert_internal(children_size.height >= children_constrains.min.height);

                children.computed_bounds.size = children_size;

                switch (widget_children_layout_direction)
                {
                    case STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL:
                        children.computed_bounds.position = next_children_top_left;
                        children_constrains.max.height -= children_size.height;
                        children_constrains.min.height = Math.Min(children_constrains.min.height, children_constrains.max.height);
                        accumulated_children_size.height += children_size.height;
                        accumulated_children_size.width = Math.Max(accumulated_children_size.width, children_size.width);
                        next_children_top_left.y += children_size.height;
                        break;

                    case STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL:
                        children.computed_bounds.position = next_children_top_left;
                        children_constrains.max.width -= children_size.height;
                        children_constrains.min.width = Math.Min(children_constrains.min.width, children_constrains.max.width);
                        accumulated_children_size.width += children_size.width;
                        accumulated_children_size.height = Math.Max(accumulated_children_size.height, children_size.height);
                        next_children_top_left.x += children_size.width;
                        break;

                    case STBG_CHILDREN_LAYOUT_DIRECTION.FREE:
                        children.computed_bounds.position.x =
                            next_children_top_left.x + Math.Min(children.layout.intrinsic_position.x, children_constrains.max.width - children_size.width);
                        children.computed_bounds.position.y =
                            next_children_top_left.y + Math.Min(children.layout.intrinsic_position.y, children_constrains.max.height - children_size.height);
                        break;
                }

                children_id = children.hierarchy.next_sibling_id;
                first_chilren = false;
            } while (children_id != STBG_WIDGET_ID_NULL);
        }

        stbg_size widget_size = stbg_size_max(intrinsic_size, accumulated_children_size);
        widget_size = stbg_size_add_padding(widget_size, widget_inner_padding);
        widget_size = stbg_size_constrain(widget_size, constrains);

        return widget_size;
    }

    private static stbg_size stbg__get_instrinsic_size(stbg_widget widget, stbg_widget_intrinsic_size widget_intrinsic_size)
    {
        return widget_intrinsic_size.type == STBG_INTRINSIC_SIZE_TYPE.FIXED_PIXELS ?
            widget_intrinsic_size.size :
            widget_intrinsic_size.type == STBG_INTRINSIC_SIZE_TYPE.MEASURE_TEXT ?
            stbg_measure_text(widget.text) :
            stbg_build_size_zero();
    }
}
