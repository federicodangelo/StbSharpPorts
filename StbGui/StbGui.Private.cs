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

    private static void stbg__layout_widgets()
    {
        // Ideas from:
        // - Originally using https://www.rfleury.com/p/ui-part-2-build-it-every-frame-immediate but it was waaay to complex
        // - Switched to https://docs.flutter.dev/ui/layout/constraints (let's see how it goes..)

        // - Constrains go down
        // - Sizes go up
        // - Parent sets position

        stbg_widget_constrains constrains = stbg_build_constrains_unconstrained();

        stbg__layout_widget(constrains, ref stbg_get_widget_by_id(context.root_widget_id));

        stbg__update_global_rect(ref stbg_get_widget_by_id(context.root_widget_id), new stbg_rect());

    }

    private static void stbg__update_global_rect(ref stbg_widget widget, stbg_rect parent_global_rect)
    {
        // Update self bounds
        ref var widget_computed_bounds = ref widget.computed_bounds;
        ref var widget_global_rect = ref widget.computed_bounds.global_rect;
        
        widget_global_rect = stbg_build_rect(
            widget_computed_bounds.position.x, widget_computed_bounds.position.y, 
            widget_computed_bounds.position.x + widget_computed_bounds.size.width, widget_computed_bounds.position.y + widget_computed_bounds.size.height);

        widget_global_rect = stbg_translate_rect(widget_global_rect, parent_global_rect.x0, parent_global_rect.y0);

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
        var widget_constrains = widget_layout.constrains;
        var widget_inner_padding = widget.layout.inner_padding;

        // Build current contrains by merging the widget constrains with the parent constrains
        var constrains = stbg_build_constrains(
                Math.Max(parent_constrains.min.width, widget_constrains.min.width),
                Math.Max(parent_constrains.min.height, widget_constrains.min.height),
                Math.Min(parent_constrains.max.width, widget_constrains.max.width),
                Math.Min(parent_constrains.max.height, widget_constrains.max.height)
        );

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
            var widget_children_spacing = widget.layout.children_spacing;
            var widget_children_layout_direction = widget.layout.children_layout_direction;

            // Build children constrains by removing padding
            var children_constrains = stbg_constrains_remove_padding(constrains, widget_inner_padding);

            // Iterate children
            var children_id = widget.hierarchy.first_children_id;

            // TODO:
            // - add "expand_width" and "expand_height" properties, used to resize children to fill all available space 
            //   (they would modify min.width and min.height to match the expected size)

            var next_children_top_left = stbg_build_position(widget_inner_padding.left, widget_inner_padding.top);

            var first_chilren = true;

            do
            {
                ref var children = ref stbg_get_widget_by_id(children_id);

                if (!first_chilren && widget_children_spacing != 0)
                {
                    switch (widget_children_layout_direction)
                    {
                        case STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL:
                            {
                                var available_spacing = Math.Min(widget_children_spacing, children_constrains.max.height);
                                next_children_top_left.y += available_spacing;
                                children_constrains.max.height -= available_spacing;
                                accumulated_children_size.height += available_spacing;
                                break;
                            }

                        case STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL:
                            {
                                var available_spacing = Math.Min(widget_children_spacing, children_constrains.max.width);
                                next_children_top_left.x += available_spacing;
                                children_constrains.max.width -= available_spacing;
                                accumulated_children_size.width += available_spacing;
                                break;
                            }
                    }
                }

                var children_size = stbg__layout_widget(children_constrains, ref children);

                stbg__assert_internal(children_size.width <= children_constrains.max.width);
                stbg__assert_internal(children_size.width >= children_constrains.min.width);
                stbg__assert_internal(children_size.height <= children_constrains.max.height);
                stbg__assert_internal(children_size.height >= children_constrains.min.height);

                children.computed_bounds.size = children_size;

                switch (widget_children_layout_direction)
                {
                    case STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL:
                        children.computed_bounds.position = next_children_top_left;
                        children_constrains.max.height -= children_size.height;
                        children_constrains.min.height = Math.Min(children_constrains.min.height, children_constrains.max.height);
                        accumulated_children_size.height += children_size.height;
                        accumulated_children_size.width = Math.Max(accumulated_children_size.width, children_size.width);
                        next_children_top_left.y += children_size.height;
                        break;

                    case STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL:
                        children.computed_bounds.position = next_children_top_left;
                        children_constrains.max.width -= children_size.height;
                        children_constrains.min.width = Math.Min(children_constrains.min.width, children_constrains.max.width);
                        accumulated_children_size.width += children_size.width;
                        accumulated_children_size.height = Math.Max(accumulated_children_size.height, children_size.height);
                        next_children_top_left.x += children_size.width;
                        break;

                    case STBG_CHILDREN_LAYOUT_DIRECTION.FREE:
                        children.computed_bounds.position.x =
                            next_children_top_left.x + Math.Min(children.layout.intrinsic_position.x, children_constrains.max.width - children_size.width);
                        children.computed_bounds.position.y =
                            next_children_top_left.y + Math.Min(children.layout.intrinsic_position.y, children_constrains.max.height - children_size.height);
                        break;
                }

                children_id = children.hierarchy.next_sibling_id;
                first_chilren = false;
            } while (children_id != STBG_WIDGET_ID_NULL);
        }

        stbg_size widget_size = new stbg_size()
        {
            width = Math.Max(
                Math.Min(
                    Math.Max(intrinsic_size.width, accumulated_children_size.width) + widget_inner_padding.right + widget_inner_padding.left,
                    constrains.max.width),
                constrains.min.width),
            height = Math.Max(
                Math.Min(
                    Math.Max(intrinsic_size.height, accumulated_children_size.height) + widget_inner_padding.top + widget_inner_padding.bottom,
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
