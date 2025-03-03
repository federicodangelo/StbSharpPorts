#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static ref stbg_widget stbg__container_create(ReadOnlySpan<char> identifier, STBG_CHILDREN_LAYOUT layout_direction, stbg_widget_constrains constrains, float spacing)
    {
        ref var container = ref stbg__add_widget(STBG_WIDGET_TYPE.CONTAINER, identifier, out _);

        container.flags |= STBG_WIDGET_FLAGS.ALLOW_CHILDREN;

        ref var layout = ref container.properties.layout;

        layout.inner_padding = new stbg_padding();
        layout.constrains = constrains;
        layout.children_layout_direction = layout_direction;
        layout.children_spacing = spacing;

        return ref container;               
    }
}
