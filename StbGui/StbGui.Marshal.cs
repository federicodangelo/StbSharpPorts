#pragma warning disable IDE1006 // Naming Styles

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StbSharp;

public partial class StbGui
{
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

    // We cache these values becase for some reason Marshal.OffsetOf() and Marshal.SizeOf() allocate memory in desktop AOT builds (but not in WebAssembly AOT ones..)
    private struct stbg__marshal_info
    {
        public int size;
        public int alignment;
    }

    private static Dictionary<Type, stbg__marshal_info> stbg__marshal_infos = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static stbg__marshal_info stbg__get_marshal_info<T>() where T : unmanaged
    {
        if (!stbg__marshal_infos.TryGetValue(typeof(T), out var info))
        {
            info.size = Marshal.SizeOf<T>();
            info.alignment = stbg__alignment_of<T>();
            stbg__marshal_infos[typeof(T)] = info;
        }

        return info;
    }
}
