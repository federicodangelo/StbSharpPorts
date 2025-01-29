//#define DEBUG_USING_SVG
#define STB_RECT_PACK_VERSION

//   Starting with version 1.06, the rasterizer was replaced with a new,
//   faster and generally-more-precise rasterizer. The new rasterizer more
//   accurately measures pixel coverage for anti-aliasing, except in the case
//   where multiple shapes overlap, in which case it overestimates the AA pixel
//   coverage. Thus, anti-aliasing of intersecting shapes may look wrong. If
//   this turns out to be a problem, you can re-enable the old rasterizer with
//        #define STBTT_RASTERIZER_VERSION 1
//   which will incur about a 15% speed hit.
//
#define STBTT_RASTERIZER_VERSION_2
//#define STBTT_RASTERIZER_VERSION_1

#if !STB_RECT_PACK_VERSION
using stbrp_coord = int;
#else 
using stbrp_coord = int;
using stbrp_node = StbSharp.StbRectPack.stbrp_node;
using stbrp_context = StbSharp.StbRectPack.stbrp_context;
using stbrp_rect = StbSharp.StbRectPack.stbrp_rect;
#endif

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using StbSharp.StbCommon;

using stbtt_uint8 = byte;
using stbtt_int8 = sbyte;
using stbtt_uint16 = ushort;
using stbtt_int16 = short;
using stbtt_uint32 = uint;
using stbtt_int32 = int;
using stbtt_vertex_type = short;
using size_t = int;

namespace StbSharp;

public class StbTrueType
{

   // stb_truetype.h - v1.26 - public domain
   // authored from 2009-2021 by Sean Barrett / RAD Game Tools
   //
   // =======================================================================
   //
   //    NO SECURITY GUARANTEE -- DO NOT USE THIS ON UNTRUSTED FONT FILES
   //
   // This library does no range checking of the offsets found in the file,
   // meaning an attacker can use it to read arbitrary memory.
   //
   // =======================================================================
   //
   //   This library processes TrueType files:
   //        parse files
   //        extract glyph metrics
   //        extract glyph shapes
   //        render glyphs to one-channel bitmaps with antialiasing (box filter)
   //        render glyphs to one-channel SDF bitmaps (signed-distance field/function)
   //
   //   Todo:
   //        non-MS cmaps
   //        crashproof on bad data
   //        hinting? (no longer patented)
   //        cleartype-style AA?
   //        optimize: use simple memory allocator for intermediates
   //        optimize: build edge-list directly from curves
   //        optimize: rasterize directly from curves?
   //
   // ADDITIONAL CONTRIBUTORS
   //
   //   Mikko Mononen: compound shape support, more cmap formats
   //   Tor Andersson: kerning, subpixel rendering
   //   Dougall Johnson: OpenType / Type 2 font handling
   //   Daniel Ribeiro Maciel: basic GPOS-based kerning
   //
   //   Misc other:
   //       Ryan Gordon
   //       Simon Glass
   //       github:IntellectualKitty
   //       Imanol Celaya
   //       Daniel Ribeiro Maciel
   //
   //   Bug/warning reports/fixes:
   //       "Zer" on mollyrocket       Fabian "ryg" Giesen   github:NiLuJe
   //       Cass Everitt               Martins Mozeiko       github:aloucks
   //       stoiko (Haemimont Games)   Cap Petschulat        github:oyvindjam
   //       Brian Hook                 Omar Cornut           github:vassvik
   //       Walter van Niftrik         Ryan Griege
   //       David Gow                  Peter LaValle
   //       David Given                Sergey Popov
   //       Ivan-Assen Ivanov          Giumo X. Clanjor
   //       Anthony Pesch              Higor Euripedes
   //       Johan Duparc               Thomas Fields
   //       Hou Qiming                 Derek Vinyard
   //       Rob Loach                  Cort Stratton
   //       Kenney Phillis Jr.         Brian Costabile
   //       Ken Voskuil (kaesve)       Yakov Galka
   //
   // VERSION HISTORY
   //
   //   1.26 (2021-08-28) fix broken rasterizer
   //   1.25 (2021-07-11) many fixes
   //   1.24 (2020-02-05) fix warning
   //   1.23 (2020-02-02) query SVG data for glyphs; query whole kerning table (but only kern not GPOS)
   //   1.22 (2019-08-11) minimize missing-glyph duplication; fix kerning if both 'GPOS' and 'kern' are defined
   //   1.21 (2019-02-25) fix warning
   //   1.20 (2019-02-07) PackFontRange skips missing codepoints; GetScaleFontVMetrics()
   //   1.19 (2018-02-11) GPOS kerning, STBTT_fmod
   //   1.18 (2018-01-29) add missing function
   //   1.17 (2017-07-23) make more arguments const; doc fix
   //   1.16 (2017-07-12) SDF support
   //   1.15 (2017-03-03) make more arguments const
   //   1.14 (2017-01-16) num-fonts-in-TTC function
   //   1.13 (2017-01-02) support OpenType fonts, certain Apple fonts
   //   1.12 (2016-10-25) suppress warnings about casting away const with -Wcast-qual
   //   1.11 (2016-04-02) fix unused-variable warning
   //   1.10 (2016-04-02) user-defined fabs(); rare memory leak; remove duplicate typedef
   //   1.09 (2016-01-16) warning fix; avoid crash on outofmem; use allocation userdata properly
   //   1.08 (2015-09-13) document stbtt_Rasterize(); fixes for vertical & horizontal edges
   //   1.07 (2015-08-01) allow PackFontRanges to accept arrays of sparse codepoints;
   //                     variant PackFontRanges to pack and render in separate phases;
   //                     fix stbtt_GetFontOFfsetForIndex (never worked for non-0 input?);
   //                     fixed an assert() bug in the new rasterizer
   //                     replace assert() with STBTT_assert() in new rasterizer
   //
   //   Full history can be found at the end of this file.
   //
   // LICENSE
   //
   //   See end of file for license information.
   //
   // USAGE
   //
   //   Include this file in whatever places need to refer to it. In ONE C/C++
   //   file, write:
   //      #define STB_TRUETYPE_IMPLEMENTATION
   //   before the #include of this file. This expands out the actual
   //   implementation into that C/C++ file.
   //
   //   To make the implementation private to the file that generates the implementation,
   //      #define STBTT_STATIC
   //
   //   Simple 3D API (don't ship this, but it's fine for tools and quick start)
   //           stbtt_BakeFontBitmap()               -- bake a font to a bitmap for use as texture
   //           stbtt_GetBakedQuad()                 -- compute quad to draw for a given char
   //
   //   Improved 3D API (more shippable):
   //           #include "stb_rect_pack.h"           -- optional, but you really want it
   //           stbtt_PackBegin()
   //           stbtt_PackSetOversampling()          -- for improved quality on small fonts
   //           stbtt_PackFontRanges()               -- pack and renders
   //           stbtt_PackEnd()
   //           stbtt_GetPackedQuad()
   //
   //   "Load" a font file from a memory buffer (you have to keep the buffer loaded)
   //           stbtt_InitFont()
   //           stbtt_GetFontOffsetForIndex()        -- indexing for TTC font collections
   //           stbtt_GetNumberOfFonts()             -- number of fonts for TTC font collections
   //
   //   Render a unicode codepoint to a bitmap
   //           stbtt_GetCodepointBitmap()           -- allocates and returns a bitmap
   //           stbtt_MakeCodepointBitmap()          -- renders into bitmap you provide
   //           stbtt_GetCodepointBitmapBox()        -- how big the bitmap must be
   //
   //   Character advance/positioning
   //           stbtt_GetCodepointHMetrics()
   //           stbtt_GetFontVMetrics()
   //           stbtt_GetFontVMetricsOS2()
   //           stbtt_GetCodepointKernAdvance()
   //
   // ADDITIONAL DOCUMENTATION
   //
   //   Immediately after this block comment are a series of sample programs.
   //
   //   After the sample programs is the "header file" section. This section
   //   includes documentation for each API function.
   //
   //   Some important concepts to understand to use this library:
   //
   //      Codepoint
   //         Characters are defined by unicode codepoints, e.g. 65 is
   //         uppercase A, 231 is lowercase c with a cedilla, 0x7e30 is
   //         the hiragana for "ma".
   //
   //      Glyph
   //         A visual character shape (every codepoint is rendered as
   //         some glyph)
   //
   //      Glyph index
   //         A font-specific integer ID representing a glyph
   //
   //      Baseline
   //         Glyph shapes are defined relative to a baseline, which is the
   //         bottom of uppercase characters. Characters extend both above
   //         and below the baseline.
   //
   //      Current Point
   //         As you draw text to the screen, you keep track of a "current point"
   //         which is the origin of each character. The current point's vertical
   //         position is the baseline. Even "baked fonts" use this model.
   //
   //      Vertical Font Metrics
   //         The vertical qualities of the font, used to vertically position
   //         and space the characters. See docs for stbtt_GetFontVMetrics.
   //
   //      Font Size in Pixels or Points
   //         The preferred interface for specifying font sizes in stb_truetype
   //         is to specify how tall the font's vertical extent should be in pixels.
   //         If that sounds good enough, skip the next paragraph.
   //
   //         Most font APIs instead use "points", which are a common typographic
   //         measurement for describing font size, defined as 72 points per inch.
   //         stb_truetype provides a point API for compatibility. However, true
   //         "per inch" conventions don't make much sense on computer displays
   //         since different monitors have different number of pixels per
   //         inch. For example, Windows traditionally uses a convention that
   //         there are 96 pixels per inch, thus making 'inch' measurements have
   //         nothing to do with inches, and thus effectively defining a point to
   //         be 1.333 pixels. Additionally, the TrueType font data provides
   //         an explicit scale factor to scale a given font's glyphs to points,
   //         but the author has observed that this scale factor is often wrong
   //         for non-commercial fonts, thus making fonts scaled in points
   //         according to the TrueType spec incoherently sized in practice.
   //
   // DETAILED USAGE:
   //
   //  Scale:
   //    Select how high you want the font to be, in points or pixels.
   //    Call ScaleForPixelHeight or ScaleForMappingEmToPixels to compute
   //    a scale factor SF that will be used by all other functions.
   //
   //  Baseline:
   //    You need to select a y-coordinate that is the baseline of where
   //    your text will appear. Call GetFontBoundingBox to get the baseline-relative
   //    bounding box for all characters. SF*-y0 will be the distance in pixels
   //    that the worst-case character could extend above the baseline, so if
   //    you want the top edge of characters to appear at the top of the
   //    screen where y=0, then you would set the baseline to SF*-y0.
   //
   //  Current point:
   //    Set the current point where the first character will appear. The
   //    first character could extend left of the current point; this is font
   //    dependent. You can either choose a current point that is the leftmost
   //    point and hope, or add some padding, or check the bounding box or
   //    left-side-bearing of the first character to be displayed and set
   //    the current point based on that.
   //
   //  Displaying a character:
   //    Compute the bounding box of the character. It will contain signed values
   //    relative to <current_point, baseline>. I.e. if it returns x0,y0,x1,y1,
   //    then the character should be displayed in the rectangle from
   //    <current_point+SF*x0, baseline+SF*y0> to <current_point+SF*x1,baseline+SF*y1).
   //
   //  Advancing for the next character:
   //    Call GlyphHMetrics, and compute 'current_point += SF * advance'.
   //
   //
   // ADVANCED USAGE
   //
   //   Quality:
   //
   //    - Use the functions with Subpixel at the end to allow your characters
   //      to have subpixel positioning. Since the font is anti-aliased, not
   //      hinted, this is very import for quality. (This is not possible with
   //      baked fonts.)
   //
   //    - Kerning is now supported, and if you're supporting subpixel rendering
   //      then kerning is worth using to give your text a polished look.
   //
   //   Performance:
   //
   //    - Convert Unicode codepoints to glyph indexes and operate on the glyphs;
   //      if you don't do this, stb_truetype is forced to do the conversion on
   //      every call.
   //
   //    - There are a lot of memory allocations. We should modify it to take
   //      a temp buffer and allocate from the temp buffer (without freeing),
   //      should help performance a lot.
   //
   // NOTES
   //
   //   The system uses the raw data found in the .ttf file without changing it
   //   and without building auxiliary data structures. This is a bit inefficient
   //   on little-endian systems (the data is big-endian), but assuming you're
   //   caching the bitmaps or glyph shapes this shouldn't be a big deal.
   //
   //   It appears to be very hard to programmatically determine what font a
   //   given file is in a general way. I provide an API for this, but I don't
   //   recommend it.
   //
   //
   // PERFORMANCE MEASUREMENTS FOR 1.06:
   //
   //                      32-bit     64-bit
   //   Previous release:  8.83 s     7.68 s
   //   Pool allocations:  7.72 s     6.34 s
   //   Inline sort     :  6.54 s     5.65 s
   //   New rasterizer  :  5.63 s     5.00 s

   //////////////////////////////////////////////////////////////////////////////
   //////////////////////////////////////////////////////////////////////////////
   ////
   ////   INTEGRATION WITH YOUR CODEBASE
   ////
   ////   The following sections allow you to supply alternate definitions
   ////   of C library functions used by stb_truetype, e.g. if you don't
   ////   link with the C runtime library.

   //#ifndef STB_TRUETYPE_IMPLEMENTATION

   static int STBTT_ifloor(float x) => ((int)MathF.Floor(x));
   static int STBTT_iceil(float x) => ((int)MathF.Ceiling(x));

   static float STBTT_sqrt(float x) => MathF.Sqrt(x);
   static float STBTT_pow(float x, float y) => MathF.Pow(x, y);
   static float STBTT_fmod(float x, float y) => x % y;
   static float STBTT_cos(float x) => MathF.Cos(x);
   static float STBTT_acos(float x) => MathF.Acos(x);
   static float STBTT_fabs(float x) => MathF.Abs(x);

   /*
      // #define your own functions "STBTT_malloc" / "STBTT_free" to avoid malloc.h
      #ifndef STBTT_malloc
      #include <stdlib.h>
      #define STBTT_malloc(x,u)  ((void)(u),malloc(x))
      #define STBTT_free(x,u)    ((void)(u),free(x))
      #endif

      #ifndef STBTT_assert
      #include <assert.h>
      #define STBTT_assert(x)    assert(x)
      #endif

      #ifndef STBTT_strlen
      #include <string.h>
      #define STBTT_strlen(x)    strlen(x)
      #endif

      #ifndef STBTT_memcpy
      #include <string.h>
      #define STBTT_memcpy       memcpy
      #define STBTT_memset       memset
      #endif
   #endif
   */

   ///////////////////////////////////////////////////////////////////////////////
   ///////////////////////////////////////////////////////////////////////////////
   ////
   ////   INTERFACE
   ////
   ////

   // private structure
   public struct stbtt__buf
   {
      public BytePtr data;
      public int cursor;
      public int size;
   }

   //////////////////////////////////////////////////////////////////////////////
   //
   // TEXTURE BAKING API
   //
   // If you use this API, you only have to call two functions ever.
   //

   public struct stbtt_bakedchar
   {
      public ushort x0, y0, x1, y1; // coordinates of bbox in bitmap
      public float xoff, yoff, xadvance;
   }

   // if return is positive, the first unused row of the bitmap
   // if return is negative, returns the negative of the number of characters that fit
   // if return is 0, no characters fit and no rows were used
   // This uses a very crappy packing.
   static public int stbtt_BakeFontBitmap(byte[] data, int offset,  // font location (use offset=0 for plain .ttf)
                                   float pixel_height,                     // height of font in pixels
                                   byte[] pixels, int pw, int ph,  // bitmap to be filled in
                                   int first_char, int num_chars,          // characters to bake
                                   stbtt_bakedchar[] chardata)             // you allocate this, it's num_chars long
   {
      return stbtt_BakeFontBitmap_internal(data, offset, pixel_height, pixels, pw, ph, first_char, num_chars, chardata);
   }

   public struct stbtt_aligned_quad
   {
      public float x0, y0, s0, t0; // top-left
      public float x1, y1, s1, t1; // bottom-right
   }

   // Call GetBakedQuad with char_index = 'character - first_char', and it
   // creates the quad you need to draw and advances the current position.
   //
   // The coordinate system used assumes y increases downwards.
   //
   // Characters will extend both above and below the current position;
   // see discussion of "BASELINE" above.
   //
   // It's inefficient; you might want to c&p it and optimize it.
   static public void stbtt_GetBakedQuad(stbtt_bakedchar[] chardata, int pw, int ph,  // same data as above
                                  int char_index,             // character to display
                                  ref float xpos, ref float ypos,   // pointers to current position in screen pixel space
                                  out stbtt_aligned_quad q,      // output: quad to draw
                                  bool opengl_fillrule)       // true if opengl fill rule; false if DX9 or earlier
   {
      float d3d_bias = opengl_fillrule ? 0 : -0.5f;
      float ipw = 1.0f / pw, iph = 1.0f / ph;
      ref var b = ref chardata[char_index];
      int round_x = STBTT_ifloor((xpos + b.xoff) + 0.5f);
      int round_y = STBTT_ifloor((ypos + b.yoff) + 0.5f);

      q.x0 = round_x + d3d_bias;
      q.y0 = round_y + d3d_bias;
      q.x1 = round_x + b.x1 - b.x0 + d3d_bias;
      q.y1 = round_y + b.y1 - b.y0 + d3d_bias;

      q.s0 = b.x0 * ipw;
      q.t0 = b.y0 * iph;
      q.s1 = b.x1 * ipw;
      q.t1 = b.y1 * iph;

      xpos += b.xadvance;

   }

   // Query the font vertical metrics without having to create a font first.
   static public void stbtt_GetScaledFontVMetrics(BytePtr fontdata, int index, float size, out float ascent, out float descent, out float lineGap)
   {
      int i_ascent, i_descent, i_lineGap;
      float scale;
      stbtt_fontinfo info;
      stbtt_InitFont(out info, fontdata, stbtt_GetFontOffsetForIndex(fontdata, index));
      scale = size > 0 ? stbtt_ScaleForPixelHeight(ref info, size) : stbtt_ScaleForMappingEmToPixels(ref info, -size);
      stbtt_GetFontVMetrics(ref info, out i_ascent, out i_descent, out i_lineGap);
      ascent = (float)i_ascent * scale;
      descent = (float)i_descent * scale;
      lineGap = (float)i_lineGap * scale;
   }


   //////////////////////////////////////////////////////////////////////////////
   //
   // NEW TEXTURE BAKING API
   //
   // This provides options for packing multiple fonts into one atlas, not
   // perfectly but better than nothing.

   public struct stbtt_packedchar
   {
      public ushort x0, y0, x1, y1; // coordinates of bbox in bitmap
      public float xoff, yoff, xadvance;
      public float xoff2, yoff2;
   }

   // Initializes a packing context stored in the passed-in stbtt_pack_context.
   // Future calls using this context will pack characters into the bitmap passed
   // in here: a 1-channel bitmap that is width * height. stride_in_bytes is
   // the distance from one row to the next (or 0 to mean they are packed tightly
   // together). "padding" is the amount of padding to leave between each
   // character (normally you want '1' for bitmaps you'll use as textures with
   // bilinear filtering).
   //
   // Returns 0 on failure, 1 on success.
   static public int stbtt_PackBegin(out stbtt_pack_context spc, BytePtr pixels, int pw, int ph, int stride_in_bytes, int padding)
   {
      int num_nodes = pw - padding;
      stbrp_node[] nodes = new stbrp_node[num_nodes];

      //if (context == NULL || nodes == NULL) {
      //   if (context != NULL) STBTT_free(context, alloc_context);
      //   if (nodes   != NULL) STBTT_free(nodes  , alloc_context);
      //   return 0;
      //}

      spc.width = pw;
      spc.height = ph;
      spc.pixels = pixels;
      spc.nodes = nodes;
      spc.padding = padding;
      spc.stride_in_bytes = stride_in_bytes != 0 ? stride_in_bytes : pw;
      spc.h_oversample = 1;
      spc.v_oversample = 1;
      spc.skip_missing = false;

#if STB_RECT_PACK_VERSION
      StbRectPack.stbrp_init_target(out spc.pack_info, pw - padding, ph - padding, nodes, num_nodes);
#else
      stbrp_init_target(out spc.pack_info, pw - padding, ph - padding, nodes, num_nodes);
#endif

      if (!pixels.IsNull)
         pixels.Fill(0, pw * ph); // background of 0 around pixels

      return 1;
   }

   // Cleans up the packing context and frees all memory.
   static public void stbtt_PackEnd(ref stbtt_pack_context spc)
   {
      //STBTT_free(spc.nodes    , spc.user_allocator_context);
      //STBTT_free(spc.pack_info, spc.user_allocator_context);
   }

   static public int STBTT_POINT_SIZE(int x) => (-(x));

   // Creates character bitmaps from the font_index'th font found in fontdata (use
   // font_index=0 if you don't know what that is). It creates num_chars_in_range
   // bitmaps for characters with unicode values starting at first_unicode_char_in_range
   // and increasing. Data for how to render them is stored in chardata_for_range;
   // pass these to stbtt_GetPackedQuad to get back renderable quads.
   //
   // font_size is the full height of the character from ascender to descender,
   // as computed by stbtt_ScaleForPixelHeight. To use a point size as computed
   // by stbtt_ScaleForMappingEmToPixels, wrap the point size in STBTT_POINT_SIZE()
   // and pass that result as 'font_size':
   //       ...,                  20 , ... // font max minus min y is 20 pixels tall
   //       ..., STBTT_POINT_SIZE(20), ... // 'M' is 20 pixels tall
   static public int stbtt_PackFontRange(ref stbtt_pack_context spc, BytePtr fontdata, int font_index, float font_size,
                  int first_unicode_codepoint_in_range, int num_chars_in_range, stbtt_packedchar[] chardata_for_range)
   {
      stbtt_pack_range range = new();
      range.first_unicode_codepoint_in_range = first_unicode_codepoint_in_range;
      range.array_of_unicode_codepoints = null;
      range.num_chars = num_chars_in_range;
      range.chardata_for_range = chardata_for_range;
      range.font_size = font_size;
      return stbtt_PackFontRanges(ref spc, fontdata, font_index, [range], 1);
   }

   public struct stbtt_pack_range
   {
      public float font_size;
      public int first_unicode_codepoint_in_range;  // if non-zero, then the chars are continuous, and this is the first codepoint
      public int[]? array_of_unicode_codepoints;       // if non-zero, then this is an array of unicode codepoints
      public int num_chars;
      public stbtt_packedchar[] chardata_for_range; // output
      public byte h_oversample, v_oversample; // don't set these, they're used internally
   }

   // Creates character bitmaps from multiple ranges of characters stored in
   // ranges. This will usually create a better-packed bitmap than multiple
   // calls to stbtt_PackFontRange. Note that you can call this multiple
   // times within a single PackBegin/PackEnd.
   static public int stbtt_PackFontRanges(ref stbtt_pack_context spc, BytePtr fontdata, int font_index, stbtt_pack_range[] ranges, int num_ranges)
   {
      stbtt_fontinfo info = new();
      int i, j, n, return_value = 1;
      //stbrp_context *context = (stbrp_context *) spc.pack_info;
      stbrp_rect[] rects;

      // flag all characters as NOT packed
      for (i = 0; i < num_ranges; ++i)
         for (j = 0; j < ranges[i].num_chars; ++j)
            ranges[i].chardata_for_range[j].x0 =
            ranges[i].chardata_for_range[j].y0 =
            ranges[i].chardata_for_range[j].x1 =
            ranges[i].chardata_for_range[j].y1 = 0;

      n = 0;
      for (i = 0; i < num_ranges; ++i)
         n += ranges[i].num_chars;

      rects = new stbrp_rect[n];
      //if (rects == NULL)
      //   return 0;

      //info.userdata = spc.user_allocator_context;
      stbtt_InitFont(out info, fontdata, stbtt_GetFontOffsetForIndex(fontdata, font_index));

      n = stbtt_PackFontRangesGatherRects(ref spc, ref info, ranges, num_ranges, rects);

      stbtt_PackFontRangesPackRects(ref spc, rects, n);

      return_value = stbtt_PackFontRangesRenderIntoRects(ref spc, ref info, ranges, num_ranges, rects);

      //STBTT_free(rects, spc.user_allocator_context);
      return return_value;
   }

   // Oversampling a font increases the quality by allowing higher-quality subpixel
   // positioning, and is especially valuable at smaller text sizes.
   //
   // This function sets the amount of oversampling for all following calls to
   // stbtt_PackFontRange(s) or stbtt_PackFontRangesGatherRects for a given
   // pack context. The default (no oversampling) is achieved by h_oversample=1
   // and v_oversample=1. The total number of pixels required is
   // h_oversample*v_oversample larger than the default; for example, 2x2
   // oversampling requires 4x the storage of 1x1. For best results, render
   // oversampled textures with bilinear filtering. Look at the readme in
   // stb/tests/oversample for information about oversampled fonts
   //
   // To use with PackFontRangesGather etc., you must set it before calls
   // call to PackFontRangesGatherRects.
   static public void stbtt_PackSetOversampling(ref stbtt_pack_context spc, uint h_oversample, uint v_oversample)
   {
      STBTT_assert(h_oversample <= STBTT_MAX_OVERSAMPLE);
      STBTT_assert(v_oversample <= STBTT_MAX_OVERSAMPLE);
      if (h_oversample <= STBTT_MAX_OVERSAMPLE)
         spc.h_oversample = h_oversample;
      if (v_oversample <= STBTT_MAX_OVERSAMPLE)
         spc.v_oversample = v_oversample;
   }

   // If skip != 0, this tells stb_truetype to skip any codepoints for which
   // there is no corresponding glyph. If skip=0, which is the default, then
   // codepoints without a glyph recived the font's "missing character" glyph,
   // typically an empty box by convention.
   static public void stbtt_PackSetSkipMissingCodepoints(ref stbtt_pack_context spc, bool skip)
   {
      spc.skip_missing = skip;
   }

   static public void stbtt_GetPackedQuad(stbtt_packedchar[] chardata, int pw, int ph, int char_index, ref float xpos, ref float ypos, out stbtt_aligned_quad q, bool align_to_integer)
   {
      float ipw = 1.0f / pw, iph = 1.0f / ph;
      ref stbtt_packedchar b = ref chardata[char_index];

      if (align_to_integer)
      {
         float x = (float)STBTT_ifloor((xpos + b.xoff) + 0.5f);
         float y = (float)STBTT_ifloor((ypos + b.yoff) + 0.5f);
         q.x0 = x;
         q.y0 = y;
         q.x1 = x + b.xoff2 - b.xoff;
         q.y1 = y + b.yoff2 - b.yoff;
      }
      else
      {
         q.x0 = xpos + b.xoff;
         q.y0 = ypos + b.yoff;
         q.x1 = xpos + b.xoff2;
         q.y1 = ypos + b.yoff2;
      }

      q.s0 = b.x0 * ipw;
      q.t0 = b.y0 * iph;
      q.s1 = b.x1 * ipw;
      q.t1 = b.y1 * iph;

      xpos += b.xadvance;
   }

   // Calling these functions in sequence is roughly equivalent to calling
   // stbtt_PackFontRanges(). If you more control over the packing of multiple
   // fonts, or if you want to pack custom data into a font texture, take a look
   // at the source to of stbtt_PackFontRanges() and create a custom version
   // using these functions, e.g. call GatherRects multiple times,
   // building up a single array of rects, then call PackRects once,
   // then call RenderIntoRects repeatedly. This may result in a
   // better packing than calling PackFontRanges multiple times
   // (or it may not).
   static public int stbtt_PackFontRangesGatherRects(ref stbtt_pack_context spc, ref stbtt_fontinfo info, stbtt_pack_range[] ranges, int num_ranges, stbrp_rect[] rects)
   {
      int i, j, k;
      bool missing_glyph_added = false;

      k = 0;
      for (i = 0; i < num_ranges; ++i)
      {
         ref var range = ref ranges[i];

         float fh = range.font_size;
         float scale = fh > 0 ? stbtt_ScaleForPixelHeight(ref info, fh) : stbtt_ScaleForMappingEmToPixels(ref info, -fh);
         range.h_oversample = (byte)spc.h_oversample;
         range.v_oversample = (byte)spc.v_oversample;
         for (j = 0; j < ranges[i].num_chars; ++j)
         {
            int x0, y0, x1, y1;
            int codepoint = range.array_of_unicode_codepoints == null ? range.first_unicode_codepoint_in_range + j : range.array_of_unicode_codepoints[j];
            int glyph = stbtt_FindGlyphIndex(ref info, codepoint);
            if (glyph == 0 && (spc.skip_missing || missing_glyph_added))
            {
               rects[k].w = rects[k].h = 0;
            }
            else
            {
               stbtt_GetGlyphBitmapBoxSubpixel(ref info, glyph,
                                               scale * spc.h_oversample,
                                               scale * spc.v_oversample,
                                               0, 0,
                                               out x0, out y0, out x1, out y1);
               rects[k].w = (stbrp_coord)(x1 - x0 + spc.padding + spc.h_oversample - 1);
               rects[k].h = (stbrp_coord)(y1 - y0 + spc.padding + spc.v_oversample - 1);
               if (glyph == 0)
                  missing_glyph_added = true;
            }
            ++k;
         }
      }

      return k;
   }
   static public void stbtt_PackFontRangesPackRects(ref stbtt_pack_context spc, stbrp_rect[] rects, int num_rects)
   {
#if STB_RECT_PACK_VERSION
      StbRectPack.stbrp_pack_rects(ref spc.pack_info, rects, num_rects);
#else
      stbrp_pack_rects(ref spc.pack_info, rects, num_rects);
#endif
   }


   // rects array must be big enough to accommodate all characters in the given ranges
   static public int stbtt_PackFontRangesRenderIntoRects(ref stbtt_pack_context spc, ref stbtt_fontinfo info, stbtt_pack_range[] ranges, int num_ranges, stbrp_rect[] rects)
   {
      int i, j, k, missing_glyph = -1, return_value = 1;

      // save current values
      int old_h_over = (int)spc.h_oversample;
      int old_v_over = (int)spc.v_oversample;

      k = 0;
      for (i = 0; i < num_ranges; ++i)
      {
         ref var range = ref ranges[i];
         float fh = range.font_size;
         float scale = fh > 0 ? stbtt_ScaleForPixelHeight(ref info, fh) : stbtt_ScaleForMappingEmToPixels(ref info, -fh);
         float recip_h, recip_v, sub_x, sub_y;
         spc.h_oversample = range.h_oversample;
         spc.v_oversample = range.v_oversample;
         recip_h = 1.0f / spc.h_oversample;
         recip_v = 1.0f / spc.v_oversample;
         sub_x = stbtt__oversample_shift((int)spc.h_oversample);
         sub_y = stbtt__oversample_shift((int)spc.v_oversample);
         for (j = 0; j < range.num_chars; ++j)
         {
            ref stbrp_rect r = ref rects[k];
            if (r.was_packed != 0 && r.w != 0 && r.h != 0)
            {
               ref stbtt_packedchar bc = ref ranges[i].chardata_for_range[j];
               int advance, lsb, x0, y0, x1, y1;
               int codepoint = range.array_of_unicode_codepoints == null ? range.first_unicode_codepoint_in_range + j : range.array_of_unicode_codepoints[j];
               int glyph = stbtt_FindGlyphIndex(ref info, codepoint);
               stbrp_coord pad = (stbrp_coord)spc.padding;

               // pad on left and top
               r.x += pad;
               r.y += pad;
               r.w -= pad;
               r.h -= pad;
               stbtt_GetGlyphHMetrics(ref info, glyph, out advance, out lsb);
               stbtt_GetGlyphBitmapBox(ref info, glyph,
                                       scale * spc.h_oversample,
                                       scale * spc.v_oversample,
                                       out x0, out y0, out x1, out y1);
               stbtt_MakeGlyphBitmapSubpixel(ref info,
                                             spc.pixels + r.x + r.y * spc.stride_in_bytes,
                                             (int)(r.w - spc.h_oversample + 1),
                                             (int)(r.h - spc.v_oversample + 1),
                                             spc.stride_in_bytes,
                                             scale * spc.h_oversample,
                                             scale * spc.v_oversample,
                                             0, 0,
                                             glyph);

               if (spc.h_oversample > 1)
                  stbtt__h_prefilter(((BytePtr)spc.pixels) + r.x + r.y * spc.stride_in_bytes,
                                     r.w, r.h, spc.stride_in_bytes,
                                     (int)spc.h_oversample);

               if (spc.v_oversample > 1)
                  stbtt__v_prefilter(((BytePtr)spc.pixels) + r.x + r.y * spc.stride_in_bytes,
                                     r.w, r.h, spc.stride_in_bytes,
                                     (int)spc.v_oversample);

               bc.x0 = (ushort)(stbtt_int16)r.x;
               bc.y0 = (ushort)(stbtt_int16)r.y;
               bc.x1 = (ushort)(stbtt_int16)(r.x + r.w);
               bc.y1 = (ushort)(stbtt_int16)(r.y + r.h);
               bc.xadvance = scale * advance;
               bc.xoff = (float)x0 * recip_h + sub_x;
               bc.yoff = (float)y0 * recip_v + sub_y;
               bc.xoff2 = (x0 + r.w) * recip_h + sub_x;
               bc.yoff2 = (y0 + r.h) * recip_v + sub_y;

               if (glyph == 0)
                  missing_glyph = j;
            }
            else if (spc.skip_missing)
            {
               return_value = 0;
            }
            else if (r.was_packed != 0 && r.w == 0 && r.h == 0 && missing_glyph >= 0)
            {
               ranges[i].chardata_for_range[j] = ranges[i].chardata_for_range[missing_glyph];
            }
            else
            {
               return_value = 0; // if any fail, report failure
            }

            ++k;
         }
      }

      // restore original values
      spc.h_oversample = (uint)old_h_over;
      spc.v_oversample = (uint)old_v_over;

      return return_value;
   }

   // this is an opaque structure that you shouldn't mess with which holds
   // all the context needed from PackBegin to PackEnd.
   public struct stbtt_pack_context
   {
      public stbrp_context pack_info;
      public int width;
      public int height;
      public int stride_in_bytes;
      public int padding;
      public bool skip_missing;
      public uint h_oversample, v_oversample;
      public BytePtr pixels;
      public stbrp_node[] nodes;
   };

   //////////////////////////////////////////////////////////////////////////////
   //
   // FONT LOADING
   //
   //

   // This function will determine the number of fonts in a font file.  TrueType
   // collection (.ttc) files may contain multiple fonts, while TrueType font
   // (.ttf) files only contain one font. The number of fonts can be used for
   // indexing with the previous function where the index is between zero and one
   // less than the total fonts. If an error occurs, -1 is returned.
   static public int stbtt_GetNumberOfFonts(BytePtr data)
   {
      return stbtt_GetNumberOfFonts_internal(data);
   }

   // Each .ttf/.ttc file may have more than one font. Each font has a sequential
   // index number starting from 0. Call this function to get the font offset for
   // a given index; it returns -1 if the index is out of range. A regular .ttf
   // file will only define one font and it always be at offset 0, so it will
   // return '0' for index 0, and -1 for all other indices.
   static public int stbtt_GetFontOffsetForIndex(BytePtr data, int index)
   {
      return stbtt_GetFontOffsetForIndex_internal(data, index);
   }

   // The following structure is defined publicly so you can declare one on
   // the stack or as a global or etc, but you should treat it as opaque.
   public struct stbtt_fontinfo
   {
      public BytePtr data;              // pointer to .ttf file
      public int fontstart;         // offset of start of font

      public int numGlyphs;                     // number of glyphs, needed for range checking

      public int loca, head, glyf, hhea, hmtx, kern, gpos, svg; // table locations as offset from start of .ttf
      public int index_map;                     // a cmap mapping for our chosen character encoding
      public int indexToLocFormat;              // format needed to map from glyph index to glyph

      public stbtt__buf cff;                    // cff font data
      public stbtt__buf charstrings;            // the charstring index
      public stbtt__buf gsubrs;                 // global charstring subroutines index
      public stbtt__buf subrs;                  // private charstring subroutines index
      public stbtt__buf fontdicts;              // array of font dicts
      public stbtt__buf fdselect;               // map from glyph to fontdict
   };

   // Given an offset into the file that defines a font, this function builds
   // the necessary cached info for the rest of the system. You must allocate
   // the stbtt_fontinfo yourself, and stbtt_InitFont will fill it out. You don't
   // need to do anything special to free it, because the contents are pure
   // value data with no additional data structures. Returns 0 on failure.
   static public int stbtt_InitFont(out stbtt_fontinfo info, BytePtr data, int offset)
   {
      return stbtt_InitFont_internal(out info, data, offset);
   }


   //////////////////////////////////////////////////////////////////////////////
   //
   // CHARACTER TO GLYPH-INDEX CONVERSIOn

   // If you're going to perform multiple operations on the same character
   // and you want a speed-up, call this function with the character you're
   // going to process, then use glyph-based functions instead of the
   // codepoint-based functions.
   // Returns 0 if the character codepoint is not defined in the font.
   static public int stbtt_FindGlyphIndex(ref stbtt_fontinfo info, int unicode_codepoint)
   {
      BytePtr data = info.data;
      stbtt_uint32 index_map = (uint)info.index_map;

      stbtt_uint16 format = ttUSHORT(data + index_map + 0);
      if (format == 0)
      { // apple byte encoding
         stbtt_int32 bytes = ttUSHORT(data + index_map + 2);
         if (unicode_codepoint < bytes - 6)
            return ttBYTE(data + index_map + 6 + unicode_codepoint);
         return 0;
      }
      else if (format == 6)
      {
         stbtt_uint32 first = ttUSHORT(data + index_map + 6);
         stbtt_uint32 count = ttUSHORT(data + index_map + 8);
         if ((stbtt_uint32)unicode_codepoint >= first && (stbtt_uint32)unicode_codepoint < first + count)
            return ttUSHORT(data + (int)(index_map + 10 + (unicode_codepoint - first) * 2));
         return 0;
      }
      else if (format == 2)
      {
         STBTT_assert(false); // @TODO: high-byte mapping for japanese/chinese/korean
         return 0;
      }
      else if (format == 4)
      { // standard mapping for windows fonts: binary search collection of ranges
         stbtt_uint16 segcount = (ushort)(ttUSHORT(data + index_map + 6) >> 1);
         stbtt_uint16 searchRange = (ushort)(ttUSHORT(data + index_map + 8) >> 1);
         stbtt_uint16 entrySelector = ttUSHORT(data + index_map + 10);
         stbtt_uint16 rangeShift = (ushort)(ttUSHORT(data + index_map + 12) >> 1);

         // do a binary search of the segments
         stbtt_uint32 endCount = index_map + 14;
         stbtt_uint32 search = endCount;

         if (unicode_codepoint > 0xffff)
            return 0;

         // they lie from endCount .. endCount + segCount
         // but searchRange is the nearest power of two, so...
         if (unicode_codepoint >= ttUSHORT(data + search + rangeShift * 2))
            search += (uint)(rangeShift * 2);

         // now decrement to bias correctly to find smallest
         search -= 2;
         while (entrySelector != 0)
         {
            stbtt_uint16 end;
            searchRange >>= 1;
            end = ttUSHORT(data + search + searchRange * 2);
            if (unicode_codepoint > end)
               search += (uint)(searchRange * 2);
            --entrySelector;
         }
         search += 2;

         {
            stbtt_uint16 offset, start, last;
            stbtt_uint16 item = (stbtt_uint16)((search - endCount) >> 1);

            start = ttUSHORT(data + index_map + 14 + segcount * 2 + 2 + 2 * item);
            last = ttUSHORT(data + endCount + 2 * item);
            if (unicode_codepoint < start || unicode_codepoint > last)
               return 0;

            offset = ttUSHORT(data + index_map + 14 + segcount * 6 + 2 + 2 * item);
            if (offset == 0)
               return (stbtt_uint16)(unicode_codepoint + ttSHORT(data + index_map + 14 + segcount * 4 + 2 + 2 * item));

            return ttUSHORT(data + offset + (unicode_codepoint - start) * 2 + index_map + 14 + segcount * 6 + 2 + 2 * item);
         }
      }
      else if (format == 12 || format == 13)
      {
         stbtt_uint32 ngroups = ttULONG(data + index_map + 12);
         stbtt_int32 low, high;
         low = 0; high = (stbtt_int32)ngroups;
         // Binary search the right group.
         while (low < high)
         {
            stbtt_int32 mid = low + ((high - low) >> 1); // rounds down, so low <= mid < high
            stbtt_uint32 start_char = ttULONG(data + index_map + 16 + mid * 12);
            stbtt_uint32 end_char = ttULONG(data + index_map + 16 + mid * 12 + 4);
            if ((stbtt_uint32)unicode_codepoint < start_char)
               high = mid;
            else if ((stbtt_uint32)unicode_codepoint > end_char)
               low = mid + 1;
            else
            {
               stbtt_uint32 start_glyph = ttULONG(data + index_map + 16 + mid * 12 + 8);
               if (format == 12)
                  return (int)(start_glyph + unicode_codepoint - start_char);
               else // format == 13
                  return (int)start_glyph;
            }
         }
         return 0; // not found
      }
      // @TODO
      STBTT_assert(false);
      return 0;
   }


   //////////////////////////////////////////////////////////////////////////////
   //
   // CHARACTER PROPERTIES
   //

   // computes a scale factor to produce a font whose "height" is 'pixels' tall.
   // Height is measured as the distance from the highest ascender to the lowest
   // descender; in other words, it's equivalent to calling stbtt_GetFontVMetrics
   // and computing:
   //       scale = pixels / (ascent - descent)
   // so if you prefer to measure height by the ascent only, use a similar calculation.
   static public float stbtt_ScaleForPixelHeight(ref stbtt_fontinfo info, float height)
   {
      int fheight = ttSHORT(info.data + info.hhea + 4) - ttSHORT(info.data + info.hhea + 6);
      return (float)height / fheight;
   }

   // computes a scale factor to produce a font whose EM size is mapped to
   // 'pixels' tall. This is probably what traditional APIs compute, but
   // I'm not positive.
   static public float stbtt_ScaleForMappingEmToPixels(ref stbtt_fontinfo info, float pixels)
   {
      int unitsPerEm = ttUSHORT(info.data + info.head + 18);
      return pixels / unitsPerEm;
   }

   // ascent is the coordinate above the baseline the font extends; descent
   // is the coordinate below the baseline the font extends (i.e. it is typically negative)
   // lineGap is the spacing between one row's descent and the next row's ascent...
   // so you should advance the vertical position by "*ascent - *descent + *lineGap"
   //   these are expressed in unscaled coordinates, so you must multiply by
   //   the scale factor for a given size
   static public void stbtt_GetFontVMetrics(ref stbtt_fontinfo info, out int ascent, out int descent, out int lineGap)
   {
      ascent = ttSHORT(info.data + info.hhea + 4);
      descent = ttSHORT(info.data + info.hhea + 6);
      lineGap = ttSHORT(info.data + info.hhea + 8);
   }

   // analogous to GetFontVMetrics, but returns the "typographic" values from the OS/2
   // table (specific to MS/Windows TTF files).
   //
   // Returns 1 on success (table present), 0 on failure.
   static public bool stbtt_GetFontVMetricsOS2(ref stbtt_fontinfo info, out int typoAscent, out int typoDescent, out int typoLineGap)
   {
      int tab = (int)stbtt__find_table(info.data, (uint)info.fontstart, "OS/2");
      if (tab == 0)
      {
         typoAscent = 0;
         typoDescent = 0;
         typoLineGap = 0;
         return false;
      }

      typoAscent = ttSHORT(info.data + tab + 68);
      typoDescent = ttSHORT(info.data + tab + 70);
      typoLineGap = ttSHORT(info.data + tab + 72);
      return true;
   }

   // the bounding box around all possible characters
   static public void stbtt_GetFontBoundingBox(ref stbtt_fontinfo info, out int x0, out int y0, out int x1, out int y1)
   {
      x0 = ttSHORT(info.data + info.head + 36);
      y0 = ttSHORT(info.data + info.head + 38);
      x1 = ttSHORT(info.data + info.head + 40);
      y1 = ttSHORT(info.data + info.head + 42);
   }

   // leftSideBearing is the offset from the current horizontal position to the left edge of the character
   // advanceWidth is the offset from the current horizontal position to the next horizontal position
   //   these are expressed in unscaled coordinates
   static public void stbtt_GetCodepointHMetrics(ref stbtt_fontinfo info, int codepoint, out int advanceWidth, out int leftSideBearing)
   {
      stbtt_GetGlyphHMetrics(ref info, stbtt_FindGlyphIndex(ref info, codepoint), out advanceWidth, out leftSideBearing);
   }

   // an additional amount to add to the 'advance' value between ch1 and ch2
   static public int stbtt_GetCodepointKernAdvance(ref stbtt_fontinfo info, int ch1, int ch2)
   {
      if (info.kern == 0 && info.gpos == 0) // if no kerning table, don't waste time looking up both codepoint.glyphs
         return 0;
      return stbtt_GetGlyphKernAdvance(ref info, stbtt_FindGlyphIndex(ref info, ch1), stbtt_FindGlyphIndex(ref info, ch2));
   }

   // Gets the bounding box of the visible part of the glyph, in unscaled coordinates
   static public int stbtt_GetCodepointBox(ref stbtt_fontinfo info, int codepoint, out int x0, out int y0, out int x1, out int y1)
   {
      return stbtt_GetGlyphBox(ref info, stbtt_FindGlyphIndex(ref info, codepoint), out x0, out y0, out x1, out y1);
   }

   // as above, but takes one or more glyph indices for greater efficiency
   static public void stbtt_GetGlyphHMetrics(ref stbtt_fontinfo info, int glyph_index, out int advanceWidth, out int leftSideBearing)
   {
      stbtt_uint16 numOfLongHorMetrics = ttUSHORT(info.data + info.hhea + 34);
      if (glyph_index < numOfLongHorMetrics)
      {
         advanceWidth = ttSHORT(info.data + info.hmtx + 4 * glyph_index);
         leftSideBearing = ttSHORT(info.data + info.hmtx + 4 * glyph_index + 2);
      }
      else
      {
         advanceWidth = ttSHORT(info.data + info.hmtx + 4 * (numOfLongHorMetrics - 1));
         leftSideBearing = ttSHORT(info.data + info.hmtx + 4 * numOfLongHorMetrics + 2 * (glyph_index - numOfLongHorMetrics));
      }
   }

   static public int stbtt_GetGlyphKernAdvance(ref stbtt_fontinfo info, int g1, int g2)
   {
      int xAdvance = 0;

      if (info.gpos != 0)
         xAdvance += stbtt__GetGlyphGPOSInfoAdvance(ref info, g1, g2);
      else if (info.kern != 0)
         xAdvance += stbtt__GetGlyphKernInfoAdvance(ref info, g1, g2);

      return xAdvance;
   }

   static public int stbtt_GetGlyphBox(ref stbtt_fontinfo info, int glyph_index, out int x0, out int y0, out int x1, out int y1)
   {
      if (info.cff.size != 0)
      {
         stbtt__GetGlyphInfoT2(ref info, glyph_index, out x0, out y0, out x1, out y1);
      }
      else
      {
         int g = stbtt__GetGlyfOffset(ref info, glyph_index);
         if (g < 0)
         {
            x0 = y0 = x1 = y1 = 0;
            return 0;
         }

         x0 = ttSHORT(info.data + g + 2);
         y0 = ttSHORT(info.data + g + 4);
         x1 = ttSHORT(info.data + g + 6);
         y1 = ttSHORT(info.data + g + 8);
      }
      return 1;
   }

   public struct stbtt_kerningentry
   {
      public int glyph1; // use stbtt_FindGlyphIndex
      public int glyph2;
      public int advance;
   }

   // Retrieves a complete list of all of the kerning pairs provided by the font
   // stbtt_GetKerningTable never writes more than table_length entries and returns how many entries it did write.
   // The table will be sorted by (a.glyph1 == b.glyph1)?(a.glyph2 < b.glyph2):(a.glyph1 < b.glyph1)
   static public int stbtt_GetKerningTableLength(ref stbtt_fontinfo info)
   {
      BytePtr data = info.data + info.kern;

      // we only look at the first table. it must be 'horizontal' and format 0.
      if (info.kern == 0)
         return 0;
      if (ttUSHORT(data + 2) < 1) // number of tables, need at least 1
         return 0;
      if (ttUSHORT(data + 8) != 1) // horizontal flag must be set in format
         return 0;

      return ttUSHORT(data + 10);
   }

   static public int stbtt_GetKerningTable(ref stbtt_fontinfo info, stbtt_kerningentry[] table, int table_length)
   {
      BytePtr data = info.data + info.kern;
      int k, length;

      // we only look at the first table. it must be 'horizontal' and format 0.
      if (info.kern == 0)
         return 0;
      if (ttUSHORT(data + 2) < 1) // number of tables, need at least 1
         return 0;
      if (ttUSHORT(data + 8) != 1) // horizontal flag must be set in format
         return 0;

      length = ttUSHORT(data + 10);
      if (table_length < length)
         length = table_length;

      for (k = 0; k < length; k++)
      {
         table[k].glyph1 = ttUSHORT(data + 18 + (k * 6));
         table[k].glyph2 = ttUSHORT(data + 20 + (k * 6));
         table[k].advance = ttSHORT(data + 22 + (k * 6));
      }

      return length;
   }

   //////////////////////////////////////////////////////////////////////////////
   //
   // GLYPH SHAPES (you probably don't need these, but they have to go before
   // the bitmaps for C declaration-order reasons)
   //

   public enum STBTT : byte
   {
      vmove = 1,
      vline,
      vcurve,
      vcubic
   }

   public struct stbtt_vertex
   {
      public stbtt_vertex_type x, y, cx, cy, cx1, cy1;
      public STBTT type;
      public byte padding;
   }

   // returns non-zero if nothing is drawn for this glyph
   static public bool stbtt_IsGlyphEmpty(ref stbtt_fontinfo info, int glyph_index)
   {
      stbtt_int16 numberOfContours;
      int g;
      if (info.cff.size != 0)
         return stbtt__GetGlyphInfoT2(ref info, glyph_index, out _, out _, out _, out _) == 0;
      g = stbtt__GetGlyfOffset(ref info, glyph_index);
      if (g < 0) return true;
      numberOfContours = ttSHORT(info.data + g);
      return numberOfContours == 0;
   }

   // returns # of vertices and fills *vertices with the pointer to them
   //   these are expressed in "unscaled" coordinates
   //
   // The shape is a series of contours. Each one starts with
   // a STBTT_moveto, then consists of a series of mixed
   // STBTT_lineto and STBTT_curveto segments. A lineto
   // draws a line from previous endpoint to its x,y; a curveto
   // draws a quadratic bezier from previous endpoint to
   // its x,y, using cx,cy as the bezier control point.
   static public int stbtt_GetCodepointShape(ref stbtt_fontinfo info, int unicode_codepoint, out stbtt_vertex[] vertices)
   {
      return stbtt_GetGlyphShape(ref info, stbtt_FindGlyphIndex(ref info, unicode_codepoint), out vertices);
   }

   static public int stbtt_GetGlyphShape(ref stbtt_fontinfo info, int glyph_index, out stbtt_vertex[] pvertices)
   {
      if (info.cff.size == 0)
         return stbtt__GetGlyphShapeTT(ref info, glyph_index, out pvertices);
      else
         return stbtt__GetGlyphShapeT2(ref info, glyph_index, out pvertices);
   }

   // frees the data allocated above
   static public void stbtt_FreeShape(ref stbtt_fontinfo info, stbtt_vertex[] v)
   {
      //STBTT_free(v, info.userdata);
   }

   // fills svg with the character's SVG data.
   // returns data size or 0 if SVG not found.
   static public BytePtr stbtt_FindSVGDoc(ref stbtt_fontinfo info, int gl)
   {
      int i;
      BytePtr data = info.data;
      BytePtr svg_doc_list = data + stbtt__get_svg(ref info);

      int numEntries = ttUSHORT(svg_doc_list);
      BytePtr svg_docs = svg_doc_list + 2;

      for (i = 0; i < numEntries; i++)
      {
         BytePtr svg_doc = svg_docs + (12 * i);
         if ((gl >= ttUSHORT(svg_doc)) && (gl <= ttUSHORT(svg_doc + 2)))
            return svg_doc;
      }
      return BytePtr.Null;
   }

   static public int stbtt_GetGlyphSVG(ref stbtt_fontinfo info, int gl, out BytePtr svg)
   {
      BytePtr data = info.data;
      BytePtr svg_doc;

      if (info.svg == 0)
      {
         svg = BytePtr.Null;
         return 0;
      }

      svg_doc = stbtt_FindSVGDoc(ref info, gl);
      if (!svg_doc.IsNull)
      {
         svg = data + info.svg + ttULONG(svg_doc + 4);
         return (int)ttULONG(svg_doc + 8);
      }
      else
      {
         svg = BytePtr.Null;
         return 0;
      }
   }

   static public int stbtt_GetCodepointSVG(ref stbtt_fontinfo info, int unicode_codepoint, out BytePtr svg)
   {
      return stbtt_GetGlyphSVG(ref info, stbtt_FindGlyphIndex(ref info, unicode_codepoint), out svg);
   }

   //////////////////////////////////////////////////////////////////////////////
   //
   // BITMAP RENDERING
   //

   // frees the bitmap allocated below
   static public void stbtt_FreeBitmap(byte[] bitmap)
   {
      //STBTT_free(bitmap, userdata);
   }

   // allocates a large-enough single-channel 8bpp bitmap and renders the
   // specified character/glyph at the specified scale into it, with
   // antialiasing. 0 is no coverage (transparent), 255 is fully covered (opaque).
   // *width & *height are filled out with the width & height of the bitmap,
   // which is stored left-to-right, top-to-bottom.
   //
   // xoff/yoff are the offset it pixel space from the glyph origin to the top-left of the bitmap
   static public byte[]? stbtt_GetCodepointBitmap(ref stbtt_fontinfo info, float scale_x, float scale_y, int codepoint, out int width, out int height, out int xoff, out int yoff)
   {
      return stbtt_GetCodepointBitmapSubpixel(ref info, scale_x, scale_y, 0.0f, 0.0f, codepoint, out width, out height, out xoff, out yoff);
   }

   // the same as stbtt_GetCodepoitnBitmap, but you can specify a subpixel
   // shift for the character
   static public byte[]? stbtt_GetCodepointBitmapSubpixel(ref stbtt_fontinfo info, float scale_x, float scale_y, float shift_x, float shift_y, int codepoint, out int width, out int height, out int xoff, out int yoff)
   {
      return stbtt_GetGlyphBitmapSubpixel(ref info, scale_x, scale_y, shift_x, shift_y, stbtt_FindGlyphIndex(ref info, codepoint), out width, out height, out xoff, out yoff);
   }

   // the same as stbtt_GetCodepointBitmap, but you pass in storage for the bitmap
   // in the form of 'output', with row spacing of 'out_stride' bytes. the bitmap
   // is clipped to out_w/out_h bytes. Call stbtt_GetCodepointBitmapBox to get the
   // width and height and positioning info for it first.
   static public void stbtt_MakeCodepointBitmap(ref stbtt_fontinfo info, BytePtr output, int out_w, int out_h, int out_stride, float scale_x, float scale_y, int codepoint)
   {
      stbtt_MakeCodepointBitmapSubpixel(ref info, output, out_w, out_h, out_stride, scale_x, scale_y, 0.0f, 0.0f, codepoint);
   }

   // same as stbtt_MakeCodepointBitmap, but you can specify a subpixel
   // shift for the character
   static public void stbtt_MakeCodepointBitmapSubpixel(ref stbtt_fontinfo info, BytePtr output, int out_w, int out_h, int out_stride, float scale_x, float scale_y, float shift_x, float shift_y, int codepoint)
   {
      stbtt_MakeGlyphBitmapSubpixel(ref info, output, out_w, out_h, out_stride, scale_x, scale_y, shift_x, shift_y, stbtt_FindGlyphIndex(ref info, codepoint));
   }

   // same as stbtt_MakeCodepointBitmapSubpixel, but prefiltering
   // is performed (see stbtt_PackSetOversampling)
   static public void stbtt_MakeCodepointBitmapSubpixelPrefilter(ref stbtt_fontinfo info, BytePtr output, int out_w, int out_h, int out_stride, float scale_x, float scale_y, float shift_x, float shift_y, int oversample_x, int oversample_y, out float sub_x, out float sub_y, int codepoint)
   {
      stbtt_MakeGlyphBitmapSubpixelPrefilter(ref info, output, out_w, out_h, out_stride, scale_x, scale_y, shift_x, shift_y, oversample_x, oversample_y, out sub_x, out sub_y, stbtt_FindGlyphIndex(ref info, codepoint));
   }

   // get the bbox of the bitmap centered around the glyph origin; so the
   // bitmap width is ix1-ix0, height is iy1-iy0, and location to place
   // the bitmap top left is (leftSideBearing*scale,iy0).
   // (Note that the bitmap uses y-increases-down, but the shape uses
   // y-increases-up, so CodepointBitmapBox and CodepointBox are inverted.)
   static public void stbtt_GetCodepointBitmapBox(ref stbtt_fontinfo font, int codepoint, float scale_x, float scale_y, out int ix0, out int iy0, out int ix1, out int iy1)
   {
      stbtt_GetCodepointBitmapBoxSubpixel(ref font, codepoint, scale_x, scale_y, 0.0f, 0.0f, out ix0, out iy0, out ix1, out iy1);
   }

   // same as stbtt_GetCodepointBitmapBox, but you can specify a subpixel
   // shift for the character
   static public void stbtt_GetCodepointBitmapBoxSubpixel(ref stbtt_fontinfo font, int codepoint, float scale_x, float scale_y, float shift_x, float shift_y, out int ix0, out int iy0, out int ix1, out int iy1)
   {
      stbtt_GetGlyphBitmapBoxSubpixel(ref font, stbtt_FindGlyphIndex(ref font, codepoint), scale_x, scale_y, shift_x, shift_y, out ix0, out iy0, out ix1, out iy1);
   }

   // the following functions are equivalent to the above functions, but operate
   // on glyph indices instead of Unicode codepoints (for efficiency)
   static public byte[]? stbtt_GetGlyphBitmap(ref stbtt_fontinfo info, float scale_x, float scale_y, int glyph, out int width, out int height, out int xoff, out int yoff)
   {
      return stbtt_GetGlyphBitmapSubpixel(ref info, scale_x, scale_y, 0.0f, 0.0f, glyph, out width, out height, out xoff, out yoff);
   }
   static public byte[]? stbtt_GetGlyphBitmapSubpixel(ref stbtt_fontinfo info, float scale_x, float scale_y, float shift_x, float shift_y, int glyph, out int width, out int height, out int xoff, out int yoff)
   {
      int ix0, iy0, ix1, iy1;
      stbtt__bitmap gbm;
      stbtt_vertex[] vertices;
      int num_verts = stbtt_GetGlyphShape(ref info, glyph, out vertices);

      if (scale_x == 0) scale_x = scale_y;
      if (scale_y == 0)
      {
         if (scale_x == 0)
         {
            width = 0;
            height = 0;
            xoff = 0;
            yoff = 0;
            //STBTT_free(vertices, info.userdata);
            return null;
         }
         scale_y = scale_x;
      }

      stbtt_GetGlyphBitmapBoxSubpixel(ref info, glyph, scale_x, scale_y, shift_x, shift_y, out ix0, out iy0, out ix1, out iy1);

      // now we get the size
      gbm.w = (ix1 - ix0);
      gbm.h = (iy1 - iy0);
      gbm.pixels = BytePtr.Null; // in case we error

      width = gbm.w;
      height = gbm.h;
      xoff = ix0;
      yoff = iy0;

      if (gbm.w != 0 && gbm.h != 0)
      {
         gbm.pixels = new byte[gbm.w * gbm.h];
         //if (gbm.pixels != null) {
         gbm.stride = gbm.w;

         stbtt_Rasterize(ref gbm, 0.35f, vertices, num_verts, scale_x, scale_y, shift_x, shift_y, ix0, iy0, true);
         //}
      }
      //STBTT_free(vertices, info.userdata);
      return gbm.pixels.Raw;
   }

   static public void stbtt_MakeGlyphBitmap(ref stbtt_fontinfo info, BytePtr output, int out_w, int out_h, int out_stride, float scale_x, float scale_y, int glyph)
   {
      stbtt_MakeGlyphBitmapSubpixel(ref info, output, out_w, out_h, out_stride, scale_x, scale_y, 0.0f, 0.0f, glyph);
   }

   static public void stbtt_MakeGlyphBitmapSubpixel(ref stbtt_fontinfo info, BytePtr output, int out_w, int out_h, int out_stride, float scale_x, float scale_y, float shift_x, float shift_y, int glyph)
   {
      int ix0, iy0;
      stbtt_vertex[]? vertices;
      int num_verts = stbtt_GetGlyphShape(ref info, glyph, out vertices);

      stbtt__bitmap gbm;

      stbtt_GetGlyphBitmapBoxSubpixel(ref info, glyph, scale_x, scale_y, shift_x, shift_y, out ix0, out iy0, out _, out _);
      gbm.pixels = output;
      gbm.w = out_w;
      gbm.h = out_h;
      gbm.stride = out_stride;

      if (gbm.w != 0 && gbm.h != 0)
         stbtt_Rasterize(ref gbm, 0.35f, vertices, num_verts, scale_x, scale_y, shift_x, shift_y, ix0, iy0, true);

      //STBTT_free(vertices, info.userdata);
   }

   static public void stbtt_MakeGlyphBitmapSubpixelPrefilter(ref stbtt_fontinfo info, BytePtr output, int out_w, int out_h, int out_stride, float scale_x, float scale_y, float shift_x, float shift_y, int prefilter_x, int prefilter_y, out float sub_x, out float sub_y, int glyph)
   {
      stbtt_MakeGlyphBitmapSubpixel(ref info,
                                    output,
                                    out_w - (prefilter_x - 1),
                                    out_h - (prefilter_y - 1),
                                    out_stride,
                                    scale_x,
                                    scale_y,
                                    shift_x,
                                    shift_y,
                                    glyph);

      if (prefilter_x > 1)
         stbtt__h_prefilter(output, out_w, out_h, out_stride, prefilter_x);

      if (prefilter_y > 1)
         stbtt__v_prefilter(output, out_w, out_h, out_stride, prefilter_y);

      sub_x = stbtt__oversample_shift(prefilter_x);
      sub_y = stbtt__oversample_shift(prefilter_y);
   }

   static public void stbtt_GetGlyphBitmapBox(ref stbtt_fontinfo font, int glyph, float scale_x, float scale_y, out int ix0, out int iy0, out int ix1, out int iy1)
   {
      stbtt_GetGlyphBitmapBoxSubpixel(ref font, glyph, scale_x, scale_y, 0.0f, 0.0f, out ix0, out iy0, out ix1, out iy1);
   }
   static public void stbtt_GetGlyphBitmapBoxSubpixel(ref stbtt_fontinfo font, int glyph, float scale_x, float scale_y, float shift_x, float shift_y, out int ix0, out int iy0, out int ix1, out int iy1)
   {
      int x0 = 0, y0 = 0, x1, y1; // =0 suppresses compiler warning
      if (stbtt_GetGlyphBox(ref font, glyph, out x0, out y0, out x1, out y1) == 0)
      {
         // e.g. space character
         ix0 = 0;
         iy0 = 0;
         ix1 = 0;
         iy1 = 0;
      }
      else
      {
         // move to integral bboxes (treating pixels as little squares, what pixels get touched)?
         ix0 = STBTT_ifloor(x0 * scale_x + shift_x);
         iy0 = STBTT_ifloor(-y1 * scale_y + shift_y);
         ix1 = STBTT_iceil(x1 * scale_x + shift_x);
         iy1 = STBTT_iceil(-y0 * scale_y + shift_y);
      }
   }


   // @TODO: don't expose this structure
   public struct stbtt__bitmap
   {
      public int w, h, stride;
      public BytePtr pixels;
   }

   // rasterize a shape with quadratic beziers into a bitmap
   static public void stbtt_Rasterize(ref stbtt__bitmap result, float flatness_in_pixels, stbtt_vertex[] vertices, int num_verts, float scale_x, float scale_y, float shift_x, float shift_y, int x_off, int y_off, bool invert)
   {
      float scale = scale_x > scale_y ? scale_y : scale_x;
      stbtt__point[]? windings = stbtt_FlattenCurves(vertices, num_verts, flatness_in_pixels / scale, out int[]? winding_lengths, out int winding_count);
      if (windings != null && winding_lengths != null)
      {
#if DEBUG_USING_SVG
         string svgPath = string.Join(" ", windings.Select((w, idx) => (idx > 0 ? "L " : "M ") + w.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + w.y.ToString(System.Globalization.CultureInfo.InvariantCulture)));
         Debug.WriteLine("- Windings SVG Path: " + svgPath);
#endif

         stbtt__rasterize(ref result, windings, winding_lengths, winding_count, scale_x, scale_y, shift_x, shift_y, x_off, y_off, invert);
         //STBTT_free(winding_lengths, userdata);
         //STBTT_free(windings, userdata);
      }
   }

   //////////////////////////////////////////////////////////////////////////////
   //
   // Signed Distance Function (or Field) rendering

   // frees the SDF bitmap allocated below
   static public void stbtt_FreeSDF(byte[] bitmap)
   {
      //STBTT_free(bitmap, userdata);
   }

   // These functions compute a discretized SDF field for a single character, suitable for storing
   // in a single-channel texture, sampling with bilinear filtering, and testing against
   // larger than some threshold to produce scalable fonts.
   //        info              --  the font
   //        scale             --  controls the size of the resulting SDF bitmap, same as it would be creating a regular bitmap
   //        glyph/codepoint   --  the character to generate the SDF for
   //        padding           --  extra "pixels" around the character which are filled with the distance to the character (not 0),
   //                                 which allows effects like bit outlines
   //        onedge_value      --  value 0-255 to test the SDF against to reconstruct the character (i.e. the isocontour of the character)
   //        pixel_dist_scale  --  what value the SDF should increase by when moving one SDF "pixel" away from the edge (on the 0..255 scale)
   //                                 if positive, > onedge_value is inside; if negative, < onedge_value is inside
   //        width,height      --  output height & width of the SDF bitmap (including padding)
   //        xoff,yoff         --  output origin of the character
   //        return value      --  a 2D array of bytes 0..255, width*height in size
   //
   // pixel_dist_scale & onedge_value are a scale & bias that allows you to make
   // optimal use of the limited 0..255 for your application, trading off precision
   // and special effects. SDF values outside the range 0..255 are clamped to 0..255.
   //
   // Example:
   //      scale = stbtt_ScaleForPixelHeight(22)
   //      padding = 5
   //      onedge_value = 180
   //      pixel_dist_scale = 180/5.0 = 36.0
   //
   //      This will create an SDF bitmap in which the character is about 22 pixels
   //      high but the whole bitmap is about 22+5+5=32 pixels high. To produce a filled
   //      shape, sample the SDF at each pixel and fill the pixel if the SDF value
   //      is greater than or equal to 180/255. (You'll actually want to antialias,
   //      which is beyond the scope of this example.) Additionally, you can compute
   //      offset outlines (e.g. to stroke the character border inside & outside,
   //      or only outside). For example, to fill outside the character up to 3 SDF
   //      pixels, you would compare against (180-36.0*3)/255 = 72/255. The above
   //      choice of variables maps a range from 5 pixels outside the shape to
   //      2 pixels inside the shape to 0..255; this is intended primarily for apply
   //      outside effects only (the interior range is needed to allow proper
   //      antialiasing of the font at *smaller* sizes)
   //
   // The function computes the SDF analytically at each SDF pixel, not by e.g.
   // building a higher-res bitmap and approximating it. In theory the quality
   // should be as high as possible for an SDF of this size & representation, but
   // unclear if this is true in practice (perhaps building a higher-res bitmap
   // and computing from that can allow drop-out prevention).
   //
   // The algorithm has not been optimized at all, so expect it to be slow
   // if computing lots of characters or very large sizes.
   static public byte[]? stbtt_GetGlyphSDF(ref stbtt_fontinfo info, float scale, int glyph, int padding, byte onedge_value, float pixel_dist_scale, out int width, out int height, out int xoff, out int yoff)
   {
      float scale_x = scale, scale_y = scale;
      int ix0, iy0, ix1, iy1;
      int w, h;
      byte[] data;

      if (scale == 0)
      {
         width = 0;
         height = 0;
         xoff = 0;
         yoff = 0;
         return null;
      }

      stbtt_GetGlyphBitmapBoxSubpixel(ref info, glyph, scale, scale, 0.0f, 0.0f, out ix0, out iy0, out ix1, out iy1);

      // if empty, return NULL
      if (ix0 == ix1 || iy0 == iy1)
      {
         width = 0;
         height = 0;
         xoff = 0;
         yoff = 0;
         return null;
      }

      ix0 -= padding;
      iy0 -= padding;
      ix1 += padding;
      iy1 += padding;

      w = (ix1 - ix0);
      h = (iy1 - iy0);

      width = w;
      height = h;
      xoff = ix0;
      yoff = iy0;

      // invert for y-downwards bitmaps
      scale_y = -scale_y;

      {
         // distance from singular values (in the same units as the pixel grid)
         const float eps = 1.0f / 1024, eps2 = eps * eps;
         int x, y, i, j;
         float[] precompute;
         stbtt_vertex[]? verts;
         int num_verts = stbtt_GetGlyphShape(ref info, glyph, out verts);
         data = new byte[w * h];
         precompute = new float[num_verts];

         for (i = 0, j = num_verts - 1; i < num_verts; j = i++)
         {
            if (verts[i].type == STBTT.vline)
            {
               float x0 = verts[i].x * scale_x, y0 = verts[i].y * scale_y;
               float x1 = verts[j].x * scale_x, y1 = verts[j].y * scale_y;
               float dist = (float)STBTT_sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
               precompute[i] = (dist < eps) ? 0.0f : 1.0f / dist;
            }
            else if (verts[i].type == STBTT.vcurve)
            {
               float x2 = verts[j].x * scale_x, y2 = verts[j].y * scale_y;
               float x1 = verts[i].cx * scale_x, y1 = verts[i].cy * scale_y;
               float x0 = verts[i].x * scale_x, y0 = verts[i].y * scale_y;
               float bx = x0 - 2 * x1 + x2, by = y0 - 2 * y1 + y2;
               float len2 = bx * bx + by * by;
               if (len2 >= eps2)
                  precompute[i] = 1.0f / len2;
               else
                  precompute[i] = 0.0f;
            }
            else
               precompute[i] = 0.0f;
         }

         Span<float> res = stackalloc float[] { 0.0f, 0.0f, 0.0f };

         for (y = iy0; y < iy1; ++y)
         {
            for (x = ix0; x < ix1; ++x)
            {
               float val;
               float min_dist = 999999.0f;
               float sx = (float)x + 0.5f;
               float sy = (float)y + 0.5f;
               float x_gspace = (sx / scale_x);
               float y_gspace = (sy / scale_y);

               int winding = stbtt__compute_crossings_x(x_gspace, y_gspace, num_verts, verts); // @OPTIMIZE: this could just be a rasterization, but needs to be line vs. non-tesselated curves so a new path

               for (i = 0; i < num_verts; ++i)
               {
                  float x0 = verts[i].x * scale_x, y0 = verts[i].y * scale_y;

                  if (verts[i].type == STBTT.vline && precompute[i] != 0.0f)
                  {
                     float x1 = verts[i - 1].x * scale_x, y1 = verts[i - 1].y * scale_y;

                     float dist, dist2 = (x0 - sx) * (x0 - sx) + (y0 - sy) * (y0 - sy);
                     if (dist2 < min_dist * min_dist)
                        min_dist = (float)STBTT_sqrt(dist2);

                     // coarse culling against bbox
                     //if (sx > STBTT_min(x0,x1)-min_dist && sx < STBTT_max(x0,x1)+min_dist &&
                     //    sy > STBTT_min(y0,y1)-min_dist && sy < STBTT_max(y0,y1)+min_dist)
                     dist = (float)STBTT_fabs((x1 - x0) * (y0 - sy) - (y1 - y0) * (x0 - sx)) * precompute[i];
                     STBTT_assert(i != 0);
                     if (dist < min_dist)
                     {
                        // check position along line
                        // x' = x0 + t*(x1-x0), y' = y0 + t*(y1-y0)
                        // minimize (x'-sx)*(x'-sx)+(y'-sy)*(y'-sy)
                        float dx = x1 - x0, dy = y1 - y0;
                        float px = x0 - sx, py = y0 - sy;
                        // minimize (px+t*dx)^2 + (py+t*dy)^2 = px*px + 2*px*dx*t + t^2*dx*dx + py*py + 2*py*dy*t + t^2*dy*dy
                        // derivative: 2*px*dx + 2*py*dy + (2*dx*dx+2*dy*dy)*t, set to 0 and solve
                        float t = -(px * dx + py * dy) / (dx * dx + dy * dy);
                        if (t >= 0.0f && t <= 1.0f)
                           min_dist = dist;
                     }
                  }
                  else if (verts[i].type == STBTT.vcurve)
                  {
                     float x2 = verts[i - 1].x * scale_x, y2 = verts[i - 1].y * scale_y;
                     float x1 = verts[i].cx * scale_x, y1 = verts[i].cy * scale_y;
                     float box_x0 = STBTT_min(STBTT_min(x0, x1), x2);
                     float box_y0 = STBTT_min(STBTT_min(y0, y1), y2);
                     float box_x1 = STBTT_max(STBTT_max(x0, x1), x2);
                     float box_y1 = STBTT_max(STBTT_max(y0, y1), y2);
                     // coarse culling against bbox to avoid computing cubic unnecessarily
                     if (sx > box_x0 - min_dist && sx < box_x1 + min_dist && sy > box_y0 - min_dist && sy < box_y1 + min_dist)
                     {
                        int num = 0;
                        float ax = x1 - x0, ay = y1 - y0;
                        float bx = x0 - 2 * x1 + x2, by = y0 - 2 * y1 + y2;
                        float mx = x0 - sx, my = y0 - sy;
                        res[0] = 0.0f;
                        res[1] = 0.0f;
                        res[2] = 0.0f;
                        float px, py, t, it, dist2;
                        float a_inv = precompute[i];
                        if (a_inv == 0.0)
                        { // if a_inv is 0, it's 2nd degree so use quadratic formula
                           float a = 3 * (ax * bx + ay * by);
                           float b = 2 * (ax * ax + ay * ay) + (mx * bx + my * by);
                           float c = mx * ax + my * ay;
                           if (STBTT_fabs(a) < eps2)
                           { // if a is 0, it's linear
                              if (STBTT_fabs(b) >= eps2)
                              {
                                 res[num++] = -c / b;
                              }
                           }
                           else
                           {
                              float discriminant = b * b - 4 * a * c;
                              if (discriminant < 0)
                                 num = 0;
                              else
                              {
                                 float root = (float)STBTT_sqrt(discriminant);
                                 res[0] = (-b - root) / (2 * a);
                                 res[1] = (-b + root) / (2 * a);
                                 num = 2; // don't bother distinguishing 1-solution case, as code below will still work
                              }
                           }
                        }
                        else
                        {
                           float b = 3 * (ax * bx + ay * by) * a_inv; // could precompute this as it doesn't depend on sample point
                           float c = (2 * (ax * ax + ay * ay) + (mx * bx + my * by)) * a_inv;
                           float d = (mx * ax + my * ay) * a_inv;
                           num = stbtt__solve_cubic(b, c, d, res);
                        }
                        dist2 = (x0 - sx) * (x0 - sx) + (y0 - sy) * (y0 - sy);
                        if (dist2 < min_dist * min_dist)
                           min_dist = (float)STBTT_sqrt(dist2);

                        if (num >= 1 && res[0] >= 0.0f && res[0] <= 1.0f)
                        {
                           t = res[0]; it = 1.0f - t;
                           px = it * it * x0 + 2 * t * it * x1 + t * t * x2;
                           py = it * it * y0 + 2 * t * it * y1 + t * t * y2;
                           dist2 = (px - sx) * (px - sx) + (py - sy) * (py - sy);
                           if (dist2 < min_dist * min_dist)
                              min_dist = (float)STBTT_sqrt(dist2);
                        }
                        if (num >= 2 && res[1] >= 0.0f && res[1] <= 1.0f)
                        {
                           t = res[1]; it = 1.0f - t;
                           px = it * it * x0 + 2 * t * it * x1 + t * t * x2;
                           py = it * it * y0 + 2 * t * it * y1 + t * t * y2;
                           dist2 = (px - sx) * (px - sx) + (py - sy) * (py - sy);
                           if (dist2 < min_dist * min_dist)
                              min_dist = (float)STBTT_sqrt(dist2);
                        }
                        if (num >= 3 && res[2] >= 0.0f && res[2] <= 1.0f)
                        {
                           t = res[2]; it = 1.0f - t;
                           px = it * it * x0 + 2 * t * it * x1 + t * t * x2;
                           py = it * it * y0 + 2 * t * it * y1 + t * t * y2;
                           dist2 = (px - sx) * (px - sx) + (py - sy) * (py - sy);
                           if (dist2 < min_dist * min_dist)
                              min_dist = (float)STBTT_sqrt(dist2);
                        }
                     }
                  }
               }
               if (winding == 0)
                  min_dist = -min_dist;  // if outside the shape, value is negative
               val = onedge_value + pixel_dist_scale * min_dist;
               if (val < 0)
                  val = 0;
               else if (val > 255)
                  val = 255;
               data[(y - iy0) * w + (x - ix0)] = (byte)val;
            }
         }
         //STBTT_free(precompute, info.userdata);
         //STBTT_free(verts, info.userdata);
      }
      return data;
   }

   static public byte[]? stbtt_GetCodepointSDF(ref stbtt_fontinfo info, float scale, int codepoint, int padding, byte onedge_value, float pixel_dist_scale, out int width, out int height, out int xoff, out int yoff)
   {
      return stbtt_GetGlyphSDF(ref info, scale, stbtt_FindGlyphIndex(ref info, codepoint), padding, onedge_value, pixel_dist_scale, out width, out height, out xoff, out yoff);
   }


   //////////////////////////////////////////////////////////////////////////////
   //
   // Finding the right font...
   //
   // You should really just solve this offline, keep your own tables
   // of what font is what, and don't try to get it out of the .ttf file.
   // That's because getting it out of the .ttf file is really hard, because
   // the names in the file can appear in many possible encodings, in many
   // possible languages, and e.g. if you need a case-insensitive comparison,
   // the details of that depend on the encoding & language in a complex way
   // (actually underspecified in truetype, but also gigantic).
   //
   // But you can use the provided functions in two possible ways:
   //     stbtt_FindMatchingFont() will use *case-sensitive* comparisons on
   //             unicode-encoded names to try to find the font you want;
   //             you can run this before calling stbtt_InitFont()
   //
   //     stbtt_GetFontNameString() lets you get any of the various strings
   //             from the file yourself and do your own comparisons on them.
   //             You have to have called stbtt_InitFont() first.


   // returns the offset (not index) of the font that matches, or -1 if none
   //   if you use STBTT_MACSTYLE_DONTCARE, use a font name like "Arial Bold".
   //   if you use any other flag, use a font name like "Arial"; this checks
   //     the 'macStyle' header field; i don't know if fonts set this consistently
   static public int stbtt_FindMatchingFont(BytePtr fontdata, BytePtr name, STBTT_MACSTYLE flags)
   {
      return stbtt_FindMatchingFont_internal(fontdata, name, flags);
   }


   [Flags]
   public enum STBTT_MACSTYLE : int
   {
      DONTCARE = 0,
      STBTT_MACSTYLE_BOLD = 1,
      STBTT_MACSTYLE_ITALIC = 2,
      STBTT_MACSTYLE_UNDERSCORE = 4,
      STBTT_MACSTYLE_NONE = 8,   // <= not same as 0, this makes us check the bitfield is 0
   }

   // returns 1/0 whether the first string interpreted as utf8 is identical to
   // the second string interpreted as big-endian utf16... useful for strings from next func
   static public bool stbtt_CompareUTF8toUTF16_bigendian(BytePtr s1, int len1, BytePtr s2, int len2)
   {
      return stbtt_CompareUTF8toUTF16_bigendian_internal(s1, len1, s2, len2);
   }

   // returns the string (which may be big-endian double byte, e.g. for unicode)
   // and puts the length in bytes in *length.
   // returns results in whatever encoding you request... but note that 2-byte encodings
   // will be BIG-ENDIAN... use stbtt_CompareUTF8toUTF16_bigendian() to compare
   static public BytePtr stbtt_GetFontNameString(ref stbtt_fontinfo font, out int length, STBTT_PLATFORM_ID platformID, int encodingID, int languageID, int nameID)
   {
      stbtt_int32 i, count, stringOffset;
      BytePtr fc = font.data;
      stbtt_uint32 offset = (uint)font.fontstart;
      stbtt_uint32 nm = stbtt__find_table(fc, offset, "name");
      if (nm == 0)
      {
         length = 0;
         return BytePtr.Null;
      }

      count = ttUSHORT(fc + nm + 2);
      stringOffset = (int)(nm + ttUSHORT(fc + nm + 4));
      for (i = 0; i < count; ++i)
      {
         stbtt_uint32 loc = (uint)(nm + 6 + 12 * i);
         if (platformID == (STBTT_PLATFORM_ID)ttUSHORT(fc + loc + 0) && encodingID == ttUSHORT(fc + loc + 2)
             && languageID == ttUSHORT(fc + loc + 4) && nameID == ttUSHORT(fc + loc + 6))
         {
            length = ttUSHORT(fc + loc + 8);
            return (fc + stringOffset + ttUSHORT(fc + loc + 10));
         }
      }

      length = 0;
      return BytePtr.Null;
   }

   // some of the values for the IDs are below; for more see the truetype spec:
   //     http://developer.apple.com/textfonts/TTRefMan/RM06/Chap6name.html
   //     http://www.microsoft.com/typography/otspec/name.htm

   public enum STBTT_PLATFORM_ID
   { // platformID
      UNICODE = 0,
      MAC = 1,
      ISO = 2,
      MICROSOFT = 3
   };

   public enum STBTT_UNICODE_EID
   { // encodingID for STBTT_PLATFORM_ID_UNICODE
      UNICODE_1_0 = 0,
      UNICODE_1_1 = 1,
      ISO_10646 = 2,
      UNICODE_2_0_BMP = 3,
      UNICODE_2_0_FULL = 4
   };

   public enum STBTT_MS_EID
   { // encodingID for STBTT_PLATFORM_ID_MICROSOFT
      SYMBOL = 0,
      UNICODE_BMP = 1,
      SHIFTJIS = 2,
      UNICODE_FULL = 10
   };

   public enum STBTT_MAC_EID
   { // encodingID for STBTT_PLATFORM_ID_MAC; same as Script Manager codes
      ROMAN = 0, STBTT_MAC_EID_ARABIC = 4,
      JAPANESE = 1, STBTT_MAC_EID_HEBREW = 5,
      CHINESE_TRAD = 2, STBTT_MAC_EID_GREEK = 6,
      KOREAN = 3, STBTT_MAC_EID_RUSSIAN = 7
   };

   public enum STBTT_MS_LANG
   { // languageID for STBTT_PLATFORM_ID_MICROSOFT; same as LCID...
     // problematic because there are e.g. 16 english LCIDs and 16 arabic LCIDs
      ENGLISH = 0x0409,
      ITALIAN = 0x0410,
      CHINESE = 0x0804,
      JAPANESE = 0x0411,
      DUTCH = 0x0413,
      KOREAN = 0x0412,
      FRENCH = 0x040c,
      RUSSIAN = 0x0419,
      GERMAN = 0x0407,
      SPANISH = 0x0409,
      HEBREW = 0x040d,
      SWEDISH = 0x041D
   };

   public enum STBTT_MAC_LANG
   { // languageID for STBTT_PLATFORM_ID_MAC
      ENGLISH = 0,
      JAPANESE = 11,
      ARABIC = 12,
      KOREAN = 23,
      DUTCH = 4,
      RUSSIAN = 32,
      FRENCH = 1,
      SPANISH = 6,
      GERMAN = 2,
      SWEDISH = 5,
      HEBREW = 10,
      CHINESE_SIMPLIFIED = 33,
      ITALIAN = 3,
      CHINESE_TRAD = 19
   };

   ///////////////////////////////////////////////////////////////////////////////
   ///////////////////////////////////////////////////////////////////////////////
   ////
   ////   IMPLEMENTATION
   ////
   ////
   /// <summary>
   /// 
   /// </summary>

   [Conditional("DEBUG")]
   static private void STBTT_assert([DoesNotReturnIf(false)] bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
   {
      Debug.Assert(condition, message, string.Empty);
   }

   private const int STBTT_MAX_OVERSAMPLE = 8;

   // typedef int stbtt__test_oversample_pow2[(STBTT_MAX_OVERSAMPLE & (STBTT_MAX_OVERSAMPLE-1)) == 0 ? 1 : -1];

   //////////////////////////////////////////////////////////////////////////
   //
   // stbtt__buf helpers to parse data from file
   //

   static stbtt_uint8 stbtt__buf_get8(ref stbtt__buf b)
   {
      if (b.cursor >= b.size)
         return 0;
      return b.data[b.cursor++];
   }

   static stbtt_uint8 stbtt__buf_peek8(ref stbtt__buf b)
   {
      if (b.cursor >= b.size)
         return 0;
      return b.data[b.cursor];
   }

   static void stbtt__buf_seek(ref stbtt__buf b, int o)
   {
      STBTT_assert(!(o > b.size || o < 0));
      b.cursor = (o > b.size || o < 0) ? b.size : o;
   }

   static void stbtt__buf_skip(ref stbtt__buf b, int o)
   {
      stbtt__buf_seek(ref b, b.cursor + o);
   }

   static stbtt_uint32 stbtt__buf_get(ref stbtt__buf b, int n)
   {
      stbtt_uint32 v = 0;
      int i;
      STBTT_assert(n >= 1 && n <= 4);
      for (i = 0; i < n; i++)
         v = (v << 8) | stbtt__buf_get8(ref b);
      return v;
   }

   static stbtt__buf stbtt__new_buf(BytePtr p, size_t size)
   {
      stbtt__buf r;
      STBTT_assert(size < 0x40000000);
      r.data = p;
      r.size = (int)size;
      r.cursor = 0;
      return r;
   }

   static stbtt_uint16 stbtt__buf_get16(ref stbtt__buf b) => (stbtt_uint16)stbtt__buf_get(ref b, 2);
   static stbtt_int32 stbtt__buf_get32(ref stbtt__buf b) => (stbtt_int32)stbtt__buf_get(ref b, 4);

   static stbtt__buf stbtt__buf_range(ref stbtt__buf b, int o, int s)
   {
      stbtt__buf r = stbtt__new_buf(BytePtr.Null, 0);
      if (o < 0 || s < 0 || o > b.size || s > b.size - o) return r;
      r.data = b.data + o;
      r.size = s;
      return r;
   }

   static stbtt__buf stbtt__cff_get_index(ref stbtt__buf b)
   {
      int count, start, offsize;
      start = b.cursor;
      count = stbtt__buf_get16(ref b);
      if (count != 0)
      {
         offsize = stbtt__buf_get8(ref b);
         STBTT_assert(offsize >= 1 && offsize <= 4);
         stbtt__buf_skip(ref b, offsize * count);
         stbtt__buf_skip(ref b, (int)(stbtt__buf_get(ref b, offsize) - 1));
      }
      return stbtt__buf_range(ref b, start, b.cursor - start);
   }

   static stbtt_uint32 stbtt__cff_int(ref stbtt__buf b)
   {
      int b0 = stbtt__buf_get8(ref b);
      if (b0 >= 32 && b0 <= 246) return (stbtt_uint32)(b0 - 139);
      else if (b0 >= 247 && b0 <= 250) return (stbtt_uint32)((b0 - 247) * 256 + stbtt__buf_get8(ref b) + 108);
      else if (b0 >= 251 && b0 <= 254) return (stbtt_uint32)(-(b0 - 251) * 256 - stbtt__buf_get8(ref b) - 108);
      else if (b0 == 28) return (stbtt_uint32)(stbtt__buf_get16(ref b));
      else if (b0 == 29) return (stbtt_uint32)(stbtt__buf_get32(ref b));
      STBTT_assert(false);
      return 0;
   }

   static void stbtt__cff_skip_operand(ref stbtt__buf b)
   {
      int v, b0 = stbtt__buf_peek8(ref b);
      STBTT_assert(b0 >= 28);
      if (b0 == 30)
      {
         stbtt__buf_skip(ref b, 1);
         while (b.cursor < b.size)
         {
            v = stbtt__buf_get8(ref b);
            if ((v & 0xF) == 0xF || (v >> 4) == 0xF)
               break;
         }
      }
      else
      {
         stbtt__cff_int(ref b);
      }
   }

   static stbtt__buf stbtt__dict_get(ref stbtt__buf b, int key)
   {
      stbtt__buf_seek(ref b, 0);
      while (b.cursor < b.size)
      {
         int start = b.cursor, end, op;
         while (stbtt__buf_peek8(ref b) >= 28)
            stbtt__cff_skip_operand(ref b);
         end = b.cursor;
         op = stbtt__buf_get8(ref b);
         if (op == 12) op = stbtt__buf_get8(ref b) | 0x100;
         if (op == key) return stbtt__buf_range(ref b, start, end - start);
      }
      return stbtt__buf_range(ref b, 0, 0);
   }

   static void stbtt__dict_get_ints(ref stbtt__buf b, int key, int outcount, Span<stbtt_uint32> _out)
   {
      int i;
      stbtt__buf operands = stbtt__dict_get(ref b, key);
      for (i = 0; i < outcount && operands.cursor < operands.size; i++)
         _out[i] = stbtt__cff_int(ref operands);
   }

   static void stbtt__dict_get_ints(ref stbtt__buf b, int key, int outcount, out stbtt_uint32 _out)
   {
      STBTT_assert(outcount != 0);
      int i;
      stbtt__buf operands = stbtt__dict_get(ref b, key);
      _out = 0;
      for (i = 0; i < outcount && operands.cursor < operands.size; i++)
         _out = stbtt__cff_int(ref operands);
   }

   static int stbtt__cff_index_count(ref stbtt__buf b)
   {
      stbtt__buf_seek(ref b, 0);
      return stbtt__buf_get16(ref b);
   }

   static stbtt__buf stbtt__cff_index_get(stbtt__buf b, int i)
   {
      int count, offsize, start, end;
      stbtt__buf_seek(ref b, 0);
      count = stbtt__buf_get16(ref b);
      offsize = stbtt__buf_get8(ref b);
      STBTT_assert(i >= 0 && i < count);
      STBTT_assert(offsize >= 1 && offsize <= 4);
      stbtt__buf_skip(ref b, i * offsize);
      start = (int)stbtt__buf_get(ref b, offsize);
      end = (int)stbtt__buf_get(ref b, offsize);
      return stbtt__buf_range(ref b, 2 + (count + 1) * offsize + start, end - start);
   }

   //////////////////////////////////////////////////////////////////////////
   //
   // accessors to parse data from file
   //

   // on platforms that don't allow misaligned reads, if we want to allow
   // truetype fonts that aren't padded to alignment, define ALLOW_UNALIGNED_TRUETYPE

   static stbtt_uint8 ttBYTE(BytePtr p) => p[0];
   static stbtt_int8 ttCHAR(BytePtr p) => (stbtt_int8)(byte)p[0];
   static stbtt_int32 ttFixed(BytePtr p) => ttLONG(p);

   static stbtt_uint16 ttUSHORT(BytePtr p) { return (stbtt_uint16)(p[0] * 256 + p[1]); }
   static stbtt_int16 ttSHORT(BytePtr p) { return (stbtt_int16)(p[0] * 256 + p[1]); }
   static stbtt_uint32 ttULONG(BytePtr p) { return (stbtt_uint32)((p[0] << 24) + (p[1] << 16) + (p[2] << 8) + p[3]); }
   static stbtt_int32 ttLONG(BytePtr p) { return (p[0] << 24) + (p[1] << 16) + (p[2] << 8) + p[3]; }

   static bool stbtt_tag4(BytePtr p, byte c0, byte c1, byte c2, byte c3) => ((p)[0] == (c0) && (p)[1] == (c1) && (p)[2] == (c2) && (p)[3] == (c3));
   static bool stbtt_tag(BytePtr p, string str) => stbtt_tag4(p, (byte)str[0], (byte)str[1], (byte)str[2], (byte)str[3]);

   static bool stbtt__isfont(BytePtr font)
   {
      // check the version number
      if (stbtt_tag4(font, (byte)'1', 0, 0, 0)) return true; // TrueType 1
      if (stbtt_tag(font, "typ1")) return true; // TrueType with type 1 font -- we don't support this!
      if (stbtt_tag(font, "OTTO")) return true; // OpenType with CFF
      if (stbtt_tag4(font, 0, 1, 0, 0)) return true; // OpenType 1.0
      if (stbtt_tag(font, "true")) return true; // Apple specification for TrueType fonts
      return false;
   }

   // @OPTIMIZE: binary search
   static stbtt_uint32 stbtt__find_table(BytePtr data, stbtt_uint32 fontstart, string tag)
   {
      stbtt_int32 num_tables = ttUSHORT(data + fontstart + 4);
      stbtt_uint32 tabledir = fontstart + 12;
      stbtt_int32 i;
      for (i = 0; i < num_tables; ++i)
      {
         stbtt_uint32 loc = (stbtt_uint32)(tabledir + 16 * i);
         if (stbtt_tag(data + loc + 0, tag))
            return ttULONG(data + loc + 8);
      }
      return 0;
   }

   static int stbtt_GetFontOffsetForIndex_internal(BytePtr font_collection, int index)
   {
      // if it's just a font, there's only one valid index
      if (stbtt__isfont(font_collection))
         return index == 0 ? 0 : -1;

      // check if it's a TTC
      if (stbtt_tag(font_collection, "ttcf"))
      {
         // version 1?
         if (ttULONG(font_collection + 4) == 0x00010000 || ttULONG(font_collection + 4) == 0x00020000)
         {
            stbtt_int32 n = ttLONG(font_collection + 8);
            if (index >= n)
               return -1;
            return (int)ttULONG(font_collection + 12 + index * 4);
         }
      }
      return -1;
   }

   static int stbtt_GetNumberOfFonts_internal(BytePtr font_collection)
   {
      // if it's just a font, there's only one valid font
      if (stbtt__isfont(font_collection))
         return 1;

      // check if it's a TTC
      if (stbtt_tag(font_collection, "ttcf"))
      {
         // version 1?
         if (ttULONG(font_collection + 4) == 0x00010000 || ttULONG(font_collection + 4) == 0x00020000)
         {
            return ttLONG(font_collection + 8);
         }
      }
      return 0;
   }

   static stbtt__buf stbtt__get_subrs(stbtt__buf cff, stbtt__buf fontdict)
   {
      stbtt_uint32 subrsoff = 0;
      Span<stbtt_uint32> private_loc = stackalloc[] { (uint)0, (uint)0 };
      stbtt__buf pdict;
      stbtt__dict_get_ints(ref fontdict, 18, 2, private_loc);
      if (private_loc[1] == 0 || private_loc[0] == 0) return stbtt__new_buf(BytePtr.Null, 0);
      pdict = stbtt__buf_range(ref cff, (int)private_loc[1], (int)private_loc[0]);
      stbtt__dict_get_ints(ref pdict, 19, 1, out subrsoff);
      if (subrsoff == 0) return stbtt__new_buf(BytePtr.Null, 0);
      stbtt__buf_seek(ref cff, (int)(private_loc[1] + subrsoff));
      return stbtt__cff_get_index(ref cff);
   }

   // since most people won't use this, find this table the first time it's needed
   static int stbtt__get_svg(ref stbtt_fontinfo info)
   {
      stbtt_uint32 t;
      if (info.svg < 0)
      {
         t = stbtt__find_table(info.data, (uint)info.fontstart, "SVG ");
         if (t != 0)
         {
            stbtt_uint32 offset = ttULONG(info.data + t + 2);
            info.svg = (int)(t + offset);
         }
         else
         {
            info.svg = 0;
         }
      }
      return info.svg;
   }

   static int stbtt_InitFont_internal(out stbtt_fontinfo info, BytePtr data, int fontstart)
   {
      info = default;

      stbtt_uint32 cmap, t;
      stbtt_int32 i, numTables;

      info.data = data;
      info.fontstart = fontstart;
      info.cff = stbtt__new_buf(BytePtr.Null, 0);

      cmap = stbtt__find_table(data, (stbtt_uint32)fontstart, "cmap");       // required
      info.loca = (int)stbtt__find_table(data, (stbtt_uint32)fontstart, "loca"); // required
      info.head = (int)stbtt__find_table(data, (stbtt_uint32)fontstart, "head"); // required
      info.glyf = (int)stbtt__find_table(data, (stbtt_uint32)fontstart, "glyf"); // required
      info.hhea = (int)stbtt__find_table(data, (stbtt_uint32)fontstart, "hhea"); // required
      info.hmtx = (int)stbtt__find_table(data, (stbtt_uint32)fontstart, "hmtx"); // required
      info.kern = (int)stbtt__find_table(data, (stbtt_uint32)fontstart, "kern"); // not required
      info.gpos = (int)stbtt__find_table(data, (stbtt_uint32)fontstart, "GPOS"); // not required

      if (cmap == 0 || info.head == 0 || info.hhea == 0 || info.hmtx == 0)
      {
         return 0;
      }
      if (info.glyf != 0)
      {
         // required for truetype
         if (info.loca == 0) return 0;
      }
      else
      {
         // initialization for CFF / Type2 fonts (OTF)
         stbtt__buf b, topdict, topdictidx;
         stbtt_uint32 cstype = 2, charstrings = 0, fdarrayoff = 0, fdselectoff = 0;
         stbtt_uint32 cff;

         cff = stbtt__find_table(data, (stbtt_uint32)fontstart, "CFF ");
         if (cff == 0) return 0;

         info.fontdicts = stbtt__new_buf(BytePtr.Null, 0);
         info.fdselect = stbtt__new_buf(BytePtr.Null, 0);

         // @TODO this should use size from table (not 512MB)
         info.cff = stbtt__new_buf(data + cff, 512 * 1024 * 1024);
         b = info.cff;

         // read the header
         stbtt__buf_skip(ref b, 2);
         stbtt__buf_seek(ref b, stbtt__buf_get8(ref b)); // hdrsize

         // @TODO the name INDEX could list multiple fonts,
         // but we just use the first one.
         stbtt__cff_get_index(ref b);  // name INDEX
         topdictidx = stbtt__cff_get_index(ref b);
         topdict = stbtt__cff_index_get(topdictidx, 0);
         stbtt__cff_get_index(ref b);  // string INDEX
         info.gsubrs = stbtt__cff_get_index(ref b);

         stbtt__dict_get_ints(ref topdict, 17, 1, out charstrings);
         stbtt__dict_get_ints(ref topdict, 0x100 | 6, 1, out cstype);
         stbtt__dict_get_ints(ref topdict, 0x100 | 36, 1, out fdarrayoff);
         stbtt__dict_get_ints(ref topdict, 0x100 | 37, 1, out fdselectoff);
         info.subrs = stbtt__get_subrs(b, topdict);

         // we only support Type 2 charstrings
         if (cstype != 2) return 0;
         if (charstrings == 0) return 0;

         if (fdarrayoff != 0)
         {
            // looks like a CID font
            if (fdselectoff == 0) return 0;
            stbtt__buf_seek(ref b, (int)fdarrayoff);
            info.fontdicts = stbtt__cff_get_index(ref b);
            info.fdselect = stbtt__buf_range(ref b, (int)fdselectoff, (int)(b.size - fdselectoff));
         }

         stbtt__buf_seek(ref b, (int)charstrings);
         info.charstrings = stbtt__cff_get_index(ref b);
      }

      t = stbtt__find_table(data, (uint)fontstart, "maxp");
      if (t != 0)
         info.numGlyphs = ttUSHORT(data + t + 4);
      else
         info.numGlyphs = 0xffff;

      info.svg = -1;

      // find a cmap encoding table we understand *now* to avoid searching
      // later. (todo: could make this installable)
      // the same regardless of glyph.
      numTables = ttUSHORT(data + cmap + 2);
      info.index_map = 0;
      for (i = 0; i < numTables; ++i)
      {
         stbtt_uint32 encoding_record = (uint)(cmap + 4 + 8 * i);
         // find an encoding we understand:
         switch (ttUSHORT(data + encoding_record))
         {
            case (ushort)STBTT_PLATFORM_ID.MICROSOFT:
               switch (ttUSHORT(data + encoding_record + 2))
               {
                  case (ushort)STBTT_MS_EID.UNICODE_BMP:
                  case (ushort)STBTT_MS_EID.UNICODE_FULL:
                     // MS/Unicode
                     info.index_map = (int)(cmap + ttULONG(data + encoding_record + 4));
                     break;
               }
               break;
            case (ushort)STBTT_PLATFORM_ID.UNICODE:
               // Mac/iOS has these
               // all the encodingIDs are unicode, so we don't bother to check it
               info.index_map = (int)(cmap + ttULONG(data + encoding_record + 4));
               break;
         }
      }
      if (info.index_map == 0)
         return 0;

      info.indexToLocFormat = ttUSHORT(data + info.head + 50);
      return 1;
   }




   static void stbtt_setvertex(ref stbtt_vertex v, STBTT type, stbtt_int32 x, stbtt_int32 y, stbtt_int32 cx, stbtt_int32 cy)
   {
      v = new stbtt_vertex();
      v.type = type;
      v.x = (stbtt_int16)x;
      v.y = (stbtt_int16)y;
      v.cx = (stbtt_int16)cx;
      v.cy = (stbtt_int16)cy;
   }

   static int stbtt__GetGlyfOffset(ref stbtt_fontinfo info, int glyph_index)
   {
      int g1, g2;

      STBTT_assert(info.cff.size == 0);

      if (glyph_index >= info.numGlyphs) return -1; // glyph index out of range
      if (info.indexToLocFormat >= 2) return -1; // unknown index.glyph map format

      if (info.indexToLocFormat == 0)
      {
         g1 = info.glyf + ttUSHORT(info.data + info.loca + glyph_index * 2) * 2;
         g2 = info.glyf + ttUSHORT(info.data + info.loca + glyph_index * 2 + 2) * 2;
      }
      else
      {
         g1 = (int)(info.glyf + ttULONG(info.data + info.loca + glyph_index * 4));
         g2 = (int)(info.glyf + ttULONG(info.data + info.loca + glyph_index * 4 + 4));
      }

      return g1 == g2 ? -1 : g1; // if length is 0, return -1
   }

   static int stbtt__GetGlyphInfoT2(ref stbtt_fontinfo info, int glyph_index, out int x0, out int y0, out int x1, out int y1)
   {
      stbtt__csctx c = STBTT__CSCTX_INIT(1);
      bool r = stbtt__run_charstring(ref info, glyph_index, ref c);
      x0 = r ? c.min_x : 0;
      y0 = r ? c.min_y : 0;
      x1 = r ? c.max_x : 0;
      y1 = r ? c.max_y : 0;
      return r ? c.num_vertices : 0;
   }

   static int stbtt__close_shape(stbtt_vertex[] vertices, int num_vertices, bool was_off, bool start_off,
       stbtt_int32 sx, stbtt_int32 sy, stbtt_int32 scx, stbtt_int32 scy, stbtt_int32 cx, stbtt_int32 cy)
   {
      if (start_off)
      {
         if (was_off)
            stbtt_setvertex(ref vertices[num_vertices++], STBTT.vcurve, (cx + scx) >> 1, (cy + scy) >> 1, cx, cy);
         stbtt_setvertex(ref vertices[num_vertices++], STBTT.vcurve, sx, sy, scx, scy);
      }
      else
      {
         if (was_off)
            stbtt_setvertex(ref vertices[num_vertices++], STBTT.vcurve, sx, sy, cx, cy);
         else
            stbtt_setvertex(ref vertices[num_vertices++], STBTT.vline, sx, sy, 0, 0);
      }
      return num_vertices;
   }

   static int stbtt__GetGlyphShapeTT(ref stbtt_fontinfo info, int glyph_index, out stbtt_vertex[] pvertices)
   {
      stbtt_int16 numberOfContours;
      BytePtr endPtsOfContours;
      BytePtr data = info.data;
      stbtt_vertex[] vertices = [];
      int num_vertices = 0;
      int g = stbtt__GetGlyfOffset(ref info, glyph_index);
      pvertices = [];

      if (g < 0) return 0;

      numberOfContours = ttSHORT(data + g);

      if (numberOfContours > 0)
      {
         stbtt_uint8 flags = 0, flagcount;
         stbtt_int32 ins, i, j = 0, m, n, next_move, off;
         bool was_off = false, start_off = false;
         stbtt_int32 x, y, cx, cy, sx, sy, scx, scy;
         BytePtr points;
         endPtsOfContours = (data + g + 10);
         ins = ttUSHORT(data + g + 10 + numberOfContours * 2);
         points = data + g + 10 + numberOfContours * 2 + 2 + ins;

         n = 1 + ttUSHORT(endPtsOfContours + (int)(numberOfContours * 2 - 2));

         m = n + 2 * numberOfContours;  // a loose bound on how many vertices we might need
         vertices = new stbtt_vertex[m];
         //if (vertices == null)
         //   return 0;

         next_move = 0;
         flagcount = 0;

         // in first pass, we load uninterpreted data into the allocated array
         // above, shifted to the end of the array so we won't overwrite it when
         // we create our final data starting from the front

         off = m - n; // starting offset for uninterpreted data, regardless of how m ends up being calculated

         // first load flags

         for (i = 0; i < n; ++i)
         {
            if (flagcount == 0)
            {
               flags = points++;
               if ((flags & 8) != 0)
                  flagcount = points++;
            }
            else
               --flagcount;
            vertices[off + i].type = (STBTT)flags;
         }

         // now load x coordinates
         x = 0;
         for (i = 0; i < n; ++i)
         {
            flags = (byte)vertices[off + i].type;
            if ((flags & 2) != 0)
            {
               stbtt_int16 dx = points++;
               x += (flags & 16) != 0 ? dx : -dx; // ???
            }
            else
            {
               if ((flags & 16) == 0)
               {
                  x = x + (stbtt_int16)(points[0] * 256 + points[1]);
                  points += 2;
               }
            }
            vertices[off + i].x = (stbtt_int16)x;
         }

         // now load y coordinates
         y = 0;
         for (i = 0; i < n; ++i)
         {
            flags = (byte)vertices[off + i].type;
            if ((flags & 4) != 0)
            {
               stbtt_int16 dy = points++;
               y += (flags & 32) != 0 ? dy : -dy; // ???
            }
            else
            {
               if ((flags & 32) == 0)
               {
                  y = y + (stbtt_int16)(points[0] * 256 + points[1]);
                  points += 2;
               }
            }
            vertices[off + i].y = (stbtt_int16)y;
         }

         // now convert them to our format
         num_vertices = 0;
         sx = sy = cx = cy = scx = scy = 0;
         for (i = 0; i < n; ++i)
         {
            flags = (byte)vertices[off + i].type;
            x = (stbtt_int16)vertices[off + i].x;
            y = (stbtt_int16)vertices[off + i].y;

            if (next_move == i)
            {
               if (i != 0)
                  num_vertices = stbtt__close_shape(vertices, num_vertices, was_off, start_off, sx, sy, scx, scy, cx, cy);

               // now start the new one
               start_off = (flags & 1) == 0;
               if (start_off)
               {
                  // if we start off with an off-curve point, then when we need to find a point on the curve
                  // where we can start, and we need to save some state for when we wraparound.
                  scx = x;
                  scy = y;
                  if (((int)((vertices[off + i + 1].type)) & 1) == 0)
                  {
                     // next point is also a curve point, so interpolate an on-point curve
                     sx = (x + (stbtt_int32)vertices[off + i + 1].x) >> 1;
                     sy = (y + (stbtt_int32)vertices[off + i + 1].y) >> 1;
                  }
                  else
                  {
                     // otherwise just use the next point as our start point
                     sx = (stbtt_int32)vertices[off + i + 1].x;
                     sy = (stbtt_int32)vertices[off + i + 1].y;
                     ++i; // we're using point i+1 as the starting point, so skip it
                  }
               }
               else
               {
                  sx = x;
                  sy = y;
               }
               stbtt_setvertex(ref vertices[num_vertices++], STBTT.vmove, sx, sy, 0, 0);
               was_off = false;
               next_move = 1 + ttUSHORT(endPtsOfContours + j * 2);
               ++j;
            }
            else
            {
               if ((flags & 1) == 0)
               { // if it's a curve
                  if (was_off) // two off-curve control points in a row means interpolate an on-curve midpoint
                     stbtt_setvertex(ref vertices[num_vertices++], STBTT.vcurve, (cx + x) >> 1, (cy + y) >> 1, cx, cy);
                  cx = x;
                  cy = y;
                  was_off = true;
               }
               else
               {
                  if (was_off)
                     stbtt_setvertex(ref vertices[num_vertices++], STBTT.vcurve, x, y, cx, cy);
                  else
                     stbtt_setvertex(ref vertices[num_vertices++], STBTT.vline, x, y, 0, 0);
                  was_off = false;
               }
            }
         }
         num_vertices = stbtt__close_shape(vertices, num_vertices, was_off, start_off, sx, sy, scx, scy, cx, cy);
      }
      else if (numberOfContours < 0)
      {
         // Compound shapes.
         int more = 1;
         BytePtr comp = data + g + 10;
         num_vertices = 0;
         vertices = [];
         while (more != 0)
         {
            stbtt_uint16 flags, gidx;
            int comp_num_verts = 0, i;
            stbtt_vertex[]? comp_verts = null, tmp = null;
            Span<float> mtx = stackalloc[] { 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f };
            float m, n;

            flags = (ushort)ttSHORT(comp); comp += 2;
            gidx = (ushort)ttSHORT(comp); comp += 2;

            if ((flags & 2) != 0)
            { // XY values
               if ((flags & 1) != 0)
               { // shorts
                  mtx[4] = ttSHORT(comp); comp += 2;
                  mtx[5] = ttSHORT(comp); comp += 2;
               }
               else
               {
                  mtx[4] = ttCHAR(comp); comp += 1;
                  mtx[5] = ttCHAR(comp); comp += 1;
               }
            }
            else
            {
               // @TODO handle matching point
               STBTT_assert(false);
            }
            if ((flags & (1 << 3)) != 0)
            { // WE_HAVE_A_SCALE
               mtx[0] = mtx[3] = ttSHORT(comp) / 16384.0f; comp += 2;
               mtx[1] = mtx[2] = 0;
            }
            else if ((flags & (1 << 6)) != 0)
            { // WE_HAVE_AN_X_AND_YSCALE
               mtx[0] = ttSHORT(comp) / 16384.0f; comp += 2;
               mtx[1] = mtx[2] = 0;
               mtx[3] = ttSHORT(comp) / 16384.0f; comp += 2;
            }
            else if ((flags & (1 << 7)) != 0)
            { // WE_HAVE_A_TWO_BY_TWO
               mtx[0] = ttSHORT(comp) / 16384.0f; comp += 2;
               mtx[1] = ttSHORT(comp) / 16384.0f; comp += 2;
               mtx[2] = ttSHORT(comp) / 16384.0f; comp += 2;
               mtx[3] = ttSHORT(comp) / 16384.0f; comp += 2;
            }

            // Find transformation scales.
            m = (float)STBTT_sqrt(mtx[0] * mtx[0] + mtx[1] * mtx[1]);
            n = (float)STBTT_sqrt(mtx[2] * mtx[2] + mtx[3] * mtx[3]);

            // Get indexed glyph.
            comp_num_verts = stbtt_GetGlyphShape(ref info, gidx, out comp_verts);
            if (comp_num_verts > 0)
            {
               // Transform vertices.
               for (i = 0; i < comp_num_verts; ++i)
               {
                  ref stbtt_vertex v = ref comp_verts[i];
                  stbtt_vertex_type x, y;
                  x = v.x; y = v.y;
                  v.x = (stbtt_vertex_type)(m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                  v.y = (stbtt_vertex_type)(n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                  x = v.cx; y = v.cy;
                  v.cx = (stbtt_vertex_type)(m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                  v.cy = (stbtt_vertex_type)(n * (mtx[1] * x + mtx[3] * y + mtx[5]));
               }
               // Append vertices.
               tmp = new stbtt_vertex[num_vertices + comp_num_verts];
               //if (tmp == null)
               //{
                  //if (vertices) STBTT_free(vertices, info.userdata);
                  //if (comp_verts) STBTT_free(comp_verts, info.userdata);
               //   return 0;
               //}
               if (num_vertices > 0) Array.Copy(vertices, tmp, num_vertices);
               Array.Copy(comp_verts, 0, tmp, num_vertices, comp_num_verts);
               //if (vertices) STBTT_free(vertices, info.userdata);
               vertices = tmp;
               //STBTT_free(comp_verts, info.userdata);
               num_vertices += comp_num_verts;
            }
            // More components ?
            more = flags & (1 << 5);
         }
      }
      else
      {
         // numberOfCounters == 0, do nothing
      }

      pvertices = vertices;
      return num_vertices;
   }

   private struct stbtt__csctx
   {
      public int bounds;
      public bool started;
      public float first_x, first_y;
      public float x, y;
      public stbtt_int32 min_x, max_x, min_y, max_y;

      public stbtt_vertex[] pvertices;
      public int num_vertices;
   }

   private static stbtt__csctx STBTT__CSCTX_INIT(int bounds)
   {
      stbtt__csctx ctx = new stbtt__csctx();
      ctx.bounds = bounds;
      return ctx;
   }

   static void stbtt__track_vertex(ref stbtt__csctx c, stbtt_int32 x, stbtt_int32 y)
   {
      if (x > c.max_x || !c.started) c.max_x = x;
      if (y > c.max_y || !c.started) c.max_y = y;
      if (x < c.min_x || !c.started) c.min_x = x;
      if (y < c.min_y || !c.started) c.min_y = y;
      c.started = true;
   }

   static void stbtt__csctx_v(ref stbtt__csctx c, STBTT type, stbtt_int32 x, stbtt_int32 y, stbtt_int32 cx, stbtt_int32 cy, stbtt_int32 cx1, stbtt_int32 cy1)
   {
      if (c.bounds != 0)
      {
         stbtt__track_vertex(ref c, x, y);
         if (type == STBTT.vcubic)
         {
            stbtt__track_vertex(ref c, cx, cy);
            stbtt__track_vertex(ref c, cx1, cy1);
         }
      }
      else
      {
         stbtt_setvertex(ref c.pvertices[c.num_vertices], type, x, y, cx, cy);
         c.pvertices[c.num_vertices].cx1 = (stbtt_int16)cx1;
         c.pvertices[c.num_vertices].cy1 = (stbtt_int16)cy1;
      }
      c.num_vertices++;
   }

   static void stbtt__csctx_close_shape(ref stbtt__csctx ctx)
   {
      if (ctx.first_x != ctx.x || ctx.first_y != ctx.y)
         stbtt__csctx_v(ref ctx, STBTT.vline, (int)ctx.first_x, (int)ctx.first_y, 0, 0, 0, 0);
   }

   static void stbtt__csctx_rmove_to(ref stbtt__csctx ctx, float dx, float dy)
   {
      stbtt__csctx_close_shape(ref ctx);
      ctx.first_x = ctx.x = ctx.x + dx;
      ctx.first_y = ctx.y = ctx.y + dy;
      stbtt__csctx_v(ref ctx, STBTT.vmove, (int)ctx.x, (int)ctx.y, 0, 0, 0, 0);
   }

   static void stbtt__csctx_rline_to(ref stbtt__csctx ctx, float dx, float dy)
   {
      ctx.x += dx;
      ctx.y += dy;
      stbtt__csctx_v(ref ctx, STBTT.vline, (int)ctx.x, (int)ctx.y, 0, 0, 0, 0);
   }

   static void stbtt__csctx_rccurve_to(ref stbtt__csctx ctx, float dx1, float dy1, float dx2, float dy2, float dx3, float dy3)
   {
      float cx1 = ctx.x + dx1;
      float cy1 = ctx.y + dy1;
      float cx2 = cx1 + dx2;
      float cy2 = cy1 + dy2;
      ctx.x = cx2 + dx3;
      ctx.y = cy2 + dy3;
      stbtt__csctx_v(ref ctx, STBTT.vcubic, (int)ctx.x, (int)ctx.y, (int)cx1, (int)cy1, (int)cx2, (int)cy2);
   }

   static stbtt__buf stbtt__get_subr(stbtt__buf idx, int n)
   {
      int count = stbtt__cff_index_count(ref idx);
      int bias = 107;
      if (count >= 33900)
         bias = 32768;
      else if (count >= 1240)
         bias = 1131;
      n += bias;
      if (n < 0 || n >= count)
         return stbtt__new_buf(BytePtr.Null, 0);
      return stbtt__cff_index_get(idx, n);
   }

   static stbtt__buf stbtt__cid_get_glyph_subrs(ref stbtt_fontinfo info, int glyph_index)
   {
      stbtt__buf fdselect = info.fdselect;
      int nranges, start, end, v, fmt, fdselector = -1, i;

      stbtt__buf_seek(ref fdselect, 0);
      fmt = stbtt__buf_get8(ref fdselect);
      if (fmt == 0)
      {
         // untested
         stbtt__buf_skip(ref fdselect, glyph_index);
         fdselector = stbtt__buf_get8(ref fdselect);
      }
      else if (fmt == 3)
      {
         nranges = stbtt__buf_get16(ref fdselect);
         start = stbtt__buf_get16(ref fdselect);
         for (i = 0; i < nranges; i++)
         {
            v = stbtt__buf_get8(ref fdselect);
            end = stbtt__buf_get16(ref fdselect);
            if (glyph_index >= start && glyph_index < end)
            {
               fdselector = v;
               break;
            }
            start = end;
         }
      }
      if (fdselector == -1) stbtt__new_buf(BytePtr.Null, 0);
      return stbtt__get_subrs(info.cff, stbtt__cff_index_get(info.fontdicts, fdselector));
   }

   static bool stbtt__run_charstring(ref stbtt_fontinfo info, int glyph_index, ref stbtt__csctx c)
   {
      int maskbits = 0, subr_stack_height = 0, sp = 0, v, i, b0;
      bool in_header = true;
      bool has_subrs = false, clear_stack;
      Span<float> s = stackalloc float[48];
      Span<stbtt__buf> subr_stack = new stbtt__buf[10];
      stbtt__buf subrs = info.subrs;
      stbtt__buf b;
      float f;

      bool STBTT__CSERR(string s)
      {
         Console.Error.WriteLine(s);
         return false;
      }

      // this currently ignores the initial width value, which isn't needed if we have hmtx
      b = stbtt__cff_index_get(info.charstrings, glyph_index);
      while (b.cursor < b.size)
      {
         i = 0;
         clear_stack = true;
         b0 = stbtt__buf_get8(ref b);
         switch (b0)
         {
            // @TODO implement hinting
            case 0x13: // hintmask
            case 0x14: // cntrmask
               if (in_header)
                  maskbits += (sp / 2); // implicit "vstem"
               in_header = false;
               stbtt__buf_skip(ref b, (maskbits + 7) / 8);
               break;

            case 0x01: // hstem
            case 0x03: // vstem
            case 0x12: // hstemhm
            case 0x17: // vstemhm
               maskbits += (sp / 2);
               break;

            case 0x15: // rmoveto
               in_header = false;
               if (sp < 2) return STBTT__CSERR("rmoveto stack");
               stbtt__csctx_rmove_to(ref c, s[sp - 2], s[sp - 1]);
               break;
            case 0x04: // vmoveto
               in_header = false;
               if (sp < 1) return STBTT__CSERR("vmoveto stack");
               stbtt__csctx_rmove_to(ref c, 0, s[sp - 1]);
               break;
            case 0x16: // hmoveto
               in_header = false;
               if (sp < 1) return STBTT__CSERR("hmoveto stack");
               stbtt__csctx_rmove_to(ref c, s[sp - 1], 0);
               break;

            case 0x05: // rlineto
               if (sp < 2) return STBTT__CSERR("rlineto stack");
               for (; i + 1 < sp; i += 2)
                  stbtt__csctx_rline_to(ref c, s[i], s[i + 1]);
               break;

            // hlineto/vlineto and vhcurveto/hvcurveto alternate horizontal and vertical
            // starting from a different place.

            case 0x07: // vlineto
               if (sp < 1) return STBTT__CSERR("vlineto stack");
               if (i < sp)
               {
                  stbtt__csctx_rline_to(ref c, 0, s[i]);
                  i++;
               }
               for (; ; )
               {
                  if (i >= sp) break;
                  stbtt__csctx_rline_to(ref c, s[i], 0);
                  i++;
                  if (i >= sp) break;
                  stbtt__csctx_rline_to(ref c, 0, s[i]);
                  i++;
               }
               break;
            case 0x06: // hlineto
               if (sp < 1) return STBTT__CSERR("hlineto stack");
               for (; ; )
               {
                  if (i >= sp) break;
                  stbtt__csctx_rline_to(ref c, s[i], 0);
                  i++;
                  if (i >= sp) break;
                  stbtt__csctx_rline_to(ref c, 0, s[i]);
                  i++;
               }
               break;

            case 0x1F: // hvcurveto
               if (sp < 4) return STBTT__CSERR("hvcurveto stack");
               if (i + 3 < sp)
               {
                  stbtt__csctx_rccurve_to(ref c, s[i], 0, s[i + 1], s[i + 2], (sp - i == 5) ? s[i + 4] : 0.0f, s[i + 3]);
                  i += 4;
               }
               for (; ; )
               {
                  if (i + 3 >= sp) break;
                  stbtt__csctx_rccurve_to(ref c, 0, s[i], s[i + 1], s[i + 2], s[i + 3], (sp - i == 5) ? s[i + 4] : 0.0f);
                  i += 4;
                  if (i + 3 >= sp) break;
                  stbtt__csctx_rccurve_to(ref c, s[i], 0, s[i + 1], s[i + 2], (sp - i == 5) ? s[i + 4] : 0.0f, s[i + 3]);
                  i += 4;
               }
               break;

            case 0x1E: // vhcurveto
               if (sp < 4) return STBTT__CSERR("vhcurveto stack");
               for (; ; )
               {
                  if (i + 3 >= sp) break;
                  stbtt__csctx_rccurve_to(ref c, 0, s[i], s[i + 1], s[i + 2], s[i + 3], (sp - i == 5) ? s[i + 4] : 0.0f);
                  i += 4;
                  if (i + 3 >= sp) break;
                  stbtt__csctx_rccurve_to(ref c, s[i], 0, s[i + 1], s[i + 2], (sp - i == 5) ? s[i + 4] : 0.0f, s[i + 3]);
                  i += 4;
               }
               break;

            case 0x08: // rrcurveto
               if (sp < 6) return STBTT__CSERR("rcurveline stack");
               for (; i + 5 < sp; i += 6)
                  stbtt__csctx_rccurve_to(ref c, s[i], s[i + 1], s[i + 2], s[i + 3], s[i + 4], s[i + 5]);
               break;

            case 0x18: // rcurveline
               if (sp < 8) return STBTT__CSERR("rcurveline stack");
               for (; i + 5 < sp - 2; i += 6)
                  stbtt__csctx_rccurve_to(ref c, s[i], s[i + 1], s[i + 2], s[i + 3], s[i + 4], s[i + 5]);
               if (i + 1 >= sp) return STBTT__CSERR("rcurveline stack");
               stbtt__csctx_rline_to(ref c, s[i], s[i + 1]);
               break;

            case 0x19: // rlinecurve
               if (sp < 8) return STBTT__CSERR("rlinecurve stack");
               for (; i + 1 < sp - 6; i += 2)
                  stbtt__csctx_rline_to(ref c, s[i], s[i + 1]);
               if (i + 5 >= sp) return STBTT__CSERR("rlinecurve stack");
               stbtt__csctx_rccurve_to(ref c, s[i], s[i + 1], s[i + 2], s[i + 3], s[i + 4], s[i + 5]);
               break;

            case 0x1A: // vvcurveto
            case 0x1B: // hhcurveto
               if (sp < 4) return STBTT__CSERR("(vv|hh)curveto stack");
               f = 0.0f;
               if ((sp & 1) != 0) { f = s[i]; i++; }
               for (; i + 3 < sp; i += 4)
               {
                  if (b0 == 0x1B)
                     stbtt__csctx_rccurve_to(ref c, s[i], f, s[i + 1], s[i + 2], s[i + 3], 0.0f);
                  else
                     stbtt__csctx_rccurve_to(ref c, f, s[i], s[i + 1], s[i + 2], 0.0f, s[i + 3]);
                  f = 0.0f;
               }
               break;

            case 0x0A: // callsubr
               if (!has_subrs)
               {
                  if (info.fdselect.size != 0)
                     subrs = stbtt__cid_get_glyph_subrs(ref info, glyph_index);
                  has_subrs = true;
               }
               goto case 0x1D;
            // FALLTHROUGH
            case 0x1D: // callgsubr
               if (sp < 1) return STBTT__CSERR("call(g|)subr stack");
               v = (int)s[--sp];
               if (subr_stack_height >= 10) return STBTT__CSERR("recursion limit");
               subr_stack[subr_stack_height++] = b;
               b = stbtt__get_subr(b0 == 0x0A ? subrs : info.gsubrs, v);
               if (b.size == 0) return STBTT__CSERR("subr not found");
               b.cursor = 0;
               clear_stack = false;
               break;

            case 0x0B: // return
               if (subr_stack_height <= 0) return STBTT__CSERR("return outside subr");
               b = subr_stack[--subr_stack_height];
               clear_stack = false;
               break;

            case 0x0E: // endchar
               stbtt__csctx_close_shape(ref c);
               return true;

            case 0x0C:
               { // two-byte escape
                  float dx1, dx2, dx3, dx4, dx5, dx6, dy1, dy2, dy3, dy4, dy5, dy6;
                  float dx, dy;
                  int b1 = stbtt__buf_get8(ref b);
                  switch (b1)
                  {
                     // @TODO These "flex" implementations ignore the flex-depth and resolution,
                     // and always draw beziers.
                     case 0x22: // hflex
                        if (sp < 7) return STBTT__CSERR("hflex stack");
                        dx1 = s[0];
                        dx2 = s[1];
                        dy2 = s[2];
                        dx3 = s[3];
                        dx4 = s[4];
                        dx5 = s[5];
                        dx6 = s[6];
                        stbtt__csctx_rccurve_to(ref c, dx1, 0, dx2, dy2, dx3, 0);
                        stbtt__csctx_rccurve_to(ref c, dx4, 0, dx5, -dy2, dx6, 0);
                        break;

                     case 0x23: // flex
                        if (sp < 13) return STBTT__CSERR("flex stack");
                        dx1 = s[0];
                        dy1 = s[1];
                        dx2 = s[2];
                        dy2 = s[3];
                        dx3 = s[4];
                        dy3 = s[5];
                        dx4 = s[6];
                        dy4 = s[7];
                        dx5 = s[8];
                        dy5 = s[9];
                        dx6 = s[10];
                        dy6 = s[11];
                        //fd is s[12]
                        stbtt__csctx_rccurve_to(ref c, dx1, dy1, dx2, dy2, dx3, dy3);
                        stbtt__csctx_rccurve_to(ref c, dx4, dy4, dx5, dy5, dx6, dy6);
                        break;

                     case 0x24: // hflex1
                        if (sp < 9) return STBTT__CSERR("hflex1 stack");
                        dx1 = s[0];
                        dy1 = s[1];
                        dx2 = s[2];
                        dy2 = s[3];
                        dx3 = s[4];
                        dx4 = s[5];
                        dx5 = s[6];
                        dy5 = s[7];
                        dx6 = s[8];
                        stbtt__csctx_rccurve_to(ref c, dx1, dy1, dx2, dy2, dx3, 0);
                        stbtt__csctx_rccurve_to(ref c, dx4, 0, dx5, dy5, dx6, -(dy1 + dy2 + dy5));
                        break;

                     case 0x25: // flex1
                        if (sp < 11) return STBTT__CSERR("flex1 stack");
                        dx1 = s[0];
                        dy1 = s[1];
                        dx2 = s[2];
                        dy2 = s[3];
                        dx3 = s[4];
                        dy3 = s[5];
                        dx4 = s[6];
                        dy4 = s[7];
                        dx5 = s[8];
                        dy5 = s[9];
                        dx6 = dy6 = s[10];
                        dx = dx1 + dx2 + dx3 + dx4 + dx5;
                        dy = dy1 + dy2 + dy3 + dy4 + dy5;
                        if (STBTT_fabs(dx) > STBTT_fabs(dy))
                           dy6 = -dy;
                        else
                           dx6 = -dx;
                        stbtt__csctx_rccurve_to(ref c, dx1, dy1, dx2, dy2, dx3, dy3);
                        stbtt__csctx_rccurve_to(ref c, dx4, dy4, dx5, dy5, dx6, dy6);
                        break;

                     default:
                        return STBTT__CSERR("unimplemented");
                  }
               }
               break;

            default:
               if (b0 != 255 && b0 != 28 && b0 < 32)
                  return STBTT__CSERR("reserved operator");

               // push immediate
               if (b0 == 255)
               {
                  f = (float)(stbtt_int32)stbtt__buf_get32(ref b) / 0x10000;
               }
               else
               {
                  stbtt__buf_skip(ref b, -1);
                  f = (float)(stbtt_int16)stbtt__cff_int(ref b);
               }
               if (sp >= 48) return STBTT__CSERR("push stack overflow");
               s[sp++] = f;
               clear_stack = false;
               break;
         }
         if (clear_stack) sp = 0;
      }
      return STBTT__CSERR("no endchar");
   }

   static int stbtt__GetGlyphShapeT2(ref stbtt_fontinfo info, int glyph_index, out stbtt_vertex[] pvertices)
   {
      // runs the charstring twice, once to count and once to output (to avoid realloc)
      stbtt__csctx count_ctx = STBTT__CSCTX_INIT(1);
      stbtt__csctx output_ctx = STBTT__CSCTX_INIT(0);
      if (stbtt__run_charstring(ref info, glyph_index, ref count_ctx))
      {
         pvertices = new stbtt_vertex[count_ctx.num_vertices];
         output_ctx.pvertices = pvertices;
         if (stbtt__run_charstring(ref info, glyph_index, ref output_ctx))
         {
            STBTT_assert(output_ctx.num_vertices == count_ctx.num_vertices);
            return output_ctx.num_vertices;
         }
      }
      pvertices = [];
      return 0;
   }






   static int stbtt__GetGlyphKernInfoAdvance(ref stbtt_fontinfo info, int glyph1, int glyph2)
   {
      BytePtr data = info.data + info.kern;
      stbtt_uint32 needle, straw;
      int l, r, m;

      // we only look at the first table. it must be 'horizontal' and format 0.
      if (info.kern == 0)
         return 0;
      if (ttUSHORT(data + 2) < 1) // number of tables, need at least 1
         return 0;
      if (ttUSHORT(data + 8) != 1) // horizontal flag must be set in format
         return 0;

      l = 0;
      r = ttUSHORT(data + 10) - 1;
      needle = (uint)(glyph1 << 16 | glyph2);
      while (l <= r)
      {
         m = (l + r) >> 1;
         straw = ttULONG(data + 18 + (m * 6)); // note: unaligned read
         if (needle < straw)
            r = m - 1;
         else if (needle > straw)
            l = m + 1;
         else
            return ttSHORT(data + 22 + (m * 6));
      }
      return 0;
   }

   static stbtt_int32 stbtt__GetCoverageIndex(BytePtr coverageTable, int glyph)
   {
      stbtt_uint16 coverageFormat = ttUSHORT(coverageTable);
      switch (coverageFormat)
      {
         case 1:
            {
               stbtt_uint16 glyphCount = ttUSHORT(coverageTable + 2);

               // Binary search.
               stbtt_int32 l = 0, r = glyphCount - 1, m;
               int straw, needle = glyph;
               while (l <= r)
               {
                  BytePtr glyphArray = coverageTable + 4;
                  stbtt_uint16 glyphID;
                  m = (l + r) >> 1;
                  glyphID = ttUSHORT(glyphArray + 2 * m);
                  straw = glyphID;
                  if (needle < straw)
                     r = m - 1;
                  else if (needle > straw)
                     l = m + 1;
                  else
                  {
                     return m;
                  }
               }
               break;
            }

         case 2:
            {
               stbtt_uint16 rangeCount = ttUSHORT(coverageTable + 2);
               BytePtr rangeArray = coverageTable + 4;

               // Binary search.
               stbtt_int32 l = 0, r = rangeCount - 1, m;
               int strawStart, strawEnd, needle = glyph;
               while (l <= r)
               {
                  BytePtr rangeRecord;
                  m = (l + r) >> 1;
                  rangeRecord = rangeArray + 6 * m;
                  strawStart = ttUSHORT(rangeRecord);
                  strawEnd = ttUSHORT(rangeRecord + 2);
                  if (needle < strawStart)
                     r = m - 1;
                  else if (needle > strawEnd)
                     l = m + 1;
                  else
                  {
                     stbtt_uint16 startCoverageIndex = ttUSHORT(rangeRecord + 4);
                     return startCoverageIndex + glyph - strawStart;
                  }
               }
               break;
            }

         default: return -1; // unsupported
      }

      return -1;
   }

   static stbtt_int32 stbtt__GetGlyphClass(BytePtr classDefTable, int glyph)
   {
      stbtt_uint16 classDefFormat = ttUSHORT(classDefTable);
      switch (classDefFormat)
      {
         case 1:
            {
               stbtt_uint16 startGlyphID = ttUSHORT(classDefTable + 2);
               stbtt_uint16 glyphCount = ttUSHORT(classDefTable + 4);
               BytePtr classDef1ValueArray = classDefTable + 6;

               if (glyph >= startGlyphID && glyph < startGlyphID + glyphCount)
                  return (stbtt_int32)ttUSHORT(classDef1ValueArray + 2 * (glyph - startGlyphID));
               break;
            }

         case 2:
            {
               stbtt_uint16 classRangeCount = ttUSHORT(classDefTable + 2);
               BytePtr classRangeRecords = classDefTable + 4;

               // Binary search.
               stbtt_int32 l = 0, r = classRangeCount - 1, m;
               int strawStart, strawEnd, needle = glyph;
               while (l <= r)
               {
                  BytePtr classRangeRecord;
                  m = (l + r) >> 1;
                  classRangeRecord = classRangeRecords + 6 * m;
                  strawStart = ttUSHORT(classRangeRecord);
                  strawEnd = ttUSHORT(classRangeRecord + 2);
                  if (needle < strawStart)
                     r = m - 1;
                  else if (needle > strawEnd)
                     l = m + 1;
                  else
                     return (stbtt_int32)ttUSHORT(classRangeRecord + 4);
               }
               break;
            }

         default:
            return -1; // Unsupported definition type, return an error.
      }

      // "All glyphs not assigned to a class fall into class 0". (OpenType spec)
      return 0;
   }

   // Define to STBTT_assert(x) if you want to break on unimplemented formats.
   // #define STBTT_GPOS_TODO_assert(x)

   static stbtt_int32 stbtt__GetGlyphGPOSInfoAdvance(ref stbtt_fontinfo info, int glyph1, int glyph2)
   {
      stbtt_uint16 lookupListOffset;
      BytePtr lookupList;
      stbtt_uint16 lookupCount;
      BytePtr data;
      stbtt_int32 i, sti;

      if (info.gpos == 0) return 0;

      data = info.data + info.gpos;

      if (ttUSHORT(data + 0) != 1) return 0; // Major version 1
      if (ttUSHORT(data + 2) != 0) return 0; // Minor version 0

      lookupListOffset = ttUSHORT(data + 8);
      lookupList = data + lookupListOffset;
      lookupCount = ttUSHORT(lookupList);

      for (i = 0; i < lookupCount; ++i)
      {
         stbtt_uint16 lookupOffset = ttUSHORT(lookupList + 2 + 2 * i);
         BytePtr lookupTable = lookupList + lookupOffset;

         stbtt_uint16 lookupType = ttUSHORT(lookupTable);
         stbtt_uint16 subTableCount = ttUSHORT(lookupTable + 4);
         BytePtr subTableOffsets = lookupTable + 6;
         if (lookupType != 2) // Pair Adjustment Positioning Subtable
            continue;

         for (sti = 0; sti < subTableCount; sti++)
         {
            stbtt_uint16 subtableOffset = ttUSHORT(subTableOffsets + 2 * sti);
            BytePtr table = lookupTable + subtableOffset;
            stbtt_uint16 posFormat = ttUSHORT(table);
            stbtt_uint16 coverageOffset = ttUSHORT(table + 2);
            stbtt_int32 coverageIndex = stbtt__GetCoverageIndex(table + coverageOffset, glyph1);
            if (coverageIndex == -1) continue;

            switch (posFormat)
            {
               case 1:
                  {
                     stbtt_int32 l, r, m;
                     int straw, needle;
                     stbtt_uint16 valueFormat1 = ttUSHORT(table + 4);
                     stbtt_uint16 valueFormat2 = ttUSHORT(table + 6);
                     if (valueFormat1 == 4 && valueFormat2 == 0)
                     { // Support more formats?
                        stbtt_int32 valueRecordPairSizeInBytes = 2;
                        stbtt_uint16 pairSetCount = ttUSHORT(table + 8);
                        stbtt_uint16 pairPosOffset = ttUSHORT(table + 10 + 2 * coverageIndex);
                        BytePtr pairValueTable = table + pairPosOffset;
                        stbtt_uint16 pairValueCount = ttUSHORT(pairValueTable);
                        BytePtr pairValueArray = pairValueTable + 2;

                        if (coverageIndex >= pairSetCount) return 0;

                        needle = glyph2;
                        r = pairValueCount - 1;
                        l = 0;

                        // Binary search.
                        while (l <= r)
                        {
                           stbtt_uint16 secondGlyph;
                           BytePtr pairValue;
                           m = (l + r) >> 1;
                           pairValue = pairValueArray + (2 + valueRecordPairSizeInBytes) * m;
                           secondGlyph = ttUSHORT(pairValue);
                           straw = secondGlyph;
                           if (needle < straw)
                              r = m - 1;
                           else if (needle > straw)
                              l = m + 1;
                           else
                           {
                              stbtt_int16 xAdvance = ttSHORT(pairValue + 2);
                              return xAdvance;
                           }
                        }
                     }
                     else
                        return 0;
                     break;
                  }

               case 2:
                  {
                     stbtt_uint16 valueFormat1 = ttUSHORT(table + 4);
                     stbtt_uint16 valueFormat2 = ttUSHORT(table + 6);
                     if (valueFormat1 == 4 && valueFormat2 == 0)
                     { // Support more formats?
                        stbtt_uint16 classDef1Offset = ttUSHORT(table + 8);
                        stbtt_uint16 classDef2Offset = ttUSHORT(table + 10);
                        int glyph1class = stbtt__GetGlyphClass(table + classDef1Offset, glyph1);
                        int glyph2class = stbtt__GetGlyphClass(table + classDef2Offset, glyph2);

                        stbtt_uint16 class1Count = ttUSHORT(table + 12);
                        stbtt_uint16 class2Count = ttUSHORT(table + 14);
                        BytePtr class1Records, class2Records;
                        stbtt_int16 xAdvance;

                        if (glyph1class < 0 || glyph1class >= class1Count) return 0; // malformed
                        if (glyph2class < 0 || glyph2class >= class2Count) return 0; // malformed

                        class1Records = table + 16;
                        class2Records = class1Records + 2 * (glyph1class * class2Count);
                        xAdvance = ttSHORT(class2Records + 2 * glyph2class);
                        return xAdvance;
                     }
                     else
                        return 0;
                  }

               default:
                  return 0; // Unsupported position format
            }
         }
      }

      return 0;
   }




   //////////////////////////////////////////////////////////////////////////////
   //
   // antialiasing software rasterizer
   //

   //////////////////////////////////////////////////////////////////////////////
   //
   //  Rasterizer

   struct stbtt__edge
   {
      public float x0, y0, x1, y1;
      public bool invert;
   };



   struct stbtt__active_edge_collection
   {
      public stbtt__active_edge[] active_edges;

      public int active_edges_count;

      public int[] active_indices;
      public int active_indices_count;
   }

   static void stbtt__init_active_collection(out stbtt__active_edge_collection collection, int maxActiveEdgesCount)
   {
      collection = new();

      collection.active_edges = new stbtt__active_edge[maxActiveEdgesCount];
      collection.active_indices_count = 0;

      collection.active_indices = new int[maxActiveEdgesCount];
      collection.active_indices_count = 0;
   }

   static void stbtt__free_active_collection(ref stbtt__active_edge_collection collection)
   {
      //
   }

   static void stbtt__remove_from_active_collection(ref stbtt__active_edge_collection collection, int active_index)
   {
      if (collection.active_indices_count == 1)
      {
         collection.active_indices_count = 0;
         return;
      }

      collection.active_indices[active_index] = collection.active_indices[collection.active_indices_count - 1];
      collection.active_indices_count--;
   }

   struct stbtt__active_edge
   {
#if STBTT_RASTERIZER_VERSION_1
      public int x, dx;
      public float ey;
      public int direction;
#elif STBTT_RASTERIZER_VERSION_2
      public float fx, fdx, fdy;
      public float direction;
      public float sy;
      public float ey;
#else
#error "Unrecognized value of STBTT_RASTERIZER_VERSION"
#endif
   }

#if STBTT_RASTERIZER_VERSION_1
   const int STBTT_FIXSHIFT = 10;
   const int STBTT_FIX = (1 << STBTT_FIXSHIFT);
   const int STBTT_FIXMASK = (STBTT_FIX - 1);

   static int stbtt__new_active(ref stbtt__active_edge_collection coll, ref stbtt__edge e, int off_x, float start_point)
   {
      STBTT_assert(coll.active_edges_count + 1 < coll.active_edges.Length);
      STBTT_assert(coll.active_indices_count + 1 < coll.active_indices.Length);

      stbtt__active_edge active_edge = new ();
      float dxdy = (e.x1 - e.x0) / (e.y1 - e.y0);
      // STBTT_assert(z != NULL);
      // if (!z) return z;

      // round dx down to avoid overshooting
      if (dxdy < 0)
         active_edge.dx = -STBTT_ifloor(STBTT_FIX * -dxdy);
      else
         active_edge.dx = STBTT_ifloor(STBTT_FIX * dxdy);

      active_edge.x = STBTT_ifloor(STBTT_FIX * e.x0 + active_edge.dx * (start_point - e.y0)); // use z.dx so when we offset later it's by the same amount
      active_edge.x -= off_x * STBTT_FIX;

      active_edge.ey = e.y1;
      active_edge.direction = e.invert ? 1 : -1;

      int index = coll.active_edges_count;

      coll.active_edges[index] = active_edge;
      coll.active_edges_count++;


      // find insertion point
      int insert_index;

      if (coll.active_indices_count == 0)
      {
         // first element
         insert_index = 0;
      }
      else if (active_edge.x < coll.active_edges[coll.active_indices[0]].x)
      {
         // insert at front
         insert_index = 0;
      }
      else // TODO: Maybe we can validate against the last element too
      {
         
         // find thing to insert AFTER
         insert_index = 0;
         while(insert_index < coll.active_indices_count &&
               coll.active_edges[coll.active_indices[insert_index]].x < active_edge.x)
         {
            insert_index++;
         }
      }
      if (insert_index < coll.active_indices_count)
      {
         // Displace items below
         Array.Copy(coll.active_indices, insert_index, coll.active_indices, insert_index + 1, coll.active_indices_count - insert_index);
      }
      coll.active_indices[insert_index] = index;
      coll.active_indices_count++;

      return index;
   }

#elif STBTT_RASTERIZER_VERSION_2
   static int stbtt__new_active(ref stbtt__active_edge_collection coll, ref stbtt__edge e, int off_x, float start_point)
   {
      STBTT_assert(coll.active_edges_count + 1 < coll.active_edges.Length);
      STBTT_assert(coll.active_indices_count + 1 < coll.active_indices.Length);

      float dxdy = (e.x1 - e.x0) / (e.y1 - e.y0);
      //STBTT_assert(e.y0 <= start_point);
      //if (!z) return z;
      stbtt__active_edge active_edge = new();
      active_edge.fdx = dxdy;
      active_edge.fdy = dxdy != 0.0f ? (1.0f / dxdy) : 0.0f;
      active_edge.fx = e.x0 + dxdy * (start_point - e.y0);
      active_edge.fx -= off_x;
      active_edge.direction = e.invert ? 1.0f : -1.0f;
      active_edge.sy = e.y0;
      active_edge.ey = e.y1;

      int index = coll.active_edges_count;

      coll.active_edges[index] = active_edge;
      coll.active_edges_count++;

      coll.active_indices[coll.active_indices_count] = index;
      coll.active_indices_count++;

      return index;
   }
#else
#error "Unrecognized value of STBTT_RASTERIZER_VERSION"
#endif

#if STBTT_RASTERIZER_VERSION_1
   // note: this routine clips fills that extend off the edges... ideally this
   // wouldn't happen, but it could happen if the truetype glyph bounding boxes
   // are wrong, or if the user supplies a too-small bitmap
   static void stbtt__fill_active_edges(Span<byte> scanline, int len, ref stbtt__active_edge_collection hh, int max_weight)
   {
      // non-zero winding fill
      int x0 = 0, w = 0;

      for (int idx = 0; idx < hh.active_edges_count; idx++)
      {
         ref var e = ref hh.active_edges[hh.active_indices[idx]];
         if (w == 0)
         {
            // if we're currently at zero, we need to record the edge start point
            x0 = e.x; w += e.direction;
         }
         else
         {
            int x1 = e.x; w += e.direction;
            // if we went to zero, we need to draw
            if (w == 0)
            {
               int i = x0 >> STBTT_FIXSHIFT;
               int j = x1 >> STBTT_FIXSHIFT;

               if (i < len && j >= 0)
               {
                  if (i == j)
                  {
                     // x0,x1 are the same pixel, so compute combined coverage
                     scanline[i] = (byte)(scanline[i] + (stbtt_uint8)((x1 - x0) * max_weight >> STBTT_FIXSHIFT));
                  }
                  else
                  {
                     if (i >= 0) // add antialiasing for x0
                        scanline[i] = (byte)(scanline[i] + (stbtt_uint8)(((STBTT_FIX - (x0 & STBTT_FIXMASK)) * max_weight) >> STBTT_FIXSHIFT));
                     else
                        i = -1; // clip

                     if (j < len) // add antialiasing for x1
                        scanline[j] = (byte)(scanline[j] + (stbtt_uint8)(((x1 & STBTT_FIXMASK) * max_weight) >> STBTT_FIXSHIFT));
                     else
                        j = len; // clip

                     for (++i; i < j; ++i) // fill pixels between x0 and x1
                        scanline[i] = (byte)(scanline[i] + (stbtt_uint8)max_weight);
                  }
               }
            }
         }
      }
   }

   static void stbtt__rasterize_sorted_edges(ref stbtt__bitmap result, stbtt__edge[] e, int n, int vsubsample, int off_x, int off_y)
   {
      stbtt__active_edge_collection hh;
      stbtt__init_active_collection(out hh, e.Length);

      int y, j = 0;
      int max_weight = (255 / vsubsample);  // weight per vertical scanline
      int s; // vertical subsample index
      Span<byte> scanline = result.w > 512 ? new byte[result.w] : stackalloc byte[512];

      y = off_y * vsubsample;
      e[n].y0 = (off_y + result.h) * (float)vsubsample + 1;

      int edgeIndex = 0;

      while (j < result.h)
      {
         scanline.Clear();

         for (s = 0; s < vsubsample; ++s)
         {
            // find center of pixel for this scanline
            float scan_y = y + 0.5f;
            
            // update all active edges;
            // remove all active edges that terminate before the center of this scanline
            for (var idx = hh.active_indices_count - 1; idx >= 0; idx--)
            {
               ref var z = ref hh.active_edges[hh.active_indices[idx]];
               
               if (z.ey <= scan_y)
               {
                  stbtt__remove_from_active_collection(ref hh, idx);
                  STBTT_assert(z.direction != 0);
                  z.direction = 0;
               }
            }

            // resort the list if needed
            for (; ; )
            {
               bool changed = false;
               
               for (var idx = 0; idx < hh.active_indices_count - 1; idx++)
               {
                  ref var t = ref hh.active_edges[hh.active_indices[idx]];
                  ref var q = ref hh.active_edges[hh.active_indices[idx + 1]];

                  if (t.x > q.x)
                  {
                     var tmp = hh.active_indices[idx];
                     hh.active_indices[idx] = hh.active_indices[idx + 1];
                     hh.active_indices[idx + 1] = tmp;
                     changed = true;
                  }
               }

               if (!changed) break;
            }

            // insert all edges that start before the center of this scanline -- omit ones that also end on this scanline
            while (e[edgeIndex].y0 <= scan_y)
            {
               if (e[edgeIndex].y1 > scan_y)
               {
                  stbtt__new_active(ref hh, ref e[edgeIndex], off_x, scan_y);
               }
               edgeIndex++;
            }

            // now process all active edges in XOR fashion
            if (hh.active_indices_count > 0)
               stbtt__fill_active_edges(scanline, result.w, ref hh, max_weight);

            ++y;
         }

         for (var c = 0; c < result.w; c++)
            result.pixels.GetRef(j * result.stride + c) = scanline[c];
            
         ++j;
      }

      stbtt__free_active_collection(ref hh);

      //if (scanline != scanline_data)
      //   STBTT_free(scanline, userdata);
   }

#elif STBTT_RASTERIZER_VERSION_2

   // the edge passed in here does not cross the vertical line at x or the vertical line at x+1
   // (i.e. it has already been clipped to those)
   static void stbtt__handle_clipped_edge(Span<float> scanline, int x, ref stbtt__active_edge e, float x0, float y0, float x1, float y1)
   {
      if (y0 == y1) return;
      STBTT_assert(y0 < y1);
      STBTT_assert(e.sy <= e.ey);
      if (y0 > e.ey) return;
      if (y1 < e.sy) return;
      if (y0 < e.sy)
      {
         x0 += (x1 - x0) * (e.sy - y0) / (y1 - y0);
         y0 = e.sy;
      }
      if (y1 > e.ey)
      {
         x1 += (x1 - x0) * (e.ey - y1) / (y1 - y0);
         y1 = e.ey;
      }

      if (x0 == x)
         STBTT_assert(x1 <= x + 1);
      else if (x0 == x + 1)
         STBTT_assert(x1 >= x);
      else if (x0 <= x)
         STBTT_assert(x1 <= x);
      else if (x0 >= x + 1)
         STBTT_assert(x1 >= x + 1);
      else
         STBTT_assert(x1 >= x && x1 <= x + 1);

      if (x0 <= x && x1 <= x)
      {
         scanline[x] += e.direction * (y1 - y0);
      }
      else if (x0 >= x + 1 && x1 >= x + 1)
      {

      }
      else
      {
         STBTT_assert(x0 >= x && x0 <= x + 1 && x1 >= x && x1 <= x + 1);
         scanline[x] += e.direction * (y1 - y0) * (1 - ((x0 - x) + (x1 - x)) / 2); // coverage = 1 - average x position
      }
   }

   static float stbtt__sized_trapezoid_area(float height, float top_width, float bottom_width)
   {
      STBTT_assert(top_width >= 0);
      STBTT_assert(bottom_width >= 0);
      return (top_width + bottom_width) / 2.0f * height;
   }

   static float stbtt__position_trapezoid_area(float height, float tx0, float tx1, float bx0, float bx1)
   {
      return stbtt__sized_trapezoid_area(height, tx1 - tx0, bx1 - bx0);
   }

   static float stbtt__sized_triangle_area(float height, float width)
   {
      return height * width / 2;
   }

   static void stbtt__fill_active_edges_new(Span<float> scanline, Span<float> scanline_fill, Span<float> scanline_fill_minus_one, int len, ref stbtt__active_edge_collection hh, float y_top)
   {
      float y_bottom = y_top + 1;

      for (int i = 0; i < hh.active_indices_count; i++)
      {
         // brute force every pixel
         ref var e = ref hh.active_edges[hh.active_indices[i]];

         // compute intersection points with top & bottom
         STBTT_assert(e.ey >= y_top);

         if (e.fdx == 0)
         {
            float x0 = e.fx;
            if (x0 < len)
            {
               if (x0 >= 0)
               {
                  stbtt__handle_clipped_edge(scanline, (int)x0, ref e, x0, y_top, x0, y_bottom);
                  stbtt__handle_clipped_edge(scanline_fill_minus_one, (int)x0 + 1, ref e, x0, y_top, x0, y_bottom);
               }
               else
               {
                  stbtt__handle_clipped_edge(scanline_fill_minus_one, 0, ref e, x0, y_top, x0, y_bottom);
               }
            }
         }
         else
         {
            float x0 = e.fx;
            float dx = e.fdx;
            float xb = x0 + dx;
            float x_top, x_bottom;
            float sy0, sy1;
            float dy = e.fdy;
            STBTT_assert(e.sy <= y_bottom && e.ey >= y_top);

            // compute endpoints of line segment clipped to this scanline (if the
            // line segment starts on this scanline. x0 is the intersection of the
            // line with y_top, but that may be off the line segment.
            if (e.sy > y_top)
            {
               x_top = x0 + dx * (e.sy - y_top);
               sy0 = e.sy;
            }
            else
            {
               x_top = x0;
               sy0 = y_top;
            }
            if (e.ey < y_bottom)
            {
               x_bottom = x0 + dx * (e.ey - y_top);
               sy1 = e.ey;
            }
            else
            {
               x_bottom = xb;
               sy1 = y_bottom;
            }

            if (x_top >= 0 && x_bottom >= 0 && x_top < len && x_bottom < len)
            {
               // from here on, we don't have to range check x values

               if ((int)x_top == (int)x_bottom)
               {
                  float height;
                  // simple case, only spans one pixel
                  int x = (int)x_top;
                  height = (sy1 - sy0) * e.direction;
                  STBTT_assert(x >= 0 && x < len);
                  scanline[x] += stbtt__position_trapezoid_area(height, x_top, x + 1.0f, x_bottom, x + 1.0f);
                  scanline_fill[x] += height; // everything right of this pixel is filled
               }
               else
               {
                  int x, x1, x2;
                  float y_crossing, y_final, step, sign, area;
                  // covers 2+ pixels
                  if (x_top > x_bottom)
                  {
                     // flip scanline vertically; signed area is the same
                     float t;
                     sy0 = y_bottom - (sy0 - y_top);
                     sy1 = y_bottom - (sy1 - y_top);
                     t = sy0; sy0 = sy1; sy1 = t;
                     t = x_bottom; x_bottom = x_top; x_top = t;
                     dx = -dx;
                     dy = -dy;
                     t = x0; x0 = xb; xb = t;
                  }
                  STBTT_assert(dy >= 0);
                  STBTT_assert(dx >= 0);

                  x1 = (int)x_top;
                  x2 = (int)x_bottom;
                  // compute intersection with y axis at x1+1
                  y_crossing = y_top + dy * (x1 + 1 - x0);

                  // compute intersection with y axis at x2
                  y_final = y_top + dy * (x2 - x0);

                  //           x1    x_top                            x2    x_bottom
                  //     y_top  +------|-----+------------+------------+--------|---+------------+
                  //            |            |            |            |            |            |
                  //            |            |            |            |            |            |
                  //       sy0  |      Txxxxx|............|............|............|............|
                  // y_crossing |            *xxxxx.......|............|............|............|
                  //            |            |     xxxxx..|............|............|............|
                  //            |            |     /-   xx*xxxx........|............|............|
                  //            |            | dy <       |    xxxxxx..|............|............|
                  //   y_final  |            |     \-     |          xx*xxx.........|............|
                  //       sy1  |            |            |            |   xxxxxB...|............|
                  //            |            |            |            |            |            |
                  //            |            |            |            |            |            |
                  //  y_bottom  +------------+------------+------------+------------+------------+
                  //
                  // goal is to measure the area covered by '.' in each pixel

                  // if x2 is right at the right edge of x1, y_crossing can blow up, github #1057
                  // @TODO: maybe test against sy1 rather than y_bottom?
                  if (y_crossing > y_bottom)
                     y_crossing = y_bottom;

                  sign = e.direction;

                  // area of the rectangle covered from sy0..y_crossing
                  area = sign * (y_crossing - sy0);

                  // area of the triangle (x_top,sy0), (x1+1,sy0), (x1+1,y_crossing)
                  scanline[x1] += stbtt__sized_triangle_area(area, x1 + 1 - x_top);

                  // check if final y_crossing is blown up; no test case for this
                  if (y_final > y_bottom)
                  {
                     y_final = y_bottom;
                     dy = (y_final - y_crossing) / (x2 - (x1 + 1)); // if denom=0, y_final = y_crossing, so y_final <= y_bottom
                  }

                  // in second pixel, area covered by line segment found in first pixel
                  // is always a rectangle 1 wide * the height of that line segment; this
                  // is exactly what the variable 'area' stores. it also gets a contribution
                  // from the line segment within it. the THIRD pixel will get the first
                  // pixel's rectangle contribution, the second pixel's rectangle contribution,
                  // and its own contribution. the 'own contribution' is the same in every pixel except
                  // the leftmost and rightmost, a trapezoid that slides down in each pixel.
                  // the second pixel's contribution to the third pixel will be the
                  // rectangle 1 wide times the height change in the second pixel, which is dy.

                  step = sign * dy * 1; // dy is dy/dx, change in y for every 1 change in x,
                                        // which multiplied by 1-pixel-width is how much pixel area changes for each step in x
                                        // so the area advances by 'step' every time

                  for (x = x1 + 1; x < x2; ++x)
                  {
                     scanline[x] += area + step / 2; // area of trapezoid is 1*step/2
                     area += step;
                  }
                  STBTT_assert(STBTT_fabs(area) <= 1.01f); // accumulated error from area += step unless we round step down
                  STBTT_assert(sy1 > y_final - 0.01f);

                  // area covered in the last pixel is the rectangle from all the pixels to the left,
                  // plus the trapezoid filled by the line segment in this pixel all the way to the right edge
                  scanline[x2] += area + sign * stbtt__position_trapezoid_area(sy1 - y_final, (float)x2, x2 + 1.0f, x_bottom, x2 + 1.0f);

                  // the rest of the line is filled based on the total height of the line segment in this pixel
                  scanline_fill[x2] += sign * (sy1 - sy0);
               }
            }
            else
            {
               // if edge goes outside of box we're drawing, we require
               // clipping logic. since this does not match the intended use
               // of this library, we use a different, very slow brute
               // force implementation
               // note though that this does happen some of the time because
               // x_top and x_bottom can be extrapolated at the top & bottom of
               // the shape and actually lie outside the bounding box
               int x;
               for (x = 0; x < len; ++x)
               {
                  // cases:
                  //
                  // there can be up to two intersections with the pixel. any intersection
                  // with left or right edges can be handled by splitting into two (or three)
                  // regions. intersections with top & bottom do not necessitate case-wise logic.
                  //
                  // the old way of doing this found the intersections with the left & right edges,
                  // then used some simple logic to produce up to three segments in sorted order
                  // from top-to-bottom. however, this had a problem: if an x edge was epsilon
                  // across the x border, then the corresponding y position might not be distinct
                  // from the other y segment, and it might ignored as an empty segment. to avoid
                  // that, we need to explicitly produce segments based on x positions.

                  // rename variables to clearly-defined pairs
                  float y0 = y_top;
                  float x1 = (float)(x);
                  float x2 = (float)(x + 1);
                  float x3 = xb;
                  float y3 = y_bottom;

                  // x = e.x + e.dx * (y-y_top)
                  // (y-y_top) = (x - e.x) / e.dx
                  // y = (x - e.x) / e.dx + y_top
                  float y1 = (x - x0) / dx + y_top;
                  float y2 = (x + 1 - x0) / dx + y_top;

                  if (x0 < x1 && x3 > x2)
                  {         // three segments descending down-right
                     stbtt__handle_clipped_edge(scanline, x, ref e, x0, y0, x1, y1);
                     stbtt__handle_clipped_edge(scanline, x, ref e, x1, y1, x2, y2);
                     stbtt__handle_clipped_edge(scanline, x, ref e, x2, y2, x3, y3);
                  }
                  else if (x3 < x1 && x0 > x2)
                  {  // three segments descending down-left
                     stbtt__handle_clipped_edge(scanline, x, ref e, x0, y0, x2, y2);
                     stbtt__handle_clipped_edge(scanline, x, ref e, x2, y2, x1, y1);
                     stbtt__handle_clipped_edge(scanline, x, ref e, x1, y1, x3, y3);
                  }
                  else if (x0 < x1 && x3 > x1)
                  {  // two segments across x, down-right
                     stbtt__handle_clipped_edge(scanline, x, ref e, x0, y0, x1, y1);
                     stbtt__handle_clipped_edge(scanline, x, ref e, x1, y1, x3, y3);
                  }
                  else if (x3 < x1 && x0 > x1)
                  {  // two segments across x, down-left
                     stbtt__handle_clipped_edge(scanline, x, ref e, x0, y0, x1, y1);
                     stbtt__handle_clipped_edge(scanline, x, ref e, x1, y1, x3, y3);
                  }
                  else if (x0 < x2 && x3 > x2)
                  {  // two segments across x+1, down-right
                     stbtt__handle_clipped_edge(scanline, x, ref e, x0, y0, x2, y2);
                     stbtt__handle_clipped_edge(scanline, x, ref e, x2, y2, x3, y3);
                  }
                  else if (x3 < x2 && x0 > x2)
                  {  // two segments across x+1, down-left
                     stbtt__handle_clipped_edge(scanline, x, ref e, x0, y0, x2, y2);
                     stbtt__handle_clipped_edge(scanline, x, ref e, x2, y2, x3, y3);
                  }
                  else
                  {  // one segment
                     stbtt__handle_clipped_edge(scanline, x, ref e, x0, y0, x3, y3);
                  }
               }
            }
         }
      }
   }

   // directly AA rasterize edges w/o supersampling
   static void stbtt__rasterize_sorted_edges(ref stbtt__bitmap result, stbtt__edge[] e, int n, int vsubsample, int off_x, int off_y)
   {
      stbtt__active_edge_collection hh;
      stbtt__init_active_collection(out hh, e.Length);

      int y, j = 0, i;
      Span<float> scanline = result.w > 64 ? new float[result.w * 2 + 1] : stackalloc float[129];
      Span<float> scanline2 = scanline.Slice(result.w);

      //STBTT__NOTUSED(vsubsample);

      y = off_y;
      e[n].y0 = (float)(off_y + result.h) + 1;

      int edgeIndex = 0;

      while (j < result.h)
      {
         // find center of pixel for this scanline
         float scan_y_top = y + 0.0f;
         float scan_y_bottom = y + 1.0f;

         scanline.Clear();
         scanline2.Clear();

         // update all active edges;
         // remove all active edges that terminate before the top of this scanline
         for (var idx = hh.active_indices_count - 1; idx >= 0; idx--)
         {
            ref var z = ref hh.active_edges[hh.active_indices[idx]];

            if (z.ey <= scan_y_top)
            {
               stbtt__remove_from_active_collection(ref hh, idx);
               STBTT_assert(z.direction != 0);
               z.direction = 0;
            }
         }

         // insert all edges that start before the bottom of this scanline
         while (e[edgeIndex].y0 <= scan_y_bottom)
         {
            if (e[edgeIndex].y0 != e[edgeIndex].y1)
            {
               var index = stbtt__new_active(ref hh, ref e[edgeIndex], off_x, scan_y_top);

               ref var active_edge = ref hh.active_edges[index];

               //if (!z.IsNull)
               {
                  if (j == 0 && off_y != 0)
                  {
                     if (active_edge.ey < scan_y_top)
                     {
                        // this can happen due to subpixel positioning and some kind of fp rounding error i think
                        active_edge.ey = scan_y_top;
                     }
                  }
                  STBTT_assert(active_edge.ey >= scan_y_top); // if we get really unlucky a tiny bit of an edge can be out of bounds
                                                              // insert at front
               }
            }
            edgeIndex++;
         }

         // now process all active edges
         if (hh.active_indices_count > 0)
            stbtt__fill_active_edges_new(scanline, scanline2.Slice(1), scanline2, result.w, ref hh, scan_y_top);

         {
            float sum = 0;
            for (i = 0; i < result.w; ++i)
            {
               float k;
               int m;
               sum += scanline2[i];
               k = scanline[i] + sum;
               k = (float)STBTT_fabs(k) * 255 + 0.5f;
               m = (int)k;
               if (m > 255) m = 255;
               result.pixels[j * result.stride + i].GetRef() = (byte)m;
            }
         }

         // advance all the edges
         for (var idx = 0; idx < hh.active_indices_count; idx++)
         {
            ref var z = ref hh.active_edges[hh.active_indices[idx]];
            z.fx += z.fdx; // advance to position for current scanline
         }

         ++y;
         ++j;
      }

      stbtt__free_active_collection(ref hh);

      //if (scanline != scanline_data)
      //   STBTT_free(scanline, userdata);
   }
#else
#error "Unrecognized value of STBTT_RASTERIZER_VERSION"
#endif

   static int STBTT__COMPARE(ref stbtt__edge a, ref stbtt__edge b)
   {

      if ((a).y0 < (b).y0) return -1;
      if ((a).y0 > (b).y0) return 1;

      if ((a).x0 < (b).x0) return -1;
      if ((a).x0 > (b).x0) return 1;

      return 0;
   }

   static void stbtt__sort_edges(stbtt__edge[] p, int n)
   {
      p.AsSpan(0, n).Sort((a, b) => STBTT__COMPARE(ref a, ref b));
   }

   struct stbtt__point
   {
      public float x, y;
   }

   static void stbtt__rasterize(ref stbtt__bitmap result, stbtt__point[] pts, int[] wcount, int windings, float scale_x, float scale_y, float shift_x, float shift_y, int off_x, int off_y, bool invert)
   {
      float y_scale_inv = invert ? -scale_y : scale_y;
      stbtt__edge[] e;
      int n, i, j, k, m;
#if STBTT_RASTERIZER_VERSION_1
      int vsubsample = result.h < 8 ? 15 : 5;
#elif STBTT_RASTERIZER_VERSION_2
      int vsubsample = 1;
#else
#error "Unrecognized value of STBTT_RASTERIZER_VERSION"
#endif
      // vsubsample should divide 255 evenly; otherwise we won't reach full opacity

      // now we have to blow out the windings into explicit edge lists
      n = 0;
      for (i = 0; i < windings; ++i)
         n += wcount[i];

      e = new stbtt__edge[n + 1]; // add an extra one as a sentinel
                                  //if (e == 0) return;
      n = 0;

      m = 0;
      for (i = 0; i < windings; ++i)
      {
         Ptr<stbtt__point> p = new Ptr<stbtt__point>(pts, m);
         m += wcount[i];
         j = wcount[i] - 1;
         for (k = 0; k < wcount[i]; j = k++)
         {
            int a = k, b = j;
            // skip the edge if horizontal
            if (p[j].GetRef().y == p[k].GetRef().y)
               continue;
            // add edge from j to k to the list
            e[n].invert = false;
            if (invert ? p[j].GetRef().y > p[k].GetRef().y : p[j].GetRef().y < p[k].GetRef().y)
            {
               e[n].invert = true;
               a = j; b = k;
            }
            e[n].x0 = p[a].GetRef().x * scale_x + shift_x;
            e[n].y0 = (p[a].GetRef().y * y_scale_inv + shift_y) * vsubsample;
            e[n].x1 = p[b].GetRef().x * scale_x + shift_x;
            e[n].y1 = (p[b].GetRef().y * y_scale_inv + shift_y) * vsubsample;
            ++n;
         }
      }

      // now sort the edges by their highest point (should snap to integer, and then by x)
      //STBTT_sort(e, n, sizeof(e[0]), stbtt__edge_compare);

#if DEBUG_USING_SVG
      string svgPath = string.Join(" ", e.Select(w => "M " + w.x0.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + w.y0.ToString(System.Globalization.CultureInfo.InvariantCulture) +
         "L " + w.x1.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + w.y1.ToString(System.Globalization.CultureInfo.InvariantCulture)));
      Debug.WriteLine("- Edges SVG Path: " + svgPath);
#endif

      stbtt__sort_edges(e, n);

#if DEBUG_USING_SVG
      string svgPathSorted = string.Join(" ", e.Select(w => "M " + w.x0.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + w.y0.ToString(System.Globalization.CultureInfo.InvariantCulture) +
         "L " + w.x1.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + w.y1.ToString(System.Globalization.CultureInfo.InvariantCulture)));
      Debug.WriteLine("- Edges SVG Path Sorted: " + svgPathSorted);
#endif

      // now, traverse the scanlines and find the intersections on each scanline, use xor winding rule
      stbtt__rasterize_sorted_edges(ref result, e, n, vsubsample, off_x, off_y);

      //STBTT_free(e, userdata);
   }

   static void stbtt__add_point(stbtt__point[]? points, int n, float x, float y)
   {
      if (points == null) return; // during first pass, it's unallocated
      points[n].x = x;
      points[n].y = y;
   }

   // tessellate until threshold p is happy... @TODO warped to compensate for non-linear stretching
   static int stbtt__tesselate_curve(stbtt__point[]? points, ref int num_points, float x0, float y0, float x1, float y1, float x2, float y2, float objspace_flatness_squared, int n)
   {
      // midpoint
      float mx = (x0 + 2 * x1 + x2) / 4;
      float my = (y0 + 2 * y1 + y2) / 4;
      // versus directly drawn line
      float dx = (x0 + x2) / 2 - mx;
      float dy = (y0 + y2) / 2 - my;
      if (n > 16) // 65536 segments on one curve better be enough!
         return 1;
      if (dx * dx + dy * dy > objspace_flatness_squared)
      { // half-pixel error allowed... need to be smaller if AA
         stbtt__tesselate_curve(points, ref num_points, x0, y0, (x0 + x1) / 2.0f, (y0 + y1) / 2.0f, mx, my, objspace_flatness_squared, n + 1);
         stbtt__tesselate_curve(points, ref num_points, mx, my, (x1 + x2) / 2.0f, (y1 + y2) / 2.0f, x2, y2, objspace_flatness_squared, n + 1);
      }
      else
      {
         stbtt__add_point(points, num_points, x2, y2);
         num_points = num_points + 1;
      }
      return 1;
   }

   static void stbtt__tesselate_cubic(stbtt__point[]? points, ref int num_points, float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3, float objspace_flatness_squared, int n)
   {
      // @TODO this "flatness" calculation is just made-up nonsense that seems to work well enough
      float dx0 = x1 - x0;
      float dy0 = y1 - y0;
      float dx1 = x2 - x1;
      float dy1 = y2 - y1;
      float dx2 = x3 - x2;
      float dy2 = y3 - y2;
      float dx = x3 - x0;
      float dy = y3 - y0;
      float longlen = (float)(STBTT_sqrt(dx0 * dx0 + dy0 * dy0) + STBTT_sqrt(dx1 * dx1 + dy1 * dy1) + STBTT_sqrt(dx2 * dx2 + dy2 * dy2));
      float shortlen = (float)STBTT_sqrt(dx * dx + dy * dy);
      float flatness_squared = longlen * longlen - shortlen * shortlen;

      if (n > 16) // 65536 segments on one curve better be enough!
         return;

      if (flatness_squared > objspace_flatness_squared)
      {
         float x01 = (x0 + x1) / 2;
         float y01 = (y0 + y1) / 2;
         float x12 = (x1 + x2) / 2;
         float y12 = (y1 + y2) / 2;
         float x23 = (x2 + x3) / 2;
         float y23 = (y2 + y3) / 2;

         float xa = (x01 + x12) / 2;
         float ya = (y01 + y12) / 2;
         float xb = (x12 + x23) / 2;
         float yb = (y12 + y23) / 2;

         float mx = (xa + xb) / 2;
         float my = (ya + yb) / 2;

         stbtt__tesselate_cubic(points, ref num_points, x0, y0, x01, y01, xa, ya, mx, my, objspace_flatness_squared, n + 1);
         stbtt__tesselate_cubic(points, ref num_points, mx, my, xb, yb, x23, y23, x3, y3, objspace_flatness_squared, n + 1);
      }
      else
      {
         stbtt__add_point(points, num_points, x3, y3);
         num_points = num_points + 1;
      }
   }

   // returns number of contours
   static stbtt__point[]? stbtt_FlattenCurves(stbtt_vertex[] vertices, int num_verts, float objspace_flatness, out int[]? contour_lengths, out int num_contours)
   {
      stbtt__point[]? points = null;
      int num_points = 0;

      float objspace_flatness_squared = objspace_flatness * objspace_flatness;
      int i, n = 0, start = 0, pass;

      // count how many "moves" there are to get the contour count
      for (i = 0; i < num_verts; ++i)
         if (vertices[i].type == STBTT.vmove)
            ++n;

      num_contours = n;
      if (n == 0)
      {
         contour_lengths = null;
         return null;
      }

      contour_lengths = new int[n];

      //if (contour_lengths == 0) {
      //   num_contours = 0;
      //   return 0;
      //}

      // make two passes through the points so we don't need to realloc
      for (pass = 0; pass < 2; ++pass)
      {
         float x = 0, y = 0;
         if (pass == 1)
         {
            points = new stbtt__point[num_points];
            //if (points == NULL) goto error;
         }
         num_points = 0;
         n = -1;
         for (i = 0; i < num_verts; ++i)
         {
            switch (vertices[i].type)
            {
               case STBTT.vmove:
                  // start the next contour
                  if (n >= 0)
                     contour_lengths[n] = num_points - start;
                  ++n;
                  start = num_points;

                  x = vertices[i].x; y = vertices[i].y;
                  stbtt__add_point(points, num_points++, x, y);
                  break;
               case STBTT.vline:
                  x = vertices[i].x; y = vertices[i].y;
                  stbtt__add_point(points, num_points++, x, y);
                  break;
               case STBTT.vcurve:
                  stbtt__tesselate_curve(points, ref num_points, x, y,
                                           vertices[i].cx, vertices[i].cy,
                                           vertices[i].x, vertices[i].y,
                                           objspace_flatness_squared, 0);
                  x = vertices[i].x; y = vertices[i].y;
                  break;
               case STBTT.vcubic:
                  stbtt__tesselate_cubic(points, ref num_points, x, y,
                                           vertices[i].cx, vertices[i].cy,
                                           vertices[i].cx1, vertices[i].cy1,
                                           vertices[i].x, vertices[i].y,
                                           objspace_flatness_squared, 0);
                  x = vertices[i].x; y = vertices[i].y;
                  break;
            }
         }
         contour_lengths[n] = num_points - start;
      }

      return points;
   }

   //////////////////////////////////////////////////////////////////////////////
   //
   // bitmap baking
   //
   // This is SUPER-CRAPPY packing to keep source code small

   static int stbtt_BakeFontBitmap_internal(BytePtr data, int offset,  // font location (use offset=0 for plain .ttf)
                                   float pixel_height,                     // height of font in pixels
                                   byte[] pixels, int pw, int ph,  // bitmap to be filled in
                                   int first_char, int num_chars,          // characters to bake
                                   stbtt_bakedchar[] chardata)
   {
      float scale;
      int x, y, bottom_y, i;
      stbtt_fontinfo f;

      if (stbtt_InitFont(out f, data, offset) == 0)
         return -1;

      Array.Clear(pixels);
      x = y = 1;
      bottom_y = 1;

      scale = stbtt_ScaleForPixelHeight(ref f, pixel_height);

      for (i = 0; i < num_chars; ++i)
      {
         int advance, lsb, x0, y0, x1, y1, gw, gh;
         int g = stbtt_FindGlyphIndex(ref f, first_char + i);
         stbtt_GetGlyphHMetrics(ref f, g, out advance, out lsb);
         stbtt_GetGlyphBitmapBox(ref f, g, scale, scale, out x0, out y0, out x1, out y1);
         gw = x1 - x0;
         gh = y1 - y0;
         if (x + gw + 1 >= pw)
         {
            y = bottom_y; x = 1; // advance to next row
         }
         if (y + gh + 1 >= ph) // check if it fits vertically AFTER potentially moving to next row
            return -i;
         STBTT_assert(x + gw < pw);
         STBTT_assert(y + gh < ph);
         stbtt_MakeGlyphBitmap(ref f, ((BytePtr)pixels) + x + y * pw, gw, gh, pw, scale, scale, g);
         chardata[i].x0 = (ushort)(stbtt_int16)x;
         chardata[i].y0 = (ushort)(stbtt_int16)y;
         chardata[i].x1 = (ushort)(stbtt_int16)(x + gw);
         chardata[i].y1 = (ushort)(stbtt_int16)(y + gh);
         chardata[i].xadvance = scale * advance;
         chardata[i].xoff = (float)x0;
         chardata[i].yoff = (float)y0;
         x = x + gw + 1;
         if (y + gh + 1 > bottom_y)
            bottom_y = y + gh + 1;
      }
      return bottom_y;
   }

   //////////////////////////////////////////////////////////////////////////////
   //
   // rectangle packing replacement routines if you don't have stb_rect_pack.h
   //

#if !STB_RECT_PACK_VERSION

   ////////////////////////////////////////////////////////////////////////////////////
   //                                                                                //
   //                                                                                //
   // COMPILER WARNING ?!?!?                                                         //
   //                                                                                //
   //                                                                                //
   // if you get a compile warning due to these symbols being defined more than      //
   // once, move #include "stb_rect_pack.h" before #include "stb_truetype.h"         //
   //                                                                                //
   ////////////////////////////////////////////////////////////////////////////////////

   public struct stbrp_context
   {
      public int width, height;
      public int x, y, bottom_y;
   }

   public struct stbrp_node
   {
      public byte x;
   }

   public struct stbrp_rect
   {
      public stbrp_coord x, y;
      public int id, w, h;
      public int was_packed;
   };

   static void stbrp_init_target(out stbrp_context con, int pw, int ph, stbrp_node[] nodes, int num_nodes)
   {
      con.width = pw;
      con.height = ph;
      con.x = 0;
      con.y = 0;
      con.bottom_y = 0;
      //STBTT__NOTUSED(nodes);
      //STBTT__NOTUSED(num_nodes);
   }

   static void stbrp_pack_rects(ref stbrp_context con, stbrp_rect[] rects, int num_rects)
   {
      int i;
      for (i = 0; i < num_rects; ++i)
      {
         if (con.x + rects[i].w > con.width)
         {
            con.x = 0;
            con.y = con.bottom_y;
         }
         if (con.y + rects[i].h > con.height)
            break;
         rects[i].x = con.x;
         rects[i].y = con.y;
         rects[i].was_packed = 1;
         con.x += rects[i].w;
         if (con.y + rects[i].h > con.bottom_y)
            con.bottom_y = con.y + rects[i].h;
      }
      for (; i < num_rects; ++i)
         rects[i].was_packed = 0;
   }
#endif

   //////////////////////////////////////////////////////////////////////////////
   //
   // bitmap baking
   //
   // This is SUPER-AWESOME (tm Ryan Gordon) packing using stb_rect_pack.h. If
   // stb_rect_pack.h isn't available, it uses the BakeFontBitmap strategy.

   const int STBTT__OVER_MASK = (STBTT_MAX_OVERSAMPLE - 1);

   static void stbtt__h_prefilter(BytePtr pixels, int w, int h, int stride_in_bytes, int kernel_width)
   {
      Span<byte> buffer = stackalloc byte[STBTT_MAX_OVERSAMPLE];

      int safe_w = (int)(w - kernel_width);
      int j;
      buffer.Clear();
      for (j = 0; j < h; ++j)
      {
         int i;
         uint total;
         buffer.Clear();

         total = 0;

         // make kernel_width a constant in common cases so compiler can optimize out the divide
         switch (kernel_width)
         {
            case 2:
               for (i = 0; i <= safe_w; ++i)
               {
                  total += (uint)(pixels[i] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i];
                  pixels[i].GetRef() = (byte)(total / 2);
               }
               break;
            case 3:
               for (i = 0; i <= safe_w; ++i)
               {
                  total += (uint)(pixels[i] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i];
                  pixels[i].GetRef() = (byte)(total / 3);
               }
               break;
            case 4:
               for (i = 0; i <= safe_w; ++i)
               {
                  total += (uint)(pixels[i] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i];
                  pixels[i].GetRef() = (byte)(total / 4);
               }
               break;
            case 5:
               for (i = 0; i <= safe_w; ++i)
               {
                  total += (uint)(pixels[i] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i];
                  pixels[i].GetRef() = (byte)(total / 5);
               }
               break;
            default:
               for (i = 0; i <= safe_w; ++i)
               {
                  total += (uint)(pixels[i] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i];
                  pixels[i].GetRef() = (byte)(total / kernel_width);
               }
               break;
         }

         for (; i < w; ++i)
         {
            STBTT_assert(pixels[i] == 0);
            total -= buffer[i & STBTT__OVER_MASK];
            pixels[i].GetRef() = (byte)(total / kernel_width);
         }

         pixels += stride_in_bytes;
      }
   }

   static void stbtt__v_prefilter(BytePtr pixels, int w, int h, int stride_in_bytes, int kernel_width)
   {
      Span<byte> buffer = stackalloc byte[STBTT_MAX_OVERSAMPLE];
      int safe_h = (int)(h - kernel_width);
      int j;
      buffer.Clear();
      for (j = 0; j < w; ++j)
      {
         int i;
         uint total;
         buffer.Clear();

         total = 0;

         // make kernel_width a constant in common cases so compiler can optimize out the divide
         switch (kernel_width)
         {
            case 2:
               for (i = 0; i <= safe_h; ++i)
               {
                  total += (uint)(pixels[i * stride_in_bytes] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i * stride_in_bytes];
                  pixels[i * stride_in_bytes].GetRef() = (byte)(total / 2);
               }
               break;
            case 3:
               for (i = 0; i <= safe_h; ++i)
               {
                  total += (uint)(pixels[i * stride_in_bytes] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i * stride_in_bytes];
                  pixels[i * stride_in_bytes].GetRef() = (byte)(total / 3);
               }
               break;
            case 4:
               for (i = 0; i <= safe_h; ++i)
               {
                  total += (uint)(pixels[i * stride_in_bytes] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i * stride_in_bytes];
                  pixels[i * stride_in_bytes].GetRef() = (byte)(total / 4);
               }
               break;
            case 5:
               for (i = 0; i <= safe_h; ++i)
               {
                  total += (uint)(pixels[i * stride_in_bytes] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i * stride_in_bytes];
                  pixels[i * stride_in_bytes].GetRef() = (byte)(total / 5);
               }
               break;
            default:
               for (i = 0; i <= safe_h; ++i)
               {
                  total += (uint)(pixels[i * stride_in_bytes] - buffer[i & STBTT__OVER_MASK]);
                  buffer[(i + kernel_width) & STBTT__OVER_MASK] = pixels[i * stride_in_bytes];
                  pixels[i * stride_in_bytes].GetRef() = (byte)(total / kernel_width);
               }
               break;
         }

         for (; i < h; ++i)
         {
            STBTT_assert(pixels[i * stride_in_bytes] == 0);
            total -= buffer[i & STBTT__OVER_MASK];
            pixels[i * stride_in_bytes].GetRef() = (byte)(total / kernel_width);
         }

         pixels += 1;
      }
   }

   static float stbtt__oversample_shift(int oversample)
   {
      if (oversample == 0)
         return 0.0f;

      // The prefilter is a box filter of width "oversample",
      // which shifts phase by (oversample - 1)/2 pixels in
      // oversampled space. We want to shift in the opposite
      // direction to counter this.
      return (float)-(oversample - 1) / (2.0f * (float)oversample);
   }

   // rects array must be big enough to accommodate all characters in the given ranges










   //////////////////////////////////////////////////////////////////////////////
   //
   // sdf computation
   //

   static private float STBTT_min(float a, float b) => ((a) < (b) ? (a) : (b));
   static private float STBTT_max(float a, float b) => ((a) < (b) ? (b) : (a));

   static private float STBTT_min(int a, int b) => ((a) < (b) ? (a) : (b));
   static private float STBTT_max(int a, int b) => ((a) < (b) ? (b) : (a));

   static int stbtt__ray_intersect_bezier(Span<float> orig, Span<float> ray, Span<float> q0, Span<float> q1, Span<float> q2, Span<float> hits)
   {
      float q0perp = q0[1] * ray[0] - q0[0] * ray[1];
      float q1perp = q1[1] * ray[0] - q1[0] * ray[1];
      float q2perp = q2[1] * ray[0] - q2[0] * ray[1];
      float roperp = orig[1] * ray[0] - orig[0] * ray[1];

      float a = q0perp - 2 * q1perp + q2perp;
      float b = q1perp - q0perp;
      float c = q0perp - roperp;

      float s0 = 0.0f, s1 = 0.0f;
      int num_s = 0;

      if (a != 0.0)
      {
         float discr = b * b - a * c;
         if (discr > 0.0)
         {
            float rcpna = -1 / a;
            float d = (float)STBTT_sqrt(discr);
            s0 = (b + d) * rcpna;
            s1 = (b - d) * rcpna;
            if (s0 >= 0.0 && s0 <= 1.0)
               num_s = 1;
            if (d > 0.0 && s1 >= 0.0 && s1 <= 1.0)
            {
               if (num_s == 0) s0 = s1;
               ++num_s;
            }
         }
      }
      else
      {
         // 2*b*s + c = 0
         // s = -c / (2*b)
         s0 = c / (-2 * b);
         if (s0 >= 0.0 && s0 <= 1.0)
            num_s = 1;
      }

      if (num_s == 0)
         return 0;
      else
      {
         float rcp_len2 = 1 / (ray[0] * ray[0] + ray[1] * ray[1]);
         float rayn_x = ray[0] * rcp_len2, rayn_y = ray[1] * rcp_len2;

         float q0d = q0[0] * rayn_x + q0[1] * rayn_y;
         float q1d = q1[0] * rayn_x + q1[1] * rayn_y;
         float q2d = q2[0] * rayn_x + q2[1] * rayn_y;
         float rod = orig[0] * rayn_x + orig[1] * rayn_y;

         float q10d = q1d - q0d;
         float q20d = q2d - q0d;
         float q0rd = q0d - rod;

         hits[0] = q0rd + s0 * (2.0f - 2.0f * s0) * q10d + s0 * s0 * q20d;
         hits[1] = a * s0 + b;

         if (num_s > 1)
         {
            hits[2] = q0rd + s1 * (2.0f - 2.0f * s1) * q10d + s1 * s1 * q20d;
            hits[3] = a * s1 + b;
            return 2;
         }
         else
         {
            return 1;
         }
      }
   }

   static bool equal(Span<float> a, Span<float> b)
   {
      return (a[0] == b[0] && a[1] == b[1]);
   }

   static int stbtt__compute_crossings_x(float x, float y, int nverts, stbtt_vertex[] verts)
   {
      int i;
      Span<float> orig = stackalloc float[2];
      Span<float> ray = stackalloc float[] { 1.0f, 0.0f };
      float y_frac;
      int winding = 0;

      // make sure y never passes through a vertex of the shape
      y_frac = (float)STBTT_fmod(y, 1.0f);
      if (y_frac < 0.01f)
         y += 0.01f;
      else if (y_frac > 0.99f)
         y -= 0.01f;

      orig[0] = x;
      orig[1] = y;

      Span<float> q0 = stackalloc float[2], q1 = stackalloc float[2], q2 = stackalloc float[2];
      Span<float> hits = stackalloc float[4];

      // test a ray from (-infinity,y) to (x,y)
      for (i = 0; i < nverts; ++i)
      {
         if (verts[i].type == STBTT.vline)
         {
            int x0 = (int)verts[i - 1].x, y0 = (int)verts[i - 1].y;
            int x1 = (int)verts[i].x, y1 = (int)verts[i].y;
            if (y > STBTT_min(y0, y1) && y < STBTT_max(y0, y1) && x > STBTT_min(x0, x1))
            {
               float x_inter = (y - y0) / (y1 - y0) * (x1 - x0) + x0;
               if (x_inter < x)
                  winding += (y0 < y1) ? 1 : -1;
            }
         }
         if (verts[i].type == STBTT.vcurve)
         {
            int x0 = (int)verts[i - 1].x, y0 = (int)verts[i - 1].y;
            int x1 = (int)verts[i].cx, y1 = (int)verts[i].cy;
            int x2 = (int)verts[i].x, y2 = (int)verts[i].y;
            int ax = (int)STBTT_min(x0, STBTT_min(x1, x2)), ay = (int)STBTT_min(y0, STBTT_min(y1, y2));
            int by = (int)STBTT_max(y0, STBTT_max(y1, y2));
            if (y > ay && y < by && x > ax)
            {
               q0[0] = (float)x0;
               q0[1] = (float)y0;
               q1[0] = (float)x1;
               q1[1] = (float)y1;
               q2[0] = (float)x2;
               q2[1] = (float)y2;
               if (equal(q0, q1) || equal(q1, q2))
               {
                  x0 = (int)verts[i - 1].x;
                  y0 = (int)verts[i - 1].y;
                  x1 = (int)verts[i].x;
                  y1 = (int)verts[i].y;
                  if (y > STBTT_min(y0, y1) && y < STBTT_max(y0, y1) && x > STBTT_min(x0, x1))
                  {
                     float x_inter = (y - y0) / (y1 - y0) * (x1 - x0) + x0;
                     if (x_inter < x)
                        winding += (y0 < y1) ? 1 : -1;
                  }
               }
               else
               {
                  int num_hits = stbtt__ray_intersect_bezier(orig, ray, q0, q1, q2, hits);
                  if (num_hits >= 1)
                     if (hits[0] < 0)
                        winding += (hits[1] < 0 ? -1 : 1);
                  if (num_hits >= 2)
                     if (hits[2] < 0)
                        winding += (hits[3] < 0 ? -1 : 1);
               }
            }
         }
      }
      return winding;
   }

   static float stbtt__cuberoot(float x)
   {
      if (x < 0)
         return -(float)STBTT_pow(-x, 1.0f / 3.0f);
      else
         return (float)STBTT_pow(x, 1.0f / 3.0f);
   }

   // x^3 + a*x^2 + b*x + c = 0
   static int stbtt__solve_cubic(float a, float b, float c, Span<float> r)
   {
      float s = -a / 3;
      float p = b - a * a / 3;
      float q = a * (2 * a * a - 9 * b) / 27 + c;
      float p3 = p * p * p;
      float d = q * q + 4 * p3 / 27;
      if (d >= 0)
      {
         float z = (float)STBTT_sqrt(d);
         float u = (-q + z) / 2;
         float v = (-q - z) / 2;
         u = stbtt__cuberoot(u);
         v = stbtt__cuberoot(v);
         r[0] = s + u + v;
         return 1;
      }
      else
      {
         float u = (float)STBTT_sqrt(-p / 3);
         float v = (float)STBTT_acos(-STBTT_sqrt(-27 / p3) * q / 2) / 3; // p3 must be negative, since d is negative
         float m = (float)STBTT_cos(v);
         float n = (float)STBTT_cos(v - 3.141592f / 2) * 1.732050808f;
         r[0] = s + u * 2 * m;
         r[1] = s - u * (m + n);
         r[2] = s - u * (m - n);

         //STBTT_assert( STBTT_fabs(((r[0]+a)*r[0]+b)*r[0]+c) < 0.05f);  // these asserts may not be safe at all scales, though they're in bezier t parameter units so maybe?
         //STBTT_assert( STBTT_fabs(((r[1]+a)*r[1]+b)*r[1]+c) < 0.05f);
         //STBTT_assert( STBTT_fabs(((r[2]+a)*r[2]+b)*r[2]+c) < 0.05f);
         return 3;
      }
   }





   //////////////////////////////////////////////////////////////////////////////
   //
   // font name matching -- recommended not to use this
   //

   // check if a utf8 string contains a prefix which is the utf16 string; if so return length of matching utf8 string
   static stbtt_int32 stbtt__CompareUTF8toUTF16_bigendian_prefix(BytePtr s1, stbtt_int32 len1, BytePtr s2, stbtt_int32 len2)
   {
      stbtt_int32 i = 0;

      // convert utf16 to utf8 and compare the results while converting
      while (len2 != 0)
      {
         stbtt_uint16 ch = (ushort)(s2[0] * 256 + s2[1]);
         if (ch < 0x80)
         {
            if (i >= len1) return -1;
            if (s1[i++] != ch) return -1;
         }
         else if (ch < 0x800)
         {
            if (i + 1 >= len1) return -1;
            if (s1[i++] != 0xc0 + (ch >> 6)) return -1;
            if (s1[i++] != 0x80 + (ch & 0x3f)) return -1;
         }
         else if (ch >= 0xd800 && ch < 0xdc00)
         {
            stbtt_uint32 c;
            stbtt_uint16 ch2 = (ushort)(s2[2] * 256 + s2[3]);
            if (i + 3 >= len1) return -1;
            c = (uint)(((ch - 0xd800) << 10) + (ch2 - 0xdc00) + 0x10000);
            if (s1[i++] != 0xf0 + (c >> 18)) return -1;
            if (s1[i++] != 0x80 + ((c >> 12) & 0x3f)) return -1;
            if (s1[i++] != 0x80 + ((c >> 6) & 0x3f)) return -1;
            if (s1[i++] != 0x80 + ((c) & 0x3f)) return -1;
            s2 += 2; // plus another 2 below
            len2 -= 2;
         }
         else if (ch >= 0xdc00 && ch < 0xe000)
         {
            return -1;
         }
         else
         {
            if (i + 2 >= len1) return -1;
            if (s1[i++] != 0xe0 + (ch >> 12)) return -1;
            if (s1[i++] != 0x80 + ((ch >> 6) & 0x3f)) return -1;
            if (s1[i++] != 0x80 + ((ch) & 0x3f)) return -1;
         }
         s2 += 2;
         len2 -= 2;
      }
      return i;
   }

   static bool stbtt_CompareUTF8toUTF16_bigendian_internal(BytePtr s1, int len1, BytePtr s2, int len2)
   {
      return len1 == stbtt__CompareUTF8toUTF16_bigendian_prefix(s1, len1, s2, len2);
   }



   static bool stbtt__matchpair(BytePtr fc, stbtt_uint32 nm, BytePtr name, stbtt_int32 nlen, stbtt_int32 target_id, stbtt_int32 next_id)
   {
      stbtt_int32 i;
      stbtt_int32 count = ttUSHORT(fc + nm + 2);
      stbtt_int32 stringOffset = (int)(nm + ttUSHORT(fc + nm + 4));

      for (i = 0; i < count; ++i)
      {
         stbtt_uint32 loc = (uint)(nm + 6 + 12 * i);
         stbtt_int32 id = ttUSHORT(fc + loc + 6);
         if (id == target_id)
         {
            // find the encoding
            stbtt_int32 platform = ttUSHORT(fc + loc + 0), encoding = ttUSHORT(fc + loc + 2), language = ttUSHORT(fc + loc + 4);

            // is this a Unicode encoding?
            if (platform == 0 || (platform == 3 && encoding == 1) || (platform == 3 && encoding == 10))
            {
               stbtt_int32 slen = ttUSHORT(fc + loc + 8);
               stbtt_int32 off = ttUSHORT(fc + loc + 10);

               // check if there's a prefix match
               stbtt_int32 matchlen = stbtt__CompareUTF8toUTF16_bigendian_prefix(name, nlen, fc + stringOffset + off, slen);
               if (matchlen >= 0)
               {
                  // check for target_id+1 immediately following, with same encoding & language
                  if (i + 1 < count && ttUSHORT(fc + loc + 12 + 6) == next_id && ttUSHORT(fc + loc + 12) == platform && ttUSHORT(fc + loc + 12 + 2) == encoding && ttUSHORT(fc + loc + 12 + 4) == language)
                  {
                     slen = ttUSHORT(fc + loc + 12 + 8);
                     off = ttUSHORT(fc + loc + 12 + 10);
                     if (slen == 0)
                     {
                        if (matchlen == nlen)
                           return true;
                     }
                     else if (matchlen < nlen && name[matchlen] == ' ')
                     {
                        ++matchlen;
                        if (stbtt_CompareUTF8toUTF16_bigendian_internal(name + matchlen, nlen - matchlen, fc + stringOffset + off, slen))
                           return true;
                     }
                  }
                  else
                  {
                     // if nothing immediately following
                     if (matchlen == nlen)
                        return true;
                  }
               }
            }

            // @TODO handle other encodings
         }
      }
      return false;
   }

   static bool stbtt__matches(BytePtr fc, stbtt_uint32 offset, BytePtr name, STBTT_MACSTYLE flags)
   {
      stbtt_int32 nlen = name.FirstIndexOf(0);
      stbtt_uint32 nm, hd;
      if (!stbtt__isfont(fc + offset)) return false;

      // check italics/bold/underline flags in macStyle...
      if (flags != 0)
      {
         hd = stbtt__find_table(fc, offset, "head");
         if ((ttUSHORT(fc + hd + 44) & 7) != ((int)flags & 7)) return false;
      }

      nm = stbtt__find_table(fc, offset, "name");
      if (nm == 0) return false;

      if (flags != 0)
      {
         // if we checked the macStyle flags, then just check the family and ignore the subfamily
         if (stbtt__matchpair(fc, nm, name, nlen, 16, -1)) return true;
         if (stbtt__matchpair(fc, nm, name, nlen, 1, -1)) return true;
         if (stbtt__matchpair(fc, nm, name, nlen, 3, -1)) return true;
      }
      else
      {
         if (stbtt__matchpair(fc, nm, name, nlen, 16, 17)) return true;
         if (stbtt__matchpair(fc, nm, name, nlen, 1, 2)) return true;
         if (stbtt__matchpair(fc, nm, name, nlen, 3, -1)) return true;
      }

      return false;
   }

   static int stbtt_FindMatchingFont_internal(BytePtr font_collection, BytePtr name_utf8, STBTT_MACSTYLE flags)
   {
      stbtt_int32 i;
      for (i = 0; ; ++i)
      {
         stbtt_int32 off = stbtt_GetFontOffsetForIndex(font_collection, i);
         if (off < 0) return off;
         if (stbtt__matches(font_collection, (uint)off, name_utf8, flags))
            return off;
      }
   }






   // FULL VERSION HISTORY
   //
   //   1.25 (2021-07-11) many fixes
   //   1.24 (2020-02-05) fix warning
   //   1.23 (2020-02-02) query SVG data for glyphs; query whole kerning table (but only kern not GPOS)
   //   1.22 (2019-08-11) minimize missing-glyph duplication; fix kerning if both 'GPOS' and 'kern' are defined
   //   1.21 (2019-02-25) fix warning
   //   1.20 (2019-02-07) PackFontRange skips missing codepoints; GetScaleFontVMetrics()
   //   1.19 (2018-02-11) OpenType GPOS kerning (horizontal only), STBTT_fmod
   //   1.18 (2018-01-29) add missing function
   //   1.17 (2017-07-23) make more arguments const; doc fix
   //   1.16 (2017-07-12) SDF support
   //   1.15 (2017-03-03) make more arguments const
   //   1.14 (2017-01-16) num-fonts-in-TTC function
   //   1.13 (2017-01-02) support OpenType fonts, certain Apple fonts
   //   1.12 (2016-10-25) suppress warnings about casting away const with -Wcast-qual
   //   1.11 (2016-04-02) fix unused-variable warning
   //   1.10 (2016-04-02) allow user-defined fabs() replacement
   //                     fix memory leak if fontsize=0.0
   //                     fix warning from duplicate typedef
   //   1.09 (2016-01-16) warning fix; avoid crash on outofmem; use alloc userdata for PackFontRanges
   //   1.08 (2015-09-13) document stbtt_Rasterize(); fixes for vertical & horizontal edges
   //   1.07 (2015-08-01) allow PackFontRanges to accept arrays of sparse codepoints;
   //                     allow PackFontRanges to pack and render in separate phases;
   //                     fix stbtt_GetFontOFfsetForIndex (never worked for non-0 input?);
   //                     fixed an assert() bug in the new rasterizer
   //                     replace assert() with STBTT_assert() in new rasterizer
   //   1.06 (2015-07-14) performance improvements (~35% faster on x86 and x64 on test machine)
   //                     also more precise AA rasterizer, except if shapes overlap
   //                     remove need for STBTT_sort
   //   1.05 (2015-04-15) fix misplaced definitions for STBTT_STATIC
   //   1.04 (2015-04-15) typo in example
   //   1.03 (2015-04-12) STBTT_STATIC, fix memory leak in new packing, various fixes
   //   1.02 (2014-12-10) fix various warnings & compile issues w/ stb_rect_pack, C++
   //   1.01 (2014-12-08) fix subpixel position when oversampling to exactly match
   //                        non-oversampled; STBTT_POINT_SIZE for packed case only
   //   1.00 (2014-12-06) add new PackBegin etc. API, w/ support for oversampling
   //   0.99 (2014-09-18) fix multiple bugs with subpixel rendering (ryg)
   //   0.9  (2014-08-07) support certain mac/iOS fonts without an MS platformID
   //   0.8b (2014-07-07) fix a warning
   //   0.8  (2014-05-25) fix a few more warnings
   //   0.7  (2013-09-25) bugfix: subpixel glyph bug fixed in 0.5 had come back
   //   0.6c (2012-07-24) improve documentation
   //   0.6b (2012-07-20) fix a few more warnings
   //   0.6  (2012-07-17) fix warnings; added stbtt_ScaleForMappingEmToPixels,
   //                        stbtt_GetFontBoundingBox, stbtt_IsGlyphEmpty
   //   0.5  (2011-12-09) bugfixes:
   //                        subpixel glyph renderer computed wrong bounding box
   //                        first vertex of shape can be off-curve (FreeSans)
   //   0.4b (2011-12-03) fixed an error in the font baking example
   //   0.4  (2011-12-01) kerning, subpixel rendering (tor)
   //                    bugfixes for:
   //                        codepoint-to-glyph conversion using table fmt=12
   //                        codepoint-to-glyph conversion using table fmt=4
   //                        stbtt_GetBakedQuad with non-square texture (Zer)
   //                    updated Hello World! sample to use kerning and subpixel
   //                    fixed some warnings
   //   0.3  (2009-06-24) cmap fmt=12, compound shapes (MM)
   //                    userdata, malloc-from-userdata, non-zero fill (stb)
   //   0.2  (2009-03-11) Fix unsigned/signed char warnings
   //   0.1  (2009-03-09) First public release
   //

   /*
   ------------------------------------------------------------------------------
   This software is available under 2 licenses -- choose whichever you prefer.
   ------------------------------------------------------------------------------
   ALTERNATIVE A - MIT License
   Copyright (c) 2017 Sean Barrett
   Permission is hereby granted, free of charge, to any person obtaining a copy of
   this software and associated documentation files (the "Software"), to deal in
   the Software without restriction, including without limitation the rights to
   use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
   of the Software, and to permit persons to whom the Software is furnished to do
   so, subject to the following conditions:
   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.
   ------------------------------------------------------------------------------
   ALTERNATIVE B - Public Domain (www.unlicense.org)
   This is free and unencumbered software released into the public domain.
   Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
   software, either in source code form or as a compiled binary, for any purpose,
   commercial or non-commercial, and by any means.
   In jurisdictions that recognize copyright laws, the author or authors of this
   software dedicate any and all copyright interest in the software to the public
   domain. We make this dedication for the benefit of the public at large and to
   the detriment of our heirs and successors. We intend this dedication to be an
   overt act of relinquishment in perpetuity of all present and future rights to
   this software under copyright law.
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
   ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
   WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
   ------------------------------------------------------------------------------
   */
}