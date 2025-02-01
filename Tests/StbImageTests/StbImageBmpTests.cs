using StbSharp.StbCommon;
using Xunit.Sdk;

namespace StbSharp.Tests;

public class StbImageBmpTests : StbImageTests
{
    [Theory, CombinatorialData]
    public void TestValid(
        [CombinatorialValues(
            "bmp\\valid\\1bpp-1x1.bmp",
            "bmp\\valid\\1bpp-320x240-color.bmp",
            "bmp\\valid\\1bpp-320x240-overlappingcolor.bmp",
            "bmp\\valid\\1bpp-320x240.bmp",
            "bmp\\valid\\1bpp-321x240.bmp",
            "bmp\\valid\\1bpp-322x240.bmp",
            "bmp\\valid\\1bpp-323x240.bmp",
            "bmp\\valid\\1bpp-324x240.bmp",
            "bmp\\valid\\1bpp-325x240.bmp",
            "bmp\\valid\\1bpp-326x240.bmp",
            "bmp\\valid\\1bpp-327x240.bmp",
            "bmp\\valid\\1bpp-328x240.bmp",
            "bmp\\valid\\1bpp-329x240.bmp",
            "bmp\\valid\\1bpp-330x240.bmp",
            "bmp\\valid\\1bpp-331x240.bmp",
            "bmp\\valid\\1bpp-332x240.bmp",
            "bmp\\valid\\1bpp-333x240.bmp",
            "bmp\\valid\\1bpp-334x240.bmp",
            "bmp\\valid\\1bpp-335x240.bmp",
            "bmp\\valid\\1bpp-topdown-320x240.bmp",
            "bmp\\valid\\24bpp-1x1.bmp",
            "bmp\\valid\\24bpp-320x240.bmp",
            "bmp\\valid\\24bpp-321x240.bmp",
            "bmp\\valid\\24bpp-322x240.bmp",
            "bmp\\valid\\24bpp-323x240.bmp",
            "bmp\\valid\\24bpp-imagesize-zero.bmp",
            "bmp\\valid\\24bpp-topdown-320x240.bmp",
            "bmp\\valid\\32bpp-101110-320x240.bmp",
            "bmp\\valid\\32bpp-1x1.bmp",
            "bmp\\valid\\32bpp-320x240.bmp",
            //"bmp\\valid\\32bpp-888-optimalpalette-320x240.bmp", // loaded image is displaced 
            //"bmp\\valid\\32bpp-optimalpalette-320x240.bmp", // loaded image is displaced 
            "bmp\\valid\\32bpp-topdown-320x240.bmp",
            "bmp\\valid\\4bpp-1x1.bmp",
            "bmp\\valid\\4bpp-320x240.bmp",
            "bmp\\valid\\4bpp-321x240.bmp",
            "bmp\\valid\\4bpp-322x240.bmp",
            "bmp\\valid\\4bpp-323x240.bmp",
            "bmp\\valid\\4bpp-324x240.bmp",
            "bmp\\valid\\4bpp-325x240.bmp",
            "bmp\\valid\\4bpp-326x240.bmp",
            "bmp\\valid\\4bpp-327x240.bmp",
            "bmp\\valid\\4bpp-topdown-320x240.bmp",
            "bmp\\valid\\555-1x1.bmp",
            "bmp\\valid\\555-320x240.bmp",
            "bmp\\valid\\555-321x240.bmp",
            "bmp\\valid\\565-1x1.bmp",
            "bmp\\valid\\565-320x240-topdown.bmp",
            "bmp\\valid\\565-320x240.bmp",
            "bmp\\valid\\565-321x240-topdown.bmp",
            "bmp\\valid\\565-321x240.bmp",
            "bmp\\valid\\565-322x240-topdown.bmp",
            "bmp\\valid\\565-322x240.bmp",
            "bmp\\valid\\8bpp-1x1.bmp",
            "bmp\\valid\\8bpp-1x64000.bmp",
            "bmp\\valid\\8bpp-320x240.bmp",
            "bmp\\valid\\8bpp-321x240.bmp",
            "bmp\\valid\\8bpp-322x240.bmp",
            "bmp\\valid\\8bpp-323x240.bmp",
            "bmp\\valid\\8bpp-colorsimportant-two.bmp",
            "bmp\\valid\\8bpp-colorsused-zero.bmp",
            "bmp\\valid\\8bpp-topdown-320x240.bmp",
            "bmp\\valid\\rle4-absolute-320x240.bmp",
            "bmp\\valid\\rle4-alternate-320x240.bmp",
            "bmp\\valid\\rle4-delta-320x240.bmp",
            "bmp\\valid\\rle4-encoded-320x240.bmp",
            "bmp\\valid\\rle8-64000x1.bmp",
            "bmp\\valid\\rle8-absolute-320x240.bmp",
            "bmp\\valid\\rle8-blank-160x120.bmp",
            "bmp\\valid\\rle8-delta-320x240.bmp",
            "bmp\\valid\\rle8-encoded-320x240.bmp"
            )
        ] string imageFileName)
    {
        if (Path.GetFileName(imageFileName).StartsWith("rle"))
            throw SkipException.ForSkip("Skip RLE tests (not implemented)");

        TestImage(imageFileName);
    }

    static private void TestImage(string imageFileName)
    {
        string expectedFileName = BuildExpectedFileName(imageFileName);
        string generatedFileName = BuildGeneratedFileName(imageFileName);

        var generatedImage = LoadStbiImage(expectedFileName);

        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName);
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
}
