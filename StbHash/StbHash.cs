#pragma warning disable IDE1006 // Naming Styles

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace StbSharp;

// SipHash functions are ported from https://github.com/veorq/SipHash

public class StbHash
{
    /* default: SipHash-2-4 */
    const int STBH_SIP_HASH_C_ROUNDS = 2;
    const int STBH_SIP_HASH_D_ROUNDS = 4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ulong ROTL_64(ulong x, int b) => ((x) << (b)) | ((x) >> (64 - b));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint ROTL_32(uint x, int b) => ((x) << (b)) | ((x) >> (32 - b));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void U32TO8_LE(Span<byte> p, uint v)
    {
        p[0] = (byte)v;
        p[1] = (byte)((v) >> 8);
        p[2] = (byte)((v) >> 16);
        p[3] = (byte)((v) >> 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void U64TO8_LE(Span<byte> p, ulong v)
    {
        U32TO8_LE(p, (uint)v);
        U32TO8_LE(p.Slice(sizeof(uint)), (uint)((v) >> 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ulong U8TO64_LE(ReadOnlySpan<byte> p) =>
        ((ulong)p[0]) | (((ulong)p[1]) << 8) |
        (((ulong)p[2]) << 16) | (((ulong)p[3]) << 24) |
        (((ulong)p[4]) << 32) | (((ulong)p[5]) << 40) |
        (((ulong)p[6]) << 48) | (((ulong)p[7]) << 56);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint U8TO32_LE(ReadOnlySpan<byte> p) =>
        ((uint)p[0]) | (((uint)p[1]) << 8) |
         (((uint)p[2]) << 16) | (((uint)p[3]) << 24);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SIPROUND_64(ref ulong v0, ref ulong v1, ref ulong v2, ref ulong v3)
    {
        v0 += v1;
        v1 = ROTL_64(v1, 13);
        v1 ^= v0;
        v0 = ROTL_64(v0, 32);
        v2 += v3;
        v3 = ROTL_64(v3, 16);
        v3 ^= v2;
        v0 += v3;
        v3 = ROTL_64(v3, 21);
        v3 ^= v0;
        v2 += v1;
        v1 = ROTL_64(v1, 17);
        v1 ^= v2;
        v2 = ROTL_64(v2, 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SIPROUND_32(ref uint v0, ref uint v1, ref uint v2, ref uint v3)
    {
        v0 += v1;
        v1 = ROTL_32(v1, 5);
        v1 ^= v0;
        v0 = ROTL_32(v0, 16);
        v2 += v3;
        v3 = ROTL_32(v3, 8);
        v3 ^= v2;
        v0 += v3;
        v3 = ROTL_32(v3, 7);
        v3 ^= v0;
        v2 += v1;
        v1 = ROTL_32(v1, 13);
        v1 ^= v2;
        v2 = ROTL_32(v2, 16);
    }

    static public void stbh_siphash(ReadOnlySpan<byte> inputBytes, ReadOnlySpan<byte> inputKey, Span<byte> output)
    {
        int outlen = output.Length;
        int inlen = inputBytes.Length;

        Debug.Assert((outlen == sizeof(ulong)) || (outlen == sizeof(ulong) * 2));
        Debug.Assert(inputKey.Length == sizeof(ulong) * 2);

        ulong v0 = 0x736f6d6570736575;
        ulong v1 = 0x646f72616e646f6d;
        ulong v2 = 0x6c7967656e657261;
        ulong v3 = 0x7465646279746573;
        ulong k0 = U8TO64_LE(inputKey);
        ulong k1 = U8TO64_LE(inputKey.Slice(sizeof(ulong)));

        int fullBlocksLen = inlen & ~(sizeof(ulong) - 1);
        int left = inlen & (sizeof(ulong) - 1);
        ulong b = ((ulong)inlen) << ((sizeof(ulong) - 1) * 8);

        v3 ^= k1;
        v2 ^= k0;
        v1 ^= k1;
        v0 ^= k0;

        if (outlen == sizeof(ulong) * 2)
            v1 ^= 0xee;

        for (int i = 0; i < fullBlocksLen; i += sizeof(ulong))
        {
            ulong m = U8TO64_LE(inputBytes);
            v3 ^= m;

            for (int round = 0; round < STBH_SIP_HASH_C_ROUNDS; round++)
                SIPROUND_64(ref v0, ref v1, ref v2, ref v3);

            v0 ^= m;

            inputBytes = inputBytes.Slice(sizeof(ulong));
        }

        switch (left)
        {
            case 7:
                b |= ((ulong)inputBytes[6]) << 48;
                goto case 6;
            /* FALLTHRU */
            case 6:
                b |= ((ulong)inputBytes[5]) << 40;
                goto case 5;
            /* FALLTHRU */
            case 5:
                b |= ((ulong)inputBytes[4]) << 32;
                goto case 4;
            /* FALLTHRU */
            case 4:
                b |= ((ulong)inputBytes[3]) << 24;
                goto case 3;
            /* FALLTHRU */
            case 3:
                b |= ((ulong)inputBytes[2]) << 16;
                goto case 2;
            /* FALLTHRU */
            case 2:
                b |= ((ulong)inputBytes[1]) << 8;
                goto case 1;
            /* FALLTHRU */
            case 1:
                b |= (ulong)inputBytes[0];
                break;
            case 0:
                break;
        }

        v3 ^= b;

        for (int round = 0; round < STBH_SIP_HASH_C_ROUNDS; round++)
            SIPROUND_64(ref v0, ref v1, ref v2, ref v3);

        v0 ^= b;

        if (outlen == sizeof(ulong) * 2)
            v2 ^= 0xee;
        else
            v2 ^= 0xff;

        for (int round = 0; round < STBH_SIP_HASH_D_ROUNDS; round++)
            SIPROUND_64(ref v0, ref v1, ref v2, ref v3);

        b = v0 ^ v1 ^ v2 ^ v3;
        U64TO8_LE(output, b);

        if (outlen == sizeof(ulong) * 2)
        {
            v1 ^= 0xdd;

            for (int round = 0; round < STBH_SIP_HASH_D_ROUNDS; round++)
                SIPROUND_64(ref v0, ref v1, ref v2, ref v3);

            b = v0 ^ v1 ^ v2 ^ v3;
            U64TO8_LE(output.Slice(sizeof(ulong)), b);
        }
    }

    static public void stbh_halfsiphash(ReadOnlySpan<byte> inputBytes, ReadOnlySpan<byte> inputKey, Span<byte> output)
    {
        int outlen = output.Length;
        int inlen = inputBytes.Length;

        Debug.Assert((outlen == sizeof(uint)) || (outlen == sizeof(ulong)));
        Debug.Assert(inputKey.Length == sizeof(uint) * 2);

        uint v0 = 0;
        uint v1 = 0;
        uint v2 = 0x6c796765;
        uint v3 = 0x74656462;
        uint k0 = U8TO32_LE(inputKey);
        uint k1 = U8TO32_LE(inputKey.Slice(sizeof(uint)));

        int fullBlocksLen = inlen & ~(sizeof(uint) - 1);
        int left = inlen & (sizeof(uint) - 1);
        uint b = ((uint)inlen) << ((sizeof(uint) - 1) * 8);

        v3 ^= k1;
        v2 ^= k0;
        v1 ^= k1;
        v0 ^= k0;

        if (outlen == sizeof(ulong))
            v1 ^= 0xee;

        for (int i = 0; i < fullBlocksLen; i += sizeof(uint))
        {
            uint m = U8TO32_LE(inputBytes);
            v3 ^= m;

            for (int round = 0; round < STBH_SIP_HASH_C_ROUNDS; round++)
                SIPROUND_32(ref v0, ref v1, ref v2, ref v3);

            v0 ^= m;

            inputBytes = inputBytes.Slice(sizeof(uint));
        }

        switch (left)
        {
            /* FALLTHRU */
            case 3:
                b |= ((uint)inputBytes[2]) << 16;
                goto case 2;
            /* FALLTHRU */
            case 2:
                b |= ((uint)inputBytes[1]) << 8;
                goto case 1;
            /* FALLTHRU */
            case 1:
                b |= (uint)inputBytes[0];
                break;
            case 0:
                break;
        }

        v3 ^= b;

        for (int round = 0; round < STBH_SIP_HASH_C_ROUNDS; round++)
            SIPROUND_32(ref v0, ref v1, ref v2, ref v3);

        v0 ^= b;

        if (outlen == sizeof(ulong))
            v2 ^= 0xee;
        else
            v2 ^= 0xff;

        for (int round = 0; round < STBH_SIP_HASH_D_ROUNDS; round++)
            SIPROUND_32(ref v0, ref v1, ref v2, ref v3);

        b = v1 ^ v3;
        U32TO8_LE(output, b);

        if (outlen == sizeof(ulong))
        {
            v1 ^= 0xdd;

            for (int round = 0; round < STBH_SIP_HASH_D_ROUNDS; round++)
                SIPROUND_32(ref v0, ref v1, ref v2, ref v3);

            b = v1 ^ v3;
            U32TO8_LE(output.Slice(sizeof(uint)), b);
        }
    }    
}
