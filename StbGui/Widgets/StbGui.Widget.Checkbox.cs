#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;

using font_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{
    private static void stbg__checkbox_init_default_theme()
    {
        var font_style = context.theme.default_font_style;

        // CHECKBOX
        var checkboxBorder = 1.0f;
        var checkboxSize = MathF.Ceiling(font_style.size);
        var checkboxTextPadding = MathF.Ceiling(font_style.size * 0.25f);
        var checkboxPaddingTopBottom = 2.0f;

        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_SIZE, checkboxSize);
        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_BORDER_SIZE, checkboxBorder);

        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_PADDING_TOP, checkboxPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_PADDING_BOTTOM, checkboxPaddingTopBottom);

        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_TEXT_PADDING, checkboxTextPadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_TEXT_COLOR, rgb(44, 62, 80));

        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_BORDER_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_BACKGROUND_COLOR, STBG_COLOR_TRANSPARENT);

        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_HOVERED_BORDER_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_HOVERED_BACKGROUND_COLOR, rgba(52, 152, 219, 0.25));

        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_CHECKED_BORDER_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.CHECKBOX_CHECKED_BACKGROUND_COLOR, rgb(44, 62, 80));
    }

    private static ref stbg_widget stbg__checkbox_create(ReadOnlySpan<char> label, ref bool value)
    {
        ref var checkbox = ref stbg__add_widget(STBG_WIDGET_TYPE.CHECKBOX, label, out var is_new);
        ref var checkbox_ref_props = ref stbg__get_widget_ref_props_by_id_internal(checkbox.id);

        checkbox_ref_props.text = stbg__add_string(label);

        ref var layout = ref checkbox.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        var padding = new stbg_padding()
        {
            top = stbg_get_widget_style(STBG_WIDGET_STYLE.CHECKBOX_PADDING_TOP),
            bottom = stbg_get_widget_style(STBG_WIDGET_STYLE.CHECKBOX_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.CHECKBOX_SIZE, STBG_WIDGET_STYLE.CHECKBOX_TEXT_PADDING),
            right = 0,
        };

        var text_size = stbg__measure_text(stbg__build_text(checkbox_ref_props.text), STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE | STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE);
        text_size = stbg_size_add_padding(text_size, padding);

        layout.intrinsic_size.size = text_size;

        if (is_new || (checkbox.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED) == 0)
        {
            checkbox.properties.value.b = value;
        }
        else
        {
            value = checkbox.properties.value.b;
            checkbox.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }

        return ref checkbox;
    }

    private static bool stbg__checkbox_update_input(ref stbg_widget checkbox)
    {
        if (context.input_feedback.hovered_widget_id != checkbox.id)
            return false;

        if (context.input.mouse_button_1_down)
            context.input_feedback.pressed_widget_id = checkbox.id;

        if (context.input.mouse_button_1_up && context.input_feedback.pressed_widget_id == checkbox.id)
        {
            checkbox.properties.input_flags |= STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
            checkbox.properties.value.b = !checkbox.properties.value.b;
            context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
        }

        return
            context.input.mouse_button_1_down ||
            context.input.mouse_button_1_up ||
            context.input.mouse_button_1;
    }

    private static void stbg__checkbox_render(ref stbg_widget checkbox)
    {
        var size = checkbox.properties.computed_bounds.size;
        ref var checkbox_ref_props = ref stbg__get_widget_ref_props_by_id_internal(checkbox.id);

        bool hovered = context.input_feedback.hovered_widget_id == checkbox.id;
        bool is_checked = checkbox.properties.value.b;

        var border_color = stbg_get_widget_style_color(is_checked ? STBG_WIDGET_STYLE.CHECKBOX_CHECKED_BORDER_COLOR : hovered ? STBG_WIDGET_STYLE.CHECKBOX_HOVERED_BORDER_COLOR : STBG_WIDGET_STYLE.CHECKBOX_BORDER_COLOR);
        var background_color = stbg_get_widget_style_color(is_checked ? STBG_WIDGET_STYLE.CHECKBOX_CHECKED_BACKGROUND_COLOR : hovered ? STBG_WIDGET_STYLE.CHECKBOX_HOVERED_BACKGROUND_COLOR : STBG_WIDGET_STYLE.CHECKBOX_BACKGROUND_COLOR);
        var text_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.CHECKBOX_TEXT_COLOR);

        var checkbox_size = stbg_get_widget_style(STBG_WIDGET_STYLE.CHECKBOX_SIZE);
        var vertical_alignment = MathF.Floor((size.height - checkbox_size) / 2);

        var checkbox_rect = stbg_build_rect(0, vertical_alignment, checkbox_size, vertical_alignment + checkbox_size);

        stbg__rc_draw_border(
            checkbox_rect,
            stbg_get_widget_style(STBG_WIDGET_STYLE.CHECKBOX_BORDER_SIZE),
            border_color,
            STBG_COLOR_TRANSPARENT
        );

        if (is_checked || hovered)
        {
            stbg__rc_draw_rectangle(
                stbg_build_rect(checkbox_rect.x0 + 2, checkbox_rect.y0 + 2, checkbox_rect.x1 - 2, checkbox_rect.y1 - 2),
                background_color
            );
        }

        stbg__rc_draw_text(
            stbg_build_rect(
                stbg__sum_styles(STBG_WIDGET_STYLE.CHECKBOX_SIZE, STBG_WIDGET_STYLE.CHECKBOX_TEXT_PADDING),
                stbg__sum_styles(STBG_WIDGET_STYLE.CHECKBOX_PADDING_TOP),
                size.width,
                size.height - stbg__sum_styles(STBG_WIDGET_STYLE.CHECKBOX_PADDING_BOTTOM)
            ),
            stbg__build_text(checkbox_ref_props.text, text_color),
            0, 0,
            STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE | STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE
        );
    }
}
