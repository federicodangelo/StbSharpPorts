#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

public partial class StbGui
{
    private static void stbg__string_memory_pool_init(ref stbg_string_memory_pool pool, int size)
    {
        stbg__assert(size >= 0);
        pool.memory_pool = new Memory<char>(new char[size]);
        pool.offset = 0;
    }

    private static void stbg__string_memory_pool_reset(ref stbg_string_memory_pool pool)
    {
        pool.offset = 0;
    }

    private static ReadOnlyMemory<char> stbg__add_string(ReadOnlySpan<char> str)
    {
        ref var pool = ref context.string_memory_pool;

        var length = str.Length;

        if (pool.offset + length >= pool.memory_pool.Length)
        {
            context.frame_stats.string_memory_pool_overflowed_characters += length;
            return str.ToArray().AsMemory();
        }

        var result = pool.memory_pool.Slice(pool.offset, length);
        str.CopyTo(result.Span);
        pool.offset += length;

        context.frame_stats.string_memory_pool_used_characters += length;

        return result;
    }
}
