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
            context.theme.styles[(int)style1];
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2)
    {
        return
            context.theme.styles[(int)style1] +
            context.theme.styles[(int)style2];
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3)
    {
        return
            context.theme.styles[(int)style1] +
            context.theme.styles[(int)style2] +
            context.theme.styles[(int)style3];
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3, STBG_WIDGET_STYLE style4)
    {
        return
            context.theme.styles[(int)style1] +
            context.theme.styles[(int)style2] +
            context.theme.styles[(int)style3] +
            context.theme.styles[(int)style4];
    }

    [ExcludeFromCodeCoverage]
    static private float stbg__sum_styles(STBG_WIDGET_STYLE style1, STBG_WIDGET_STYLE style2, STBG_WIDGET_STYLE style3, STBG_WIDGET_STYLE style4, STBG_WIDGET_STYLE style5)
    {
        return
            context.theme.styles[(int)style1] +
            context.theme.styles[(int)style2] +
            context.theme.styles[(int)style3] +
            context.theme.styles[(int)style4] +
            context.theme.styles[(int)style5];
    }
}