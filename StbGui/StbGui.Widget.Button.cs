#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static ref stbg_widget stbg__button_create(string label)
    {
        ref var button = ref stbg__add_widget(STBG_WIDGET_TYPE.BUTTON, label, out _);

        button.text = label.AsMemory();

        ref var layout = ref button.layout;

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

    private static void stbg__button_update_input(ref stbg_widget button)
    {
        if (context.input_feedback.hovered_widget_id != button.id)
            return;

        if (context.io.mouse_button_1_down)
            context.input_feedback.pressed_widget_id = button.id;

        if (!context.io.mouse_button_1_down && context.input_feedback.pressed_widget_id == button.id)
        {
            button.flags |= STBG_WIDGET_FLAGS.CLICKED;
            context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
        }
    }

    private static void stbg__button_render(ref stbg_widget button, ref stbg_render_context render_context)
    {
        var size = button.computed_bounds.size;

        bool hovered = context.input_feedback.hovered_widget_id == button.id;
        bool pressed = context.input_feedback.pressed_widget_id == button.id;

        var border_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_BORDER_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_BORDER_COLOR : STBG_WIDGET_STYLE.BUTTON_BORDER_COLOR);
        var background_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_BACKGROUND_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_BACKGROUND_COLOR : STBG_WIDGET_STYLE.BUTTON_BACKGROUND_COLOR);
        var text_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_TEXT_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_TEXT_COLOR : STBG_WIDGET_STYLE.BUTTON_TEXT_COLOR);

        render_context.draw_border(
            stbg_build_rect(0, 0, size.width, size.height),
            stbg_get_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
            border_color,
            background_color
        );
        render_context.draw_text(
            stbg_build_rect(
                stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT),
                stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_TOP),
                size.width - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT),
                size.height - stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM)
            ),
            stbg__build_text(button.text, text_color)
        );
    }
}