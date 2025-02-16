#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private delegate void stbg__widget_render_delegate(ref stbg_widget widget, ref stbg_render_context render_context);
    private delegate void stbg__widget_update_input_delegate(ref stbg_widget widget);

    static private Dictionary<STBG_WIDGET_TYPE, stbg__widget_render_delegate> STBG__WIDGET_RENDER_DICTIONARY = new() {
        { STBG_WIDGET_TYPE.WINDOW, stbg__window_render },
        { STBG_WIDGET_TYPE.BUTTON, stbg__button_render },
    };

    static private Dictionary<STBG_WIDGET_TYPE, stbg__widget_update_input_delegate> STBG__WIDGET_UPDATE_INPUT_DICTIONARY = new() {
        { STBG_WIDGET_TYPE.WINDOW, stbg__window_update_input },
        { STBG_WIDGET_TYPE.BUTTON, stbg__button_update_input },
    };

    static private stbg__widget_render_delegate?[] STBG__WIDGET_RENDER_MAP = new stbg__widget_render_delegate[(int)STBG_WIDGET_TYPE.COUNT];
    static private stbg__widget_update_input_delegate?[] STBG__WIDGET_UPDATE_INPUT_MAP = new stbg__widget_update_input_delegate[(int)STBG_WIDGET_TYPE.COUNT];

    static StbGui()
    {
        // Init maps
        for (var type = STBG_WIDGET_TYPE.NONE; type < STBG_WIDGET_TYPE.COUNT; type++)
        {
            var render = STBG__WIDGET_RENDER_DICTIONARY.GetValueOrDefault(type);
            var update_input = STBG__WIDGET_UPDATE_INPUT_DICTIONARY.GetValueOrDefault(type);

            STBG__WIDGET_RENDER_MAP[(int) type] = render;
            STBG__WIDGET_UPDATE_INPUT_MAP[(int) type] = update_input;
        }
    }
}