#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;

using font_id = int;
using image_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{
    private static void stbg__image_init_default_theme()
    {
    }


    private static ref stbg_widget stbg__image_create(ReadOnlySpan<char> identifier, image_id image_id, float scale)
    {
        ref var image_widget = ref stbg__add_widget(STBG_WIDGET_TYPE.IMAGE, identifier, out _);
        ref var image = ref context.images[image_id];

        image_widget.properties.image = image_id;

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

        ref var image = ref context.images[image_widget.properties.image];

        stbg__rc_draw_image(
            stbg_build_rect(0, 0, size.width, size.height),
            image,
            STBG_COLOR_WHITE
        );
    }
}
