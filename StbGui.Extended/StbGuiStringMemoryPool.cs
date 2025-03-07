namespace StbSharp;

using System.Diagnostics;

public class StbGuiStringMemoryPool
{
    private readonly Memory<char> memoryPool;
    private int memoryPoolOffset = 0;

    public StbGuiStringMemoryPool(int size = 1024 * 1024)
    {
        Debug.Assert(size >= 0);
        memoryPool = new Memory<char>(new char[size]);
    }

    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
    {
        int length = str1.Length + str2.Length;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");
        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        str2.CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;
        return result.Span;
    }

    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, int val)
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
        return result.Span;

    }

    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, long val)
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
        return result.Span;

    }

    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, float val, string format)
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
        return result.Span;
    }

    static private string[] FLOAT_FORMATS = ["F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9"];
    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, float val, int decimals)
    {
        Debug.Assert(decimals >= 0 && decimals <= 9);
        return Concat(str1, val, FLOAT_FORMATS[decimals]);
    }

    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, float val)
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
        return result.Span;
    }

    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, bool val)
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
        return result.Span;
    }

    public void ResetPool()
    {
        memoryPoolOffset = 0;
    }
}