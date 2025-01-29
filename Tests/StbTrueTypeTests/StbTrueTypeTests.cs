#pragma warning disable CA1416 // Validate platform compatibility

using System.Drawing;
using System.Globalization;

namespace StbSharp.Tests;



public class StbTrueTypeTests
{
    [Theory, CombinatorialData]
    public void TestSingleCharacter(
        [CombinatorialValues(
            "Cousine-Regular.ttf",
            "DroidSans.ttf",
            "Karla-Regular.ttf",
            "ProggyClean.ttf",
            "ProggyTiny.ttf",
            "Roboto-Medium.ttf")] string fontFileName,
        [CombinatorialValues(8.0f, 16.0f, 32.0f)] float fontSize,
        [CombinatorialValues('a', 'b', 'x', 'A', 'B', 'X', '@', 'ñ')] char character)
    {
        var ttf_buffer = GetFontFile(fontFileName);

        var init_result = StbTrueType.stbtt_InitFont(out StbTrueType.stbtt_fontinfo font, ttf_buffer, StbTrueType.stbtt_GetFontOffsetForIndex(ttf_buffer, 0));

        Assert.True(0 != init_result, $"Failed to initialize font: {fontFileName}");

        var bitmap = StbTrueType.stbtt_GetCodepointBitmap(ref font, 0, StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize), character, out int width, out int height, out _, out _);

        Assert.True(bitmap != null, $"Failed to generate bitmap font: {fontFileName} {fontSize} {character}");

        string expectedFileName = BuildExpectedSingleCharacterImageFileName(fontFileName, fontSize, character);
        string generatedFileName = BuildGeneratedSingleCharacterImageFileName(fontFileName, fontSize, character);

        var generatedImage = GenerateImageFromFontBitmap(bitmap, width, height);

        SaveGeneratedImage(generatedFileName, generatedImage);

        AssertImagesEqual(GetExpectedFontImage(expectedFileName), generatedImage);

        // Delete generated images, that way we only leave in storage the failed images
        File.Delete(generatedFileName);
    }

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

        SaveGeneratedImage(generatedFileName, generatedImage);

        AssertImagesEqual(GetExpectedFontImage(expectedFileName), generatedImage);

        // Delete generated images, that way we only leave in storage the failed images
        File.Delete(generatedFileName);
    }    

    [Theory, CombinatorialData]
    public void TestPackComplex(
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

        StbTrueType.stbtt_pack_context spc;

        StbTrueType.stbtt_pack_range[] packRanges = new StbTrueType.stbtt_pack_range[1];
        packRanges[0].font_size = fontSize;
        packRanges[0].first_unicode_codepoint_in_range = rangeFrom;
        packRanges[0].num_chars = rangeSize;
        packRanges[0].chardata_for_range = new StbTrueType.stbtt_packedchar[rangeSize];

        StbTrueType.stbtt_PackBegin(out spc, bitmap, width, height, 0, 1);

        StbTrueType.stbtt_PackSetOversampling(ref spc, 1, 1);
        StbTrueType.stbtt_PackSetSkipMissingCodepoints(ref spc, true);
        
        StbTrueType.stbtt_PackFontRanges(ref spc, ttf_buffer, 0, packRanges, 1);

        StbTrueType.stbtt_PackEnd(ref spc);

        //Assert.True(addedCharacters == rangeSize, $"Failed to back all fonts in bitmap: {fontFileName} {fontSize} {rangeFrom}-{rangeTo}");

        string expectedFileName = BuildExpectedPackedComplexImageFileName(fontFileName, fontSize, rangeFrom, rangeTo);
        string generatedFileName = BuildGeneratedPackedComplexImageFileName(fontFileName, fontSize, rangeFrom, rangeTo);

        var generatedImage = GenerateImageFromFontBitmap(bitmap, width, height);

        SaveGeneratedImage(generatedFileName, generatedImage);

        AssertImagesEqual(GetExpectedFontImage(expectedFileName), generatedImage);

        // Delete generated images, that way we only leave in storage the failed images
        File.Delete(generatedFileName);
    }        

    const string FontsPath = "Fonts";
    const string ExpectedPath = "Expected";
    const string GeneratedPath = "Generated";

    static private byte[] GetFontFile(string fileName)
    {
        var fullPath = Path.Combine(FontsPath, fileName);

        Assert.True(File.Exists(fullPath), "Missing font file: " + fullPath);

        return File.ReadAllBytes(fullPath);
    }

    static private Bitmap GetExpectedFontImage(string fileName)
    {
        Assert.True(File.Exists(fileName), "Missing expected test image: " + fileName);

        return new Bitmap(fileName);
    }

    static private Bitmap GenerateImageFromFontBitmap(byte[] bitmap, int width, int height)
    {
        var image = new Bitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixel = bitmap[y * width + x];

                image.SetPixel(x, y, Color.FromArgb(255, pixel, pixel, pixel));
            }
        }

        return image;
    }

    static private void AssertImagesEqual(Bitmap expected, Bitmap bitmap)
    {

    }

    static private string BuildExpectedSingleCharacterImageFileName(string fontFileName, float fontSize, char character)
    {
        return Path.Combine(ExpectedPath, $"SingleCharacter_{Path.GetFileNameWithoutExtension(fontFileName)}_{fontSize}_{(int)character}.png");
    }

    static private string BuildGeneratedSingleCharacterImageFileName(string fontFileName, float fontSize, char character)
    {
        string expected = BuildExpectedSingleCharacterImageFileName(fontFileName, fontSize, character);

        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}.png");
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

    static private string BuildExpectedPackedComplexImageFileName(string fontFileName, float fontSize, int rangeFrom, int rangeTo)
    {
        return Path.Combine(ExpectedPath, $"PackedComplex_{Path.GetFileNameWithoutExtension(fontFileName)}_{fontSize}_{rangeFrom}-{rangeTo}.png");
    }

    static private string BuildGeneratedPackedComplexImageFileName(string fontFileName, float fontSize, int rangeFrom, int rangeTo)
    {
        string expected = BuildExpectedPackedComplexImageFileName(fontFileName, fontSize, rangeFrom, rangeTo);

        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}.png");
    }

    static private void SaveGeneratedImage(string fileName, Image image)
    {
        if (!Directory.Exists(GeneratedPath))
        {
            Directory.CreateDirectory(GeneratedPath);
        }

        image.Save(fileName);
    }
}
