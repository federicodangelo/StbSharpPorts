using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StbSharp.StbCommon;

public readonly struct BytePtrConvert<T2>(Memory<byte> elements) where T2 : struct
{
    private readonly Memory<byte> elements = elements;

    public readonly bool IsNull => elements.IsEmpty;

    public readonly int Length => elements.Length;

    public readonly Memory<byte> Raw => elements;

    public void Fill(T2 value, int len)
    {
        Span.Slice(len).Fill(value);
    }

    public Span<T2> Span => MemoryMarshal.Cast<byte, T2>(elements.Span);

    public readonly ref T2 Ref
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Span[0];
    }

    public readonly T2 Value { get => Span[0]; }

    public readonly BytePtrConvert<T2> this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(elements.Slice(index * Marshal.SizeOf<T2>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BytePtrConvert<T2> operator -(BytePtrConvert<T2> left, int offset)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.elements, out var segmentLeft))
        {
            Debug.Assert(false);
            return Null;
        }

        return new BytePtrConvert<T2>(segmentLeft.Array.AsMemory().Slice(segmentLeft.Offset - offset * Marshal.SizeOf<T2>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BytePtrConvert<T2>(BytePtr left)
    {
        return new BytePtrConvert<T2>(left.Raw);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BytePtrConvert<T2> operator +(BytePtrConvert<T2> left, int offset)
    {
        return new BytePtrConvert<T2>(left.elements.Slice(offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BytePtrConvert<T2> operator +(BytePtrConvert<T2> left, uint offset)
    {
        return new BytePtrConvert<T2>(left.elements.Slice((int)offset * Marshal.SizeOf<T2>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BytePtrConvert<T2> operator ++(BytePtrConvert<T2> left)
    {
        return new BytePtrConvert<T2>(left.elements.Slice(1 * Marshal.SizeOf<T2>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BytePtrConvert<T2>(Memory<byte> left)
    {
        return new BytePtrConvert<T2>(left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BytePtrConvert<T2>(byte[] left)
    {
        return new BytePtrConvert<T2>(left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T2>(BytePtrConvert<T2> left)
    {
        return left.Span;
    }

    public static readonly BytePtrConvert<T2> Null = new(Memory<byte>.Empty);

}
