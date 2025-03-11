namespace StbSharp.Tests;

public class StbGuiTextboxTests : StbGuiTestsBase
{
    [Fact]
    public void TestRenderEmptyTextbox()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(32);
            StbGui.stbg_textbox("textbox", ref text_to_edit, 1);
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
            var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(32, "Hello");
            StbGui.stbg_textbox("textbox", ref text_to_edit, 1);
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
            var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(32, "Hello World");
            StbGui.stbg_textbox("textbox", ref text_to_edit, 1);
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
            var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(32, "Hello\nWorld");
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Hello World\nWith a very long line\nand another\nand other one");
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(32, "Hello\nWorld");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Hello World Long Line\n\nAnother Line");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.RIGHT, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Hello World Long Line\nAnother Line");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        SetMousePosition(1, 1);
        SetMouseButton1(true);
        PressKey(StbGui.STBG_KEYBOARD_KEY.DOWN, StbGui.STBG_KEYBOARD_MODIFIER_FLAGS.NONE, true);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Hello World Long Line\nAnother Line");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Hello World Long Line\nAnother Line");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Hello World Long Line\nAnother Line");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Hello World Long Line\nAnother Line");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Line 1\nLine 2\nLine 3\nLine 4\nLine 5\nLine 6\nLine 7\nLine 8\nLine 9");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Line 1\nLine 2\nLine 3\nLine 4\nLine 5\nLine 6\nLine 7\nLine 8\nLine 9");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        Assert.Equal("HELLO", text_to_edit.text.Slice(0, text_to_edit.length).Span.ToString());

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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
            StbGui.stbg_set_last_widget_size(10, 0);
        }
        StbGui.stbg_end_frame();

        Assert.Equal("HEL", text_to_edit.text.Slice(0, text_to_edit.length).Span.ToString());

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

        var text_to_edit = StbGui.stbg_textbox_build_text_to_edit(64, "Hello World");

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
            StbGui.stbg_textbox("textbox", ref text_to_edit, 3);
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
