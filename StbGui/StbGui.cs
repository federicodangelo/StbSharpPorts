#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// Reference:
// - https://www.rfleury.com/p/posts-table-of-contents
// - https://www.rfleury.com/p/ui-part-1-the-interaction-medium
// - https://www.rfleury.com/p/ui-part-2-build-it-every-frame-immediate
// - https://www.rfleury.com/p/ui-part-3-the-widget-building-language
// - https://www.youtube.com/watch?v=-z8_F9ozERc
// - https://asawicki.info/Download/Productions/Lectures/Immediate%20Mode%20GUI.pdf

public class StbGui
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

    [Flags]
    public enum STBG_FONT_STYLE_FLAGS
    {
        NONE = 0,
        BOLD = 1 << 0,
        ITALIC = 1 << 1
    }

    public struct stbg_font_style
    {
        public float size;
        public STBG_FONT_STYLE_FLAGS style;
    }

    public struct stbg_font
    {
        public font_id id;
        public string name;
    }

    public struct stbg_text
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


        // Button styles
        BUTTON_BORDER_SIZE,
        BUTTON_PADDING_TOP,
        BUTTON_PADDING_BOTTOM,
        BUTTON_PADDING_LEFT,
        BUTTON_PADDING_RIGHT,


        // ALWAYS LAST!!
        COUNT
    }

    public struct stbg_theme
    {
        public font_id default_font_id;
        public stbg_font_style default_font_style;

        /// <summary>
        /// One value for each entry in STBG_WIDGET_STYLE
        /// </summary>
        public float[] styles;
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
        /// Intrinsic size, doesn't include children_padding, can be overriden if the widget doesn't fit in the expected bounds, or if it auto-expands
        /// </summary>
        public stbg_widget_intrinsic_size intrinsic_size;

        /// <summary>
        /// Intrinsic top-left position, used when the parent's children layout direction is FREE
        /// </summary>
        public stbg_position intrinsic_position;

        /// <summary>
        /// Children padding
        /// </summary>
        public stbg_padding children_padding;

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

    public class StbgAssertException : Exception
    {
        public StbgAssertException(string? message) : base(message) { }
    }


    public delegate stbg_size measure_text_delegate(ReadOnlySpan<char> text, stbg_font font, stbg_font_style style);

    public struct stbg_external_dependencies
    {
        /// <summary>
        /// Measure text
        /// </summary>
        public measure_text_delegate measure_text;
    }


    public const int DEFAULT_MAX_WIDGETS = 32767;

    public const int DEFAULT_MAX_FONTS = 32;

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
    }

    static private stbg_context context;

    /// <summary>
    /// Returns a reference to the current context
    /// </summary>
    /// <returns>Context</returns>
    public static ref stbg_context stbg_get_context() => ref context;

    /// <summary>
    /// Init GUI library.
    /// It initializes the shared context used by all the other functions.
    /// </summary>
    static public void stbg_init(stbg_external_dependencies external_dependencies, stbg_init_options options)
    {
        context = new stbg_context();

        stbg_init_context(ref context, external_dependencies, options);
    }

    /// <summary>
    /// Inits the default theme, must be called after calling stbg_init()
    /// </summary>
    static public void stbg_init_default_theme(font_id font_id, stbg_font_style font_style)
    {
        stbg__assert(!context.inside_frame);

        ref var theme = ref context.theme;

        theme.default_font_id = font_id;
        theme.default_font_style = font_style;

        var buttonBorder = 1.0f;
        var buttonPaddingTopBottom = MathF.Ceiling(font_style.size / 2);
        var buttonPaddingLeftRight = MathF.Ceiling(font_style.size / 2);

        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, buttonBorder);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_TOP, buttonPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM, buttonPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT, buttonPaddingLeftRight);
        stbg_set_widget_style(STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT, buttonPaddingLeftRight);

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
    }

    static public void stbg_set_widget_style(STBG_WIDGET_STYLE style, float value)
    {
        context.theme.styles[(int)style] = value;
    }

    static public float stbg_get_widget_style(STBG_WIDGET_STYLE style)
    {
        return context.theme.styles[(int)style];
    }

    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1)
    {
        return
            context.theme.styles[(int)style1];
    }

    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2)
    {
        return
            context.theme.styles[(int)style1] +
            context.theme.styles[(int)style2];
    }

    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3)
    {
        return
            context.theme.styles[(int)style1] +
            context.theme.styles[(int)style2] +
            context.theme.styles[(int)style3];
    }

    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3, STBG_WIDGET_STYLE style4)
    {
        return
            context.theme.styles[(int)style1] +
            context.theme.styles[(int)style2] +
            context.theme.styles[(int)style3] +
            context.theme.styles[(int)style4];
    }

    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3, STBG_WIDGET_STYLE style4, STBG_WIDGET_STYLE style5)
    {
        return
            context.theme.styles[(int)style1] +
            context.theme.styles[(int)style2] +
            context.theme.styles[(int)style3] +
            context.theme.styles[(int)style4] +
            context.theme.styles[(int)style5];
    }

    private static void stbg_init_context(ref stbg_context context, stbg_external_dependencies external_dependencies, stbg_init_options options)
    {
        stbg__assert(external_dependencies.measure_text != null, "Missing measure text function");

        if (options.max_widgets == 0)
            options.max_widgets = DEFAULT_MAX_WIDGETS;

        if (options.hash_table_size == 0)
            options.hash_table_size = options.max_widgets;

        if (options.max_fonts == 0)
            options.max_fonts = DEFAULT_MAX_FONTS;

        var widgets = new stbg_widget[options.max_widgets + 1]; // first slot is never used (null widget)
        var hashTable = new stbg_hash_entry[options.hash_table_size];
        var fonts = new stbg_font[options.max_fonts + 1]; // first slot is never used (null font)

        // init ids and flags
        for (int i = 0; i < widgets.Length; i++)
        {
            ref var widget = ref widgets[i];
            widget.id = i;
        }

        // init chain of free widgets
        // we start from the index 1, because we want to reserve the index 0 as "null"
        for (int i = 1; i < widgets.Length - 1; i++)
        {
            ref var widget = ref widgets[i];
            ref var nextWidget = ref widgets[i + 1];

            widget.hierarchy.next_sibling_id = nextWidget.id;
        }

        context.widgets = widgets;
        context.first_free_widget_id = context.widgets[1].id;
        context.hash_table = hashTable;
        context.fonts = fonts;
        context.first_free_font_id = 1;
        context.init_options = options;
        context.external_dependencies = external_dependencies;
        context.theme.styles = new float[(int)STBG_WIDGET_STYLE.COUNT];
    }

    /// <summary>
    /// Destroy GUI library.
    /// </summary>
    public static void stbg_destroy()
    {
        context = default;
    }

    /// <summary>
    /// Sets the screen size.
    /// Must be called at least once after the initialization, and after that every time that the screen size changes
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void stbg_set_screen_size(float width, float height)
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
        root.layout.constrains.min = new stbg_size();
        root.layout.constrains.max = context.screen_size;
        root.layout.children_padding = new stbg_padding();
        root.layout.children_layout_direction = STBG_CHILDREN_LAYOUT_DIRECTION.FREE;

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

    private static void stbg__layout_widgets()
    {
        // Ideas from:
        // - Originally using https://www.rfleury.com/p/ui-part-2-build-it-every-frame-immediate but it was waaay to complex
        // - Switched to https://docs.flutter.dev/ui/layout/constraints (let's see how it goes..)

        // - Constrains go down
        // - Sizes go up
        // - Parent sets position

        stbg_widget_constrains constrains = new stbg_widget_constrains();
        constrains.min.width = 0;
        constrains.min.height = 0;
        constrains.max.width = float.MaxValue;
        constrains.max.height = float.MaxValue;

        stbg__layout_widget(constrains, ref stbg_get_widget_by_id(context.root_widget_id));

        stbg__update_global_rect(ref stbg_get_widget_by_id(context.root_widget_id), new stbg_rect());

    }

    private static void stbg__update_global_rect(ref stbg_widget widget, stbg_rect parent_global_rect)
    {
        // Update self bounds
        ref var widget_computed_bounds = ref widget.computed_bounds;
        ref var widget_global_rect = ref widget.computed_bounds.global_rect;
        widget_global_rect.top_left.x = parent_global_rect.top_left.x + widget_computed_bounds.relative_position.x;
        widget_global_rect.top_left.y = parent_global_rect.top_left.y + widget_computed_bounds.relative_position.y;
        widget_global_rect.bottom_right.x = widget_global_rect.top_left.x + widget_computed_bounds.size.width;
        widget_global_rect.bottom_right.y = widget_global_rect.top_left.y + widget_computed_bounds.size.height;

        // Update children bounds
        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            // Iterate children
            var children_id = widget.hierarchy.first_children_id;

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                stbg__update_global_rect(ref children, widget_global_rect);

                children_id = children.hierarchy.next_sibling_id;
            } while (children_id != STBG_WIDGET_ID_NULL);
        }
    }

    public static font_id stbg_add_font(string name)
    {
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

    private static stbg_size stbg_measure_text(stbg_text text)
    {
        return context.external_dependencies.measure_text(text.text.Span, stbg_get_font_by_id(text.font_id), text.style);
    }

    private static stbg_size stbg__layout_widget(stbg_widget_constrains parent_constrains, ref stbg_widget widget)
    {
        // - Constrains go down
        // - Sizes go up
        // - Parent sets position
        var widget_layout = widget.layout;
        var widget_intrinsic_size = widget_layout.intrinsic_size;
        var widget_intrinsic_position = widget_layout.intrinsic_position;
        var widget_padding = widget.layout.children_padding;
        var widget_constrains = widget_layout.constrains;

        // Build current contrains by merging the widget constrains with the parent constrains
        var constrains = new stbg_widget_constrains();
        constrains.max.width = Math.Min(parent_constrains.max.width, widget_constrains.max.width);
        constrains.max.height = Math.Min(parent_constrains.max.height, widget_constrains.max.height);
        constrains.min.width = Math.Max(parent_constrains.min.width, widget_constrains.min.width);
        constrains.min.height = Math.Max(parent_constrains.min.height, widget_constrains.min.height);

        // Ensure that min values are never above max values
        constrains.min.width = Math.Min(widget_constrains.min.width, widget_constrains.max.width);
        constrains.min.height = Math.Min(widget_constrains.min.height, widget_constrains.max.height);

        stbg_size intrinsic_size =
            widget_intrinsic_size.type == STBG_INTRINSIC_SIZE_TYPE.FIXED_PIXELS ?
            widget_intrinsic_size.size :
            widget_intrinsic_size.type == STBG_INTRINSIC_SIZE_TYPE.MEASURE_TEXT ?
            stbg_measure_text(widget.text) :
            new stbg_size();

        var accumulated_children_size = new stbg_size();

        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            // Widget has children

            // Build children constrains by removing padding
            var children_constrains = new stbg_widget_constrains();
            children_constrains.max.width = Math.Max(constrains.max.width - (widget_padding.left + widget_padding.right), 0);
            children_constrains.max.height = Math.Max(constrains.max.height - (widget_padding.top + widget_padding.bottom), 0);
            children_constrains.min.width = Math.Min(constrains.min.width, children_constrains.max.width);
            children_constrains.min.height = Math.Min(constrains.min.height, children_constrains.max.height);

            // Iterate children
            var children_id = widget.hierarchy.first_children_id;

            // TODO:
            // - add "expand_width" and "expand_height" properties, used to resize children to fill all available space 
            //   (they would modify min.width and min.height to match the expected size)

            var next_children_top_left = new stbg_position()
            {
                x = widget_padding.left,
                y = widget_padding.top
            };

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                var children_size = stbg__layout_widget(children_constrains, ref children);

                stbg__assert_internal(children_size.width <= children_constrains.max.width);
                stbg__assert_internal(children_size.width >= children_constrains.min.width);
                stbg__assert_internal(children_size.height <= children_constrains.max.height);
                stbg__assert_internal(children_size.height >= children_constrains.min.height);

                children.computed_bounds.size = children_size;

                switch (widget.layout.children_layout_direction)
                {
                    case STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL:
                        children.computed_bounds.relative_position = next_children_top_left;
                        children_constrains.max.height -= children_size.height;
                        children_constrains.min.height = Math.Min(children_constrains.min.height, children_constrains.max.height);
                        accumulated_children_size.height += children_size.height;
                        accumulated_children_size.width = Math.Max(accumulated_children_size.width, children_size.width);
                        next_children_top_left.y += children_size.height;
                        break;

                    case STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL:
                        children.computed_bounds.relative_position = next_children_top_left;
                        children_constrains.max.width -= children_size.height;
                        children_constrains.min.width = Math.Min(children_constrains.min.width, children_constrains.max.width);
                        accumulated_children_size.width += children_size.width;
                        accumulated_children_size.height = Math.Max(accumulated_children_size.height, children_size.height);
                        next_children_top_left.x += children_size.width;
                        break;

                    case STBG_CHILDREN_LAYOUT_DIRECTION.FREE:
                        children.computed_bounds.relative_position.x =
                            next_children_top_left.x + Math.Min(children.layout.intrinsic_position.x, children_constrains.max.width - children_size.width);
                        children.computed_bounds.relative_position.y =
                            next_children_top_left.y + Math.Min(children.layout.intrinsic_position.y, children_constrains.max.height - children_size.height);
                        break;
                }

                children_id = children.hierarchy.next_sibling_id;
            } while (children_id != STBG_WIDGET_ID_NULL);
        }

        stbg_size widget_size = new stbg_size()
        {
            width = Math.Max(
                Math.Min(
                    Math.Max(intrinsic_size.width, accumulated_children_size.width) + widget_padding.right + widget_padding.left,
                    constrains.max.width),
                constrains.min.width),
            height = Math.Max(
                Math.Min(
                    Math.Max(intrinsic_size.height, accumulated_children_size.height) + widget_padding.top + widget_padding.bottom,
                    constrains.max.height),
                constrains.min.height),
        };


        return widget_size;
    }

    private static void stbg__destroy_unused_widgets(int amount_to_destroy)
    {
        var widgets = context.widgets;

        for (int i = 0; i < widgets.Length && amount_to_destroy > 0; i++)
        {
            if ((widgets[i].flags & STBG_WIDGET_FLAGS.USED) != 0 && widgets[i].last_used_in_frame != context.current_frame)
            {
                stbg__remove_widget(ref widgets[i]);
                amount_to_destroy--;
            }
        }
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
    /// Adds a button.
    /// Returns true if the button was pressed.
    /// </summary>
    /// <param name="label"></param>
    /// <returns>Returns true if the button was pressed</returns>
    public static bool stbg_button(string label)
    {
        ref var button = ref stbg__add_widget(STBG_WIDGET_TYPE.BUTTON, label, out _);
        button.text.text = label.AsMemory();

        ref var layout = ref button.layout;

        layout.constrains = stbg__build_constrains_unconstrained();
        layout.children_padding = new stbg_padding()
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

    public static widget_id stbg_get_last_widget_id()
    {
        return context.last_widget_id;
    }


    private static stbg_widget_intrinsic_size stbg__build_intrinsic_size_text()
    {
        return new stbg_widget_intrinsic_size()
        {
            type = STBG_INTRINSIC_SIZE_TYPE.MEASURE_TEXT,
        };
    }

    private static stbg_widget_intrinsic_size stbg__build_intrinsic_size_pixels(float width, float height)
    {
        return new stbg_widget_intrinsic_size()
        {
            type = STBG_INTRINSIC_SIZE_TYPE.FIXED_PIXELS,
            size = new stbg_size() { width = width, height = height },
        };
    }

    private static stbg_widget_constrains stbg__build_constrains_unconstrained()
    {
        return new stbg_widget_constrains()
        {
            min = new stbg_size() { width = 0, height = 0 },
            max = new stbg_size() { width = float.MaxValue, height = float.MaxValue },
        };
    }

    /// <summary>
    /// Begins a new window with the given name.
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
    /// <param name="label"></param>
    /// <returns>Returns if the window is visible or not</returns>
    public static bool stbg_begin_window(string label)
    {
        ref var window = ref stbg__add_widget(STBG_WIDGET_TYPE.WINDOW, label, out var is_new);

        window.text.text = label.AsMemory();

        ref var layout = ref window.layout;

        layout.children_padding = new stbg_padding()
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

        window.persistent_data.f3 = Math.Max(width - (window.layout.children_padding.left + window.layout.children_padding.right), 0);
        window.persistent_data.f4 = Math.Max(height - (window.layout.children_padding.top + window.layout.children_padding.bottom), 0);
        window.layout.intrinsic_size = stbg__build_intrinsic_size_pixels(window.persistent_data.f3, window.persistent_data.f4);
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
    /// Begins a new container with the specified layout direction
    /// </summary>
    /// <param name="identifier">Container identifier (must be unique inside the parent widget)</param>
    /// <param name="layout_direction">Layout direction</param>
    public static void stbg_begin_container(string identifier, STBG_CHILDREN_LAYOUT_DIRECTION layout_direction)
    {
        ref var container = ref stbg__add_widget(STBG_WIDGET_TYPE.CONTAINER, identifier, out _);

        ref var layout = ref container.layout;

        layout.children_padding = new stbg_padding();
        layout.constrains = stbg__build_constrains_unconstrained();
        layout.children_layout_direction = layout_direction;

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

    // Use to asssert public API access and potential user errors
    private static void stbg__assert(bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
    {
        switch (context.init_options.assert_behaviour)
        {
            case STBG_ASSERT_BEHAVIOUR.ASSERT:
                Debug.Assert(condition, message);
                break;
            case STBG_ASSERT_BEHAVIOUR.EXCEPTION:
                if (!condition)
                    throw new StbgAssertException(message);
                break;
            case STBG_ASSERT_BEHAVIOUR.CONSOLE:
                if (!condition)
                    Console.Error.WriteLine($"Failed assert: {message}");
                break;
            case STBG_ASSERT_BEHAVIOUR.NONE:
                break;
        }
    }

    // Use to assert internal code, it will be removed in production builds
    [Conditional("DEBUG")]
    private static void stbg__assert_internal(bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
    {
        switch (context.init_options.assert_behaviour)
        {
            case STBG_ASSERT_BEHAVIOUR.ASSERT:
                Debug.Assert(condition, message);
                break;
            case STBG_ASSERT_BEHAVIOUR.EXCEPTION:
                if (!condition)
                    throw new StbgAssertException(message);
                break;
            case STBG_ASSERT_BEHAVIOUR.CONSOLE:
                if (!condition)
                    Console.Error.WriteLine($"Failed assert: {message}");
                break;
            case STBG_ASSERT_BEHAVIOUR.NONE:
                break;
        }
    }

    private static ref stbg_widget stbg__add_widget(STBG_WIDGET_TYPE type, string identifier, out bool is_new)
    {
        ref var widget = ref stbg__add_widget(stbg__calculate_hash(type, identifier), out is_new);
        widget.type = type;
        context.last_widget_id = widget.id;

        widget.text.style = context.theme.default_font_style;
        widget.text.font_id = context.theme.default_font_id;

        return ref widget;
    }

    private static ref stbg_widget stbg__add_widget(widget_hash hash, out bool is_new)
    {
        stbg__assert(context.inside_frame);
        stbg__assert(context.first_free_widget_id != STBG_WIDGET_ID_NULL);

        ref var widget =
            ref (stbg__find_widget_by_hash(hash, out var existingWidgetId) ?
                ref stbg_get_widget_by_id(existingWidgetId) :
                ref stbg_get_widget_by_id(context.first_free_widget_id));

        if ((widget.flags & STBG_WIDGET_FLAGS.USED) == 0)
        {
            // New widget!
            is_new = true;
            context.frame_stats.new_widgets++;

            // Mark as used
            widget.flags |= STBG_WIDGET_FLAGS.USED;

            // Reset persistent data
            widget.persistent_data = new stbg_widget_persistent_data();

            // Update the next free widget id        
            context.first_free_widget_id = widget.hierarchy.next_sibling_id;
            widget.hierarchy.next_sibling_id = STBG_WIDGET_ID_NULL;

            // Add to the hash table
            widget.hash = hash;
            ref var bucket = ref stbg__get_hash_entry_by_hash(hash);

            if (bucket.first_widget_in_bucket != STBG_WIDGET_ID_NULL)
            {
                // Bucket already points to an existing widget, make that widget point back to us since the first
                // element in the bucket is going to be us.
                stbg_get_widget_by_id(bucket.first_widget_in_bucket).hash_chain.prev_same_bucket = widget.id;
                widget.hash_chain.next_same_bucket = bucket.first_widget_in_bucket;
            }
            bucket.first_widget_in_bucket = widget.id;
        }
        else
        {
            // Reused widget!
            is_new = false;
            context.frame_stats.reused_widgets++;
        }

        // Reset dynamic properties
        widget.layout = new stbg_widget_layout();
        widget.computed_bounds = new stbg_widget_computed_bounds();
        widget.text = new stbg_text();

        // Update last used
        if (widget.last_used_in_frame == context.current_frame)
        {
            context.frame_stats.duplicated_widgets_ids++;
            stbg__assert(widget.last_used_in_frame != context.current_frame, "Duplicated widget identifier!!");
        }
        widget.last_used_in_frame = context.current_frame;

        // Update hierarchy
        widget.hierarchy = new stbg_widget_hierarchy();
        widget.hierarchy.parent_id = context.current_widget_id;
        if (widget.hierarchy.parent_id != STBG_WIDGET_ID_NULL)
        {
            // If we have a parent:
            // - Update the list of children of our parent
            // - Update the list of siblings of any children
            ref var parent = ref stbg_get_widget_by_id(widget.hierarchy.parent_id);

            if (parent.hierarchy.last_children_id != STBG_WIDGET_ID_NULL)
            {
                // Parent already has children:
                // - set ourselves as the next sibling to the last children
                // - set our previous sibling to the last children
                // - replace the last children with ourselves
                stbg_get_widget_by_id(parent.hierarchy.last_children_id).hierarchy.next_sibling_id = widget.id;
                widget.hierarchy.prev_sibling_id = parent.hierarchy.last_children_id;
                parent.hierarchy.last_children_id = widget.id;
            }
            else
            {
                // Parent has no children, so we are the first and last children of our parent
                parent.hierarchy.first_children_id = parent.hierarchy.last_children_id = widget.id;
            }
        }

        return ref widget;
    }


    private static void stbg__remove_widget(ref stbg_widget widget)
    {
        stbg__assert_internal(!context.inside_frame);
        stbg__assert_internal((widget.flags & STBG_WIDGET_FLAGS.USED) != 0);
        stbg__assert_internal(widget.last_used_in_frame != context.current_frame);

        // Reset hierarchy
        widget.hierarchy = new stbg_widget_hierarchy();

        // Reset flags
        widget.flags = STBG_WIDGET_FLAGS.NONE;

        // Remove from the hash table
        if (widget.hash_chain.prev_same_bucket == STBG_WIDGET_ID_NULL)
        {
            // We are the first element in the bucket, make it point to the next element in our hash list
            ref var bucket = ref stbg__get_hash_entry_by_hash(widget.hash);

            stbg__assert_internal(bucket.first_widget_in_bucket == widget.id);

            bucket.first_widget_in_bucket = widget.hash_chain.next_same_bucket;
        }
        else
        {
            // We are NOT the first element, make the previous entry point to the next one in our hash list
            stbg_get_widget_by_id(widget.hash_chain.prev_same_bucket).hash_chain.next_same_bucket = widget.hash_chain.next_same_bucket;
        }

        widget.hash_chain = new stbg_widget_hash_chain();
        widget.hash = 0;

        // Update the next free widget id        
        widget.hierarchy.next_sibling_id = context.first_free_widget_id;
        context.first_free_widget_id = widget.id;

        context.frame_stats.destroyed_widgets++;
    }

    private static bool stbg__find_widget_by_hash(widget_hash hash, out widget_id foundId)
    {
        ref var bucket = ref stbg__get_hash_entry_by_hash(hash);

        if (bucket.first_widget_in_bucket != STBG_WIDGET_ID_NULL)
        {
            foundId = bucket.first_widget_in_bucket;

            do
            {
                ref var widget = ref stbg_get_widget_by_id(foundId);

                if (widget.hash == hash)
                    return true;

                foundId = widget.hash_chain.next_same_bucket;

            } while (foundId != STBG_WIDGET_ID_NULL);
        }

        foundId = STBG_WIDGET_ID_NULL;
        return false;
    }

    private static ref stbg_hash_entry stbg__get_hash_entry_by_hash(widget_hash hash)
    {
        int index = Math.Abs(hash % context.hash_table.Length);
        return ref context.hash_table[index];
    }

    private static widget_hash stbg__calculate_hash(STBG_WIDGET_TYPE type, string identifier)
    {
        Span<byte> key = stackalloc byte[sizeof(long)];
        Span<byte> output = stackalloc byte[sizeof(widget_hash)];

        if (context.current_widget_id == STBG_WIDGET_ID_NULL)
        {
            BitConverter.TryWriteBytes(key, 0x1234567890123456UL);
        }
        else
        {
            var parent_hash = stbg_get_widget_by_id(context.current_widget_id).hash;

            BitConverter.TryWriteBytes(key, parent_hash);
            BitConverter.TryWriteBytes(key.Slice(sizeof(widget_hash)), parent_hash);
        }

        key[0] += (byte)type; //Include the type of as part of the key, so changing the type produces a different hash

        var identifierAsBytes = MemoryMarshal.Cast<char, byte>(identifier.AsSpan());

        StbHash.stbh_halfsiphash(identifierAsBytes, key, output);

        widget_hash outputHash = BitConverter.ToInt32(output);

        return outputHash;
    }
}
