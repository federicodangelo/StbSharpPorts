Fonts taken from https://github.com/ocornut/imgui/tree/master/misc/fonts

_(You may browse this at https://github.com/ocornut/imgui/blob/master/docs/FONTS.md or view this file with any Markdown viewer)_

## Dear ImGui: Using Fonts

The code in imgui.cpp embeds a copy of 'ProggyClean.ttf' (by Tristan Grimmer),
a 13 pixels high, pixel-perfect font used by default. We embed it in the source code so you can use Dear ImGui without any file system access. ProggyClean does not scale smoothly, therefore it is recommended that you load your own file when using Dear ImGui in an application aiming to look nice and wanting to support multiple resolutions.

You may also load external .TTF/.OTF files.
In the [misc/fonts/](https://github.com/ocornut/imgui/tree/master/misc/fonts) folder you can find a few suggested fonts, provided as a convenience.

## Credits/Licenses For Fonts Included In Repository

Some fonts files are available in the `misc/fonts/` folder:

**Roboto-Medium.ttf**, by Christian Robetson
<br>Apache License 2.0
<br>https://fonts.google.com/specimen/Roboto

**Cousine-Regular.ttf**, by Steve Matteson
<br>Digitized data copyright (c) 2010 Google Corporation.
<br>Licensed under the SIL Open Font License, Version 1.1
<br>https://fonts.google.com/specimen/Cousine

**DroidSans.ttf**, by Steve Matteson
<br>Apache License 2.0
<br>https://www.fontsquirrel.com/fonts/droid-sans

**ProggyClean.ttf**, by Tristan Grimmer
<br>MIT License
<br>(recommended loading setting: Size = 13.0, GlyphOffset.y = +1)
<br>http://www.proggyfonts.net/

**ProggyTiny.ttf**, by Tristan Grimmer
<br>MIT License
<br>(recommended loading setting: Size = 10.0, GlyphOffset.y = +1)
<br>http://www.proggyfonts.net/

**Karla-Regular.ttf**, by Jonathan Pinhorn
<br>SIL OPEN FONT LICENSE Version 1.1

##### [Return to Index](#index)

## Font Links

#### ICON FONTS

- C/C++ header for icon fonts (#define with code points to use in source code string literals) https://github.com/juliettef/IconFontCppHeaders
- FontAwesome https://fortawesome.github.io/Font-Awesome
- OpenFontIcons https://github.com/traverseda/OpenFontIcons
- Google Icon Fonts https://design.google.com/icons/
- Kenney Icon Font (Game Controller Icons) https://github.com/nicodinh/kenney-icon-font
- IcoMoon - Custom Icon font builder https://icomoon.io/app

#### REGULAR FONTS

- Google Noto Fonts (worldwide languages) https://www.google.com/get/noto/
- Open Sans Fonts https://fonts.google.com/specimen/Open+Sans
- (Japanese) M+ fonts by Coji Morishita http://mplus-fonts.sourceforge.jp/mplus-outline-fonts/index-en.html

#### MONOSPACE FONTS

Pixel Perfect:
- Proggy Fonts, by Tristan Grimmer http://www.proggyfonts.net or http://upperboundsinteractive.com/fonts.php
- Sweet16, Sweet16 Mono, by Martin Sedlak (Latin + Supplemental + Extended A) https://github.com/kmar/Sweet16Font (also include an .inl file to use directly in dear imgui.)

Regular:
- Google Noto Mono Fonts https://www.google.com/get/noto/
- Typefaces for source code beautification https://github.com/chrissimpkins/codeface
- Programmation fonts http://s9w.github.io/font_compare/
- Inconsolata http://www.levien.com/type/myfonts/inconsolata.html
- Adobe Source Code Pro: Monospaced font family for ui & coding environments https://github.com/adobe-fonts/source-code-pro
- Monospace/Fixed Width Programmer's Fonts http://www.lowing.org/fonts/

Or use Arial Unicode or other Unicode fonts provided with Windows for full characters coverage (not sure of their licensing).

##### [Return to Index](#index)
