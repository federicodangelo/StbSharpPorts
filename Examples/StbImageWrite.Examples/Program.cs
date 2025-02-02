#pragma warning disable CA1416 // Validate platform compatibility
// See https://aka.ms/new-console-template for more information


using System.Diagnostics;
using System.Drawing;
using StbSharp;
using StbSharp.StbCommon;

namespace StbSharp.Examples;

static public class Program
{
    static public void Main()
    {
        var texture = LoadTexture("Images\\ProjectUtumno_full.png");

        var bytes = SaveStbiImage(texture, StbiFormat.Png, 4);

        if (!Directory.Exists("Generated"))
            Directory.CreateDirectory("Generated");

        File.WriteAllBytes("Generated\\Output.png", bytes.Span);
    }

    static Bitmap LoadTexture(string filename)
    {
        // Works only in windows, can use StbImage in other platforms!
        return new Bitmap(filename);
    }

    enum StbiFormat
    {
        Png,
        Bmp,
        Tga,
        //Hdr, // Not implemented
        Jpeg
    }

    static BytePtr SaveStbiImage(Bitmap image, StbiFormat format, int components)
    {
        byte[] pixels = new byte[image.Width * image.Height * components];

        int idx = 0;

        switch (components)
        {
            case 1: // gray
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = image.GetPixel(x, y);
                        pixels[idx + 0] = (byte)((pixel.R + pixel.G + pixel.B) / 3);
                        idx += 1;
                    }
                }
                break;
            case 2:  // gray - alpha
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = image.GetPixel(x, y);
                        pixels[idx + 0] = (byte)((pixel.R + pixel.G + pixel.B) / 3);
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
                        var pixel = image.GetPixel(x, y);
                        pixels[idx + 0] = (byte)pixel.R;
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
                        var pixel = image.GetPixel(x, y);
                        pixels[idx + 0] = (byte)pixel.R;
                        pixels[idx + 1] = (byte)pixel.G;
                        pixels[idx + 2] = (byte)pixel.B;
                        pixels[idx + 3] = (byte)pixel.A;
                        idx += 4;
                    }
                }
                break;
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

        Stopwatch sw = Stopwatch.StartNew();

        switch (format)
        {
            case StbiFormat.Png:
                // Setting a filter modes beforehand makes the encoding faster
                StbImagWrite.stbi_write_force_png_filter = StbImagWrite.STBIW_PNG_FILTER.NONE;
                StbImagWrite.stbi_write_png_to_func(write_func, image.Width, image.Height, components, pixels, 0);
                break;
            case StbiFormat.Bmp:
                StbImagWrite.stbi_write_bmp_to_func(write_func, image.Width, image.Height, components, pixels);
                break;
            case StbiFormat.Tga:
                StbImagWrite.stbi_write_tga_to_func(write_func, image.Width, image.Height, components, pixels);
                break;
            case StbiFormat.Jpeg:
                StbImagWrite.stbi_write_jpg_to_func(write_func, image.Width, image.Height, components, pixels, 95);
                break;
        }

        sw.Stop();
        Console.WriteLine($"Time to save file: {sw.ElapsedMilliseconds}ms");

        return output.ToArray();
    }

}
