namespace StbSharp.Tests;

public class StbTrueTypePackOversamplingTests : StbTrueTypeTests
{
    [Theory, CombinatorialData]
    public void TestPackOversampling(
        [CombinatorialValues(
            "DroidSans.ttf",
            "Karla-Regular.ttf",
            "Roboto-Medium.ttf",
            "FDArrayTest257.otf")] string fontFileName,
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

        StbTrueType.stbtt_PackSetOversampling(ref spc, 2, 2);
        StbTrueType.stbtt_PackSetSkipMissingCodepoints(ref spc, true);
        
        StbTrueType.stbtt_PackFontRanges(ref spc, ttf_buffer, 0, packRanges, 1);

        StbTrueType.stbtt_PackEnd(ref spc);

        //Assert.True(addedCharacters == rangeSize, $"Failed to back all fonts in bitmap: {fontFileName} {fontSize} {rangeFrom}-{rangeTo}");

        string expectedFileName = BuildExpectedFileName(fontFileName, fontSize, rangeFrom, rangeTo);
        string generatedFileName = BuildGeneratedFileName(fontFileName, fontSize, rangeFrom, rangeTo);

        var generatedImage = GenerateImageFromFontBitmap(bitmap, width, height);

        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName);
    }        

    static private string BuildExpectedFileName(string fontFileName, float fontSize, int rangeFrom, int rangeTo)
    {
        return Path.Combine(ExpectedPath, $"PackedOversampling_{Path.GetFileNameWithoutExtension(fontFileName)}_{fontSize}_{rangeFrom}-{rangeTo}.png");
    }

    static private string BuildGeneratedFileName(string fontFileName, float fontSize, int rangeFrom, int rangeTo)
    {
        string expected = BuildExpectedFileName(fontFileName, fontSize, rangeFrom, rangeTo);

        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}.png");
    }
}
