#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;
using image_id = int;
using System.Drawing;

public partial class StbGui
{
    private static void stbg__image_init_default_theme()
    {
    }

    private static ref stbg_widget stbg__image_create(ReadOnlySpan<char> identifier, image_id image_id)
    {
        ref var image_widget = ref stbg__add_widget(STBG_WIDGET_TYPE.IMAGE, identifier, out _);
        ref var image = ref context.images[image_id];

        image_widget.properties.value.i = image_id;

        ref var layout = ref image_widget.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.intrinsic_size.size = image.size;

        return ref image_widget;
    }

    private static void stbg__image_render(ref stbg_widget image_widget)
    {
        var size = image_widget.properties.computed_bounds.size;
        ref var image = ref context.images[image_widget.properties.value.i];

        stbg__rc_draw_image(
            stbg_build_rect(0, 0, size.width, size.height),
            image.id,
            STBG_COLOR_WHITE,
            image.source_rect
        );
    }
}