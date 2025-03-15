#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;

using font_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{
    private struct stbg__window_properties
    {
        public STBG_WINDOW_OPTIONS options;
        public bool open;
    }

    private static void stbg__window_init_default_theme()
    {
        var font_style = context.theme.default_font_style;

        // WINDOW
        var windowBorder = 1.0f;
        var windowTitleHeight = MathF.Ceiling(font_style.size);
        var windowTitlePadding = MathF.Ceiling(font_style.size / 4);
        var windowChildrenPadding = MathF.Ceiling(font_style.size / 2);
        var windowDefaultWidth = MathF.Ceiling(font_style.size * 30);
        var windowDefaultHeight = MathF.Ceiling(font_style.size * 15);
        var windowChildrenSpacing = MathF.Ceiling(font_style.size / 4);
        var windowSpacingBetweenNewWindows = MathF.Ceiling(font_style.size / 2); ;
        var windowBorderResizeTolerance = MathF.Ceiling(font_style.size / 4);
        var windowCloseButtonSize = MathF.Ceiling(font_style.size);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, windowBorder);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, windowTitleHeight);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, windowTitlePadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, windowTitlePadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT, windowTitlePadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT, windowTitlePadding);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP, windowChildrenPadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_BOTTOM, windowChildrenPadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_LEFT, windowChildrenPadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_RIGHT, windowChildrenPadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING, windowChildrenSpacing);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_SCROLL_LINES_AMOUNT, 3);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, windowDefaultWidth);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, windowDefaultHeight);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS, windowSpacingBetweenNewWindows);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CENTER_NEW_WINDOWS, true);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_RESIZE_TOLERANCE, windowBorderResizeTolerance);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BACKGROUND_COLOR, rgb(189, 195, 199));

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_TEXT_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_BACKGROUND_COLOR, rgb(44, 62, 80));

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_TEXT_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR, rgb(52, 73, 94));

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_HOVERED_COLOR, rgb(192, 57, 43));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_PRESSED_COLOR, rgb(231, 76, 60));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_SIZE, windowCloseButtonSize);

        // DEBUG WINDOW
        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_TEXT_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_BACKGROUND_COLOR, rgb(192, 57, 43));

        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_TEXT_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR, rgb(231, 76, 60));

        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_POSITION_X, 5);
        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_POSITION_Y, 5);
    }

    private static ref stbg_widget stbg__window_create(ReadOnlySpan<char> title, ref bool is_open, STBG_WINDOW_OPTIONS options)
    {
        ref var window = ref stbg__add_widget(STBG_WIDGET_TYPE.WINDOW, title, out var is_new, out var is_already_created_in_same_frame);
        ref var window_properties = ref stbg__add_widget_custom_properties_by_id_internal<stbg__window_properties>(window.id, is_new);

        if (!is_already_created_in_same_frame)
            stbg__window_init(ref window, ref window_properties, ref is_open, is_new, title, options);
        else
            is_open = window_properties.open;

        return ref window;
    }

    private static void stbg__window_init(ref stbg_widget window, ref stbg__window_properties window_properties, ref bool is_open, bool is_new, ReadOnlySpan<char> title, STBG_WINDOW_OPTIONS options)
    {
        ref var window_ref_props = ref stbg__get_widget_ref_props_by_id_internal(window.id);
        
        window_ref_props.text = stbg__add_string(title);
        window.flags |= STBG_WIDGET_FLAGS.ALLOW_CHILDREN;

        window_properties.options = options;

        var has_title = (window_properties.options & STBG_WINDOW_OPTIONS.NO_TITLE) == 0;
        var can_resize = (window_properties.options & STBG_WINDOW_OPTIONS.NO_RESIZE) == 0;

        ref var layout = ref window.properties.layout;

        window.properties.input_tolerance = stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_RESIZE_TOLERANCE);

        layout.inner_padding = new stbg_padding()
        {
            top = (has_title ?
                stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM) : 0) +
                stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_RIGHT),
        };
        layout.constrains = stbg_build_constrains_unconstrained();
        layout.constrains.min.height = layout.inner_padding.top + layout.inner_padding.bottom;
        layout.constrains.min.width = layout.inner_padding.right + layout.inner_padding.left;
        layout.children_layout_direction = STBG_CHILDREN_LAYOUT.VERTICAL;
        layout.children_spacing = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING);

        if (can_resize && stbg_get_widget_style_boolean(STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS))
        {
            layout.flags |= STBG_WIDGET_LAYOUT_FLAGS.ALLOW_CHILDREN_OVERFLOW;
            layout.flags |= STBG_WIDGET_LAYOUT_FLAGS.INTRINSIC_SIZE_IS_MAX_SIZE;
        }

        if (is_new)
        {
            window.properties.layout.intrinsic_size.size = stbg_build_size(stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH), stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT));
            window.properties.layout.intrinsic_sorting_index = int.MaxValue;

            if (stbg_get_widget_style_boolean(STBG_WIDGET_STYLE.WINDOW_CENTER_NEW_WINDOWS))
            {
                window.properties.layout.intrinsic_position.position = stbg_build_position(
                        (int)((context.screen_size.width - window.properties.layout.intrinsic_size.size.width) / 2),
                        (int)((context.screen_size.height - window.properties.layout.intrinsic_size.size.height) / 2)
                    );
            }
            else
            {
                var window_spacing = stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS);
                window.properties.layout.intrinsic_position.position = context.next_new_window_position;
                context.next_new_window_position = stbg_offset_position(context.next_new_window_position, window_spacing, window_spacing);
            }
        }

        if (is_new || (window.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED) == 0)
        {
            window_properties.open = is_open;
        }
        else
        {
            is_open = window_properties.open;
            window.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }

        if (is_open)
            window.flags &= ~STBG_WIDGET_FLAGS.IGNORE;
        else
            window.flags |= STBG_WIDGET_FLAGS.IGNORE;

        // Add scrollbars if required
        if (stbg__window_get_children_scrolling_info(ref window, window_properties, out var needs_horizontal_bar, out var needs_vertical_bar, out var horizontal_size, out var vertical_size) && !is_new)
        {
            var scrollbar_size = stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE);

            if (needs_horizontal_bar)
                layout.inner_padding.bottom += scrollbar_size;

            if (needs_vertical_bar)
                layout.inner_padding.right += scrollbar_size;

            // TODO: We need a better way to save / restore these values..
            var saved_last_widget_id = context.last_widget_id;
            var saved_last_widget_is_new = context.last_widget_is_new;
            var saved_current_widget_id = context.current_widget_id;

            context.current_widget_id = window.id;

            var border_size = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE);

            bool corner_busy = needs_vertical_bar && needs_horizontal_bar;

            var scroll_lines_amount = stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_SCROLL_LINES_AMOUNT);

            if (needs_horizontal_bar)
            {
                var size = horizontal_size;
                var ox = -stbg_clamp(window.properties.layout.children_offset.x, -size, 0);
                stbg__scrollbar_create("$$$$_window_scrollbar_horizontal", STBG_SCROLLBAR_DIRECTION.HORIZONTAL, ref ox, 0, size, context.theme.default_font_style.size * scroll_lines_amount, true);
                window.properties.layout.children_offset.x = -ox;
                ref var scrollbar = ref stbg_get_last_widget();
                scrollbar.properties.layout.flags |= STBG_WIDGET_LAYOUT_FLAGS.PARENT_CONTROLLED;
                var corner_size = corner_busy ? scrollbar_size : 0;
                scrollbar.properties.layout.intrinsic_position.docking = STBG_INTRINSIC_POSITION_DOCKING.BOTTOM;
                scrollbar.properties.layout.intrinsic_position.docking_padding = border_size;
                scrollbar.properties.layout.intrinsic_size.expand_width = true;
                scrollbar.properties.layout.intrinsic_size.expand_padding.left = border_size;
                scrollbar.properties.layout.intrinsic_size.expand_padding.right = border_size + corner_size;

                layout.constrains.min.height += scrollbar.properties.layout.constrains.min.height;
                layout.constrains.min.width = Math.Max(layout.constrains.min.width, scrollbar.properties.layout.constrains.min.width + border_size * 2);
            }

            if (needs_vertical_bar)
            {
                var size = vertical_size;
                var oy = -stbg_clamp(window.properties.layout.children_offset.y, -size, 0);
                stbg__scrollbar_create("$$$$_window_scrollbar_vertical", STBG_SCROLLBAR_DIRECTION.VERTICAL, ref oy, 0, size, context.theme.default_font_style.size * scroll_lines_amount, true);
                window.properties.layout.children_offset.y = -oy;
                ref var scrollbar = ref stbg_get_last_widget();
                scrollbar.properties.layout.flags |= STBG_WIDGET_LAYOUT_FLAGS.PARENT_CONTROLLED;
                var offset = has_title ? stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM) : border_size;
                scrollbar.properties.layout.intrinsic_position.docking = STBG_INTRINSIC_POSITION_DOCKING.RIGHT;
                scrollbar.properties.layout.intrinsic_position.docking_padding = border_size;
                scrollbar.properties.layout.intrinsic_size.expand_height = true;
                scrollbar.properties.layout.intrinsic_size.expand_padding.top = offset;
                scrollbar.properties.layout.intrinsic_size.expand_padding.bottom = border_size;

                layout.constrains.min.width += scrollbar.properties.layout.constrains.min.width;
                layout.constrains.min.height = Math.Max(layout.constrains.min.height, scrollbar.properties.layout.constrains.min.height + offset + border_size);
            }

            context.last_widget_id = saved_last_widget_id;
            context.last_widget_is_new = saved_last_widget_is_new;
            context.current_widget_id = saved_current_widget_id;
        }
        else
        {
            window.properties.layout.children_offset.x = 0;
            window.properties.layout.children_offset.y = 0;
        }
    }

    private static bool stbg__window_get_children_scrolling_info(ref stbg_widget window, stbg__window_properties parameters, out bool needs_horizontal_bar, out bool needs_vertical_bar, out float horizontal_size, out float vertical_size)
    {
        ref var layout = ref window.properties.layout;
        ref var computed_bounds = ref window.properties.computed_bounds;

        var has_scrollbars = (parameters.options & STBG_WINDOW_OPTIONS.NO_SCROLLBAR) == 0;
        var ignored = (window.flags & STBG_WIDGET_FLAGS.IGNORE) != 0;
        var allow_children_overflow = (layout.flags & STBG_WIDGET_LAYOUT_FLAGS.ALLOW_CHILDREN_OVERFLOW) != 0;

        if (ignored || !has_scrollbars || !allow_children_overflow)
        {
            needs_vertical_bar = false;
            needs_horizontal_bar = false;
            horizontal_size = 0;
            vertical_size = 0;
            return false;
        }

        var children_size_with_padding = stbg_size_add_padding(computed_bounds.children_size, layout.inner_padding);

        var scrollbar_size = stbg__sum_styles(STBG_WIDGET_STYLE.SCROLLBAR_SIZE);

        needs_horizontal_bar = children_size_with_padding.width > computed_bounds.size.width;
        needs_vertical_bar = children_size_with_padding.height > computed_bounds.size.height;

        if (needs_horizontal_bar)
        {
            children_size_with_padding.height += scrollbar_size;
            // Re-evaluate vertical bar
            needs_vertical_bar = children_size_with_padding.height > computed_bounds.size.height;
        }

        if (needs_vertical_bar)
        {
            children_size_with_padding.width += scrollbar_size;
            // Re-evaluate horizontal bar
            needs_horizontal_bar = children_size_with_padding.width > computed_bounds.size.width;
        }

        horizontal_size = needs_horizontal_bar ? children_size_with_padding.width - computed_bounds.size.width : 0;
        vertical_size = needs_vertical_bar ? children_size_with_padding.height - window.properties.computed_bounds.size.height : 0;

        return needs_horizontal_bar || needs_vertical_bar;
    }

    private static bool stbg__window_update_input(ref stbg_widget window)
    {
        if (context.input_feedback.hovered_widget_id != window.id)
        {
            stbg__window_update_input_scrolling(ref window);

            return false;
        }

        ref var window_properties = ref stbg__get_widget_custom_properties_by_id_internal<stbg__window_properties>(window.id);

        var parent = stbg_get_widget_by_id(window.hierarchy.parent_id);

        var mouse_position = context.input.mouse_position;

        var bounds = window.properties.computed_bounds.global_rect;

        float resize_x, resize_y;

        bool allow_resize = (window_properties.options & STBG_WINDOW_OPTIONS.NO_RESIZE) == 0;
        bool allow_move = (window_properties.options & STBG_WINDOW_OPTIONS.NO_MOVE) == 0;
        bool has_title = (window_properties.options & STBG_WINDOW_OPTIONS.NO_TITLE) == 0;
        bool has_close_button = (window_properties.options & STBG_WINDOW_OPTIONS.CLOSE_BUTTON) != 0;

        if (context.input_feedback.pressed_widget_id == window.id)
        {
            resize_x = context.input_feedback.drag_resize_x;
            resize_y = context.input_feedback.drag_resize_y;
        }
        else if (allow_resize && parent.properties.layout.children_layout_direction == STBG_CHILDREN_LAYOUT.FREE)
        {
            stbg__window_get_corner_resize(context.input.mouse_position, bounds, out resize_x, out resize_y);
        }
        else
        {
            resize_x = resize_y = 0;
        }

        stbg__window_set_resize_cursor(resize_x, resize_y);

        float title_height_total = has_title ? stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP) : 0;

        bool mouse_over_title = has_title && context.input.mouse_position.y < Math.Min(bounds.y1, bounds.y0 + title_height_total);
        bool mouse_over_close_button = mouse_over_title && has_close_button && resize_x == 0 && resize_y == 0 &&
            context.input.mouse_position.x > Math.Max(bounds.x0, bounds.x1 - stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_SIZE));

        context.input_feedback.hovered_sub_widget_part = mouse_over_close_button ? 1 : 0;

        if (context.input_feedback.pressed_widget_id == window.id)
        {
            if (context.input.mouse_button_1)
            {
                if (allow_move && mouse_over_title && context.input_feedback.pressed_sub_widget_part == 0)
                {
                    context.input_feedback.pressed_widget_id = window.id;
                    context.input_feedback.hovered_widget_id = window.id;
                    context.input_feedback.dragged_widget_id = window.id;
                }
                else if (allow_resize && (resize_x != 0 || resize_y != 0))
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
                context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;

                if (has_close_button && context.input_feedback.pressed_sub_widget_part == 1 && mouse_over_close_button)
                {
                    // Close button pressed
                    window_properties.open = false;
                    window.properties.input_flags |= STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
                }
            }
        }
        else if (context.input.mouse_button_1_down)
        {
            context.input_feedback.pressed_widget_id = window.id;
            context.input_feedback.drag_resize_x = resize_x;
            context.input_feedback.drag_resize_y = resize_y;
            context.input_feedback.drag_from_widget_x = mouse_position.x - bounds.x0;
            context.input_feedback.drag_from_widget_y = mouse_position.y - bounds.y0;
            context.input_feedback.pressed_sub_widget_part = mouse_over_close_button ? 1 : 0;
            // Bringing the window to the top is handled by StbGui.Input
        }

        if (context.input_feedback.dragged_widget_id == window.id)
        {
            var parent_bounds = parent.properties.computed_bounds.global_rect;

            ref var intrinsic_size = ref window.properties.layout.intrinsic_size;
            ref var intrinsic_position = ref window.properties.layout.intrinsic_position;

            if (resize_x == 0 && resize_y == 0 && allow_move)
            {
                intrinsic_position.position.x = stbg_clamp(mouse_position.x - parent_bounds.x0 - context.input_feedback.drag_from_widget_x, 0, parent_bounds.x1 - (bounds.x1 - bounds.x0));
                intrinsic_position.position.y = stbg_clamp(mouse_position.y - parent_bounds.y0 - context.input_feedback.drag_from_widget_y, 0, parent_bounds.y1 - (bounds.y1 - bounds.y0));
            }
            else if (allow_resize)
            {
                if (resize_x < 0)
                {
                    var x = window.properties.computed_bounds.position.x;
                    intrinsic_position.position.x = MathF.Max(mouse_position.x - parent_bounds.x0, 0);
                    intrinsic_size.size.width = (bounds.x1 - bounds.x0) - parent_bounds.x0 + (x - intrinsic_position.position.x);

                    var min_width = window.properties.layout.constrains.min.width +
                            ((window.properties.layout.flags & STBG_WIDGET_LAYOUT_FLAGS.ALLOW_CHILDREN_OVERFLOW) == 0 ? window.properties.computed_bounds.children_size.width : 0);

                    if (intrinsic_size.size.width < min_width)
                    {
                        var delta = min_width - intrinsic_size.size.width;
                        intrinsic_size.size.width = min_width;
                        intrinsic_position.position.x -= delta;
                    }
                }
                else if (resize_x > 0)
                {
                    intrinsic_size.size.width = stbg_clamp(mouse_position.x - bounds.x0, 0, parent_bounds.x1 - bounds.x0);
                }

                if (resize_y < 0)
                {
                    var y = intrinsic_position.position.y;
                    intrinsic_position.position.y = MathF.Max(mouse_position.y - parent_bounds.y0, 0);
                    intrinsic_size.size.height = (bounds.y1 - bounds.y0) - parent_bounds.y0 + (y - intrinsic_position.position.y);

                    var min_height = window.properties.layout.constrains.min.height +
                            ((window.properties.layout.flags & STBG_WIDGET_LAYOUT_FLAGS.ALLOW_CHILDREN_OVERFLOW) == 0 ? window.properties.computed_bounds.children_size.height : 0);

                    if (intrinsic_size.size.height < min_height)
                    {
                        var delta = min_height - intrinsic_size.size.height;
                        intrinsic_size.size.height = min_height;
                        intrinsic_position.position.y -= delta;
                    }
                }
                else if (resize_y > 0)
                {
                    intrinsic_size.size.height = stbg_clamp(mouse_position.y - bounds.y0, 0, parent_bounds.y1 - bounds.y0);
                }
            }
        }

        stbg__window_update_input_scrolling(ref window);

        return
            context.input.mouse_button_1_down ||
            context.input.mouse_button_1_up ||
            context.input.mouse_button_1 ||
            context.input.mouse_wheel_scroll_amount.x != 0 ||
            context.input.mouse_wheel_scroll_amount.y != 0;
    }

    private static bool stbg__window_update_input_scrolling(ref stbg_widget window)
    {
        ref var window_properties = ref stbg__get_widget_custom_properties_by_id_internal<stbg__window_properties>(window.id);

        if (context.input.mouse_wheel_scroll_amount.x != 0 || context.input.mouse_wheel_scroll_amount.y != 0)
        {
            // If we are the first window parent of the hovered widget then handle the scrolling ourselves
            if (!stbg__find_widget_parent_by_type(context.input_feedback.hovered_widget_id, STBG_WIDGET_TYPE.WINDOW, out var hovered_parent_id) || hovered_parent_id != window.id)
                return false;

            if (stbg__window_get_children_scrolling_info(ref window, window_properties, out var needs_horizontal_bar, out var needs_vertical_bar, out var horizontal_size, out var vertical_size))
            {
                var scroll_lines_amount = stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_SCROLL_LINES_AMOUNT);

                if (needs_vertical_bar && context.input.mouse_wheel_scroll_amount.y != 0)
                {
                    var dy = context.theme.default_font_style.size * scroll_lines_amount * context.input.mouse_wheel_scroll_amount.y;

                    window.properties.layout.children_offset.y = stbg_clamp(window.properties.layout.children_offset.y + dy, -vertical_size, 0);
                }

                if (needs_horizontal_bar && context.input.mouse_wheel_scroll_amount.x != 0)
                {
                    var dx = context.theme.default_font_style.size * scroll_lines_amount * -context.input.mouse_wheel_scroll_amount.x;

                    window.properties.layout.children_offset.x = stbg_clamp(window.properties.layout.children_offset.x + dx, -horizontal_size, 0);
                }

                return true;
            }
        }

        return false;
    }

    private static void stbg__window_set_resize_cursor(float resize_x, float resize_y)
    {
        if (resize_x == 0 && resize_y == 0)
            return;


        STBG_ACTIVE_CURSOR_TYPE cursor = STBG_ACTIVE_CURSOR_TYPE.DEFAULT;

        /// NW   N    NE
        ///      |
        /// W ---*--- E
        ///      |
        /// SW   S    SE

        if (resize_x < 0 && resize_y < 0)
        {
            cursor = STBG_ACTIVE_CURSOR_TYPE.RESIZE_NW;
        }
        else if (resize_x == 0 && resize_y < 0)
        {
            cursor = STBG_ACTIVE_CURSOR_TYPE.RESIZE_N;
        }
        else if (resize_x > 0 && resize_y < 0)
        {
            cursor = STBG_ACTIVE_CURSOR_TYPE.RESIZE_NE;
        }
        else if (resize_x > 0 && resize_y == 0)
        {
            cursor = STBG_ACTIVE_CURSOR_TYPE.RESIZE_E;
        }
        else if (resize_x > 0 && resize_y > 0)
        {
            cursor = STBG_ACTIVE_CURSOR_TYPE.RESIZE_SE;
        }
        else if (resize_x == 0 && resize_y > 0)
        {
            cursor = STBG_ACTIVE_CURSOR_TYPE.RESIZE_S;
        }
        else if (resize_x < 0 && resize_y > 0)
        {
            cursor = STBG_ACTIVE_CURSOR_TYPE.RESIZE_SW;
        }
        else if (resize_x < 0 && resize_y == 0)
        {
            cursor = STBG_ACTIVE_CURSOR_TYPE.RESIZE_W;
        }

        context.active_cursor = cursor;
    }

    private static void stbg__window_get_corner_resize(stbg_position mouse_position, stbg_rect bounds, out float resize_x, out float resize_y)
    {
        float border_resize_tolerance = stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_RESIZE_TOLERANCE);

        if (stbg_is_value_near(mouse_position.y, bounds.y0, border_resize_tolerance))
            resize_y = -1;
        else if (stbg_is_value_near(mouse_position.y, bounds.y1, border_resize_tolerance))
            resize_y = 1;
        else
            resize_y = 0;

        if (stbg_is_value_near(mouse_position.x, bounds.x0, border_resize_tolerance))
            resize_x = -1;
        else if (stbg_is_value_near(mouse_position.x, bounds.x1, border_resize_tolerance))
            resize_x = 1;
        else
            resize_x = 0;
    }

    private static void stbg__window_render(ref stbg_widget window)
    {
        ref var window_ref_props = ref stbg__get_widget_ref_props_by_id_internal(window.id);
        ref var window_properties = ref stbg__get_widget_custom_properties_by_id_internal<stbg__window_properties>(window.id);

        var has_title = (window_properties.options & STBG_WINDOW_OPTIONS.NO_TITLE) == 0;
        var has_close_button = (window_properties.options & STBG_WINDOW_OPTIONS.CLOSE_BUTTON) != 0;

        var size = window.properties.computed_bounds.size;

        bool active = context.input_feedback.active_window_id == window.id;

        var title_background_color = stbg_get_widget_style_color(active ? STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR : STBG_WIDGET_STYLE.WINDOW_TITLE_BACKGROUND_COLOR);
        var title_text_color = stbg_get_widget_style_color(active ? STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_TEXT_COLOR : STBG_WIDGET_STYLE.WINDOW_TITLE_TEXT_COLOR);

        if (window.hash == debug_window_hash)
        {
            title_background_color = stbg_get_widget_style_color(active ? STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR : STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_BACKGROUND_COLOR);
            title_text_color = stbg_get_widget_style_color(active ? STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_TEXT_COLOR : STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_TEXT_COLOR);
        }

        // background and border
        stbg__rc_draw_border(
            stbg_build_rect(0, 0, size.width, size.height),
            stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE),
            stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_BORDER_COLOR),
            stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_BACKGROUND_COLOR)
        );
        // title background
        if (has_title)
        {
            stbg__rc_draw_border(
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
            stbg__rc_draw_text(
                stbg_build_rect(
                    stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT),
                    stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP),
                    size.width - stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT) - (has_close_button ? stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_SIZE) : 0),
                    stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT)
                ),
                stbg__build_text(window_ref_props.text, title_text_color),
                -1, 0, // center vertically
                STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE | STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE
            );

            if (has_close_button)
            {
                var close_button_hovered = context.input_feedback.hovered_widget_id == window.id && context.input_feedback.hovered_sub_widget_part == 1;
                var close_button_pressed = context.input_feedback.pressed_sub_widget_part == window.id && context.input_feedback.pressed_sub_widget_part == 1;

                var close_button_color = stbg_get_widget_style_color_normal_hovered_pressed(
                    STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_COLOR,
                    STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_HOVERED_COLOR,
                    STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_PRESSED_COLOR,
                    close_button_hovered,
                    close_button_pressed
                );

                var close_button_size = stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_CLOSE_BUTTON_SIZE);

                var close_button_bounds = stbg_build_rect(
                    size.width - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT) - close_button_size,
                    stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE),
                    size.width - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
                    stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT)
                );

                stbg__rc_draw_text(close_button_bounds, stbg__build_text(STBG__WINDOW_CLOSE_BUTTON, close_button_color), 0, 0, STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS);
            }
        }
    }

    private static readonly ReadOnlyMemory<char> STBG__WINDOW_CLOSE_BUTTON = "X".AsMemory();
}
