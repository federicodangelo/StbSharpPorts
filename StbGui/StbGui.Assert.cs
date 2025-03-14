#pragma warning disable IDE1006 // Naming Styles

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace StbSharp;

public partial class StbGui
{
    [ExcludeFromCodeCoverage]
    public class StbgAssertException : Exception
    {
        public StbgAssertException(string? message) : base(message) { }
    }


    // Use to assert public API access and potential user errors
    [ExcludeFromCodeCoverage]
    private static void stbg__assert(bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
    {
        switch (context.init_options.assert_behavior)
        {
            case STBG_ASSERT_BEHAVIOR.ASSERT:
                Debug.Assert(condition, message);
                break;
            case STBG_ASSERT_BEHAVIOR.EXCEPTION:
                if (!condition)
                    throw new StbgAssertException(message);
                break;
            case STBG_ASSERT_BEHAVIOR.CONSOLE:
                if (!condition)
                    Console.Error.WriteLine($"Failed assert: {message}");
                break;
            case STBG_ASSERT_BEHAVIOR.NONE:
                break;
        }
    }

    // Use to assert internal code, it will be removed in production builds
    [Conditional("DEBUG")]
    [ExcludeFromCodeCoverage]
    private static void stbg__assert_internal(bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
    {
        switch (context.init_options.assert_behavior)
        {
            case STBG_ASSERT_BEHAVIOR.ASSERT:
                Debug.Assert(condition, message);
                break;
            case STBG_ASSERT_BEHAVIOR.EXCEPTION:
                if (!condition)
                    throw new StbgAssertException(message);
                break;
            case STBG_ASSERT_BEHAVIOR.CONSOLE:
                if (!condition)
                    Console.Error.WriteLine($"Failed assert: {message}");
                break;
            case STBG_ASSERT_BEHAVIOR.NONE:
                break;
        }
    }

    [Conditional("DEBUG")]
    [ExcludeFromCodeCoverage]
    private static void stbg__warning(bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
    {
        if (!condition)
            Console.WriteLine($"WARNING: {message}");
    }
}
