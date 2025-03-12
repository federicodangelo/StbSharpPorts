#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using StbSharp.StbCommon;

using STB_TEXTEDIT_CHARTYPE = char;
using STB_TEXTEDIT_KEYTYPE = int;
using STB_TEXTEDIT_POSITIONTYPE = int;

public class StbTextEdit
{
    // stb_textedit.h - v1.14  - public domain - Sean Barrett
    // Development of this library was sponsored by RAD Game Tools
    //
    // This C header file implements the guts of a multi-line text-editing
    // widget; you implement display, word-wrapping, and low-level string
    // insertion/deletion, and stb_textedit will map user inputs into
    // insertions & deletions, plus updates to the cursor position,
    // selection state, and undo state.
    //
    // It is intended for use in games and other systems that need to build
    // their own custom widgets and which do not have heavy text-editing
    // requirements (this library is not recommended for use for editing large
    // texts, as its performance does not scale and it has limited undo).
    //
    // Non-trivial behaviors are modelled after Windows text controls.
    //
    //
    // LICENSE
    //
    // See end of file for license information.
    //
    //
    // DEPENDENCIES
    //
    // Uses the C runtime function 'memmove', which you can override
    // by defining STB_TEXTEDIT_memmove before the implementation.
    // Uses no other functions. Performs no runtime allocations.
    //
    //
    // VERSION HISTORY
    //
    //   1.14 (2021-07-11) page up/down, various fixes
    //   1.13 (2019-02-07) fix bug in undo size management
    //   1.12 (2018-01-29) user can change STB_TEXTEDIT_KEYTYPE, fix redo to avoid crash
    //   1.11 (2017-03-03) fix HOME on last line, dragging off single-line textfield
    //   1.10 (2016-10-25) supress warnings about casting away const with -Wcast-qual
    //   1.9  (2016-08-27) customizable move-by-word
    //   1.8  (2016-04-02) better keyboard handling when mouse button is down
    //   1.7  (2015-09-13) change y range handling in case baseline is non-0
    //   1.6  (2015-04-15) allow STB_TEXTEDIT_memmove
    //   1.5  (2014-09-10) add support for secondary keys for OS X
    //   1.4  (2014-08-17) fix signed/unsigned warnings
    //   1.3  (2014-06-19) fix mouse clicking to round to nearest char boundary
    //   1.2  (2014-05-27) fix some RAD types that had crept into the new code
    //   1.1  (2013-12-15) move-by-word (requires STB_TEXTEDIT_IS_SPACE )
    //   1.0  (2012-07-26) improve documentation, initial public release
    //   0.3  (2012-02-24) bugfixes, single-line mode; insert mode
    //   0.2  (2011-11-28) fixes to undo/redo
    //   0.1  (2010-07-08) initial version
    //
    // ADDITIONAL CONTRIBUTORS
    //
    //   Ulf Winklemann: move-by-word in 1.1
    //   Fabian Giesen: secondary key inputs in 1.5
    //   Martins Mozeiko: STB_TEXTEDIT_memmove in 1.6
    //   Louis Schnellbach: page up/down in 1.14
    //
    //   Bugfixes:
    //      Scott Graham
    //      Daniel Keller
    //      Omar Cornut
    //      Dan Thompson
    //
    // USAGE
    //
    // This file behaves differently depending on what symbols you define
    // before including it.
    //
    //
    // Header-file mode:
    //
    //   If you do not define STB_TEXTEDIT_IMPLEMENTATION before including this,
    //   it will operate in "header file" mode. In this mode, it declares a
    //   single public symbol, STB_TexteditState, which encapsulates the current
    //   state of a text widget (except for the string, which you will store
    //   separately).
    //
    //   To compile in this mode, you must define STB_TEXTEDIT_CHARTYPE to a
    //   primitive type that defines a single character (e.g. char, wchar_t, etc).
    //
    //   To save space or increase undo-ability, you can optionally define the
    //   following things that are used by the undo system:
    //
    //      STB_TEXTEDIT_POSITIONTYPE         small int type encoding a valid cursor position
    //      STB_TEXTEDIT_UNDOSTATECOUNT       the number of undo states to allow
    //      STB_TEXTEDIT_UNDOCHARCOUNT        the number of characters to store in the undo buffer
    //
    //   If you don't define these, they are set to permissive types and
    //   moderate sizes. The undo system does no memory allocations, so
    //   it grows STB_TexteditState by the worst-case storage which is (in bytes):
    //
    //        [4 + 3 * sizeof(STB_TEXTEDIT_POSITIONTYPE)] * STB_TEXTEDIT_UNDOSTATECOUNT
    //      +          sizeof(STB_TEXTEDIT_CHARTYPE)      * STB_TEXTEDIT_UNDOCHARCOUNT
    //
    //
    // Implementation mode:
    //
    //   If you define STB_TEXTEDIT_IMPLEMENTATION before including this, it
    //   will compile the implementation of the text edit widget, depending
    //   on a large number of symbols which must be defined before the include.
    //
    //   The implementation is defined only as static functions. You will then
    //   need to provide your own APIs in the same file which will access the
    //   static functions.
    //
    //   The basic concept is that you provide a "string" object which
    //   behaves like an array of characters. stb_textedit uses indices to
    //   refer to positions in the string, implicitly representing positions
    //   in the displayed textedit. This is true for both plain text and
    //   rich text; even with rich text stb_truetype interacts with your
    //   code as if there was an array of all the displayed characters.
    //
    // Symbols that must be the same in header-file and implementation mode:
    //
    //     STB_TEXTEDIT_CHARTYPE             the character type
    //     STB_TEXTEDIT_POSITIONTYPE         small type that is a valid cursor position
    //     STB_TEXTEDIT_UNDOSTATECOUNT       the number of undo states to allow
    //     STB_TEXTEDIT_UNDOCHARCOUNT        the number of characters to store in the undo buffer
    //
    // Symbols you must define for implementation mode:
    //
    //    STB_TEXTEDIT_STRING               the type of object representing a string being edited,
    //                                      typically this is a wrapper object with other data you need
    //
    //    STB_TEXTEDIT_STRINGLEN(obj)       the length of the string (ideally O(1))
    //    STB_TEXTEDIT_LAYOUTROW(&r,obj,n)  returns the results of laying out a line of characters
    //                                        starting from character #n (see discussion below)
    //    STB_TEXTEDIT_GETWIDTH(obj,n,i)    returns the pixel delta from the xpos of the i'th character
    //                                        to the xpos of the i+1'th char for a line of characters
    //                                        starting at character #n (i.e. accounts for kerning
    //                                        with previous char)
    //    STB_TEXTEDIT_KEYTOTEXT(k)         maps a keyboard input to an insertable character
    //                                        (return type is int, -1 means not valid to insert)
    //    STB_TEXTEDIT_GETCHAR(obj,i)       returns the i'th character of obj, 0-based
    //    STB_TEXTEDIT_NEWLINE              the character returned by _GETCHAR() we recognize
    //                                        as manually wordwrapping for end-of-line positioning
    //
    //    STB_TEXTEDIT_DELETECHARS(obj,i,n)      delete n characters starting at i
    //    STB_TEXTEDIT_INSERTCHARS(obj,i,c*,n)   insert n characters at i (pointed to by STB_TEXTEDIT_CHARTYPE*)
    //
    //    STB_TEXTEDIT_K_SHIFT       a power of two that is or'd in to a keyboard input to represent the shift key
    //
    //    STB_TEXTEDIT_K_LEFT        keyboard input to move cursor left
    //    STB_TEXTEDIT_K_RIGHT       keyboard input to move cursor right
    //    STB_TEXTEDIT_K_UP          keyboard input to move cursor up
    //    STB_TEXTEDIT_K_DOWN        keyboard input to move cursor down
    //    STB_TEXTEDIT_K_PGUP        keyboard input to move cursor up a page
    //    STB_TEXTEDIT_K_PGDOWN      keyboard input to move cursor down a page
    //    STB_TEXTEDIT_K_LINESTART   keyboard input to move cursor to start of line  // e.g. HOME
    //    STB_TEXTEDIT_K_LINEEND     keyboard input to move cursor to end of line    // e.g. END
    //    STB_TEXTEDIT_K_TEXTSTART   keyboard input to move cursor to start of text  // e.g. ctrl-HOME
    //    STB_TEXTEDIT_K_TEXTEND     keyboard input to move cursor to end of text    // e.g. ctrl-END
    //    STB_TEXTEDIT_K_DELETE      keyboard input to delete selection or character under cursor
    //    STB_TEXTEDIT_K_BACKSPACE   keyboard input to delete selection or character left of cursor
    //    STB_TEXTEDIT_K_UNDO        keyboard input to perform undo
    //    STB_TEXTEDIT_K_REDO        keyboard input to perform redo
    //
    // Optional:
    //    STB_TEXTEDIT_K_INSERT              keyboard input to toggle insert mode
    //    STB_TEXTEDIT_IS_SPACE(ch)          true if character is whitespace (e.g. 'isspace'),
    //                                          required for default WORDLEFT/WORDRIGHT handlers
    //    STB_TEXTEDIT_MOVEWORDLEFT(obj,i)   custom handler for WORDLEFT, returns index to move cursor to
    //    STB_TEXTEDIT_MOVEWORDRIGHT(obj,i)  custom handler for WORDRIGHT, returns index to move cursor to
    //    STB_TEXTEDIT_K_WORDLEFT            keyboard input to move cursor left one word // e.g. ctrl-LEFT
    //    STB_TEXTEDIT_K_WORDRIGHT           keyboard input to move cursor right one word // e.g. ctrl-RIGHT
    //    STB_TEXTEDIT_K_LINESTART2          secondary keyboard input to move cursor to start of line
    //    STB_TEXTEDIT_K_LINEEND2            secondary keyboard input to move cursor to end of line
    //    STB_TEXTEDIT_K_TEXTSTART2          secondary keyboard input to move cursor to start of text
    //    STB_TEXTEDIT_K_TEXTEND2            secondary keyboard input to move cursor to end of text
    //
    // Keyboard input must be encoded as a single integer value; e.g. a character code
    // and some bitflags that represent shift states. to simplify the interface, SHIFT must
    // be a bitflag, so we can test the shifted state of cursor movements to allow selection,
    // i.e. (STB_TEXTEDIT_K_RIGHT|STB_TEXTEDIT_K_SHIFT) should be shifted right-arrow.
    //
    // You can encode other things, such as CONTROL or ALT, in additional bits, and
    // then test for their presence in e.g. STB_TEXTEDIT_K_WORDLEFT. For example,
    // my Windows implementations add an additional CONTROL bit, and an additional KEYDOWN
    // bit. Then all of the STB_TEXTEDIT_K_ values bitwise-or in the KEYDOWN bit,
    // and I pass both WM_KEYDOWN and WM_CHAR events to the "key" function in the
    // API below. The control keys will only match WM_KEYDOWN events because of the
    // keydown bit I add, and STB_TEXTEDIT_KEYTOTEXT only tests for the KEYDOWN
    // bit so it only decodes WM_CHAR events.
    //
    // STB_TEXTEDIT_LAYOUTROW returns information about the shape of one displayed
    // row of characters assuming they start on the i'th character--the width and
    // the height and the number of characters consumed. This allows this library
    // to traverse the entire layout incrementally. You need to compute word-wrapping
    // here.
    //
    // Each textfield keeps its own insert mode state, which is not how normal
    // applications work. To keep an app-wide insert mode, update/copy the
    // "insert_mode" field of STB_TexteditState before/after calling API functions.
    //
    // API
    //
    //    void stb_textedit_initialize_state(STB_TexteditState *state, int is_single_line)
    //
    //    void stb_textedit_click(STB_TEXTEDIT_STRING *str, STB_TexteditState *state, float x, float y)
    //    void stb_textedit_drag(STB_TEXTEDIT_STRING *str, STB_TexteditState *state, float x, float y)
    //    int  stb_textedit_cut(STB_TEXTEDIT_STRING *str, STB_TexteditState *state)
    //    int  stb_textedit_paste(STB_TEXTEDIT_STRING *str, STB_TexteditState *state, STB_TEXTEDIT_CHARTYPE *text, int len)
    //    void stb_textedit_key(STB_TEXTEDIT_STRING *str, STB_TexteditState *state, STB_TEXEDIT_KEYTYPE key)
    //
    //    Each of these functions potentially updates the string and updates the
    //    state.
    //
    //      initialize_state:
    //          set the textedit state to a known good default state when initially
    //          constructing the textedit.
    //
    //      click:
    //          call this with the mouse x,y on a mouse down; it will update the cursor
    //          and reset the selection start/end to the cursor point. the x,y must
    //          be relative to the text widget, with (0,0) being the top left.
    //
    //      drag:
    //          call this with the mouse x,y on a mouse drag/up; it will update the
    //          cursor and the selection end point
    //
    //      cut:
    //          call this to delete the current selection; returns true if there was
    //          one. you should FIRST copy the current selection to the system paste buffer.
    //          (To copy, just copy the current selection out of the string yourself.)
    //
    //      paste:
    //          call this to paste text at the current cursor point or over the current
    //          selection if there is one.
    //
    //      key:
    //          call this for keyboard inputs sent to the textfield. you can use it
    //          for "key down" events or for "translated" key events. if you need to
    //          do both (as in Win32), or distinguish Unicode characters from control
    //          inputs, set a high bit to distinguish the two; then you can define the
    //          various definitions like STB_TEXTEDIT_K_LEFT have the is-key-event bit
    //          set, and make STB_TEXTEDIT_KEYTOCHAR check that the is-key-event bit is
    //          clear. STB_TEXTEDIT_KEYTYPE defaults to int, but you can #define it to
    //          anything other type you wante before including.
    //
    //
    //   When rendering, you can read the cursor position and selection state from
    //   the STB_TexteditState.
    //
    //
    // Notes:
    //
    // This is designed to be usable in IMGUI, so it allows for the possibility of
    // running in an IMGUI that has NOT cached the multi-line layout. For this
    // reason, it provides an interface that is compatible with computing the
    // layout incrementally--we try to make sure we make as few passes through
    // as possible. (For example, to locate the mouse pointer in the text, we
    // could define functions that return the X and Y positions of characters
    // and binary search Y and then X, but if we're doing dynamic layout this
    // will run the layout algorithm many times, so instead we manually search
    // forward in one pass. Similar logic applies to e.g. up-arrow and
    // down-arrow movement.)
    //
    // If it's run in a widget that *has* cached the layout, then this is less
    // efficient, but it's not horrible on modern computers. But you wouldn't
    // want to edit million-line files with it.


    // DEFINITIONS 

    // the type of object representing a string being edited,
    // typically this is a wrapper object with other data you need
    public struct STB_TEXTEDIT_STRING
    {
        public Memory<STB_TEXTEDIT_CHARTYPE> text;
        public int text_length;

        // returns the results of laying out a line of characters
        // starting from character #n.
        // Returns information about the shape of one displayed
        // row of characters assuming they start on the i'th character--the width and
        // the height and the number of characters consumed. This allows this library
        // to traverse the entire layout incrementally. You need to compute word-wrapping
        // here.
        // StbTexteditRow STB_TEXTEDIT_LAYOUTROW(STB_TEXTEDIT_STRING str, int n)
        public Func<STB_TEXTEDIT_STRING, int, StbTexteditRow> layout_row;

        // returns the pixel delta from the xpos of the i'th character
        // to the xpos of the i+1'th char for a line of characters
        // starting at character #n (i.e. accounts for kerning
        // with previous char)
        // Return STB_TEXTEDIT_GETWIDTH_NEWLINE if it's a new line character!!!
        // int STB_TEXTEDIT_GETWIDTH(ref STB_TEXTEDIT_STRING str, int n, int i)
        public Func<STB_TEXTEDIT_STRING, int, int, int> get_width;
    }

    // the length of the string (ideally O(1))
    private static int STB_TEXTEDIT_STRINGLEN(ref STB_TEXTEDIT_STRING str)
    {
        return str.text_length;
    }

    // returns the results of laying out a line of characters
    // starting from character #n (see discussion below)
    // STB_TEXTEDIT_LAYOUTROW returns information about the shape of one displayed
    // row of characters assuming they start on the i'th character--the width and
    // the height and the number of characters consumed. This allows this library
    // to traverse the entire layout incrementally. You need to compute word-wrapping
    // here.
    private static void STB_TEXTEDIT_LAYOUTROW(out StbTexteditRow r, ref STB_TEXTEDIT_STRING str, int n)
    {
        r = str.layout_row(str, n);
    }

    // returns the pixel delta from the xpos of the i'th character
    // to the xpos of the i+1'th char for a line of characters
    // starting at character #n (i.e. accounts for kerning
    // with previous char)
    // Return STB_TEXTEDIT_GETWIDTH_NEWLINE if it's a new line character!!!
    private static int STB_TEXTEDIT_GETWIDTH(ref STB_TEXTEDIT_STRING str, int n, int i)
    {
        return str.get_width(str, n, i);
    }

    public static int STB_TEXTEDIT_GETWIDTH_NEWLINE = -1;

    // maps a keyboard input to an insertable character
    // (return type is int, -1 means not valid to insert)
    private static int STB_TEXTEDIT_KEYTOTEXT(STB_TEXTEDIT_KEYTYPE k)
    {
        k &= STB_TEXTEDIT_CHARS_MASK;

        if (k != 0)
            return k;

        return -1;
    }

    // returns the i'th character of obj, 0-based
    private static STB_TEXTEDIT_CHARTYPE STB_TEXTEDIT_GETCHAR(ref STB_TEXTEDIT_STRING str, int i)
    {
        return str.text.Span[i];
    }

    // delete n characters starting at i
    public static void STB_TEXTEDIT_DELETECHARS(ref STB_TEXTEDIT_STRING str, int i, int n)
    {
        str.text.Slice(i + n).CopyTo(str.text.Slice(i));
        str.text_length -= n;
    }

    // insert n characters at i (pointed to by STB_TEXTEDIT_CHARTYPE*)
    public static bool STB_TEXTEDIT_INSERTCHARS(ref STB_TEXTEDIT_STRING str, int i, ReadOnlySpan<STB_TEXTEDIT_CHARTYPE> c, int n)
    {
        // TODO: Validation for bounds?
        str.text.Slice(i, str.text_length).CopyTo(str.text.Slice(i + n));
        c.Slice(0, n).CopyTo(str.text.Slice(i).Span);
        str.text_length += n;

        return true;
    }

    public static bool STB_TEXTEDIT_IS_SPACE(STB_TEXTEDIT_CHARTYPE ch)
    {
        return ch == ' ';
    }

    // We don't define these custom handlers, the default implementation is good enough
    //    STB_TEXTEDIT_MOVEWORDLEFT(obj,i)   custom handler for WORDLEFT, returns index to move cursor to
    //    STB_TEXTEDIT_MOVEWORDRIGHT(obj,i)  custom handler for WORDRIGHT, returns index to move cursor to


    // first 16 bits: character code
    // next 8 bits: special key codes
    // remaining 8 bits: flag modifiers
    public const int STB_TEXTEDIT_CHAR_BITS = 16;
    public const int STB_TEXTEDIT_CHAR_SPECIAL_BITS = 8;
    public const int STB_TEXTEDIT_CHAR_FLAG_BITS = 8;
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_SHIFT = 1 << (STB_TEXTEDIT_CHAR_BITS + STB_TEXTEDIT_CHAR_SPECIAL_BITS + 1); //a power of two that is or'd in to a keyboard input to represent the shift key
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_CHARS_MASK = (1 << STB_TEXTEDIT_CHAR_BITS) - 1;
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_SPECIAL_KEY_CODES_START = 1 << (STB_TEXTEDIT_CHAR_BITS + STB_TEXTEDIT_CHAR_SPECIAL_BITS);

    // the character returned by _GETCHAR() we recognize
    // as manually wordwrapping for end-of-line positioning
    public const STB_TEXTEDIT_CHARTYPE STB_TEXTEDIT_NEWLINE = '\n';

    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_LEFT = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 1;        // keyboard input to move cursor left
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_RIGHT = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 2;       // keyboard input to move cursor right
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_UP = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 3;          // keyboard input to move cursor up
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_DOWN = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 4;        // keyboard input to move cursor down
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_PGUP = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 5;        // keyboard input to move cursor up a page
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_PGDOWN = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 6;      // keyboard input to move cursor down a page
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_LINESTART = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 7;   // keyboard input to move cursor to start of line  // e.g. HOME
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_LINEEND = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 8;     // keyboard input to move cursor to end of line    // e.g. END
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_TEXTSTART = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 9;   // keyboard input to move cursor to start of text  // e.g. ctrl-HOME
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_TEXTEND = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 10;     // keyboard input to move cursor to end of text    // e.g. ctrl-END
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_DELETE = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 11;      // keyboard input to delete selection or character under cursor
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_BACKSPACE = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 12;   // keyboard input to delete selection or character left of cursor
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_UNDO = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 13;        // keyboard input to perform undo
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_REDO = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 14;        // keyboard input to perform redo

    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_INSERT = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 15;     // keyboard input to toggle insert mode
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_WORDLEFT = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 16;   // keyboard input to move cursor left one word // e.g. ctrl-LEFT
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_WORDRIGHT = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 17;  // keyboard input to move cursor right one word // e.g. ctrl-RIGHT
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_LINESTART2 = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 18; // secondary keyboard input to move cursor to start of line
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_LINEEND2 = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 19;   // secondary keyboard input to move cursor to end of line
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_TEXTSTART2 = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 20; // secondary keyboard input to move cursor to start of text
    public const STB_TEXTEDIT_KEYTYPE STB_TEXTEDIT_K_TEXTEND2 = STB_TEXTEDIT_SPECIAL_KEY_CODES_START + 21;   // secondary keyboard input to move cursor to end of text    


    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////
    ////   Header-file mode
    ////
    ////

    ////////////////////////////////////////////////////////////////////////
    //
    //     STB_TexteditState
    //
    // Definition of STB_TexteditState which you should store
    // per-textfield; it includes cursor position, selection state,
    // and undo state.
    //

    const int STB_TEXTEDIT_UNDOSTATECOUNT = 99;
    const int STB_TEXTEDIT_UNDOCHARCOUNT = 999;

    public struct StbUndoRecord
    {
        // private data
        public STB_TEXTEDIT_POSITIONTYPE where;
        public STB_TEXTEDIT_POSITIONTYPE insert_length;
        public STB_TEXTEDIT_POSITIONTYPE delete_length;
        public int char_storage;
    }


    public struct StbUndoState()
    {
        // private data
        public StbUndoRecord[] undo_rec = new StbUndoRecord[STB_TEXTEDIT_UNDOSTATECOUNT];
        public STB_TEXTEDIT_CHARTYPE[] undo_char = new STB_TEXTEDIT_CHARTYPE[STB_TEXTEDIT_UNDOCHARCOUNT];
        public short undo_point, redo_point;
        public int undo_char_point, redo_char_point;
    }


    public struct STB_TexteditState
    {
        /////////////////////
        //
        // public data
        //

        public int cursor;
        // position of the text cursor within the string

        public int select_start;          // selection start point
        public int select_end;
        // selection start and end point in characters; if equal, no selection.
        // note that start may be less than or greater than end (e.g. when
        // dragging the mouse, start is where the initial click was, and you
        // can drag in either direction)

        public bool insert_mode;
        // each textfield keeps its own insert mode state. to keep an app-wide
        // insert mode, copy this value in/out of the app state

        public int row_count_per_page;
        // page size in number of row.
        // this value MUST be set to >0 for pageup or pagedown in multilines documents.

        /////////////////////
        //
        // private data
        //
        public bool cursor_at_end_of_line; // not implemented yet
        public bool initialized;
        public bool has_preferred_x;
        public bool single_line;
        //public byte padding1, padding2, padding3;
        public float preferred_x; // this determines where the cursor up/down tries to seek to along x
        public StbUndoState undostate;
    }


    ////////////////////////////////////////////////////////////////////////
    //
    //     StbTexteditRow
    //
    // Result of layout query, used by stb_textedit to determine where
    // the text in each row is.

    // result of layout query
    public struct StbTexteditRow
    {
        public float x0, x1;             // starting x location, end x location (allows for align=right, etc)
        public float baseline_y_delta;  // position of baseline relative to previous row's baseline
        public float ymin, ymax;         // height of row above and below baseline
        public int num_chars;
    }
    //#endif //INCLUDE_STB_TEXTEDIT_H


    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////
    ////   Implementation mode
    ////
    ////


    // implementation isn't include-guarded, since it might have indirectly
    // included just the "header" portion
    //# ifdef STB_TEXTEDIT_IMPLEMENTATION


    static void STB_TEXTEDIT_memmove(Span<byte> target, Span<byte> source, int sz)
    {
        source.Slice(0, sz).CopyTo(target);
    }

    static void STB_TEXTEDIT_memmove<T>(Span<T> target, Span<T> source, int sz) where T : struct
    {
        source.Slice(0, sz).CopyTo(target);
    }


    /////////////////////////////////////////////////////////////////////////////
    //
    //      Mouse input handling
    //

    // traverse the layout to locate the nearest character to a display position
    static int stb_text_locate_coord(ref STB_TEXTEDIT_STRING str, float x, float y)
    {
        StbTexteditRow r;
        int n = STB_TEXTEDIT_STRINGLEN(ref str);
        float base_y = 0, prev_x;
        int i = 0, k;

        r.x0 = r.x1 = 0;
        r.ymin = r.ymax = 0;
        r.num_chars = 0;

        // search rows to find one that straddles 'y'
        while (i < n)
        {
            STB_TEXTEDIT_LAYOUTROW(out r, ref str, i);
            if (r.num_chars <= 0)
                return n;

            if (i == 0 && y < base_y + r.ymin)
                return 0;

            if (y < base_y + r.ymax)
                break;

            i += r.num_chars;
            base_y += r.baseline_y_delta;
        }

        // below all text, return 'after' last character
        if (i >= n)
            return n;

        // check if it's before the beginning of the line
        if (x < r.x0)
            return i;

        // check if it's before the end of the line
        if (x < r.x1)
        {
            // search characters in row for one that straddles 'x'
            prev_x = r.x0;
            for (k = 0; k < r.num_chars; ++k)
            {
                float w = STB_TEXTEDIT_GETWIDTH(ref str, i, k);
                if (x < prev_x + w)
                {
                    if (x < prev_x + w / 2)
                        return k + i;
                    else
                        return k + i + 1;
                }
                prev_x += w;
            }
            // shouldn't happen, but if it does, fall through to end-of-line case
        }

        // if the last character is a newline, return that. otherwise return 'after' the last character
        if (STB_TEXTEDIT_GETCHAR(ref str, i + r.num_chars - 1) == STB_TEXTEDIT_NEWLINE)
            return i + r.num_chars - 1;
        else
            return i + r.num_chars;
    }

    // API click: on mouse down, move the cursor to the clicked location, and reset the selection
    public static void stb_textedit_click(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, float x, float y)
    {
        // In single-line mode, just always make y = 0. This lets the drag keep working if the mouse
        // goes off the top or bottom of the text
        if (state.single_line)
        {
            StbTexteditRow r;
            STB_TEXTEDIT_LAYOUTROW(out r, ref str, 0);
            y = r.ymin;
        }

        state.cursor = stb_text_locate_coord(ref str, x, y);
        state.select_start = state.cursor;
        state.select_end = state.cursor;
        state.has_preferred_x = false;
    }

    // API drag: on mouse drag, move the cursor and selection endpoint to the clicked location
    public static void stb_textedit_drag(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, float x, float y)
    {
        int p = 0;

        // In single-line mode, just always make y = 0. This lets the drag keep working if the mouse
        // goes off the top or bottom of the text
        if (state.single_line)
        {
            StbTexteditRow r;
            STB_TEXTEDIT_LAYOUTROW(out r, ref str, 0);
            y = r.ymin;
        }

        if (state.select_start == state.select_end)
            state.select_start = state.cursor;

        p = stb_text_locate_coord(ref str, x, y);
        state.cursor = state.select_end = p;
    }

    /////////////////////////////////////////////////////////////////////////////
    //
    //      Keyboard input handling
    //

    // forward declarations
    // static void stb_text_undo(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state);
    // static void stb_text_redo(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state);
    // static void stb_text_makeundo_delete(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, int where, int length);
    // static void stb_text_makeundo_insert(ref STB_TexteditState state, int where, int length);
    // static void stb_text_makeundo_replace(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, int where, int old_length, int new_length);

    struct StbFindState
    {
        public float x, y;    // position of n'th character
        public float height; // height of line
        public int first_char, length; // first char of row, and length
        public int prev_first;  // first char of previous row
    }

    // find the x/y location of a character, and remember info about the previous row in
    // case we get a move-up event (for page up, we'll have to rescan)
    static void stb_textedit_find_charpos(ref StbFindState find, ref STB_TEXTEDIT_STRING str, int n, bool single_line)
    {
        StbTexteditRow r;
        int prev_start = 0;
        int z = STB_TEXTEDIT_STRINGLEN(ref str);
        int i = 0, first;

        if (n == z)
        {
            // if it's at the end, then find the last line -- simpler than trying to
            // explicitly handle this case in the regular code
            if (single_line)
            {
                STB_TEXTEDIT_LAYOUTROW(out r, ref str, 0);
                find.y = 0;
                find.first_char = 0;
                find.length = z;
                find.height = r.ymax - r.ymin;
                find.x = r.x1;
            }
            else
            {
                find.y = 0;
                find.x = 0;
                find.height = 1;
                while (i < z)
                {
                    STB_TEXTEDIT_LAYOUTROW(out r, ref str, i);
                    prev_start = i;
                    i += r.num_chars;
                }
                find.first_char = i;
                find.length = 0;
                find.prev_first = prev_start;
            }
            return;
        }

        // search rows to find the one that straddles character n
        find.y = 0;

        for (; ; )
        {
            STB_TEXTEDIT_LAYOUTROW(out r, ref str, i);
            if (n < i + r.num_chars)
                break;
            prev_start = i;
            i += r.num_chars;
            find.y += r.baseline_y_delta;
        }

        find.first_char = first = i;
        find.length = r.num_chars;
        find.height = r.ymax - r.ymin;
        find.prev_first = prev_start;

        // now scan to find xpos
        find.x = r.x0;
        for (i = 0; first + i < n; ++i)
            find.x += STB_TEXTEDIT_GETWIDTH(ref str, first, i);
    }

    static bool STB_TEXT_HAS_SELECTION(ref STB_TexteditState s) => ((s).select_start != (s).select_end);

    // make the selection/cursor state valid if client altered the string
    static void stb_textedit_clamp(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state)
    {
        int n = STB_TEXTEDIT_STRINGLEN(ref str);
        if (STB_TEXT_HAS_SELECTION(ref state))
        {
            if (state.select_start > n) state.select_start = n;
            if (state.select_end > n) state.select_end = n;
            // if clamping forced them to be equal, move the cursor to match
            if (state.select_start == state.select_end)
                state.cursor = state.select_start;
        }
        if (state.cursor > n) state.cursor = n;
    }

    // delete characters while updating undo
    static void stb_textedit_delete(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, int where, int len)
    {
        stb_text_makeundo_delete(ref str, ref state, where, len);
        STB_TEXTEDIT_DELETECHARS(ref str, where, len);
        state.has_preferred_x = false;
    }

    // delete the section
    static void stb_textedit_delete_selection(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state)
    {
        stb_textedit_clamp(ref str, ref state);
        if (STB_TEXT_HAS_SELECTION(ref state))
        {
            if (state.select_start < state.select_end)
            {
                stb_textedit_delete(ref str, ref state, state.select_start, state.select_end - state.select_start);
                state.select_end = state.cursor = state.select_start;
            }
            else
            {
                stb_textedit_delete(ref str, ref state, state.select_end, state.select_start - state.select_end);
                state.select_start = state.cursor = state.select_end;
            }
            state.has_preferred_x = false;
        }
    }

    // canoncialize the selection so start <= end
    static void stb_textedit_sortselection(ref STB_TexteditState state)
    {
        if (state.select_end < state.select_start)
        {
            int temp = state.select_end;
            state.select_end = state.select_start;
            state.select_start = temp;
        }
    }

    // move cursor to first character of selection
    static void stb_textedit_move_to_first(ref STB_TexteditState state)
    {
        if (STB_TEXT_HAS_SELECTION(ref state))
        {
            stb_textedit_sortselection(ref state);
            state.cursor = state.select_start;
            state.select_end = state.select_start;
            state.has_preferred_x = false;
        }
    }

    // move cursor to last character of selection
    static void stb_textedit_move_to_last(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state)
    {
        if (STB_TEXT_HAS_SELECTION(ref state))
        {
            stb_textedit_sortselection(ref state);
            stb_textedit_clamp(ref str, ref state);
            state.cursor = state.select_end;
            state.select_start = state.select_end;
            state.has_preferred_x = false;
        }
    }

    //# ifdef STB_TEXTEDIT_IS_SPACE
    static bool is_word_boundary(ref STB_TEXTEDIT_STRING str, int idx)
    {
        return idx > 0 ? (STB_TEXTEDIT_IS_SPACE(STB_TEXTEDIT_GETCHAR(ref str, idx - 1)) && !STB_TEXTEDIT_IS_SPACE(STB_TEXTEDIT_GETCHAR(ref str, idx))) : true;
    }

    //# ifndef STB_TEXTEDIT_MOVEWORDLEFT
    static int STB_TEXTEDIT_MOVEWORDLEFT(ref STB_TEXTEDIT_STRING str, int c)
    {
        --c; // always move at least one character
        while (c >= 0 && !is_word_boundary(ref str, c))
            --c;

        if (c < 0)
            c = 0;

        return c;
    }
    //#define STB_TEXTEDIT_MOVEWORDLEFT stb_textedit_move_to_word_previous
    //#endif

    //# ifndef STB_TEXTEDIT_MOVEWORDRIGHT
    static int STB_TEXTEDIT_MOVEWORDRIGHT(ref STB_TEXTEDIT_STRING str, int c)
    {
        int len = STB_TEXTEDIT_STRINGLEN(ref str);
        ++c; // always move at least one character
        while (c < len && !is_word_boundary(ref str, c))
            ++c;

        if (c > len)
            c = len;

        return c;
    }
    //#define STB_TEXTEDIT_MOVEWORDRIGHT stb_textedit_move_to_word_next
    //#endif

    //#endif

    // update selection and cursor to match each other
    static void stb_textedit_prep_selection_at_cursor(ref STB_TexteditState state)
    {
        if (!STB_TEXT_HAS_SELECTION(ref state))
            state.select_start = state.select_end = state.cursor;
        else
            state.cursor = state.select_end;
    }

    // API cut: delete selection
    public static int stb_textedit_cut(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state)
    {
        if (STB_TEXT_HAS_SELECTION(ref state))
        {
            stb_textedit_delete_selection(ref str, ref state); // implicitly clamps
            state.has_preferred_x = false;
            return 1;
        }
        return 0;
    }

    // API paste: replace existing selection with passed-in text
    private static int stb_textedit_paste_internal(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, ReadOnlySpan<STB_TEXTEDIT_CHARTYPE> text, int len)
    {
        // if there's a selection, the paste should delete it
        stb_textedit_clamp(ref str, ref state);
        stb_textedit_delete_selection(ref str, ref state);
        // try to insert the characters
        if (STB_TEXTEDIT_INSERTCHARS(ref str, state.cursor, text, len))
        {
            stb_text_makeundo_insert(ref state, state.cursor, len);
            state.cursor += len;
            state.has_preferred_x = false;
            return 1;
        }
        // note: paste failure will leave deleted selection, may be restored with an undo (see https://github.com/nothings/stb/issues/734 for details)
        return 0;
    }

    // API key: process a keyboard input
    public static void stb_textedit_key(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, STB_TEXTEDIT_KEYTYPE key)
    {
    retry:
        switch (key)
        {
            default:
                {
                    int c = STB_TEXTEDIT_KEYTOTEXT(key);
                    if (c > 0)
                    {
                        Span<STB_TEXTEDIT_CHARTYPE> ch = stackalloc[] { (STB_TEXTEDIT_CHARTYPE)c };

                        // can't add newline in single-line mode
                        if (c == '\n' && state.single_line)
                            break;

                        if (state.insert_mode && !STB_TEXT_HAS_SELECTION(ref state) && state.cursor < STB_TEXTEDIT_STRINGLEN(ref str))
                        {
                            stb_text_makeundo_replace(ref str, ref state, state.cursor, 1, 1);
                            STB_TEXTEDIT_DELETECHARS(ref str, state.cursor, 1);
                            if (STB_TEXTEDIT_INSERTCHARS(ref str, state.cursor, ch, 1))
                            {
                                ++state.cursor;
                                state.has_preferred_x = false;
                            }
                        }
                        else
                        {
                            stb_textedit_delete_selection(ref str, ref state); // implicitly clamps
                            if (STB_TEXTEDIT_INSERTCHARS(ref str, state.cursor, ch, 1))
                            {
                                stb_text_makeundo_insert(ref state, state.cursor, 1);
                                ++state.cursor;
                                state.has_preferred_x = false;
                            }
                        }
                    }
                    break;
                }

            //# ifdef STB_TEXTEDIT_K_INSERT
            case STB_TEXTEDIT_K_INSERT:
                state.insert_mode = !state.insert_mode;
                break;
            //#endif

            case STB_TEXTEDIT_K_UNDO:
                stb_text_undo(ref str, ref state);
                state.has_preferred_x = false;
                break;

            case STB_TEXTEDIT_K_REDO:
                stb_text_redo(ref str, ref state);
                state.has_preferred_x = false;
                break;

            case STB_TEXTEDIT_K_LEFT:
                // if currently there's a selection, move cursor to start of selection
                if (STB_TEXT_HAS_SELECTION(ref state))
                    stb_textedit_move_to_first(ref state);
                else
                   if (state.cursor > 0)
                    --state.cursor;
                state.has_preferred_x = false;
                break;

            case STB_TEXTEDIT_K_RIGHT:
                // if currently there's a selection, move cursor to end of selection
                if (STB_TEXT_HAS_SELECTION(ref state))
                    stb_textedit_move_to_last(ref str, ref state);
                else
                    ++state.cursor;
                stb_textedit_clamp(ref str, ref state);
                state.has_preferred_x = false;
                break;

            case STB_TEXTEDIT_K_LEFT | STB_TEXTEDIT_K_SHIFT:
                stb_textedit_clamp(ref str, ref state);
                stb_textedit_prep_selection_at_cursor(ref state);
                // move selection left
                if (state.select_end > 0)
                    --state.select_end;
                state.cursor = state.select_end;
                state.has_preferred_x = false;
                break;

            //# ifdef STB_TEXTEDIT_MOVEWORDLEFT
            case STB_TEXTEDIT_K_WORDLEFT:
                if (STB_TEXT_HAS_SELECTION(ref state))
                    stb_textedit_move_to_first(ref state);
                else
                {
                    state.cursor = STB_TEXTEDIT_MOVEWORDLEFT(ref str, state.cursor);
                    stb_textedit_clamp(ref str, ref state);
                }
                break;

            case STB_TEXTEDIT_K_WORDLEFT | STB_TEXTEDIT_K_SHIFT:
                if (!STB_TEXT_HAS_SELECTION(ref state))
                    stb_textedit_prep_selection_at_cursor(ref state);

                state.cursor = STB_TEXTEDIT_MOVEWORDLEFT(ref str, state.cursor);
                state.select_end = state.cursor;

                stb_textedit_clamp(ref str, ref state);
                break;
            //#endif

            //# ifdef STB_TEXTEDIT_MOVEWORDRIGHT
            case STB_TEXTEDIT_K_WORDRIGHT:
                if (STB_TEXT_HAS_SELECTION(ref state))
                    stb_textedit_move_to_last(ref str, ref state);
                else
                {
                    state.cursor = STB_TEXTEDIT_MOVEWORDRIGHT(ref str, state.cursor);
                    stb_textedit_clamp(ref str, ref state);
                }
                break;

            case STB_TEXTEDIT_K_WORDRIGHT | STB_TEXTEDIT_K_SHIFT:
                if (!STB_TEXT_HAS_SELECTION(ref state))
                    stb_textedit_prep_selection_at_cursor(ref state);

                state.cursor = STB_TEXTEDIT_MOVEWORDRIGHT(ref str, state.cursor);
                state.select_end = state.cursor;

                stb_textedit_clamp(ref str, ref state);
                break;
            //#endif

            case STB_TEXTEDIT_K_RIGHT | STB_TEXTEDIT_K_SHIFT:
                stb_textedit_prep_selection_at_cursor(ref state);
                // move selection right
                ++state.select_end;
                stb_textedit_clamp(ref str, ref state);
                state.cursor = state.select_end;
                state.has_preferred_x = false;
                break;

            case STB_TEXTEDIT_K_DOWN:
            case STB_TEXTEDIT_K_DOWN | STB_TEXTEDIT_K_SHIFT:
            case STB_TEXTEDIT_K_PGDOWN:
            case STB_TEXTEDIT_K_PGDOWN | STB_TEXTEDIT_K_SHIFT:
                {
                    StbFindState find = new StbFindState();
                    StbTexteditRow row;
                    int i, j;
                    bool sel = (key & STB_TEXTEDIT_K_SHIFT) != 0;
                    bool is_page = (key & ~STB_TEXTEDIT_K_SHIFT) == STB_TEXTEDIT_K_PGDOWN;
                    int row_count = is_page ? state.row_count_per_page : 1;

                    if (!is_page && state.single_line)
                    {
                        // on windows, up&down in single-line behave like left&right
                        key = STB_TEXTEDIT_K_RIGHT | (key & STB_TEXTEDIT_K_SHIFT);
                        goto retry;
                    }

                    if (sel)
                        stb_textedit_prep_selection_at_cursor(ref state);
                    else if (STB_TEXT_HAS_SELECTION(ref state))
                        stb_textedit_move_to_last(ref str, ref state);

                    // compute current position of cursor point
                    stb_textedit_clamp(ref str, ref state);
                    stb_textedit_find_charpos(ref find, ref str, state.cursor, state.single_line);

                    for (j = 0; j < row_count; ++j)
                    {
                        float x, goal_x = state.has_preferred_x ? state.preferred_x : find.x;
                        int start = find.first_char + find.length;

                        if (find.length == 0)
                            break;

                        // now find character position down a row
                        state.cursor = start;
                        STB_TEXTEDIT_LAYOUTROW(out row, ref str, state.cursor);
                        x = row.x0;
                        for (i = 0; i < row.num_chars; ++i)
                        {
                            float dx = STB_TEXTEDIT_GETWIDTH(ref str, start, i);
                            //# ifdef STB_TEXTEDIT_GETWIDTH_NEWLINE
                            if (dx == STB_TEXTEDIT_GETWIDTH_NEWLINE)
                                break;
                            //#endif
                            x += dx;
                            if (x > goal_x)
                                break;
                            ++state.cursor;
                        }
                        stb_textedit_clamp(ref str, ref state);

                        state.has_preferred_x = true;
                        state.preferred_x = goal_x;

                        if (sel)
                            state.select_end = state.cursor;

                        // go to next line
                        find.first_char = find.first_char + find.length;
                        find.length = row.num_chars;
                    }
                    break;
                }

            case STB_TEXTEDIT_K_UP:
            case STB_TEXTEDIT_K_UP | STB_TEXTEDIT_K_SHIFT:
            case STB_TEXTEDIT_K_PGUP:
            case STB_TEXTEDIT_K_PGUP | STB_TEXTEDIT_K_SHIFT:
                {
                    StbFindState find = new StbFindState();
                    StbTexteditRow row;
                    int i, j, prev_scan;
                    bool sel = (key & STB_TEXTEDIT_K_SHIFT) != 0;
                    bool is_page = (key & ~STB_TEXTEDIT_K_SHIFT) == STB_TEXTEDIT_K_PGUP;
                    int row_count = is_page ? state.row_count_per_page : 1;

                    if (!is_page && state.single_line)
                    {
                        // on windows, up&down become left&right
                        key = STB_TEXTEDIT_K_LEFT | (key & STB_TEXTEDIT_K_SHIFT);
                        goto retry;
                    }

                    if (sel)
                        stb_textedit_prep_selection_at_cursor(ref state);
                    else if (STB_TEXT_HAS_SELECTION(ref state))
                        stb_textedit_move_to_first(ref state);

                    // compute current position of cursor point
                    stb_textedit_clamp(ref str, ref state);
                    stb_textedit_find_charpos(ref find, ref str, state.cursor, state.single_line);

                    for (j = 0; j < row_count; ++j)
                    {
                        float x, goal_x = state.has_preferred_x ? state.preferred_x : find.x;

                        // can only go up if there's a previous row
                        if (find.prev_first == find.first_char)
                            break;

                        // now find character position up a row
                        state.cursor = find.prev_first;
                        STB_TEXTEDIT_LAYOUTROW(out row, ref str, state.cursor);
                        x = row.x0;
                        for (i = 0; i < row.num_chars; ++i)
                        {
                            float dx = STB_TEXTEDIT_GETWIDTH(ref str, find.prev_first, i);
                            //# ifdef STB_TEXTEDIT_GETWIDTH_NEWLINE
                            if (dx == STB_TEXTEDIT_GETWIDTH_NEWLINE)
                                break;
                            //#endif
                            x += dx;
                            if (x > goal_x)
                                break;
                            ++state.cursor;
                        }
                        stb_textedit_clamp(ref str, ref state);

                        state.has_preferred_x = true;
                        state.preferred_x = goal_x;

                        if (sel)
                            state.select_end = state.cursor;

                        // go to previous line
                        // (we need to scan previous line the hard way. maybe we could expose this as a new API function?)
                        prev_scan = find.prev_first > 0 ? find.prev_first - 1 : 0;
                        while (prev_scan > 0 && STB_TEXTEDIT_GETCHAR(ref str, prev_scan - 1) != STB_TEXTEDIT_NEWLINE)
                            --prev_scan;
                        find.first_char = find.prev_first;
                        find.prev_first = prev_scan;
                    }
                    break;
                }

            case STB_TEXTEDIT_K_DELETE:
            case STB_TEXTEDIT_K_DELETE | STB_TEXTEDIT_K_SHIFT:
                if (STB_TEXT_HAS_SELECTION(ref state))
                    stb_textedit_delete_selection(ref str, ref state);
                else
                {
                    int n = STB_TEXTEDIT_STRINGLEN(ref str);
                    if (state.cursor < n)
                        stb_textedit_delete(ref str, ref state, state.cursor, 1);
                }
                state.has_preferred_x = false;
                break;

            case STB_TEXTEDIT_K_BACKSPACE:
            case STB_TEXTEDIT_K_BACKSPACE | STB_TEXTEDIT_K_SHIFT:
                if (STB_TEXT_HAS_SELECTION(ref state))
                    stb_textedit_delete_selection(ref str, ref state);
                else
                {
                    stb_textedit_clamp(ref str, ref state);
                    if (state.cursor > 0)
                    {
                        stb_textedit_delete(ref str, ref state, state.cursor - 1, 1);
                        --state.cursor;
                    }
                }
                state.has_preferred_x = false;
                break;

            //# ifdef STB_TEXTEDIT_K_TEXTSTART2
            case STB_TEXTEDIT_K_TEXTSTART2:
            //#endif
            case STB_TEXTEDIT_K_TEXTSTART:
                state.cursor = state.select_start = state.select_end = 0;
                state.has_preferred_x = false;
                break;

            //# ifdef STB_TEXTEDIT_K_TEXTEND2
            case STB_TEXTEDIT_K_TEXTEND2:
            //#endif
            case STB_TEXTEDIT_K_TEXTEND:
                state.cursor = STB_TEXTEDIT_STRINGLEN(ref str);
                state.select_start = state.select_end = 0;
                state.has_preferred_x = false;
                break;

            //# ifdef STB_TEXTEDIT_K_TEXTSTART2
            case STB_TEXTEDIT_K_TEXTSTART2 | STB_TEXTEDIT_K_SHIFT:
            //#endif
            case STB_TEXTEDIT_K_TEXTSTART | STB_TEXTEDIT_K_SHIFT:
                stb_textedit_prep_selection_at_cursor(ref state);
                state.cursor = state.select_end = 0;
                state.has_preferred_x = false;
                break;

            //# ifdef STB_TEXTEDIT_K_TEXTEND2
            case STB_TEXTEDIT_K_TEXTEND2 | STB_TEXTEDIT_K_SHIFT:
            //#endif
            case STB_TEXTEDIT_K_TEXTEND | STB_TEXTEDIT_K_SHIFT:
                stb_textedit_prep_selection_at_cursor(ref state);
                state.cursor = state.select_end = STB_TEXTEDIT_STRINGLEN(ref str);
                state.has_preferred_x = false;
                break;


            //# ifdef STB_TEXTEDIT_K_LINESTART2
            case STB_TEXTEDIT_K_LINESTART2:
            //#endif
            case STB_TEXTEDIT_K_LINESTART:
                stb_textedit_clamp(ref str, ref state);
                stb_textedit_move_to_first(ref state);
                if (state.single_line)
                    state.cursor = 0;
                else while (state.cursor > 0 && STB_TEXTEDIT_GETCHAR(ref str, state.cursor - 1) != STB_TEXTEDIT_NEWLINE)
                        --state.cursor;
                state.has_preferred_x = false;
                break;

            //# ifdef STB_TEXTEDIT_K_LINEEND2
            case STB_TEXTEDIT_K_LINEEND2:
            //#endif
            case STB_TEXTEDIT_K_LINEEND:
                {
                    int n = STB_TEXTEDIT_STRINGLEN(ref str);
                    stb_textedit_clamp(ref str, ref state);
                    stb_textedit_move_to_first(ref state);
                    if (state.single_line)
                        state.cursor = n;
                    else while (state.cursor < n && STB_TEXTEDIT_GETCHAR(ref str, state.cursor) != STB_TEXTEDIT_NEWLINE)
                            ++state.cursor;
                    state.has_preferred_x = false;
                    break;
                }

            //# ifdef STB_TEXTEDIT_K_LINESTART2
            case STB_TEXTEDIT_K_LINESTART2 | STB_TEXTEDIT_K_SHIFT:
            //#endif
            case STB_TEXTEDIT_K_LINESTART | STB_TEXTEDIT_K_SHIFT:
                stb_textedit_clamp(ref str, ref state);
                stb_textedit_prep_selection_at_cursor(ref state);
                if (state.single_line)
                    state.cursor = 0;
                else while (state.cursor > 0 && STB_TEXTEDIT_GETCHAR(ref str, state.cursor - 1) != STB_TEXTEDIT_NEWLINE)
                        --state.cursor;
                state.select_end = state.cursor;
                state.has_preferred_x = false;
                break;

            //# ifdef STB_TEXTEDIT_K_LINEEND2
            case STB_TEXTEDIT_K_LINEEND2 | STB_TEXTEDIT_K_SHIFT:
            //#endif
            case STB_TEXTEDIT_K_LINEEND | STB_TEXTEDIT_K_SHIFT:
                {
                    int n = STB_TEXTEDIT_STRINGLEN(ref str);
                    stb_textedit_clamp(ref str, ref state);
                    stb_textedit_prep_selection_at_cursor(ref state);
                    if (state.single_line)
                        state.cursor = n;
                    else while (state.cursor < n && STB_TEXTEDIT_GETCHAR(ref str, state.cursor) != STB_TEXTEDIT_NEWLINE)
                            ++state.cursor;
                    state.select_end = state.cursor;
                    state.has_preferred_x = false;
                    break;
                }
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    //
    //      Undo processing
    //
    // @OPTIMIZE: the undo/redo buffer should be circular

    static void stb_textedit_flush_redo(ref StbUndoState state)
    {
        state.redo_point = STB_TEXTEDIT_UNDOSTATECOUNT;
        state.redo_char_point = STB_TEXTEDIT_UNDOCHARCOUNT;
    }

    // discard the oldest entry in the undo list
    static void stb_textedit_discard_undo(ref StbUndoState state)
    {
        if (state.undo_point > 0)
        {
            // if the 0th undo state has characters, clean those up
            if (state.undo_rec[0].char_storage >= 0)
            {
                int n = state.undo_rec[0].insert_length, i;
                // delete n characters from all other records
                state.undo_char_point -= n;
                STB_TEXTEDIT_memmove(state.undo_char, state.undo_char.AsSpan().Slice(n), state.undo_char_point);
                for (i = 0; i < state.undo_point; ++i)
                    if (state.undo_rec[i].char_storage >= 0)
                        state.undo_rec[i].char_storage -= n; // @OPTIMIZE: get rid of char_storage and infer it
            }
            --state.undo_point;
            STB_TEXTEDIT_memmove(state.undo_rec, state.undo_rec.AsSpan().Slice(1), state.undo_point);
        }
    }

    // discard the oldest entry in the redo list--it's bad if this
    // ever happens, but because undo & redo have to store the actual
    // characters in different cases, the redo character buffer can
    // fill up even though the undo buffer didn't
    static void stb_textedit_discard_redo(ref StbUndoState state)
    {
        int k = STB_TEXTEDIT_UNDOSTATECOUNT - 1;

        if (state.redo_point <= k)
        {
            // if the k'th undo state has characters, clean those up
            if (state.undo_rec[k].char_storage >= 0)
            {
                int n = state.undo_rec[k].insert_length, i;
                // move the remaining redo character data to the end of the buffer
                state.redo_char_point += n;
                STB_TEXTEDIT_memmove(state.undo_char.AsSpan().Slice(state.redo_char_point), state.undo_char.AsSpan().Slice(state.redo_char_point - n), STB_TEXTEDIT_UNDOCHARCOUNT - state.redo_char_point);
                // adjust the position of all the other records to account for above memmove
                for (i = state.redo_point; i < k; ++i)
                    if (state.undo_rec[i].char_storage >= 0)
                        state.undo_rec[i].char_storage += n;
            }
            // now move all the redo records towards the end of the buffer; the first one is at 'redo_point'
            STB_TEXTEDIT_memmove(state.undo_rec.AsSpan().Slice(state.redo_point + 1), state.undo_rec.AsSpan().Slice(state.redo_point), STB_TEXTEDIT_UNDOSTATECOUNT - state.redo_point);
            // now move redo_point to point to the new one
            ++state.redo_point;
        }
    }

    static Ptr<StbUndoRecord> stb_text_create_undo_record(ref StbUndoState state, int numchars)
    {
        // any time we create a new undo record, we discard redo
        stb_textedit_flush_redo(ref state);

        // if we have no free records, we have to make room, by sliding the
        // existing records down
        if (state.undo_point == STB_TEXTEDIT_UNDOSTATECOUNT)
            stb_textedit_discard_undo(ref state);

        // if the characters to store won't possibly fit in the buffer, we can't undo
        if (numchars > STB_TEXTEDIT_UNDOCHARCOUNT)
        {
            state.undo_point = 0;
            state.undo_char_point = 0;
            return Ptr<StbUndoRecord>.Null;
        }

        // if we don't have enough free characters in the buffer, we have to make room
        while (state.undo_char_point + numchars > STB_TEXTEDIT_UNDOCHARCOUNT)
            stb_textedit_discard_undo(ref state);

        return new Ptr<StbUndoRecord>(state.undo_rec)[state.undo_point++];
    }

    static Ptr<STB_TEXTEDIT_CHARTYPE> stb_text_createundo(ref StbUndoState state, int pos, int insert_len, int delete_len)
    {
        var rp = stb_text_create_undo_record(ref state, insert_len);
        if (rp.IsNull)
            return Ptr<STB_TEXTEDIT_CHARTYPE>.Null;

        ref var r = ref rp.Ref;

        r.where = pos;
        r.insert_length = (STB_TEXTEDIT_POSITIONTYPE)insert_len;
        r.delete_length = (STB_TEXTEDIT_POSITIONTYPE)delete_len;

        if (insert_len == 0)
        {
            r.char_storage = -1;
            return Ptr<STB_TEXTEDIT_CHARTYPE>.Null;
        }
        else
        {
            r.char_storage = state.undo_char_point;
            state.undo_char_point += insert_len;
            return new Ptr<STB_TEXTEDIT_CHARTYPE>(state.undo_char)[r.char_storage];
        }
    }

    static void stb_text_undo(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state)
    {
        ref StbUndoState s = ref state.undostate;
        StbUndoRecord u;
        if (s.undo_point == 0)
            return;

        // we need to do two things: apply the undo record, and create a redo record
        u = s.undo_rec[s.undo_point - 1];
        ref StbUndoRecord r = ref s.undo_rec[s.redo_point - 1];
        r.char_storage = -1;

        r.insert_length = u.delete_length;
        r.delete_length = u.insert_length;
        r.where = u.where;

        if (u.delete_length != 0)
        {
            // if the undo record says to delete characters, then the redo record will
            // need to re-insert the characters that get deleted, so we need to store
            // them.

            // there are three cases:
            //    there's enough room to store the characters
            //    characters stored for *redoing* don't leave room for redo
            //    characters stored for *undoing* don't leave room for redo
            // if the last is true, we have to bail

            if (s.undo_char_point + u.delete_length >= STB_TEXTEDIT_UNDOCHARCOUNT)
            {
                // the undo records take up too much character space; there's no space to store the redo characters
                r.insert_length = 0;
            }
            else
            {
                int i;

                // there's definitely room to store the characters eventually
                while (s.undo_char_point + u.delete_length > s.redo_char_point)
                {
                    // should never happen:
                    if (s.redo_point == STB_TEXTEDIT_UNDOSTATECOUNT)
                        return;
                    // there's currently not enough room, so discard a redo record
                    stb_textedit_discard_redo(ref s);
                }
                r = ref s.undo_rec[s.redo_point - 1];

                r.char_storage = s.redo_char_point - u.delete_length;
                s.redo_char_point = s.redo_char_point - u.delete_length;

                // now save the characters
                for (i = 0; i < u.delete_length; ++i)
                    s.undo_char[r.char_storage + i] = STB_TEXTEDIT_GETCHAR(ref str, u.where + i);
            }

            // now we can carry out the deletion
            STB_TEXTEDIT_DELETECHARS(ref str, u.where, u.delete_length);
        }

        // check type of recorded action:
        if (u.insert_length != 0)
        {
            // easy case: was a deletion, so we need to insert n characters
            STB_TEXTEDIT_INSERTCHARS(ref str, u.where, s.undo_char.AsSpan().Slice(u.char_storage), u.insert_length);
            s.undo_char_point -= u.insert_length;
        }

        state.cursor = u.where + u.insert_length;

        s.undo_point--;
        s.redo_point--;
    }

    static void stb_text_redo(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state)
    {
        ref StbUndoState s = ref state.undostate;
        StbUndoRecord r;
        if (s.redo_point == STB_TEXTEDIT_UNDOSTATECOUNT)
            return;

        // we need to do two things: apply the redo record, and create an undo record
        ref StbUndoRecord u = ref s.undo_rec[s.undo_point];
        r = s.undo_rec[s.redo_point];

        // we KNOW there must be room for the undo record, because the redo record
        // was derived from an undo record

        u.delete_length = r.insert_length;
        u.insert_length = r.delete_length;
        u.where = r.where;
        u.char_storage = -1;

        if (r.delete_length != 0)
        {
            // the redo record requires us to delete characters, so the undo record
            // needs to store the characters

            if (s.undo_char_point + u.insert_length > s.redo_char_point)
            {
                u.insert_length = 0;
                u.delete_length = 0;
            }
            else
            {
                int i;
                u.char_storage = s.undo_char_point;
                s.undo_char_point = s.undo_char_point + u.insert_length;

                // now save the characters
                for (i = 0; i < u.insert_length; ++i)
                    s.undo_char[u.char_storage + i] = STB_TEXTEDIT_GETCHAR(ref str, u.where + i);
            }

            STB_TEXTEDIT_DELETECHARS(ref str, r.where, r.delete_length);
        }

        if (r.insert_length != 0)
        {
            // easy case: need to insert n characters
            STB_TEXTEDIT_INSERTCHARS(ref str, r.where, s.undo_char.AsSpan().Slice(r.char_storage), r.insert_length);
            s.redo_char_point += r.insert_length;
        }

        state.cursor = r.where + r.insert_length;

        s.undo_point++;
        s.redo_point++;
    }

    static void stb_text_makeundo_insert(ref STB_TexteditState state, int where, int length)
    {
        stb_text_createundo(ref state.undostate, where, 0, length);
    }

    static void stb_text_makeundo_delete(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, int where, int length)
    {
        int i;
        var p = stb_text_createundo(ref state.undostate, where, length, 0);
        if (!p.IsNull)
        {
            for (i = 0; i < length; ++i)
                p[i].Ref = STB_TEXTEDIT_GETCHAR(ref str, where + i);
        }
    }

    static void stb_text_makeundo_replace(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, int where, int old_length, int new_length)
    {
        int i;
        var p = stb_text_createundo(ref state.undostate, where, old_length, new_length);
        if (!p.IsNull)
        {
            for (i = 0; i < old_length; ++i)
                p[i].Ref = STB_TEXTEDIT_GETCHAR(ref str, where + i);
        }
    }

    // reset the state to default
    static void stb_textedit_clear_state(ref STB_TexteditState state, bool is_single_line)
    {
        state.undostate.undo_point = 0;
        state.undostate.undo_char_point = 0;
        state.undostate.redo_point = STB_TEXTEDIT_UNDOSTATECOUNT;
        state.undostate.redo_char_point = STB_TEXTEDIT_UNDOCHARCOUNT;
        state.select_end = state.select_start = 0;
        state.cursor = 0;
        state.has_preferred_x = false;
        state.preferred_x = 0;
        state.cursor_at_end_of_line = false;
        state.initialized = true;
        state.single_line = is_single_line;
        state.insert_mode = false;
        state.row_count_per_page = 0;
    }

    // API initialize
    public static void stb_textedit_initialize_state(ref STB_TexteditState state, bool is_single_line)
    {
        stb_textedit_clear_state(ref state, is_single_line);
    }

    //#if defined(__GNUC__) || defined(__clang__)
    //#pragma GCC diagnostic push
    //#pragma GCC diagnostic ignored "-Wcast-qual"
    //#endif

    public static int stb_textedit_paste(ref STB_TEXTEDIT_STRING str, ref STB_TexteditState state, ReadOnlySpan<STB_TEXTEDIT_CHARTYPE> ctext, int len)
    {
        return stb_textedit_paste_internal(ref str, ref state, ctext, len);
    }

    //#if defined(__GNUC__) || defined(__clang__)
    //#pragma GCC diagnostic pop
    //#endif

    //#endif//STB_TEXTEDIT_IMPLEMENTATION

    /*
    ------------------------------------------------------------------------------
    This software is available under 2 licenses -- choose whichever you prefer.
    ------------------------------------------------------------------------------
    ALTERNATIVE A - MIT License
    Copyright (c) 2017 Sean Barrett
    Permission is hereby granted, free of charge, to any person obtaining a copy of
    this software and associated documentation files (the "Software"), to deal in
    the Software without restriction, including without limitation the rights to
    use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
    of the Software, and to permit persons to whom the Software is furnished to do
    so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    ------------------------------------------------------------------------------
    ALTERNATIVE B - Public Domain (www.unlicense.org)
    This is free and unencumbered software released into the public domain.
    Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
    software, either in source code form or as a compiled binary, for any purpose,
    commercial or non-commercial, and by any means.
    In jurisdictions that recognize copyright laws, the author or authors of this
    software dedicate any and all copyright interest in the software to the public
    domain. We make this dedication for the benefit of the public at large and to
    the detriment of our heirs and successors. We intend this dedication to be an
    overt act of relinquishment in perpetuity of all present and future rights to
    this software under copyright law.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
    ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    ------------------------------------------------------------------------------
    */
}
