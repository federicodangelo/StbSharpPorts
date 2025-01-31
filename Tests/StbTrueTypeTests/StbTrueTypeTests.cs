#pragma warning disable CA1416 // Validate platform compatibility

using System.Drawing;
using System.Globalization;

namespace StbSharp.Tests;



public class StbTrueTypeTests
{
    protected const string FontsPath = "Fonts";
    protected const string ExpectedPath = "Expected";
    protected const string GeneratedPath = "Generated";

    static protected byte[] GetFontFile(string fileName)
    {
        var fullPath = Path.Combine(FontsPath, fileName);

        Assert.True(File.Exists(fullPath), "Missing font file: " + fullPath);

        var bytes = File.ReadAllBytes(fullPath);

        return bytes;
    }

    static private Bitmap GetExpectedFontImage(string fileName)
    {
        Assert.True(File.Exists(fileName), "Missing expected test image: " + fileName);

        return new Bitmap(fileName);
    }

    static protected Bitmap GenerateImageFromFontBitmap(Span<byte> bitmap, int width, int height)
    {
        var image = new Bitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixel = bitmap[y * width + x];

                image.SetPixel(x, y, Color.FromArgb(255, pixel, pixel, pixel));
            }
        }

        return image;
    }

    static protected void AssertImagesEqual(string expectedFileName, Bitmap actual, string actualFileName)
    {
        if (!File.Exists(expectedFileName))
        {
            // The expected file doesn't exists, write the actual one to disk so it can be used as a template
            SaveGeneratedImage(actualFileName, actual);
        }

        AssertImagesEqual(GetExpectedFontImage(expectedFileName), actual, actualFileName);
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
