#pragma warning disable CA1416 // Validate platform compatibility

using System.Drawing;
using System.Globalization;
using StbSharp.StbCommon;

namespace StbSharp.Tests;

public class StbImageTests
{
    protected const string ExpectedPath = "Expected";
    protected const string GeneratedPath = "Generated";

    static protected BytePtr LoadStbiImage(string fileName, out int x, out int y, out StbImage.STBI_CHANNELS channels, StbImage.STBI_CHANNELS desired_channels = 0)
    {
        Assert.True(File.Exists(fileName), $"Missing expected image: {fileName}");

        var bytes = File.ReadAllBytes(fileName);

        return StbImage.stbi_load_from_memory(bytes, bytes.Length, out x, out y, out channels, desired_channels);
    }

    static protected Bitmap LoadStbiImage(string fileName)
    {
        var pixelsPtr = LoadStbiImage(fileName, out int width, out int height, out StbImage.STBI_CHANNELS channels, 0);

        Assert.False(pixelsPtr.IsNull, $"StbImage failed to load the image: {fileName} Reason: {StbImage.stbi_failure_reason()}");

        var pixels = pixelsPtr.Span;

        var image = new Bitmap(width, height);

        int idx = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = 
                  channels == StbImage.STBI_CHANNELS.grey ?       Color.FromArgb(255, pixels[idx], pixels[idx], pixels[idx]) :
                  channels == StbImage.STBI_CHANNELS.grey_alpha ? Color.FromArgb(pixels[idx + 1], pixels[idx], pixels[idx], pixels[idx]) :
                  channels == StbImage.STBI_CHANNELS.rgb ?        Color.FromArgb(255, pixels[idx], pixels[idx + 1], pixels[idx + 2]) :
                  channels == StbImage.STBI_CHANNELS.rgb_alpha ?  Color.FromArgb(pixels[idx + 3], pixels[idx], pixels[idx + 1], pixels[idx + 2]) :
                  Color.Violet;

                image.SetPixel(x, y, color);

                idx += (int) channels;
            }
        }

        return image;
    }

    static private Bitmap GetExpectedImage(string fileName)
    {
        Assert.True(File.Exists(fileName), "Missing expected test image: " + fileName);

        return new Bitmap(fileName);
    }
    
    static protected void AssertImagesEqual(string expectedFileName, Bitmap actual, string actualFileName)
    {
        if (!File.Exists(expectedFileName))
        {
            // The expected file doesn't exists, write the actual one to disk so it can be used as a template
            SaveGeneratedImage(actualFileName, actual);
        }

        AssertImagesEqual(GetExpectedImage(expectedFileName), actual, actualFileName);
    }

    static protected void AssertImagesEqual(Bitmap expected, Bitmap actual, string actualFileName)
    {
        Assert.NotEqual(expected, actual);
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);

        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                if (expected.GetPixel(x, y) != actual.GetPixel(x, y))
                {
                    SaveGeneratedImage(actualFileName, actual);

                    string diffFileName = SaveImageDifference(expected, actual, actualFileName);

                    Assert.True(expected.GetPixel(x,y) == actual.GetPixel(x, y), $"Pixel difference at [{x},{y}], see full diff in file \"{Path.GetFullPath(diffFileName)}\"");
                }
            }
        }
    }

    static private string SaveImageDifference(Bitmap expected, Bitmap actual, string actualFileName)
    {
        Bitmap differenceImage = new Bitmap(expected.Width, expected.Height);

        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                if (expected.GetPixel(x, y) != actual.GetPixel(x, y))
                {
                    differenceImage.SetPixel(x, y, Color.Violet);
                }
                else
                {
                    differenceImage.SetPixel(x, y, actual.GetPixel(x, y));
                }
            }
        }

        string differenceFileName = Path.Combine(Path.GetDirectoryName(actualFileName) ?? "", Path.GetFileNameWithoutExtension(actualFileName) + ".diff" + Path.GetExtension(actualFileName));

        SaveGeneratedImage(differenceFileName, differenceImage);

        differenceImage.Dispose();

        return differenceFileName;
    }

    static private void SaveGeneratedImage(string fileName, Image image)
    {
        if (!Directory.Exists(GeneratedPath))
        {
            Directory.CreateDirectory(GeneratedPath);
        }

        image.Save(fileName);
    }
}
