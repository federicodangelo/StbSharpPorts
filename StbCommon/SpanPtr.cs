using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StbSharp.StbCommon;

public readonly ref struct SpanPtr<T>(Span<T> elements) where T : struct
{
    private readonly Span<T> elements = elements;

    public readonly bool IsNull => elements.IsEmpty;

    public readonly int Length => elements.Length;

    public void Fill(T value, int len)
    {
        elements.Slice(len).Fill(value);
    }

    public Span<T> Span => elements;

    public readonly ref T Ref
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref elements[0];
    }

    public readonly T Value { get => elements[0]; }

    public readonly ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref elements[index];
    }

    public SpanPtr(int size) : this(new T[size])
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpanPtr<T> operator +(SpanPtr<T> left, int offset)
    {
        return new SpanPtr<T>(left.elements.Slice(offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpanPtr<T> operator +(SpanPtr<T> left, uint offset)
    {
        return new SpanPtr<T>(left.elements.Slice((int)offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpanPtr<T> operator ++(SpanPtr<T> left)
    {
        return new SpanPtr<T>(left.elements.Slice(1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SpanPtr<T>(Memory<T> left)
    {
        return new SpanPtr<T>(left.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SpanPtr<T>(T[] left)
    {
        return new SpanPtr<T>(left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T>(SpanPtr<T> left)
    {
        return left.Span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SpanPtr<T>(Span<T> left)
    {
        return new SpanPtr<T>(left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SpanPtr<T>(BytePtr left)
    {
        return new SpanPtr<T>(MemoryMarshal.Cast<byte, T>(left.Span));
    }
}
