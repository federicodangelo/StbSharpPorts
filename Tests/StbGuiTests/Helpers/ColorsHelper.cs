using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace StbSharp.Tests;

[ExcludeFromCodeCoverage]
public class ColorsHelper
{
    private const string ANSI_COLOR_RESET = "\x1b[0m";

    private const string ANSI_COLOR_PREFIX = "\x1b[0;";
    private const string ANSI_COLOR_SEPARATOR = ";";
    private const string ANSI_COLOR_SUFFIX = "m";

    private const int ANSI_COLOR_BLACK = 30;
    private const int ANSI_COLOR_RED = 31;
    private const int ANSI_COLOR_GREEN = 32;
    private const int ANSI_COLOR_YELLOW = 33;
    private const int ANSI_COLOR_BLUE = 34;
    private const int ANSI_COLOR_MAGENTA = 35;
    private const int ANSI_COLOR_CYAN = 36;
    private const int ANSI_COLOR_WHITE = 37;
    private const int ANSI_COLOR_DEFAULT = 39;

    private const int ANSI_COLOR_BACKGROUND_BLACK = 40;
    private const int ANSI_COLOR_BACKGROUND_RED = 41;
    private const int ANSI_COLOR_BACKGROUND_GREEN = 42;
    private const int ANSI_COLOR_BACKGROUND_YELLOW = 43;
    private const int ANSI_COLOR_BACKGROUND_BLUE = 44;
    private const int ANSI_COLOR_BACKGROUND_MAGENTA = 45;
    private const int ANSI_COLOR_BACKGROUND_CYAN = 46;
    private const int ANSI_COLOR_BACKGROUND_WHITE = 47;
    private const int ANSI_COLOR_BACKGROUND_DEFAULT = 49;

    static public string GetAnsiColorReset()
    {
        return ANSI_COLOR_RESET;
    }

    static public string GetAnsiColor(StbGui.stbg_color color, StbGui.stbg_color backgroundColor)
    {
        if (color.a == 0 && backgroundColor.a == 0)
            return "";

        string c = ANSI_COLOR_PREFIX;

        if (color == StbGui.STBG_COLOR_BLACK)
            c += ANSI_COLOR_BLACK;
        else if (color == StbGui.STBG_COLOR_RED)
            c += ANSI_COLOR_RED;
        else if (color == StbGui.STBG_COLOR_GREEN)
            c += ANSI_COLOR_GREEN;
        else if (color == StbGui.STBG_COLOR_YELLOW)
            c += ANSI_COLOR_YELLOW;
        else if (color == StbGui.STBG_COLOR_BLUE)
            c += ANSI_COLOR_BLUE;
        else if (color == StbGui.STBG_COLOR_MAGENTA)
            c += ANSI_COLOR_MAGENTA;
        else if (color == StbGui.STBG_COLOR_CYAN)
            c += ANSI_COLOR_CYAN;
        else if (color == StbGui.STBG_COLOR_WHITE)
            c += ANSI_COLOR_WHITE;
        else
            c += ANSI_COLOR_DEFAULT;

        c += ANSI_COLOR_SEPARATOR;

        if (backgroundColor == StbGui.STBG_COLOR_BLACK)
            c += ANSI_COLOR_BACKGROUND_BLACK;
        else if (backgroundColor == StbGui.STBG_COLOR_RED)
            c += ANSI_COLOR_BACKGROUND_RED;
        else if (backgroundColor == StbGui.STBG_COLOR_GREEN)
            c += ANSI_COLOR_BACKGROUND_GREEN;
        else if (backgroundColor == StbGui.STBG_COLOR_YELLOW)
            c += ANSI_COLOR_BACKGROUND_YELLOW;
        else if (backgroundColor == StbGui.STBG_COLOR_BLUE)
            c += ANSI_COLOR_BACKGROUND_BLUE;
        else if (backgroundColor == StbGui.STBG_COLOR_MAGENTA)
            c += ANSI_COLOR_BACKGROUND_MAGENTA;
        else if (backgroundColor == StbGui.STBG_COLOR_CYAN)
            c += ANSI_COLOR_BACKGROUND_CYAN;
        else if (backgroundColor == StbGui.STBG_COLOR_WHITE)
            c += ANSI_COLOR_BACKGROUND_WHITE;
        else
            c += ANSI_COLOR_BACKGROUND_DEFAULT;

        c += ANSI_COLOR_SUFFIX;

        return c;
    }

    static private readonly Dictionary<StbGui.stbg_color, string> NICE_COLOR_NAMES = new() {
        { StbGui.STBG_COLOR_RED, "RED" },
        { StbGui.STBG_COLOR_GREEN, "GREEN" },
        { StbGui.STBG_COLOR_BLUE, "BLUE" },
        { StbGui.STBG_COLOR_YELLOW, "YELLOW" },
        { StbGui.STBG_COLOR_CYAN, "CYAN" },
        { StbGui.STBG_COLOR_MAGENTA, "MAGENTA" },
        { StbGui.STBG_COLOR_WHITE, "WHITE" },
        { StbGui.STBG_COLOR_BLACK, "BLACK" },
        { StbGui.STBG_COLOR_TRANSPARENT, "TRANSPARENT" },
    };

    static private readonly Dictionary<string, StbGui.stbg_color> NICE_COLOR_NAMES_REVERSE = new();

    static private readonly Dictionary<StbGui.stbg_color, char> NICE_COLOR_NAMES_SINGLE_CHAR = new();

    static private readonly Dictionary<char, StbGui.stbg_color> NICE_COLOR_NAMES_SINGLE_CHAR_REVERSE = new();

    static ColorsHelper()
    {
        foreach (var kv in NICE_COLOR_NAMES)
        {
            NICE_COLOR_NAMES_REVERSE[kv.Value] = kv.Key;

            NICE_COLOR_NAMES_SINGLE_CHAR[kv.Key] = (kv.Key == StbGui.STBG_COLOR_BLACK) ? 'K' : kv.Value[0];
        }

        foreach (var kv in NICE_COLOR_NAMES_SINGLE_CHAR)
        {
            NICE_COLOR_NAMES_SINGLE_CHAR_REVERSE[kv.Value] = kv.Key;
        }
    }

    static public string GetNiceColorName(StbGui.stbg_color color)
    {
        Debug.Assert(NICE_COLOR_NAMES.ContainsKey(color), "Received color is not a nice color" + color);

        return NICE_COLOR_NAMES[color];
    }

    static public char GetNiceColorNameSingleCharacter(StbGui.stbg_color color)
    {
        Debug.Assert(NICE_COLOR_NAMES_SINGLE_CHAR.ContainsKey(color), "Received color is not a nice color" + color);

        return NICE_COLOR_NAMES_SINGLE_CHAR[color];
    }

    static public StbGui.stbg_color GetColorFromSingleCharacterNiceColorName(char c)
    {
        Debug.Assert(NICE_COLOR_NAMES_SINGLE_CHAR_REVERSE.ContainsKey(c), "Received color is not a nice color" + c);

        return NICE_COLOR_NAMES_SINGLE_CHAR_REVERSE[c];
    }
}