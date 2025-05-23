#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;

using font_id = int;
using image_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{
    private struct stbg__button_image_properties
    {
        public image_id image;
        public image_id image_hover;
        public image_id image_pressed;
        public bool border;
    }

    private static stbg__marshal_info<stbg__button_image_properties> stbg__button_image_properties_marshal_info = new();

    private static void stbg__button_image_init_default_theme()
    {
    }


    private static ref stbg_widget stbg__button_image_create(ReadOnlySpan<char> identifier, image_id image, image_id image_hover, image_id image_pressed, bool border, float scale)
    {
        ref var button_image = ref stbg__add_widget(STBG_WIDGET_TYPE.BUTTON_IMAGE, identifier, out var is_new);
        ref var button_props = ref stbg__add_widget_custom_properties_by_id_internal(stbg__button_image_properties_marshal_info, button_image.id, is_new);

        button_props.image = image;
        button_props.image_hover = image_hover;
        button_props.image_pressed = image_pressed;
        button_props.border = border;

        button_image.properties.image = image;

        ref var image_info = ref context.images[image];

        ref var layout = ref button_image.properties.layout;

        var padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
        };

        var image_size = stbg_build_size(
            MathF.Round(image_info.size.width * scale),
            MathF.Round(image_info.size.height * scale)
        );

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.intrinsic_size.size = border ? stbg_size_add_padding(image_size, padding) : image_size;

        return ref button_image;
    }

    private static bool stbg__button_image_update_input(ref stbg_widget button_image)
    {
        // Reuse button input handling
        return stbg__button_update_input(ref button_image);
    }

    private static void stbg__button_image_render(ref stbg_widget button_image)
    {
        var size = button_image.properties.computed_bounds.size;

        bool hovered = context.input_feedback.hovered_widget_id == button_image.id;
        bool pressed = context.input_feedback.pressed_widget_id == button_image.id;

        ref var parameters = ref stbg__get_widget_custom_properties_by_id_internal<stbg__button_image_properties>(button_image.id);

        bool has_hovered_image = parameters.image_hover != STBG_IMAGE_ID_NULL;
        bool has_pressed_image = parameters.image_pressed != STBG_IMAGE_ID_NULL;

        var image_id =
            (pressed && has_pressed_image) ? parameters.image_pressed :
            (hovered && has_hovered_image) ? parameters.image_hover :
            parameters.image;

        ref var image = ref context.images[image_id];

        var border_size = parameters.border ? stbg_get_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE) : 0;

        if (parameters.border)
        {
            var border_color = stbg_get_widget_style_color(pressed ? STBG_WIDGET_STYLE.BUTTON_PRESSED_BORDER_COLOR : hovered ? STBG_WIDGET_STYLE.BUTTON_HOVERED_BORDER_COLOR : STBG_WIDGET_STYLE.BUTTON_BORDER_COLOR);

            stbg__rc_draw_border(
                stbg_build_rect(0, 0, size.width, size.height),
                stbg_get_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE),
                border_color,
                STBG_COLOR_TRANSPARENT
            );
        }

        stbg__rc_draw_image(
            stbg_build_rect(
                border_size,
                border_size,
                border_size + size.width - border_size * 2,
                border_size + size.height - border_size * 2
            ),
            image,
            STBG_COLOR_WHITE
        );
    }
}
