#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static void stbg__button_init_default_theme()
    {
        var font_style = context.theme.default_font_style;

        // BUTTON
        var buttonBorder = 1.0f;
        var buttonPaddingTopBottom = MathF.Ceiling(font_style.size / 2);
        var buttonPaddingLeftRight = MathF.Ceiling(font_style.size / 2);

        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, buttonBorder);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_TOP, buttonPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM, buttonPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT, buttonPaddingLeftRight);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT, buttonPaddingLeftRight);

        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_BACKGROUND_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_TEXT_COLOR, rgb(236, 240, 241));

        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_HOVERED_BORDER_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_HOVERED_BACKGROUND_COLOR, rgb(52, 152, 219));
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_HOVERED_TEXT_COLOR, rgb(236, 240, 241));

        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PRESSED_BORDER_COLOR, rgb(41, 128, 185));
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PRESSED_BACKGROUND_COLOR, rgb(46, 204, 113));
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PRESSED_TEXT_COLOR, rgb(236, 240, 241));
    }

    private static ref stbg_widget stbg__button_create(ReadOnlySpan<char> label)
    {
        ref var button = ref stbg__add_widget(STBG_WIDGET_TYPE.BUTTON, label, out _);

        button.properties.text = stbg__add_string(label);

        ref var layout = ref button.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT),
        };
        layout.intrinsic_size = stbg__build_intrinsic_size_text();

        return ref button;
    }

    private static bool stbg__button_update_input(ref stbg_widget button)
    {
        if (context.input_feedback.hovered_widget_id != button.id)
            return false;

        if (context.input.mouse_button_1_down)
            context.input_feedback.pressed_widget_id = button.id;

        if (context.input.mouse_button_1_up && context.input_feedback.pressed_widget_id == button.id)
        {
            button.properties.input_flags |= STBG_WIDGET_INPUT_FLAGS.CLICKED;
            context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
        }

        return 
            context.input.mouse_button_1_down || 
            context.input.mouse_button_1_up || 
            context.input.mouse_button_1;
    }

    private static void stbg__button_render(ref stbg_widget button)
    {
        var size = button.properties.computed_bounds.size;

        bool hovered = context.input_feedback.hovered_widget_id == button.id;
        bool pressed = context.input_feedback.pressed_widget_id == button.id;

        var border_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_BORDER_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_BORDER_COLOR : STBG_WIDGET_STYLE.BUTTON_BORDER_COLOR);
        var background_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_BACKGROUND_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_BACKGROUND_COLOR : STBG_WIDGET_STYLE.BUTTON_BACKGROUND_COLOR);
        var text_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_TEXT_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_TEXT_COLOR : STBG_WIDGET_STYLE.BUTTON_TEXT_COLOR);

        stbg__rc_draw_border(
            stbg_build_rect(0, 0, size.width, size.height),
            stbg_get_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
            border_color,
            background_color
        );
        stbg__rc_draw_text(
            stbg_build_rect(
                stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT),
                stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_TOP),
                size.width - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT),
                size.height - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM)
            ),
            stbg__build_text(button.properties.text, text_color),
            0, 0, STBG_RENDER_TEXT_OPTIONS.SINGLE_LINE
        );
    }
}