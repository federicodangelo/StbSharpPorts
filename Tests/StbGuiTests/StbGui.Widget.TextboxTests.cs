namespace StbSharp.Tests;

public class StbGuiTextboxTests : StbGuiTestsBase
{
    [Fact]
    public void TestRenderEmptyTextbox()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            Memory<char> text = new Memory<char>(new char[32]);
            int text_length = 0;
            StbGui.stbg_textbox("textbox", text, ref text_length, 1);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
      ]);
    }

    [Fact]
    public void TestRenderNonEmptyTextbox()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            var str = "Hello";
            Memory<char> text = new Memory<char>(new char[32]);
            str.AsSpan().CopyTo(text.Span);
            int text_length = str.Length;
            StbGui.stbg_textbox("textbox", text, ref text_length, 1);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|Hello   |""", "CB1CK5CT3CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestRenderHorizontalOverflowTextbox()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            var str = "Hello World";
            Memory<char> text = new Memory<char>(new char[32]);
            str.AsSpan().CopyTo(text.Span);
            int text_length = str.Length;
            StbGui.stbg_textbox("textbox", text, ref text_length, 1);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|Hello Wo|""", "CB1CK8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestRenderMultilineTextbox()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            var str = "Hello\nWorld";
            Memory<char> text = new Memory<char>(new char[32]);
            str.AsSpan().CopyTo(text.Span);
            int text_length = str.Length;
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|Hello   |""", "CB1CK5CT3CB1"],
            ["""|World   |""", "CB1CK5CT3CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestRenderMultilineVerticalOverflowTextbox()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            var str = "Hello World\nWith a very long line\nand another\nand other one";
            Memory<char> text = new Memory<char>(new char[64]);
            str.AsSpan().CopyTo(text.Span);
            int text_length = str.Length;
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|Hello Wo|""", "CB1CK8CB1"],
            ["""|With a v|""", "CB1CK8CB1"],
            ["""|and anot|""", "CB1CK8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditMultilineTextbox()
    {
        InitGUI();

        var str = "Hello\nWorld";
        Memory<char> text = new Memory<char>(new char[32]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""| ello   |""", "CB1YK1CK4CT3CB1"],
            ["""|World   |""", "CB1CK5CT3CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditMultilineMoveCursorRightTextbox()
    {
        InitGUI();

        var str = "Hello World Long Line\n\nAnother Line";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|H llo Wo|""", "CB1CK1YK1CK6CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""|Another |""", "CB1CK8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditMultilineMoveCursorDownTextbox()
    {
        InitGUI();

        var str = "Hello World Long Line\nAnother Line";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|Hello Wo|""", "CB1CK8CB1"],
            ["""| nother |""", "CB1YK1CK7CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditMultilineMoveCursorUpTextbox()
    {
        InitGUI();

        var str = "Hello World Long Line\nAnother Line";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.UP, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|Hello Wo|""", "CB1CK8CB1"],
            ["""| nother |""", "CB1YK1CK7CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditMultilineMoveCursorLeftTextbox()
    {
        InitGUI();

        var str = "Hello World Long Line\nAnother Line";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|H llo Wo|""", "CB1CK1YK1CK6CB1"],
            ["""|Another |""", "CB1CK8CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditMultilineMoveCursorRightUntilScrollTextbox()
    {
        InitGUI();

        var str = "Hello World Long Line\nAnother Line";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|lo World|""", "CB1CK8CB1"],
            ["""|ther Lin|""", "CB1CK8CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditMultilineMoveCursorLeftUntilScrollTextbox()
    {
        InitGUI();

        var str = "Hello World Long Line\nAnother Line";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.LEFT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""| llo Wor|""", "CB1YK1CK7CB1"],
            ["""|nother L|""", "CB1CK8CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditMultilineMoveCursorDownUntilScrollTextbox()
    {
        InitGUI();

        var str = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5\nLine 6\nLine 7\nLine 8\nLine 9";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|Line 4  |""", "CB1CK6CT2CB1"],
            ["""|Line 5  |""", "CB1CK6CT2CB1"],
            ["""| ine 6  |""", "CB1YK1CK5CT2CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }


    [Fact]
    public void TestEditMultilineMoveCursorUpUntilScrollTextbox()
    {
        InitGUI();

        var str = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5\nLine 6\nLine 7\nLine 8\nLine 9";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        PressKey(StbGui.STBG_KEYBOARD_KEY.UP, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.UP, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.UP, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.UP, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""| ine 2  |""", "CB1YK1CK5CT2CB1"],
            ["""|Line 3  |""", "CB1CK6CT2CB1"],
            ["""|Line 4  |""", "CB1CK6CT2CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditAddCharactersToTextbox()
    {
        InitGUI();

        var str = "";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKeyCharacter('H', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKeyCharacter('E', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKeyCharacter('L', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKeyCharacter('L', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKeyCharacter('O', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        Assert.Equal("HELLO", text.Slice(0, text_length).Span.ToString());

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|HELLO   |""", "CB1CK5YT1CT2CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

    [Fact]
    public void TestEditRemoveCharactersToTextbox()
    {
        InitGUI();

        var str = "";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKeyCharacter('H', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKeyCharacter('E', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKeyCharacter('L', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKeyCharacter('L', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKeyCharacter('O', StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.BACKSPACE, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.BACKSPACE, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        Assert.Equal("HEL", text.Slice(0, text_length).Span.ToString());

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|HEL     |""", "CB1CK3YT1CT4CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }

[Fact]
    public void TestRenderSelectedText()
    {
        InitGUI();

        var str = "Hello World";
        Memory<char> text = new Memory<char>(new char[64]);
        str.AsSpan().CopyTo(text.Span);
        int text_length = str.Length;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.SHIFT, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.SHIFT, true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.SHIFT, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", text, ref text_length, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------\""", "CB10"],
            ["""|Hello Wo|""", "CB1YC3CK5CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""|        |""", "CB1CT8CB1"],
            ["""\--------/""", "CB10"],
        ]);
    }    
}