#pragma warning disable IDE1006 // Naming Styles

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace StbSharp;

public partial class StbGui
{
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static stbg_widget_intrinsic_size stbg__build_intrinsic_size_text()
    {
        return new stbg_widget_intrinsic_size()
        {
            type = STBG_INTRINSIC_SIZE_TYPE.MEASURE_TEXT,
        };
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static stbg_widget_intrinsic_size stbg__build_intrinsic_size_pixels(float width, float height)
    {
        return new stbg_widget_intrinsic_size()
        {
            type = STBG_INTRINSIC_SIZE_TYPE.FIXED_PIXELS,
            size = stbg_build_size(width, height),
        };
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static stbg_widget_constrains stbg_build_constrains_unconstrained()
    {
        return new stbg_widget_constrains()
        {
            min = stbg_build_size_zero(),
            max = stbg_build_size(float.MaxValue, float.MaxValue),
        };
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static stbg_widget_constrains stbg_build_constrains(float minWidth, float minHeight, float maxWidth, float maxHeight)
    {
        return new stbg_widget_constrains()
        {
            min = stbg_build_size(minWidth, minHeight),
            max = stbg_build_size(maxWidth, maxHeight)
        };
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static stbg_widget_constrains stbg_merge_constrains(stbg_widget_constrains constrains1, stbg_widget_constrains constrains2)
    {
        var merged = stbg_build_constrains(
                Math.Max(constrains1.min.width, constrains2.min.width),
                Math.Max(constrains1.min.height, constrains2.min.height),
                Math.Min(constrains1.max.width, constrains2.max.width),
                Math.Min(constrains1.max.height, constrains2.max.height)
        );

        // Ensure that min values are never above max values
        merged.min.width = Math.Min(merged.min.width, merged.max.width);
        merged.min.height = Math.Min(merged.min.height, merged.max.height);

        return merged;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static stbg_widget_constrains stbg_constrains_remove_padding(stbg_widget_constrains constrains, stbg_padding padding)
    {
        constrains.max.width = Math.Max(constrains.max.width - (padding.left + padding.right), 0);
        constrains.max.height = Math.Max(constrains.max.height - (padding.top + padding.bottom), 0);
        constrains.min.width = Math.Min(constrains.min.width, constrains.max.width);
        constrains.min.height = Math.Min(constrains.min.height, constrains.max.height);

        return constrains;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1)
    {
        return
            (float)context.theme.styles[(int)style1];
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2)
    {
        return
            (float)context.theme.styles[(int)style1] +
            (float)context.theme.styles[(int)style2];
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3)
    {
        return
            (float)context.theme.styles[(int)style1] +
            (float)context.theme.styles[(int)style2] +
            (float)context.theme.styles[(int)style3];
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3, STBG_WIDGET_STYLE style4)
    {
        return
            (float)context.theme.styles[(int)style1] +
            (float)context.theme.styles[(int)style2] +
            (float)context.theme.styles[(int)style3] +
            (float)context.theme.styles[(int)style4];
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private stbg_text stbg__build_text(ReadOnlyMemory<char> text)
    {
        return new stbg_text() { text = text, font_id = context.theme.default_font_id, style = context.theme.default_font_style };
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static private stbg_text stbg__build_text(ReadOnlyMemory<char> text, stbg_color color)
    {
        var style = context.theme.default_font_style;
        style.color = color;
        return new stbg_text() { text = text, font_id = context.theme.default_font_id, style = style };
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_color stbg_build_color(byte r, byte g, byte b, byte a = 255) => new stbg_color() { r = r, g = g, b = b, a = a };

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_color rgb(byte r, byte g, byte b, byte a = 255) => stbg_build_color(r, g, b, a);

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public uint stbg_color_to_uint(stbg_color color) => (((uint)color.a) << 24) | (((uint)color.r) << 16) | (((uint)color.g) << 8) | (((uint)color.b) << 0);

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_color stbg_uint_to_color(uint color) => new stbg_color() { r = (byte)((color >> 16) & 0xFF), g = (byte)((color >> 8) & 0xFF), b = (byte)((color >> 0) & 0xFF), a = (byte)((color >> 24) & 0xFF) };

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_rect stbg_build_rect(float x0, float y0, float x1, float y1) => new stbg_rect() { x0 = x0, y0 = y0, x1 = x1, y1 = y1 };

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_rect stbg_build_rect_infinite() => new stbg_rect() { x0 = float.MinValue, y0 = float.MinValue, x1 = float.MaxValue, y1 = float.MaxValue };

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_rect stbg_translate_rect(stbg_rect rect, float dx, float dy)
    {
        rect.x0 += dx;
        rect.y0 += dy;
        rect.x1 += dx;
        rect.y1 += dy;

        return rect;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public bool stbg_rect_is_position_inside(stbg_rect rect, stbg_position position)
    {
        return
            position.x >= rect.x0 && position.x < rect.x1 &&
            position.y >= rect.y0 && position.y < rect.y1;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public bool stbg_rect_is_position_inside(stbg_rect rect, float x, float y)
    {
        return
            x >= rect.x0 && x < rect.x1 &&
            y >= rect.y0 && y < rect.y1;
    }


    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_rect stbg_clamp_rect(stbg_rect rect, stbg_rect must_be_inside_of_rect)
    {
        rect.x0 = Math.Clamp(rect.x0, must_be_inside_of_rect.x0, must_be_inside_of_rect.x1);
        rect.y0 = Math.Clamp(rect.y0, must_be_inside_of_rect.y0, must_be_inside_of_rect.y1);
        rect.x1 = Math.Clamp(rect.x1, must_be_inside_of_rect.x0, must_be_inside_of_rect.x1);
        rect.y1 = Math.Clamp(rect.y1, must_be_inside_of_rect.y0, must_be_inside_of_rect.y1);

        return rect;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_position stbg_build_position(float x, float y) => new stbg_position() { x = x, y = y };

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_position stbg_build_position_zero() => new stbg_position();

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_position stbg_offset_position(stbg_position position, float ox, float oy) => new stbg_position() { x = position.x + ox, y = position.y + oy };

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_size stbg_build_size(float width, float height) => new stbg_size() { width = width, height = height };

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_size stbg_build_size_zero() => new stbg_size();

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_size stbg_size_add_padding(stbg_size size, stbg_padding padding)
    {
        size.width += padding.right + padding.left;
        size.height += padding.top + padding.bottom;

        return size;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public bool stbg_size_is_smaller_than(stbg_size size, stbg_size other_size)
    {
        return size.width <= other_size.width && size.height <= other_size.height;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_size stbg_size_min(stbg_size size1, stbg_size size2)
    {
        return stbg_build_size(Math.Min(size1.width, size2.width), Math.Min(size1.height, size2.height));
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_size stbg_size_max(stbg_size size1, stbg_size size2)
    {
        return stbg_build_size(Math.Max(size1.width, size2.width), Math.Max(size1.height, size2.height));
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public stbg_size stbg_size_constrain(stbg_size size, stbg_widget_constrains constrains)
    {
        return stbg_size_min(stbg_size_max(size, constrains.min), constrains.max);
    }


    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public bool stbg_is_value_near(float value, float near, float near_tolerance)
    {
        return value >= near - near_tolerance && value <= near + near_tolerance;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static stbg_color stbg_get_widget_style_color_normal_hovered_pressed(STBG_WIDGET_STYLE style_normal, STBG_WIDGET_STYLE style_hovered, STBG_WIDGET_STYLE style_pressed, bool hovered, bool pressed)
    {
        return stbg_get_widget_style_color(pressed ? style_pressed : hovered ? style_hovered : style_normal);
    }

    static public readonly stbg_color STBG_COLOR_RED = rgb(255, 0, 0);
    static public readonly stbg_color STBG_COLOR_GREEN = rgb(0, 255, 0);
    static public readonly stbg_color STBG_COLOR_BLUE = rgb(0, 0, 255);
    static public readonly stbg_color STBG_COLOR_YELLOW = rgb(255, 255, 0);
    static public readonly stbg_color STBG_COLOR_CYAN = rgb(0, 255, 255);
    static public readonly stbg_color STBG_COLOR_MAGENTA = rgb(255, 0, 255);
    static public readonly stbg_color STBG_COLOR_WHITE = rgb(255, 255, 255);
    static public readonly stbg_color STBG_COLOR_BLACK = rgb(0, 0, 0);
    static public readonly stbg_color STBG_COLOR_TRANSPARENT = rgb(0, 0, 0, 0);
}