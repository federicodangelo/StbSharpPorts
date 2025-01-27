namespace StbTrueTypeSharp;

//////////////////////////////////////////////////////////////////////////////
//
// Complete program (this compiles): get a single bitmap, print as ASCII art
//
static public class PrintAsciiArt
{
    static public int Main(string[] args)
    {
        StbTrueType.stbtt_fontinfo font;
        byte[] bitmap;

        int c = args.Length > 1 ? int.Parse(args[1]) : 'a';
        int s = args.Length > 2 ? int.Parse(args[2]) : 20;
        string fileName = args.Length > 3 ? args[3] : "c:/windows/fonts/arialbd.ttf";

        var ttf_buffer = File.ReadAllBytes(fileName);;

        StbTrueType.stbtt_InitFont(out font, ttf_buffer, StbTrueType.stbtt_GetFontOffsetForIndex(ttf_buffer, 0));

        bitmap = StbTrueType.stbtt_GetCodepointBitmap(ref font, 0, StbTrueType.stbtt_ScaleForPixelHeight(ref font, s), c, out int w, out int h, 0, 0);

        for (var j = 0; j < h; ++j)
        {
            string line = "";

            for (var i = 0; i < w; ++i)
                line += " .:ioVM@"[bitmap[j * w + i] >> 5];

            Console.WriteLine(line);
        }
        return 0;
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