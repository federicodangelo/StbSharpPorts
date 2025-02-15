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
        context.render_commands_queue = render_commands_queue;
    }

    private static stbg_size stbg_measure_text(stbg_text text)
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
                stbg__remove_widget(ref widgets[i]);
                amount_to_destroy--;
            }
        }

        stbg__assert_internal(amount_to_destroy == 0);
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
        stbg__assert(context.first_free_widget_id != STBG_WIDGET_ID_NULL, "No more room for widgets");

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
