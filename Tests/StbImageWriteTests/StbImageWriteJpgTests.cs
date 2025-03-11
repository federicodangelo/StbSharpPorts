#pragma warning disable CA1416 // Validate platform compatibility

using System.Drawing;

using StbSharp.StbCommon;

namespace StbSharp.Tests;

public class StbImageWriteJpgTests : StbImageWriteTests
{
    [Theory, CombinatorialData]
    public void Test(
        [CombinatorialValues(
            "basi0g01.png",
            "basi0g02.png",
            "basi0g04.png",
            "basi0g08.png",
            "basi2c08.png",
            "basi3p01.png",
            "basi3p02.png",
            "basi3p04.png",
            "basi3p08.png",
            "basi4a08.png",
            "basi6a08.png",
            "basn0g01.png",
            "basn0g02.png",
            "basn0g04.png",
            "basn0g08.png",
            "basn2c08.png",
            "basn3p01.png",
            "basn3p02.png",
            "basn3p04.png",
            "basn3p08.png",
            "basn4a08.png",
            "basn6a08.png",
            "bgai4a08.png",
            "bgan6a08.png",
            "bgbn4a08.png",
            "bgwn6a08.png",
            "s01i3p01.png",
            "s01n3p01.png",
            "s02i3p01.png",
            "s02n3p01.png",
            "s03i3p01.png",
            "s03n3p01.png",
            "s04i3p01.png",
            "s04n3p01.png",
            "s05i3p02.png",
            "s05n3p02.png",
            "s06i3p02.png",
            "s06n3p02.png",
            "s07i3p02.png",
            "s07n3p02.png",
            "s08i3p02.png",
            "s08n3p02.png",
            "s09i3p02.png",
            "s09n3p02.png",
            "s32i3p04.png",
            "s32n3p04.png",
            "s33i3p04.png",
            "s33n3p04.png",
            "s34i3p04.png",
            "s34n3p04.png",
            "s35i3p04.png",
            "s35n3p04.png",
            "s36i3p04.png",
            "s36n3p04.png",
            "s37i3p04.png",
            "s37n3p04.png",
            "s38i3p04.png",
            "s38n3p04.png",
            "s39i3p04.png",
            "s39n3p04.png",
            "s40i3p04.png",
            "s40n3p04.png",
            "tbbn0g04.png",
            "tbbn3p08.png",
            "tbgn3p08.png",
            "tbrn2c08.png",
            "tbwn3p08.png",
            "tbyn3p08.png",
            "tm3n3p02.png",
            "tp0n0g08.png",
            "tp0n2c08.png",
            "tp0n3p08.png",
            "tp1n3p08.png",
            "z00n2c08.png",
            "z03n2c08.png",
            "z06n2c08.png",
            "z09n2c08.png",
            "HD1.png"
            )
        ] string imageFileName)
    {
        TestImage(imageFileName, StbiFormat.Jpeg, 4, 0.1f);
    }

}
