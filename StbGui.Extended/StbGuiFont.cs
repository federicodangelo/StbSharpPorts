namespace StbSharp;

public record class StbGuiFont : IDisposable
{
    public readonly string name;
    public readonly float size;
    public readonly float ascent;
    public readonly float descent;
    public readonly float line_gap;
    public readonly float line_height;
    public readonly float baseline;
    public readonly StbTrueType.stbtt_packedchar[] font_char_data;
    public readonly float font_scale;
    public readonly int oversampling;
    public readonly float oversampling_scale;
    public readonly nint texture_id;

    private StbTrueType.stbtt_fontinfo font_info;
    private readonly StbGuiRenderAdapter render_adapter;

    public StbGuiFont(string name, string filename, float font_size, int oversampling, bool use_bilinear_filtering, StbGuiRenderAdapter render_adapter)
        : this(name, File.ReadAllBytes(filename), font_size, oversampling, use_bilinear_filtering, render_adapter)
    {

    }

    public StbGuiFont(string name, byte[] font_bytes, float font_size, int oversampling, bool use_bilinear_filtering, StbGuiRenderAdapter render_adapter)
    {
        Console.WriteLine($"Loading font {name} ({font_size})");

        this.render_adapter = render_adapter;

        this.name = name;
        this.size = font_size;

        this.oversampling = oversampling;
        this.oversampling_scale = 1.0f / (float)oversampling;

        StbTrueType.stbtt_InitFont(out font_info, font_bytes, 0);

        font_scale = StbTrueType.stbtt_ScaleForPixelHeight(ref font_info, font_size);
        StbTrueType.stbtt_GetFontVMetrics(ref font_info, out int ascent, out int descent, out int lineGap);

        this.ascent = ascent * font_scale;
        this.descent = descent * font_scale;
        line_gap = lineGap * font_scale;
        line_height = (ascent - descent + lineGap) * font_scale;

        baseline = (int)this.ascent;

        CreateFontTexture(font_bytes, use_bilinear_filtering, out texture_id, out font_char_data);
    }

    private void CreateFontTexture(byte[] fontBytes, bool use_bilinear_filtering, out nint texture_id, out StbTrueType.stbtt_packedchar[] font_char_data)
    {
        var width = 512;
        var height = 512;

        var fontBitmap = new byte[width * height];

        int rangeFrom = 0;
        int rangeTo = 255;

        int rangeSize = rangeTo - rangeFrom;

        StbTrueType.stbtt_pack_range[] packRanges = new StbTrueType.stbtt_pack_range[1];
        packRanges[0].font_size = size;
        packRanges[0].first_unicode_codepoint_in_range = rangeFrom;
        packRanges[0].num_chars = rangeSize;
        packRanges[0].chardata_for_range = new StbTrueType.stbtt_packedchar[rangeSize];

        StbTrueType.stbtt_PackBegin(out var spc, fontBitmap, width, height, 0, 1);

        StbTrueType.stbtt_PackSetOversampling(ref spc, (uint)oversampling, (uint)oversampling);
        StbTrueType.stbtt_PackSetSkipMissingCodepoints(ref spc, true);

        StbTrueType.stbtt_PackFontRanges(ref spc, fontBytes, 0, packRanges, 1);

        StbTrueType.stbtt_PackEnd(ref spc);

        font_char_data = packRanges[0].chardata_for_range;

        var pixels = new byte[width * height * 4];

        int fontBitmapOffset = 0;
        int fontTextureOffset = 0;

        int non_zero_pixels = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte pixel = fontBitmap[fontBitmapOffset];

                if (pixel != 0)
                    non_zero_pixels++;

                pixels[fontTextureOffset + 0] = pixel;
                pixels[fontTextureOffset + 1] = pixel;
                pixels[fontTextureOffset + 2] = pixel;
                pixels[fontTextureOffset + 3] = pixel;

                fontBitmapOffset++;
                fontTextureOffset += 4;
            }
        }

        texture_id = render_adapter.create_texture(width, height,
            pixels,
            4,
            new StbGuiRenderAdapter.CreateTextureOptions()
            {
                use_bilinear_filtering = use_bilinear_filtering
            }
        );
    }

    public int GetCodepointKernAdvance(int ch1, int ch2)
    {
        return StbTrueType.stbtt_GetCodepointKernAdvance(ref font_info, ch1, ch2);
    }


    public void Dispose()
    {
        if (texture_id != 0)
        {
            render_adapter.destroy_texture(texture_id);
        }
    }
}
