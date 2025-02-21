#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;
using System.Runtime.InteropServices;

public partial class StbGui
{
    private static stbg_context context;

    private static void stbg_init_context(ref stbg_context context, stbg_external_dependencies external_dependencies, stbg_init_options options)
    {
        stbg__assert(external_dependencies.measure_text != null, "Missing measure text function");
        stbg__assert(external_dependencies.render != null, "Missing render function");

        if (options.max_widgets == 0)
            options.max_widgets = DEFAULT_MAX_WIDGETS;

        if (options.hash_table_size == 0)
            options.hash_table_size = options.max_widgets;

        if (options.max_fonts == 0)
            options.max_fonts = DEFAULT_MAX_FONTS;

        if (options.string_memory_pool_size == 0)
            options.string_memory_pool_size = DEFAULT_STRING_MEMORY_POOL_SIZE;

        if (options.render_commands_queue_size == 0)
            options.render_commands_queue_size = DEFAULT_RENDER_QUEUE_SIZE;

        var widgets = new stbg_widget[options.max_widgets + 1]; // first slot is never used (null widget)
        var hash_table = new stbg_hash_entry[options.hash_table_size];
        var fonts = new stbg_font[options.max_fonts + 1]; // first slot is never used (null font)
        var render_commands_queue = new stbg_render_command[options.render_commands_queue_size];

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
        context.hash_table = hash_table;
        context.fonts = fonts;
        context.first_free_font_id = 1;
        context.init_options = options;
        context.external_dependencies = external_dependencies;
        context.theme.styles = new double[(int)STBG_WIDGET_STYLE.COUNT];
        context.render_context.render_commands_queue = render_commands_queue;
        stbg__string_memory_pool_init(ref context.string_memory_pool, options.string_memory_pool_size);
    }

    private static stbg_size stbg__measure_text(stbg_text text)
    {
        return context.external_dependencies.measure_text(text.text.Span, stbg_get_font_by_id(text.font_id), text.style);
    }

    private static void stbg__destroy_unused_widgets(int amount_to_destroy)
    {
        var widgets = context.widgets;

        for (int i = 0; i < widgets.Length && amount_to_destroy > 0; i++)
        {
            if ((widgets[i].flags & STBG_WIDGET_FLAGS.USED) != 0 && widgets[i].last_used_in_frame != context.current_frame)
            {
                stbg__destroy_widget(ref widgets[i]);
                amount_to_destroy--;
            }
        }

        stbg__assert_internal(amount_to_destroy == 0);
    }

    private const string DEBUG_WINDOW_TITLE = "_DEBUG_";
    private static widget_hash debug_window_hash = stbg__calculate_hash(STBG_WIDGET_TYPE.WINDOW, DEBUG_WINDOW_TITLE, true);

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
            stbg__window_init(ref debug_window, is_new, DEBUG_WINDOW_TITLE);

        return ref debug_window;
    }

    private static ref stbg_widget stbg__add_widget(STBG_WIDGET_TYPE type, ReadOnlySpan<char> identifier, out bool is_new)
    {
        ref var widget = ref stbg__add_widget(stbg__calculate_hash(type, identifier), type, context.current_widget_id, out is_new, out _);
        context.last_widget_id = widget.id;
        context.last_widget_is_new = is_new;

        return ref widget;
    }

    private static ref stbg_widget stbg__add_widget(widget_hash hash, STBG_WIDGET_TYPE type, widget_id parent_id, out bool is_new, out bool is_already_created_in_same_frame, STBG__WIDGET_ADD_OPTIONS options = STBG__WIDGET_ADD_OPTIONS.NONE)
    {
        stbg__assert(context.inside_frame);
        stbg__assert(context.first_free_widget_id != STBG_WIDGET_ID_NULL, "No more room for widgets");

        ref var widget =
            ref (stbg__find_widget_by_hash(hash, out var existingWidgetId) ?
                ref stbg__get_widget_by_id_internal(existingWidgetId) :
                ref stbg__get_widget_by_id_internal(context.first_free_widget_id));

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
                stbg_get_widget_by_id(bucket.first_widget_in_bucket).hash_chain.prev_same_bucket = widget.id;
                widget.hash_chain.next_same_bucket = bucket.first_widget_in_bucket;
            }
            bucket.first_widget_in_bucket = widget.id;

            // Reset dynamic properties
            widget.properties = new stbg_widget_properties();

            // Reset hierarchy
            widget.hierarchy = new stbg_widget_hierarchy();

            // Set type
            widget.type = type;
        }
        else
        {
            // Reused widget!
            is_new = false;
            is_already_created_in_same_frame = widget.last_used_in_frame == context.current_frame;
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
            stbg__assert(widget.last_used_in_frame != context.current_frame, "Duplicated widget identifier!!");
        }
        widget.last_used_in_frame = context.current_frame;

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

        ref var parent = ref stbg_get_widget_by_id(widget.hierarchy.parent_id);

        // Update parent's first / last widget id if it matches the widget to remove
        if (parent.hierarchy.first_children_id == widget.id)
            parent.hierarchy.first_children_id = widget.hierarchy.next_sibling_id;

        if (parent.hierarchy.last_children_id == widget.id)
            parent.hierarchy.last_children_id = widget.hierarchy.prev_sibling_id;

        // If there is a next widget, update it to point to our previous widget
        if (widget.hierarchy.next_sibling_id != STBG_WIDGET_ID_NULL)
            stbg_get_widget_by_id(widget.hierarchy.next_sibling_id).hierarchy.prev_sibling_id = widget.hierarchy.prev_sibling_id;

        // If there is a previous widget, update it to point to our next widget
        if (widget.hierarchy.prev_sibling_id != STBG_WIDGET_ID_NULL)
            stbg_get_widget_by_id(widget.hierarchy.prev_sibling_id).hierarchy.next_sibling_id = widget.hierarchy.next_sibling_id;

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

        ref var parent = ref stbg_get_widget_by_id(parent_id);

        widget.hierarchy.parent_id = parent_id;

        // - Update the list of children of our parent
        // - Update the list of siblings of any children
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

    private static void stbg__add_widget_to_parent_after_sibling_or_first(ref stbg_widget widget, widget_id prev_sibling_id, widget_id parent_id)
    {
        stbg__assert_internal(widget.hierarchy.parent_id == STBG_WIDGET_ID_NULL);
        stbg__assert_internal(parent_id != STBG_WIDGET_ID_NULL);

        ref var parent = ref stbg_get_widget_by_id(parent_id);

        widget.hierarchy.parent_id = parent_id;

        if (prev_sibling_id == STBG_WIDGET_ID_NULL)
        {
            // No previous sibling, add as first children of parent
            widget.hierarchy.next_sibling_id = parent.hierarchy.first_children_id;
            parent.hierarchy.first_children_id = widget.id;

            // Update next sibling to point back to us
            if (widget.hierarchy.next_sibling_id != STBG_WIDGET_ID_NULL)
            {
                stbg_get_widget_by_id(widget.hierarchy.next_sibling_id).hierarchy.prev_sibling_id = widget.id;
            }
            else // There is no next sibling, so we are the only children of our parent
            {
                parent.hierarchy.last_children_id = widget.id;
            }
        }
        else
        {
            // Insert ourselves between our previous sibling and it's next sibling
            ref var prev_sibling = ref stbg_get_widget_by_id(prev_sibling_id);
            stbg__assert_internal(prev_sibling.hierarchy.parent_id == parent_id);

            widget.hierarchy.next_sibling_id = prev_sibling.hierarchy.next_sibling_id;
            widget.hierarchy.prev_sibling_id = prev_sibling.id;
            prev_sibling.hierarchy.next_sibling_id = widget.id;
        }

        // Update next sibling to point back to us
        if (widget.hierarchy.next_sibling_id != STBG_WIDGET_ID_NULL)
        {
            stbg_get_widget_by_id(widget.hierarchy.next_sibling_id).hierarchy.prev_sibling_id = widget.id;
        }
        else // There is no next sibling, so we are the last children of our parent
        {
            parent.hierarchy.last_children_id = widget.id;
        }
    }

    private static void stbg__destroy_widget(ref stbg_widget widget)
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

        // Remove widget from input feedback
        if (context.input_feedback.dragged_widget_id == widget.id)
            context.input_feedback.dragged_widget_id = STBG_WIDGET_ID_NULL;
        if (context.input_feedback.hovered_widget_id == widget.id)
            context.input_feedback.hovered_widget_id = STBG_WIDGET_ID_NULL;
        if (context.input_feedback.pressed_widget_id == widget.id)
            context.input_feedback.pressed_widget_id = STBG_WIDGET_ID_NULL;
        if (context.input_feedback.active_widget_id == widget.id)
            context.input_feedback.active_widget_id = STBG_WIDGET_ID_NULL;
    }

    private static bool stbg__find_widget_parent_by_type(widget_id widget_id, STBG_WIDGET_TYPE type, out widget_id found_id)
    {
        while (widget_id != STBG_WIDGET_ID_NULL)
        {
            ref var widget = ref stbg_get_widget_by_id(widget_id);

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

    private static ref stbg_widget stbg__get_widget_by_id_internal(widget_id id)
    {
        stbg__assert(id != STBG_WIDGET_ID_NULL);
        return ref context.widgets[id];
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
            var parent_hash = stbg_get_widget_by_id(context.current_widget_id).hash;

            BitConverter.TryWriteBytes(key, parent_hash);
            BitConverter.TryWriteBytes(key.Slice(sizeof(widget_hash)), parent_hash);
        }

        key[0] += (byte)type; //Include the type of as part of the key, so changing the type produces a different hash

        StbHash.stbh_halfsiphash(identifier, key, output);

        widget_hash outputHash = BitConverter.ToInt32(output);

        return outputHash;
    }
}
