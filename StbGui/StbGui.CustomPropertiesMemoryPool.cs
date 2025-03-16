#pragma warning disable IDE1006 // Naming Styles

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StbSharp;

public partial class StbGui
{
    private static void stbg__custom_properties_memory_pool_init(ref stbg_custom_properties_memory_pool pool, int size)
    {
        stbg__assert(size >= 0);
        pool.memory_pool = new Memory<byte>(new byte[size]);
        pool.offset = 0;
    }

    private static void stbg__custom_properties_memory_pool_reset(ref stbg_custom_properties_memory_pool pool)
    {
        pool.offset = 0;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T stbg__add_custom_properties<T>(T properties, out Memory<byte> memory) where T : unmanaged
    {
        ref var pool = ref context.custom_properties_memory_pool;

        var info = stbg__get_marshal_info<T>();

        var offset = pool.offset;

        // Align the offset to the size of the type
        offset += info.alignment - (offset % info.alignment);

        if (offset + info.size >= pool.memory_pool.Length)
        {
            context.frame_stats.custom_properties_memory_pool_overflowed_bytes += info.size;
            memory = new Memory<byte>(new byte[info.size]);
        }
        else
        {
            memory = pool.memory_pool.Slice(offset, info.size);
            pool.offset = offset + info.size;
            context.frame_stats.custom_properties_memory_pool_used_bytes = pool.offset;
        }

        ref var result = ref MemoryMarshal.AsRef<T>(memory.Span);
        result = properties;
        return ref result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T stbg__get_custom_properties<T>(Memory<byte> memory) where T : unmanaged
    {
        return ref MemoryMarshal.AsRef<T>(memory.Span);
    }
}
