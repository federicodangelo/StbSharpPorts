using StbSharp.StbCommon;

using Xunit.Sdk;

namespace StbSharp.Tests;

public class StbImageJpgTests : StbImageTests
{
    [Theory, CombinatorialData]
    public void TestValid(
        [CombinatorialValues(
            "jpg/valid/jpeg400jfif.jpg",
            "jpg/valid/jpeg420exif.jpg",
            "jpg/valid/jpeg422jfif.jpg",
            "jpg/valid/sample_1280x853.jpg",
            "jpg/valid/sample_1920x1280.jpg",
            "jpg/valid/sample_5184x3456.jpg",
            "jpg/valid/sample_640x426.jpg"
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

        // Use 2% tolerance for JPG files..
        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName, 0.01f);
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
