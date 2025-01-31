using System.Diagnostics;
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

    public readonly BytePtr this[int index] { get => new(bytes.Slice(index)); }

    public BytePtr(int size) : this(new byte[size])
    {
    }

    public void Fill(byte value, int len)
    {
        bytes.Span.Slice(len).Fill(value);
    }

    public int FirstIndexOf(byte value)
    {
        var span = bytes.Span;

        for (int i = 0; i < span.Length; i++)
            if (span[i] == value)
                return i;

        return 0;
    }

 
    static public BytePtr operator -(BytePtr left, int offset)
    {
        if (!MemoryMarshal.TryGetArray<byte>(left.bytes, out var segmentLeft))
        {
            Debug.Assert(false);
            return Null;
        }

        return new BytePtr(segmentLeft.Array.AsMemory().Slice(segmentLeft.Offset - offset));
    }

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

    static public BytePtr operator +(BytePtr left, int offset)
    {
        return new BytePtr(left.bytes.Slice(offset));
    }

    static public BytePtr operator +(BytePtr left, uint offset)
    {
        return new BytePtr(left.bytes.Slice((int) offset));
    }

    static public BytePtr operator ++(BytePtr left)
    {
        return new BytePtr(left.bytes.Slice(1));
    }

    static public implicit operator BytePtr(Memory<byte> left)
    {
        return new BytePtr(left);
    }

    static public implicit operator BytePtr(byte[] left)
    {
        return new BytePtr(left);
    }
    
    static public implicit operator Span<byte>(BytePtr left)
    {
        return left.Span;
    }

    static public readonly BytePtr Null = new(Memory<byte>.Empty);

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
}
