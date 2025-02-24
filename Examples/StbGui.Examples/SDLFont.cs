using System.Diagnostics;
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

    private bool use_bilinear_filtering = false;
    private int oversampling = 1;
    private float oversampling_scale = 1.0f;

    const int FONT_BITMAP_SIZE = 512;
    private nint fontTexture;
    private StbTrueType.stbtt_packedchar[] fontCharData;

    private nint renderer;

    public SDLFont(string name, string fileName, float fontSize, int oversampling, bool use_bilinear_filtering, nint renderer)
    {
        Name = name;
        Size = fontSize;
        this.renderer = renderer;

        this.oversampling = oversampling;
        this.oversampling_scale = 1.0f / (float)oversampling;
        this.use_bilinear_filtering = use_bilinear_filtering;

        byte[] fontBytes = File.ReadAllBytes(fileName);
        StbTrueType.stbtt_InitFont(out font, fontBytes, 0);

        fontScale = StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize);
        StbTrueType.stbtt_GetFontVMetrics(ref font, out int ascent, out int descent, out int lineGap);

        Debug.WriteLine($"Font: {fileName}, Size: {fontSize}, Scale: {fontScale} Ascent: {ascent}, Descent: {descent}, LineGap: {lineGap}");

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

        StbTrueType.stbtt_PackSetOversampling(ref spc, (uint)oversampling, (uint)oversampling);
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

        if (use_bilinear_filtering)
            SDL.SetTextureScaleMode(fontTexture, SDL.ScaleMode.Linear);
        else
            SDL.SetTextureScaleMode(fontTexture, SDL.ScaleMode.Nearest);

        if (fontTexture == 0)
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to create texture: {SDL.GetError()}");
            return;
        }

        if (!SDL.UpdateTexture(fontTexture, new SDL.Rect() { W = FONT_BITMAP_SIZE, H = FONT_BITMAP_SIZE, }, fontPixels, FONT_BITMAP_SIZE * 4))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL failed to update texture: {SDL.GetError()}");
            return;
        }
    }

    private ref struct TextIterationData
    {
        public float scale;
        public char c;
        public int c_index;
        public ref StbTrueType.stbtt_packedchar c_data;
        public float x;
        public float y;
        public float dx;
        public float dy;
        public int line_number;
        public bool new_line;
        public bool final;
    }

    private delegate bool IterateTextInternalDelegate(ref TextIterationData data);

    private void IterateTextInternal(
        ReadOnlySpan<char> text, StbGui.stbg_font_style style,
        StbGui.STBG_MEASURE_TEXT_OPTIONS options,
        IterateTextInternalDelegate callback)
    {
        TextIterationData iteration_data = new TextIterationData();
        iteration_data.scale = style.size / Size;

        float current_line_height = 0;

        var ignore_metrics = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS) != 0;
        var use_only_baseline_for_first_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE) != 0;
        var single_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE) != 0;

        float get_line_height(ref TextIterationData data)
        {
            return (ignore_metrics && current_line_height != 0) ? current_line_height :
                (use_only_baseline_for_first_line && data.line_number == 0) ? Baseline * data.scale :
                LineHeight * data.scale;
        }

        bool stop = false;

        while (iteration_data.c_index < text.Length && !stop)
        {
            iteration_data.c = text[iteration_data.c_index];
            iteration_data.new_line = false;

            if (iteration_data.c == '\n')
            {
                if (!single_line)
                {
                    iteration_data.new_line = true;
                    iteration_data.dx = 0;
                    iteration_data.dy = get_line_height(ref iteration_data);
                }
                else
                {
                    iteration_data.dx = 0;
                    iteration_data.dy = 0;
                    // ignore the new line if we are in a single line scenario
                }
            }
            else
            {
                iteration_data.c_data = ref fontCharData[iteration_data.c];

                if (ignore_metrics)
                {
                    current_line_height = Math.Max(current_line_height, (iteration_data.c_data.y1 - iteration_data.c_data.y0) * iteration_data.scale * oversampling_scale);
                    iteration_data.dx = ((iteration_data.c_data.x1 - iteration_data.c_data.x0) + 1) * iteration_data.scale * oversampling_scale;
                }
                else
                {
                    iteration_data.dx = iteration_data.c_data.xadvance * iteration_data.scale;
                    if (iteration_data.c_index + 1 < text.Length)
                        iteration_data.dx += fontScale * StbTrueType.stbtt_GetCodepointKernAdvance(ref font, iteration_data.c, text[iteration_data.c_index + 1]) * iteration_data.scale;
                }

                iteration_data.dy = 0;
            }

            if (iteration_data.new_line)
            {
                iteration_data.y += iteration_data.dy;
                iteration_data.x = 0;

                current_line_height = 0;

                iteration_data.line_number++;
            }

            stop = callback(ref iteration_data);

            iteration_data.x += iteration_data.dx;
            iteration_data.y += iteration_data.dy;

            iteration_data.c_index++;
        }

        if (!stop)
        {
            // Call callback one last time with the final X/Y position 
            var last_line_height = get_line_height(ref iteration_data);
            iteration_data.final = true;
            iteration_data.y += last_line_height;
            iteration_data.dy = last_line_height;
            iteration_data.dx = 0;
            iteration_data.new_line = false;

            callback(ref iteration_data);
        }
    }


    public StbGui.stbg_size MeasureText(ReadOnlySpan<char> text, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options)
    {
        float width = 0, height = 0;

        IterateTextInternal(text, style, options, (ref TextIterationData data) =>
        {
            if (data.new_line || data.final)
            {
                width = Math.Max(width, data.x);
                height += data.dy;
            }

            return false;
        });

        return StbGui.stbg_build_size(width, height);
    }

    public StbGui.stbg_position GetCharacterPositionInText(ReadOnlySpan<char> text, StbGui.stbg_font_style style, StbGui.STBG_MEASURE_TEXT_OPTIONS options, int character_index)
    {
        StbGui.stbg_position position = StbGui.stbg_build_position_zero();

        IterateTextInternal(text, style, options, (ref TextIterationData data) =>
        {
            if (data.c_index == character_index)
            {
                position.x = data.x;
                position.y = data.y;
                return true;
            }

            return false;
        });

        return position;
    }

    public void DrawText(ReadOnlySpan<char> text, StbGui.stbg_font_style style, StbGui.stbg_rect bounds, float horizontal_alignment, float vertical_alignment, StbGui.STBG_MEASURE_TEXT_OPTIONS measure_options, StbGui.STBG_RENDER_TEXT_OPTIONS render_options)
    {
        float scale = style.size / Size;

        var bounds_width = bounds.x1 - bounds.x0;
        var bounds_height = bounds.y1 - bounds.y0;

        if (bounds_width <= 0 || bounds_height <= 0)
            return;

        var full_text_bounds = MeasureText(text, style, measure_options | StbGui.STBG_MEASURE_TEXT_OPTIONS.USE_ONLY_BASELINE_FOR_FIRST_LINE);

        bool dont_clip = (render_options & StbGui.STBG_RENDER_TEXT_OPTIONS.DONT_CLIP) != 0;

        var center_y_offset = full_text_bounds.height < bounds_height ?
            MathF.Floor(((bounds_height - full_text_bounds.height) / 2) * (1 + vertical_alignment)) :
            0;

        var center_x_offset = full_text_bounds.width < bounds_width ?
            MathF.Floor(((bounds_width - full_text_bounds.width) / 2) * (1 + horizontal_alignment)) :
            0;

        var ignore_metrics = (measure_options & StbGui.STBG_MEASURE_TEXT_OPTIONS.IGNORE_METRICS) != 0;
        var use_clipping = !dont_clip && (full_text_bounds.height > bounds_height || full_text_bounds.width > bounds_width);

        if (use_clipping)
        {
            SDLHelper.PushClipRect(renderer, bounds);
        }

        SDL.SetTextureColorMod(fontTexture, style.color.r, style.color.g, style.color.b);

        IterateTextInternal(text, style, measure_options, (ref TextIterationData data) =>
        {
            if (!data.new_line && !data.final)
            {
                ref var char_data = ref data.c_data;

                var metricsX = (ignore_metrics ? 0 : char_data.xoff) * scale;
                var metricsY = (ignore_metrics ? 0 : (Baseline + char_data.yoff)) * scale;

                float xpos = bounds.x0 + center_x_offset + data.x;
                float ypos = bounds.y0 + center_y_offset + data.y;

                var fromRect = new SDL.FRect() { X = char_data.x0, Y = char_data.y0, W = char_data.x1 - char_data.x0, H = char_data.y1 - char_data.y0 };
                var toRect = new SDL.FRect() { X = xpos + metricsX, Y = ypos + metricsY, W = (char_data.x1 - char_data.x0) * scale * oversampling_scale, H = (char_data.y1 - char_data.y0) * scale * oversampling_scale };

                if (!SDL.RenderTexture(renderer, fontTexture, fromRect, toRect))
                {
                    SDL.LogError(SDL.LogCategory.System, $"SDL failed to render texture: {SDL.GetError()}");
                    return true;
                }
            }

            return false;
        });

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