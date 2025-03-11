using StbSharp;

public class TestImagesWindow : TestWindow
{
    private bool show_subimages = true;
    private readonly int test_image_id;
    private readonly int[] test_sub_images = new int[4];

    public TestImagesWindow(StbGuiAppBase appBase, StbGuiStringMemoryPool mp) : base("Test Images", appBase, mp)
    {
        test_image_id = appBase.add_image(GetResourceFileBytes("test.png"), false);
        test_sub_images[0] = appBase.add_sub_image(test_image_id, 0, 0, 128, 128);
        test_sub_images[1] = appBase.add_sub_image(test_image_id, 128, 0, 128, 128);
        test_sub_images[2] = appBase.add_sub_image(test_image_id, 0, 128, 128, 128);
        test_sub_images[3] = appBase.add_sub_image(test_image_id, 128, 128, 128, 128);
    }

    static private byte[] GetResourceFileBytes(string fileName)
    {
        var resourceName = "StbGui.Examples.Resources." + fileName.Replace("\\", ".").Replace("/", ".");

        using (var memoryStream = new MemoryStream())
        {
            using (var stream = typeof(TestImagesWindow).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Resource file not found.");

                stream.CopyTo(memoryStream);
            }
            return memoryStream.ToArray();
        }
    }

    public override void Render()
    {
        if (StbGui.stbg_begin_window(title, ref open))
        {
            StbGui.stbg_set_last_widget_size_if_new(400, 300);

            StbGui.stbg_begin_container("hor", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL);
            {
                StbGui.stbg_image("image", test_image_id, 0.5f);

                if (show_subimages)
                {
                    StbGui.stbg_begin_container("subV1", StbGui.STBG_CHILDREN_LAYOUT.VERTICAL);
                    {
                        StbGui.stbg_image(mp.Build("subimage") + 0, test_sub_images[0], 0.5f);
                        StbGui.stbg_image(mp.Build("subimage") + 2, test_sub_images[2], 0.5f);
                    }
                    StbGui.stbg_end_container();

                    StbGui.stbg_begin_container("subV2", StbGui.STBG_CHILDREN_LAYOUT.VERTICAL);
                    {
                        StbGui.stbg_image(mp.Build("subimage") + 1, test_sub_images[1], 0.5f);
                        StbGui.stbg_image(mp.Build("subimage") + 3, test_sub_images[3], 0.5f);
                    }
                    StbGui.stbg_end_container();
                }
            }
            StbGui.stbg_end_container();

            StbGui.stbg_label("The image below is a button!");
            if (StbGui.stbg_button("imagebutton", test_sub_images[0], test_sub_images[1], test_sub_images[2], true, 0.25f))
                show_subimages = !show_subimages;

            StbGui.stbg_end_window();
        }
    }
}
