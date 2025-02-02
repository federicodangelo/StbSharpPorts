using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StbSharp.StbCommon;

public readonly struct PtrConvert<T, T2>(Memory<T> elements) where T : struct where T2: struct
{
    private readonly Memory<T> elements = elements;

    public readonly bool IsNull => elements.IsEmpty;

    public readonly int Length => elements.Length;

    public readonly Memory<T> Raw => elements;

    public void Fill(T2 value, int len)
    {
        Span.Slice(len).Fill(value);
    }

    public Span<T2> Span => MemoryMarshal.Cast<T, T2>(elements.Span);

    public readonly ref T2 Ref
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Span[0];
    }

    public readonly T2 Value { get => Span[0]; }

    public readonly PtrConvert<T, T2> this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(elements.Slice(index * Marshal.SizeOf<T2>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public PtrConvert<T, T2> operator -(PtrConvert<T, T2> left, int offset)
    {
        if (!MemoryMarshal.TryGetArray<T>(left.elements, out var segmentLeft))
        {
            Debug.Assert(false);
            return Null;
        }

        return new PtrConvert<T, T2>(segmentLeft.Array.AsMemory().Slice(segmentLeft.Offset - offset * Marshal.SizeOf<T2>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public implicit operator PtrConvert<T,T2>(Ptr<T> left)
    {
        return new PtrConvert<T,T2>(left.Raw);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public PtrConvert<T,T2> operator +(PtrConvert<T,T2> left, int offset)
    {
        return new PtrConvert<T,T2>(left.elements.Slice(offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public PtrConvert<T,T2> operator +(PtrConvert<T,T2> left, uint offset)
    {
        return new PtrConvert<T,T2>(left.elements.Slice((int)offset* Marshal.SizeOf<T2>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public PtrConvert<T,T2> operator ++(PtrConvert<T,T2> left)
    {
        return new PtrConvert<T,T2>(left.elements.Slice(1* Marshal.SizeOf<T2>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public implicit operator PtrConvert<T,T2>(Memory<T> left)
    {
        return new PtrConvert<T,T2>(left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public implicit operator PtrConvert<T,T2>(T[] left)
    {
        return new PtrConvert<T,T2>(left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public implicit operator Span<T2>(PtrConvert<T,T2> left)
    {
        return left.Span;
    }

    static public readonly PtrConvert<T,T2> Null = new(Memory<T>.Empty);

}

