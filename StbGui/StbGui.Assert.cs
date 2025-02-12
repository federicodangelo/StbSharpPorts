#pragma warning disable IDE1006 // Naming Styles

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace StbSharp;

public partial class StbGui
{
    // Use to asssert public API access and potential user errors
    private static void stbg__assert(bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
    {
        switch (context.init_options.assert_behaviour)
        {
            case STBG_ASSERT_BEHAVIOUR.ASSERT:
                Debug.Assert(condition, message);
                break;
            case STBG_ASSERT_BEHAVIOUR.EXCEPTION:
                if (!condition)
                    throw new StbgAssertException(message);
                break;
            case STBG_ASSERT_BEHAVIOUR.CONSOLE:
                if (!condition)
                    Console.Error.WriteLine($"Failed assert: {message}");
                break;
            case STBG_ASSERT_BEHAVIOUR.NONE:
                break;
        }
    }

    // Use to assert internal code, it will be removed in production builds
    [Conditional("DEBUG")]
    private static void stbg__assert_internal(bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
    {
        switch (context.init_options.assert_behaviour)
        {
            case STBG_ASSERT_BEHAVIOUR.ASSERT:
                Debug.Assert(condition, message);
                break;
            case STBG_ASSERT_BEHAVIOUR.EXCEPTION:
                if (!condition)
                    throw new StbgAssertException(message);
                break;
            case STBG_ASSERT_BEHAVIOUR.CONSOLE:
                if (!condition)
                    Console.Error.WriteLine($"Failed assert: {message}");
                break;
            case STBG_ASSERT_BEHAVIOUR.NONE:
                break;
        }
    }
}