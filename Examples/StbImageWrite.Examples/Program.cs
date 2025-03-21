﻿#pragma warning disable CA1416 // Validate platform compatibility
// See https://aka.ms/new-console-template for more information


using System.Diagnostics;

using ImageMagick;

using StbSharp;
using StbSharp.StbCommon;

namespace StbSharp.Examples;

public static class Program
{
    public static void Main()
    {
        var texture = LoadTexture("Images\\ProjectUtumno_full.png");

        var bytes = SaveStbiImage(texture, StbiFormat.Png, 4);

        if (!Directory.Exists("Generated"))
            Directory.CreateDirectory("Generated");

        File.WriteAllBytes("Generated\\Output.png", bytes.Span);
    }

    static MagickImage LoadTexture(string filename)
    {
        // Works only in windows, can use StbImage in other platforms!
        return new MagickImage(filename);
    }

    enum StbiFormat
    {
        Png,
        Bmp,
        Tga,
        //Hdr, // Not implemented
        Jpeg
    }

    static BytePtr SaveStbiImage(MagickImage image, StbiFormat format, int components)
    {
        byte[] pixels = new byte[image.Width * image.Height * components];

        var imagePixels = image.GetPixels();

        int idx = 0;

        switch (components)
        {
            case 1: // gray
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = imagePixels.GetPixel(x, y).ToColor();
                        pixels[idx + 0] = (byte)((pixel!.R + pixel.G + pixel.B) / 3);
                        idx += 1;
                    }
                }
                break;
            case 2:  // gray - alpha
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = imagePixels.GetPixel(x, y).ToColor();
                        pixels[idx + 0] = (byte)((pixel!.R + pixel.G + pixel.B) / 3);
                        pixels[idx + 1] = (byte)pixel.A;
                        idx += 2;
                    }
                }
                break;
            case 3:  // rgb
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = imagePixels.GetPixel(x, y).ToColor();
                        pixels[idx + 0] = (byte)pixel!.R;
                        pixels[idx + 1] = (byte)pixel.G;
                        pixels[idx + 2] = (byte)pixel.B;
                        idx += 3;
                    }
                }
                break;
            case 4:  // rgb - alpha
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = imagePixels.GetPixel(x, y).ToColor();
                        pixels[idx + 0] = (byte)pixel!.R;
                        pixels[idx + 1] = (byte)pixel.G;
                        pixels[idx + 2] = (byte)pixel.B;
                        pixels[idx + 3] = (byte)pixel.A;
                        idx += 4;
                    }
                }
                break;
        }

        var output = new MemoryStream();
        StbImageWrite.stbi_write_func write_func = (data, size) =>
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

        Stopwatch sw = Stopwatch.StartNew();

        switch (format)
        {
            case StbiFormat.Png:
                // Setting a filter modes beforehand makes the encoding faster
                StbImageWrite.stbi_write_force_png_filter = StbImageWrite.STBIW_PNG_FILTER.NONE;
                StbImageWrite.stbi_write_png_to_func(write_func, (int)image.Width, (int)image.Height, components, pixels, 0);
                break;
            case StbiFormat.Bmp:
                StbImageWrite.stbi_write_bmp_to_func(write_func, (int)image.Width, (int)image.Height, components, pixels);
                break;
            case StbiFormat.Tga:
                StbImageWrite.stbi_write_tga_to_func(write_func, (int)image.Width, (int)image.Height, components, pixels);
                break;
            case StbiFormat.Jpeg:
                StbImageWrite.stbi_write_jpg_to_func(write_func, (int)image.Width, (int)image.Height, components, pixels, 95);
                break;
        }

        sw.Stop();
        Console.WriteLine($"Time to save file: {sw.ElapsedMilliseconds}ms");

        return output.ToArray();
    }

}
