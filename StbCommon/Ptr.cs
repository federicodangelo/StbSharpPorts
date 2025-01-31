using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StbSharp.StbCommon;

public readonly struct Ptr<T>(Memory<T> elements)
{
    private readonly Memory<T> elements = elements;

    public readonly bool IsNull => elements.IsEmpty;

    public readonly int Length => elements.Length;

    public readonly Memory<T> Raw => elements;

    public void Fill(T value, int len)
    {
        elements.Span.Slice(len).Fill(value);
    }

    public Span<T> Span => elements.Span;

    public readonly ref T Ref { get => ref elements.Span[0]; }

    public readonly T Value { get => elements.Span[0]; } 

    public readonly Ptr<T> this[int index] { get => new(elements.Slice(index)); }

    static public Ptr<T> operator -(Ptr<T> left, int offset)
    {
        if (!MemoryMarshal.TryGetArray<T>(left.elements, out var segmentLeft))
        {
            Debug.Assert(false);
            return Null;
        }

        return new Ptr<T>(segmentLeft.Array.AsMemory().Slice(segmentLeft.Offset - offset));
    }

    static public Ptr<T> operator +(Ptr<T> left, int offset)
    {
        return new Ptr<T>(left.elements.Slice(offset));
    }

    static public Ptr<T> operator +(Ptr<T> left, uint offset)
    {
        return new Ptr<T>(left.elements.Slice((int) offset));
    }

    static public Ptr<T> operator ++(Ptr<T> left)
    {
        return new Ptr<T>(left.elements.Slice(1));
    }

    static public implicit operator Ptr<T>(Memory<T> left)
    {
        return new Ptr<T>(left);
    }

    static public implicit operator Ptr<T>(T[] left)
    {
        return new Ptr<T>(left);
    }
    
    static public implicit operator Span<T>(Ptr<T> left)
    {
        return left.Span;
    }

    static public readonly Ptr<T> Null = new(Memory<T>.Empty);

    public static bool operator <(Ptr<T> left, Ptr<T> right)
    {
        if (!MemoryMarshal.TryGetArray<T>(left.elements, out var segmentLeft) || 
            !MemoryMarshal.TryGetArray<T>(right.elements, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset < segmentRight.Offset;
    }

    public static bool operator >(Ptr<T> left, Ptr<T> right)
    {
        if (!MemoryMarshal.TryGetArray<T>(left.elements, out var segmentLeft) || 
            !MemoryMarshal.TryGetArray<T>(right.elements, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset > segmentRight.Offset;
    }

    public static bool operator <=(Ptr<T> left, Ptr<T> right)
    {
        if (!MemoryMarshal.TryGetArray<T>(left.elements, out var segmentLeft) || 
            !MemoryMarshal.TryGetArray<T>(right.elements, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset <= segmentRight.Offset;
    }

    public static bool operator >=(Ptr<T> left, Ptr<T> right)
    {
        if (!MemoryMarshal.TryGetArray<T>(left.elements, out var segmentLeft) || 
            !MemoryMarshal.TryGetArray<T>(right.elements, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset >= segmentRight.Offset;
    }
}
