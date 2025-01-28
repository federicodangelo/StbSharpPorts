namespace StbTrueTypeSharp;

//////////////////////////////////////////////////////////////////////////////
//
// Complete program: print "Hello World!" banner, with bugs
//
static public class PrintHelloWorld
{
    static public void Run(string[] args)
    {
        const int ScreenWidth = 100;
        const int ScreenHeight = 20;

        BytePtr screen = new BytePtr(new byte[ScreenHeight * ScreenWidth]);

        StbTrueType.stbtt_fontinfo font;
        int baseline, ch = 0;
        float scale, xpos = 2; // leave a little padding in case the character extends left
        ReadOnlySpan<char> text = "Heljo World!"; // intentionally misspelled to show 'lj' brokenness

        byte[] buffer = File.ReadAllBytes("c:/windows/fonts/arialbd.ttf");
        StbTrueType.stbtt_InitFont(out font, buffer, 0);

        scale = StbTrueType.stbtt_ScaleForPixelHeight(ref font, 15);
        StbTrueType.stbtt_GetFontVMetrics(ref font, out int ascent, out _, out int _);
        
        baseline = (int)(ascent * scale);

        while (ch < text.Length)
        {
            int advance, lsb, x0, y0, x1, y1;
            float x_shift = xpos - (float)MathF.Floor(xpos);

            StbTrueType.stbtt_GetCodepointHMetrics(ref font, text[ch], out advance, out lsb);
            StbTrueType.stbtt_GetCodepointBitmapBoxSubpixel(ref font, text[ch], scale, scale, x_shift, 0, out x0, out y0, out x1, out y1);
            StbTrueType.stbtt_MakeCodepointBitmapSubpixel(ref font, screen[(baseline + y0) * ScreenWidth + (int)xpos + x0], x1 - x0, y1 - y0, ScreenWidth, scale, scale, x_shift, 0, text[ch]);
            
            // note that this stomps the old data, so where character boxes overlap (e.g. 'lj') it's wrong
            // because this API is really for baking character bitmaps into textures. if you want to render
            // a sequence of characters, you really need to render each bitmap to a temp buffer, then
            // "alpha blend" that into the working buffer
            xpos += (advance * scale);
            if (ch + 1 < text.Length)
                xpos += scale * StbTrueType.stbtt_GetCodepointKernAdvance(ref font, text[ch], text[ch + 1]);
            ++ch;
        }

        for (int j = 0; j < ScreenHeight; ++j)
        {
            string line = "";
            for (int i = 0; i < ScreenWidth - 1; ++i)
                line += " .:ioVM@"[(screen[j * ScreenWidth + i]) >> 5];
            Console.WriteLine(line);
        }
    }
}
