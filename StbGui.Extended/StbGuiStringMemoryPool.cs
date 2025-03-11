namespace StbSharp;

using System.Diagnostics;

public readonly ref struct StbGuiString
{
    private readonly ReadOnlySpan<char> chars;
    private readonly StbGuiStringMemoryPool pool;

    public StbGuiString(StbGuiStringMemoryPool pool, ReadOnlySpan<char> chars)
    {
        this.pool = pool;
        this.chars = chars;
    }

    static public StbGuiString operator +(StbGuiString str1, ReadOnlySpan<char> chars)
    {
        return str1.pool.Concat(str1.chars, chars);
    }

    static public StbGuiString operator +(StbGuiString str1, int val)
    {
        return str1.pool.Concat(str1.chars, val);
    }

    static public StbGuiString operator +(StbGuiString str1, long val)
    {
        return str1.pool.Concat(str1.chars, val);
    }

    static public StbGuiString operator +(StbGuiString str1, float val)
    {
        return str1.pool.Concat(str1.chars, val);
    }

    static public StbGuiString operator +(StbGuiString str1, double val)
    {
        return str1.pool.Concat(str1.chars, val);
    }

    static public StbGuiString operator +(StbGuiString str1, bool val)
    {
        return str1.pool.Concat(str1.chars, val);
    }

    static public implicit operator ReadOnlySpan<char>(StbGuiString str)
    {
        return str.chars;
    }

    public StbGuiString Append(ReadOnlySpan<char> chars)
    {
        return pool.Concat(chars, chars);
    }

    public StbGuiString Append(int val)
    {
        return pool.Concat(chars, val);
    }
    public StbGuiString Append(long val)
    {
        return pool.Concat(chars, val);
    }
    public StbGuiString Append(float val)
    {
        return pool.Concat(chars, val);
    }
    public StbGuiString Append(double val)
    {
        return pool.Concat(chars, val);
    }
    public StbGuiString Append(bool val)
    {
        return pool.Concat(chars, val);
    }

    public StbGuiString Append(float val, string format)
    {
        return pool.Concat(chars, val, format);
    }

    public StbGuiString Append(float val, int decimals)
    {
        return pool.Concat(chars, val, decimals);
    }
}

public class StbGuiStringMemoryPool
{
    private readonly Memory<char> memoryPool;
    private int memoryPoolOffset = 0;

    public StbGuiStringMemoryPool(int size = 1024 * 1024)
    {
        Debug.Assert(size >= 0);
        memoryPool = new Memory<char>(new char[size]);
    }

    public StbGuiString Build(ReadOnlySpan<char> str)
    {
        int length = str.Length;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str.CopyTo(result.Span);
        memoryPoolOffset += length;
        return new StbGuiString(this, result.Span);
    }

    public StbGuiString Concat(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
    {
        int length = str1.Length + str2.Length;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        str2.CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;
        return new StbGuiString(this, result.Span);
    }

    public StbGuiString Concat(ReadOnlySpan<char> str1, int val)
    {
        Span<char> val_str = stackalloc char[32];
        val.TryFormat(val_str, out var val_str_len);
        int length = str1.Length + val_str_len;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.Slice(0, val_str_len).CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;
        return new StbGuiString(this, result.Span);

    }

    public StbGuiString Concat(ReadOnlySpan<char> str1, long val)
    {
        Span<char> val_str = stackalloc char[32];
        val.TryFormat(val_str, out var val_str_len);
        int length = str1.Length + val_str_len;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.Slice(0, val_str_len).CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;
        return new StbGuiString(this, result.Span);

    }

    public StbGuiString Concat(ReadOnlySpan<char> str1, float val, string format)
    {
        Span<char> val_str = stackalloc char[32];
        val.TryFormat(val_str, out var val_str_len, format);
        int length = str1.Length + val_str_len;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.Slice(0, val_str_len).CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;
        return new StbGuiString(this, result.Span);
    }

    private static readonly string[] FLOAT_FORMATS = ["F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9"];
    public StbGuiString Concat(ReadOnlySpan<char> str1, float val, int decimals)
    {
        Debug.Assert(decimals >= 0 && decimals <= 9);
        return Concat(str1, val, FLOAT_FORMATS[decimals]);
    }

    public StbGuiString Concat(ReadOnlySpan<char> str1, float val)
    {
        Span<char> val_str = stackalloc char[32];
        val.TryFormat(val_str, out var val_str_len);
        int length = str1.Length + val_str_len;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.Slice(0, val_str_len).CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;
        return new StbGuiString(this, result.Span);
    }

    public StbGuiString Concat(ReadOnlySpan<char> str1, double val)
    {
        Span<char> val_str = stackalloc char[32];
        val.TryFormat(val_str, out var val_str_len);
        int length = str1.Length + val_str_len;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.Slice(0, val_str_len).CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;
        return new StbGuiString(this, result.Span);
    }

    public StbGuiString Concat(ReadOnlySpan<char> str1, bool val)
    {
        Span<char> val_str = stackalloc char[32];
        val.TryFormat(val_str, out var val_str_len);
        int length = str1.Length + val_str_len;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.Slice(0, val_str_len).CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;
        return new StbGuiString(this, result.Span);
    }

    public void ResetPool()
    {
        memoryPoolOffset = 0;
    }
}
