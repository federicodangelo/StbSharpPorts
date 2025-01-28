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
        public byte[] bitmap;
        public int bitmapWidth;
        public int bitmapHeight;

        // Indexed from FontOptions.rangeFrom
        public StbTrueType.stbtt_bakedchar[] cdata;

        public int addedCharacters;
    }

    static public BuildFontBitmapOutput? BuildFontBitmap(FontOptions? fontOptions = null, BitmapOptions? bitmapOptions = null)
    {
        if (fontOptions == null)
            fontOptions = new FontOptions();

        if (bitmapOptions == null)
            bitmapOptions = new BitmapOptions();

        var fontBytes = File.ReadAllBytes(fontOptions.fontFilename);

        int rangeSize = fontOptions.rangeTo - fontOptions.rangeFrom;

        StbTrueType.stbtt_bakedchar[] cdata = new StbTrueType.stbtt_bakedchar[rangeSize];

        byte[] bitmap = new byte[bitmapOptions.outputBitmapWidth * bitmapOptions.outputBitmapHeight];


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