#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

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
        public float x0, y0;
        public float x1, y1;
    }


    public const int DEFAULT_MAX_WIDGETS = 32767;

    public const int DEFAULT_MAX_FONTS = 32;

    public const int DEFAULT_RENDER_QUEUE_SIZE = 128;

    public const int DEFAULT_STRING_MEMORY_POOL_SIZE = 1024 * 1024;

    public const int DEFAULT_MAX_USER_INPUT_EVENTS_QUEUE_SIZE = 128;

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
        /// Size of string memory pool, defaults to DEFAULT_STRING_MEMORY_POOL_SIZE
        /// </summary>
        public int string_memory_pool_size;

        /// <summary>
        /// Max number of loaded fonts, defaults to DEFAULT_MAX_FONTS
        /// </summary>
        public int max_fonts;

        /// <summary>
        /// Behavior of assert calls, defaults to ASSERT
        /// </summary>
        public STBG_ASSERT_BEHAVIOR assert_behavior;

        /// <summary>
        /// Size of render commands queue, defaults to DEFAULT_RENDER_QUEUE_SIZE
        /// </summary>
        public int render_commands_queue_size;

        /// <summary>
        /// Size of user input events queue, defaults to DEFAULT_MAX_USER_INPUT_EVENTS_QUEUE_SIZE
        /// </summary>
        public int max_user_input_events_queue_size;

        /// <summary>
        /// Disables the nesting of non window root elements into the debug window
        /// </summary>
        public bool dont_nest_non_window_root_elements_into_debug_window;
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
        /// Widget has an intrinsic size based on the measurement of the text
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

    /// <summary>
    /// Widget styles
    /// - Paddings always go in (top, bottom, left, right) order (so we can automate padding object construction given a single index)
    /// </summary>
    public enum STBG_WIDGET_STYLE
    {
        NONE = 0,

        // Root style
        ROOT_BACKGROUND_COLOR,

        // Debug window
        DEBUG_WINDOW_TITLE_TEXT_COLOR,
        DEBUG_WINDOW_TITLE_BACKGROUND_COLOR,
        DEBUG_WINDOW_TITLE_ACTIVE_TEXT_COLOR,
        DEBUG_WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR,

        // Window styles
        WINDOW_DEFAULT_WIDTH,
        WINDOW_DEFAULT_HEIGHT,
        WINDOW_SPACING_BETWEEN_NEW_WINDOWS,
        WINDOW_BORDER_RESIZE_TOLERANCE,
        WINDOW_ALLOW_SCROLLBARS,
        WINDOW_SCROLL_LINES_AMOUNT,
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
        WINDOW_CHILDREN_SPACING,
        WINDOW_BORDER_COLOR,
        WINDOW_BACKGROUND_COLOR,
        WINDOW_TITLE_TEXT_COLOR,
        WINDOW_TITLE_BACKGROUND_COLOR,
        WINDOW_TITLE_ACTIVE_TEXT_COLOR,
        WINDOW_TITLE_ACTIVE_BACKGROUND_COLOR,
        WINDOW_TITLE_CLOSE_BUTTON_SIZE,
        WINDOW_TITLE_CLOSE_BUTTON_COLOR,
        WINDOW_TITLE_CLOSE_BUTTON_HOVERED_COLOR,
        WINDOW_TITLE_CLOSE_BUTTON_PRESSED_COLOR,

        // Button styles
        BUTTON_BORDER_SIZE,
        BUTTON_PADDING_TOP,
        BUTTON_PADDING_BOTTOM,
        BUTTON_PADDING_LEFT,
        BUTTON_PADDING_RIGHT,
        BUTTON_BORDER_COLOR,
        BUTTON_BACKGROUND_COLOR,
        BUTTON_TEXT_COLOR,
        BUTTON_HOVERED_BORDER_COLOR,
        BUTTON_HOVERED_BACKGROUND_COLOR,
        BUTTON_HOVERED_TEXT_COLOR,
        BUTTON_PRESSED_BORDER_COLOR,
        BUTTON_PRESSED_BACKGROUND_COLOR,
        BUTTON_PRESSED_TEXT_COLOR,

        // Label styles
        LABEL_PADDING_TOP,
        LABEL_PADDING_BOTTOM,
        LABEL_PADDING_LEFT,
        LABEL_PADDING_RIGHT,
        LABEL_BACKGROUND_COLOR,
        LABEL_TEXT_COLOR,

        // Scrollbar styles
        SCROLLBAR_SIZE,
        SCROLLBAR_BUTTON_SIZE,
        SCROLLBAR_THUMB_SIZE,
        SCROLLBAR_BACKGROUND_COLOR,
        SCROLLBAR_THUMB_COLOR,
        SCROLLBAR_BUTTON_BACKGROUND_COLOR,
        SCROLLBAR_BUTTON_COLOR,
        SCROLLBAR_THUMB_HOVERED_COLOR,
        SCROLLBAR_BUTTON_HOVERED_BACKGROUND_COLOR,
        SCROLLBAR_BUTTON_HOVERED_COLOR,
        SCROLLBAR_THUMB_PRESSED_COLOR,
        SCROLLBAR_BUTTON_PRESSED_BACKGROUND_COLOR,
        SCROLLBAR_BUTTON_PRESSED_COLOR,


        // Textbox styles
        TEXTBOX_PADDING_TOP,
        TEXTBOX_PADDING_BOTTOM,
        TEXTBOX_PADDING_LEFT,
        TEXTBOX_PADDING_RIGHT,
        TEXTBOX_BACKGROUND_COLOR,
        TEXTBOX_BORDER_COLOR,
        TEXTBOX_BORDER_SIZE,
        TEXTBOX_TEXT_COLOR,
        TEXTBOX_CURSOR_COLOR,
        TEXTBOX_CURSOR_HEIGHT,
        TEXTBOX_CURSOR_WIDTH,

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

    public enum STBG_CHILDREN_LAYOUT
    {
        FREE,
        VERTICAL,
        HORIZONTAL,
    }

    [Flags]
    public enum STBG_WIDGET_LAYOUT_FLAGS
    {
        NONE = 0,
        ALLOW_CHILDREN_OVERFLOW = 1 << 0,
        PARENT_CONTROLLED = 1 << 1,
    }

    public struct stbg_widget_layout
    {
        /// <summary>
        /// Widget size constrains, can be overridden if the widget doesn't fit in the expected bounds, or if it auto-expands
        /// </summary>
        public stbg_widget_constrains constrains;

        /// <summary>
        /// Intrinsic size, doesn't include inner_padding, can be overridden if the widget doesn't fit in the expected bounds, or if it auto-expands
        /// </summary>
        public stbg_widget_intrinsic_size intrinsic_size;

        /// <summary>
        /// Intrinsic top-left position, used when the parent's children layout direction is FREE
        /// </summary>
        public stbg_position intrinsic_position;

        /// <summary>
        /// Intrinsic sorting index value, used when the parent's children layout direction is FREE to sort the children
        /// </summary>
        public int intrinsic_sorting_index;

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
        public STBG_CHILDREN_LAYOUT children_layout_direction;

        /// <summary>
        /// Children offset, doesn't include padding
        /// </summary>
        public stbg_position children_offset;

        /// <summary>
        /// Widget layout flags
        /// </summary>
        public STBG_WIDGET_LAYOUT_FLAGS flags;

        /// <summary>
        /// Last frame in which the widget was layout
        /// </summary>
        public int last_layout_in_frame;
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
        /// <summary>
        /// Position relative to parent
        /// </summary>
        public stbg_position position;

        /// <summary>
        /// Size
        /// </summary>
        public stbg_size size;

        /// <summary>
        /// Global rect bounds
        /// </summary>
        public stbg_rect global_rect;

        /// <summary>
        /// Children size
        /// </summary>
        public stbg_size children_size;
    }

    public enum STBG_WIDGET_TYPE
    {
        NONE,
        ROOT,
        WINDOW,
        CONTAINER,
        BUTTON,
        LABEL,
        SCROLLBAR,
        TEXTBOX,
        COUNT, // MUST BE LAST
    }

    [Flags]
    public enum STBG_WIDGET_FLAGS
    {
        NONE = 0,
        USED = 1 << 0,
        IGNORE = 1 << 1,
    }

    [Flags]
    public enum STBG_WIDGET_INPUT_FLAGS
    {
        NONE = 0,
        CLICKED = 1 << 0,
        VALUE_UPDATED = 1 << 1,
    }

    public struct stbg_widget_hash_chain
    {
        public widget_id next_same_bucket;
        public widget_id prev_same_bucket;
    }

    public struct stbg_widget_value
    {
        public float f;
        public bool b;
    }

    public struct stbg_widget_parameters
    {
        public int sub_type;
        public int flags;
        public stbg_widget_value parameter1;
        public stbg_widget_value parameter2;
        public stbg_widget_value min_value;
        public stbg_widget_value max_value;
    }

    public struct stbg_widget_properties
    {
        public stbg_widget_parameters parameters;

        public stbg_widget_layout layout;

        public stbg_widget_computed_bounds computed_bounds;

        public ReadOnlyMemory<char> text;

        public Memory<char> text_editable;
        public int text_editable_length;

        public stbg_widget_value value;

        public float mouse_tolerance;

        public STBG_WIDGET_INPUT_FLAGS input_flags;
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

        public stbg_widget_properties properties;
    }


    /// <summary>
    /// Input state derived from user input
    /// </summary>
    public struct stbg_input_derived
    {
        public stbg_position mouse_position;
        public bool mouse_position_valid;

        public stbg_position mouse_wheel_scroll_amount;

        /// <summary>
        /// True only the first frame that is down
        /// </summary>
        public bool mouse_button_1_down;
        /// <summary>
        /// True only while the button is pressed
        /// </summary>
        public bool mouse_button_1;
        /// <summary>
        /// True only the first frame that is released
        /// </summary>
        public bool mouse_button_1_up;

        /// <summary>
        /// True only the first frame that is down
        /// </summary>
        public bool mouse_button_2_down;
        /// <summary>
        /// True only while the button is pressed
        /// </summary>
        public bool mouse_button_2;
        /// <summary>
        /// True only the first frame that is released
        /// </summary>
        public bool mouse_button_2_up;
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
        public int string_memory_pool_used_characters;
        public int string_memory_pool_overflowed_characters;
    }

    public struct stbg_context_input_feedback
    {
        public widget_id hovered_widget_id;
        public widget_id pressed_widget_id;
        public widget_id dragged_widget_id;
        public widget_id active_widget_id;
        public widget_id editing_text_widget_id;

        public stbg_input_method_editor_info ime_info;

        public int hovered_sub_widget_part;
        public int pressed_sub_widget_part;
        public float drag_resize_x;
        public float drag_resize_y;
        public float drag_from_widget_x;
        public float drag_from_widget_y;
    }

    public struct stbg_string_memory_pool
    {
        public Memory<char> memory_pool;
        public int offset;
    }

    public struct stbg_text_edit
    {
        public widget_id widget_id;
        public widget_hash widget_hash;
        public StbTextEdit.STB_TexteditState state;
        public StbTextEdit.STB_TEXTEDIT_STRING str;
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

        public bool last_widget_is_new;

        public widget_id root_widget_id;

        public stbg_context_frame_stats frame_stats;

        public stbg_context_frame_stats prev_frame_stats;

        public stbg_init_options init_options;

        public stbg_external_dependencies external_dependencies;

        public stbg_size screen_size;

        public stbg_theme theme;

        public stbg_render_context render_context;

        public stbg_position next_new_window_position;

        public stbg_user_input_input_event[] user_input_events_queue;

        public int user_input_events_queue_offset;

        public stbg_input_derived input;

        public stbg_context_input_feedback input_feedback;

        public STBG_ACTIVE_CURSOR_TYPE active_cursor;

        public stbg_string_memory_pool string_memory_pool;

        public stbg_text_edit text_edit;
    }

    public enum STBG_ASSERT_BEHAVIOR
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

        /// <summary>
        /// Push clipping rect using bounds
        /// </summary>
        PUSH_CLIPPING_RECT,

        /// <summary>
        /// Pops clipping rect
        /// </summary>
        POP_CLIPPING_RECT,
    }

    public record struct stbg_render_command
    {
        public STBG_RENDER_COMMAND_TYPE type;
        public float size;
        public stbg_rect bounds;
        public stbg_color color;
        public stbg_color background_color;
        public stbg_text text;
        public float text_horizontal_alignment;
        public float text_vertical_alignment;
        public STBG_MEASURE_TEXT_OPTIONS text_measure_options;
        public STBG_RENDER_TEXT_OPTIONS text_render_options;
    }


    public struct stbg_input_method_editor_info
    {
        public bool enable;
        public stbg_rect editing_global_rect;
        public float editing_cursor_global_x;
        public widget_id widget_id;
    }

    [Flags]
    public enum STBG_MEASURE_TEXT_OPTIONS
    {
        NONE = 0,
        USE_ONLY_BASELINE_FOR_FIRST_LINE = 1 << 0,
        IGNORE_METRICS = 1 << 1,
        SINGLE_LINE = 1 << 2,
    }

    public struct stbg_external_dependencies
    {
        /// <summary>
        /// Measure text
        /// </summary>
        public delegate stbg_size stbg_measure_text_delegate(ReadOnlySpan<char> text, stbg_font font, stbg_font_style style, STBG_MEASURE_TEXT_OPTIONS measure_options);
        public stbg_measure_text_delegate measure_text;

        /// <summary>
        /// Get character position in text
        /// </summary>
        public delegate stbg_position stbg_get_character_position_in_text_delegate(ReadOnlySpan<char> text, stbg_font font, stbg_font_style style, STBG_MEASURE_TEXT_OPTIONS options, int character_index);
        public stbg_get_character_position_in_text_delegate get_character_position_in_text;

        /// <summary>
        /// Render
        /// </summary>
        public delegate void stbg_render_delegate(Span<stbg_render_command> commands);
        public stbg_render_delegate render;

        /// <summary>
        /// Set input method editor (show keyboard)
        /// </summary>
        public delegate void stbg_set_input_method_editor_delegate(stbg_input_method_editor_info info);
        public stbg_set_input_method_editor_delegate set_input_method_editor;

        /// <summary>
        /// Copy text to clipboard
        /// </summary>
        public delegate void stbg_copy_text_to_clipboard_delegate(ReadOnlySpan<char> text_to_copy);
        public stbg_copy_text_to_clipboard_delegate copy_text_to_clipboard;

        /// <summary>
        /// Get text from clipboard
        /// </summary>
        public delegate ReadOnlySpan<char> stbg_get_clipboard_text_delegate();
        public stbg_get_clipboard_text_delegate get_clipboard_text;
    }


    /// <summary>
    /// Active cursor (based on SDL library!!)
    /// </summary>
    public enum STBG_ACTIVE_CURSOR_TYPE
    {
        /// <summary>
        /// Default cursor. Usually an arrow.
        /// </summary>
        DEFAULT,
        /// <summary>
        /// Text selection. Usually an I-beam.
        /// </summary>
        TEXT,
        /// <summary>
        /// Wait. Usually an hourglass or watch or spinning ball.
        /// </summary>
        WAIT,
        /// <summary>
        /// Crosshair.
        /// </summary>
        CROSSHAIR,
        /// <summary>
        /// Program is busy but still interactive. Usually it's WAIT with an arrow.
        /// </summary>
        PROGRESS,
        /// <summary>
        /// Four pointed arrow pointing north, south, east, and west.
        /// </summary>
        MOVE,
        /// <summary>
        /// Not permitted. Usually a slashed circle or crossbones.
        /// </summary>
        NOT_ALLOW,
        /// <summary>
        /// Pointer that indicates a link. Usually a pointing hand.
        /// </summary>
        POINTER,
        /// <summary>
        /// Double arrow pointing northwest and southeast.
        /// </summary>
        RESIZE_NWSE,
        /// <summary>
        /// Double arrow pointing northeast and southwest.
        /// </summary>
        RESIZE_NESW,
        /// <summary>
        /// Double arrow pointing west and east.
        /// </summary>
        RESIZE_EW,
        /// <summary>
        /// Double arrow pointing north and south.
        /// </summary>
        RESIZE_NS,
        /// <summary>
        /// Window resize top-left.
        /// </summary>
        RESIZE_NW,
        /// <summary>
        /// Window resize top.
        /// </summary>
        RESIZE_N,
        /// <summary>
        /// Window resize top-right.
        /// </summary>
        RESIZE_NE,
        /// <summary>
        /// Window resize right
        /// </summary>
        RESIZE_E,
        /// <summary>
        /// Window resize bottom-right
        /// </summary>
        RESIZE_SE,
        /// <summary>
        /// Window resize bottom
        /// </summary>
        RESIZE_S,
        /// <summary>
        /// Window resize bottom-left
        /// </summary>
        RESIZE_SW,
        /// <summary>
        /// Window resize left
        /// </summary>
        RESIZE_W,
    }

    public enum STBG_SCROLLBAR_DIRECTION
    {
        HORIZONTAL,
        VERTICAL
    }
}
