using System.Diagnostics.CodeAnalysis;
using SDL3;

namespace StbSharp.Examples;

public class SDLFont : IDisposable
{
    public readonly string Name;
    public readonly float Size = 13;

    private StbTrueType.stbtt_fontinfo font;
    private float fontScale;

    public readonly float Ascent;
    public readonly float Descent;
    public readonly float LineGap;
    public readonly float LineHeight;
    public readonly float Baseline;

    const int FONT_BITMAP_SIZE = 512;
    private nint fontTexture;
    private StbTrueType.stbtt_packedchar[] fontCharData;

    private nint renderer;

    public SDLFont(string name, string fileName, float fontSize, nint renderer)
    {
        Name = name;
        Size = fontSize;
        this.renderer = renderer;

        byte[] fontBytes = File.ReadAllBytes(fileName);
        StbTrueType.stbtt_InitFont(out font, fontBytes, 0);

        fontScale = StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize);
        StbTrueType.stbtt_GetFontVMetrics(ref font, out int ascent, out int descent, out int lineGap);

        Ascent = ascent * fontScale;
        Descent = descent * fontScale;
        LineGap = lineGap * fontScale;
        LineHeight = (ascent - descent + lineGap) * fontScale;

        Baseline = (int)Ascent;

        InitFontTexture(fontBytes);
    }

    [MemberNotNull(nameof(fontCharData))]
    private void InitFontTexture(byte[] fontBytes)
    {
        var fontBitmap = new byte[FONT_BITMAP_SIZE * FONT_BITMAP_SIZE];

        int rangeFrom = 0;
        int rangeTo = 255;

        int rangeSize = rangeTo - rangeFrom;

        StbTrueType.stbtt_pack_range[] packRanges = new StbTrueType.stbtt_pack_range[1];
        packRanges[0].font_size = Size;
        packRanges[0].first_unicode_codepoint_in_range = rangeFrom;
        packRanges[0].num_chars = rangeSize;
        packRanges[0].chardata_for_range = new StbTrueType.stbtt_packedchar[rangeSize];

        StbTrueType.stbtt_PackBegin(out var spc, fontBitmap, FONT_BITMAP_SIZE, FONT_BITMAP_SIZE, 0, 1);

        StbTrueType.stbtt_PackSetOversampling(ref spc, 1, 1);
        StbTrueType.stbtt_PackSetSkipMissingCodepoints(ref spc, true);

        StbTrueType.stbtt_PackFontRanges(ref spc, fontBytes, 0, packRanges, 1);

        StbTrueType.stbtt_PackEnd(ref spc);

        fontCharData = packRanges[0].chardata_for_range;

        byte[] fontPixels = new byte[FONT_BITMAP_SIZE * FONT_BITMAP_SIZE * 4];

        int fontBitmapOffset = 0;
        int fontTextureOffset = 0;

        for (int y = 0; y < FONT_BITMAP_SIZE; y++)
        {
            for (int x = 0; x < FONT_BITMAP_SIZE; x++)
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

        fontTexture = SDL.CreateTexture(renderer, SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Static, FONT_BITMAP_SIZE, FONT_BITMAP_SIZE);

        if (fontTexture == 0)
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to create texture: {SDL.GetError()}");
            return;
        }

        SDL.SetTextureScaleMode(fontTexture, SDL.ScaleMode.Nearest);

        if (!SDL.UpdateTexture(fontTexture, new SDL.Rect() { W = FONT_BITMAP_SIZE, H = FONT_BITMAP_SIZE, }, fontPixels, FONT_BITMAP_SIZE * 4))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to update texture: {SDL.GetError()}");
            return;
        }
    }

    public StbGui.stbg_size MeasureText(ReadOnlySpan<char> text, StbGui.stbg_font_style style, bool use_baseline = false)
    {
        float scale = style.size / Size;

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
                lines++;
            }*/

            xpos += dx;

            ++ch;
        }

        return StbGui.stbg_build_size(
            xpos,
            use_baseline ?
                (Baseline + (lines - 1) * LineHeight) * scale :
                LineHeight * lines * scale
        );
    }

    public void DrawText(ReadOnlySpan<char> text, StbGui.stbg_font_style style, StbGui.stbg_rect bounds, float horizontal_alignment, float vertical_alignment)
    {
        float scale = style.size / Size;

        var bounds_width = (bounds.x1 - bounds.x0) * scale;
        var bounds_height = (bounds.y1 - bounds.y0) * scale;

        if (bounds_width <= 0 || bounds_height <= 0)
            return;

        var full_text_bounds = MeasureText(text, style, true);

        var center_y_offset = full_text_bounds.height < bounds_height ?
            MathF.Floor(((bounds_height - full_text_bounds.height) / 2) * (1 + vertical_alignment)) :
            0;

        var center_x_offset = full_text_bounds.width < bounds_width ?
            MathF.Floor(((bounds_width - full_text_bounds.width) / 2) * (1 + horizontal_alignment)) :
            0;

        var use_clipping = false;

        if (full_text_bounds.height > bounds_height ||
            full_text_bounds.width > bounds_width)
        {
            use_clipping = true;
        }

        if (use_clipping)
        {
            SDLHelper.PushClipRect(renderer, bounds);
        }

        int ch = 0;
        float xpos = bounds.x0 + center_x_offset;
        float ypos = bounds.y0 + center_y_offset;

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
                ypos += LineHeight * scale;
                xpos = bounds.x0 + center_x_offset;

                if (ypos + LineHeight * scale > bounds.y1)
                    break;
            }

            var fromRect = new SDL.FRect() { X = charData.x0, Y = charData.y0, W = charData.x1 - charData.x0, H = charData.y1 - charData.y0 };
            var toRect = new SDL.FRect() { X = xpos + charData.xoff * scale, Y = ypos + (Baseline + charData.yoff) * scale, W = (charData.x1 - charData.x0) * scale, H = (charData.y1 - charData.y0) * scale };

            if (!SDL.RenderTexture(renderer, fontTexture, fromRect, toRect))
            {
                SDL.LogError(SDL.LogCategory.System, $"SDL failed to render texture: {SDL.GetError()}");
                break;
            }

            firstLine = false;

            xpos += dx;

            ++ch;
        }

        if (use_clipping)
        {
            SDLHelper.PopClipRect(renderer);
        }

    }

    public void Dispose()
    {
        if (fontTexture != 0)
        {
            SDL.DestroyTexture(fontTexture);
            fontTexture = 0;
        }
    }
}