#pragma warning disable IDE1006 // Naming Styles

using System.Diagnostics.CodeAnalysis;

namespace StbSharp;

public partial class StbGui
{
    [ExcludeFromCodeCoverage]
    private static stbg_widget_intrinsic_size stbg__build_intrinsic_size_text()
    {
        return new stbg_widget_intrinsic_size()
        {
            type = STBG_INTRINSIC_SIZE_TYPE.MEASURE_TEXT,
        };
    }

    [ExcludeFromCodeCoverage]
    private static stbg_widget_intrinsic_size stbg__build_intrinsic_size_pixels(float width, float height)
    {
        return new stbg_widget_intrinsic_size()
        {
            type = STBG_INTRINSIC_SIZE_TYPE.FIXED_PIXELS,
            size = new stbg_size() { width = width, height = height },
        };
    }

    [ExcludeFromCodeCoverage]
    private static stbg_widget_constrains stbg__build_constrains_unconstrained()
    {
        return new stbg_widget_constrains()
        {
            min = new stbg_size() { width = 0, height = 0 },
            max = new stbg_size() { width = float.MaxValue, height = float.MaxValue },
        };
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1)
    {
        return
            (float)context.theme.styles[(int)style1];
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2)
    {
        return
            (float)context.theme.styles[(int)style1] +
            (float)context.theme.styles[(int)style2];
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3)
    {
        return
            (float)context.theme.styles[(int)style1] +
            (float)context.theme.styles[(int)style2] +
            (float)context.theme.styles[(int)style3];
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3, STBG_WIDGET_STYLE style4)
    {
        return
            (float)context.theme.styles[(int)style1] +
            (float)context.theme.styles[(int)style2] +
            (float)context.theme.styles[(int)style3] +
            (float)context.theme.styles[(int)style4];
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3, STBG_WIDGET_STYLE style4, STBG_WIDGET_STYLE style5)
    {
        return
            (float)context.theme.styles[(int)style1] +
            (float)context.theme.styles[(int)style2] +
            (float)context.theme.styles[(int)style3] +
            (float)context.theme.styles[(int)style4] +
            (float)context.theme.styles[(int)style5];
    }

    [ExcludeFromCodeCoverage]
    static public stbg_color stbg_build_color(byte r, byte g, byte b, byte a = 255) => new stbg_color() { r = r, g = g, b = b, a = a };

    [ExcludeFromCodeCoverage]
    static public uint stbg_color_to_uint(stbg_color color) => (((uint)color.a) << 24) | (((uint)color.r) << 16) | (((uint)color.g) << 8) | (((uint)color.b) << 0);

    [ExcludeFromCodeCoverage]
    static public stbg_color stbg_uint_to_color(uint color) => new stbg_color() { r = (byte)((color >> 16) & 0xFF), g = (byte)((color >> 8) & 0xFF), b = (byte)((color >> 0) & 0xFF), a = (byte)((color >> 24) & 0xFF) };

    [ExcludeFromCodeCoverage]
    static public stbg_rect stbg_build_rect(float x0, float y0, float x1, float y1) => new stbg_rect() { top_left = new stbg_position() { x = x0, y = y0 }, bottom_right = new stbg_position() { x = x1, y = y1 } };

    [ExcludeFromCodeCoverage]
    static public stbg_position stbg_build_position(float x, float y) => new stbg_position() { x = x, y = y };

    static public readonly stbg_color STBG_COLOR_RED = stbg_build_color(255, 0, 0);
    static public readonly stbg_color STBG_COLOR_GREEN = stbg_build_color(0, 255, 0);
    static public readonly stbg_color STBG_COLOR_BLUE = stbg_build_color(0, 0, 255);
    static public readonly stbg_color STBG_COLOR_YELLOW = stbg_build_color(255, 255, 0);
    static public readonly stbg_color STBG_COLOR_CYAN = stbg_build_color(0, 255, 255);
    static public readonly stbg_color STBG_COLOR_MAGENTA = stbg_build_color(255, 0, 255);
    static public readonly stbg_color STBG_COLOR_WHITE = stbg_build_color(255, 255, 255);
    static public readonly stbg_color STBG_COLOR_GRAY = stbg_build_color(128, 128, 128);
    static public readonly stbg_color STBG_COLOR_BLACK = stbg_build_color(0, 0, 0);
    static public readonly stbg_color STBG_COLOR_TRANSPARENT = stbg_build_color(0, 0, 0, 0);
}