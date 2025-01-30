using System.Diagnostics;

namespace StbSharp.StbCommon;

public readonly struct BytePtr(byte[] bytes, int offset = 0)
{
    private readonly byte[] bytes = bytes;
    private readonly int offset = offset;

    public readonly bool IsNull => bytes == null || bytes.Length == 0;

    public readonly int Length => Math.Max(bytes.Length - offset, 0);

    public readonly byte[] Raw => bytes;

    public readonly int RawOffset => offset; 

    public void Fill(byte value, int len)
    {
        Array.Fill(bytes, value, offset, len);
    }

    public int FirstIndexOf(byte value)
    {
        for (int i = offset; i < bytes.Length; i++)
            if (bytes[i] == value)
                return i - offset;

        return 0;
    }

    public Span<byte> AsSpan() => new Span<byte>(bytes, offset, Length);

    public readonly BytePtr this[int index] { get => new(bytes, offset + index); }

    // Defining this operator broke tests :-(    
    /*static public BytePtr operator -(BytePtr left, int offset)
    {
        return new BytePtr(left.bytes, left.offset - offset);
    }*/

    static public BytePtr operator +(BytePtr left, int offset)
    {
        return new BytePtr(left.bytes, left.offset + offset);
    }

    static public BytePtr operator +(BytePtr left, uint offset)
    {
        return new BytePtr(left.bytes, (int)(left.offset + offset));
    }

    static public BytePtr operator ++(BytePtr left)
    {
        return new BytePtr(left.bytes, left.offset + 1);
    }

    static public implicit operator BytePtr(byte[] left)
    {
        return new BytePtr(left, 0);
    }

    static public implicit operator byte(BytePtr left)
    {
        return left.bytes[left.offset];
    }

    static public implicit operator Span<byte>(BytePtr left)
    {
        return left.AsSpan();
    }

    public readonly ref byte GetRef(int index) => ref bytes[offset + index];

    public readonly ref byte GetRef() => ref bytes[offset];

    static public readonly BytePtr Null = new([], 0);

    public static bool operator <(BytePtr left, BytePtr right)
    {
        Debug.Assert(left.bytes == right.bytes);
        return left.offset < right.offset; ;
    }

    public static bool operator >(BytePtr left, BytePtr right)
    {
        Debug.Assert(left.bytes == right.bytes);
        return left.offset > right.offset; ;
    }

    public static bool operator <=(BytePtr left, BytePtr right)
    {
        Debug.Assert(left.bytes == right.bytes);
        return left.offset <= right.offset; ;
    }

    public static bool operator >=(BytePtr left, BytePtr right)
    {
        Debug.Assert(left.bytes == right.bytes);
        return left.offset >= right.offset; ;
    }
}

public readonly struct Ptr<T>(T[] elements, int offset = 0)
{
    private readonly T[] elements = elements;
    private readonly int offset = offset;

    public readonly T[] Raw => elements;
    public readonly int RawOffset => offset;

    public readonly bool IsNull => elements == null || elements.Length == 0;

    public readonly int Length => Math.Max((elements != null ? elements.Length : 0) - offset, 0);

    public readonly Ptr<T> this[int index] { get => new(elements, offset + index); }

    public Span<T> AsSpan() => new Span<T>(elements, offset, Length);

    static public Ptr<T> operator +(Ptr<T> left, int offset)
    {
        return new Ptr<T>(left.elements, left.offset + offset);
    }

    static public Ptr<T> operator ++(Ptr<T> left)
    {
        return new Ptr<T>(left.elements, left.offset + 1);
    }

    static public implicit operator Ptr<T>(T[] left)
    {
        return new Ptr<T>(left, 0);
    }

    static public implicit operator T(Ptr<T> left)
    {
        return left.elements[left.offset];
    }

    public readonly ref T GetRef(int index) => ref elements[offset + index];

    public readonly ref T GetRef() => ref elements[offset];

    static public readonly Ptr<T> Null = new([], 0);
}
