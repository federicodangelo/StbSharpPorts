#pragma warning disable CA1416 // Validate platform compatibility

using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;

namespace StbTrueTypeSharp;

public class BatchBuildFontsExample
{
    struct BakedCharData
    {
        public int codePoint;
        public string character;
        public StbTrueType.stbtt_bakedchar bounds;
    }

    static public void BuildBitmapFonts()
    {
        string[] SampleFonts = [
            "SampleFonts/Cousine-Regular.ttf",
            "SampleFonts/DroidSans.ttf",
            "SampleFonts/Karla-Regular.ttf",
            "SampleFonts/ProggyClean.ttf",
            "SampleFonts/ProggyTiny.ttf",
            "SampleFonts/Roboto-Medium.ttf",
        ];

        float[] SampleFontSizes = [13.0f, 16.0f, 24.0f, 32.0f];

        const string OutputDirectory = "OutputBitmapFonts";

        Directory.Delete(OutputDirectory, true);

        foreach (var sampleFont in SampleFonts)
        {
            foreach (var sampleFontSize in SampleFontSizes)
            {
                var fontOptions = new BuildFontBitmapExample.FontOptions
                {
                    fontFilename = sampleFont,
                    fontSize = sampleFontSize,
                };

                var bitmapOptions = new BuildFontBitmapExample.BitmapOptions();

                var output = BuildFontBitmapExample.BuildFontBitmap(fontOptions, bitmapOptions);

                if (output == null)
                {
                    Console.Error.WriteLine("Failed to generate font " + sampleFont + " with size " + sampleFontSize);
                    continue;
                }

                var outputImageFileName = $"{OutputDirectory}/{Path.GetFileNameWithoutExtension(sampleFont)}-{(int)sampleFontSize}.png";
                var outputJsonFileName = $"{OutputDirectory}/{Path.GetFileNameWithoutExtension(sampleFont)}-{(int)sampleFontSize}.json";

                using (var image = new Bitmap(output.bitmapWidth, output.bitmapHeight))
                {
                    for (int y = 0; y < output.bitmapHeight; y++)
                    {
                        for (int x = 0; x < output.bitmapWidth; x++)
                        {
                            int pixel = output.bitmap[y * output.bitmapWidth + x];

                            image.SetPixel(x, y, Color.FromArgb(255, pixel, pixel, pixel));
                        }
                    }

                    Directory.CreateDirectory(OutputDirectory);

                    image.Save($"OutputBitmapFonts/{Path.GetFileNameWithoutExtension(sampleFont)}-{(int)sampleFontSize}.png", ImageFormat.Png);
                }


                File.WriteAllText(outputJsonFileName, JsonSerializer.Serialize(output.cdata.Select((c, idx) => new BakedCharData
                {
                    bounds = c,
                    codePoint = fontOptions.rangeFrom + idx,
                    character = new string([(char)(fontOptions.rangeFrom + idx)]),
                }), new JsonSerializerOptions
                {
                    IncludeFields = true,
                    WriteIndented = true,
                }));


                Console.WriteLine("Generated font " + sampleFont + " with size " + sampleFontSize);
            }

        }
    }
}