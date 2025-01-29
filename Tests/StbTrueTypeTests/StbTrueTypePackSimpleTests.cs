namespace StbSharp.Tests;

public class StbTrueTypePackSimpleTests : StbTrueTypeTests
{
    [Theory, CombinatorialData]
    public void TestPackSimple(
        [CombinatorialValues(
            "Cousine-Regular.ttf",
            "DroidSans.ttf",
            "Karla-Regular.ttf",
            "ProggyClean.ttf",
            "ProggyTiny.ttf",
            "Roboto-Medium.ttf")] string fontFileName,
        [CombinatorialValues(8.0f, 16.0f, 32.0f)] float fontSize)
    {
        var ttf_buffer = GetFontFile(fontFileName);

        const int rangeFrom = 0;
        const int rangeTo = 255;
        const int width = 512;
        const int height = 512;

        byte[] bitmap = new byte[width * height];

        int rangeSize = rangeTo - rangeFrom;

        StbTrueType.stbtt_bakedchar[] cdata = new StbTrueType.stbtt_bakedchar[rangeSize];

        // no guarantee this fits!
        // can free ttf_buffer at this point
        int result = StbTrueType.stbtt_BakeFontBitmap(ttf_buffer, 0, fontSize, bitmap, width, height, rangeFrom, rangeSize, cdata);

        Assert.True(result != 0, $"Failed to bake font bitmap: {fontFileName} {fontSize} {rangeFrom}-{rangeTo}");

        int addedCharacters = result < 0
            ? -result //if result is negative, returns the negative of the number of characters that fit
            : rangeSize;

        Assert.True(addedCharacters == rangeSize, $"Failed to back all fonts in bitmap: {fontFileName} {fontSize} {rangeFrom}-{rangeTo}");

        string expectedFileName = BuildExpectedPackedSimpleImageFileName(fontFileName, fontSize, rangeFrom, rangeTo);
        string generatedFileName = BuildGeneratedPackedSimpleImageFileName(fontFileName, fontSize, rangeFrom, rangeTo);

        var generatedImage = GenerateImageFromFontBitmap(bitmap, width, height);

        AssertImagesEqual(GetExpectedFontImage(expectedFileName), generatedImage, generatedFileName);
    }

    static private string BuildExpectedPackedSimpleImageFileName(string fontFileName, float fontSize, int rangeFrom, int rangeTo)
    {
        return Path.Combine(ExpectedPath, $"PackedSimple_{Path.GetFileNameWithoutExtension(fontFileName)}_{fontSize}_{rangeFrom}-{rangeTo}.png");
    }

    static private string BuildGeneratedPackedSimpleImageFileName(string fontFileName, float fontSize, int rangeFrom, int rangeTo)
    {
        string expected = BuildExpectedPackedSimpleImageFileName(fontFileName, fontSize, rangeFrom, rangeTo);

        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}.png");
    }
}
