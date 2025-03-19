#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using System.Runtime.InteropServices;

using font_id = int;
using widget_hash = int;
using widget_id = int;

public partial class StbGui
{

    private static ref stbg_hash_bucket stbg__get_hash_bucket_by_hash(widget_hash hash)
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

    private static long stbg__get_render_hash()
    {
        var start_hash_time = stbg__get_performance_counter();

        long hash = 0x1234567890123456L;
        hash = stbg__hash_widgets(hash);
        hash = stbg__hash_input(hash);
        hash = stbg__hash_text_edit(hash);

        context.frame_stats.performance.hash_time_us = ((stbg__get_performance_counter() - start_hash_time) * MICROSECONDS) / stbg__get_performance_counter_frequency();

        return hash;
    }

    private static long stbg__hash_text_edit(long hash)
    {
        if (context.text_edit.widget_id == STBG_WIDGET_ID_NULL)
            return hash;

        hash = StbHash.stbh_halfsiphash_long(context.text_edit.state.cursor, hash);
        hash = StbHash.stbh_halfsiphash_long(context.text_edit.state.select_start, hash);
        hash = StbHash.stbh_halfsiphash_long(context.text_edit.state.select_end, hash);
        hash = StbHash.stbh_halfsiphash_long(MemoryMarshal.AsBytes(context.text_edit.str.text.Span.Slice(0, context.text_edit.str.text_length)), hash);

        return hash;
    }

    private static long stbg__hash_input(long key)
    {
        ReadOnlySpan<byte> input_bytes = MemoryMarshal.AsBytes(new Span<stbg_context_input_feedback>(ref context.input_feedback));

        var hash = StbHash.stbh_halfsiphash_long(input_bytes, key);

        return hash;
    }

    private static long stbg__hash_widgets(long previous_hash)
    {
        ReadOnlySpan<byte> widgets_bytes = MemoryMarshal.AsBytes(context.widgets.AsSpan());

        var hash = stbg__hash_widget(context.root_widget_id, widgets_bytes, previous_hash);

        return hash;
    }

    private static long stbg__hash_widget(int widget_id, ReadOnlySpan<byte> widgets_bytes, long previous_hash)
    {
        ref var widget = ref stbg__get_widget_by_id_internal(widget_id);
        if ((widget.flags & STBG_WIDGET_FLAGS.IGNORE) != 0)
            return previous_hash;

        var widget_offset = widget_id * stbg__widget_marshal_info.size;
        var widget_bytes = widgets_bytes.Slice(widget_offset, stbg__widget_marshal_info.size);

        var hash = StbHash.stbh_halfsiphash_long(widget_bytes, previous_hash);
        hash = stbg__hash_widget_ref_props(widget_id, hash);

        if (widget.hierarchy.first_children_id != STBG_WIDGET_ID_NULL)
        {
            var children_id = widget.hierarchy.first_children_id;

            do
            {
                hash = stbg__hash_widget(children_id, widgets_bytes, hash);
                children_id = stbg_get_widget_by_id(children_id).hierarchy.next_sibling_id;
            } while (children_id != STBG_WIDGET_ID_NULL);
        }

        return hash;
    }

    private static long stbg__hash_widget_ref_props(int widget_id, long hash)
    {
        ref var widget_ref_props = ref stbg__get_widget_ref_props_by_id_internal(widget_id);

        if (widget_ref_props.text.Length > 0)
        {
            ReadOnlySpan<byte> text_bytes = MemoryMarshal.AsBytes(widget_ref_props.text.Span);
            hash = StbHash.stbh_halfsiphash_long(text_bytes, hash);
        }

        if (widget_ref_props.text_to_edit.length > 0)
        {
            ReadOnlySpan<byte> text_editable_bytes = MemoryMarshal.AsBytes(widget_ref_props.text_to_edit.text.Span.Slice(0, widget_ref_props.text_to_edit.length));
            hash = StbHash.stbh_halfsiphash_long(text_editable_bytes, hash);
        }

        if (widget_ref_props.custom.Length > 0)
        {
            ReadOnlySpan<byte> custom_properties_bytes = MemoryMarshal.AsBytes(widget_ref_props.custom.Span);
            hash = StbHash.stbh_halfsiphash_long(custom_properties_bytes, hash);
        }

        return hash;
    }
}
