#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

namespace StbSharp.Tests;

public class StbHashTests
{
    public enum Version
    {
        Sip64,
        Sip128,
        HSip32,
        HSip64
    }

    [Theory, CombinatorialData]
    public void TestSipHash([CombinatorialValues(Version.Sip64, Version.Sip128, Version.HSip32, Version.HSip64)] Version version)
    {
        Span<byte> _in = stackalloc byte[64], _out = stackalloc byte[16], _k = stackalloc byte[16];

        for (int i = 0; i < 16; ++i)
            _k[i] = (byte)i;


        byte[][] vectors = version switch
        {
            Version.Sip64 => StbHashTestsVectors.vectors_sip64,
            Version.Sip128 => StbHashTestsVectors.vectors_sip128,
            Version.HSip32 => StbHashTestsVectors.vectors_hsip32,
            Version.HSip64 => StbHashTestsVectors.vectors_hsip64,
        };

        for (int i = 0; i < vectors.Length; ++i)
        {
            _in[i] = (byte)i;
            byte[] vector = vectors[i];
            int len = vector.Length;

            bool useHalf = (int)version >= 2;

            var inputBytes = _in.Slice(0, i);
            var inputKey = useHalf ? _k.Slice(0, _k.Length / 2) : _k;
            var outputHash = _out.Slice(0, len);

            if (useHalf)
                StbHash.stbh_halfsiphash(inputBytes, inputKey, outputHash);
            else
                StbHash.stbh_siphash(inputBytes, inputKey, outputHash);

            Assert.True(vector.AsSpan().SequenceCompareTo(outputHash) == 0, $"Hash mismatch in vector {i}");
        }
    }
}
