using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StbSharp.StbCommon;

public readonly struct BytePtr(Memory<byte> bytes)
{
    private readonly Memory<byte> bytes = bytes;

    public readonly bool IsNull => bytes.IsEmpty;

    public readonly int Length => bytes.Length;

    public readonly Memory<byte> Raw => bytes;

    public readonly byte[]? Array => MemoryMarshal.TryGetArray<byte>(bytes, out var segment) ? segment.Array : null;

    public Span<byte> Span => bytes.Span;

    public int Offset => MemoryMarshal.TryGetArray<byte>(bytes, out var segment) ? segment.Offset : 0;

    public readonly ref byte Ref { get => ref bytes.Span[0]; }

    public readonly byte Value { get => bytes.Span[0]; }

    public readonly BytePtr this[int index]
    {
        get
        {
            if (index >= 0)
            {
                return new(bytes.Slice(index));
            }
            else
            {
                // Slice doesn't support negative numbers, so we get the original array and offset from the start again
                Debug.Assert(MemoryMarshal.TryGetArray<byte>(bytes, out var segment));
                return new BytePtr(segment.Array.AsMemory().Slice(segment.Offset + index));
            }
        }
    }

    public BytePtr(int size) : this(new byte[size])
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(byte value, int len)
    {
        bytes.Span.Slice(len).Fill(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int FirstIndexOf(byte value)
    {
        var span = bytes.Span;

        for (int i = 0; i < span.Length; i++)
            if (span[i] == value)
                return i;

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public BytePtr operator -(BytePtr left, int offset)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft))
        {
            Debug.Assert(false);
            return Null;
        }

        return new BytePtr(segmentLeft.Array.AsMemory().Slice(segmentLeft.Offset - offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BytePtr operator -(BytePtr left, BytePtr right)
    {
        if (MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft) &&
            MemoryMarshal.TryGetArray<byte>(right.bytes, out var segmentRight))
        {
            Debug.Assert(segmentLeft.Array == segmentRight.Array);
            return new BytePtr(segmentLeft.Array.AsMemory().Slice(segmentLeft.Offset - segmentRight.Offset));
        }

        return BytePtr.Null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public BytePtr operator +(BytePtr left, int offset)
    {
        return new BytePtr(left.bytes.Slice(offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public BytePtr operator +(BytePtr left, uint offset)
    {
        return new BytePtr(left.bytes.Slice((int)offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public BytePtr operator ++(BytePtr left)
    {
        return new BytePtr(left.bytes.Slice(1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public implicit operator BytePtr(Memory<byte> left)
    {
        return new BytePtr(left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public implicit operator BytePtr(byte[] left)
    {
        return new BytePtr(left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public implicit operator Span<byte>(BytePtr left)
    {
        return left.Span;
    }

    static public readonly BytePtr Null = new(Memory<byte>.Empty);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BytePtr left, BytePtr right)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft) ||
            !MemoryMarshal.TryGetArray<byte>(right.bytes, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset == segmentRight.Offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BytePtr left, BytePtr right)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft) ||
            !MemoryMarshal.TryGetArray<byte>(right.bytes, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        return (segmentLeft.Array != segmentRight.Array) ||
                (segmentLeft.Offset != segmentRight.Offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BytePtr left, BytePtr right)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft) ||
            !MemoryMarshal.TryGetArray<byte>(right.bytes, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset < segmentRight.Offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(BytePtr left, BytePtr right)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft) ||
            !MemoryMarshal.TryGetArray<byte>(right.bytes, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset > segmentRight.Offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BytePtr left, BytePtr right)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft) ||
            !MemoryMarshal.TryGetArray<byte>(right.bytes, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset <= segmentRight.Offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BytePtr left, BytePtr right)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft) ||
            !MemoryMarshal.TryGetArray<byte>(right.bytes, out var segmentRight))
        {
            Debug.Assert(false);
            return false;
        }
        Debug.Assert(segmentLeft.Array == segmentRight.Array);
        return segmentLeft.Offset >= segmentRight.Offset;
    }

    public override bool Equals(object? obj)
    {
        return (obj is BytePtr ptr) ? ptr == this : false;
    }

    public override int GetHashCode()
    {
        return bytes.GetHashCode();
    }
}
