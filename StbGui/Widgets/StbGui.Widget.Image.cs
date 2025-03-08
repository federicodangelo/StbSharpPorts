#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;
using image_id = int;

public partial class StbGui
{
    private struct stbg__image_parameters
    {
        public float scale;
    }

    private static void stbg__image_init_default_theme()
    {
    }


    private static void stbg__image_set_parameters(ref stbg_widget widget, stbg__image_parameters parameters)
    {
        widget.properties.parameters.parameter2.f = parameters.scale;
    }

    private static void stbg__image_get_parameters(ref stbg_widget widget, out stbg__image_parameters parameters)
    {
        parameters = new stbg__image_parameters();
        parameters.scale = widget.properties.parameters.parameter2.f;
    }

    private static ref stbg_widget stbg__image_create(ReadOnlySpan<char> identifier, image_id image_id, float scale)
    {
        ref var image_widget = ref stbg__add_widget(STBG_WIDGET_TYPE.IMAGE, identifier, out _);
        ref var image = ref context.images[image_id];

        image_widget.properties.image = image_id;

        stbg__image_set_parameters(ref image_widget,
            new stbg__image_parameters
            {
                scale = scale
            }
        );

        ref var layout = ref image_widget.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.intrinsic_size.size = stbg_build_size(
            MathF.Round(image.size.width * scale),
            MathF.Round(image.size.height * scale)
        );

        return ref image_widget;
    }

    private static void stbg__image_render(ref stbg_widget image_widget)
    {
        var size = image_widget.properties.computed_bounds.size;

        stbg__image_get_parameters(ref image_widget, out var parameters);

        ref var image = ref context.images[image_widget.properties.image];

        stbg__rc_draw_image(
            stbg_build_rect(0, 0, size.width, size.height),
            image.original_image_id,
            STBG_COLOR_WHITE,
            image.rect
        );
    }
}