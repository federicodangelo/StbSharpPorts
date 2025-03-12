using StbSharp.StbCommon;

using Xunit.Sdk;

namespace StbSharp.Tests;

public class StbImageTgaTests : StbImageTests
{
    [Theory, CombinatorialData]
    public void TestConformance(
        [CombinatorialValues(
            "tga/conformance/cbw8.tga",
            "tga/conformance/ccm8.tga",
            "tga/conformance/ctc24.tga",
            "tga/conformance/ubw8.tga",
            "tga/conformance/ucm8.tga",
            "tga/conformance/utc16.tga",
            "tga/conformance/utc24.tga"
            // "tga/conformance/utc32.tga" // file has a footer that needs to be processed to correctly decode, and the StbImage doesn't support it
            )
        ] string imageFileName)
    {
        TestImage(imageFileName);
    }

    [Theory, CombinatorialData]
    public void TestFileFormat(
        [CombinatorialValues(
            "tga/fileformat/cbw8.tga",
            "tga/fileformat/ccm8.tga",
            "tga/fileformat/ctc16.tga",
            "tga/fileformat/ctc24.tga",
            "tga/fileformat/ctc32.tga",
            "tga/fileformat/flag_b16.tga",
            "tga/fileformat/flag_b24.tga",
            "tga/fileformat/flag_b32.tga",
            "tga/fileformat/flag_t16.tga",
            "tga/fileformat/flag_t32.tga",
            "tga/fileformat/marbles.tga",
            "tga/fileformat/ubw8.tga",
            "tga/fileformat/ucm8.tga",
            "tga/fileformat/utc16.tga",
            "tga/fileformat/utc24.tga",
            "tga/fileformat/utc32.tga",
            "tga/fileformat/xing_b16.tga",
            "tga/fileformat/xing_b24.tga",
            "tga/fileformat/xing_b32.tga",
            "tga/fileformat/xing_t16.tga",
            "tga/fileformat/xing_t24.tga",
            "tga/fileformat/xing_t32.tga"
            )
        ] string imageFileName)
    {
        TestImage(imageFileName);
    }

    private static void TestImage(string imageFileName)
    {
        string expectedFileName = BuildExpectedFileName(imageFileName);
        string generatedFileName = BuildGeneratedFileName(imageFileName);

        var generatedImage = LoadStbiImage(expectedFileName);

        // Ok, testing TGA files is a nightmare, because FOR SOME REASON, even in fully tested programs (like Paint.NET), some TGA
        // files load with the incorrect pixel values (small diferences, but still..).. an the test suite that I found for TGA files
        // has the same inconsistencies.. so I kind of gave up and i'm adding a small tolerance value..
        // Even the library that i'm using to load them natively has inconsistencies... seems that no one really uses TGA files anymore
        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName, 0.02f);
    }

    private static string BuildExpectedFileName(string fontFileName)
    {
        return Path.Combine(ExpectedPath, fontFileName);
    }

    private static string BuildGeneratedFileName(string fontFileName)
    {
        string expected = BuildExpectedFileName(fontFileName);

        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}.png");
    }
}
