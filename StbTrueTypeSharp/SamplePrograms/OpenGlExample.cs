namespace StbTrueTypeSharp;

//  Incomplete text-in-3d-api example, which draws quads properly aligned to be lossless.
//  See "tests/truetype_demo_win32.c" for a complete version.
static public class OpenGlExample
{
    static private byte[] ttf_buffer;
    static private byte[] temp_bitmap = new byte[512 * 512];

    static private StbTrueType.stbtt_bakedchar[] cdata = new StbTrueType.stbtt_bakedchar[96]; // ASCII 32..126 is 95 glyphs

    static private uint ftex;

    static private void my_stbtt_initfont()
    {
        ttf_buffer = File.ReadAllBytes("c:/windows/fonts/times.ttf");

        StbTrueType.stbtt_BakeFontBitmap(ttf_buffer, 0, 32.0f, temp_bitmap, 512, 512, 32, 96, cdata); // no guarantee this fits!
                                                                                         // can free ttf_buffer at this point

        //glGenTextures(1, &ftex);
        //glBindTexture(GL_TEXTURE_2D, ftex);
        //glTexImage2D(GL_TEXTURE_2D, 0, GL_ALPHA, 512, 512, 0, GL_ALPHA, GL_UNSIGNED_BYTE, temp_bitmap);

        // can free temp_bitmap at this point
        //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    }

    static private void my_stbtt_print(float x, float y, ReadOnlySpan<char> text)
    {
        // assume orthographic projection with units = screen pixels, origin at top left
        //glEnable(GL_BLEND);
        //glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        //glEnable(GL_TEXTURE_2D);
        //glBindTexture(GL_TEXTURE_2D, ftex);
        //glBegin(GL_QUADS);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c >= 32 && c < 128)
            {
                StbTrueType.stbtt_aligned_quad q;
                StbTrueType.stbtt_GetBakedQuad(cdata, 512, 512, c - 32, ref x, ref y, out q, 1);//1=opengl & d3d10+,0=d3d9

                //glTexCoord2f(q.s0, q.t0); glVertex2f(q.x0, q.y0);
                //glTexCoord2f(q.s1, q.t0); glVertex2f(q.x1, q.y0);
                //glTexCoord2f(q.s1, q.t1); glVertex2f(q.x1, q.y1);
                //glTexCoord2f(q.s0, q.t1); glVertex2f(q.x0, q.y1);
            }
        }

        //glEnd();
    }
}