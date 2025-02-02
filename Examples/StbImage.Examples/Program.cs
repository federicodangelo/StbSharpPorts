// See https://aka.ms/new-console-template for more information


using StbSharp;
using StbSharp.StbCommon;

LoadTexture("Images\\ProjectUtumno_full.png");

static void LoadTexture(string filename)
{
    byte[] bytes = File.ReadAllBytes(filename);

    BytePtr ptr = StbImage.stbi_load_from_memory(bytes, bytes.Length, out int width, out int height, out var channels_in_file, StbImage.STBI_CHANNELS._default);

    if (ptr.IsNull)
    {
        throw new Exception($"Failed to load {filename} reason: {StbImage.stbi_failure_reason()}");
    }

    Console.WriteLine($"Loaded texture {filename} width: {width} height: {height} channels: {channels_in_file}");
}

