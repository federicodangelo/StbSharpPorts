using StbSharp;
using StbSharp.StbCommon;

namespace StbSharp.Examples;

//  Incomplete text-in-3d-api example, which draws quads properly aligned to be lossless.
//  See "tests/truetype_demo_win32.c" for a complete version.

static public class BuildFontBitmapExample
{
    public class FontOptions
    {
        public string fontFilename = "Fonts/Karla-Regular.ttf";
        public float fontSize = 13;
        public int rangeFrom = 0x20;
        public int rangeTo = 0xFF;
    }

    public class BitmapOptions
    {
        public int outputBitmapWidth = 512;
        public int outputBitmapHeight = 512;
    }

    public class BuildFontBitmapOutput
    {
        public byte[] bitmap = [];
        public int bitmapWidth;
        public int bitmapHeight;

        // Indexed from FontOptions.rangeFrom
        public StbTrueType.stbtt_bakedchar[] cdata = [];

        public int addedCharacters;

        public BuildFontBitmapOutput()
        {

        }
    }

    public enum PackFormat
    {
        Fast,
        Better,
    }

    static public BuildFontBitmapOutput? BuildFontBitmap(FontOptions fontOptions, BitmapOptions bitmapOptions, PackFormat packFormat)
    {
        var fontBytes = File.ReadAllBytes(fontOptions.fontFilename);

        switch (packFormat)
        {
            case PackFormat.Fast:
                return PackFast(fontOptions, bitmapOptions, fontBytes);

            default:
            case PackFormat.Better:
                return PackBetter(fontOptions, bitmapOptions, fontBytes);
        }

    }

    private static BuildFontBitmapOutput? PackBetter(FontOptions fontOptions, BitmapOptions bitmapOptions, byte[] fontBytes)
    {
        byte[] bitmap = new byte[bitmapOptions.outputBitmapWidth * bitmapOptions.outputBitmapHeight];

        int rangeSize = fontOptions.rangeTo - fontOptions.rangeFrom;

        StbTrueType.stbtt_pack_context spc;

        StbTrueType.stbtt_pack_range[] packRanges = new StbTrueType.stbtt_pack_range[1];
        packRanges[0].font_size = fontOptions.fontSize;
        packRanges[0].first_unicode_codepoint_in_range = fontOptions.rangeFrom;
        packRanges[0].num_chars = rangeSize;
        packRanges[0].chardata_for_range = new StbTrueType.stbtt_packedchar[rangeSize];

        StbTrueType.stbtt_PackBegin(out spc, bitmap, bitmapOptions.outputBitmapWidth, bitmapOptions.outputBitmapHeight, 0, 1);

        StbTrueType.stbtt_PackSetOversampling(ref spc, 1, 1);
        StbTrueType.stbtt_PackSetSkipMissingCodepoints(ref spc, true);

        StbTrueType.stbtt_PackFontRanges(ref spc, fontBytes, 0, packRanges, 1);

        StbTrueType.stbtt_PackEnd(ref spc);
        //StbTrueType.stbtt_GetPackedQuad()

        return new BuildFontBitmapOutput
        {
            bitmap = bitmap,
            bitmapWidth = bitmapOptions.outputBitmapWidth,
            bitmapHeight = bitmapOptions.outputBitmapHeight,
            cdata = packRanges[0].chardata_for_range.Select(c => new StbTrueType.stbtt_bakedchar
            {
                x0 = c.x0,
                x1 = c.x1,
                y0 = c.y0,
                y1 = c.y1,

                xoff = c.xoff,
                yoff = c.yoff,
                xadvance = c.xadvance,
            }).ToArray(),
            addedCharacters = rangeSize,
        };
    }

    private static BuildFontBitmapOutput? PackFast(FontOptions fontOptions, BitmapOptions bitmapOptions, byte[] fontBytes)
    {
        byte[] bitmap = new byte[bitmapOptions.outputBitmapWidth * bitmapOptions.outputBitmapHeight];

        int rangeSize = fontOptions.rangeTo - fontOptions.rangeFrom;

        StbTrueType.stbtt_bakedchar[] cdata = new StbTrueType.stbtt_bakedchar[rangeSize];

        // no guarantee this fits!
        // can free ttf_buffer at this point
        int result = StbTrueType.stbtt_BakeFontBitmap(fontBytes, 0, fontOptions.fontSize, bitmap, bitmapOptions.outputBitmapWidth, bitmapOptions.outputBitmapHeight, fontOptions.rangeFrom, rangeSize, cdata);

        if (result == 0)
        {
            return null;
        }

        return new BuildFontBitmapOutput
        {
            bitmap = bitmap,
            bitmapWidth = bitmapOptions.outputBitmapWidth,
            bitmapHeight = bitmapOptions.outputBitmapHeight,
            cdata = cdata,
            addedCharacters = result < 0
                ? -result //if result is negative, returns the negative of the number of characters that fit
                : rangeSize,
        };
    }
}
