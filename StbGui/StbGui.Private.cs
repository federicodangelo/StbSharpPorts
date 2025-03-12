#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using font_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{
    private static stbg_context context;

    private static void stbg_init_context(ref stbg_context context, stbg_external_dependencies external_dependencies, stbg_init_options options)
    {
        // Initialize the assert behavior first so all configuration related asserts work as expected
        context.init_options.assert_behavior = options.assert_behavior;

        stbg__assert(external_dependencies.render_adapter != null);
        stbg__assert(external_dependencies.get_clipboard_text != null);
        stbg__assert(external_dependencies.copy_text_to_clipboard != null);
        stbg__assert(external_dependencies.set_input_method_editor != null);
        stbg__assert(external_dependencies.get_performance_counter != null);

        if (options.max_widgets == 0)
            options.max_widgets = DEFAULT_MAX_WIDGETS;

        if (options.hash_table_size == 0)
            options.hash_table_size = options.max_widgets;

        if (options.max_fonts == 0)
            options.max_fonts = DEFAULT_MAX_FONTS;

        if (options.max_images == 0)
            options.max_images = DEFAULT_MAX_IMAGES;

        if (options.string_memory_pool_size == 0)
            options.string_memory_pool_size = DEFAULT_STRING_MEMORY_POOL_SIZE;

        if (options.render_commands_queue_size == 0)
            options.render_commands_queue_size = DEFAULT_RENDER_QUEUE_SIZE;

        if (options.max_user_input_events_queue_size == 0)
            options.max_user_input_events_queue_size = DEFAULT_MAX_USER_INPUT_EVENTS_QUEUE_SIZE;

        var widgets = new stbg_widget[options.max_widgets + 1]; // first slot is never used (null widget)
        var widgets_reference_properties = new stbg_widget_reference_properties[options.max_widgets + 1]; // first slot is never used (null widget)
        var hash_table = new stbg_hash_entry[options.hash_table_size];
        var fonts = new stbg_font[options.max_fonts + 1]; // first slot is never used (null font)
        var images = new stbg_image_info[options.max_images + 1]; // first slot is never used (null image)
        var force_render_queue_entries = !options.force_always_render ?
             new stbg_force_render_queue_entry[Math.Max(options.max_widgets / 10, 100)] : // 10% of the widgets are forced to render at most];
             [];

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
        context.widgets_reference_properties = widgets_reference_properties;
        context.first_free_widget_id = context.widgets[1].id;
        context.hash_table = hash_table;
        context.fonts = fonts;
        context.first_free_font_id = 1;
        context.images = images;
        context.first_free_image_id = 1;
        context.init_options = options;
        context.external_dependencies = external_dependencies;
        context.theme.styles = new double[(int)STBG_WIDGET_STYLE.COUNT];
#pragma warning disable CS8601 // Possible null reference assignment.
        context.render_context.render_adapter = external_dependencies.render_adapter;
#pragma warning restore CS8601 // Possible null reference assignment.
        context.user_input_events_queue = new stbg_user_input_input_event[options.max_user_input_events_queue_size];
        context.force_render_queue.entries = force_render_queue_entries;
        stbg__string_memory_pool_init(ref context.string_memory_pool, options.string_memory_pool_size);

        for (var i = 0; i < STBG__WIDGET_INIT_CONTEXT_LIST.Length; i++)
            STBG__WIDGET_INIT_CONTEXT_LIST[i](ref context);
    }

    private static stbg_size stbg__measure_text(stbg_text text, STBG_MEASURE_TEXT_OPTIONS options)
    {
        return context.render_context.render_adapter.measure_text(text.text.Span, stbg_get_font_by_id(text.font_id), text.style, options);
    }

    private static stbg_position stbg__get_character_position_in_text(stbg_text text, int character_index, STBG_MEASURE_TEXT_OPTIONS options)
    {
        return context.render_context.render_adapter.get_character_position_in_text(text.text.Span, stbg_get_font_by_id(text.font_id), text.style, options, character_index);
    }

    private static void stbg__destroy_unused_widgets(int amount_to_destroy)
    {
        var widgets = context.widgets;
        var widgets_reference_properties = context.widgets_reference_properties;

        for (int i = 0; i < widgets.Length && amount_to_destroy > 0; i++)
        {
            if ((widgets[i].flags & STBG_WIDGET_FLAGS.USED) != 0 && widgets_reference_properties[i].last_used_in_frame != context.current_frame)
            {
                stbg__destroy_widget(ref widgets[i]);
                amount_to_destroy--;
            }
        }

        stbg__assert_internal(amount_to_destroy == 0);
    }

    private const string DEBUG_WINDOW_TITLE = "_DEBUG_";
    private static readonly widget_hash debug_window_hash = stbg__calculate_hash(STBG_WIDGET_TYPE.WINDOW, DEBUG_WINDOW_TITLE, true);

    [Flags]
    private enum STBG__WIDGET_ADD_OPTIONS
    {
        NONE = 0,
        IGNORE_DUPLICATED = 1 << 0,
    }

    private static ref stbg_widget stbg__get_or_create_debug_window()
    {
        ref var debug_window = ref stbg__add_widget(debug_window_hash, STBG_WIDGET_TYPE.WINDOW, context.root_widget_id, out var is_new, out var is_already_created_in_same_frame, STBG__WIDGET_ADD_OPTIONS.IGNORE_DUPLICATED);

        if (!is_already_created_in_same_frame)
        {
            var is_open = true;
            stbg__window_init(ref debug_window, ref is_open, is_new, DEBUG_WINDOW_TITLE, STBG_WINDOW_OPTIONS.DEFAULT);
        }

        if (is_new)
            stbg_set_widget_position(debug_window.id, stbg_get_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_POSITION_X), stbg_get_widget_style(STBG_WIDGET_STYLE.DEBUG_WINDOW_POSITION_Y));

        return ref debug_window;
    }

    private static ref stbg_widget stbg__add_widget(STBG_WIDGET_TYPE type, ReadOnlySpan<char> identifier, out bool is_new)
    {
        ref var widget = ref stbg__add_widget(stbg__calculate_hash(type, identifier), type, context.current_widget_id, out is_new, out _);
        context.last_widget_id = widget.id;
        context.last_widget_is_new = is_new;

        return ref widget;
    }

    private static ref stbg_widget stbg__add_widget(STBG_WIDGET_TYPE type, ReadOnlySpan<char> identifier, out bool is_new, out bool is_already_created_in_same_frame)
    {
        ref var widget = ref stbg__add_widget(stbg__calculate_hash(type, identifier), type, context.current_widget_id, out is_new, out is_already_created_in_same_frame, STBG__WIDGET_ADD_OPTIONS.IGNORE_DUPLICATED);
        context.last_widget_id = widget.id;
        context.last_widget_is_new = is_new;

        return ref widget;
    }

    private static ref stbg_widget stbg__add_widget(widget_hash hash, STBG_WIDGET_TYPE type, widget_id parent_id, out bool is_new, out bool is_already_created_in_same_frame, STBG__WIDGET_ADD_OPTIONS options = STBG__WIDGET_ADD_OPTIONS.NONE)
    {
        stbg__assert(context.inside_frame);
        stbg__assert(context.first_free_widget_id != STBG_WIDGET_ID_NULL, "No more room for widgets");
        stbg__assert(parent_id == STBG_WIDGET_ID_NULL || (stbg__get_widget_by_id_internal(parent_id).flags & STBG_WIDGET_FLAGS.ALLOW_CHILDREN) != 0, "Parent widget does not allow children");

        ref var widget =
            ref (stbg__find_widget_by_hash(hash, out var existingWidgetId) ?
                ref stbg__get_widget_by_id_internal(existingWidgetId) :
                ref stbg__get_widget_by_id_internal(context.first_free_widget_id));

        ref var widget_ref_props = ref stbg__get_widget_ref_props_by_id_internal(widget.id);

        is_already_created_in_same_frame = false;

        if ((widget.flags & STBG_WIDGET_FLAGS.USED) == 0)
        {
            // New widget!
            is_new = true;

            context.frame_stats.new_widgets++;

            // Mark as used
            widget.flags |= STBG_WIDGET_FLAGS.USED;

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
                stbg__get_widget_by_id_internal(bucket.first_widget_in_bucket).hash_chain.prev_same_bucket = widget.id;
                widget.hash_chain.next_same_bucket = bucket.first_widget_in_bucket;
            }
            bucket.first_widget_in_bucket = widget.id;

            // Reset dynamic properties (no need, they are reset in the destroy method)
            // widget.properties = new stbg_widget_properties();
            // widget_ref_props = new stbg_widget_reference_properties();

            // Reset hierarchy
            widget.hierarchy = new stbg_widget_hierarchy();

            // Set type
            widget.type = type;
        }
        else
        {
            // Reused widget!
            is_new = false;
            is_already_created_in_same_frame = widget_ref_props.last_used_in_frame == context.current_frame;
            if (!is_already_created_in_same_frame)
            {
                context.frame_stats.reused_widgets++;
                widget.hierarchy = new stbg_widget_hierarchy();
            }
        }

        // Update last used
        if (is_already_created_in_same_frame && (options & STBG__WIDGET_ADD_OPTIONS.IGNORE_DUPLICATED) == 0)
        {
            context.frame_stats.duplicated_widgets_ids++;
            stbg__assert(widget_ref_props.last_used_in_frame != context.current_frame, "Duplicated widget identifier!!");
        }
        widget_ref_props.last_used_in_frame = context.current_frame;

        // If the parent is root and we are not creating a window, use the debug window
        if (type != STBG_WIDGET_TYPE.WINDOW &&
            context.root_widget_id != STBG_WIDGET_ID_NULL &&
            parent_id == context.root_widget_id &&
            !context.init_options.dont_nest_non_window_root_elements_into_debug_window)
        {
            parent_id = stbg__get_or_create_debug_window().id;
        }

        // Update hierarchy
        if (parent_id != STBG_WIDGET_ID_NULL)
        {
            if (!is_already_created_in_same_frame || widget.hierarchy.parent_id != parent_id)
                stbg__add_widget_to_parent_last(ref widget, parent_id);
        }

        return ref widget;
    }

    private static void stbg__set_widget_last_in_parent(ref stbg_widget widget)
    {
        if (widget.hierarchy.next_sibling_id != STBG_WIDGET_ID_NULL)
        {
            var parent_id = widget.hierarchy.parent_id;
            stbg__remove_widget_from_parent(ref widget);
            stbg__add_widget_to_parent_last(ref widget, parent_id);
        }
    }

    private static void stbg__remove_widget_from_parent(ref stbg_widget widget)
    {
        stbg__assert_internal(widget.hierarchy.parent_id != STBG_WIDGET_ID_NULL);

        ref var parent = ref stbg__get_widget_by_id_internal(widget.hierarchy.parent_id);

        // Update parent's first / last widget id if it matches the widget to remove
        if (parent.hierarchy.first_children_id == widget.id)
            parent.hierarchy.first_children_id = widget.hierarchy.next_sibling_id;

        if (parent.hierarchy.last_children_id == widget.id)
            parent.hierarchy.last_children_id = widget.hierarchy.prev_sibling_id;

        // If there is a next widget, update it to point to our previous widget
        if (widget.hierarchy.next_sibling_id != STBG_WIDGET_ID_NULL)
            stbg__get_widget_by_id_internal(widget.hierarchy.next_sibling_id).hierarchy.prev_sibling_id = widget.hierarchy.prev_sibling_id;

        // If there is a previous widget, update it to point to our next widget
        if (widget.hierarchy.prev_sibling_id != STBG_WIDGET_ID_NULL)
            stbg__get_widget_by_id_internal(widget.hierarchy.prev_sibling_id).hierarchy.next_sibling_id = widget.hierarchy.next_sibling_id;

        // NULL parent and next / prev siblings
        widget.hierarchy.parent_id = STBG_WIDGET_ID_NULL;
        widget.hierarchy.next_sibling_id = STBG_WIDGET_ID_NULL;
        widget.hierarchy.prev_sibling_id = STBG_WIDGET_ID_NULL;
    }

    private static void stbg__add_widget_to_parent_last(ref stbg_widget widget, widget_id parent_id)
    {
        stbg__assert_internal(widget.hierarchy.parent_id == STBG_WIDGET_ID_NULL);
        stbg__assert_internal(widget.hierarchy.next_sibling_id == STBG_WIDGET_ID_NULL);
        stbg__assert_internal(widget.hierarchy.prev_sibling_id == STBG_WIDGET_ID_NULL);
        stbg__assert_internal(parent_id != STBG_WIDGET_ID_NULL);

        ref var parent = ref stbg__get_widget_by_id_internal(parent_id);

        widget.hierarchy.parent_id = parent_id;

        // - Update the list of children of our parent
        // - Update the list of siblings of any children
        if (parent.hierarchy.last_children_id != STBG_WIDGET_ID_NULL)
        {
            // Parent already has children:
            // - set ourselves as the next sibling to the last children
            // - set our previous sibling to the last children
            // - replace the last children with ourselves
            stbg__get_widget_by_id_internal(parent.hierarchy.last_children_id).hierarchy.next_sibling_id = widget.id;
            widget.hierarchy.prev_sibling_id = parent.hierarchy.last_children_id;
            parent.hierarchy.last_children_id = widget.id;
        }
        else
        {
            // Parent has no children, so we are the first and last children of our parent
            parent.hierarchy.first_children_id = parent.hierarchy.last_children_id = widget.id;
        }
    }

    private static void stbg__add_widget_to_parent_after_sibling_or_first(ref stbg_widget widget, widget_id prev_sibling_id, widget_id parent_id)
    {
        stbg__assert_internal(widget.hierarchy.parent_id == STBG_WIDGET_ID_NULL);
        stbg__assert_internal(parent_id != STBG_WIDGET_ID_NULL);

        ref var parent = ref stbg__get_widget_by_id_internal(parent_id);

        widget.hierarchy.parent_id = parent_id;

        if (prev_sibling_id == STBG_WIDGET_ID_NULL)
        {
            // No previous sibling, add as first children of parent
            widget.hierarchy.next_sibling_id = parent.hierarchy.first_children_id;
            parent.hierarchy.first_children_id = widget.id;

            // Update next sibling to point back to us
            if (widget.hierarchy.next_sibling_id != STBG_WIDGET_ID_NULL)
            {
                stbg__get_widget_by_id_internal(widget.hierarchy.next_sibling_id).hierarchy.prev_sibling_id = widget.id;
            }
            else // There is no next sibling, so we are the only children of our parent
            {
                parent.hierarchy.last_children_id = widget.id;
            }
        }
        else
        {
            // Insert ourselves between our previous sibling and it's next sibling
            ref var prev_sibling = ref stbg__get_widget_by_id_internal(prev_sibling_id);
            stbg__assert_internal(prev_sibling.hierarchy.parent_id == parent_id);

            widget.hierarchy.next_sibling_id = prev_sibling.hierarchy.next_sibling_id;
            widget.hierarchy.prev_sibling_id = prev_sibling.id;
            prev_sibling.hierarchy.next_sibling_id = widget.id;
        }

        // Update next sibling to point back to us
        if (widget.hierarchy.next_sibling_id != STBG_WIDGET_ID_NULL)
        {
            stbg__get_widget_by_id_internal(widget.hierarchy.next_sibling_id).hierarchy.prev_sibling_id = widget.id;
        }
        else // There is no next sibling, so we are the last children of our parent
        {
            parent.hierarchy.last_children_id = widget.id;
        }
    }

    private static void stbg__destroy_widget(ref stbg_widget widget)
    {
        ref var widget_ref_props = ref stbg__get_widget_ref_props_by_id_internal(widget.id);

        stbg__assert_internal(!context.inside_frame);
        stbg__assert_internal((widget.flags & STBG_WIDGET_FLAGS.USED) != 0);
        stbg__assert_internal(widget_ref_props.last_used_in_frame != context.current_frame);

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

            if (widget.hash_chain.next_same_bucket != STBG_WIDGET_ID_NULL)
                stbg__get_widget_by_id_internal(widget.hash_chain.next_same_bucket).hash_chain.prev_same_bucket = STBG_WIDGET_ID_NULL;
        }
        else
        {
            // We are NOT the first element, make the previous entry point to the next one in our hash list
            stbg__get_widget_by_id_internal(widget.hash_chain.prev_same_bucket).hash_chain.next_same_bucket = widget.hash_chain.next_same_bucket;

            // And now make the next entry point to the previous one in our hash list
            if (widget.hash_chain.next_same_bucket != STBG_WIDGET_ID_NULL)
                stbg__get_widget_by_id_internal(widget.hash_chain.next_same_bucket).hash_chain.prev_same_bucket = widget.hash_chain.prev_same_bucket;
        }

        widget.hash_chain = new stbg_widget_hash_chain();
        widget.hash = 0;

        // Update the next free widget id        
        widget.hierarchy.next_sibling_id = context.first_free_widget_id;
        context.first_free_widget_id = widget.id;

        context.frame_stats.destroyed_widgets++;

        // Clear widget properties
        widget.properties = new stbg_widget_properties();
        widget_ref_props = new stbg_widget_reference_properties();

        // Remove widget from input feedback
        if (context.input_feedback.dragged_widget_id == widget.id)
            context.input_feedback.dragged_widget_id = STBG_WIDGET_ID_NULL;
        if (context.input_feedback.hovered_widget_id == widget.id)
            context.input_feedback.hovered_widget_id = STBG_WIDGET_ID_NULL;
        if (context.input_feedback.pressed_widget_id == widget.id)
            context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
        if (context.input_feedback.active_window_id == widget.id)
            context.input_feedback.active_window_id = STBG_WIDGET_ID_NULL;
        if (context.input_feedback.editing_text_widget_id == widget.id)
            context.input_feedback.editing_text_widget_id = STBG_WIDGET_ID_NULL;
        if (context.input_feedback.ime_info.widget_id == widget.id)
            context.input_feedback.ime_info = new stbg_input_method_editor_info();
    }

    private static bool stbg__find_widget_parent_by_type(widget_id widget_id, STBG_WIDGET_TYPE type, out widget_id found_id)
    {
        while (widget_id != STBG_WIDGET_ID_NULL)
        {
            ref var widget = ref stbg__get_widget_by_id_internal(widget_id);

            if (widget.type == type)
            {
                found_id = widget.id;
                return true;
            }

            widget_id = widget.hierarchy.parent_id;
        }

        found_id = STBG_WIDGET_ID_NULL;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref stbg_widget stbg__get_widget_by_id_internal(widget_id id)
    {
        stbg__assert_internal(id != STBG_WIDGET_ID_NULL);
        return ref context.widgets[id];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref stbg_widget_reference_properties stbg__get_widget_ref_props_by_id_internal(widget_id id)
    {
        stbg__assert_internal(id != STBG_WIDGET_ID_NULL);
        return ref context.widgets_reference_properties[id];
    }

    private static bool stbg__find_widget_by_hash(widget_hash hash, out widget_id found_id)
    {
        ref var bucket = ref stbg__get_hash_entry_by_hash(hash);

        if (bucket.first_widget_in_bucket != STBG_WIDGET_ID_NULL)
        {
            found_id = bucket.first_widget_in_bucket;

            do
            {
                ref var widget = ref stbg__get_widget_by_id_internal(found_id);

                if (widget.hash == hash)
                    return true;

                found_id = widget.hash_chain.next_same_bucket;

            } while (found_id != STBG_WIDGET_ID_NULL);
        }

        found_id = STBG_WIDGET_ID_NULL;
        return false;
    }

    private static ref stbg_hash_entry stbg__get_hash_entry_by_hash(widget_hash hash)
    {
        int index = Math.Abs(hash % context.hash_table.Length);
        return ref context.hash_table[index];
    }

    private static widget_hash stbg__calculate_hash(STBG_WIDGET_TYPE type, ReadOnlySpan<char> identifier, bool ignore_parent = false)
    {
        return stbg__calculate_hash(type, MemoryMarshal.Cast<char, byte>(identifier), ignore_parent);
    }

    private static widget_hash stbg__calculate_hash(STBG_WIDGET_TYPE type, ReadOnlySpan<byte> identifier, bool ignore_parent = false)
    {
        Span<byte> key = stackalloc byte[sizeof(long)];
        Span<byte> output = stackalloc byte[sizeof(widget_hash)];

        if (context.current_widget_id == STBG_WIDGET_ID_NULL || ignore_parent)
        {
            BitConverter.TryWriteBytes(key, 0x1234567890123456UL);
        }
        else
        {
            var parent_hash = stbg__get_widget_by_id_internal(context.current_widget_id).hash;

            BitConverter.TryWriteBytes(key, parent_hash);
            BitConverter.TryWriteBytes(key.Slice(sizeof(widget_hash)), parent_hash);
        }

        key[0] += (byte)type; //Include the type of as part of the key, so changing the type produces a different hash

        StbHash.stbh_halfsiphash(identifier, key, output);

        widget_hash outputHash = BitConverter.ToInt32(output);

        return outputHash;
    }

    private static long stbg__get_time_milliseconds()
    {
        return context.external_dependencies.get_time_milliseconds();
    }

    private static long stbg__get_performance_counter()
    {
        return context.external_dependencies.get_performance_counter();
    }

    private static long stbg__get_performance_counter_frequency()
    {
        return context.external_dependencies.get_performance_counter_frequency();
    }

    private static void stbg__update_average_performance_metrics()
    {
        if (context.time_between_frames_milliseconds == 0)
            return;

        int instant_fps = 1000 / (int)context.time_between_frames_milliseconds;
        int smoothing_factor = Math.Max(instant_fps / 2, 1);

        context.performance_metrics.process_input_time_us += (context.performance_metrics.process_input_time_us - context.frame_stats.performance.process_input_time_us) / smoothing_factor;
        context.performance_metrics.layout_widgets_time_us += (context.frame_stats.performance.layout_widgets_time_us - context.performance_metrics.layout_widgets_time_us) / smoothing_factor;
        context.performance_metrics.hash_time_us += (context.frame_stats.performance.hash_time_us - context.performance_metrics.hash_time_us) / smoothing_factor;

        if (context.init_options.force_always_render)
        {
            context.performance_metrics.render_time_us += (context.frame_stats.performance.render_time_us - context.performance_metrics.render_time_us) / smoothing_factor;
        }
        else
        {
            // Only update the render time if we are not skipping the render due to the same hash
            // We hardcode the smoothing factor used here to 2 so it converges faster when most of the frames are skipped
            int render_time_smoothing_factor = 2;
            if (!context.frame_stats.render_skipped_due_to_same_hash)
                context.performance_metrics.render_time_us += (context.frame_stats.performance.render_time_us - context.performance_metrics.render_time_us) / render_time_smoothing_factor;
        }
    }
}
