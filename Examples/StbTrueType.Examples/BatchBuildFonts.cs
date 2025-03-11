#pragma warning disable CA1416 // Validate platform compatibility

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using ImageMagick;

using StbSharp;
using StbSharp.StbCommon;

namespace StbSharp.Examples;

struct BakedCharData
{
    public int codePoint;
    public string character;
    public StbTrueType.stbtt_bakedchar bounds;
}

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
[JsonSerializable(typeof(BakedCharData[]))] // Add all types you intend to serialize
internal partial class MyJsonContext : JsonSerializerContext
{

}

public class BatchBuildFontsExample
{
    static public void BuildBitmapFonts()
    {
        string[] SampleFonts = [
            "Fonts/Cousine-Regular.ttf",
            "Fonts/DroidSans.ttf",
            "Fonts/Karla-Regular.ttf",
            "Fonts/ProggyClean.ttf",
            "Fonts/ProggyTiny.ttf",
            "Fonts/Roboto-Medium.ttf",
        ];

        float[] SampleFontSizes = [13.0f, 16.0f, 24.0f, 32.0f];

        const string OutputDirectory = "OutputBitmapFonts";

        if (Directory.Exists(OutputDirectory))
            Directory.Delete(OutputDirectory, true);

        var sw = new Stopwatch();

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

                sw.Start();
                var output = BuildFontBitmapExample.BuildFontBitmap(fontOptions, bitmapOptions, BuildFontBitmapExample.PackFormat.Better);
                sw.Stop();

                if (output == null)
                {
                    Console.Error.WriteLine("Failed to generate font " + sampleFont + " with size " + sampleFontSize);
                    continue;
                }

                var outputImageFileName = $"{OutputDirectory}/{Path.GetFileNameWithoutExtension(sampleFont)}-{(int)sampleFontSize}.png";
                var outputJsonFileName = $"{OutputDirectory}/{Path.GetFileNameWithoutExtension(sampleFont)}-{(int)sampleFontSize}.json";

                using (var image = new MagickImage(MagickColors.Transparent, (uint)output.bitmapWidth, (uint)output.bitmapHeight))
                {
                    var imagePixels = image.GetPixels();

                    for (int y = 0; y < output.bitmapHeight; y++)
                    {
                        for (int x = 0; x < output.bitmapWidth; x++)
                        {
                            byte pixel = output.bitmap[y * output.bitmapWidth + x];

                            imagePixels.SetPixel(x, y, MagickColor.FromRgba(pixel, pixel, pixel, 255).ToByteArray());
                        }
                    }

                    Directory.CreateDirectory(OutputDirectory);

                    image.Format = MagickFormat.Png;
                    image.Write($"OutputBitmapFonts/{Path.GetFileNameWithoutExtension(sampleFont)}-{(int)sampleFontSize}.png");
                }


                File.WriteAllText(outputJsonFileName, JsonSerializer.Serialize(
                    output.cdata.Select((c, idx) => new BakedCharData
                    {
                        bounds = c,
                        codePoint = fontOptions.rangeFrom + idx,
                        character = new string([(char)(fontOptions.rangeFrom + idx)]),
                    }).ToArray(),
                    MyJsonContext.Default.BakedCharDataArray
                ));


                Console.WriteLine("Generated font " + sampleFont + " with size " + sampleFontSize);
            }

        }

        sw.Stop();

        Console.WriteLine($"Time to generate all fonts: {sw.ElapsedMilliseconds}ms");
    }
}
