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

    // Taken from https://stackoverflow.com/questions/77212211/how-do-i-find-the-alignment-of-a-struct-in-c
    private struct AlignmentHelper<T> where T : unmanaged
    {
        #pragma warning disable CS0649
        public byte padding;
        public T target;
        #pragma warning restore CS0649
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int stbg__alignment_of<T>() where T : unmanaged
    {
        return (int)Marshal.OffsetOf<AlignmentHelper<T>>(nameof(AlignmentHelper<T>.target));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T stbg__add_custom_properties<T>(T properties, out Memory<byte> memory) where T : unmanaged
    {
        ref var pool = ref context.custom_properties_memory_pool;

        var alignment = stbg__alignment_of<T>();

        var size = Marshal.SizeOf<T>();

        var offset = pool.offset;

        // Align the offset to the size of the type
        offset += alignment - (offset % alignment);

        if (offset + size >= pool.memory_pool.Length)
        {
            context.frame_stats.custom_properties_memory_pool_overflowed_bytes += size;
            memory = new Memory<byte>(new byte[size]);
        }
        else
        {
            memory = pool.memory_pool.Slice(offset, size);
            pool.offset = offset + size;
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
