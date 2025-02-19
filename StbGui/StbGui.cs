#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

// Reference:
// - https://www.rfleury.com/p/posts-table-of-contents
// - https://www.rfleury.com/p/ui-part-1-the-interaction-medium
// - https://www.rfleury.com/p/ui-part-2-build-it-every-frame-immediate
// - https://www.rfleury.com/p/ui-part-3-the-widget-building-language
// - https://www.youtube.com/watch?v=-z8_F9ozERc
// - https://asawicki.info/Download/Productions/Lectures/Immediate%20Mode%20GUI.pdf

public partial class StbGui
{
    /// <summary>
    /// Returns a reference to the current context
    /// </summary>
    /// <returns>Context</returns>
    public static ref stbg_context stbg_get_context() => ref context;

    /// <summary>
    /// Init GUI library.
    /// It initializes the shared context used by all the other functions.
    /// </summary>
    public static void stbg_init(stbg_external_dependencies external_dependencies, stbg_init_options options)
    {
        context = new stbg_context();

        stbg_init_context(ref context, external_dependencies, options);
    }

    /// <summary>
    /// Destroy GUI library.
    /// </summary>
    public static void stbg_destroy()
    {
        context = new stbg_context();
    }

    /// <summary>
    /// Inits the default theme, must be called after calling stbg_init()
    /// </summary>
    public static void stbg_init_default_theme(font_id font_id, stbg_font_style font_style)
    {
        // https://materialui.co/flatuicolors

        stbg__assert(!context.inside_frame);

        ref var theme = ref context.theme;

        theme.default_font_id = font_id;
        theme.default_font_style = font_style;

        // ROOT
        stbg_set_widget_style(STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR, rgb(236, 240, 241));

        for (var i = 0; i < STBG__WIDGET_INIT_DEFAULT_THEME_LIST.Length; i++)
            STBG__WIDGET_INIT_DEFAULT_THEME_LIST[i]();
    }

    /// <summary>
    /// Set widget style value
    /// </summary>
    public static void stbg_set_widget_style(STBG_WIDGET_STYLE style, float value)
    {
        stbg__assert(!context.inside_frame);
        context.theme.styles[(int)style] = value;
    }

    /// <summary>
    /// Set widget style value
    /// </summary>
    public static void stbg_set_widget_style(STBG_WIDGET_STYLE style, stbg_color color)
    {
        stbg__assert(!context.inside_frame);
        context.theme.styles[(int)style] = stbg_color_to_uint(color);
    }

    /// <summary>
    /// Get widget style value
    /// </summary>
    public static float stbg_get_widget_style(STBG_WIDGET_STYLE style)
    {
        return (float)context.theme.styles[(int)style];
    }

    /// <summary>
    /// Get widget style value
    /// </summary>
    public static stbg_color stbg_get_widget_style_color(STBG_WIDGET_STYLE style)
    {
        return stbg_uint_to_color((uint)context.theme.styles[(int)style]);
    }

    /// <summary>
    /// Sets the screen size.
    /// Must be called at least once after the initialization, and after that every time that the screen size changes
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void stbg_set_screen_size(int width, int height)
    {
        stbg__assert(!context.inside_frame);
        context.screen_size.width = width;
        context.screen_size.height = height;
    }

    /// <summary>
    /// User Input
    /// </summary>
    public struct stbg_user_input
    {
        /// <summary>
        /// Mouse position
        /// </summary>
        public stbg_position mouse_position;
        /// <summary>
        /// Mouse position valid or not (set to false when the mouse is outside the window)
        /// </summary>
        public bool mouse_position_valid;
        /// <summary>
        /// Mouse button 1 is pressed
        /// </summary>
        public bool mouse_button_1;
        /// <summary>
        /// Mouse button 2 is pressed
        /// </summary>
        public bool mouse_button_2;
    }

    /// <summary>
    /// Sets the input
    /// </summary>
    public static void stbg_set_user_input(stbg_user_input user_input)
    {
        stbg__assert(!context.inside_frame);
        context.user_input = user_input;
    }

    /// <summary>
    /// Returns the active cursor
    /// </summary>
    public static STBG_ACTIVE_CURSOR_TYPE stbg_get_cursor()
    {
        return context.active_cursor;
    }

    /// <summary>
    /// Starts a new frame
    /// </summary>
    public static void stbg_begin_frame()
    {
        stbg__assert(!context.inside_frame);

        context.active_cursor = STBG_ACTIVE_CURSOR_TYPE.DEFAULT;

        stbg__process_input(); // Process previous frame input over the last updated hierarchy

        context.inside_frame = true;
        context.current_widget_id = STBG_WIDGET_ID_NULL;
        context.last_widget_id = STBG_WIDGET_ID_NULL;
        context.last_widget_is_new = false;
        context.current_frame++;
        context.prev_frame_stats = context.frame_stats;
        context.frame_stats = new stbg_context_frame_stats();

        ref var root = ref stbg__add_widget(STBG_WIDGET_TYPE.ROOT, "root", out _);
        root.properties.layout.constrains.max = context.screen_size;

        context.current_widget_id = root.id;
        context.root_widget_id = root.id;
    }

    /// <summary>
    /// Ends the current frame
    /// </summary>
    public static void stbg_end_frame()
    {
        stbg__assert(context.inside_frame);
        stbg__assert(context.current_widget_id == context.root_widget_id, "Unbalanced begin() / end() calls");

        context.inside_frame = false;
        context.current_widget_id = STBG_WIDGET_ID_NULL;

        // If we didn't reuse the same amount of widgets that existed in the last frame, then some widgets need to be destroyed!
        if (context.prev_frame_stats.new_widgets + context.prev_frame_stats.reused_widgets > context.frame_stats.reused_widgets)
        {
            int amount_to_destroy = (context.prev_frame_stats.new_widgets + context.prev_frame_stats.reused_widgets) - context.frame_stats.reused_widgets;
            stbg__destroy_unused_widgets(amount_to_destroy);
        }

        stbg__layout_widgets();
    }

    /// <summary>
    /// Issues all render commands for the content of the screen
    /// </summary>
    public static void stbg_render()
    {
        stbg__assert(!context.inside_frame);

        stbg__render();
    }

    /// <summary>
    /// Adds a font
    /// </summary>
    /// <param name="name">Font name</param>
    /// <returns>A unique font identifier</returns>
    public static font_id stbg_add_font(string name)
    {
        stbg__assert(context.first_free_font_id + 1 < context.fonts.Length, "No more room for fonts");

        var newFont = new stbg_font()
        {
            id = context.first_free_font_id,
            name = name,
        };

        context.first_free_font_id++;
        context.fonts[newFont.id] = newFont;

        return newFont.id;
    }

    /// <summary>
    /// Returns the font with the specified id
    /// </summary>
    /// <param name="font_id">Font id</param>
    /// <returns>Font</returns>
    public static stbg_font stbg_get_font_by_id(font_id font_id)
    {
        return context.fonts[font_id];
    }

    /// <summary>
    /// Returns the widget_id of the last created widget
    /// </summary>
    public static widget_id stbg_get_last_widget_id()
    {
        return context.last_widget_id;
    }

    /// <summary>
    /// Returns if the last widget was a new widget
    /// </summary>
    public static bool stbg_get_last_widget_is_new()
    {
        return context.last_widget_is_new;
    }

    /// <summary>
    /// Returns a reference to a widget given a widget id
    /// </summary>
    /// <param name="id">Widget id</param>
    /// <returns>Widget reference</returns>
    public static ref stbg_widget stbg_get_widget_by_id(widget_id id)
    {
        return ref context.widgets[id];
    }

    /// <summary>
    /// Begins a new window with the given title.
    /// Returns true if the window is visible, false otherwise.
    /// If it returns true, you MUST skip the window content and the stbg_end_window() call.
    /// <code>
    /// if (StbGui.stbg_begin_window("Window 1"))
    /// {
    ///   // window visible, add content
    ///   StbGui.stbg_button("Button 1");
    ///   ...
    ///   StbGui.stbg_end_window(); // don't forget to end the window
    /// }
    /// </code>
    /// </summary>
    /// <param name="title">Window title. Must be unique inside the parent container</param>
    /// <returns>Returns if the window is visible or not</returns>
    public static bool stbg_begin_window(string title)
    {
        ref var window = ref stbg__window_create(title);

        bool visible = true; //TODO: Implement

        if (visible)
            context.current_widget_id = window.id;

        return visible;
    }

    /// <summary>
    /// Ends the current window.
    /// </summary>
    public static void stbg_end_window()
    {
        stbg__assert(context.inside_frame);
        ref var window = ref stbg_get_widget_by_id(context.current_widget_id);
        stbg__assert(window.type == STBG_WIDGET_TYPE.WINDOW, "Unbalanced begin() / end() calls");

        context.current_widget_id = window.hierarchy.parent_id;
    }

    /// <summary>
    /// Change existing widget intrinsic position
    /// </summary>
    public static void stbg_set_widget_position(widget_id widget_id, float x, float y)
    {
        stbg__assert(context.inside_frame);
        stbg__assert(widget_id != context.root_widget_id);

        ref var widget = ref stbg_get_widget_by_id(widget_id);

        stbg__warning(stbg_get_widget_by_id(widget.hierarchy.parent_id).properties.layout.children_layout_direction == STBG_CHILDREN_LAYOUT.FREE, "Position of widgets inside non-free parents is ignored");

        widget.properties.layout.intrinsic_position.x = Math.Max(x, 0);
        widget.properties.layout.intrinsic_position.y = Math.Max(y, 0);
    }

    /// <summary>
    /// Changes existing widget instrinsic size
    /// </summary>
    public static void stbg_set_widget_size(widget_id widget_id, float width, float height)
    {
        stbg__assert(context.inside_frame);
        stbg__assert(widget_id != context.root_widget_id);
        ref var widget = ref stbg_get_widget_by_id(widget_id);

        stbg__warning(widget.properties.layout.intrinsic_size.type == STBG_INTRINSIC_SIZE_TYPE.FIXED_PIXELS, "Size of widgets without FIXED_PIXELS size is ignored");

        widget.properties.layout.intrinsic_size.size.width = Math.Max(width, 0);
        widget.properties.layout.intrinsic_size.size.height = Math.Max(height, 0);
    }

    /// <summary>
    /// Begins a new container with the specified layout direction
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="layout_direction">Layout direction</param>
    public static void stbg_begin_container(string identifier, STBG_CHILDREN_LAYOUT layout_direction)
    {
        stbg_begin_container(identifier, layout_direction, stbg_build_constrains_unconstrained(), stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING));
    }

    /// <summary>
    /// Begins a new container with the specified layout direction
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="spacing">Spacing between children</param>
    /// <param name="constrains">size constrains</param>
    public static void stbg_begin_container(string identifier, STBG_CHILDREN_LAYOUT layout_direction, stbg_widget_constrains constrains)
    {
        stbg_begin_container(identifier, layout_direction, constrains, stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING));
    }
    
    /// <summary>
    /// Begins a new container with the specified layout direction and spacing between children
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="layout_direction">Layout direction</param>
    /// <param name="spacing">Spacing between children</param>
    public static void stbg_begin_container(string identifier, STBG_CHILDREN_LAYOUT layout_direction, float spacing)
    {
        stbg_begin_container(identifier, layout_direction, stbg_build_constrains_unconstrained(), spacing);
    }

    /// <summary>
    /// Begins a new container with the specified layout direction, spacing between children and size constrains
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="layout_direction">Layout direction</param>
    /// <param name="constrains">Size constrains</param>
    /// <param name="spacing">Spacing between children</param>
    public static void stbg_begin_container(string identifier, STBG_CHILDREN_LAYOUT layout_direction, stbg_widget_constrains constrains, float spacing)
    {
        ref var container = ref stbg__add_widget(STBG_WIDGET_TYPE.CONTAINER, identifier, out _);

        ref var layout = ref container.properties.layout;

        layout.inner_padding = new stbg_padding();
        layout.constrains = constrains;
        layout.children_layout_direction = layout_direction;
        layout.children_spacing = spacing;

        context.current_widget_id = container.id;
    }

    /// <summary>
    /// Ends the current container.
    /// </summary>
    public static void stbg_end_container()
    {
        stbg__assert(context.inside_frame);
        ref var container = ref stbg_get_widget_by_id(context.current_widget_id);
        stbg__assert(container.type == STBG_WIDGET_TYPE.CONTAINER, "Unbalanced begin() / end() calls");

        context.current_widget_id = container.hierarchy.parent_id;
    }

    /// <summary>
    /// Adds a button.
    /// Returns true if the button was pressed.
    /// </summary>
    /// <param name="label">Button label, must be unique in the parent</param>
    /// <returns>Returns true if the button was pressed</returns>
    public static bool stbg_button(string label)
    {
        ref var button = ref stbg__button_create(label);

        bool clicked = (button.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.CLICKED) != 0;

        if (clicked)
        {
            button.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.CLICKED;
        }

        return clicked;
    }

    /// <summary>
    /// Shows a label.
    /// </summary>
    /// <param name="text">Label text, must be unique in the parent</param>
    public static void stbg_label(string text)
    {
        stbg__label_create(text);
    }

    /// <summary>
    /// Shows a scrollbar.
    /// </summary>
    public static void stbg_scrollbar(string identifier, STBG_SCROLLBAR_DIRECTION direction, ref float value, float min_value, float max_value)
    {
        stbg__scrollbar_create(identifier, direction, ref value, min_value, max_value);
    }
}
