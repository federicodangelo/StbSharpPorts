namespace StbSharp.Tests;

public class StbTrueTypeCharacterTests : StbTrueTypeTests
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
        [CombinatorialValues(8.0f, 16.0f, 32.0f, 64.0f, 128.0f)] float fontSize,
        [CombinatorialValues('a', 'b', 'x', 'A', 'B', 'X', '@', 'ñ')] char character)
    {
        var ttf_buffer = GetFontFile(fontFileName);

        var init_result = StbTrueType.stbtt_InitFont(out StbTrueType.stbtt_fontinfo font, ttf_buffer, StbTrueType.stbtt_GetFontOffsetForIndex(ttf_buffer, 0));

        Assert.True(0 != init_result, $"Failed to initialize font: {fontFileName}");

        var bitmap = StbTrueType.stbtt_GetCodepointBitmap(ref font, 0, StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize), character, out int width, out int height, out _, out _);

        Assert.True(!bitmap.IsNull, $"Failed to generate bitmap font: {fontFileName} {fontSize} {character}");

        string expectedFileName = BuildExpectedSingleCharacterImageFileName(fontFileName, fontSize, character);
        string generatedFileName = BuildGeneratedSingleCharacterImageFileName(fontFileName, fontSize, character);

        var generatedImage = GenerateImageFromFontBitmap(bitmap, width, height);

        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName);
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

}
