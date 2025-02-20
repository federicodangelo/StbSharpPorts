using ImageMagick;
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

    static protected MagickImage LoadStbiImage(string fileName)
    {
        var pixelsPtr = LoadStbiImage(fileName, out int width, out int height, out StbImage.STBI_CHANNELS channels, 0);

        Assert.False(pixelsPtr.IsNull, $"StbImage failed to load the image: {fileName} Reason: {StbImage.stbi_failure_reason()}");

        var pixels = pixelsPtr.Span;

        var image = new MagickImage(MagickColors.Transparent, (uint) width, (uint) height);
        var imagePixels = image.GetPixels();

        int idx = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                MagickColor color =
                  channels == StbImage.STBI_CHANNELS.grey ? MagickColor.FromRgba(pixels[idx], pixels[idx], pixels[idx], 255) :
                  channels == StbImage.STBI_CHANNELS.grey_alpha ? MagickColor.FromRgba(pixels[idx], pixels[idx], pixels[idx], pixels[idx + 1]) :
                  channels == StbImage.STBI_CHANNELS.rgb ? MagickColor.FromRgba(pixels[idx], pixels[idx + 1], pixels[idx + 2], 255) :
                  channels == StbImage.STBI_CHANNELS.rgb_alpha ? MagickColor.FromRgba(pixels[idx], pixels[idx + 1], pixels[idx + 2], pixels[idx + 3]) :
                  MagickColors.Violet;

                imagePixels.SetPixel(x, y, color.ToByteArray());

                idx += (int)channels;
            }
        }

        return image;
    }

    static protected Ptr<ushort> LoadStbiImage16(string fileName, out int x, out int y, out StbImage.STBI_CHANNELS channels, StbImage.STBI_CHANNELS desired_channels = 0)
    {
        Assert.True(File.Exists(fileName), $"Missing expected image: {fileName}");

        var bytes = File.ReadAllBytes(fileName);

        return StbImage.stbi_load_16_from_memory(bytes, bytes.Length, out x, out y, out channels, desired_channels);
    }

    static protected MagickImage LoadStbiImage16(string fileName)
    {
        var pixelsPtr = LoadStbiImage16(fileName, out int width, out int height, out StbImage.STBI_CHANNELS channels, 0);

        Assert.False(pixelsPtr.IsNull, $"StbImage failed to load the image: {fileName} Reason: {StbImage.stbi_failure_reason()}");

        var pixels = pixelsPtr.Span;

        var image = new MagickImage(MagickColors.Transparent, (uint) width, (uint) height);
        var imagePixels = image.GetPixels();

        int idx = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                MagickColor color =
                  channels == StbImage.STBI_CHANNELS.grey ?       MagickColor.FromRgba((byte) (pixels[idx] >> 8), (byte) (pixels[idx] >> 8),     (byte) (pixels[idx] >> 8),        255) :
                  channels == StbImage.STBI_CHANNELS.grey_alpha ? MagickColor.FromRgba((byte) (pixels[idx] >> 8), (byte) (pixels[idx] >> 8),     (byte) (pixels[idx] >> 8),        (byte) (pixels[idx + 1] >> 8)) :
                  channels == StbImage.STBI_CHANNELS.rgb ?        MagickColor.FromRgba((byte) (pixels[idx] >> 8), (byte) (pixels[idx + 1] >> 8), (byte) (pixels[idx + 2] >> 8),    255) :
                  channels == StbImage.STBI_CHANNELS.rgb_alpha ?  MagickColor.FromRgba((byte) (pixels[idx] >> 8), (byte) (pixels[idx + 1] >> 8), (byte) (pixels[idx + 2] >> 8),    (byte) (pixels[idx + 3] >> 8)) :
                  MagickColors.Violet;

                imagePixels.SetPixel(x, y, color.ToByteArray());

                idx += (int)channels;
            }
        }

        return image;
    }

    static private MagickImage GetExpectedImage(string fileName)
    {
        Assert.True(File.Exists(fileName), "Missing expected test image: " + fileName);

        return new MagickImage(fileName);
    }

    static protected void AssertImagesEqual(string expectedFileName, MagickImage actual, string actualFileName, float tolerance = 0)
    {
        if (!File.Exists(expectedFileName))
        {
            // The expected file doesn't exists, write the actual one to disk so it can be used as a template
            SaveGeneratedImage(actualFileName, actual);
        }

        AssertImagesEqual(GetExpectedImage(expectedFileName), actual, actualFileName, tolerance);
    }

    static protected void AssertImagesEqual(MagickImage expected, MagickImage actual, string actualFileName, float tolerance = 0)
    {
        //Assert.NotEqual(expected, actual);
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);

        var expectedPixels = expected.GetPixels();
        var actualPixels = actual.GetPixels();

        int badPixels = 0;

        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                var actualPixel = actualPixels.GetPixel(x, y).ToColor();
                var expectedPixel = expectedPixels.GetPixel(x, y).ToColor();

                if (!actualPixel!.Equals(expectedPixel) &&
                    (actualPixel.A != 0 || expectedPixel!.A != 0) // ignore color difference is the alpha channel of both pixels is 0
                    )
                {
                    if (tolerance != 0)
                    {
                        int diffR = actualPixel.R - expectedPixel!.R;
                        int diffG = actualPixel.G - expectedPixel.G;
                        int diffB = actualPixel.B - expectedPixel.B;

                        if (Math.Abs(diffR) / 255.0f < tolerance &&
                            Math.Abs(diffG) / 255.0f < tolerance &&
                            Math.Abs(diffB) / 255.0f < tolerance)
                        {
                            continue;
                        }

                        badPixels++;
                        if ((float)badPixels < (expected.Width * expected.Height) * tolerance)
                            continue;
                    }

                    SaveGeneratedImage(actualFileName, actual);

                    string diffFileName = SaveImageDifference(expected, actual, actualFileName);

                    Assert.True(expectedPixel!.Equals(actualPixel), $"Pixel difference at [{x},{y}], see full diff in file \"{Path.GetFullPath(diffFileName)}\"");
                }
            }
        }
    }

    static private string SaveImageDifference(MagickImage expected, MagickImage actual, string actualFileName)
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

    static private void SaveGeneratedImage(string fileName, MagickImage image)
    {
        if (!Directory.Exists(GeneratedPath))
        {
            Directory.CreateDirectory(GeneratedPath);
        }

        image.Write(fileName);
    }
}
