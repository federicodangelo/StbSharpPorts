using SDL3;

namespace StbSharp.Examples;

public class SDLFont
{
    public readonly string Name;
    public readonly float FontSize = 13;

    private StbTrueType.stbtt_fontinfo font;
    private float fontScale;
    private float fontBaseline;
    private int fontBitmapSize = 512;
    private byte[] fontBitmap;

    private nint fontTexture;
    private StbTrueType.stbtt_packedchar[] fontCharData;

    private nint renderer;

    public SDLFont(string name, string fileName, float fontSize, nint renderer)
    {
        this.Name = name;
        this.FontSize = fontSize;
        this.renderer = renderer;

        byte[] fontBytes = File.ReadAllBytes("Fonts/ProggyClean.ttf");
        StbTrueType.stbtt_InitFont(out font, fontBytes, 0);

        fontScale = StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize);
        StbTrueType.stbtt_GetFontVMetrics(ref font, out int ascent, out _, out int _);

        fontBaseline = (int)(ascent * fontScale);

        fontBitmap = new byte[fontBitmapSize * fontBitmapSize];

        int rangeFrom = 0;
        int rangeTo = 255;

        int rangeSize = rangeTo - rangeFrom;

        StbTrueType.stbtt_pack_context spc;

        StbTrueType.stbtt_pack_range[] packRanges = new StbTrueType.stbtt_pack_range[1];
        packRanges[0].font_size = fontSize;
        packRanges[0].first_unicode_codepoint_in_range = rangeFrom;
        packRanges[0].num_chars = rangeSize;
        packRanges[0].chardata_for_range = new StbTrueType.stbtt_packedchar[rangeSize];

        StbTrueType.stbtt_PackBegin(out spc, fontBitmap, fontBitmapSize, fontBitmapSize, 0, 1);

        StbTrueType.stbtt_PackSetOversampling(ref spc, 1, 1);
        StbTrueType.stbtt_PackSetSkipMissingCodepoints(ref spc, true);

        StbTrueType.stbtt_PackFontRanges(ref spc, fontBytes, 0, packRanges, 1);

        StbTrueType.stbtt_PackEnd(ref spc);

        fontCharData = packRanges[0].chardata_for_range;

        fontTexture = SDL.CreateTexture(renderer, SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Static, fontBitmapSize, fontBitmapSize);

        if (fontTexture == 0)
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to create texture: {SDL.GetError()}");
            return;
        }

        SDL.SetTextureScaleMode(fontTexture, SDL.ScaleMode.Nearest);

        byte[] fontPixels = new byte[fontBitmapSize * fontBitmapSize * 4];

        int fontBitmapOffset = 0;
        int fontTextureOffset = 0;

        for (int y = 0; y < fontBitmapSize; y++)
        {
            for (int x = 0; x < fontBitmapSize; x++)
            {
                byte pixel = fontBitmap[fontBitmapOffset];

                fontPixels[fontTextureOffset + 0] = pixel;
                fontPixels[fontTextureOffset + 1] = pixel;
                fontPixels[fontTextureOffset + 2] = pixel;
                fontPixels[fontTextureOffset + 3] = pixel;

                fontBitmapOffset++;
                fontTextureOffset += 4;
            }
        }

        if (!SDL.UpdateTexture(fontTexture, new SDL.Rect() { W = fontBitmapSize, H = fontBitmapSize, }, fontPixels, fontBitmapSize * 4))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to update texture: {SDL.GetError()}");
            return;
        }
    }

    public StbGui.stbg_size MeasureText(ReadOnlySpan<char> text, StbGui.stbg_font_style style)
    {
        float scale = style.size / FontSize;

        int ch = 0;
        float xpos = 0;
        int lines = 1;
        while (ch < text.Length)
        {
            char c = text[ch];

            var charData = fontCharData[c];

            float dx = charData.xadvance * scale;
            if (ch + 1 < text.Length)
                dx += fontScale * StbTrueType.stbtt_GetCodepointKernAdvance(ref font, text[ch], text[ch + 1]) * scale;

            /*if (xpos + dx > bounds.x1)
            {
                ypos += FontSize * scale;
                xpos = bounds.x0;
            }*/

            xpos += dx;

            ++ch;
        }

        return StbGui.stbg_build_size(xpos, FontSize * scale * lines);
    }


    public void DrawText(ReadOnlySpan<char> text, StbGui.stbg_font_style style, StbGui.stbg_rect bounds)
    {
        float scale = style.size / FontSize;

        int ch = 0;
        float xpos = bounds.x0;
        float ypos = bounds.y0;

        SDL.SetTextureColorMod(fontTexture, style.color.r, style.color.g, style.color.b);

        bool firstLine = true;

        while (ch < text.Length)
        {
            char c = text[ch];

            var charData = fontCharData[c];

            float dx = charData.xadvance * scale;
            if (ch + 1 < text.Length)
                dx += fontScale * StbTrueType.stbtt_GetCodepointKernAdvance(ref font, text[ch], text[ch + 1]) * scale;

            if (xpos + dx > bounds.x1 && !firstLine)
            {
                ypos += FontSize * scale;
                xpos = bounds.x0;
            }

            var fromRect = new SDL.FRect() { X = charData.x0, Y = charData.y0, W = charData.x1 - charData.x0, H = charData.y1 - charData.y0 };
            var toRect = new SDL.FRect() { X = xpos + charData.xoff * scale, Y = ypos + (FontSize + charData.yoff) * scale, W = (charData.x1 - charData.x0) * scale, H = (charData.y1 - charData.y0) * scale };

            if (!SDL.RenderTexture(renderer, fontTexture, fromRect, toRect))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to render texture: {SDL.GetError()}");
                return;
            }

            firstLine = false;

            xpos += dx;

            ++ch;
        }
    }
}