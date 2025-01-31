using StbSharp.StbCommon;

namespace StbSharp.Tests;

public class StbImagePngTests : StbImageTests
{
    [Theory, CombinatorialData]
    public void TestPrimary(
        [CombinatorialValues(
            "primary\\z06n2c08.png")
        ] string imageFileName)
    {
        string expectedFileName = BuildExpectedFileName(imageFileName);
        string generatedFileName = BuildGeneratedFileName(imageFileName);

        var generatedImage = LoadStbiImage(expectedFileName);

        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName);

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
