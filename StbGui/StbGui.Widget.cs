#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;

using font_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{
    private delegate void stbg__widget_render_delegate(ref stbg_widget widget);
    private delegate bool stbg__widget_update_input_delegate(ref stbg_widget widget);
    private delegate void stbg__widget_init_default_theme_delegate();
    private delegate void stbg__widget_init_context_delegate(ref stbg_context context);

    private static readonly Dictionary<STBG_WIDGET_TYPE, stbg__widget_render_delegate> STBG__WIDGET_RENDER_DICTIONARY = new() {
        { STBG_WIDGET_TYPE.WINDOW, stbg__window_render },
        { STBG_WIDGET_TYPE.BUTTON, stbg__button_render },
        { STBG_WIDGET_TYPE.BUTTON_IMAGE, stbg__button_image_render },
        { STBG_WIDGET_TYPE.CHECKBOX, stbg__checkbox_render },
        { STBG_WIDGET_TYPE.LABEL, stbg__label_render },
        { STBG_WIDGET_TYPE.SCROLLBAR, stbg__scrollbar_render },
        { STBG_WIDGET_TYPE.TEXTBOX, stbg__textbox_render },
        { STBG_WIDGET_TYPE.IMAGE, stbg__image_render },
        { STBG_WIDGET_TYPE.NODES_CONTAINER, stbg__nodes_container_render },
    };

    private static readonly Dictionary<STBG_WIDGET_TYPE, stbg__widget_update_input_delegate?> STBG__WIDGET_UPDATE_INPUT_DICTIONARY = new() {
        { STBG_WIDGET_TYPE.WINDOW, stbg__window_update_input },
        { STBG_WIDGET_TYPE.BUTTON, stbg__button_update_input },
        { STBG_WIDGET_TYPE.BUTTON_IMAGE, stbg__button_image_update_input },
        { STBG_WIDGET_TYPE.CHECKBOX, stbg__checkbox_update_input },
        { STBG_WIDGET_TYPE.LABEL, null }, // Labels don't handle any input
        { STBG_WIDGET_TYPE.SCROLLBAR, stbg__scrollbar_update_input },
        { STBG_WIDGET_TYPE.TEXTBOX, stbg__textbox_update_input },
        { STBG_WIDGET_TYPE.IMAGE, null }, // Images don't handle any input
        { STBG_WIDGET_TYPE.NODES_CONTAINER, stbg__nodes_container_update_input },
    };

    private static readonly stbg__widget_init_default_theme_delegate[] STBG__WIDGET_INIT_DEFAULT_THEME_LIST = [
        stbg__button_init_default_theme,
        stbg__button_image_init_default_theme,
        stbg__checkbox_init_default_theme,
        stbg__window_init_default_theme,
        stbg__label_init_default_theme,
        stbg__scrollbar_init_default_theme,
        stbg__textbox_init_default_theme,
        stbg__image_init_default_theme,
        stbg__nodes_container_init_default_theme,
    ];

    private static readonly stbg__widget_init_context_delegate[] STBG__WIDGET_INIT_CONTEXT_LIST = [
        stbg__textbox_init_context,
    ];

    private static readonly stbg__widget_render_delegate?[] STBG__WIDGET_RENDER_MAP = new stbg__widget_render_delegate[(int)STBG_WIDGET_TYPE.COUNT];
    private static readonly stbg__widget_update_input_delegate?[] STBG__WIDGET_UPDATE_INPUT_MAP = new stbg__widget_update_input_delegate[(int)STBG_WIDGET_TYPE.COUNT];

    static StbGui()
    {
        // Init maps
        for (var type = STBG_WIDGET_TYPE.NONE; type < STBG_WIDGET_TYPE.COUNT; type++)
        {
            var render = STBG__WIDGET_RENDER_DICTIONARY.GetValueOrDefault(type);
            var update_input = STBG__WIDGET_UPDATE_INPUT_DICTIONARY.GetValueOrDefault(type);

            STBG__WIDGET_RENDER_MAP[(int)type] = render;
            STBG__WIDGET_UPDATE_INPUT_MAP[(int)type] = update_input;
        }
    }
}
