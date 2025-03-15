using StbSharp;

public class TestTextboxesWindow : TestWindow
{
    public TestTextboxesWindow(StbGuiAppBase appBase, StbGuiStringMemoryPool mp) : base("Test Textboxes", appBase, mp)
    {
        text_to_edit = StbGui.stbg_textbox_build_text_to_edit(1024, "Hello World THIS IS A VERY LONG TEXT TO EDITTTT");
        text_to_edit2 = StbGui.stbg_textbox_build_text_to_edit(1024, "Hello World THIS IS MULTILINE!\nYESSS!!!");
    }

    private StbGui.stbg_textbox_text_to_edit text_to_edit;
    private StbGui.stbg_textbox_text_to_edit text_to_edit2;

    public override void Render()
    {
        if (StbGui.stbg_begin_window(title, ref open))
        {
            StbGui.stbg_textbox("textbox1", ref text_to_edit);
            StbGui.stbg_textbox("textbox2", ref text_to_edit2, 3);

            StbGui.stbg_end_window();
        }
    }
}
