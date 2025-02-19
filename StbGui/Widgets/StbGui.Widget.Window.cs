#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
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
        var windowChidlrenSpacing = MathF.Ceiling(font_style.size / 4);
        var windowSpacingBetweenNewWindows = MathF.Ceiling(font_style.size / 2);
        var windowBorderResizeTolerance = MathF.Ceiling(font_style.size / 4);

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
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING, windowChidlrenSpacing);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, windowDefaultWidth);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, windowDefaultHeight);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS, windowSpacingBetweenNewWindows);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_RESIZE_TOLERANCE, windowBorderResizeTolerance);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BACKGROUND_COLOR, rgb(189, 195, 199));

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_TEXT_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_BACKGROUND_COLOR, rgb(44, 62, 80));

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_TEXT_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR, rgb(52, 73, 94));

        // DEBUG WINDOW
        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_TEXT_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_BACKGROUND_COLOR, rgb(192, 57, 43));

        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_TEXT_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR, rgb(231, 76, 60));
    }

    private static ref stbg_widget stbg__window_create(string title)
    {
        ref var window = ref stbg__add_widget(STBG_WIDGET_TYPE.WINDOW, title, out var is_new);

        stbg__window_init(ref window, is_new, title);

        return ref window;
    }

    private static void stbg__window_init(ref stbg_widget window, bool is_new, string title)
    {
        window.properties.text = title.AsMemory();

        ref var layout = ref window.properties.layout;

        window.properties.mouse_tolerance = stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_RESIZE_TOLERANCE);

        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_RIGHT),
        };
        layout.constrains = stbg_build_constrains_unconstrained();
        layout.children_layout_direction = STBG_CHILDREN_LAYOUT.VERTICAL;
        layout.children_spacing = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING);

        if (is_new)
        {
            window.properties.layout.intrinsic_position = context.next_new_window_position;
            window.properties.layout.intrinsic_size = stbg__build_intrinsic_size_pixels(stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH), stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT));

            context.next_new_window_position = stbg_offset_position(context.next_new_window_position, stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS), stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS));
        }
    }

    private static void stbg__window_update_input(ref stbg_widget window)
    {
        if (context.input_feedback.hovered_widget_id != window.id)
            return;

        var parent = stbg_get_widget_by_id(window.hierarchy.parent_id);

        var mouse_position = context.input.mouse_position;

        var bounds = window.properties.computed_bounds.global_rect;

        float resize_x, resize_y;

        if (context.input_feedback.pressed_widget_id == window.id)
        {
            resize_x = context.input_feedback.drag_resize_x;
            resize_y = context.input_feedback.drag_resize_y;
        }
        else if (parent.properties.layout.children_layout_direction == STBG_CHILDREN_LAYOUT.FREE)
        {
            stbg__window_get_corner_resize(context.input.mouse_position, bounds, out resize_x, out resize_y);
        }
        else
        {
            resize_x = resize_y = 0;
        }

        stbg__window_set_resize_cursor(resize_x, resize_y);

        float title_height_total = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP);

        bool mouse_over_title = context.input.mouse_position.y < Math.Min(bounds.y1, bounds.y0 + title_height_total);

        if (context.input_feedback.pressed_widget_id == window.id)
        {
            if (context.input.mouse_button_1)
            {
                if (mouse_over_title || resize_x != 0 || resize_y != 0)
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
            }
        }
        else if (context.input.mouse_button_1_down)
        {
            context.input_feedback.pressed_widget_id = window.id;
            context.input_feedback.active_widget_id = window.id;
            context.input_feedback.drag_resize_x = resize_x;
            context.input_feedback.drag_resize_y = resize_y;
            context.input_feedback.drag_from_widget_x = mouse_position.x - bounds.x0;
            context.input_feedback.drag_from_widget_y = mouse_position.y - bounds.y0;
            window.properties.layout.intrinsic_sorting_index = int.MaxValue;
        }

        if (context.input_feedback.dragged_widget_id == window.id)
        {
            var parent_bounds = parent.properties.computed_bounds.global_rect;

            ref var intrinsic_size = ref window.properties.layout.intrinsic_size.size;
            ref var intrinsic_position = ref window.properties.layout.intrinsic_position;

            if (resize_x == 0 && resize_y == 0)
            {
                intrinsic_position.x = Math.Clamp(mouse_position.x - parent_bounds.x0 - context.input_feedback.drag_from_widget_x, 0, parent_bounds.x1 - (bounds.x1 - bounds.x0));
                intrinsic_position.y = Math.Clamp(mouse_position.y - parent_bounds.y0 - context.input_feedback.drag_from_widget_y, 0, parent_bounds.y1 - (bounds.y1 - bounds.y0));
            }
            else
            {
                if (resize_x < 0)
                {
                    var x = window.properties.computed_bounds.position.x;
                    intrinsic_position.x = MathF.Max(mouse_position.x - parent_bounds.x0, 0);
                    intrinsic_size.width = (bounds.x1 - bounds.x0) - parent_bounds.x0 + (x - intrinsic_position.x);

                    var min_width = window.properties.computed_bounds.children_size.width + window.properties.layout.inner_padding.left + window.properties.layout.inner_padding.right;
                    if (intrinsic_size.width < min_width)
                    {
                        var delta = min_width - intrinsic_size.width;
                        intrinsic_size.width = min_width;
                        intrinsic_position.x -= delta;
                    }
                }
                else if (resize_x > 0)
                {
                    intrinsic_size.width = Math.Clamp(mouse_position.x - bounds.x0, 0, parent_bounds.x1 - bounds.x0);
                }

                if (resize_y < 0)
                {
                    var y = intrinsic_position.y;
                    intrinsic_position.y = MathF.Max(mouse_position.y - parent_bounds.y0, 0);
                    intrinsic_size.height = (bounds.y1 - bounds.y0) - parent_bounds.y0 + (y - intrinsic_position.y);

                    var min_height = window.properties.computed_bounds.children_size.height + window.properties.layout.inner_padding.top + window.properties.layout.inner_padding.bottom;
                    if (intrinsic_size.height < min_height)
                    {
                        var delta = min_height - intrinsic_size.height;
                        intrinsic_size.height = min_height;
                        intrinsic_position.y -= delta;
                    }
                }
                else if (resize_y > 0)
                {
                    intrinsic_size.height = Math.Clamp(mouse_position.y - bounds.y0, 0, parent_bounds.y1 - bounds.y0);
                }
            }
        }
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

    private static void stbg__window_render(ref stbg_widget window, ref stbg_render_context render_context)
    {
        var size = window.properties.computed_bounds.size;

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
            stbg__build_text(window.properties.text, title_text_color),
            -1, 0 // center vertically
        );
    }
}
