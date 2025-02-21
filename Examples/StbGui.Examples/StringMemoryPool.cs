namespace StbSharp.Examples;

using System.Diagnostics;
using StbSharp;

public class StringMemoryPool
{
    private readonly Memory<char> memoryPool;
    private int memoryPoolOffset = 0;

    public StringMemoryPool(int size = 1024 * 1024)
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

        int length = str1.Length + val_str.Length;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");

        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;

        return result.Span;
    }

    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, long val)
    {
        Span<char> val_str = stackalloc char[32];
        val.TryFormat(val_str, out var val_str_len);

        int length = str1.Length + val_str.Length;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");

        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;

        return result.Span;
    }

    public ReadOnlySpan<char> Concat(ReadOnlySpan<char> str1, float val)
    {
        Span<char> val_str = stackalloc char[32];
        val.TryFormat(val_str, out var val_str_len);

        int length = str1.Length + val_str.Length;
        if (memoryPoolOffset + length >= memoryPool.Length)
            throw new InvalidOperationException("Memory pool exhausted");

        var result = memoryPool.Slice(memoryPoolOffset, length);
        str1.CopyTo(result.Span);
        val_str.CopyTo(result.Span.Slice(str1.Length));
        memoryPoolOffset += length;

        return result.Span;
    }

    public void ResetPool()
    {
        memoryPoolOffset = 0;
    }
}