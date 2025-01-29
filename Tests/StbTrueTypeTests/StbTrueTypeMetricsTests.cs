using StbSharp.StbCommon;

namespace StbSharp.Tests;

public class StbTrueTypeMetricsTests : StbTrueTypeTests
{
    [Theory, CombinatorialData]
    public void TestMetrics(
        [CombinatorialValues(
            "Cousine-Regular.ttf",
            "DroidSans.ttf",
            "Karla-Regular.ttf",
            "ProggyClean.ttf",
            "ProggyTiny.ttf",
            "Roboto-Medium.ttf")] string fontFileName,
        [CombinatorialValues(32.0f)] float fontSize)
    {
        const string text = "Hello World!";

        var ttf_buffer = GetFontFile(fontFileName);

        var init_result = StbTrueType.stbtt_InitFont(out StbTrueType.stbtt_fontinfo font, ttf_buffer, StbTrueType.stbtt_GetFontOffsetForIndex(ttf_buffer, 0));
        
        StbTrueType.stbtt_GetFontVMetrics(ref font, out int ascent, out _, out int _);

        Assert.True(0 != init_result, $"Failed to initialize font: {fontFileName}");

        float scale = StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize);
        int baseline = (int)(ascent * scale);

        float xpos = 2;

        int textureWidth = (int) (fontSize * text.Length);
        int textureHeight = (int) fontSize;

        BytePtr bitmap = new BytePtr(new byte[textureHeight * textureWidth]);

        var index = 0;

        foreach (var character in text)
        {
            float x_shift = xpos - (float)MathF.Floor(xpos);

            StbTrueType.stbtt_GetCodepointHMetrics(ref font, character, out int advance, out int lsb);
            StbTrueType.stbtt_GetCodepointBitmapBoxSubpixel(ref font, character, scale, scale, x_shift, 0, out int x0, out int y0, out int x1, out int y1);
            StbTrueType.stbtt_MakeCodepointBitmapSubpixel(ref font, bitmap[Math.Max(baseline + y0, 0) * textureWidth + (int)xpos + x0], x1 - x0, y1 - y0, textureWidth, scale, scale, x_shift, 0, character);

            // note that this stomps the old data, so where character boxes overlap (e.g. 'lj') it's wrong
            // because this API is really for baking character bitmaps into textures. if you want to render
            // a sequence of characters, you really need to render each bitmap to a temp buffer, then
            // "alpha blend" that into the working buffer
            xpos += advance * scale;
            if (index + 1 < text.Length)
                xpos += scale * StbTrueType.stbtt_GetCodepointKernAdvance(ref font, text[index], text[index + 1]);

            index++;
        }

        string expectedFileName = BuildExpectedFileName(fontFileName, fontSize);
        string generatedFileName = BuildGeneratedFileName(fontFileName, fontSize);

        var generatedImage = GenerateImageFromFontBitmap(bitmap.Raw, textureWidth, textureHeight);

        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName);

        AssertImagesEqual(expectedFileName, generatedImage, generatedFileName);
    }

    static private string BuildExpectedFileName(string fontFileName, float fontSize)
    {
        return Path.Combine(ExpectedPath, $"Metrics_{Path.GetFileNameWithoutExtension(fontFileName)}_{fontSize}_.png");
    }

    static private string BuildGeneratedFileName(string fontFileName, float fontSize)
    {
        string expected = BuildExpectedFileName(fontFileName, fontSize);

        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}.png");
    }

}
