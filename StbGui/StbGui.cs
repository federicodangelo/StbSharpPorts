#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;

using font_id = int;
using image_id = int;
using widget_hash = int;
using widget_id = int;

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
    /// Set widget style value
    /// </summary>
    public static void stbg_set_widget_style(STBG_WIDGET_STYLE style, bool value)
    {
        stbg__assert(!context.inside_frame);
        context.theme.styles[(int)style] = value ? 1 : 0;
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
    /// Get widget style value
    /// </summary>
    public static bool stbg_get_widget_style_boolean(STBG_WIDGET_STYLE style)
    {
        return context.theme.styles[(int)style] != 0;
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

    public enum STBG_INPUT_EVENT_TYPE
    {
        NONE,
        MOUSE_POSITION,
        MOUSE_SCROLL_WHEEL,
        MOUSE_BUTTON,
        KEYBOARD_KEY,
    }

    [Flags]
    public enum STBG_KEYBOARD_MODIFIER_FLAGS
    {
        NONE = 0,
        SHIFT = 1 << 0,
        CONTROL = 1 << 1,
        ALT = 1 << 2,
        SUPER = 1 << 3,
    }

    public enum STBG_KEYBOARD_KEY
    {
        NONE,
        CHARACTER, // Pressed character is in key_character
        BACKSPACE,
        DELETE,
        RETURN,
        LEFT,
        RIGHT,
        UP,
        DOWN,
        CONTROL_LEFT,
        CONTROL_RIGHT,
        SHIFT_LEFT,
        SHIFT_RIGHT,
        ALT_LEFT,
        ALT_RIGHT,
        HOME,
        END,
        PAGE_UP,
        PAGE_DOWN,
    }

    public struct stbg_user_input_input_event
    {
        public STBG_INPUT_EVENT_TYPE type;

        // Mouse related properties
        public int mouse_button;
        public bool mouse_button_pressed;
        public stbg_position mouse_position;
        public bool mouse_position_valid;
        public stbg_position mouse_scroll_wheel;

        // Key related properties
        public STBG_KEYBOARD_KEY key;
        public char key_character;
        public bool key_pressed;
        public STBG_KEYBOARD_MODIFIER_FLAGS key_modifiers;
    }

    /// <summary>
    /// Add mouse position changed event
    /// </summary>
    public static void stbg_add_user_input_event_mouse_position(float x, float y, bool valid = true)
    {
        stbg__add_user_input_event(new stbg_user_input_input_event()
        {
            type = STBG_INPUT_EVENT_TYPE.MOUSE_POSITION,
            mouse_position = stbg_build_position(x, y),
            mouse_position_valid = valid
        });
    }

    /// <summary>
    /// Add mouse button changed event
    /// </summary>
    public static void stbg_add_user_input_event_mouse_button(int button, bool pressed)
    {
        stbg__add_user_input_event(new stbg_user_input_input_event()
        {
            type = STBG_INPUT_EVENT_TYPE.MOUSE_BUTTON,
            mouse_button = button,
            mouse_button_pressed = pressed,
        });
    }

    /// <summary>
    /// Add mouse wheel changed event
    /// </summary>
    public static void stbg_add_user_input_event_mouse_wheel(float dx, float dy)
    {
        stbg__add_user_input_event(new stbg_user_input_input_event()
        {
            type = STBG_INPUT_EVENT_TYPE.MOUSE_SCROLL_WHEEL,
            mouse_scroll_wheel = stbg_build_position(dx, dy)
        });
    }

    /// <summary>
    /// Add keyboard key pressed / released event
    /// </summary>
    public static void stbg_add_user_input_event_keyboard_key_character(char character, STBG_KEYBOARD_MODIFIER_FLAGS modifiers, bool pressed)
    {
        stbg__add_user_input_event(new stbg_user_input_input_event()
        {
            type = STBG_INPUT_EVENT_TYPE.KEYBOARD_KEY,
            key = STBG_KEYBOARD_KEY.CHARACTER,
            key_character = character,
            key_pressed = pressed,
            key_modifiers = modifiers,
        });
    }

    /// <summary>
    /// Add keyboard key pressed / released event
    /// </summary>
    public static void stbg_add_user_input_event_keyboard_key(STBG_KEYBOARD_KEY key, STBG_KEYBOARD_MODIFIER_FLAGS modifiers, bool pressed)
    {
        stbg__add_user_input_event(new stbg_user_input_input_event()
        {
            type = STBG_INPUT_EVENT_TYPE.KEYBOARD_KEY,
            key = key,
            key_pressed = pressed,
            key_modifiers = modifiers,
        });
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

        stbg__update_average_performance_metrics();

        var start_process_input_time = stbg__get_performance_counter();
        stbg__process_input(); // Process previous frame input over the last updated hierarchy
        context.frame_stats.performance.process_input_time_us = ((stbg__get_performance_counter() - start_process_input_time) * MICROSECONDS) / stbg__get_performance_counter_frequency();

        var now = context.external_dependencies.get_time_milliseconds();

        context.inside_frame = true;
        context.current_widget_id = STBG_WIDGET_ID_NULL;
        context.last_widget_id = STBG_WIDGET_ID_NULL;
        context.last_widget_is_new = false;
        context.current_frame++;
        context.time_between_frames_milliseconds = context.current_time_milliseconds != 0 ? Math.Max(now - context.current_time_milliseconds, 1) : 33; // default non-zero value to prevent divisions by zero..
        context.current_time_milliseconds = now;
        context.prev_frame_stats = context.frame_stats;
        context.frame_stats = new stbg_context_frame_stats();

        stbg__string_memory_pool_reset(ref context.string_memory_pool);

        ref var root = ref stbg__add_widget(STBG_WIDGET_TYPE.ROOT, "root", out _);
        root.properties.layout.constrains.max = context.screen_size;
        root.flags |= STBG_WIDGET_FLAGS.ALLOW_CHILDREN;

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

        var start_layout_widgets_time = stbg__get_performance_counter();
        stbg__layout_widgets();
        context.frame_stats.performance.layout_widgets_time_us = ((stbg__get_performance_counter() - start_layout_widgets_time) * MICROSECONDS) / stbg__get_performance_counter_frequency();
    }

    /// <summary>
    /// Returns the current frame stats
    /// </summary>
    /// <returns></returns>
    public static stbg_context_frame_stats stbg_get_frame_stats()
    {
        return context.prev_frame_stats;
    }

    /// <summary>
    /// Returns the average performance metrics (values are averaged over half a second)
    /// </summary>
    /// <returns></returns>
    public static stbg_performance_metrics stbg_get_average_performance_metrics()
    {
        return context.performance_metrics;
    }

    /// <summary>
    /// Updates the render options flags
    /// </summary>
    public static void stbg_set_render_options_flag(STBG_RENDER_OPTIONS flag, bool value)
    {
        if (value)
            context.render_options |= flag;
        else
            context.render_options &= ~flag;
    }

    /// <summary>
    /// Issues all render commands for the content of the screen
    /// </summary>
    public static bool stbg_render()
    {
        return stbg__render();
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
    /// Adds an image
    /// </summary>
    public static image_id stbg_add_image(int width, int height)
    {
        stbg__assert(context.first_free_image_id + 1 < context.images.Length, "No more room for images");

        var new_image = new stbg_image_info()
        {
            id = context.first_free_image_id,
            original_image_id = context.first_free_image_id,
            size = stbg_build_size(width, height),
            rect = stbg_build_rect(0, 0, width, height),
        };

        context.first_free_image_id++;
        context.images[new_image.id] = new_image;

        return new_image.id;
    }

    /// <summary>
    /// Adds an image that is a sub-image of another image
    /// </summary>
    public static image_id stbg_add_sub_image(image_id image_id, int sub_x, int sub_y, int sub_width, int sub_height)
    {
        stbg__assert(context.first_free_image_id + 1 < context.images.Length, "No more room for images");
        stbg__assert(image_id > 0 && image_id < context.images.Length);

        ref var image = ref context.images[image_id];

        stbg__assert(sub_width >= 0 && sub_height >= 0);
        stbg__assert(sub_x >= image.rect.x0 && sub_x <= image.rect.x1);
        stbg__assert(sub_y >= image.rect.y0 && sub_y <= image.rect.y1);
        stbg__assert(sub_x + sub_width >= image.rect.x0 && sub_x + sub_width <= image.rect.x1);
        stbg__assert(sub_y + sub_height >= image.rect.y0 && sub_y + sub_height <= image.rect.y1);

        var new_image = new stbg_image_info()
        {
            id = context.first_free_image_id,
            size = stbg_build_size(sub_width, sub_height),
            rect = stbg_build_rect(sub_x, sub_y, sub_x + sub_width, sub_y + sub_height),
            original_image_id = image.original_image_id,
        };

        context.first_free_image_id++;
        context.images[new_image.id] = new_image;

        return new_image.id;
    }

    /// <summary>
    /// Returns the given image
    /// </summary>
    public static stbg_image_info stbg_get_image(image_id image_id)
    {
        return context.images[image_id];
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
        stbg__assert(id != STBG_WIDGET_ID_NULL);
        stbg__assert(context.widgets_reference_properties[id].last_used_in_frame == context.current_frame, "Can't access previous frame widget until they are created again");
        return ref stbg__get_widget_by_id_internal(id);
    }

    /// <summary>
    /// Returns a reference to the last widget
    /// </summary>
    public static ref stbg_widget stbg_get_last_widget()
    {
        var id = stbg_get_last_widget_id();
        stbg__assert(id != STBG_WIDGET_ID_NULL);
        return ref stbg_get_widget_by_id(id);
    }

    [Flags]
    public enum STBG_WINDOW_OPTIONS
    {
        NONE = 0,
        NO_MOVE = 1 << 0,
        NO_RESIZE = 1 << 1,
        NO_SCROLLBAR = 1 << 2,
        NO_TITLE = 1 << 3,
        CLOSE_BUTTON = 1 << 4,

        DEFAULT = NONE,
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
    public static bool stbg_begin_window(ReadOnlySpan<char> title, STBG_WINDOW_OPTIONS options = STBG_WINDOW_OPTIONS.DEFAULT)
    {
        var is_open = true;

        ref var window = ref stbg__window_create(title, ref is_open, options);

        if (is_open)
            context.current_widget_id = window.id;

        return is_open;
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
    /// <param name="title">Window title. If another window in the same parent with the same title is found, the new content is appended to that window</param>
    /// <returns>Returns if the window is visible or not</returns>
    public static bool stbg_begin_window(ReadOnlySpan<char> title, ref bool is_open, STBG_WINDOW_OPTIONS options = STBG_WINDOW_OPTIONS.DEFAULT)
    {
        ref var window = ref stbg__window_create(title, ref is_open, options | STBG_WINDOW_OPTIONS.CLOSE_BUTTON);

        if (is_open)
            context.current_widget_id = window.id;

        return is_open;
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

        widget.properties.layout.intrinsic_position.position.x = Math.Max(x, 0);
        widget.properties.layout.intrinsic_position.position.y = Math.Max(y, 0);
    }

    /// <summary>
    /// Change existing widget intrinsic position
    /// </summary>
    public static void stbg_set_last_widget_position(float x, float y)
    {
        stbg_set_widget_position(stbg_get_last_widget_id(), x, y);
    }

    /// <summary>
    /// Change existing widget intrinsic position if it is new
    /// </summary>
    public static void stbg_set_last_widget_position_if_new(float x, float y)
    {
        if (stbg_get_last_widget_is_new())
            stbg_set_last_widget_position(x, y);
    }

    /// <summary>
    /// Changes existing widget intrinsic size
    /// </summary>
    public static void stbg_set_widget_size(widget_id widget_id, float width, float height)
    {
        stbg__assert(context.inside_frame);
        stbg__assert(widget_id != context.root_widget_id);
        ref var widget = ref stbg_get_widget_by_id(widget_id);

        widget.properties.layout.intrinsic_size.size = stbg_build_size(Math.Max(width, 0), Math.Max(height, 0));
    }

    /// <summary>
    /// Changes existing widget intrinsic size
    /// </summary>
    public static void stbg_set_last_widget_size(float width, float height)
    {
        stbg_set_widget_size(stbg_get_last_widget_id(), width, height);
    }

    /// <summary>
    /// Changes existing widget intrinsic size if it is new
    /// </summary>
    public static void stbg_set_last_widget_size_if_new(float width, float height)
    {
        if (stbg_get_last_widget_is_new())
            stbg_set_last_widget_size(width, height);
    }

    /// <summary>
    /// Begins a new container with the specified layout direction
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="layout_direction">Layout direction</param>
    public static void stbg_begin_container(ReadOnlySpan<char> identifier, STBG_CHILDREN_LAYOUT layout_direction)
    {
        stbg_begin_container(identifier, layout_direction, stbg_build_constrains_unconstrained(), stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING));
    }

    /// <summary>
    /// Begins a new container with the specified layout direction
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="spacing">Spacing between children</param>
    /// <param name="constrains">size constrains</param>
    public static void stbg_begin_container(ReadOnlySpan<char> identifier, STBG_CHILDREN_LAYOUT layout_direction, stbg_widget_constrains constrains)
    {
        stbg_begin_container(identifier, layout_direction, constrains, stbg_get_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_SPACING));
    }

    /// <summary>
    /// Begins a new container with the specified layout direction and spacing between children
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="layout_direction">Layout direction</param>
    /// <param name="spacing">Spacing between children</param>
    public static void stbg_begin_container(ReadOnlySpan<char> identifier, STBG_CHILDREN_LAYOUT layout_direction, float spacing)
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
    public static void stbg_begin_container(ReadOnlySpan<char> identifier, STBG_CHILDREN_LAYOUT layout_direction, stbg_widget_constrains constrains, float spacing)
    {
        ref var container = ref stbg__container_create(identifier, layout_direction, constrains, spacing);

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
    public static bool stbg_button(ReadOnlySpan<char> label)
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
    /// Adds a button.
    /// Returns true if the button was pressed.
    /// </summary>
    /// <param name="identifier">Button identifier, must be unique in the parent</param>
    /// <returns>Returns true if the button was pressed</returns>
    public static bool stbg_button(ReadOnlySpan<char> identifier, image_id image, image_id image_hover, image_id image_pressed, bool border = true, float scale = 1)
    {
        stbg__assert(image > 0 && image < context.images.Length, "Invalid image_id");

        ref var button = ref stbg__button_image_create(identifier, image, image_hover, image_pressed, border, scale);

        bool clicked = (button.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.CLICKED) != 0;

        if (clicked)
        {
            button.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.CLICKED;
        }

        return clicked;
    }

    /// <summary>
    /// Adds a checkbox.
    /// </summary>
    /// <param name="label">Checkbox label, must be unique in the parent</param>
    /// <param name="value">Checkbox value</param>
    /// <returns>Returns true if the checkbox value changed</returns>
    public static bool stbg_checkbox(ReadOnlySpan<char> label, ref bool value)
    {
        stbg__checkbox_create(label, ref value, out var updated);
        return updated;
    }

    /// <summary>
    /// Shows a label.
    /// </summary>
    /// <param name="text">Label text, must be unique in the parent</param>
    public static void stbg_label(ReadOnlySpan<char> text)
    {
        stbg__label_create(text);
    }

    /// <summary>
    /// Shows a scrollbar.
    /// </summary>
    public static void stbg_scrollbar(ReadOnlySpan<char> identifier, STBG_SCROLLBAR_DIRECTION direction, ref float value, float min_value, float max_value)
    {
        stbg__scrollbar_create(identifier, direction, ref value, min_value, max_value, (max_value - min_value) / 10.0f, false);
    }

    /// <summary>
    /// Shows a scrollbar.
    /// </summary>
    public static void stbg_scrollbar(ReadOnlySpan<char> identifier, STBG_SCROLLBAR_DIRECTION direction, ref int value, int min_value, int max_value)
    {
        float f = value;
        stbg__scrollbar_create(identifier, direction, ref f, min_value, max_value, (max_value - min_value) / 10.0f, true);
        value = (int)f;
    }

    public struct stbg_textbox_text_to_edit
    {
        public Memory<char> text;
        public int length;
    }

    /// <summary>
    /// Creates an editable textbox
    /// </summary>
    public static void stbg_textbox(ReadOnlySpan<char> identifier, ref stbg_textbox_text_to_edit text_to_edit, int visible_lines = 1)
    {
        stbg__assert(visible_lines >= 1);
        stbg__textbox_create(identifier, ref text_to_edit, visible_lines);
    }

    /// <summary>
    /// Shows an image
    /// </summary>
    public static void stbg_image(ReadOnlySpan<char> identifier, image_id image_id, float scale = 1)
    {
        stbg__assert(image_id > 0 && image_id < context.images.Length, "Invalid image_id");
        stbg__image_create(identifier, image_id, scale);
    }
}
