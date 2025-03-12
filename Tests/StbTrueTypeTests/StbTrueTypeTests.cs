using ImageMagick;

namespace StbSharp.Tests;

public class StbTrueTypeTests
{
    protected const string FontsPath = "Fonts";
    protected const string ExpectedPath = "Expected";
    protected const string GeneratedPath = "Generated";

    protected static byte[] GetFontFile(string fileName)
    {
        var fullPath = Path.Combine(FontsPath, fileName);

        Assert.True(File.Exists(fullPath), "Missing font file: " + fullPath);

        var bytes = File.ReadAllBytes(fullPath);

        return bytes;
    }

    private static MagickImage GetExpectedFontImage(string fileName)
    {
        Assert.True(File.Exists(fileName), "Missing expected test image: " + fileName);

        return new MagickImage(fileName);
    }

    protected static MagickImage GenerateImageFromFontBitmap(Span<byte> bitmap, int width, int height)
    {
        var image = new MagickImage(MagickColors.Transparent, (uint)width, (uint)height);

        var pixels = image.GetPixels();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte pixel = bitmap[y * width + x];

                pixels.SetPixel(x, y, MagickColor.FromRgba(pixel, pixel, pixel, 255).ToByteArray());
            }
        }

        return image;
    }

    protected static void AssertImagesEqual(string expectedFileName, MagickImage actual, string actualFileName)
    {
        if (!File.Exists(expectedFileName))
        {
            // The expected file doesn't exists, write the actual one to disk so it can be used as a template
            SaveGeneratedImage(actualFileName, actual);
        }

        AssertImagesEqual(GetExpectedFontImage(expectedFileName), actual, actualFileName);
    }

    protected static void AssertImagesEqual(MagickImage expected, MagickImage actual, string actualFileName)
    {
        //Assert.NotEqual(expected, actual);
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);

        var expectedPixels = expected.GetPixels();
        var actualPixels = actual.GetPixels();

        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                var p1 = expectedPixels.GetPixel(x, y);
                var p2 = actualPixels.GetPixel(x, y);
                if (!p1.Equals(p2))
                {
                    SaveGeneratedImage(actualFileName, actual);

                    string diffFileName = SaveImageDifference(expected, actual, actualFileName);

                    Assert.True(p1.Equals(p2), $"Pixel difference at [{x},{y}], see full diff in file \"{Path.GetFullPath(diffFileName)}\"");
                }
            }
        }
    }

    private static string SaveImageDifference(MagickImage expected, MagickImage actual, string actualFileName)
    {
        MagickImage differenceImage = new MagickImage(MagickColors.Transparent, expected.Width, expected.Height);

        var expectedPixels = expected.GetPixels();
        var actualPixels = actual.GetPixels();
        var differencePixels = differenceImage.GetPixels();

        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                if (!expectedPixels.GetPixel(x, y).Equals(actualPixels.GetPixel(x, y)))
                {
                    differencePixels.SetPixel(x, y, MagickColors.Violet.ToByteArray());
                }
                else
                {
                    differencePixels.SetPixel(x, y, actualPixels.GetPixel(x, y).ToArray());
                }
            }
        }

        string differenceFileName = Path.Combine(Path.GetDirectoryName(actualFileName) ?? "", Path.GetFileNameWithoutExtension(actualFileName) + ".diff" + Path.GetExtension(actualFileName));

        SaveGeneratedImage(differenceFileName, differenceImage);

        differenceImage.Dispose();

        return differenceFileName;
    }

    private static void SaveGeneratedImage(string fileName, MagickImage image)
    {
        if (!Directory.Exists(GeneratedPath))
        {
            Directory.CreateDirectory(GeneratedPath);
        }

        image.Format = MagickFormat.Png;
        image.Write(fileName);
    }
}
