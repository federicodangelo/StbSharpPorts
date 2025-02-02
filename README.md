C# Port of some STB header files
https://github.com/nothings/stb/

[![build](https://github.com/federicodangelo/StbSharpPorts/actions/workflows/build.yml/badge.svg)](https://github.com/federicodangelo/StbSharpPorts/actions/workflows/build.yml)

Not everything is ported, I'm slowly making my way through all the headers :-)

I did this as a personal project, there is no real benefit of using this over the original files or some of the existing C# wrappers, except that I got everything to work WITHOUT using ANY unsafe code.. so if for some reason you don't want to use wrappers that rely on unsafe code, feel free to use this!

Did some basic benchmarks, and loading / saving big PNG files is roughly ~7x SLOWER than the C implementation (when compiling C# using AOT), and near ~10x SLOWER without AOT.. 

Ported:
- StbImage: PNG / TGA / BMP / JPEG support
- StbImageWrite: Fully ported
- StbRectPack: Fully ported
- StbTrueType: Fully ported

I tried to add some tests for everything, but there is a VERY HIGH chance that I introduced some really creative bugs in the porting process, so use evertything at your own risk.. :-)

I'm using the same LINCESE file that is being used by https://github.com/nothings/stb/ , all the amazing people in there did the original work, I just ported it!!
