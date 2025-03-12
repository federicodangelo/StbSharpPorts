using StbSharp;
using StbSharp.StbCommon;

namespace StbSharp.Examples;

//////////////////////////////////////////////////////////////////////////////
//
// Complete program (this compiles): get a single bitmap, print as ASCII art
//
public static class PrintCharacterAsciiArt
{
    public static void Run(char character = 'a', string fileName = "Fonts/Karla-Regular.ttf", int fontSize = 20)
    {
        var ttf_buffer = File.ReadAllBytes(fileName); ;

        if (StbTrueType.stbtt_InitFont(out StbTrueType.stbtt_fontinfo font, ttf_buffer, StbTrueType.stbtt_GetFontOffsetForIndex(ttf_buffer, 0)) == 0)
        {
            Console.Error.WriteLine("Failed to initialize font " + fileName);
            return;
        }

        var bitmap = StbTrueType.stbtt_GetCodepointBitmap(ref font, 0, StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize), character, out int w, out int h, out _, out _);

        if (bitmap.IsNull)
        {
            Console.Error.WriteLine($"Failed to generate bitmap font {fileName}");
            return;
        }

        for (var j = 0; j < h; ++j)
        {
            string line = "";

            if (j == 0)
                Console.WriteLine(new string('-', w));

            for (var i = 0; i < w; ++i)
            {
                //line += bitmap[j * w + i].ToString("X2");

                line += " .:ioVM@"[bitmap[j * w + i].Value >> 5];
            }

            Console.WriteLine(line);

            if (j == h - 1)
                Console.WriteLine(new string('-', w));
        }
    }
}
//
// Output:
//
//     .ii.
//    @@@@@@.
//   V@Mio@@o
//   :i.  V@V
//     :oM@@M
//   :@@@MM@M
//   @@o  o@M
//  :@@.  M@M
//   @@@o@@@@
//   :M@@V:@@.
//
