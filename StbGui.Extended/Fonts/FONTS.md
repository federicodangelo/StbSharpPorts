Fonts taken from https://github.com/ocornut/imgui/tree/master/misc/fonts

_(You may browse this at https://github.com/ocornut/imgui/blob/master/docs/FONTS.md or view this file with any Markdown viewer)_

## Dear ImGui: Using Fonts

The code in imgui.cpp embeds a copy of 'ProggyClean.ttf' (by Tristan Grimmer),
a 13 pixels high, pixel-perfect font used by default. We embed it in the source code so you can use Dear ImGui without any file system access. ProggyClean does not scale smoothly, therefore it is recommended that you load your own file when using Dear ImGui in an application aiming to look nice and wanting to support multiple resolutions.

You may also load external .TTF/.OTF files.
In the [misc/fonts/](https://github.com/ocornut/imgui/tree/master/misc/fonts) folder you can find a few suggested fonts, provided as a convenience.

## Credits/Licenses For Fonts Included In Repository

Some fonts files are available in the `misc/fonts/` folder:

**ProggyClean.ttf**, by Tristan Grimmer
<br>MIT License
<br>(recommended loading setting: Size = 13.0, GlyphOffset.y = +1)
<br>http://www.proggyfonts.net/

**ProggyTiny.ttf**, by Tristan Grimmer
<br>MIT License
<br>(recommended loading setting: Size = 10.0, GlyphOffset.y = +1)
<br>http://www.proggyfonts.net/

