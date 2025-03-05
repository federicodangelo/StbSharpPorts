#pragma warning disable CA1416 // Validate platform compatibility

using System.ComponentModel;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.FileProviders;
using StbSharp;
using StbSharp.StbCommon;

public partial class Sample
{
    // Make the method accessible from JS
    [JSExport]
    public static int Add(int a, int b)
    {
        return a + b;
    }

    private static Sample instance;

    [JSExport]
    public static string Init()
    {
        instance = new Sample();
        
        return instance.Run();
    }

    public Sample()
    {
        InitFont();
        InitStbGui();
    }

    private string Run(string text = "hello World", string fileName = "Fonts/Karla-Regular.ttf", int fontSize = 20)
    {
        var console = "";

        int ScreenWidth = 100;
        int ScreenHeight = fontSize;

        BytePtr screen = new BytePtr(new byte[ScreenHeight * ScreenWidth]);

        StbTrueType.stbtt_fontinfo font;
        int baseline, ch = 0;
        float scale, xpos = 2; // leave a little padding in case the character extends left
        
        byte[] buffer = GetBytes(fileName);
        StbTrueType.stbtt_InitFont(out font, buffer, 0);

        scale = StbTrueType.stbtt_ScaleForPixelHeight(ref font, fontSize);
        StbTrueType.stbtt_GetFontVMetrics(ref font, out int ascent, out _, out int _);
        
        baseline = (int)(ascent * scale);

        while (ch < text.Length)
        {
            int advance, lsb, x0, y0, x1, y1;
            float x_shift = xpos - (float)MathF.Floor(xpos);

            StbTrueType.stbtt_GetCodepointHMetrics(ref font, text[ch], out advance, out lsb);
            StbTrueType.stbtt_GetCodepointBitmapBoxSubpixel(ref font, text[ch], scale, scale, x_shift, 0, out x0, out y0, out x1, out y1);
            StbTrueType.stbtt_MakeCodepointBitmapSubpixel(ref font, screen[(baseline + y0) * ScreenWidth + (int)xpos + x0], x1 - x0, y1 - y0, ScreenWidth, scale, scale, x_shift, 0, text[ch]);
            
            // note that this stomps the old data, so where character boxes overlap (e.g. 'lj') it's wrong
            // because this API is really for baking character bitmaps into textures. if you want to render
            // a sequence of characters, you really need to render each bitmap to a temp buffer, then
            // "alpha blend" that into the working buffer
            xpos += (advance * scale);
            if (ch + 1 < text.Length)
                xpos += scale * StbTrueType.stbtt_GetCodepointKernAdvance(ref font, text[ch], text[ch + 1]);
            ++ch;
        }

        for (int j = 0; j < ScreenHeight; ++j)
        {
            string line = "";
            for (int i = 0; i < ScreenWidth - 1; ++i)
                line += " .:ioVM@"[(screen[j * ScreenWidth + i].Value) >> 5];
            console += line + "\n";
        }

        return console;
    }    

    private StbGuiFont font;

    private DummyRenderAdapter render_adapter = new DummyRenderAdapter();

    private EmbeddedFileProvider embeddedFileProvider;

    private byte[] GetBytes(string path)
    {
        if (embeddedFileProvider == null)
            embeddedFileProvider = new EmbeddedFileProvider(typeof(Sample).Assembly);

        var file = embeddedFileProvider.GetFileInfo(path);

        if (!file.Exists)
            throw new FileNotFoundException(path);

        using (var memoryStream = new MemoryStream())
        {
            using (var stream = file.CreateReadStream())
            {
                stream.CopyTo(memoryStream);
            }
            return memoryStream.ToArray();
        }

    }


    private void InitFont()
    {
        var bytes = GetBytes("Fonts/ProggyClean.ttf");
        font = new StbGuiFont("ProggyClean", bytes, 13, 1, false, render_adapter);
    }

    private void InitStbGui()
    {
        // Initialize the GUI
        StbGui.stbg_init(BuildExternalDependencies(),
            new()
            {
                assert_behavior = StbGui.STBG_ASSERT_BEHAVIOR.EXCEPTION
            }
        );


        var font_id = StbGui.stbg_add_font(font.name);
        StbGui.stbg_init_default_theme(font_id, new() { color = StbGui.STBG_COLOR_BLACK, size = font.size, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE });
        StbGui.stbg_set_screen_size(1920, 1080);
    }

    private StbGui.stbg_external_dependencies BuildExternalDependencies()
    {
        return new()
        {
            measure_text = (text, font, style, options) => new StbGui.stbg_size() { width = text.Length * style.size, height = style.size },
            render = (commands) =>
            {
            },
            copy_text_to_clipboard = (text) => { },
            get_clipboard_text = () => "",
            get_character_position_in_text = (text, font, style, options, character_index) =>
            {
                int x = 0;
                int y = 0;
                var single_line = (options & StbGui.STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE) != 0;

                for (int i = 0; i < character_index; i++)
                {
                    char c = text[i];
                    if (i == character_index)
                    {
                        break;
                    }
                    if (c == '\n')
                    {
                        if (single_line)
                        {
                            c = ' ';
                        }
                        else
                        {
                            y += 1;
                            x = 0;
                            continue;
                        }
                    }
                    x++;
                }

                return StbGui.stbg_build_position(x, y);
            },
            set_input_method_editor = (info) => { },
            get_time_milliseconds = () => 0,
            get_performance_counter = () => 0,
            get_performance_counter_frequency = () => 1,
        };

    }
}