#pragma warning disable CA1416 // Validate platform compatibility

using System.Drawing;
using System.Globalization;
using StbSharp.StbCommon;

namespace StbSharp.Tests;

public class StbImageWriteTests
{
    protected const string ExpectedPath = "Expected";
    protected const string GeneratedPath = "Generated";

    public enum StbiFormat
    {
        Png,
        Bmp,
        Tga,
        //Hdr, // Not implemented
        Jpeg
    }

    static protected BytePtr SaveStbiImage(Bitmap image, StbiFormat format, int components)
    {
        byte[] pixels = new byte[image.Width * image.Height * components];

        int idx = 0;
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);

                switch (components)
                {
                    case 1: // gray
                        pixels[idx + 0] = (byte)((pixel.R + pixel.G + pixel.B) / 3);
                        break;
                    case 2:  // gray - alpha
                        pixels[idx + 0] = (byte)((pixel.R + pixel.G + pixel.B) / 3);
                        pixels[idx + 1] = (byte)pixel.A;
                        break;
                    case 3:  // rgb
                        pixels[idx + 0] = (byte)pixel.R;
                        pixels[idx + 1] = (byte)pixel.G;
                        pixels[idx + 2] = (byte)pixel.B;
                        break;
                    case 4:  // rgb - alpha
                        pixels[idx + 0] = (byte)pixel.R;
                        pixels[idx + 1] = (byte)pixel.G;
                        pixels[idx + 2] = (byte)pixel.B;
                        pixels[idx + 3] = (byte)pixel.A;
                        break;
                }

                idx += components;
            }
        }

        var output = new MemoryStream();
        StbImagWrite.stbi_write_func write_func = (data, size) =>
        {
            if (size < 16)
            {
                for (int i = 0; i < size; i++)
                    output.WriteByte(data[i]);
            }
            else
            {
                output.Write(data.ToArray(), 0, size);
            }
        };

        switch (format)
        {
            case StbiFormat.Png:
                Assert.True(StbImagWrite.stbi_write_png_to_func(write_func, image.Width, image.Height, components, pixels, 0));
                break;
            case StbiFormat.Bmp:
                Assert.True(StbImagWrite.stbi_write_bmp_to_func(write_func, image.Width, image.Height, components, pixels));
                break;
            case StbiFormat.Tga:
                Assert.True(StbImagWrite.stbi_write_tga_to_func(write_func, image.Width, image.Height, components, pixels));
                break;
            case StbiFormat.Jpeg:
                Assert.True(StbImagWrite.stbi_write_jpg_to_func(write_func, image.Width, image.Height, components, pixels, 95));
                break;
        }

        return output.ToArray();
    }

    static protected Bitmap GetExpectedImage(string fileName)
    {
        Assert.True(File.Exists(fileName), "Missing expected test image: " + fileName);

        return new Bitmap(fileName);
    }

    static protected void AssertImagesEqual(Bitmap expected, Bitmap actual, string actualFileName, float tolerance)
    {
        Assert.NotEqual(expected, actual);
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);

        int badPixels = 0;

        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                Color actualPixel = actual.GetPixel(x, y);
                Color expectedPixel = expected.GetPixel(x, y);

                if (actualPixel != expectedPixel &&
                    (actualPixel.A != 0 || expectedPixel.A != 0) // ignore color difference is the alpha channel of both pixels is 0
                    )
                {
                    if (tolerance != 0)
                    {
                        int diffR = actualPixel.R - expectedPixel.R;
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

                    Assert.True(expected.GetPixel(x, y) == actual.GetPixel(x, y), $"Pixel difference at [{x},{y}], see full diff in file \"{Path.GetFullPath(diffFileName)}\"");
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

        string differenceFileName = Path.Combine(Path.GetDirectoryName(actualFileName) ?? "", Path.GetFileNameWithoutExtension(actualFileName) + ".diff" + ".png");

        SaveGeneratedImage(differenceFileName, differenceImage);

        differenceImage.Dispose();

        return differenceFileName;
    }

    static protected void SaveGeneratedImage(string fileName, Image image)
    {
        if (!Directory.Exists(GeneratedPath))
        {
            Directory.CreateDirectory(GeneratedPath);
        }

        image.Save(fileName);
    }


    static protected void SaveGeneratedImage(string fileName, BytePtr bytes)
    {
        if (!Directory.Exists(GeneratedPath))
        {
            Directory.CreateDirectory(GeneratedPath);
        }

        File.WriteAllBytes(fileName, bytes.Span);
    }


    static protected void TestImage(string imageFileName, StbiFormat format, int channels, float tolerance)
    {
        string expectedFileName = BuildExpectedFileName(imageFileName);
        string generatedFileName = BuildGeneratedFileName(imageFileName, format);

        var expectedImage = GetExpectedImage(expectedFileName);

        var savedImage = SaveStbiImage(expectedImage, format, channels);

        Bitmap generatedImage;

        try
        {
            generatedImage = new Bitmap(new MemoryStream(savedImage.Span.ToArray()));
        }
        catch (Exception)
        {
            SaveGeneratedImage(generatedFileName, savedImage);
            Assert.Fail("Failed to re-load generated image: " + expectedFileName);
            throw;
        }

        AssertImagesEqual(expectedImage, generatedImage, generatedFileName, tolerance);
    }

    static private string GetExtension(StbiFormat format)
    {
        switch (format)
        {
            case StbiFormat.Png:
                return ".png";
            case StbiFormat.Bmp:
                return ".bmp";
            case StbiFormat.Tga:
                return ".tga";
            case StbiFormat.Jpeg:
                return ".jpg";
        }
        Assert.Fail();
        return "";
    }

    static private string BuildExpectedFileName(string fontFileName)
    {
        return Path.Combine(ExpectedPath, fontFileName);
    }

    static private string BuildGeneratedFileName(string fontFileName, StbiFormat format)
    {
        string expected = BuildExpectedFileName(fontFileName);


        return Path.Combine(GeneratedPath, $"{Path.GetFileNameWithoutExtension(expected)}{GetExtension(format)}");
    }
}
