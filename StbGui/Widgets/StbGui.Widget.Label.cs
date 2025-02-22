#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static void stbg__label_init_default_theme()
    {
        // LABEL
        var labelPaddingTopBottom = 0;
        var labelPaddingLeftRight = 0;

        stbg_set_widget_style(STBG_WIDGET_STYLE.LABEL_PADDING_TOP, labelPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.LABEL_PADDING_BOTTOM, labelPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.LABEL_PADDING_LEFT, labelPaddingLeftRight);
        stbg_set_widget_style(STBG_WIDGET_STYLE.LABEL_PADDING_RIGHT, labelPaddingLeftRight);

        stbg_set_widget_style(STBG_WIDGET_STYLE.LABEL_BACKGROUND_COLOR, STBG_COLOR_TRANSPARENT);
        stbg_set_widget_style(STBG_WIDGET_STYLE.LABEL_TEXT_COLOR, rgb(44, 62, 80));
    }

    private static ref stbg_widget stbg__label_create(ReadOnlySpan<char> text)
    {
        ref var label = ref stbg__add_widget(STBG_WIDGET_TYPE.LABEL, text, out _);

        label.properties.text = stbg__add_string(text);

        ref var layout = ref label.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.constrains.min.height = context.theme.default_font_style.size;
        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.LABEL_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.LABEL_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.LABEL_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.LABEL_PADDING_RIGHT),
        };
        layout.intrinsic_size = stbg__build_intrinsic_size_text();

        return ref label;
    }

    private static void stbg__label_render(ref stbg_widget label)
    {
        var size = label.properties.computed_bounds.size;

        var background_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.LABEL_BACKGROUND_COLOR);
        var text_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.LABEL_TEXT_COLOR);

        stbg__rc_draw_rectangle(
            stbg_build_rect(0, 0, size.width, size.height),
            background_color
        );

        stbg__rc_draw_text(
            stbg_build_rect(
                stbg__sum_styles(STBG_WIDGET_STYLE.LABEL_PADDING_LEFT),
                stbg__sum_styles(STBG_WIDGET_STYLE.LABEL_PADDING_TOP),
                size.width - stbg__sum_styles(STBG_WIDGET_STYLE.LABEL_PADDING_RIGHT),
                size.height - stbg__sum_styles(STBG_WIDGET_STYLE.LABEL_PADDING_BOTTOM)
            ),
            stbg__build_text(label.properties.text, text_color)
        );
    }
}