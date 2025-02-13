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
    public const widget_id STBG_WIDGET_ID_NULL = 0;

    public const font_id STBG_FONT_ID_NULL = 0;

    public record struct stbg_size
    {
        public float width;
        public float height;
    }

    public record struct stbg_position
    {
        public float x;
        public float y;
    }

    public record struct stbg_rect
    {
        public stbg_position top_left;
        public stbg_position bottom_right;
    }

    public record struct stbg_padding
    {
        public float left;
        public float right;
        public float top;
        public float bottom;
    }

    public enum STBG_INTRINSIC_SIZE_TYPE
    {
        /// <summary>
        /// Widget has no intrinsic size
        /// </summary>
        NONE,
        /// <summary>
        /// Widget has an intrinsic size in pixels
        /// </summary>
        FIXED_PIXELS,
        /// <summary>
        /// Widget has an instrinsic size based on the measurement of the text
        /// </summary>
        MEASURE_TEXT
    };

    public record struct stbg_color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
    }

    [Flags]
    public enum STBG_FONT_STYLE_FLAGS
    {
        NONE = 0,
        BOLD = 1 << 0,
        ITALIC = 1 << 1
    }

    public record struct stbg_font_style
    {
        public float size;
        public STBG_FONT_STYLE_FLAGS style;
        public stbg_color color;
    }

    public struct stbg_font
    {
        public font_id id;
        public string name;
    }

    public record struct stbg_text
    {
        public ReadOnlyMemory<char> text;
        public font_id font_id;
        public stbg_font_style style;
    }

    public struct stbg_widget_style
    {
        public font_id font_id;
        public stbg_font_style font_style;
        public stbg_widget_layout layout;
    }

    /// <summary>
    /// Widget styles
    /// - Paddings always go in (top, bottom, left, right) order (so we can automate padding object construction given a single index)
    /// </summary>
    public enum STBG_WIDGET_STYLE
    {
        NONE = 0,

        // Root style
        ROOT_BACKGROUND_COLOR,

        // Window styles
        WINDOW_BORDER_SIZE,
        WINDOW_TITLE_HEIGHT,
        WINDOW_TITLE_PADDING_TOP,
        WINDOW_TITLE_PADDING_BOTTOM,
        WINDOW_TITLE_PADDING_LEFT,
        WINDOW_TITLE_PADDING_RIGHT,
        WINDOW_CHILDREN_PADDING_TOP,
        WINDOW_CHILDREN_PADDING_BOTTOM,
        WINDOW_CHILDREN_PADDING_LEFT,
        WINDOW_CHILDREN_PADDING_RIGHT,
        WINDOW_DEFAULT_WIDTH,
        WINDOW_DEFAULT_HEIGHT,
        WINDOW_BORDER_COLOR,
        WINDOW_BACKGROUND_COLOR,
        WINDOW_TITLE_TEXT_COLOR,
        WINDOW_TITLE_BACKGROUND_COLOR,

        // Button styles
        BUTTON_BORDER_SIZE,
        BUTTON_PADDING_TOP,
        BUTTON_PADDING_BOTTOM,
        BUTTON_PADDING_LEFT,
        BUTTON_PADDING_RIGHT,
        BUTTON_BORDER_COLOR,
        BUTTON_BACKGROUND_COLOR,
        BUTTON_TEXT_COLOR,

        // ALWAYS LAST!!
        COUNT
    }

    public struct stbg_theme
    {
        public font_id default_font_id;
        public stbg_font_style default_font_style;

        /// <summary>
        /// One value for each entry in STBG_WIDGET_STYLE
        /// We store doubles instead of floats so we can store the full 
        /// range of a uint value, used to encode colors in the styles
        /// </summary>
        public double[] styles;
    }

    public struct stbg_widget_intrinsic_size
    {
        public STBG_INTRINSIC_SIZE_TYPE type;
        public stbg_size size;
    }

    public struct stbg_widget_constrains
    {
        public stbg_size min;
        public stbg_size max;
    }

    public enum STBG_CHILDREN_LAYOUT_DIRECTION
    {
        FREE,
        VERTICAL,
        HORIZONTAL,
    }

    public struct stbg_widget_layout
    {
        /// <summary>
        /// Widget size constrains, can be overriden if the widget doesn't fit in the expected bounds, or if it auto-expands
        /// </summary>
        public stbg_widget_constrains constrains;

        /// <summary>
        /// Intrinsic size, doesn't include inner_padding, can be overriden if the widget doesn't fit in the expected bounds, or if it auto-expands
        /// </summary>
        public stbg_widget_intrinsic_size intrinsic_size;

        /// <summary>
        /// Intrinsic top-left position, used when the parent's children layout direction is FREE
        /// </summary>
        public stbg_position intrinsic_position;

        /// <summary>
        /// Inner padding
        /// </summary>
        public stbg_padding inner_padding;

        /// <summary>
        /// Children spacing (used when layout direction is NOT FREE)
        /// </summary>
        public float children_spacing;

        /// <summary>
        /// Children layout direction
        /// </summary>
        public STBG_CHILDREN_LAYOUT_DIRECTION children_layout_direction;
    }

    public struct stbg_widget_hierarchy
    {
        // Parent id
        public widget_id parent_id;

        // Siblings ids (widgets with same parent_id)
        public widget_id next_sibling_id; // points to next free widget id if widget not used
        public widget_id prev_sibling_id;

        // Children ids (widgets that have us in their parent_id)
        public widget_id first_children_id;
        public widget_id last_children_id;
    }

    public struct stbg_widget_computed_bounds
    {
        public stbg_position relative_position;
        public stbg_size size;
        public stbg_rect global_rect;
    }

    public enum STBG_WIDGET_TYPE
    {
        NONE,
        ROOT,
        WINDOW,
        CONTAINER,
        BUTTON,
        COUNT, // MUST BE LAST
    }

    [Flags]
    public enum STBG_WIDGET_FLAGS
    {
        NONE = 0,
        USED = 1 << 0,
    }

    public struct stbg_widget_hash_chain
    {
        public widget_id next_same_bucket;
        public widget_id prev_same_bucket;
    }

    public struct stbg_widget_persistent_data
    {
        public float f1;
        public float f2;
        public float f3;
        public float f4;
    }

    public struct stbg_widget
    {
        // This widget id (it's also the index in the main widgets array)
        public widget_id id;

        // This widget hash (generated by combining the unique identifier in the context of the widget + the hash of the parent)
        public widget_hash hash;

        // Last frame in which this widget was used
        public int last_used_in_frame;

        // Type of widget
        public STBG_WIDGET_TYPE type;

        public STBG_WIDGET_FLAGS flags;

        public stbg_widget_hierarchy hierarchy;

        public stbg_widget_hash_chain hash_chain;

        public stbg_widget_persistent_data persistent_data;

        public stbg_widget_layout layout;

        public stbg_widget_computed_bounds computed_bounds;

        public stbg_text text;
    }

    public struct stbg_hash_entry
    {
        public widget_id first_widget_in_bucket;
    }

    public struct stbg_context_frame_stats
    {
        public int new_widgets;
        public int reused_widgets;
        public int destroyed_widgets;
        public int duplicated_widgets_ids;
    }

    public struct stbg_context
    {
        public stbg_widget[] widgets;

        public stbg_hash_entry[] hash_table;

        public widget_id first_free_widget_id;

        public stbg_font[] fonts;
        public font_id first_free_font_id;

        public bool inside_frame;

        public int current_frame;

        public widget_id current_widget_id;

        public widget_id last_widget_id;

        public widget_id root_widget_id;

        public stbg_context_frame_stats frame_stats;

        public stbg_context_frame_stats prev_frame_stats;

        public stbg_init_options init_options;

        public stbg_external_dependencies external_dependencies;

        public stbg_size screen_size;

        public stbg_theme theme;

        public stbg_render_command[] render_commands_queue;
    }

    public enum STBG_ASSERT_BEHAVIOUR
    {
        /// <summary>
        /// Asserts in DEBUG builds, does nothing in RELEASE builds
        /// </summary>
        ASSERT,
        /// <summary>
        /// Throws an exception in both DEBUG and RELEASE builds
        /// </summary>
        EXCEPTION,
        /// <summary>
        /// Logs an error message in the error console in DEBUG and RELEASE builds
        /// </summary>
        CONSOLE,
        /// <summary>
        /// Does nothing in DEBUG and RELEASE builds
        /// </summary>
        NONE,
    }

    public delegate stbg_size stbg_measure_text_delegate(ReadOnlySpan<char> text, stbg_font font, stbg_font_style style);

    public enum STBG_RENDER_COMMAND_TYPE
    {
        /// <summary>
        /// Start rendering new frame, using bounds as screen size and background_color (as GUI background color)
        /// </summary>
        BEGIN_FRAME,

        /// <summary>
        /// Finish rendering current frame
        /// </summary>
        END_FRAME,

        /// <summary>
        /// Render rectangle using bounds and background_color (as fill color)
        /// </summary>
        RECTANGLE,

        /// <summary>
        /// Render border using bounds, size (border size), color (border color) and background_color (used to fill the content)
        /// </summary>
        BORDER,

        /// <summary>
        /// Render text using bounds, color, text, font and font_style
        /// </summary>
        TEXT,
    }

    public record struct stbg_render_command
    {
        public STBG_RENDER_COMMAND_TYPE type;
        public float size;
        public stbg_rect bounds;
        public stbg_color color;
        public stbg_color background_color;
        public stbg_text text;
    }

    public delegate void stbg_render_delegate(Span<stbg_render_command> commands);

    public struct stbg_external_dependencies
    {
        /// <summary>
        /// Measure text
        /// </summary>
        public stbg_measure_text_delegate measure_text;

        /// <summary>
        /// Render
        /// </summary>
        public stbg_render_delegate render;
    }

    public const int DEFAULT_MAX_WIDGETS = 32767;

    public const int DEFAULT_MAX_FONTS = 32;

    public const int DEFAULT_RENDER_QUEUE_SIZE = 128;

    public struct stbg_init_options
    {
        /// <summary>
        /// Max number of widgets, defaults to DEFAULT_MAX_WIDGETS
        /// </summary>
        public int max_widgets;

        /// <summary>
        /// Hash table size, defaults to max_widgets
        /// </summary>
        public int hash_table_size;

        /// <summary>
        /// Max number of loaded fonts, defaults to DEFAULT_MAX_FONTS
        /// </summary>
        public int max_fonts;

        /// <summary>
        /// Behaviour of assert calls, defaults to ASSERT
        /// </summary>
        public STBG_ASSERT_BEHAVIOUR assert_behaviour;

        /// <summary>
        /// Size of render commands queue, defaults to DEFAULT_RENDER_QUEUE_SIZE
        /// </summary>
        public int render_commands_queue_size;
    }

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
        context = default;
    }

    /// <summary>
    /// Inits the default theme, must be called after calling stbg_init()
    /// </summary>
    public static void stbg_init_default_theme(font_id font_id, stbg_font_style font_style)
    {
        stbg__assert(!context.inside_frame);

        ref var theme = ref context.theme;

        theme.default_font_id = font_id;
        theme.default_font_style = font_style;

        stbg_set_widget_style(STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR, STBG_COLOR_CYAN);

        var buttonBorder = 1.0f;
        var buttonPaddingTopBottom = MathF.Ceiling(font_style.size / 2);
        var buttonPaddingLeftRight = MathF.Ceiling(font_style.size / 2);

        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, buttonBorder);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_TOP, buttonPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM, buttonPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT, buttonPaddingLeftRight);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT, buttonPaddingLeftRight);

        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_COLOR, STBG_COLOR_BLUE);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_BACKGROUND_COLOR, STBG_COLOR_CYAN);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_TEXT_COLOR, STBG_COLOR_WHITE);

        var windowBorder = 1.0f;
        var windowTitleHeight = MathF.Ceiling(font_style.size);
        var windowTitlePadding = MathF.Ceiling(font_style.size / 4);
        var windowChildrenPadding = MathF.Ceiling(font_style.size / 2);
        var windowDefaultWidth = MathF.Ceiling(font_style.size * 30);
        var windowDefaultHeight = MathF.Ceiling(font_style.size * 15);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, windowBorder);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, windowTitleHeight);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, windowTitlePadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, windowTitlePadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT, windowTitlePadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT, windowTitlePadding);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP, windowChildrenPadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_BOTTOM, windowChildrenPadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_LEFT, windowChildrenPadding);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_RIGHT, windowChildrenPadding);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, windowDefaultWidth);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, windowDefaultHeight);

        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BORDER_COLOR, STBG_COLOR_WHITE);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_BACKGROUND_COLOR, STBG_COLOR_BLUE);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_TEXT_COLOR, STBG_COLOR_WHITE);
        stbg_set_widget_style(STBG_WIDGET_STYLE.WINDOW_TITLE_BACKGROUND_COLOR, STBG_COLOR_MAGENTA);
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
    /// Starts a new frame
    /// </summary>
    public static void stbg_begin_frame()
    {
        stbg__assert(!context.inside_frame);
        context.inside_frame = true;
        context.current_widget_id = STBG_WIDGET_ID_NULL;
        context.last_widget_id = STBG_WIDGET_ID_NULL;
        context.current_frame++;
        context.prev_frame_stats = context.frame_stats;
        context.frame_stats = new stbg_context_frame_stats();

        ref var root = ref stbg__add_widget(STBG_WIDGET_TYPE.ROOT, "root", out _);
        root.layout.constrains.max = context.screen_size;

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
        ref var window = ref stbg__add_widget(STBG_WIDGET_TYPE.WINDOW, title, out var is_new);

        window.text.style.color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.WINDOW_TITLE_TEXT_COLOR);
        window.text.text = title.AsMemory();

        ref var layout = ref window.layout;

        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_RIGHT),
        };
        layout.constrains = stbg__build_constrains_unconstrained();
        layout.children_layout_direction = STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL;

        if (is_new)
        {
            window.persistent_data.f1 = 0;
            window.persistent_data.f2 = 0;
            window.persistent_data.f3 = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH);
            window.persistent_data.f4 = stbg__sum_styles(STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT);
        }

        window.layout.intrinsic_position.x = window.persistent_data.f1;
        window.layout.intrinsic_position.y = window.persistent_data.f2;
        layout.intrinsic_size = stbg__build_intrinsic_size_pixels(window.persistent_data.f3, window.persistent_data.f4);

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
    /// Change existing window position
    /// </summary>
    public static void stbg_move_window(widget_id window_id, float x, float y)
    {
        stbg__assert(context.inside_frame);
        ref var window = ref stbg_get_widget_by_id(window_id);
        stbg__assert(window.type == STBG_WIDGET_TYPE.WINDOW);

        window.persistent_data.f1 = x;
        window.persistent_data.f2 = y;
        window.layout.intrinsic_position.x = window.persistent_data.f1;
        window.layout.intrinsic_position.y = window.persistent_data.f2;
    }

    /// <summary>
    /// Resize existing window
    /// </summary>
    public static void stbg_resize_window(widget_id window_id, float width, float height)
    {
        stbg__assert(context.inside_frame);
        ref var window = ref stbg_get_widget_by_id(window_id);
        stbg__assert(window.type == STBG_WIDGET_TYPE.WINDOW);

        window.persistent_data.f3 = Math.Max(width - (window.layout.inner_padding.left + window.layout.inner_padding.right), 0);
        window.persistent_data.f4 = Math.Max(height - (window.layout.inner_padding.top + window.layout.inner_padding.bottom), 0);
        window.layout.intrinsic_size = stbg__build_intrinsic_size_pixels(window.persistent_data.f3, window.persistent_data.f4);
    }

    /// <summary>
    /// Begins a new container with the specified layout direction
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="layout_direction">Layout direction</param>
    public static void stbg_begin_container(string identifier, STBG_CHILDREN_LAYOUT_DIRECTION layout_direction)
    {
        stbg_begin_container(identifier, layout_direction, 0);
    }

    /// <summary>
    /// Begins a new container with the specified layout direction and spacing between children
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="layout_direction">Layout direction</param>
    /// <param name="spacing">Spacing between children</param>
    public static void stbg_begin_container(string identifier, STBG_CHILDREN_LAYOUT_DIRECTION layout_direction, float spacing)
    {
        ref var container = ref stbg__add_widget(STBG_WIDGET_TYPE.CONTAINER, identifier, out _);

        ref var layout = ref container.layout;

        layout.inner_padding = new stbg_padding();
        layout.constrains = stbg__build_constrains_unconstrained();
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
        ref var button = ref stbg__add_widget(STBG_WIDGET_TYPE.BUTTON, label, out _);
        button.text.style.color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.BUTTON_TEXT_COLOR);
        button.text.text = label.AsMemory();

        ref var layout = ref button.layout;

        layout.constrains = stbg__build_constrains_unconstrained();
        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT),
        };
        layout.intrinsic_size = stbg__build_intrinsic_size_text();

        bool pressed = false; // TODO: Implement!

        return pressed;
    }
}
