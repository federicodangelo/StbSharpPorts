using System.Diagnostics.CodeAnalysis;

namespace StbSharp;

public class StbGuiFont : IDisposable
{
    public readonly string Name;
    public readonly float Size = 13;

    public readonly float Ascent;
    public readonly float Descent;
    public readonly float LineGap;
    public readonly float LineHeight;
    public readonly float Baseline;

    public StbTrueType.stbtt_fontinfo font_info;
    public float font_scale { get; private set; }
    public int oversampling { get; private set; } = 1;
    public float oversampling_scale { get; private set; } = 1.0f;
    public StbTrueType.stbtt_packedchar[] font_char_data { get; private set; }
    public nint texture_id { get; private set; }

    private StbGuiRenderAdapter render_adapter;

    public StbGuiFont(string name, string fileName, float fontSize, int oversampling, bool use_bilinear_filtering, StbGuiRenderAdapter render_adapter)
    {
        this.render_adapter = render_adapter;

        Name = name;
        Size = fontSize;

        this.oversampling = oversampling;
        this.oversampling_scale = 1.0f / (float)oversampling;

        byte[] fontBytes = File.ReadAllBytes(fileName);
        StbTrueType.stbtt_InitFont(out font_info, fontBytes, 0);

        font_scale = StbTrueType.stbtt_ScaleForPixelHeight(ref font_info, fontSize);
        StbTrueType.stbtt_GetFontVMetrics(ref font_info, out int ascent, out int descent, out int lineGap);

        Ascent = ascent * font_scale;
        Descent = descent * font_scale;
        LineGap = lineGap * font_scale;
        LineHeight = (ascent - descent + lineGap) * font_scale;

        Baseline = (int)Ascent;

        CreateFontTexture(fontBytes, use_bilinear_filtering);
    }

    [MemberNotNull(nameof(font_char_data))]
    private void CreateFontTexture(byte[] fontBytes, bool use_bilinear_filtering)
    {
        var width = 512;
        var height = 512;

        var fontBitmap = new byte[width * height];

        int rangeFrom = 0;
        int rangeTo = 255;

        int rangeSize = rangeTo - rangeFrom;

        StbTrueType.stbtt_pack_range[] packRanges = new StbTrueType.stbtt_pack_range[1];
        packRanges[0].font_size = Size;
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

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte pixel = fontBitmap[fontBitmapOffset];

                pixels[fontTextureOffset + 0] = pixel;
                pixels[fontTextureOffset + 1] = pixel;
                pixels[fontTextureOffset + 2] = pixel;
                pixels[fontTextureOffset + 3] = pixel;

                fontBitmapOffset++;
                fontTextureOffset += 4;
            }
        }

        texture_id = render_adapter.create_texture(width, height,
            new StbGuiRenderAdapter.CreateTextureOptions()
            {
                use_bilinear_filtering = use_bilinear_filtering
            }
        );

        render_adapter.set_texture_pixels(texture_id, StbGui.stbg_build_size(width, height), pixels);
    }

    public void Dispose()
    {
        if (texture_id != 0)
        {
            render_adapter.destroy_texture(texture_id);
            texture_id = 0;
        }
    }
}