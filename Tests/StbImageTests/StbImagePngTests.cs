using StbSharp.StbCommon;

namespace StbSharp.Tests;

public class StbImagePngTests : StbImageTests
{
    [Theory, CombinatorialData]
    public void TestPrimary(
        [CombinatorialValues(
            "png/primary/basi0g01.png",
            "png/primary/basi0g02.png",
            "png/primary/basi0g04.png",
            "png/primary/basi0g08.png",
            "png/primary/basi2c08.png",
            "png/primary/basi3p01.png",
            "png/primary/basi3p02.png",
            "png/primary/basi3p04.png",
            "png/primary/basi3p08.png",
            "png/primary/basi4a08.png",
            "png/primary/basi6a08.png",
            "png/primary/basn0g01.png",
            "png/primary/basn0g02.png",
            "png/primary/basn0g04.png",
            "png/primary/basn0g08.png",
            "png/primary/basn2c08.png",
            "png/primary/basn3p01.png",
            "png/primary/basn3p02.png",
            "png/primary/basn3p04.png",
            "png/primary/basn3p08.png",
            "png/primary/basn4a08.png",
            "png/primary/basn6a08.png",
            "png/primary/bgai4a08.png",
            "png/primary/bgan6a08.png",
            "png/primary/bgbn4a08.png",
            "png/primary/bgwn6a08.png",
            "png/primary/s01i3p01.png",
            "png/primary/s01n3p01.png",
            "png/primary/s02i3p01.png",
            "png/primary/s02n3p01.png",
            "png/primary/s03i3p01.png",
            "png/primary/s03n3p01.png",
            "png/primary/s04i3p01.png",
            "png/primary/s04n3p01.png",
            "png/primary/s05i3p02.png",
            "png/primary/s05n3p02.png",
            "png/primary/s06i3p02.png",
            "png/primary/s06n3p02.png",
            "png/primary/s07i3p02.png",
            "png/primary/s07n3p02.png",
            "png/primary/s08i3p02.png",
            "png/primary/s08n3p02.png",
            "png/primary/s09i3p02.png",
            "png/primary/s09n3p02.png",
            "png/primary/s32i3p04.png",
            "png/primary/s32n3p04.png",
            "png/primary/s33i3p04.png",
            "png/primary/s33n3p04.png",
            "png/primary/s34i3p04.png",
            "png/primary/s34n3p04.png",
            "png/primary/s35i3p04.png",
            "png/primary/s35n3p04.png",
            "png/primary/s36i3p04.png",
            "png/primary/s36n3p04.png",
            "png/primary/s37i3p04.png",
            "png/primary/s37n3p04.png",
            "png/primary/s38i3p04.png",
            "png/primary/s38n3p04.png",
            "png/primary/s39i3p04.png",
            "png/primary/s39n3p04.png",
            "png/primary/s40i3p04.png",
            "png/primary/s40n3p04.png",
            "png/primary/tbbn0g04.png",
            "png/primary/tbbn3p08.png",
            "png/primary/tbgn3p08.png",
            "png/primary/tbrn2c08.png",
            "png/primary/tbwn3p08.png",
            "png/primary/tbyn3p08.png",
            "png/primary/tm3n3p02.png",
            "png/primary/tp0n0g08.png",
            "png/primary/tp0n2c08.png",
            "png/primary/tp0n3p08.png",
            "png/primary/tp1n3p08.png",
            "png/primary/z00n2c08.png",
            "png/primary/z03n2c08.png",
            "png/primary/z06n2c08.png",
            "png/primary/z09n2c08.png"
            )
        ] string imageFileName)
    {
        TestImage(imageFileName);
    }

    [Theory, CombinatorialData]
    public void TestPrimary_Check(
        [CombinatorialValues(
            "png/primary_check/basi0g01.png",
            "png/primary_check/basi0g02.png",
            "png/primary_check/basi0g04.png",
            "png/primary_check/basi0g08.png",
            "png/primary_check/basi2c08.png",
            "png/primary_check/basi3p01.png",
            "png/primary_check/basi3p02.png",
            "png/primary_check/basi3p04.png",
            "png/primary_check/basi3p08.png",
            "png/primary_check/basi4a08.png",
            "png/primary_check/basi6a08.png",
            "png/primary_check/basn0g01.png",
            "png/primary_check/basn0g02.png",
            "png/primary_check/basn0g04.png",
            "png/primary_check/basn0g08.png",
            "png/primary_check/basn2c08.png",
            "png/primary_check/basn3p01.png",
            "png/primary_check/basn3p02.png",
            "png/primary_check/basn3p04.png",
            "png/primary_check/basn3p08.png",
            "png/primary_check/basn4a08.png",
            "png/primary_check/basn6a08.png",
            "png/primary_check/bgai4a08.png",
            "png/primary_check/bgan6a08.png",
            "png/primary_check/bgbn4a08.png",
            "png/primary_check/bgwn6a08.png",
            "png/primary_check/s01i3p01.png",
            "png/primary_check/s01n3p01.png",
            "png/primary_check/s02i3p01.png",
            "png/primary_check/s02n3p01.png",
            "png/primary_check/s03i3p01.png",
            "png/primary_check/s03n3p01.png",
            "png/primary_check/s04i3p01.png",
            "png/primary_check/s04n3p01.png",
            "png/primary_check/s05i3p02.png",
            "png/primary_check/s05n3p02.png",
            "png/primary_check/s06i3p02.png",
            "png/primary_check/s06n3p02.png",
            "png/primary_check/s07i3p02.png",
            "png/primary_check/s07n3p02.png",
            "png/primary_check/s08i3p02.png",
            "png/primary_check/s08n3p02.png",
            "png/primary_check/s09i3p02.png",
            "png/primary_check/s09n3p02.png",
            "png/primary_check/s32i3p04.png",
            "png/primary_check/s32n3p04.png",
            "png/primary_check/s33i3p04.png",
            "png/primary_check/s33n3p04.png",
            "png/primary_check/s34i3p04.png",
            "png/primary_check/s34n3p04.png",
            "png/primary_check/s35i3p04.png",
            "png/primary_check/s35n3p04.png",
            "png/primary_check/s36i3p04.png",
            "png/primary_check/s36n3p04.png",
            "png/primary_check/s37i3p04.png",
            "png/primary_check/s37n3p04.png",
            "png/primary_check/s38i3p04.png",
            "png/primary_check/s38n3p04.png",
            "png/primary_check/s39i3p04.png",
            "png/primary_check/s39n3p04.png",
            "png/primary_check/s40i3p04.png",
            "png/primary_check/s40n3p04.png",
            "png/primary_check/tbbn0g04.png",
            "png/primary_check/tbbn3p08.png",
            "png/primary_check/tbgn3p08.png",
            "png/primary_check/tbrn2c08.png",
            "png/primary_check/tbwn3p08.png",
            "png/primary_check/tbyn3p08.png",
            "png/primary_check/tm3n3p02.png",
            "png/primary_check/tp0n0g08.png",
            "png/primary_check/tp0n2c08.png",
            "png/primary_check/tp0n3p08.png",
            "png/primary_check/tp1n3p08.png",
            "png/primary_check/z00n2c08.png",
            "png/primary_check/z03n2c08.png",
            "png/primary_check/z06n2c08.png",
            "png/primary_check/z09n2c08.png"
            )
        ] string imageFileName)
    {
        TestImage(imageFileName);
    }

    [Theory, CombinatorialData]
    public void TestIphone(
        [CombinatorialValues(
            "png/iphone/iphone_basi0g01.png",
            "png/iphone/iphone_basi0g02.png",
            "png/iphone/iphone_basi3p02.png",
            "png/iphone/iphone_bgwn6a08.png",
            "png/iphone/iphone_bgyn6a16.png",
            "png/iphone/iphone_tbyn3p08.png",
            "png/iphone/iphone_z06n2c08.png"
            )
        ] string imageFileName)
    {
        // We can't compare the images because loading the apple png format fails on ImageMagick
        TestLoadImageOnly(imageFileName);
    }

    [Theory, CombinatorialData]
    public void Test16bit_Check(
        [CombinatorialValues(
            "png/16bit/basi2c16.png",
            "png/16bit/basi4a16.png",
            "png/16bit/basi6a16.png",
            "png/16bit/basn2c16.png",
            "png/16bit/basn4a16.png",
            "png/16bit/basn6a16.png",
            "png/16bit/bgai4a16.png",
            "png/16bit/bgan6a16.png",
            "png/16bit/bggn4a16.png",
            "png/16bit/bgyn6a16.png",
            "png/16bit/oi1n2c16.png",
            "png/16bit/oi2n2c16.png",
            "png/16bit/oi4n2c16.png",
            "png/16bit/oi9n2c16.png",
            "png/16bit/tbbn2c16.png",
            "png/16bit/tbgn2c16.png",
            "png/16bit/tbwn0g16.png"
            //"png/16bit/basi0g16.png", // Fails (grayscale difference)
            //"png/16bit/basn0g16.png", // Fails (grayscale difference)
            //"png/16bit/oi1n0g16.png", // Fails (grayscale difference)
            //"png/16bit/oi2n0g16.png", // Fails (grayscale difference)
            //"png/16bit/oi4n0g16.png", // Fails (grayscale difference)
            //"png/16bit/oi9n0g16.png", // Fails (grayscale difference)
            )
        ] string imageFileName,
        [CombinatorialValues(true, false)] bool test16bits)
    {
        if (test16bits)
            TestImage16(imageFileName, 0.1f);
        else
            TestImage(imageFileName, 0.1f);
    }

    static private void TestLoadImageOnly(string imageFileName)
    {
        string expectedFileName = BuildExpectedFileName(imageFileName);

        LoadStbiImage(expectedFileName);
    }

    static private void TestImage(string imageFileName, float tolerance = 0)
    {
        string expectedFileName = BuildExpectedFileName(imageFileName);
        string generatedFileName = BuildGeneratedFileName(imageFileName);

        var generatedImage = LoadStbiImage(expectedFileName);

        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName, tolerance);
    }

    static private void TestImage16(string imageFileName, float tolerance = 0)
    {
        string expectedFileName = BuildExpectedFileName(imageFileName);
        string generatedFileName = BuildGenerated16bitsFileName(imageFileName);

        var generatedImage = LoadStbiImage16(expectedFileName);

        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName, tolerance);
    }

    static private string BuildExpectedFileName(string fontFileName)
    {
        return Path.Combine(ExpectedPath, fontFileName);
    }

    static private string BuildGeneratedFileName(string fontFileName)
    {
        string expected = BuildExpectedFileName(fontFileName);

        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}.png");
    }

    static private string BuildGenerated16bitsFileName(string fontFileName)
    {
        string expected = BuildExpectedFileName(fontFileName);

        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}_16bits.png");
    }
}
