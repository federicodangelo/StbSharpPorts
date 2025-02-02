#pragma warning disable IDE1006 // Naming Styles

#define STBI_ONLY_PNG 
#define STBI_ONLY_TGA
#define STBI_ONLY_BMP
#define STBI_ONLY_JPEG

#define STBI_NO_STDIO
#define STBI_NO_LINEAR
#define STBI_NO_SIMD

#if STBI_ONLY_JPEG || STBI_ONLY_PNG || STBI_ONLY_BMP || STBI_ONLY_TGA || STBI_ONLY_GIF || STBI_ONLY_PSD || STBI_ONLY_HDR || STBI_ONLY_PIC || STBI_ONLY_PNM || STBI_ONLY_ZLIB
#if !STBI_ONLY_JPEG
#define STBI_NO_JPEG
#endif
#if !STBI_ONLY_PNG
#define STBI_NO_PNG
#endif
#if !STBI_ONLY_BMP
#define STBI_NO_BMP
#endif
#if !STBI_ONLY_PSD
#define STBI_NO_PSD
#endif
#if !STBI_ONLY_TGA
#define STBI_NO_TGA
#endif
#if !STBI_ONLY_GIF
#define STBI_NO_GIF
#endif
#if !STBI_ONLY_HDR
#define STBI_NO_HDR
#endif
#if !STBI_ONLY_PIC
#define STBI_NO_PIC
#endif
#if !STBI_ONLY_PNM
#define STBI_NO_PNM
#endif
#endif

#if STBI_NO_PNG && !STBI_SUPPORT_ZLIB && !STBI_NO_ZLIB
#define STBI_NO_ZLIB
#endif

namespace StbSharp;

using stbi_uc = byte;
using stbi_us = ushort;
using stbi__uint16 = ushort;
using stbi__int16 = short;
using stbi__uint32 = uint;
using stbi__int32 = int;
using size_t = int;

using System.Diagnostics;
using StbSharp.StbCommon;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO.Compression;

public class StbImage
{
   /* stb_image - v2.30 - public domain image loader - http://nothings.org/stb
                                     no warranty implied; use at your own risk

      Do this:
         #define STB_IMAGE_IMPLEMENTATION
      before you include this file in *one* C or C++ file to create the implementation.

      // i.e. it should look like this:
      #include ...
      #include ...
      #include ...
      #define STB_IMAGE_IMPLEMENTATION
      #include "stb_image.h"

      You can #define STBI_ASSERT(x) before the #include to avoid using assert.h.
      And #define STBI_MALLOC, STBI_REALLOC, and STBI_FREE to avoid using malloc,realloc,free


      QUICK NOTES:
         Primarily of interest to game developers and other people who can
             avoid problematic images and only need the trivial interface

         JPEG baseline & progressive (12 bpc/arithmetic not supported, same as stock IJG lib)
         PNG 1/2/4/8/16-bit-per-channel

         TGA (not sure what subset, if a subset)
         BMP non-1bpp, non-RLE
         PSD (composited view only, no extra channels, 8/16 bit-per-channel)

         GIF (*comp always reports as 4-channel)
         HDR (radiance rgbE format)
         PIC (Softimage PIC)
         PNM (PPM and PGM binary only)

         Animated GIF still needs a proper API, but here's one way to do it:
             http://gist.github.com/urraka/685d9a6340b26b830d49

         - decode from memory or through FILE (define STBI_NO_STDIO to remove code)
         - decode from arbitrary I/O callbacks
         - SIMD acceleration on x86/x64 (SSE2) and ARM (NEON)

      Full documentation under "DOCUMENTATION" below.


   LICENSE

     See end of file for license information.

   RECENT REVISION HISTORY:

         2.30  (2024-05-31) avoid erroneous gcc warning
         2.29  (2023-05-xx) optimizations
         2.28  (2023-01-29) many error fixes, security errors, just tons of stuff
         2.27  (2021-07-11) document stbi_info better, 16-bit PNM support, bug fixes
         2.26  (2020-07-13) many minor fixes
         2.25  (2020-02-02) fix warnings
         2.24  (2020-02-02) fix warnings; thread-local failure_reason and flip_vertically
         2.23  (2019-08-11) fix clang static analysis warning
         2.22  (2019-03-04) gif fixes, fix warnings
         2.21  (2019-02-25) fix typo in comment
         2.20  (2019-02-07) support utf8 filenames in Windows; fix warnings and platform ifdefs
         2.19  (2018-02-11) fix warning
         2.18  (2018-01-30) fix warnings
         2.17  (2018-01-29) bugfix, 1-bit BMP, 16-bitness query, fix warnings
         2.16  (2017-07-23) all functions have 16-bit variants; optimizations; bugfixes
         2.15  (2017-03-18) fix png-1,2,4; all Imagenet JPGs; no runtime SSE detection on GCC
         2.14  (2017-03-03) remove deprecated STBI_JPEG_OLD; fixes for Imagenet JPGs
         2.13  (2016-12-04) experimental 16-bit API, only for PNG so far; fixes
         2.12  (2016-04-02) fix typo in 2.11 PSD fix that caused crashes
         2.11  (2016-04-02) 16-bit PNGS; enable SSE2 in non-gcc x64
                            RGB-format JPEG; remove white matting in PSD;
                            allocate large structures on the stack;
                            correct channel count for PNG & BMP
         2.10  (2016-01-22) avoid warning introduced in 2.09
         2.09  (2016-01-16) 16-bit TGA; comments in PNM files; STBI_REALLOC_SIZED

      See end of file for full revision history.


    ============================    Contributors    =========================

    Image formats                          Extensions, features
       Sean Barrett (jpeg, png, bmp)          Jetro Lauha (stbi_info)
       Nicolas Schulz (hdr, psd)              Martin "SpartanJ" Golini (stbi_info)
       Jonathan Dummer (tga)                  James "moose2000" Brown (iPhone PNG)
       Jean-Marc Lienher (gif)                Ben "Disch" Wenger (io callbacks)
       Tom Seddon (pic)                       Omar Cornut (1/2/4-bit PNG)
       Thatcher Ulrich (psd)                  Nicolas Guillemot (vertical flip)
       Ken Miller (pgm, ppm)                  Richard Mitton (16-bit PSD)
       github:urraka (animated gif)           Junggon Kim (PNM comments)
       Christopher Forseth (animated gif)     Daniel Gibson (16-bit TGA)
                                              socks-the-fox (16-bit PNG)
                                              Jeremy Sawicki (handle all ImageNet JPGs)
    Optimizations & bugfixes                  Mikhail Morozov (1-bit BMP)
       Fabian "ryg" Giesen                    Anael Seghezzi (is-16-bit query)
       Arseny Kapoulkine                      Simon Breuss (16-bit PNM)
       John-Mark Allen
       Carmelo J Fdez-Aguera

    Bug & warning fixes
       Marc LeBlanc            David Woo          Guillaume George     Martins Mozeiko
       Christpher Lloyd        Jerry Jansson      Joseph Thomson       Blazej Dariusz Roszkowski
       Phil Jordan                                Dave Moore           Roy Eltham
       Hayaki Saito            Nathan Reed        Won Chun
       Luke Graham             Johan Duparc       Nick Verigakis       the Horde3D community
       Thomas Ruf              Ronny Chevalier                         github:rlyeh
       Janez Zemva             John Bartholomew   Michal Cichon        github:romigrou
       Jonathan Blow           Ken Hamada         Tero Hanninen        github:svdijk
       Eugene Golushkov        Laurent Gomila     Cort Stratton        github:snagar
       Aruelien Pocheville     Sergio Gonzalez    Thibault Reuille     github:Zelex
       Cass Everitt            Ryamond Barbiero                        github:grim210
       Paul Du Bois            Engin Manap        Aldo Culquicondor    github:sammyhw
       Philipp Wiesemann       Dale Weiler        Oriol Ferrer Mesia   github:phprus
       Josh Tobin              Neil Bickford      Matthew Gregan       github:poppolopoppo
       Julian Raschke          Gregory Mullen     Christian Floisand   github:darealshinji
       Baldur Karlsson         Kevin Schmidt      JR Smith             github:Michaelangel007
                               Brad Weinberger    Matvey Cherevko      github:mosra
       Luca Sas                Alexander Veselov  Zack Middleton       [reserved]
       Ryan C. Gordon          [reserved]                              [reserved]
                        DO NOT ADD YOUR NAME HERE

                        Jacko Dirks

     To add your name to the credits, pick a random blank space in the middle and fill it.
     80% of merge conflicts on stb PRs are due to people adding their name at the end
     of the credits.
   */

   // DOCUMENTATION
   //
   // Limitations:
   //    - no 12-bit-per-channel JPEG
   //    - no JPEGs with arithmetic coding
   //    - GIF always returns *comp=4
   //
   // Basic usage (see HDR discussion below for HDR usage):
   //    int x,y,n;
   //    BytePtr data = stbi_load(filename, &x, &y, &n, 0);
   //    // ... process data if not NULL ...
   //    // ... x = width, y = height, n = # 8-bit components per pixel ...
   //    // ... replace '0' with '1'..'4' to force that many components per pixel
   //    // ... but 'n' will always be the number that it would have been if you said 0
   //    stbi_image_free(data);
   //
   // Standard parameters:
   //    int *x                 -- outputs image width in pixels
   //    int *y                 -- outputs image height in pixels
   //    int *channels_in_file  -- outputs # of image components in image file
   //    int desired_channels   -- if non-zero, # of image components requested in result
   //
   // The return value from an image loader is an 'BytePtr ' which points
   // to the pixel data, or NULL on an allocation failure or if the image is
   // corrupt or invalid. The pixel data consists of *y scanlines of *x pixels,
   // with each pixel consisting of N interleaved 8-bit components; the first
   // pixel pointed to is top-left-most in the image. There is no padding between
   // image scanlines or between pixels, regardless of format. The number of
   // components N is 'desired_channels' if desired_channels is non-zero, or
   // *channels_in_file otherwise. If desired_channels is non-zero,
   // *channels_in_file has the number of components that _would_ have been
   // output otherwise. E.g. if you set desired_channels to 4, you will always
   // get RGBA output, but you can check *channels_in_file to see if it's trivially
   // opaque because e.g. there were only 3 channels in the source image.
   //
   // An output image with N components has the following components interleaved
   // in this order in each pixel:
   //
   //     N=#comp     components
   //       1           grey
   //       2           grey, alpha
   //       3           red, green, blue
   //       4           red, green, blue, alpha
   //
   // If image loading fails for any reason, the return value will be NULL,
   // and *x, *y, *channels_in_file will be unchanged. The function
   // stbi_failure_reason() can be queried for an extremely brief, end-user
   // unfriendly explanation of why the load failed. Define STBI_NO_FAILURE_STRINGS
   // to avoid compiling these strings at all, and STBI_FAILURE_USERMSG to get slightly
   // more user-friendly ones.
   //
   // Paletted PNG, BMP, GIF, and PIC images are automatically depalettized.
   //
   // To query the width, height and component count of an image without having to
   // decode the full file, you can use the stbi_info family of functions:
   //
   //   int x,y,n,ok;
   //   ok = stbi_info(filename, &x, &y, &n);
   //   // returns ok=1 and sets x, y, n if image is a supported format,
   //   // 0 otherwise.
   //
   // Note that stb_image pervasively uses ints in its public API for sizes,
   // including sizes of memory buffers. This is now part of the API and thus
   // hard to change without causing breakage. As a result, the various image
   // loaders all have certain limits on image size; these differ somewhat
   // by format but generally boil down to either just under 2GB or just under
   // 1GB. When the decoded image would be larger than this, stb_image decoding
   // will fail.
   //
   // Additionally, stb_image will reject image files that have any of their
   // dimensions set to a larger value than the configurable STBI_MAX_DIMENSIONS,
   // which defaults to 2**24 = 16777216 pixels. Due to the above memory limit,
   // the only way to have an image with such dimensions load correctly
   // is for it to have a rather extreme aspect ratio. Either way, the
   // assumption here is that such larger images are likely to be malformed
   // or malicious. If you do need to load an image with individual dimensions
   // larger than that, and it still fits in the overall size limit, you can
   // #define STBI_MAX_DIMENSIONS on your own to be something larger.
   //
   // ===========================================================================
   //
   // UNICODE:
   //
   //   If compiling for Windows and you wish to use Unicode filenames, compile
   //   with
   //       #define STBI_WINDOWS_UTF8
   //   and pass utf8-encoded filenames. Call stbi_convert_wchar_to_utf8 to convert
   //   Windows wchar_t filenames to utf8.
   //
   // ===========================================================================
   //
   // Philosophy
   //
   // stb libraries are designed with the following priorities:
   //
   //    1. easy to use
   //    2. easy to maintain
   //    3. good performance
   //
   // Sometimes I let "good performance" creep up in priority over "easy to maintain",
   // and for best performance I may provide less-easy-to-use APIs that give higher
   // performance, in addition to the easy-to-use ones. Nevertheless, it's important
   // to keep in mind that from the standpoint of you, a client of this library,
   // all you care about is #1 and #3, and stb libraries DO NOT emphasize #3 above all.
   //
   // Some secondary priorities arise directly from the first two, some of which
   // provide more explicit reasons why performance can't be emphasized.
   //
   //    - Portable ("ease of use")
   //    - Small source code footprint ("easy to maintain")
   //    - No dependencies ("ease of use")
   //
   // ===========================================================================
   //
   // I/O callbacks
   //
   // I/O callbacks allow you to read from arbitrary sources, like packaged
   // files or some other source. Data read from callbacks are processed
   // through a small internal buffer (currently 128 bytes) to try to reduce
   // overhead.
   //
   // The three functions you must define are "read" (reads some bytes of data),
   // "skip" (skips some bytes of data), "eof" (reports if the stream is at the end).
   //
   // ===========================================================================
   //
   // SIMD support
   //
   // The JPEG decoder will try to automatically use SIMD kernels on x86 when
   // supported by the compiler. For ARM Neon support, you must explicitly
   // request it.
   //
   // (The old do-it-yourself SIMD API is no longer supported in the current
   // code.)
   //
   // On x86, SSE2 will automatically be used when available based on a run-time
   // test; if not, the generic C versions are used as a fall-back. On ARM targets,
   // the typical path is to have separate builds for NEON and non-NEON devices
   // (at least this is true for iOS and Android). Therefore, the NEON support is
   // toggled by a build flag: define STBI_NEON to get NEON loops.
   //
   // If for some reason you do not want to use any of SIMD code, or if
   // you have issues compiling it, you can disable it entirely by
   // defining STBI_NO_SIMD.
   //
   // ===========================================================================
   //
   // HDR image support   (disable by defining STBI_NO_HDR)
   //
   // stb_image supports loading HDR images in general, and currently the Radiance
   // .HDR file format specifically. You can still load any file through the existing
   // interface; if you attempt to load an HDR file, it will be automatically remapped
   // to LDR, assuming gamma 2.2 and an arbitrary scale factor defaulting to 1;
   // both of these constants can be reconfigured through this interface:
   //
   //     stbi_hdr_to_ldr_gamma(2.2f);
   //     stbi_hdr_to_ldr_scale(1.0f);
   //
   // (note, do not use _inverse_ constants; stbi_image will invert them
   // appropriately).
   //
   // Additionally, there is a new, parallel interface for loading files as
   // (linear) floats to preserve the full dynamic range:
   //
   //    float *data = stbi_loadf(filename, &x, &y, &n, 0);
   //
   // If you load LDR images through this interface, those images will
   // be promoted to floating point values, run through the inverse of
   // constants corresponding to the above:
   //
   //     stbi_ldr_to_hdr_scale(1.0f);
   //     stbi_ldr_to_hdr_gamma(2.2f);
   //
   // Finally, given a filename (or an open file or memory block--see header
   // file for details) containing image data, you can query for the "most
   // appropriate" interface to use (that is, whether the image is HDR or
   // not), using:
   //
   //     stbi_is_hdr(char *filename);
   //
   // ===========================================================================
   //
   // iPhone PNG support:
   //
   // We optionally support converting iPhone-formatted PNGs (which store
   // premultiplied BGRA) back to RGB, even though they're internally encoded
   // differently. To enable this conversion, call
   // stbi_convert_iphone_png_to_rgb(1).
   //
   // Call stbi_set_unpremultiply_on_load(1) as well to force a divide per
   // pixel to remove any premultiplied alpha *only* if the image file explicitly
   // says there's premultiplied data (currently only happens in iPhone images,
   // and only if iPhone convert-to-rgb processing is on).
   //
   // ===========================================================================
   //
   // ADDITIONAL CONFIGURATION
   //
   //  - You can suppress implementation of any of the decoders to reduce
   //    your code footprint by #defining one or more of the following
   //    symbols before creating the implementation.
   //
   //        STBI_NO_JPEG
   //        STBI_NO_PNG
   //        STBI_NO_BMP
   //        STBI_NO_PSD
   //        STBI_NO_TGA
   //        STBI_NO_GIF
   //        STBI_NO_HDR
   //        STBI_NO_PIC
   //        STBI_NO_PNM   (.ppm and .pgm)
   //
   //  - You can request *only* certain decoders and suppress all other ones
   //    (this will be more forward-compatible, as addition of new decoders
   //    doesn't require you to disable them explicitly):
   //
   //        STBI_ONLY_JPEG
   //        STBI_ONLY_PNG
   //        STBI_ONLY_BMP
   //        STBI_ONLY_PSD
   //        STBI_ONLY_TGA
   //        STBI_ONLY_GIF
   //        STBI_ONLY_HDR
   //        STBI_ONLY_PIC
   //        STBI_ONLY_PNM   (.ppm and .pgm)
   //
   //   - If you use STBI_NO_PNG (or _ONLY_ without PNG), and you still
   //     want the zlib decoder to be available, #define STBI_SUPPORT_ZLIB
   //
   //  - If you define STBI_MAX_DIMENSIONS, stb_image will reject images greater
   //    than that size (in either width or height) without further processing.
   //    This is to let programs in the wild set an upper bound to prevent
   //    denial-of-service attacks on untrusted data, as one could generate a
   //    valid image of gigantic dimensions and force stb_image to allocate a
   //    huge block of memory and spend disproportionate time decoding it. By
   //    default this is set to (1 << 24), which is 16777216, but that's still
   //    very big.

   const int STBI_VERSION = 1;

   public enum STBI_CHANNELS : int
   {
      _default = 0, // only used for desired_channels
      grey = 1,
      grey_alpha = 2,
      rgb = 3,
      rgb_alpha = 4
   }

   //////////////////////////////////////////////////////////////////////////////
   //
   // PRIMARY API - works on images of any type
   //

   //
   // load image by filename, open file, or memory buffer
   //

   public struct stbi_io_callbacks
   {
      public delegate int read_delegate(Span<byte> data, int size);
      public delegate void skip_delegate(int n);
      public delegate bool eof_delegate();

      public read_delegate? read;   // fill 'data' with 'size' bytes.  return number of bytes actually read
      public skip_delegate? skip;   // skip the next 'n' bytes, or 'unget' the last -n bytes if negative
      public eof_delegate? eof;                       // returns nonzero if we are at end of file/data
   }

   ////////////////////////////////////
   //
   // 8-bits-per-channel interface
   //

   static public BytePtr stbi_load_from_memory(BytePtr buffer, int len, out int x, out int y, out STBI_CHANNELS channels_in_file, STBI_CHANNELS desired_channels)
   {
      stbi__context s = new stbi__context();
      stbi__start_mem(ref s, buffer, len);
      return stbi__load_and_postprocess_8bit(ref s, out x, out y, out channels_in_file, desired_channels);
   }


   static public BytePtr stbi_load_from_callbacks(ref stbi_io_callbacks clbk, out int x, out int y, out STBI_CHANNELS channels_in_file, STBI_CHANNELS desired_channels)
   {
      stbi__context s = new stbi__context();
      stbi__start_callbacks(ref s, ref clbk);
      return stbi__load_and_postprocess_8bit(ref s, out x, out y, out channels_in_file, desired_channels);
   }

#if !STBI_NO_GIF
static public BytePtr stbi_load_gif_from_memory(BytePtr buffer, int len, out int[] delays, out int x, out int y, out int z, out int comp, int req_comp);
#endif

   // #if STBI_WINDOWS_UTF8
   // static public int stbi_convert_wchar_to_utf8(char *buffer, size_t bufferlen, const wchar_t* input);
   // #endif

   ////////////////////////////////////
   //
   // 16-bits-per-channel interface
   //

   static public Ptr<stbi_us> stbi_load_16_from_memory(BytePtr buffer, int len, out int x, out int y, out STBI_CHANNELS channels_in_file, STBI_CHANNELS desired_channels)
   {
      stbi__context s = new stbi__context();
      stbi__start_mem(ref s, buffer, len);
      return stbi__load_and_postprocess_16bit(ref s, out x, out y, out channels_in_file, desired_channels);
   }

   static public Ptr<stbi_us> stbi_load_16_from_callbacks(ref stbi_io_callbacks clbk, out int x, out int y, out STBI_CHANNELS channels_in_file, STBI_CHANNELS desired_channels)
   {
      stbi__context s = new stbi__context();
      stbi__start_callbacks(ref s, ref clbk);
      return stbi__load_and_postprocess_16bit(ref s, out x, out y, out channels_in_file, desired_channels);
   }

#if !STBI_NO_STDIO
static public stbi_us *stbi_load_16          (BytePtr filename, out int x, out int y, out int channels_in_file, int desired_channels);
static public stbi_us *stbi_load_from_file_16(FILE *f, out int x, out int y, out int channels_in_file, int desired_channels);
#endif

   ////////////////////////////////////
   //
   // float-per-channel interface
   //
#if !STBI_NO_LINEAR
   static public float *stbi_loadf_from_memory     (stbi_uc const *buffer, int len, out int x, out int y, out int channels_in_file, int desired_channels);
   static public float *stbi_loadf_from_callbacks  (stbi_io_callbacks const *clbk, void *user, int *x, int *y,  int *channels_in_file, int desired_channels);

#if !STBI_NO_STDIO
   static public float *stbi_loadf            (BytePtr filename, out int x, out int y, out int channels_in_file, int desired_channels);
   static public float *stbi_loadf_from_file  (FILE *f, out int x, out int y, out int channels_in_file, int desired_channels);
#endif
#endif

#if !STBI_NO_HDR
   static public void   stbi_hdr_to_ldr_gamma(float gamma);
   static public void   stbi_hdr_to_ldr_scale(float scale);
#endif // STBI_NO_HDR

#if !STBI_NO_LINEAR
   static public void   stbi_ldr_to_hdr_gamma(float gamma);
   static public void   stbi_ldr_to_hdr_scale(float scale);
#endif // STBI_NO_LINEAR

   // stbi_is_hdr is always defined, but always returns false if STBI_NO_HDR
   static public bool stbi_is_hdr_from_callbacks(ref stbi_io_callbacks clbk)
   {
#if !STBI_NO_HDR
   stbi__context s;
   stbi__start_callbacks(&s, (stbi_io_callbacks *) clbk, user);
   return stbi__hdr_test(&s);
#else
      //STBI_NOTUSED(clbk);
      //STBI_NOTUSED(user);
      return false;
#endif
   }

   static public bool stbi_is_hdr_from_memory(BytePtr buffer, int len)
   {
#if !STBI_NO_HDR
   stbi__context s;
   stbi__start_mem(&s,buffer,len);
   return stbi__hdr_test(&s);
#else
      //STBI_NOTUSED(buffer);
      //STBI_NOTUSED(len);
      return false;
#endif
   }


#if !STBI_NO_STDIO
static public bool      stbi_is_hdr          (string filename);
static public bool      stbi_is_hdr_from_file(FILE *f);
#endif // STBI_NO_STDIO

   // get a VERY brief reason for failure
   // on most compilers (and ALL modern mainstream compilers) this is threadsafe
   static string stbi__g_failure_reason = "";

   static public string stbi_failure_reason()
   {
      return stbi__g_failure_reason;
   }

#if !STBI_NO_FAILURE_STRINGS
   static int stbi__err(string str)
   {
      stbi__g_failure_reason = str;
      return 0;
   }
#endif

   // stbi__err - error
   // stbi__errpf - error returning pointer to float
   // stbi__errpuc - error returning pointer to byte

#if STBI_NO_FAILURE_STRINGS
static int stbi__err(string x, string y) => 0;
#elif (STBI_FAILURE_USERMSG)
static int  stbi__err(string x,string y) => stbi__err(y);
#else
   static int stbi__err(string x, string y) => stbi__err(x);
#endif

   static Ptr<float> stbi__errpf(string x, string y)
   {
      stbi__err(x, y);
      return Ptr<float>.Null;
   }
   static BytePtr stbi__errpuc(string x, string y)
   {
      stbi__err(x, y);
      return BytePtr.Null;
   }

   static Ptr<T> stbi__errpuc<T>(string x, string y)
   {
      stbi__err(x, y);
      return Ptr<T>.Null;
   }

   // free the loaded image -- this is just free()
   static public void stbi_image_free(BytePtr retval_from_stbi_load)
   {
      //STBI_FREE(retval_from_stbi_load);
   }

   // get image dimensions & components without fully decoding
   static public bool stbi_info_from_memory(BytePtr buffer, int len, out int x, out int y, out STBI_CHANNELS comp)
   {
      stbi__context s = new stbi__context();
      stbi__start_mem(ref s, buffer, len);
      return stbi__info_main(ref s, out x, out y, out comp);
   }

   static public bool stbi_info_from_callbacks(ref stbi_io_callbacks c, out int x, out int y, out STBI_CHANNELS comp)
   {
      stbi__context s = new stbi__context();
      stbi__start_callbacks(ref s, ref c);
      return stbi__info_main(ref s, out x, out y, out comp);
   }

   static public bool stbi_is_16_bit_from_memory(BytePtr buffer, int len)
   {
      stbi__context s = new stbi__context();
      stbi__start_mem(ref s, buffer, len);
      return stbi__is_16_main(ref s);
   }

   static public bool stbi_is_16_bit_from_callbacks(ref stbi_io_callbacks c)
   {
      stbi__context s = new stbi__context();
      stbi__start_callbacks(ref s, ref c);
      return stbi__is_16_main(ref s);
   }

#if !STBI_NO_STDIO
static public int      stbi_info               (BytePtr filename,     int *x, int *y, int *comp);
static public int      stbi_info_from_file     (FILE *f,                  int *x, int *y, int *comp);
static public int      stbi_is_16_bit          (BytePtr filename);
static public int      stbi_is_16_bit_from_file(FILE *f);
#endif

   // for image formats that explicitly notate that they have premultiplied alpha,
   // we just return the colors as stored in the file. set this flag to force
   // unpremultiplication. results are undefined if the unpremultiply overflow.
   static public void stbi_set_unpremultiply_on_load(bool flag_true_if_should_unpremultiply)
   {
      stbi__unpremultiply_on_load = flag_true_if_should_unpremultiply;
   }

   // indicate whether we should process iphone images back to canonical format,
   // or just pass them through "as-is"
   static public void stbi_convert_iphone_png_to_rgb(bool flag_true_if_should_convert)
   {
      stbi__de_iphone_flag = flag_true_if_should_convert;
   }

   // flip the image vertically, so the first pixel in the output array is the bottom left
   static public void stbi_set_flip_vertically_on_load(bool flag_true_if_should_flip)
   {
      stbi__vertically_flip_on_load = flag_true_if_should_flip;

   }

   static bool stbi__unpremultiply_on_load = false;
   static bool stbi__de_iphone_flag = false;
   static bool stbi__vertically_flip_on_load = false;


   // as above, but only applies to images loaded on the thread that calls the function
   // this function is only available if your compiler supports thread-local variables;
   // calling it will fail to link if your compiler doesn't
   // static public void stbi_set_unpremultiply_on_load_thread(bool flag_true_if_should_unpremultiply);
   // static public void stbi_convert_iphone_png_to_rgb_thread(bool flag_true_if_should_convert);
   // static public void stbi_set_flip_vertically_on_load_thread(bool flag_true_if_should_flip);

   // ZLIB client - used by PNG, available for other purposes


   static public BytePtr stbi_zlib_decode_malloc(BytePtr buffer, int len, out int outlen)
   {
      return stbi_zlib_decode_malloc_guesssize(buffer, len, 16384, out outlen);
   }

   static BytePtr stbi_zlib_decode_malloc_guesssize_headerflag(BytePtr buffer, int len, int initial_size, out int outlen, bool parse_header)
   {
      stbi__zbuf a = new stbi__zbuf();
      BytePtr p = stbi__malloc(initial_size);
      if (p.IsNull)
      {
         outlen = 0;
         return BytePtr.Null;
      }
      a.zbuffer = buffer;
      a.zbuffer_end = buffer + len;
      if (stbi__do_zlib(ref a, p, initial_size, true, parse_header))
      {
         outlen = (int)(a.zout - a.zout_start).Offset;
         return a.zout_start;
      }
      else
      {
         STBI_FREE(a.zout_start);
         outlen = 0;
         return BytePtr.Null;
      }
   }

   static public int stbi_zlib_decode_buffer(BytePtr obuffer, int olen, BytePtr ibuffer, int ilen)
   {
      stbi__zbuf a = new stbi__zbuf();
      a.zbuffer = ibuffer;
      a.zbuffer_end = ibuffer + ilen;
      if (stbi__do_zlib(ref a, obuffer, olen, false, true))
         return (int)(a.zout - a.zout_start).Offset;
      else
         return -1;
   }

   static public BytePtr stbi_zlib_decode_noheader_malloc(BytePtr buffer, int len, out int outlen)
   {
      stbi__zbuf a = new stbi__zbuf();
      BytePtr p = (BytePtr)stbi__malloc(16384);
      if (p.IsNull)
      {
         outlen = 0;
         return BytePtr.Null;
      }
      a.zbuffer = (BytePtr)buffer;
      a.zbuffer_end = (BytePtr)buffer + len;
      if (stbi__do_zlib(ref a, p, 16384, true, false))
      {
         outlen = (int)(a.zout - a.zout_start).Offset;
         return a.zout_start;
      }
      else
      {
         STBI_FREE(a.zout_start);
         outlen = 0;
         return BytePtr.Null;
      }
   }

   static public int stbi_zlib_decode_noheader_buffer(BytePtr obuffer, int olen, BytePtr ibuffer, int ilen)
   {
      stbi__zbuf a = new stbi__zbuf();
      a.zbuffer = (BytePtr)ibuffer;
      a.zbuffer_end = (BytePtr)ibuffer + ilen;
      if (stbi__do_zlib(ref a, obuffer, olen, false, false))
         return (int)(a.zout - a.zout_start).Offset;
      else
         return -1;
   }

   //
   //
   ////   end header file   /////////////////////////////////////////////////////

   [Conditional("DEBUG")]
   static private void STBI_ASSERT([DoesNotReturnIf(false)] bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
   {
      Debug.Assert(condition, message, string.Empty);
   }


   // should produce compiler error if size is wrong
   // typedef byte validate_uint32[sizeof(stbi__uint32)==4 ? 1 : -1];

   // #if _MSC_VER
   // #define STBI_NOTUSED(v)  (void)(v)
   // #else
   // #define STBI_NOTUSED(v)  (void)sizeof(v)
   // #endif
   // 
   // #if _MSC_VER
   // #define STBI_HAS_LROTL
   // #endif

   // #if STBI_HAS_LROTL
   //    #define stbi_lrot(x,y)  _lrotl(x,y)
   // #else
   // static private uint stbi_lrot(uint x,uint y)  => (((x) << (y)) | ((x) >> (-(y) & 31)));
   // C# 11 added logical right shift for uints !
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static private uint stbi_lrot(uint x, int y) => x >>> y;
   // #endif

   // #if (STBI_MALLOC) && (STBI_FREE) && ((STBI_REALLOC) || (STBI_REALLOC_SIZED))
   // ok
   // #elif !(STBI_MALLOC) && !(STBI_FREE) && !(STBI_REALLOC) && !(STBI_REALLOC_SIZED)
   // ok
   // #else
   // #error "Must define all or none of STBI_MALLOC, STBI_FREE, and STBI_REALLOC (or STBI_REALLOC_SIZED)."
   // #endif

   // #if !STBI_MALLOC
   // #define STBI_MALLOC(sz)           malloc(sz)
   // #define STBI_REALLOC(p,newsz)     realloc(p,newsz)
   // #define STBI_FREE(p)              free(p)
   // #endif

   // #if !STBI_REALLOC_SIZED
   // #define STBI_REALLOC_SIZED(p,oldsz,newsz) STBI_REALLOC(p,newsz)
   // #endif

   // x86/x64 detection
   // #if (__x86_64__) || (_M_X64)
   // #define STBI__X64_TARGET
   // #elif (__i386) || (_M_IX86)
   // #define STBI__X86_TARGET
   // #endif

   // #if (__GNUC__) && (STBI__X86_TARGET) && !(__SSE2__) && !(STBI_NO_SIMD)
   // gcc doesn't support sse2 intrinsics unless you compile with -msse2,
   // which in turn means it gets to use SSE2 everywhere. This is unfortunate,
   // but previous attempts to provide the SSE2 functions with runtime
   // detection caused numerous issues. The way architecture extensions are
   // exposed in GCC/Clang is, sadly, not really suited for one-file libs.
   // New behavior: if compiled with -msse2, we use SSE2 without any
   // detection; if not, we don't use it at all.
   // #define STBI_NO_SIMD
   // #endif

   // #if (__MINGW32__) && (STBI__X86_TARGET) && !(STBI_MINGW_ENABLE_SSE2) && !(STBI_NO_SIMD)
   // Note that __MINGW32__ doesn't actually mean 32-bit, so we have to avoid STBI__X64_TARGET
   //
   // 32-bit MinGW wants ESP to be 16-byte aligned, but this is not in the
   // Windows ABI and VC++ as well as Windows DLLs don't maintain that invariant.
   // As a result, enabling SSE2 on 32-bit MinGW is dangerous when not
   // simultaneously enabling "-mstackrealign".
   //
   // See https://github.com/nothings/stb/issues/81 for more information.
   //
   // So default to no SSE2 on 32-bit MinGW. If you've read this far and added
   // -mstackrealign to your build settings, feel free to #define STBI_MINGW_ENABLE_SSE2.
   // #define STBI_NO_SIMD
   // #endif

   // #if !STBI_NO_SIMD) && ((STBI__X86_TARGET) || (STBI__X64_TARGET))
   // #define STBI_SSE2
   // #include <emmintrin.h>

   // #if _MSC_VER
   // 
   // #if _MSC_VER >= 1400  // not VC6
   // #include <intrin.h> // __cpuid
   // static int stbi__cpuid3(void)
   // {
   //    int info[4];
   //    __cpuid(info,1);
   //    return info[3];
   // }
   // #else
   // static int stbi__cpuid3(void)
   // {
   //    int res;
   //    __asm {
   //       mov  eax,1
   //       cpuid
   //       mov  res,edx
   //    }
   //    return res;
   // }
   // #endif

   // #define STBI_SIMD_ALIGN(type, name) __declspec(align(16)) type name
   // 
   // #if !(STBI_NO_JPEG) && (STBI_SSE2)
   // static int stbi__sse2_available(void)
   // {
   //    int info3 = stbi__cpuid3();
   //    return ((info3 >> 26) & 1) != 0;
   // }
   // #endif

   // #else // assume GCC-style if not VC++
   // #define STBI_SIMD_ALIGN(type, name) type name __attribute__((aligned(16)))
   // 
   // #if !(STBI_NO_JPEG) && (STBI_SSE2)
   // static int stbi__sse2_available(void)
   // {
   //    // If we're even attempting to compile this on GCC/Clang, that means
   //    // -msse2 is on, which means the compiler is allowed to use SSE2
   //    // instructions at will, and so are we.
   //    return 1;
   // }
   // #endif
   // 
   // #endif
   // #endif

   // ARM NEON
   // #if (STBI_NO_SIMD) && (STBI_NEON)
   // #undef STBI_NEON
   // #endif
   // 
   // #if STBI_NEON
   // #include <arm_neon.h>
   // #if _MSC_VER
   // #define STBI_SIMD_ALIGN(type, name) __declspec(align(16)) type name
   // #else
   // #define STBI_SIMD_ALIGN(type, name) type name __attribute__((aligned(16)))
   // #endif
   // #endif
   // 
   // #if !STBI_SIMD_ALIGN
   // #define STBI_SIMD_ALIGN(type, name) type name
   // #endif
   // 

   const int STBI_MAX_DIMENSIONS = (1 << 24);

   ///////////////////////////////////////////////
   //
   //  stbi__context struct and start_xxx functions

   // stbi__context structure is our basic context used by all images, so it
   // contains all the IO context, plus some basic image information
   struct stbi__context
   {
      public stbi__uint32 img_x, img_y;
      public STBI_CHANNELS img_n, img_out_n;

      public stbi_io_callbacks io;
      //public void *io_user_data;

      public bool read_from_callbacks;
      public int buflen;
      public BytePtr buffer_start = new BytePtr(new byte[128]);
      public int callback_already_read;

      public BytePtr img_buffer, img_buffer_end;
      public BytePtr img_buffer_original, img_buffer_original_end;

      public stbi__context()
      {
      }
   }

   static void stbi__refill_buffer(ref stbi__context s)
   {
      Debug.Assert(s.io.read != null);
      int n = s.io.read(s.buffer_start, s.buflen);
      s.callback_already_read += (s.img_buffer - s.img_buffer_original).Offset;
      if (n == 0)
      {
         // at end of file, treat same as if from memory, but need to handle case
         // where s.img_buffer isn't pointing to safe memory, e.g. 0-byte file
         s.read_from_callbacks = false;
         s.img_buffer = s.buffer_start;
         s.img_buffer_end = s.buffer_start + 1;
         s.img_buffer.Ref = 0;
      }
      else
      {
         s.img_buffer = s.buffer_start;
         s.img_buffer_end = s.buffer_start + n;
      }
   }

   // initialize a memory-decode context
   static void stbi__start_mem(ref stbi__context s, BytePtr buffer, int len)
   {
      s.io.read = null;
      s.read_from_callbacks = false;
      s.callback_already_read = 0;
      s.img_buffer = s.img_buffer_original = buffer;
      s.img_buffer_end = s.img_buffer_original_end = buffer + len;
   }

   // initialize a callback-based context
   static void stbi__start_callbacks(ref stbi__context s, ref stbi_io_callbacks c)
   {
      s.io = c;
      //s.io_user_data = user;
      s.buflen = s.buffer_start.Length;
      s.read_from_callbacks = true;
      s.callback_already_read = 0;
      s.img_buffer = s.img_buffer_original = s.buffer_start;
      stbi__refill_buffer(ref s);
      s.img_buffer_original_end = s.img_buffer_end;
   }

#if !STBI_NO_STDIO

static int stbi__stdio_read(void *user, char *data, int size)
{
   return (int) fread(data,1,size,(FILE*) user);
}

static void stbi__stdio_skip(void *user, int n)
{
   int ch;
   fseek((FILE*) user, n, SEEK_CUR);
   ch = fgetc((FILE*) user);  /* have to read a byte to reset feof()'s flag */
   if (ch != EOF) {
      ungetc(ch, (FILE *) user);  /* push byte back onto stream if valid. */
   }
}

static int stbi__stdio_eof(void *user)
{
   return feof((FILE*) user) || ferror((FILE *) user);
}

static stbi_io_callbacks stbi__stdio_callbacks =
{
   stbi__stdio_read,
   stbi__stdio_skip,
   stbi__stdio_eof,
};

static void stbi__start_file(ref stbi__context s, FILE *f)
{
   stbi__start_callbacks(s, &stbi__stdio_callbacks, (void *) f);
}

//static void stop_file(ref stbi__context s) { }

#endif // !STBI_NO_STDIO

   static void stbi__rewind(ref stbi__context s)
   {
      // conceptually rewind SHOULD rewind to the beginning of the stream,
      // but we just rewind to the beginning of the initial buffer, because
      // we only use it after doing 'test', which only ever looks at at most 92 bytes
      s.img_buffer = s.img_buffer_original;
      s.img_buffer_end = s.img_buffer_original_end;
   }

   enum STBI_ORDER
   {
      RGB,
      BGR
   }

   struct stbi__result_info
   {
      public int bits_per_channel;
      public int num_channels;
      public STBI_ORDER channel_order;
   }

#if !STBI_NO_JPEG
   static bool stbi__jpeg_test(ref stbi__context s)
   {
      bool r;
      stbi__jpeg j = new stbi__jpeg(ref s);
      stbi__setup_jpeg(ref j);
      r = stbi__decode_jpeg_header(ref j, STBI__SCAN.type);
      stbi__rewind(ref s);
      return r;
   }

   static BytePtr stbi__jpeg_load(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri)
   {
      BytePtr result;
      stbi__jpeg j = new stbi__jpeg(ref s);
      //STBI_NOTUSED(ri);
      stbi__setup_jpeg(ref j);
      result = load_jpeg_image(ref j, out x, out y, out comp, req_comp);
      return result;
   }

   static bool stbi__jpeg_info(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp)
   {
      bool result;
      stbi__jpeg j = new stbi__jpeg(ref s);
      result = stbi__jpeg_info_raw(ref j, out x, out y, out comp);
      return result;
   }

#endif

#if !STBI_NO_PNG
   static BytePtr stbi__png_load(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri)
   {
      stbi__png p = new stbi__png();
      p.s = s;
      var toReturn = stbi__do_png(ref p, out x, out y, out comp, req_comp, ref ri);
      s = p.s;
      return toReturn;
   }

   static bool stbi__png_test(ref stbi__context s)
   {
      bool r;
      r = stbi__check_png_header(ref s);
      stbi__rewind(ref s);
      return r;
   }


   static bool stbi__png_info(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp)
   {
      stbi__png p = new stbi__png();
      p.s = s;
      var toReturn = stbi__png_info_raw(ref p, out x, out y, out comp);
      s = p.s;
      return toReturn;
   }

   static bool stbi__png_is16(ref stbi__context s)
   {
      stbi__png p = new stbi__png();
      p.s = s;
      if (!stbi__png_info_raw(ref p, out _, out _, out _))
      {
         s = p.s;
         return false;
      }
      if (p.depth != 16)
      {
         stbi__rewind(ref p.s);
         s = p.s;
         return false;
      }
      s = p.s;
      return true;
   }
#endif

#if !STBI_NO_BMP
   static bool stbi__bmp_test(ref stbi__context s)
   {
      bool r = stbi__bmp_test_raw(ref s);
      stbi__rewind(ref s);
      return r;
   }

   static BytePtr stbi__bmp_load(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri)
   {
      BytePtr _out;
      uint mr = 0, mg = 0, mb = 0, ma = 0, all_a;
      stbi_uc[,] pal = new stbi_uc[256, 4];
      int psize = 0, i, j, width;
      bool flip_vertically;
      int pad;
      STBI_CHANNELS target;
      stbi__bmp_data info = new stbi__bmp_data();
      //STBI_NOTUSED(ri);

      x = y = 0;
      comp = STBI_CHANNELS._default;

      info.all_a = 255;
      if (!stbi__bmp_parse_header(ref s, ref info))
         return BytePtr.Null; // error code already set

      flip_vertically = ((int)s.img_y) > 0;
      s.img_y = (uint)Math.Abs((int)s.img_y);

      if (s.img_y > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large", "Very large image (corrupt?)");
      if (s.img_x > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large", "Very large image (corrupt?)");

      mr = info.mr;
      mg = info.mg;
      mb = info.mb;
      ma = info.ma;
      all_a = info.all_a;

      if (info.hsz == 12)
      {
         if (info.bpp < 24)
            psize = (info.offset - info.extra_read - 24) / 3;
      }
      else
      {
         if (info.bpp < 16)
            psize = (info.offset - info.extra_read - info.hsz) >> 2;
      }
      if (psize == 0)
      {
         // accept some number of extra bytes after the header, but if the offset points either to before
         // the header ends or implies a large amount of extra data, reject the file as malformed
         int bytes_read_so_far = s.callback_already_read + (int)(s.img_buffer - s.img_buffer_original).Offset;
         int header_limit = 1024; // max we actually read is below 256 bytes currently.
         int extra_data_limit = 256 * 4; // what ordinarily goes here is a palette; 256 entries*4 bytes is its max size.
         if (bytes_read_so_far <= 0 || bytes_read_so_far > header_limit)
         {
            return stbi__errpuc("bad header", "Corrupt BMP");
         }
         // we established that bytes_read_so_far is positive and sensible.
         // the first half of this test rejects offsets that are either too small positives, or
         // negative, and guarantees that info.offset >= bytes_read_so_far > 0. this in turn
         // ensures the number computed in the second half of the test can't overflow.
         if (info.offset < bytes_read_so_far || info.offset - bytes_read_so_far > extra_data_limit)
         {
            return stbi__errpuc("bad offset", "Corrupt BMP");
         }
         else
         {
            stbi__skip(ref s, info.offset - bytes_read_so_far);
         }
      }

      if (info.bpp == 24 && ma == 0xff000000)
         s.img_n = STBI_CHANNELS.rgb;
      else
         s.img_n = ma != 0 ? STBI_CHANNELS.rgb_alpha : STBI_CHANNELS.rgb;
      if (req_comp != 0 && (int)req_comp >= 3) // we can directly decode 3 or 4
         target = req_comp;
      else
         target = s.img_n; // if they want monochrome, we'll post-convert

      // sanity-check size
      if (!stbi__mad3sizes_valid((int)target, (int)s.img_x, (int)s.img_y, 0))
         return stbi__errpuc("too large", "Corrupt BMP");

      _out = (BytePtr)stbi__malloc_mad3((int)target, (int)s.img_x, (int)s.img_y, 0);
      if (_out.IsNull) return stbi__errpuc("outofmem", "Out of memory");
      if (info.bpp < 16)
      {
         int z = 0;
         if (psize == 0 || psize > 256) { STBI_FREE(_out); return stbi__errpuc("invalid", "Corrupt BMP"); }
         for (i = 0; i < psize; ++i)
         {
            pal[i, 2] = stbi__get8(ref s);
            pal[i, 1] = stbi__get8(ref s);
            pal[i, 0] = stbi__get8(ref s);
            if (info.hsz != 12) stbi__get8(ref s);
            pal[i, 3] = 255;
         }
         stbi__skip(ref s, info.offset - info.extra_read - info.hsz - psize * (info.hsz == 12 ? 3 : 4));
         if (info.bpp == 1) width = (int)((s.img_x + 7) >> 3);
         else if (info.bpp == 4) width = (int)((s.img_x + 1) >> 1);
         else if (info.bpp == 8) width = (int)s.img_x;
         else { STBI_FREE(_out); return stbi__errpuc("bad bpp", "Corrupt BMP"); }
         pad = (-width) & 3;
         if (info.bpp == 1)
         {
            for (j = 0; j < (int)s.img_y; ++j)
            {
               int bit_offset = 7, v = stbi__get8(ref s);
               for (i = 0; i < (int)s.img_x; ++i)
               {
                  int color = (v >> bit_offset) & 0x1;
                  _out[z++].Ref = pal[color, 0];
                  _out[z++].Ref = pal[color, 1];
                  _out[z++].Ref = pal[color, 2];
                  if (target == STBI_CHANNELS.rgb_alpha) _out[z++].Ref = 255;
                  if (i + 1 == (int)s.img_x) break;
                  if ((--bit_offset) < 0)
                  {
                     bit_offset = 7;
                     v = stbi__get8(ref s);
                  }
               }
               stbi__skip(ref s, pad);
            }
         }
         else
         {
            for (j = 0; j < (int)s.img_y; ++j)
            {
               for (i = 0; i < (int)s.img_x; i += 2)
               {
                  int v = stbi__get8(ref s), v2 = 0;
                  if (info.bpp == 4)
                  {
                     v2 = v & 15;
                     v >>= 4;
                  }
                  _out[z++].Ref = pal[v, 0];
                  _out[z++].Ref = pal[v, 1];
                  _out[z++].Ref = pal[v, 2];
                  if (target == STBI_CHANNELS.rgb_alpha) _out[z++].Ref = 255;
                  if (i + 1 == (int)s.img_x) break;
                  v = (info.bpp == 8) ? stbi__get8(ref s) : v2;
                  _out[z++].Ref = pal[v, 0];
                  _out[z++].Ref = pal[v, 1];
                  _out[z++].Ref = pal[v, 2];
                  if (target == STBI_CHANNELS.rgb_alpha) _out[z++].Ref = 255;
               }
               stbi__skip(ref s, pad);
            }
         }
      }
      else
      {
         int rshift = 0, gshift = 0, bshift = 0, ashift = 0, rcount = 0, gcount = 0, bcount = 0, acount = 0;
         int z = 0;
         int easy = 0;
         stbi__skip(ref s, info.offset - info.extra_read - info.hsz);
         if (info.bpp == 24) width = (int)(3 * s.img_x);
         else if (info.bpp == 16) width = (int)(2 * s.img_x);
         else /* bpp = 32 and pad = 0 */ width = 0;
         pad = (-width) & 3;
         if (info.bpp == 24)
         {
            easy = 1;
         }
         else if (info.bpp == 32)
         {
            if (mb == 0xff && mg == 0xff00 && mr == 0x00ff0000 && ma == 0xff000000)
               easy = 2;
         }
         if (easy == 0)
         {
            if (mr == 0 || mg == 0 || mb == 0) { STBI_FREE(_out); return stbi__errpuc("bad masks", "Corrupt BMP"); }
            // right shift amt to put high bit in position #7
            rshift = stbi__high_bit(mr) - 7; rcount = stbi__bitcount(mr);
            gshift = stbi__high_bit(mg) - 7; gcount = stbi__bitcount(mg);
            bshift = stbi__high_bit(mb) - 7; bcount = stbi__bitcount(mb);
            ashift = stbi__high_bit(ma) - 7; acount = stbi__bitcount(ma);
            if (rcount > 8 || gcount > 8 || bcount > 8 || acount > 8)
            {
               // federicodangelo: try to support masks with more than 8 bits:
               // This simple conversion seems to do the trick... at least on the only test image with this format that I found
               rcount = Math.Min(rcount, 8);
               gcount = Math.Min(gcount, 8);
               bcount = Math.Min(bcount, 8);
               acount = Math.Min(acount, 8);
               //STBI_FREE(_out); return stbi__errpuc("bad masks", "Corrupt BMP"); 
            }
         }
         for (j = 0; j < (int)s.img_y; ++j)
         {
            if (easy != 0)
            {
               for (i = 0; i < (int)s.img_x; ++i)
               {
                  byte a;
                  _out[z + 2].Ref = stbi__get8(ref s);
                  _out[z + 1].Ref = stbi__get8(ref s);
                  _out[z + 0].Ref = stbi__get8(ref s);
                  z += 3;
                  a = (easy == 2 ? stbi__get8(ref s) : (byte)255);
                  all_a |= a;
                  if (target == STBI_CHANNELS.rgb_alpha) _out[z++].Ref = a;
               }
            }
            else
            {
               int bpp = info.bpp;
               for (i = 0; i < (int)s.img_x; ++i)
               {
                  stbi__uint32 v = (bpp == 16 ? (stbi__uint32)stbi__get16le(ref s) : stbi__get32le(ref s));
                  uint a;
                  _out[z++].Ref = STBI__BYTECAST(stbi__shiftsigned(v & mr, rshift, rcount));
                  _out[z++].Ref = STBI__BYTECAST(stbi__shiftsigned(v & mg, gshift, gcount));
                  _out[z++].Ref = STBI__BYTECAST(stbi__shiftsigned(v & mb, bshift, bcount));
                  a = (ma != 0 ? (byte)stbi__shiftsigned(v & ma, ashift, acount) : (byte)255);
                  all_a |= a;
                  if (target == STBI_CHANNELS.rgb_alpha) _out[z++].Ref = STBI__BYTECAST((int)a);
               }
            }
            stbi__skip(ref s, pad);
         }
      }

      // if alpha channel is all 0s, replace with all 255s
      if (target == STBI_CHANNELS.rgb_alpha && all_a == 0)
         for (i = 4 * (int)s.img_x * (int)s.img_y - 1; i >= 0; i -= 4)
            _out[i].Ref = 255;

      if (flip_vertically)
      {
         stbi_uc t;
         for (j = 0; j < (int)s.img_y >> 1; ++j)
         {
            BytePtr p1 = _out + j * (int)s.img_x * (int)target;
            BytePtr p2 = _out + ((int)s.img_y - 1 - j) * (int)s.img_x * (int)target;
            for (i = 0; i < (int)s.img_x * (int)target; ++i)
            {
               t = p1[i].Value; p1[i].Ref = p2[i].Value; p2[i].Ref = t;
            }
         }
      }

      if (req_comp != 0 && req_comp != target)
      {
         _out = stbi__convert_format(_out, target, req_comp, (int)s.img_x, (int)s.img_y);
         if (_out.IsNull) return _out; // stbi__convert_format frees input on failure
      }

      x = (int)s.img_x;
      y = (int)s.img_y;
      comp = s.img_n;
      return _out;
   }

   static bool stbi__bmp_info(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp)
   {
      stbi__bmp_data info = new stbi__bmp_data();

      info.all_a = 255;
      if (!stbi__bmp_parse_header(ref s, ref info))
      {
         stbi__rewind(ref s);
         x = y = 0;
         comp = 0;
         return false;
      }
      x = (int)s.img_x;
      y = (int)s.img_y;
      if (info.bpp == 24 && info.ma == 0xff000000)
         comp = STBI_CHANNELS.rgb;
      else
         comp = info.ma != 0 ? STBI_CHANNELS.rgb_alpha : STBI_CHANNELS.rgb;
      return true;
   }

#endif

#if !STBI_NO_TGA
   static bool stbi__tga_test(ref stbi__context s)
   {
      bool res = false;
      int sz, tga_color_type;
      stbi__get8(ref s);      //   discard Offset
      tga_color_type = stbi__get8(ref s);   //   color type
      if (tga_color_type > 1) goto errorEnd;   //   only RGB or indexed allowed
      sz = stbi__get8(ref s);   //   image type
      if (tga_color_type == 1)
      { // colormapped (paletted) image
         if (sz != 1 && sz != 9) goto errorEnd; // colortype 1 demands image type 1 or 9
         stbi__skip(ref s, 4);       // skip index of first colormap entry and number of entries
         sz = stbi__get8(ref s);    //   check bits per palette color entry
         if ((sz != 8) && (sz != 15) && (sz != 16) && (sz != 24) && (sz != 32)) goto errorEnd;
         stbi__skip(ref s, 4);       // skip image x and y origin
      }
      else
      { // "normal" image w/o colormap
         if ((sz != 2) && (sz != 3) && (sz != 10) && (sz != 11)) goto errorEnd; // only RGB or grey allowed, +/- RLE
         stbi__skip(ref s, 9); // skip colormap specification and image x/y origin
      }
      if (stbi__get16le(ref s) < 1) goto errorEnd;      //   test width
      if (stbi__get16le(ref s) < 1) goto errorEnd;      //   test height
      sz = stbi__get8(ref s);   //   bits per pixel
      if ((tga_color_type == 1) && (sz != 8) && (sz != 16)) goto errorEnd; // for colormapped images, bpp is size of an index
      if ((sz != 8) && (sz != 15) && (sz != 16) && (sz != 24) && (sz != 32)) goto errorEnd;

      res = true; // if we got this far, everything's good and we can return 1 instead of 0

   errorEnd:
      stbi__rewind(ref s);
      return res;
   }

   static BytePtr stbi__tga_load(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri)
   {
      //   read in the TGA header stuff
      int tga_offset = stbi__get8(ref s);
      int tga_indexed = stbi__get8(ref s);
      int tga_image_type = stbi__get8(ref s);
      int tga_is_RLE = 0;
      int tga_palette_start = stbi__get16le(ref s);
      int tga_palette_len = stbi__get16le(ref s);
      int tga_palette_bits = stbi__get8(ref s);
      int tga_x_origin = stbi__get16le(ref s);
      int tga_y_origin = stbi__get16le(ref s);
      int tga_width = stbi__get16le(ref s);
      int tga_height = stbi__get16le(ref s);
      int tga_bits_per_pixel = stbi__get8(ref s);
      STBI_CHANNELS tga_comp;
      bool tga_rgb16 = false;
      int tga_inverted = stbi__get8(ref s);
      // int tga_alpha_bits = tga_inverted & 15; // the 4 lowest bits - unused (useless?)
      //   image data
      BytePtr tga_data;
      BytePtr tga_palette = BytePtr.Null;
      int i, j;
      Span<byte> raw_data = new byte[4] { 0, 0, 0, 0 };
      int RLE_count = 0;
      int RLE_repeating = 0;
      int read_next_pixel = 1;
      // STBI_NOTUSED(ri);
      // STBI_NOTUSED(tga_x_origin); // @TODO
      // STBI_NOTUSED(tga_y_origin); // @TODO

      x = y = 0;
      comp = 0;

      if (tga_height > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large", "Very large image (corrupt?)");
      if (tga_width > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large", "Very large image (corrupt?)");

      //   do a tiny bit of precessing
      if (tga_image_type >= 8)
      {
         tga_image_type -= 8;
         tga_is_RLE = 1;
      }
      tga_inverted = 1 - ((tga_inverted >> 5) & 1);

      //   If I'm paletted, then I'll use the number of bits from the palette
      if (tga_indexed != 0) tga_comp = stbi__tga_get_comp(tga_palette_bits, false, out tga_rgb16);
      else tga_comp = stbi__tga_get_comp(tga_bits_per_pixel, (tga_image_type == 3), out tga_rgb16);

      if (tga_comp == STBI_CHANNELS._default) // shouldn't really happen, stbi__tga_test() should have ensured basic consistency
         return stbi__errpuc("bad format", "Can't find out TGA pixelformat");

      //   tga info
      x = tga_width;
      y = tga_height;
      comp = tga_comp;

      if (!stbi__mad3sizes_valid(tga_width, tga_height, (int)tga_comp, 0))
         return stbi__errpuc("too large", "Corrupt TGA");

      tga_data = (BytePtr)stbi__malloc_mad3(tga_width, tga_height, (int)tga_comp, 0);
      if (tga_data.IsNull) return stbi__errpuc("outofmem", "Out of memory");

      // skip to the data's starting position (offset usually = 0)
      stbi__skip(ref s, tga_offset);

      if (tga_indexed == 0 && tga_is_RLE == 0 && !tga_rgb16)
      {
         for (i = 0; i < tga_height; ++i)
         {
            int row = tga_inverted != 0 ? tga_height - i - 1 : i;
            BytePtr tga_row = tga_data + row * tga_width * (int)tga_comp;
            stbi__getn(ref s, tga_row, tga_width * (int)tga_comp);
         }
      }
      else
      {
         //   do I need to load a palette?
         if (tga_indexed != 0)
         {
            if (tga_palette_len == 0)
            {  /* you have to have at least one entry! */
               STBI_FREE(tga_data);
               return stbi__errpuc("bad palette", "Corrupt TGA");
            }

            //   any data to skip? (offset usually = 0)
            stbi__skip(ref s, tga_palette_start);
            //   load the palette
            tga_palette = (BytePtr)stbi__malloc_mad2(tga_palette_len, (int)tga_comp, 0);
            if (tga_palette.IsNull)
            {
               STBI_FREE(tga_data);
               return stbi__errpuc("outofmem", "Out of memory");
            }
            if (tga_rgb16)
            {
               BytePtr pal_entry = tga_palette;
               STBI_ASSERT(tga_comp == STBI_CHANNELS.rgb);
               for (i = 0; i < tga_palette_len; ++i)
               {
                  stbi__tga_read_rgb16(ref s, pal_entry);
                  pal_entry += (int)tga_comp;
               }
            }
            else if (!stbi__getn(ref s, tga_palette, tga_palette_len * (int)tga_comp))
            {
               STBI_FREE(tga_data);
               STBI_FREE(tga_palette);
               return stbi__errpuc("bad palette", "Corrupt TGA");
            }
         }
         //   load the data
         for (i = 0; i < tga_width * tga_height; ++i)
         {
            //   if I'm in RLE mode, do I need to get a RLE stbi__pngchunk?
            if (tga_is_RLE != 0)
            {
               if (RLE_count == 0)
               {
                  //   yep, get the next byte as a RLE command
                  int RLE_cmd = stbi__get8(ref s);
                  RLE_count = 1 + (RLE_cmd & 127);
                  RLE_repeating = RLE_cmd >> 7;
                  read_next_pixel = 1;
               }
               else if (RLE_repeating == 0)
               {
                  read_next_pixel = 1;
               }
            }
            else
            {
               read_next_pixel = 1;
            }
            //   OK, if I need to read a pixel, do it now
            if (read_next_pixel != 0)
            {
               //   load however much data we did have
               if (tga_indexed != 0)
               {
                  // read in index, then perform the lookup
                  int pal_idx = (tga_bits_per_pixel == 8) ? stbi__get8(ref s) : stbi__get16le(ref s);
                  if (pal_idx >= tga_palette_len)
                  {
                     // invalid index
                     pal_idx = 0;
                  }
                  pal_idx *= (int)tga_comp;
                  for (j = 0; j < (int)tga_comp; ++j)
                  {
                     raw_data[j] = tga_palette[pal_idx + j].Value;
                  }
               }
               else if (tga_rgb16)
               {
                  STBI_ASSERT(tga_comp == STBI_CHANNELS.rgb);
                  stbi__tga_read_rgb16(ref s, raw_data);
               }
               else
               {
                  //   read in the data raw
                  for (j = 0; j < (int)tga_comp; ++j)
                  {
                     raw_data[j] = stbi__get8(ref s);
                  }
               }
               //   clear the reading flag for the next pixel
               read_next_pixel = 0;
            } // end of reading a pixel

            // copy data
            for (j = 0; j < (int)tga_comp; ++j)
               tga_data[i * (int)tga_comp + j].Ref = raw_data[j];

            //   in case we're in RLE mode, keep counting down
            --RLE_count;
         }
         //   do I need to invert the image?
         if (tga_inverted != 0)
         {
            for (j = 0; j * 2 < tga_height; ++j)
            {
               int index1 = j * tga_width * (int)tga_comp;
               int index2 = (tga_height - 1 - j) * tga_width * (int)tga_comp;
               for (i = tga_width * (int)tga_comp; i > 0; --i)
               {
                  byte temp = tga_data[index1].Value;
                  tga_data[index1].Ref = tga_data[index2].Value;
                  tga_data[index2].Ref = temp;
                  ++index1;
                  ++index2;
               }
            }
         }
         //   clear my palette, if I had one
         if (!tga_palette.IsNull)
         {
            STBI_FREE(tga_palette);
         }
      }

      // swap RGB - if the source data was RGB16, it already is in the right order
      if ((int)tga_comp >= 3 && !tga_rgb16)
      {
         BytePtr tga_pixel = tga_data;

         for (i = 0; i < tga_width * tga_height; ++i)
         {
            byte temp = tga_pixel[0].Value;
            tga_pixel[0].Ref = tga_pixel[2].Value;
            tga_pixel[2].Ref = temp;
            tga_pixel += (int)tga_comp;
         }
      }

      // convert to target component count
      if (req_comp != 0 && req_comp != tga_comp)
         tga_data = stbi__convert_format(tga_data, tga_comp, req_comp, tga_width, tga_height);

      //   the things I do to get rid of an error message, and yet keep
      //   Microsoft's C compilers happy... [8^(
      // tga_palette_start = tga_palette_len = tga_palette_bits =
      //      tga_x_origin = tga_y_origin = 0;
      //STBI_NOTUSED(tga_palette_start);
      //   OK, done
      return tga_data;
   }

   static bool stbi__tga_info(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp)
   {
      STBI_CHANNELS tga_comp;
      int tga_w, tga_h, tga_image_type, tga_bits_per_pixel, tga_colormap_bpp;
      int sz, tga_colormap_type;
      stbi__get8(ref s);                   // discard Offset
      tga_colormap_type = stbi__get8(ref s); // colormap type

      x = y = 0;
      comp = STBI_CHANNELS._default;

      if (tga_colormap_type > 1)
      {
         stbi__rewind(ref s);
         return false;      // only RGB or indexed allowed
      }
      tga_image_type = stbi__get8(ref s); // image type
      if (tga_colormap_type == 1)
      { // colormapped (paletted) image
         if (tga_image_type != 1 && tga_image_type != 9)
         {
            stbi__rewind(ref s);
            return false;
         }
         stbi__skip(ref s, 4);       // skip index of first colormap entry and number of entries
         sz = stbi__get8(ref s);    //   check bits per palette color entry
         if ((sz != 8) && (sz != 15) && (sz != 16) && (sz != 24) && (sz != 32))
         {
            stbi__rewind(ref s);
            return false;
         }
         stbi__skip(ref s, 4);       // skip image x and y origin
         tga_colormap_bpp = sz;
      }
      else
      { // "normal" image w/o colormap - only RGB or grey allowed, +/- RLE
         if ((tga_image_type != 2) && (tga_image_type != 3) && (tga_image_type != 10) && (tga_image_type != 11))
         {
            stbi__rewind(ref s);
            return false; // only RGB or grey allowed, +/- RLE
         }
         stbi__skip(ref s, 9); // skip colormap specification and image x/y origin
         tga_colormap_bpp = 0;
      }
      tga_w = stbi__get16le(ref s);
      if (tga_w < 1)
      {
         stbi__rewind(ref s);
         return false;   // test width
      }
      tga_h = stbi__get16le(ref s);
      if (tga_h < 1)
      {
         stbi__rewind(ref s);
         return false;   // test height
      }
      tga_bits_per_pixel = stbi__get8(ref s); // bits per pixel
      stbi__get8(ref s); // ignore alpha bits
      if (tga_colormap_bpp != 0)
      {
         if ((tga_bits_per_pixel != 8) && (tga_bits_per_pixel != 16))
         {
            // when using a colormap, tga_bits_per_pixel is the size of the indexes
            // I don't think anything but 8 or 16bit indexes makes sense
            stbi__rewind(ref s);
            return false;
         }
         tga_comp = stbi__tga_get_comp(tga_colormap_bpp, false, out _);
      }
      else
      {
         tga_comp = stbi__tga_get_comp(tga_bits_per_pixel, (tga_image_type == 3) || (tga_image_type == 11), out _);
      }
      if (tga_comp == STBI_CHANNELS._default)
      {
         stbi__rewind(ref s);
         return false;
      }
      x = tga_w;
      y = tga_h;
      comp = tga_comp;
      return true;                   // seems to have passed everything
   }

#endif

#if !STBI_NO_PSD
static int      stbi__psd_test(ref stbi__context s);
static void    *stbi__psd_load(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri, int bpc);
static int      stbi__psd_info(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp);
static int      stbi__psd_is16(ref stbi__context s);
#endif

#if !STBI_NO_HDR
static int      stbi__hdr_test(ref stbi__context s);
static Ptr<float> stbi__hdr_load(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri);
static int      stbi__hdr_info(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp);
#endif

#if !STBI_NO_PIC
static int      stbi__pic_test(ref stbi__context s);
static BytePtr  stbi__pic_load(ref stbi__context s, int out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri);
static int      stbi__pic_info(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp);
#endif

#if !STBI_NO_GIF
static int      stbi__gif_test(ref stbi__context s);
static BytePtr  stbi__gif_load(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri);
static BytePtr  stbi__load_gif_main(ref stbi__context s, out int[] delays, out int x, out int y, out int z, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri);
static int      stbi__gif_info(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp);
#endif

#if !STBI_NO_PNM
static int      stbi__pnm_test(ref stbi__context s);
static BytePtr  stbi__pnm_load(ref stbi__context s, int out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, ref stbi__result_info ri);
static int      stbi__pnm_info(ref stbi__context s, int out int x, out int y, out STBI_CHANNELS comp);
static int      stbi__pnm_is16(ref stbi__context s);
#endif

   static BytePtr stbi__malloc(size_t size)
   {
      return new BytePtr(new byte[size]);
   }

   static Ptr<T> stbi__malloc<T>(size_t size)
   {
      return new Ptr<T>(new T[size]);
   }

   static void memcpy(Span<byte> target, Span<byte> source, size_t size)
   {
      source.Slice(0, size).CopyTo(target);
   }

   static void STBI_FREE(BytePtr p)
   {

   }

   static void STBI_FREE<T>(Ptr<T> p)
   {

   }

   static BytePtr STBI_REALLOC_SIZED(BytePtr p, uint oldSize, uint newSize)
   {
      STBI_ASSERT(p.Offset == 0);
      byte[]? array = p.Array;

      Array.Resize(ref array, (int)newSize);

      return new BytePtr(array);
   }


   // stb_image uses ints pervasively, including for offset calculations.
   // therefore the largest decoded image size we can support with the
   // current code, even on 64-bit targets, is INT_MAX. this is not a
   // significant limitation for the intended use case.
   //
   // we do, however, need to make sure our size calculations don't
   // overflow. hence a few helper functions for size calculations that
   // multiply integers together, making sure that they're non-negative
   // and no overflow occurs.

   // return 1 if the sum is valid, 0 on overflow.
   // negative terms are considered invalid.
   static bool stbi__addsizes_valid(int a, int b)
   {
      if (b < 0) return false;
      // now 0 <= b <= INT_MAX, hence also
      // 0 <= INT_MAX - b <= INTMAX.
      // And "a + b <= INT_MAX" (which might overflow) is the
      // same as a <= INT_MAX - b (no overflow)
      return a <= int.MaxValue - b;
   }

   // returns 1 if the product is valid, 0 on overflow.
   // negative factors are considered invalid.
   static bool stbi__mul2sizes_valid(int a, int b)
   {
      if (a < 0 || b < 0) return false;
      if (b == 0) return true; // mul-by-0 is always safe
                               // portable way to check for no overflows in a*b
      return a <= int.MaxValue / b;
   }

#if !STBI_NO_JPEG || !STBI_NO_PNG || !STBI_NO_TGA || !STBI_NO_HDR
   // returns 1 if "a*b + add" has no negative terms/factors and doesn't overflow
   static bool stbi__mad2sizes_valid(int a, int b, int add)
   {
      return stbi__mul2sizes_valid(a, b) && stbi__addsizes_valid(a * b, add);
   }
#endif

   // returns 1 if "a*b*c + add" has no negative terms/factors and doesn't overflow
   static bool stbi__mad3sizes_valid(int a, int b, int c, int add)
   {
      return stbi__mul2sizes_valid(a, b) && stbi__mul2sizes_valid(a * b, c) &&
         stbi__addsizes_valid(a * b * c, add);
   }

   // returns 1 if "a*b*c*d + add" has no negative terms/factors and doesn't overflow
#if !STBI_NO_LINEAR || !STBI_NO_HDR || !STBI_NO_PNM
   static bool stbi__mad4sizes_valid(int a, int b, int c, int d, int add)
   {
      return stbi__mul2sizes_valid(a, b) && stbi__mul2sizes_valid(a * b, c) &&
         stbi__mul2sizes_valid(a * b * c, d) && stbi__addsizes_valid(a * b * c * d, add);
   }
#endif

#if !(STBI_NO_JPEG) || !(STBI_NO_PNG) || !(STBI_NO_TGA) || !(STBI_NO_HDR)
   // mallocs with size overflow checking
   static BytePtr stbi__malloc_mad2(int a, int b, int add)
   {
      if (!stbi__mad2sizes_valid(a, b, add)) return BytePtr.Null;
      return stbi__malloc(a * b + add);
   }
#endif

   static BytePtr stbi__malloc_mad3(int a, int b, int c, int add)
   {
      if (!stbi__mad3sizes_valid(a, b, c, add)) return BytePtr.Null;
      return stbi__malloc(a * b * c + add);
   }

#if !(STBI_NO_LINEAR) || !(STBI_NO_HDR) || !(STBI_NO_PNM)
   static BytePtr stbi__malloc_mad4(int a, int b, int c, int d, int add)
   {
      if (!stbi__mad4sizes_valid(a, b, c, d, add)) return BytePtr.null;
      return stbi__malloc(a * b * c * d + add);
   }
#endif

   // returns 1 if the sum of two signed ints is valid (between -2^31 and 2^31-1 inclusive), 0 on overflow.
   static bool stbi__addints_valid(int a, int b)
   {
      if ((a >= 0) != (b >= 0)) return true; // a and b have different signs, so no overflow
      if (a < 0 && b < 0) return a >= int.MinValue - b; // same as a + b >= INT_MIN; INT_MIN - b cannot overflow since b < 0.
      return a <= int.MaxValue - b;
   }

   const int SHRT_MAX = 32767;
   const int SHRT_MIN = -32768;

   // returns 1 if the product of two ints fits in a signed short, 0 on overflow.
   static bool stbi__mul2shorts_valid(int a, int b)
   {
      if (b == 0 || b == -1) return true; // multiplication by 0 is always 0; check for -1 so SHRT_MIN/b doesn't overflow
      if ((a >= 0) == (b >= 0)) return a <= SHRT_MAX / b; // product is positive, so similar to mul2sizes_valid
      if (b < 0) return a <= SHRT_MIN / b; // same as a * b >= SHRT_MIN
      return a >= SHRT_MIN / b;
   }


#if !STBI_NO_LINEAR
static float   *stbi__ldr_to_hdr(BytePtr data, int x, int y, int comp);
#endif

#if !STBI_NO_HDR
static BytePtr stbi__hdr_to_ldr(float   *data, int x, int y, int comp);
#endif

   // #if !STBI_THREAD_LOCAL
   // #define stbi__vertically_flip_on_load  stbi__vertically_flip_on_load_global
   // // #else
   // static bool stbi__vertically_flip_on_load_local, stbi__vertically_flip_on_load_set;
   // 
   // static public void stbi_set_flip_vertically_on_load_thread(bool flag_true_if_should_flip)
   // {
   //    stbi__vertically_flip_on_load_local = flag_true_if_should_flip;
   //    stbi__vertically_flip_on_load_set = true;
   // }

   // #endif // STBI_THREAD_LOCAL

   static BytePtr stbi__load_main(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp, out stbi__result_info ri, int bpc)
   {
      ri = new stbi__result_info();

      ri.bits_per_channel = 8; // default is 8 so most paths don't have to be changed
      ri.channel_order = STBI_ORDER.RGB; // all current input & output are this, but this is here so we can add BGR order
      ri.num_channels = 0;

      // test the formats with a very explicit header first (at least a FOURCC
      // or distinctive magic number first)
#if !STBI_NO_PNG
      if (stbi__png_test(ref s)) return stbi__png_load(ref s, out x, out y, out comp, req_comp, ref ri);
#endif
#if !STBI_NO_BMP
      if (stbi__bmp_test(ref s)) return stbi__bmp_load(ref s, out x, out y, out comp, req_comp, ref ri);
#endif
#if !STBI_NO_GIF
   if (stbi__gif_test(ref s))  return stbi__gif_load(s,x,y,comp,req_comp, ri);
#endif
#if !STBI_NO_PSD
   if (stbi__psd_test(ref s))  return stbi__psd_load(s,x,y,comp,req_comp, ri, bpc);
#else
      //STBI_NOTUSED(bpc);
#endif
#if !STBI_NO_PIC
   if (stbi__pic_test(ref s))  return stbi__pic_load(s,x,y,comp,req_comp, ri);
#endif

      // then the formats that can end up attempting to load with just 1 or 2
      // bytes matching expectations; these are prone to false positives, so
      // try them later
#if !STBI_NO_JPEG
      if (stbi__jpeg_test(ref s)) return stbi__jpeg_load(ref s, out x, out y, out comp, req_comp, ref ri);
#endif
#if !STBI_NO_PNM
   if (stbi__pnm_test(ref s))  return stbi__pnm_load(s,x,y,comp,req_comp, ri);
#endif

#if !STBI_NO_HDR
   if (stbi__hdr_test(ref s)) {
      Ptr<float> hdr = stbi__hdr_load(s, x,y,comp,req_comp, ri);
      return stbi__hdr_to_ldr(hdr, out x, out y, req_comp != 0 ? req_comp : comp);
   }
#endif

#if !STBI_NO_TGA
      // test tga last because it's a crappy test!
      if (stbi__tga_test(ref s))
         return stbi__tga_load(ref s, out x, out y, out comp, req_comp, ref ri);
#endif

      x = 0;
      y = 0;
      comp = 0;
      return stbi__errpuc("unknown image type", "Image not of any known type, or corrupt");
   }

   static BytePtr stbi__convert_16_to_8(Span<stbi__uint16> orig, int w, int h, STBI_CHANNELS channels)
   {
      int i;
      int img_len = w * h * (int)channels;
      BytePtr reduced = stbi__malloc(img_len);
      if (reduced.IsNull) return stbi__errpuc("outofmem", "Out of memory");

      for (i = 0; i < img_len; ++i)
         reduced[i].Ref = (stbi_uc)((orig[i] >> 8) & 0xFF); // top half of each byte is sufficient approx of 16.8 bit scaling

      //STBI_FREE(orig);
      return reduced;
   }

   static Ptr<stbi__uint16> stbi__convert_8_to_16(BytePtr orig, int w, int h, STBI_CHANNELS channels)
   {
      int i;
      int img_len = w * h * (int)channels;
      Ptr<stbi__uint16> enlarged;

      enlarged = stbi__malloc<stbi__uint16>(img_len/* * 2*/);
      if (enlarged.IsNull) return stbi__errpuc<stbi__uint16>("outofmem", "Out of memory");

      for (i = 0; i < img_len; ++i)
         enlarged[i].Ref = (stbi__uint16)((orig[i].Value << 8) + orig[i].Value); // replicate to high and low byte, maps 0.0, 255.0xffff

      STBI_FREE(orig);
      return enlarged;
   }

   static void stbi__vertical_flip(BytePtr image, int w, int h, int bytes_per_pixel)
   {
      const int sizeofTemp = 2048;

      int row;
      size_t bytes_per_row = (size_t)w * bytes_per_pixel;
      Span<stbi_uc> temp = stackalloc stbi_uc[sizeofTemp];
      BytePtr bytes = image;

      for (row = 0; row < (h >> 1); row++)
      {
         BytePtr row0 = bytes + row * bytes_per_row;
         BytePtr row1 = bytes + (h - row - 1) * bytes_per_row;

         // swap row0 with row1
         size_t bytes_left = bytes_per_row;
         while (bytes_left != 0)
         {
            size_t bytes_copy = (bytes_left < sizeofTemp) ? bytes_left : sizeofTemp;

            memcpy(temp, row0, bytes_copy);
            memcpy(row0, row1, bytes_copy);
            memcpy(row1, temp, bytes_copy);

            row0 += bytes_copy;
            row1 += bytes_copy;
            bytes_left -= bytes_copy;
         }
      }
   }

#if !STBI_NO_GIF
static void stbi__vertical_flip_slices(void *image, int w, int h, int z, int bytes_per_pixel)
{
   int slice;
   int slice_size = w * h * bytes_per_pixel;

   BytePtr bytes = (BytePtr )image;
   for (slice = 0; slice < z; ++slice) {
      stbi__vertical_flip(bytes, w, h, bytes_per_pixel);
      bytes += slice_size;
   }
}
#endif

   static BytePtr stbi__load_and_postprocess_8bit(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp)
   {
      stbi__result_info ri;
      BytePtr result = stbi__load_main(ref s, out x, out y, out comp, req_comp, out ri, 8);

      if (result.IsNull)
         return BytePtr.Null;

      // it is the responsibility of the loaders to make sure we get either 8 or 16 bit.
      STBI_ASSERT(ri.bits_per_channel == 8 || ri.bits_per_channel == 16);

      if (ri.bits_per_channel != 8)
      {
         result = stbi__convert_16_to_8(MemoryMarshal.Cast<byte, stbi__uint16>(result.Span), x, y, req_comp == STBI_CHANNELS._default ? comp : req_comp);
         ri.bits_per_channel = 8;
      }

      // @TODO: move stbi__convert_format to here

      if (stbi__vertically_flip_on_load)
      {
         STBI_CHANNELS channels = req_comp != STBI_CHANNELS._default ? req_comp : comp;
         stbi__vertical_flip(result, x, y, (int)channels * sizeof(stbi_uc));
      }

      return result;
   }

   static Ptr<stbi__uint16> stbi__load_and_postprocess_16bit(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp)
   {
      stbi__result_info ri;
      BytePtr result = stbi__load_main(ref s, out x, out y, out comp, req_comp, out ri, 16);

      if (result.IsNull)
         return Ptr<stbi__uint16>.Null;

      // it is the responsibility of the loaders to make sure we get either 8 or 16 bit.
      STBI_ASSERT(ri.bits_per_channel == 8 || ri.bits_per_channel == 16);

      if (ri.bits_per_channel != 16)
      {
         // TODO: Memory allocation!!
         result = MemoryMarshal.Cast<stbi__uint16, byte>(stbi__convert_8_to_16(result, x, y, req_comp == STBI_CHANNELS._default ? comp : req_comp).Span).ToArray();
         ri.bits_per_channel = 16;
      }

      // @TODO: move stbi__convert_format16 to here
      // @TODO: special case RGB-to-Y (and RGBA-to-YA) for 8-bit-to-16-bit case to keep more precision

      if (stbi__vertically_flip_on_load)
      {
         STBI_CHANNELS channels = req_comp != STBI_CHANNELS._default ? req_comp : comp;
         stbi__vertical_flip(result, x, y, (int)channels * sizeof(stbi__uint16));
      }

      // TODO: Memory allocation!!
      return MemoryMarshal.Cast<byte, stbi__uint16>(result.Span).ToArray();
   }

#if !(STBI_NO_HDR) && !(STBI_NO_LINEAR)
   static void stbi__float_postprocess(float* result, out int x, out int y, out int comp, int req_comp)
   {
      if (stbi__vertically_flip_on_load && result != NULL)
      {
         int channels = req_comp ? req_comp : *comp;
         stbi__vertical_flip(result, *x, *y, channels * sizeof(float));
      }
   }
#endif

#if !STBI_NO_STDIO

// #if (_WIN32) && (STBI_WINDOWS_UTF8)
// STBI_EXTERN __declspec(dllimport) int __stdcall MultiByteToWideChar(uint cp, unsigned long flags, const char *str, int cbmb, wchar_t *widestr, int cchwide);
// STBI_EXTERN __declspec(dllimport) int __stdcall WideCharToMultiByte(uint cp, unsigned long flags, const wchar_t *widestr, int cchwide, char *str, int cbmb, const char *defchar, int *used_default);
// #endif

// #if (_WIN32) && (STBI_WINDOWS_UTF8)
// static public int stbi_convert_wchar_to_utf8(char *buffer, size_t bufferlen, const wchar_t* input)
// {
// 	return WideCharToMultiByte(65001 /* UTF8 */, 0, input, -1, buffer, (int) bufferlen, NULL, NULL);
// }
// #endif

static FILE *stbi__fopen(BytePtr filename, BytePtr mode)
{
   FILE *f;
#if (_WIN32) && (STBI_WINDOWS_UTF8)
   wchar_t wMode[64];
   wchar_t wFilename[1024];
	if (0 == MultiByteToWideChar(65001 /* UTF8 */, 0, filename, -1, wFilename, sizeof(wFilename)/sizeof(*wFilename)))
      return 0;

	if (0 == MultiByteToWideChar(65001 /* UTF8 */, 0, mode, -1, wMode, sizeof(wMode)/sizeof(*wMode)))
      return 0;

// #if (_MSC_VER) && _MSC_VER >= 1400
// 	if (0 != _wfopen_s(&f, wFilename, wMode))
// 		f = 0;
// #else
   f = _wfopen(wFilename, wMode);
//#endif

// #elif (_MSC_VER) && _MSC_VER >= 1400
//    if (0 != fopen_s(&f, filename, mode))
//       f=0;
// #else
   f = fopen(filename, mode);
#endif
   return f;
}


static public BytePtr stbi_load(BytePtr filename, int *x, int *y, int *comp, int req_comp)
{
   FILE *f = stbi__fopen(filename, "rb");
   BytePtr result;
   if (!f) return stbi__errpuc("can't fopen", "Unable to open file");
   result = stbi_load_from_file(f,x,y,comp,req_comp);
   fclose(f);
   return result;
}

static public BytePtr stbi_load_from_file(FILE *f, int *x, int *y, int *comp, int req_comp)
{
   BytePtr result;
   stbi__context s;
   stbi__start_file(&s,f);
   result = stbi__load_and_postprocess_8bit(&s,x,y,comp,req_comp);
   if (result) {
      // need to 'unget' all the characters in the IO buffer
      fseek(f, - (int) (s.img_buffer_end - s.img_buffer), SEEK_CUR);
   }
   return result;
}

static public stbi__uint16 *stbi_load_from_file_16(FILE *f, int *x, int *y, int *comp, int req_comp)
{
   stbi__uint16 *result;
   stbi__context s;
   stbi__start_file(&s,f);
   result = stbi__load_and_postprocess_16bit(&s,x,y,comp,req_comp);
   if (result) {
      // need to 'unget' all the characters in the IO buffer
      fseek(f, - (int) (s.img_buffer_end - s.img_buffer), SEEK_CUR);
   }
   return result;
}

static public stbi_us *stbi_load_16(BytePtr filename, int *x, int *y, int *comp, int req_comp)
{
   FILE *f = stbi__fopen(filename, "rb");
   stbi__uint16 *result;
   if (!f) return (stbi_us *) stbi__errpuc("can't fopen", "Unable to open file");
   result = stbi_load_from_file_16(f,x,y,comp,req_comp);
   fclose(f);
   return result;
}


#endif //!STBI_NO_STDIO



#if !STBI_NO_GIF
static public BytePtr stbi_load_gif_from_memory(stbi_uc const *buffer, int len, int **delays, int *x, int *y, int *z, int *comp, int req_comp)
{
   BytePtr result;
   stbi__context s;
   stbi__start_mem(&s,buffer,len);

   result = (BytePtr) stbi__load_gif_main(&s, delays, x, y, z, comp, req_comp);
   if (stbi__vertically_flip_on_load) {
      stbi__vertical_flip_slices( result, *x, *y, *z, *comp );
   }

   return result;
}
#endif

#if !STBI_NO_LINEAR
static float *stbi__loadf_main(ref stbi__context s, int *x, int *y, int *comp, int req_comp)
{
   BytePtr data;
#if !STBI_NO_HDR
   if (stbi__hdr_test(s)) {
      stbi__result_info ri;
      float *hdr_data = stbi__hdr_load(s,x,y,comp,req_comp, &ri);
      if (hdr_data)
         stbi__float_postprocess(hdr_data,x,y,comp,req_comp);
      return hdr_data;
   }
#endif
   data = stbi__load_and_postprocess_8bit(s, x, y, comp, req_comp);
   if (data)
      return stbi__ldr_to_hdr(data, *x, *y, req_comp ? req_comp : *comp);
   return stbi__errpf("unknown image type", "Image not of any known type, or corrupt");
}

static public float *stbi_loadf_from_memory(stbi_uc const *buffer, int len, int *x, int *y, int *comp, int req_comp)
{
   stbi__context s;
   stbi__start_mem(&s,buffer,len);
   return stbi__loadf_main(&s,x,y,comp,req_comp);
}

static public float *stbi_loadf_from_callbacks(stbi_io_callbacks const *clbk, void *user, int *x, int *y, int *comp, int req_comp)
{
   stbi__context s;
   stbi__start_callbacks(&s, (stbi_io_callbacks *) clbk, user);
   return stbi__loadf_main(&s,x,y,comp,req_comp);
}

#if !STBI_NO_STDIO
static public float *stbi_loadf(BytePtr filename, int *x, int *y, int *comp, int req_comp)
{
   float *result;
   FILE *f = stbi__fopen(filename, "rb");
   if (!f) return stbi__errpf("can't fopen", "Unable to open file");
   result = stbi_loadf_from_file(f,x,y,comp,req_comp);
   fclose(f);
   return result;
}

static public float *stbi_loadf_from_file(FILE *f, int *x, int *y, int *comp, int req_comp)
{
   stbi__context s;
   stbi__start_file(&s,f);
   return stbi__loadf_main(&s,x,y,comp,req_comp);
}
#endif // !STBI_NO_STDIO

#endif // !STBI_NO_LINEAR

   // these is-hdr-or-not is defined independent of whether STBI_NO_LINEAR is
   // defined, for API simplicity; if STBI_NO_LINEAR is defined, it always
   // reports false!


#if !STBI_NO_STDIO
static public int      stbi_is_hdr          (BytePtr filename)
{
   FILE *f = stbi__fopen(filename, "rb");
   int result=0;
   if (f) {
      result = stbi_is_hdr_from_file(f);
      fclose(f);
   }
   return result;
}

static public int stbi_is_hdr_from_file(FILE *f)
{
#if !STBI_NO_HDR
   long pos = ftell(f);
   int res;
   stbi__context s;
   stbi__start_file(&s,f);
   res = stbi__hdr_test(&s);
   fseek(f, pos, SEEK_SET);
   return res;
#else
   STBI_NOTUSED(f);
   return 0;
#endif
}
#endif // !STBI_NO_STDIO


#if !STBI_NO_LINEAR
static float stbi__l2h_gamma=2.2f, stbi__l2h_scale=1.0f;

static public void   stbi_ldr_to_hdr_gamma(float gamma) { stbi__l2h_gamma = gamma; }
static public void   stbi_ldr_to_hdr_scale(float scale) { stbi__l2h_scale = scale; }
#endif

   static float stbi__h2l_gamma_i = 1.0f / 2.2f, stbi__h2l_scale_i = 1.0f;

   static public void stbi_hdr_to_ldr_gamma(float gamma) { stbi__h2l_gamma_i = 1 / gamma; }
   static public void stbi_hdr_to_ldr_scale(float scale) { stbi__h2l_scale_i = 1 / scale; }


   //////////////////////////////////////////////////////////////////////////////
   //
   // Common code used by all image loaders
   //

   enum STBI__SCAN
   {
      load = 0,
      type,
      header
   };


   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static stbi_uc stbi__get8(ref stbi__context s)
   {
      if (s.img_buffer < s.img_buffer_end)
         return (s.img_buffer++).Value;
      if (s.read_from_callbacks)
      {
         stbi__refill_buffer(ref s);
         return (s.img_buffer++).Value;
      }
      return 0;
   }

#if (STBI_NO_JPEG) && (STBI_NO_HDR) && (STBI_NO_PIC) && (STBI_NO_PNM)
   // nothing
#else
   static bool stbi__at_eof(ref stbi__context s)
   {
      if (s.io.read != null)
      {
         if (s.io.eof == null) return true;
         if (!s.io.eof()) return false;
         // if feof() is true, check if buffer = end
         // special case: we've only got the special 0 character at the end
         if (!s.read_from_callbacks) return true;
      }

      return s.img_buffer >= s.img_buffer_end;
   }
#endif

#if (STBI_NO_JPEG) && (STBI_NO_PNG) && (STBI_NO_BMP) && (STBI_NO_PSD) && (STBI_NO_TGA) && (STBI_NO_GIF) && (STBI_NO_PIC)
// nothing
#else
   static void stbi__skip(ref stbi__context s, int n)
   {
      if (n == 0) return;  // already there!
      if (n < 0)
      {
         s.img_buffer = s.img_buffer_end;
         return;
      }
      if (s.io.read != null)
      {
         int blen = (int)(s.img_buffer_end - s.img_buffer).Offset;
         if (blen < n)
         {
            Debug.Assert(s.io.skip != null);
            s.img_buffer = s.img_buffer_end;
            s.io.skip(n - blen);
            return;
         }
      }
      s.img_buffer += n;
   }
#endif

#if (STBI_NO_PNG) && (STBI_NO_TGA) && (STBI_NO_HDR) && (STBI_NO_PNM)
// nothing
#else
   static bool stbi__getn(ref stbi__context s, BytePtr buffer, int n)
   {
      if (s.io.read != null)
      {
         int blen = (int)(s.img_buffer_end - s.img_buffer).Offset;
         if (blen < n)
         {
            bool res;
            int count;

            memcpy(buffer, s.img_buffer, blen);

            count = s.io.read(buffer + blen, n - blen);
            res = (count == (n - blen));
            s.img_buffer = s.img_buffer_end;
            return res;
         }
      }

      if (s.img_buffer + n <= s.img_buffer_end)
      {
         memcpy(buffer, s.img_buffer, n);

         s.img_buffer += n;
         return true;
      }
      else
         return false;
   }
#endif

#if (STBI_NO_JPEG) && (STBI_NO_PNG) && (STBI_NO_PSD) && (STBI_NO_PIC)
// nothing
#else
   static int stbi__get16be(ref stbi__context s)
   {
      int z = stbi__get8(ref s);
      return (z << 8) + stbi__get8(ref s);
   }
#endif

#if (STBI_NO_PNG) && (STBI_NO_PSD) && (STBI_NO_PIC)
// nothing
#else
   static stbi__uint32 stbi__get32be(ref stbi__context s)
   {
      stbi__uint32 z = (uint)stbi__get16be(ref s);
      return (z << 16) + (stbi__uint32)stbi__get16be(ref s);
   }
#endif

#if (STBI_NO_BMP) && (STBI_NO_TGA) && (STBI_NO_GIF)
   // nothing
#else
   static int stbi__get16le(ref stbi__context s)
   {
      int z = stbi__get8(ref s);
      return z + (stbi__get8(ref s) << 8);
   }
#endif

#if !STBI_NO_BMP
   static stbi__uint32 stbi__get32le(ref stbi__context s)
   {
      stbi__uint32 z = (stbi__uint32)stbi__get16le(ref s);
      z += (stbi__uint32)stbi__get16le(ref s) << 16;
      return z;
   }
#endif

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static byte STBI__BYTECAST(int x) => ((stbi_uc)((x) & 255));  // truncate int to byte without warnings

#if (STBI_NO_JPEG) && (STBI_NO_PNG) && (STBI_NO_BMP) && (STBI_NO_PSD) && (STBI_NO_TGA) && (STBI_NO_GIF) && (STBI_NO_PIC) && (STBI_NO_PNM)
// nothing
#else
   //////////////////////////////////////////////////////////////////////////////
   //
   //  generic converter from built-in img_n to req_comp
   //    individual types do this automatically as much as possible (e.g. jpeg
   //    does all cases internally since it needs to colorspace convert anyway,
   //    and it never has alpha, so very few cases ). png can automatically
   //    interleave an alpha=255 channel, but falls back to this for other cases
   //
   //  assume data buffer is malloced, so malloc a new one and free that one
   //  only failure mode is malloc failing

   static stbi_uc stbi__compute_y(int r, int g, int b)
   {
      return (stbi_uc)(((r * 77) + (g * 150) + (29 * b)) >> 8);
   }
#endif

#if (STBI_NO_PNG) && (STBI_NO_BMP) && (STBI_NO_PSD) && (STBI_NO_TGA) && (STBI_NO_GIF) && (STBI_NO_PIC) && (STBI_NO_PNM)
// nothing
#else
   static BytePtr stbi__convert_format(BytePtr data, STBI_CHANNELS img_n, STBI_CHANNELS req_comp, int x, int y)
   {
      int i, j;
      BytePtr good;

      if (req_comp == img_n) return data;
      STBI_ASSERT((int)req_comp >= 1 && (int)req_comp <= 4);

      good = stbi__malloc_mad3((int)req_comp, x, y, 0);
      if (good.IsNull)
      {
         STBI_FREE(data);
         return stbi__errpuc("outofmem", "Out of memory");
      }

      int STBI__COMBO(int a, int b) => ((a) * 8 + (b));
      void STBI__CASE(int a, int b, ref BytePtr src, ref BytePtr dest, Action action)
      {
         for (i = x - 1; i >= 0; --i, src += a, dest += b)
            action();
      }

      for (j = 0; j < (int)y; ++j)
      {
         BytePtr src = data + j * x * (int)img_n;
         BytePtr dest = good + j * x * (int)req_comp;

         // convert source image with img_n components to one with req_comp components;
         // avoid switch per pixel, so use switch per scanline and massive macros

         int combo = STBI__COMBO((int)img_n, (int)req_comp);

         if (combo == STBI__COMBO(1, 2)) STBI__CASE(1, 2, ref src, ref dest, () => { dest[0].Ref = src[0].Value; dest[1].Ref = 255; });
         else if (combo == STBI__COMBO(1, 3)) STBI__CASE(1, 3, ref src, ref dest, () => { dest[0].Ref = dest[1].Ref = dest[2].Ref = src[0].Value; });
         else if (combo == STBI__COMBO(1, 4)) STBI__CASE(1, 4, ref src, ref dest, () => { dest[0].Ref = dest[1].Ref = dest[2].Ref = src[0].Value; dest[3].Ref = 255; });
         else if (combo == STBI__COMBO(2, 1)) STBI__CASE(2, 1, ref src, ref dest, () => { dest[0].Ref = src[0].Value; });
         else if (combo == STBI__COMBO(2, 3)) STBI__CASE(2, 3, ref src, ref dest, () => { dest[0].Ref = dest[1].Ref = dest[2].Ref = src[0].Value; });
         else if (combo == STBI__COMBO(2, 4)) STBI__CASE(2, 4, ref src, ref dest, () => { dest[0].Ref = dest[1].Ref = dest[2].Ref = src[0].Value; dest[3].Ref = src[1].Value; });
         else if (combo == STBI__COMBO(3, 4)) STBI__CASE(3, 4, ref src, ref dest, () => { dest[0].Ref = src[0].Value; dest[1].Ref = src[1].Value; dest[2].Ref = src[2].Value; dest[3].Ref = 255; });
         else if (combo == STBI__COMBO(3, 1)) STBI__CASE(3, 1, ref src, ref dest, () => { dest[0].Ref = stbi__compute_y(src[0].Value, src[1].Value, src[2].Value); });
         else if (combo == STBI__COMBO(3, 2)) STBI__CASE(3, 2, ref src, ref dest, () => { dest[0].Ref = stbi__compute_y(src[0].Value, src[1].Value, src[2].Value); dest[1].Ref = 255; });
         else if (combo == STBI__COMBO(4, 1)) STBI__CASE(4, 1, ref src, ref dest, () => { dest[0].Ref = stbi__compute_y(src[0].Value, src[1].Value, src[2].Value); });
         else if (combo == STBI__COMBO(4, 2)) STBI__CASE(4, 2, ref src, ref dest, () => { dest[0].Ref = stbi__compute_y(src[0].Value, src[1].Value, src[2].Value); dest[1].Ref = src[3].Value; });
         else if (combo == STBI__COMBO(4, 3)) STBI__CASE(4, 3, ref src, ref dest, () => { dest[0].Ref = src[0].Value; dest[1].Ref = src[1].Value; dest[2].Ref = src[2].Value; });
         else { STBI_ASSERT(false); STBI_FREE(data); STBI_FREE(good); return stbi__errpuc("unsupported", "Unsupported format conversion"); }
      }

      STBI_FREE(data);
      return good;
   }
#endif

#if (STBI_NO_PNG) && (STBI_NO_PSD)
// nothing
#else
   static stbi__uint16 stbi__compute_y_16(int r, int g, int b)
   {
      return (stbi__uint16)(((r * 77) + (g * 150) + (29 * b)) >> 8);
   }
#endif

#if (STBI_NO_PNG) && (STBI_NO_PSD)
// nothing
#else
   static Ptr<stbi__uint16> stbi__convert_format16(Ptr<stbi__uint16> data, STBI_CHANNELS img_n, STBI_CHANNELS req_comp, int x, int y)
   {
      int i, j;
      Ptr<stbi__uint16> good;

      if (req_comp == img_n) return data;
      STBI_ASSERT((int)req_comp >= 1 && (int)req_comp <= 4);

      good = stbi__malloc<stbi__uint16>((int)req_comp * x * y /* * 2 */);
      if (good.IsNull)
      {
         STBI_FREE(data);
         return stbi__errpuc<stbi__uint16>("outofmem", "Out of memory");
      }

      int STBI__COMBO(int a, int b) => ((a) * 8 + (b));
      void STBI__CASE(int a, int b, ref Ptr<stbi__uint16> src, ref Ptr<stbi__uint16> dest, Action action)
      {
         for (i = x - 1; i >= 0; --i, src += a, dest += b)
            action();
      }

      for (j = 0; j < (int)y; ++j)
      {
         Ptr<stbi__uint16> src = data + j * x * (int)img_n;
         Ptr<stbi__uint16> dest = good + j * x * (int)req_comp;

         // convert source image with img_n components to one with req_comp components;
         // avoid switch per pixel, so use switch per scanline and massive macros
         int combo = STBI__COMBO((int)img_n, (int)req_comp);

         if (combo == STBI__COMBO(1, 2)) STBI__CASE(1, 2, ref src, ref dest, () => { dest[0].Ref = src[0].Value; dest[1].Ref = 0xffff; });
         else if (combo == STBI__COMBO(1, 3)) STBI__CASE(1, 3, ref src, ref dest, () => { dest[0].Ref = dest[1].Ref = dest[2].Ref = src[0].Value; });
         else if (combo == STBI__COMBO(1, 4)) STBI__CASE(1, 4, ref src, ref dest, () => { dest[0].Ref = dest[1].Ref = dest[2].Ref = src[0].Value; dest[3].Ref = 0xffff; });
         else if (combo == STBI__COMBO(2, 1)) STBI__CASE(2, 1, ref src, ref dest, () => { dest[0].Ref = src[0].Value; });
         else if (combo == STBI__COMBO(2, 3)) STBI__CASE(2, 3, ref src, ref dest, () => { dest[0].Ref = dest[1].Ref = dest[2].Ref = src[0].Value; });
         else if (combo == STBI__COMBO(2, 4)) STBI__CASE(2, 4, ref src, ref dest, () => { dest[0].Ref = dest[1].Ref = dest[2].Ref = src[0].Value; dest[3].Ref = src[1].Value; });
         else if (combo == STBI__COMBO(3, 4)) STBI__CASE(3, 4, ref src, ref dest, () => { dest[0].Ref = src[0].Value; dest[1].Ref = src[1].Value; dest[2].Ref = src[2].Value; dest[3].Ref = 0xffff; });
         else if (combo == STBI__COMBO(3, 1)) STBI__CASE(3, 1, ref src, ref dest, () => { dest[0].Ref = stbi__compute_y_16(src[0].Value, src[1].Value, src[2].Value); });
         else if (combo == STBI__COMBO(3, 2)) STBI__CASE(3, 2, ref src, ref dest, () => { dest[0].Ref = stbi__compute_y_16(src[0].Value, src[1].Value, src[2].Value); dest[1].Ref = 0xffff; });
         else if (combo == STBI__COMBO(4, 1)) STBI__CASE(4, 1, ref src, ref dest, () => { dest[0].Ref = stbi__compute_y_16(src[0].Value, src[1].Value, src[2].Value); });
         else if (combo == STBI__COMBO(4, 2)) STBI__CASE(4, 2, ref src, ref dest, () => { dest[0].Ref = stbi__compute_y_16(src[0].Value, src[1].Value, src[2].Value); dest[1].Ref = src[3].Value; });
         else if (combo == STBI__COMBO(4, 3)) STBI__CASE(4, 3, ref src, ref dest, () => { dest[0].Ref = src[0].Value; dest[1].Ref = src[1].Value; dest[2].Ref = src[2].Value; });
         else { STBI_ASSERT(false); STBI_FREE(data); STBI_FREE(good); return stbi__errpuc<stbi__uint16>("unsupported", "Unsupported format conversion"); }
      }

      STBI_FREE(data);
      return good;
   }
#endif

#if !STBI_NO_LINEAR
static float   *stbi__ldr_to_hdr(BytePtr data, int x, int y, int comp)
{
   int i,k,n;
   float *output;
   if (!data) return NULL;
   output = (float *) stbi__malloc_mad4(x, y, comp, sizeof(float), 0);
   if (output == NULL) { STBI_FREE(data); return stbi__errpf("outofmem", "Out of memory"); }
   // compute number of non-alpha components
   if (comp & 1) n = comp; else n = comp-1;
   for (i=0; i < x*y; ++i) {
      for (k=0; k < n; ++k) {
         output[i*comp + k] = (float) (pow(data[i*comp+k]/255.0f, stbi__l2h_gamma) * stbi__l2h_scale);
      }
   }
   if (n < comp) {
      for (i=0; i < x*y; ++i) {
         output[i*comp + n] = data[i*comp + n]/255.0f;
      }
   }
   STBI_FREE(data);
   return output;
}
#endif

#if !STBI_NO_HDR
static int stbi__float2int(float x) =>((int) (x));
static BytePtr stbi__hdr_to_ldr(float   *data, int x, int y, int comp)
{
   int i,k,n;
   BytePtr output;
   if (!data) return NULL;
   output = (BytePtr ) stbi__malloc_mad3(x, y, comp, 0);
   if (output == NULL) { STBI_FREE(data); return stbi__errpuc("outofmem", "Out of memory"); }
   // compute number of non-alpha components
   if (comp & 1) n = comp; else n = comp-1;
   for (i=0; i < x*y; ++i) {
      for (k=0; k < n; ++k) {
         float z = (float) pow(data[i*comp+k]*stbi__h2l_scale_i, stbi__h2l_gamma_i) * 255 + 0.5f;
         if (z < 0) z = 0;
         if (z > 255) z = 255;
         output[i*comp + k] = (stbi_uc) stbi__float2int(z);
      }
      if (k < comp) {
         float z = data[i*comp+k] * 255 + 0.5f;
         if (z < 0) z = 0;
         if (z > 255) z = 255;
         output[i*comp + k] = (stbi_uc) stbi__float2int(z);
      }
   }
   STBI_FREE(data);
   return output;
}
#endif

   //////////////////////////////////////////////////////////////////////////////
   //
   //  "baseline" JPEG/JFIF decoder
   //
   //    simple implementation
   //      - doesn't support delayed output of y-dimension
   //      - simple interface (only one output format: 8-bit interleaved RGB)
   //      - doesn't try to recover corrupt jpegs
   //      - doesn't allow partial loading, loading multiple at once
   //      - still fast on x86 (copying globals into locals doesn't help x86)
   //      - allocates lots of intermediate memory (full size of all components)
   //        - non-interleaved case requires this anyway
   //        - allows good upsampling (see next)
   //    high-quality
   //      - upsampled channels are bilinearly interpolated, even across blocks
   //      - quality integer IDCT derived from IJG's 'slow'
   //    performance
   //      - fast huffman; reasonable integer IDCT
   //      - some SIMD kernels for common paths on targets with SSE2/NEON
   //      - uses a lot of intermediate memory, could cache poorly

#if !STBI_NO_JPEG

   // // huffman decoding acceleration
   const int FAST_BITS = 9;  // larger handles more cases; smaller stomps less cache

   struct stbi__huffman
   {
      public stbi_uc[] fast = new stbi_uc[1 << FAST_BITS];
      // weirdly, repacking this into AoS is a 10% speed loss, instead of a win
      public stbi__uint16[] code = new stbi__uint16[256];
      public stbi_uc[] values = new stbi_uc[256];
      public stbi_uc[] size = new stbi_uc[257];
      public uint[] maxcode = new uint[18];
      public int[] delta = new int[17];   // old 'firstsymbol' - old 'firstcode'

      public stbi__huffman()
      {
      }
   }

   // definition of jpeg image component
   struct stbi__jpeg_img_comp
   {
      public int id;
      public int h, v;
      public int tq;
      public int hd, ha;
      public int dc_pred;

      public int x, y, w2, h2;
      public BytePtr data;
      public BytePtr raw_data, raw_coeff;
      public BytePtr linebuf;
      public BytePtrConvert<short> coeff;   // progressive only
      public int coeff_w, coeff_h; // number of 8x8 coefficient blocks
   }

   public delegate void idct_block_kernel_delegate(BytePtr _out, int out_stride, Span<short> data);
   public delegate void YCbCr_to_RGB_kernel_delegate(BytePtr _out, Span<byte> y, Span<byte> pcb, Span<byte> pcr, int count, int step);
   public delegate BytePtr resample_row_hv_2_kernel_delegate(BytePtr _out, Span<byte> in_near, Span<byte> in_far, int w, int hs);

   ref struct stbi__jpeg
   {
      public ref stbi__context s;
      public stbi__huffman[] huff_dc = new stbi__huffman[4] { new stbi__huffman(), new stbi__huffman(), new stbi__huffman(), new stbi__huffman() };
      public stbi__huffman[] huff_ac = new stbi__huffman[4] { new stbi__huffman(), new stbi__huffman(), new stbi__huffman(), new stbi__huffman() };
      public stbi__uint16[][] dequant;
      public stbi__int16[][] fast_ac;

      // sizes for components, interleaved MCUs
      public int img_h_max, img_v_max;
      public int img_mcu_x, img_mcu_y;
      public int img_mcu_w, img_mcu_h;

      // definition of jpeg image component
      public stbi__jpeg_img_comp[] img_comp = new stbi__jpeg_img_comp[4];

      public stbi__uint32 code_buffer; // jpeg entropy-coded buffer
      public int code_bits;   // number of valid bits
      public byte marker;      // marker seen while filling entropy buffer
      public int nomore;      // flag if we saw a marker so must stop

      public int progressive;
      public int spec_start;
      public int spec_end;
      public int succ_high;
      public int succ_low;
      public int eob_run;
      public int jfif;
      public int app14_color_transform; // Adobe APP14 tag
      public int rgb;

      public int scan_n;
      public int[] order = new int[4];
      public int restart_interval, todo;

      // kernels  
      public idct_block_kernel_delegate idct_block_kernel = (_, _, _) => { };
      public YCbCr_to_RGB_kernel_delegate YCbCr_to_RGB_kernel = (_, _, _, _, _, _) => { };
      public resample_row_hv_2_kernel_delegate resample_row_hv_2_kernel = (_, _, _, _, _) => BytePtr.Null;

      public stbi__jpeg(ref stbi__context s)
      {
         this.s = ref s;
         dequant = new stbi__uint16[4][];
         for (int i = 0; i < dequant.Length; i++)
         {
            dequant[i] = new stbi__uint16[64];
         }

         fast_ac = new stbi__int16[4][];
         for (int i = 0; i < fast_ac.Length; i++)
         {
            fast_ac[i] = new stbi__int16[1 << FAST_BITS];
         }
      }
   }

   static int stbi__build_huffman(ref stbi__huffman h, Span<int> count)
   {
      int i, j, k = 0;
      uint code;
      // build size list for each symbol (from JPEG spec)
      for (i = 0; i < 16; ++i)
      {
         for (j = 0; j < count[i]; ++j)
         {
            h.size[k++] = (stbi_uc)(i + 1);
            if (k >= 257) return stbi__err("bad size list", "Corrupt JPEG");
         }
      }
      h.size[k] = 0;

      // compute actual symbols (from jpeg spec)
      code = 0;
      k = 0;
      for (j = 1; j <= 16; ++j)
      {
         // compute delta to add to code to compute symbol id
         h.delta[j] = (int)(k - code);
         if (h.size[k] == j)
         {
            while (h.size[k] == j)
               h.code[k++] = (stbi__uint16)(code++);
            if (code - 1 >= (1u << j)) return stbi__err("bad code lengths", "Corrupt JPEG");
         }
         // compute largest code + 1 for this size, preshifted as needed later
         h.maxcode[j] = code << (16 - j);
         code <<= 1;
      }
      h.maxcode[j] = 0xffffffff;

      // build non-spec acceleration table; 255 is flag for not-accelerated
      //memset(h.fast, 255, 1 << FAST_BITS);
      Array.Fill<stbi_uc>(h.fast, 255);
      for (i = 0; i < k; ++i)
      {
         int s = h.size[i];
         if (s <= FAST_BITS)
         {
            int c = h.code[i] << (FAST_BITS - s);
            int m = 1 << (FAST_BITS - s);
            for (j = 0; j < m; ++j)
            {
               h.fast[c + j] = (stbi_uc)i;
            }
         }
      }
      return 1;
   }

   // build a table that decodes both magnitude and value of small ACs in
   // one go.
   static void stbi__build_fast_ac(stbi__int16[] fast_ac, ref stbi__huffman h)
   {
      int i;
      for (i = 0; i < (1 << FAST_BITS); ++i)
      {
         stbi_uc fast = h.fast[i];
         fast_ac[i] = 0;
         if (fast < 255)
         {
            int rs = h.values[fast];
            int run = (rs >> 4) & 15;
            int magbits = rs & 15;
            int len = h.size[fast];

            if (magbits != 0 && len + magbits <= FAST_BITS)
            {
               // magnitude code followed by receive_extend code
               int k = ((i << len) & ((1 << FAST_BITS) - 1)) >> (FAST_BITS - magbits);
               int m = 1 << (magbits - 1);
               if (k < m) k += (int)((~0U << magbits) + 1);
               // if the result is small enough, we can fit it in fast_ac table
               if (k >= -128 && k <= 127)
                  fast_ac[i] = (stbi__int16)((k * 256) + (run * 16) + (len + magbits));
            }
         }
      }
   }

   static void stbi__grow_buffer_unsafe(ref stbi__jpeg j)
   {
      do
      {
         uint b = j.nomore != 0 ? (uint)0 : stbi__get8(ref j.s);
         if (b == 0xff)
         {
            int c = stbi__get8(ref j.s);
            while (c == 0xff) c = stbi__get8(ref j.s); // consume fill bytes
            if (c != 0)
            {
               j.marker = (byte)c;
               j.nomore = 1;
               return;
            }
         }
         j.code_buffer |= b << (24 - j.code_bits);
         j.code_bits += 8;
      } while (j.code_bits <= 24);
   }

   // (1 << n) - 1
   static stbi__uint32[] stbi__bmask = [0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 4095, 8191, 16383, 32767, 65535];

   // decode a jpeg huffman value from the bitstream
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__jpeg_huff_decode(ref stbi__jpeg j, ref stbi__huffman h)
   {
      uint temp;
      int c, k;

      if (j.code_bits < 16) stbi__grow_buffer_unsafe(ref j);

      // look at the top FAST_BITS and determine what symbol ID it is,
      // if the code is <= FAST_BITS
      c = (int)((j.code_buffer >> (32 - FAST_BITS)) & ((1 << FAST_BITS) - 1));
      k = h.fast[c];
      if (k < 255)
      {
         int s = h.size[k];
         if (s > j.code_bits)
            return -1;
         j.code_buffer <<= s;
         j.code_bits -= s;
         return h.values[k];
      }

      // naive test is to shift the code_buffer down so k bits are
      // valid, then test against maxcode. To speed this up, we've
      // preshifted maxcode left so that it has (16-k) 0s at the
      // end; in other words, regardless of the number of bits, it
      // wants to be compared against something shifted to have 16;
      // that way we don't need to shift inside the loop.
      temp = j.code_buffer >> 16;
      for (k = FAST_BITS + 1; ; ++k)
         if (temp < h.maxcode[k])
            break;
      if (k == 17)
      {
         // error! code not found
         j.code_bits -= 16;
         return -1;
      }

      if (k > j.code_bits)
         return -1;

      // convert the huffman code to the symbol id
      c = (int)(((j.code_buffer >> (32 - k)) & stbi__bmask[k]) + h.delta[k]);
      if (c < 0 || c >= 256) // symbol id out of bounds!
         return -1;
      STBI_ASSERT((((j.code_buffer) >> (32 - h.size[c])) & stbi__bmask[h.size[c]]) == h.code[c]);

      // convert the id to a symbol
      j.code_bits -= k;
      j.code_buffer <<= k;
      return h.values[c];
   }

   // bias[n] = (-1<<n) + 1
   static int[] stbi__jbias = [0, -1, -3, -7, -15, -31, -63, -127, -255, -511, -1023, -2047, -4095, -8191, -16383, -32767];

   // combined JPEG 'receive' and JPEG 'extend', since baseline
   // always extends everything it receives.
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__extend_receive(ref stbi__jpeg j, int n)
   {
      uint k;
      int sgn;
      if (j.code_bits < n) stbi__grow_buffer_unsafe(ref j);
      if (j.code_bits < n) return 0; // ran out of bits from stream, return 0s intead of continuing

      sgn = (int)(j.code_buffer >> 31); // sign bit always in MSB; 0 if MSB clear (positive), 1 if MSB set (negative)
      k = stbi_lrot(j.code_buffer, n);
      j.code_buffer = k & ~stbi__bmask[n];
      k &= stbi__bmask[n];
      j.code_bits -= n;
      return (int)(k + (stbi__jbias[n] & (sgn - 1)));
   }

   // get some unsigned bits
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__jpeg_get_bits(ref stbi__jpeg j, int n)
   {
      uint k;
      if (j.code_bits < n) stbi__grow_buffer_unsafe(ref j);
      if (j.code_bits < n) return 0; // ran out of bits from stream, return 0s intead of continuing
      k = stbi_lrot(j.code_buffer, n);
      j.code_buffer = k & ~stbi__bmask[n];
      k &= stbi__bmask[n];
      j.code_bits -= n;
      return (int)k;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__jpeg_get_bit(ref stbi__jpeg j)
   {
      uint k;
      if (j.code_bits < 1) stbi__grow_buffer_unsafe(ref j);
      if (j.code_bits < 1) return 0; // ran out of bits from stream, return 0s intead of continuing
      k = j.code_buffer;
      j.code_buffer <<= 1;
      --j.code_bits;
      return (int)(k & 0x80000000);
   }

   // given a value that's at position X in the zigzag stream,
   // where does it appear in the 8x8 matrix coded as row-major?
   static stbi_uc[] stbi__jpeg_dezigzag =
   [
       0,  1,  8, 16,  9,  2,  3, 10,
      17, 24, 32, 25, 18, 11,  4,  5,
      12, 19, 26, 33, 40, 48, 41, 34,
      27, 20, 13,  6,  7, 14, 21, 28,
      35, 42, 49, 56, 57, 50, 43, 36,
      29, 22, 15, 23, 30, 37, 44, 51,
      58, 59, 52, 45, 38, 31, 39, 46,
      53, 60, 61, 54, 47, 55, 62, 63,
      // let corrupt input sample past end
      63, 63, 63, 63, 63, 63, 63, 63,
      63, 63, 63, 63, 63, 63, 63
   ];

   // decode one 64-entry block--
   static bool stbi__jpeg_decode_block(ref stbi__jpeg j, Span<short> data, ref stbi__huffman hdc, ref stbi__huffman hac, stbi__int16[] fac, int b, stbi__uint16[] dequant)
   {
      int diff, dc, k;
      int t;

      if (j.code_bits < 16) stbi__grow_buffer_unsafe(ref j);
      t = stbi__jpeg_huff_decode(ref j, ref hdc);
      if (t < 0 || t > 15) return stbi__err("bad huffman code", "Corrupt JPEG") != 0;

      // 0 all the ac values now so we can do it 32-bits at a time
      //memset(data,0,64*sizeof(data[0]));
      data.Clear();

      diff = t != 0 ? stbi__extend_receive(ref j, t) : 0;
      if (!stbi__addints_valid(j.img_comp[b].dc_pred, diff)) return stbi__err("bad delta", "Corrupt JPEG") != 0;
      dc = j.img_comp[b].dc_pred + diff;
      j.img_comp[b].dc_pred = dc;
      if (!stbi__mul2shorts_valid(dc, dequant[0])) return stbi__err("can't merge dc and ac", "Corrupt JPEG") != 0;
      data[0] = (short)(dc * dequant[0]);

      // decode AC components, see JPEG spec
      k = 1;
      do
      {
         int zig;
         int c, r, s;
         if (j.code_bits < 16) stbi__grow_buffer_unsafe(ref j);
         c = (int)((j.code_buffer >> (32 - FAST_BITS)) & ((1 << FAST_BITS) - 1));
         r = fac[c];
         if (r != 0)
         { // fast-AC path
            k += (r >> 4) & 15; // run
            s = r & 15; // combined length
            if (s > j.code_bits) return stbi__err("bad huffman code", "Combined length longer than code bits available") != 0;
            j.code_buffer <<= s;
            j.code_bits -= s;
            // decode into unzigzag'd location
            zig = stbi__jpeg_dezigzag[k++];
            data[zig] = (short)((r >> 8) * dequant[zig]);
         }
         else
         {
            int rs = stbi__jpeg_huff_decode(ref j, ref hac);
            if (rs < 0) return stbi__err("bad huffman code", "Corrupt JPEG") != 0;
            s = rs & 15;
            r = rs >> 4;
            if (s == 0)
            {
               if (rs != 0xf0) break; // end block
               k += 16;
            }
            else
            {
               k += r;
               // decode into unzigzag'd location
               zig = stbi__jpeg_dezigzag[k++];
               data[zig] = (short)(stbi__extend_receive(ref j, s) * dequant[zig]);
            }
         }
      } while (k < 64);
      return true;
   }

   static bool stbi__jpeg_decode_block_prog_dc(ref stbi__jpeg j, Span<short> data, ref stbi__huffman hdc, int b)
   {
      int diff, dc;
      int t;
      if (j.spec_end != 0) return stbi__err("can't merge dc and ac", "Corrupt JPEG") != 0;

      if (j.code_bits < 16) stbi__grow_buffer_unsafe(ref j);

      if (j.succ_high == 0)
      {
         // first scan for DC coefficient, must be first
         //memset(data,0,64*sizeof(data[0])); // 0 all the ac values now
         data.Clear();
         t = stbi__jpeg_huff_decode(ref j, ref hdc);
         if (t < 0 || t > 15) return stbi__err("can't merge dc and ac", "Corrupt JPEG") != 0;
         diff = t != 0 ? stbi__extend_receive(ref j, t) : 0;

         if (!stbi__addints_valid(j.img_comp[b].dc_pred, diff)) return stbi__err("bad delta", "Corrupt JPEG") != 0;
         dc = j.img_comp[b].dc_pred + diff;
         j.img_comp[b].dc_pred = dc;
         if (!stbi__mul2shorts_valid(dc, 1 << j.succ_low)) return stbi__err("can't merge dc and ac", "Corrupt JPEG") != 0;
         data[0] = (short)(dc * (1 << j.succ_low));
      }
      else
      {
         // refinement scan for DC coefficient
         if (stbi__jpeg_get_bit(ref j) != 0)
            data[0] += (short)(1 << j.succ_low);
      }
      return true;
   }

   // @OPTIMIZE: store non-zigzagged during the decode passes,
   // and only de-zigzag when dequantizing
   static bool stbi__jpeg_decode_block_prog_ac(ref stbi__jpeg j, Span<short> data, ref stbi__huffman hac, stbi__int16[] fac)
   {
      int k;
      if (j.spec_start == 0) return stbi__err("can't merge dc and ac", "Corrupt JPEG") != 0;

      if (j.succ_high == 0)
      {
         int shift = j.succ_low;

         if (j.eob_run != 0)
         {
            --j.eob_run;
            return true;
         }

         k = j.spec_start;
         do
         {
            int zig;
            int c, r, s;
            if (j.code_bits < 16) stbi__grow_buffer_unsafe(ref j);
            c = (int)((j.code_buffer >> (32 - FAST_BITS)) & ((1 << FAST_BITS) - 1));
            r = fac[c];
            if (r != 0)
            { // fast-AC path
               k += (r >> 4) & 15; // run
               s = r & 15; // combined length
               if (s > j.code_bits) return stbi__err("bad huffman code", "Combined length longer than code bits available") != 0;
               j.code_buffer <<= s;
               j.code_bits -= s;
               zig = stbi__jpeg_dezigzag[k++];
               data[zig] = (short)((r >> 8) * (1 << shift));
            }
            else
            {
               int rs = stbi__jpeg_huff_decode(ref j, ref hac);
               if (rs < 0) return stbi__err("bad huffman code", "Corrupt JPEG") != 0;
               s = rs & 15;
               r = rs >> 4;
               if (s == 0)
               {
                  if (r < 15)
                  {
                     j.eob_run = (1 << r);
                     if (r != 0)
                        j.eob_run += stbi__jpeg_get_bits(ref j, r);
                     --j.eob_run;
                     break;
                  }
                  k += 16;
               }
               else
               {
                  k += r;
                  zig = stbi__jpeg_dezigzag[k++];
                  data[zig] = (short)(stbi__extend_receive(ref j, s) * (1 << shift));
               }
            }
         } while (k <= j.spec_end);
      }
      else
      {
         // refinement scan for these AC coefficients

         short bit = (short)(1 << j.succ_low);

         if (j.eob_run != 0)
         {
            --j.eob_run;
            for (k = j.spec_start; k <= j.spec_end; ++k)
            {
               ref var p = ref data[stbi__jpeg_dezigzag[k]];
               if (p != 0)
                  if (stbi__jpeg_get_bit(ref j) != 0)
                     if ((p & bit) == 0)
                     {
                        if (p > 0)
                           p += bit;
                        else
                           p -= bit;
                     }
            }
         }
         else
         {
            k = j.spec_start;
            do
            {
               int r, s;
               int rs = stbi__jpeg_huff_decode(ref j, ref hac); // @OPTIMIZE see if we can use the fast path here, advance-by-r is so slow, eh
               if (rs < 0) return stbi__err("bad huffman code", "Corrupt JPEG") != 0;
               s = rs & 15;
               r = rs >> 4;
               if (s == 0)
               {
                  if (r < 15)
                  {
                     j.eob_run = (1 << r) - 1;
                     if (r != 0)
                        j.eob_run += stbi__jpeg_get_bits(ref j, r);
                     r = 64; // force end of block
                  }
                  else
                  {
                     // r=15 s=0 should write 16 0s, so we just do
                     // a run of 15 0s and then write s (which is 0),
                     // so we don't have to do anything special here
                  }
               }
               else
               {
                  if (s != 1) return stbi__err("bad huffman code", "Corrupt JPEG") != 0;
                  // sign bit
                  if (stbi__jpeg_get_bit(ref j) != 0)
                     s = bit;
                  else
                     s = -bit;
               }

               // advance by r
               while (k <= j.spec_end)
               {
                  ref var p = ref data[stbi__jpeg_dezigzag[k++]];
                  if (p != 0)
                  {
                     if (stbi__jpeg_get_bit(ref j) != 0)
                        if ((p & bit) == 0)
                        {
                           if (p > 0)
                              p += bit;
                           else
                              p -= bit;
                        }
                  }
                  else
                  {
                     if (r == 0)
                     {
                        p = (short)s;
                        break;
                     }
                     --r;
                  }
               }
            } while (k <= j.spec_end);
         }
      }
      return true;
   }

   // take a -128..127 value and stbi__clamp it and convert to 0..255
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static stbi_uc stbi__clamp(int x)
   {
      // trick to use a single test to catch both cases
      if ((uint)x > 255)
      {
         if (x < 0) return 0;
         if (x > 255) return 255;
      }
      return (stbi_uc)x;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__f2f(float x) => ((int)(((x) * 4096 + 0.5f)));

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__fsh(int x) => ((x) * 4096);

   // derived from jidctint -- DCT_ISLOW

   // Manually inlined the code below..
   // static void STBI__IDCT_1D(ref int s0,ref int s1,ref int s2,ref int s3,ref int s4,ref int s5,ref int s6,ref int s7)
   // {
   //    int t0,t1,t2,t3,p1,p2,p3,p4,p5,x0,x1,x2,x3; 
   //    p2 = s2;                                    
   //    p3 = s6;                                    
   //    p1 = (p2+p3) * stbi__f2f(0.5411961f);       
   //    t2 = p1 + p3*stbi__f2f(-1.847759065f);      
   //    t3 = p1 + p2*stbi__f2f( 0.765366865f);      
   //    p2 = s0;                                    
   //    p3 = s4;                                    
   //    t0 = stbi__fsh(p2+p3);                      
   //    t1 = stbi__fsh(p2-p3);                      
   //    x0 = t0+t3;                                 
   //    x3 = t0-t3;                                 
   //    x1 = t1+t2;                                 
   //    x2 = t1-t2;                                 
   //    t0 = s7;                                    
   //    t1 = s5;                                    
   //    t2 = s3;                                    
   //    t3 = s1;                                    
   //    p3 = t0+t2;                                 
   //    p4 = t1+t3;                                 
   //    p1 = t0+t3;                                 
   //    p2 = t1+t2;                                 
   //    p5 = (p3+p4)*stbi__f2f( 1.175875602f);      
   //    t0 = t0*stbi__f2f( 0.298631336f);           
   //    t1 = t1*stbi__f2f( 2.053119869f);           
   //    t2 = t2*stbi__f2f( 3.072711026f);           
   //    t3 = t3*stbi__f2f( 1.501321110f);           
   //    p1 = p5 + p1*stbi__f2f(-0.899976223f);      
   //    p2 = p5 + p2*stbi__f2f(-2.562915447f);      
   //    p3 = p3*stbi__f2f(-1.961570560f);           
   //    p4 = p4*stbi__f2f(-0.390180644f);           
   //    t3 += p1+p4;                                
   //    t2 += p2+p3;                                
   //    t1 += p2+p4;                                
   //    t0 += p1+p3;
   // }

   static void stbi__idct_block(BytePtr _out, int out_stride, Span<short> data)
   {
      int i;
      Span<int> val = stackalloc int[64];

      SpanPtr<int> v = val;

      SpanPtr<byte> o;

      SpanPtr<short> d = data;

      // columns
      for (i = 0; i < 8; ++i, ++d, ++v)
      {
         // if all zeroes, shortcut -- this avoids dequantizing 0s and IDCTing
         if (d[8] == 0 && d[16] == 0 && d[24] == 0 && d[32] == 0
              && d[40] == 0 && d[48] == 0 && d[56] == 0)
         {
            //    no shortcut                 0     seconds
            //    (1|2|3|4|5|6|7)==0          0     seconds
            //    all separate               -0.047 seconds
            //    1 && 2|3 && 4|5 && 6|7:    -0.047 seconds
            int dcterm = d[0] * 4;
            v[0] = v[8] = v[16] = v[24] = v[32] = v[40] = v[48] = v[56] = dcterm;
         }
         else
         {
            //STBI__IDCT_1D(,,,,,,);
            ref short s0 = ref d[0];
            ref short s1 = ref d[8];
            ref short s2 = ref d[16];
            ref short s3 = ref d[24];
            ref short s4 = ref d[32];
            ref short s5 = ref d[40];
            ref short s6 = ref d[48];
            ref short s7 = ref d[56];

            int t0, t1, t2, t3, p1, p2, p3, p4, p5, x0, x1, x2, x3;
            p2 = s2;
            p3 = s6;
            p1 = (p2 + p3) * stbi__f2f(0.5411961f);
            t2 = p1 + p3 * stbi__f2f(-1.847759065f);
            t3 = p1 + p2 * stbi__f2f(0.765366865f);
            p2 = s0;
            p3 = s4;
            t0 = stbi__fsh(p2 + p3);
            t1 = stbi__fsh(p2 - p3);
            x0 = t0 + t3;
            x3 = t0 - t3;
            x1 = t1 + t2;
            x2 = t1 - t2;
            t0 = s7;
            t1 = s5;
            t2 = s3;
            t3 = s1;
            p3 = t0 + t2;
            p4 = t1 + t3;
            p1 = t0 + t3;
            p2 = t1 + t2;
            p5 = (p3 + p4) * stbi__f2f(1.175875602f);
            t0 = t0 * stbi__f2f(0.298631336f);
            t1 = t1 * stbi__f2f(2.053119869f);
            t2 = t2 * stbi__f2f(3.072711026f);
            t3 = t3 * stbi__f2f(1.501321110f);
            p1 = p5 + p1 * stbi__f2f(-0.899976223f);
            p2 = p5 + p2 * stbi__f2f(-2.562915447f);
            p3 = p3 * stbi__f2f(-1.961570560f);
            p4 = p4 * stbi__f2f(-0.390180644f);
            t3 += p1 + p4;
            t2 += p2 + p3;
            t1 += p2 + p4;
            t0 += p1 + p3;

            // constants scaled things up by 1<<12; let's bring them back
            // down, but keep 2 extra bits of precision
            x0 += 512; x1 += 512; x2 += 512; x3 += 512;
            v[0] = (x0 + t3) >> 10;
            v[56] = (x0 - t3) >> 10;
            v[8] = (x1 + t2) >> 10;
            v[48] = (x1 - t2) >> 10;
            v[16] = (x2 + t1) >> 10;
            v[40] = (x2 - t1) >> 10;
            v[24] = (x3 + t0) >> 10;
            v[32] = (x3 - t0) >> 10;
         }
      }

      for (i = 0, v = val, o = _out; i < 8; ++i, v += 8, o += out_stride)
      {
         // no fast case since the first 1D IDCT spread components _out
         // STBI__IDCT_1D(ref v[0],ref v[1],ref v[2],ref v[3],ref v[4],ref v[5],ref v[6],ref v[7]);
         // static void STBI__IDCT_1D(ref int s0,ref int s1,ref int s2,ref int s3,ref int s4,ref int s5,ref int s6,ref int s7)

         ref int s0 = ref v[0];
         ref int s1 = ref v[1];
         ref int s2 = ref v[2];
         ref int s3 = ref v[3];
         ref int s4 = ref v[4];
         ref int s5 = ref v[5];
         ref int s6 = ref v[6];
         ref int s7 = ref v[7];

         int t0, t1, t2, t3, p1, p2, p3, p4, p5, x0, x1, x2, x3;
         p2 = s2;
         p3 = s6;
         p1 = (p2 + p3) * stbi__f2f(0.5411961f);
         t2 = p1 + p3 * stbi__f2f(-1.847759065f);
         t3 = p1 + p2 * stbi__f2f(0.765366865f);
         p2 = s0;
         p3 = s4;
         t0 = stbi__fsh(p2 + p3);
         t1 = stbi__fsh(p2 - p3);
         x0 = t0 + t3;
         x3 = t0 - t3;
         x1 = t1 + t2;
         x2 = t1 - t2;
         t0 = s7;
         t1 = s5;
         t2 = s3;
         t3 = s1;
         p3 = t0 + t2;
         p4 = t1 + t3;
         p1 = t0 + t3;
         p2 = t1 + t2;
         p5 = (p3 + p4) * stbi__f2f(1.175875602f);
         t0 = t0 * stbi__f2f(0.298631336f);
         t1 = t1 * stbi__f2f(2.053119869f);
         t2 = t2 * stbi__f2f(3.072711026f);
         t3 = t3 * stbi__f2f(1.501321110f);
         p1 = p5 + p1 * stbi__f2f(-0.899976223f);
         p2 = p5 + p2 * stbi__f2f(-2.562915447f);
         p3 = p3 * stbi__f2f(-1.961570560f);
         p4 = p4 * stbi__f2f(-0.390180644f);
         t3 += p1 + p4;
         t2 += p2 + p3;
         t1 += p2 + p4;
         t0 += p1 + p3;

         // constants scaled things up by 1<<12, plus we had 1<<2 from first
         // loop, plus horizontal and vertical each scale by sqrt(8) so together
         // we've got an extra 1<<3, so 1<<17 total we need to remove.
         // so we want to round that, which means adding 0.5 * 1<<17,
         // aka 65536. Also, we'll end up with -128 to 127 that we want
         // to encode as 0..255 by adding 128, so we'll add that before the shift
         x0 += 65536 + (128 << 17);
         x1 += 65536 + (128 << 17);
         x2 += 65536 + (128 << 17);
         x3 += 65536 + (128 << 17);
         // tried computing the shifts into temps, or'ing the temps to see
         // if any were _out of range, but that was slower
         o[0] = stbi__clamp((x0 + t3) >> 17);
         o[7] = stbi__clamp((x0 - t3) >> 17);
         o[1] = stbi__clamp((x1 + t2) >> 17);
         o[6] = stbi__clamp((x1 - t2) >> 17);
         o[2] = stbi__clamp((x2 + t1) >> 17);
         o[5] = stbi__clamp((x2 - t1) >> 17);
         o[3] = stbi__clamp((x3 + t0) >> 17);
         o[4] = stbi__clamp((x3 - t0) >> 17);
      }
   }

   const byte STBI__MARKER_none = 0xff;

   // if there's a pending marker from the entropy stream, return that
   // otherwise, fetch from the stream and get a marker. if there's no
   // marker, return 0xff, which is never a valid marker value
   static stbi_uc stbi__get_marker(ref stbi__jpeg j)
   {
      stbi_uc x;
      if (j.marker != STBI__MARKER_none) { x = j.marker; j.marker = STBI__MARKER_none; return x; }
      x = stbi__get8(ref j.s);
      if (x != 0xff) return STBI__MARKER_none;
      while (x == 0xff)
         x = stbi__get8(ref j.s); // consume repeated 0xff fill bytes
      return x;
   }

   // in each scan, we'll have scan_n components, and the order
   // of the components is specified by order[]
   static bool STBI__RESTART(int x) => ((x) >= 0xd0 && (x) <= 0xd7);

   // after a restart interval, stbi__jpeg_reset the entropy decoder and
   // the dc prediction
   static void stbi__jpeg_reset(ref stbi__jpeg j)
   {
      j.code_bits = 0;
      j.code_buffer = 0;
      j.nomore = 0;
      j.img_comp[0].dc_pred = j.img_comp[1].dc_pred = j.img_comp[2].dc_pred = j.img_comp[3].dc_pred = 0;
      j.marker = STBI__MARKER_none;
      j.todo = j.restart_interval != 0 ? j.restart_interval : 0x7fffffff;
      j.eob_run = 0;
      // no more than 1<<31 MCUs if no restart_interal? that's plenty safe,
      // since we don't even allow 1<<30 pixels
   }

   static bool stbi__parse_entropy_coded_data(ref stbi__jpeg z)
   {
      stbi__jpeg_reset(ref z);
      if (z.progressive == 0)
      {
         if (z.scan_n == 1)
         {
            int i, j;
            Span<short> data = new short[64];
            int n = z.order[0];
            // non-interleaved data, we just need to process one block at a time,
            // in trivial scanline order
            // number of blocks to do just depends on how many actual "pixels" this
            // component has, independent of interleaved MCU blocking and such
            int w = (z.img_comp[n].x + 7) >> 3;
            int h = (z.img_comp[n].y + 7) >> 3;
            for (j = 0; j < h; ++j)
            {
               for (i = 0; i < w; ++i)
               {
                  int ha = z.img_comp[n].ha;
                  if (!stbi__jpeg_decode_block(ref z, data, ref z.huff_dc[z.img_comp[n].hd], ref z.huff_ac[ha], z.fast_ac[ha], n, z.dequant[z.img_comp[n].tq]))
                     return false;

                  z.idct_block_kernel(z.img_comp[n].data + z.img_comp[n].w2 * j * 8 + i * 8, z.img_comp[n].w2, data);
                  // every data block is an MCU, so countdown the restart interval
                  if (--z.todo <= 0)
                  {
                     if (z.code_bits < 24) stbi__grow_buffer_unsafe(ref z);
                     // if it's NOT a restart, then just bail, so we get corrupt data
                     // rather than no data
                     if (!STBI__RESTART(z.marker)) return true;
                     stbi__jpeg_reset(ref z);
                  }
               }
            }
            return true;
         }
         else
         { // interleaved
            int i, j, k, x, y;
            Span<short> data = new short[64];
            for (j = 0; j < z.img_mcu_y; ++j)
            {
               for (i = 0; i < z.img_mcu_x; ++i)
               {
                  // scan an interleaved mcu... process scan_n components in order
                  for (k = 0; k < z.scan_n; ++k)
                  {
                     int n = z.order[k];
                     // scan out an mcu's worth of this component; that's just determined
                     // by the basic H and V specified for the component
                     for (y = 0; y < z.img_comp[n].v; ++y)
                     {
                        for (x = 0; x < z.img_comp[n].h; ++x)
                        {
                           int x2 = (i * z.img_comp[n].h + x) * 8;
                           int y2 = (j * z.img_comp[n].v + y) * 8;
                           int ha = z.img_comp[n].ha;
                           if (!stbi__jpeg_decode_block(ref z, data, ref z.huff_dc[z.img_comp[n].hd], ref z.huff_ac[ha], z.fast_ac[ha], n, z.dequant[z.img_comp[n].tq])) return false;
                           z.idct_block_kernel(z.img_comp[n].data + z.img_comp[n].w2 * y2 + x2, z.img_comp[n].w2, data);
                        }
                     }
                  }
                  // after all interleaved components, that's an interleaved MCU,
                  // so now count down the restart interval
                  if (--z.todo <= 0)
                  {
                     if (z.code_bits < 24) stbi__grow_buffer_unsafe(ref z);
                     if (!STBI__RESTART(z.marker)) return true;
                     stbi__jpeg_reset(ref z);
                  }
               }
            }
            return true;
         }
      }
      else
      {
         if (z.scan_n == 1)
         {
            int i, j;
            int n = z.order[0];
            // non-interleaved data, we just need to process one block at a time,
            // in trivial scanline order
            // number of blocks to do just depends on how many actual "pixels" this
            // component has, independent of interleaved MCU blocking and such
            int w = (z.img_comp[n].x + 7) >> 3;
            int h = (z.img_comp[n].y + 7) >> 3;
            for (j = 0; j < h; ++j)
            {
               for (i = 0; i < w; ++i)
               {
                  BytePtrConvert<short> data = z.img_comp[n].coeff + 64 * (i + j * z.img_comp[n].coeff_w);
                  if (z.spec_start == 0)
                  {
                     if (!stbi__jpeg_decode_block_prog_dc(ref z, data, ref z.huff_dc[z.img_comp[n].hd], n))
                        return false;
                  }
                  else
                  {
                     int ha = z.img_comp[n].ha;
                     if (!stbi__jpeg_decode_block_prog_ac(ref z, data, ref z.huff_ac[ha], z.fast_ac[ha]))
                        return false;
                  }
                  // every data block is an MCU, so countdown the restart interval
                  if (--z.todo <= 0)
                  {
                     if (z.code_bits < 24) stbi__grow_buffer_unsafe(ref z);
                     if (!STBI__RESTART(z.marker)) return true;
                     stbi__jpeg_reset(ref z);
                  }
               }
            }
            return true;
         }
         else
         { // interleaved
            int i, j, k, x, y;
            for (j = 0; j < z.img_mcu_y; ++j)
            {
               for (i = 0; i < z.img_mcu_x; ++i)
               {
                  // scan an interleaved mcu... process scan_n components in order
                  for (k = 0; k < z.scan_n; ++k)
                  {
                     int n = z.order[k];
                     // scan out an mcu's worth of this component; that's just determined
                     // by the basic H and V specified for the component
                     for (y = 0; y < z.img_comp[n].v; ++y)
                     {
                        for (x = 0; x < z.img_comp[n].h; ++x)
                        {
                           int x2 = (i * z.img_comp[n].h + x);
                           int y2 = (j * z.img_comp[n].v + y);
                           BytePtrConvert<short> data = z.img_comp[n].coeff + 64 * (x2 + y2 * z.img_comp[n].coeff_w);
                           if (!stbi__jpeg_decode_block_prog_dc(ref z, data, ref z.huff_dc[z.img_comp[n].hd], n))
                              return false;
                        }
                     }
                  }
                  // after all interleaved components, that's an interleaved MCU,
                  // so now count down the restart interval
                  if (--z.todo <= 0)
                  {
                     if (z.code_bits < 24) stbi__grow_buffer_unsafe(ref z);
                     if (!STBI__RESTART(z.marker)) return true;
                     stbi__jpeg_reset(ref z);
                  }
               }
            }
            return true;
         }
      }
   }

   static void stbi__jpeg_dequantize(Span<short> data, Span<stbi__uint16> dequant)
   {
      int i;
      for (i = 0; i < 64; ++i)
         data[i] = (short)(data[i] * dequant[i]);
   }

   static void stbi__jpeg_finish(ref stbi__jpeg z)
   {
      if (z.progressive != 0)
      {
         // dequantize and idct the data
         int i, j, n;
         for (n = 0; n < (int)z.s.img_n; ++n)
         {
            int w = (z.img_comp[n].x + 7) >> 3;
            int h = (z.img_comp[n].y + 7) >> 3;
            for (j = 0; j < h; ++j)
            {
               for (i = 0; i < w; ++i)
               {
                  BytePtrConvert<short> data = z.img_comp[n].coeff + 64 * (i + j * z.img_comp[n].coeff_w);
                  stbi__jpeg_dequantize(data, z.dequant[z.img_comp[n].tq]);
                  z.idct_block_kernel(z.img_comp[n].data + z.img_comp[n].w2 * j * 8 + i * 8, z.img_comp[n].w2, data);
               }
            }
         }
      }
   }

   static bool stbi__process_marker(ref stbi__jpeg z, int m)
   {
      int L;
      switch (m)
      {
         case STBI__MARKER_none: // no marker found
            return stbi__err("expected marker", "Corrupt JPEG") != 0;

         case 0xDD: // DRI - specify restart interval
            if (stbi__get16be(ref z.s) != 4) return stbi__err("bad DRI len", "Corrupt JPEG") != 0;
            z.restart_interval = stbi__get16be(ref z.s);
            return true;

         case 0xDB: // DQT - define quantization table
            L = stbi__get16be(ref z.s) - 2;
            while (L > 0)
            {
               int q = stbi__get8(ref z.s);
               int p = q >> 4;
               bool sixteen = (p != 0);
               int t = q & 15, i;
               if (p != 0 && p != 1) return stbi__err("bad DQT type", "Corrupt JPEG") != 0;
               if (t > 3) return stbi__err("bad DQT table", "Corrupt JPEG") != 0;

               for (i = 0; i < 64; ++i)
                  z.dequant[t][stbi__jpeg_dezigzag[i]] = (stbi__uint16)(sixteen ? stbi__get16be(ref z.s) : stbi__get8(ref z.s));
               L -= (sixteen ? 129 : 65);
            }
            return L == 0;

         case 0xC4: // DHT - define huffman table
            L = stbi__get16be(ref z.s) - 2;
            Span<int> sizes = stackalloc int[16];
            while (L > 0)
            {
               stbi_uc[] v;
               int i, n = 0;
               int q = stbi__get8(ref z.s);
               int tc = q >> 4;
               int th = q & 15;
               if (tc > 1 || th > 3) return stbi__err("bad DHT header", "Corrupt JPEG") != 0;
               for (i = 0; i < 16; ++i)
               {
                  sizes[i] = stbi__get8(ref z.s);
                  n += sizes[i];
               }
               if (n > 256) return stbi__err("bad DHT header", "Corrupt JPEG") != 0; // Loop over i < n would write past end of values!
               L -= 17;
               if (tc == 0)
               {
                  if (stbi__build_huffman(ref z.huff_dc[th], sizes) == 0) return false;
                  v = z.huff_dc[th].values;
               }
               else
               {
                  if (stbi__build_huffman(ref z.huff_ac[th], sizes) == 0) return false;
                  v = z.huff_ac[th].values;
               }
               for (i = 0; i < n; ++i)
                  v[i] = stbi__get8(ref z.s);
               if (tc != 0)
                  stbi__build_fast_ac(z.fast_ac[th], ref z.huff_ac[th]);
               L -= n;
            }
            return L == 0;
      }

      // check for comment block or APP blocks
      if ((m >= 0xE0 && m <= 0xEF) || m == 0xFE)
      {
         L = stbi__get16be(ref z.s);
         if (L < 2)
         {
            if (m == 0xFE)
               return stbi__err("bad COM len", "Corrupt JPEG") != 0;
            else
               return stbi__err("bad APP len", "Corrupt JPEG") != 0;
         }
         L -= 2;

         if (m == 0xE0 && L >= 5)
         { // JFIF APP0 segment
            Span<byte> tag = stackalloc byte[5] { (byte)'J', (byte)'F', (byte)'I', (byte)'F', (byte)'\0' };
            int ok = 1;
            int i;
            for (i = 0; i < 5; ++i)
               if (stbi__get8(ref z.s) != tag[i])
                  ok = 0;
            L -= 5;
            if (ok != 0)
               z.jfif = 1;
         }
         else if (m == 0xEE && L >= 12)
         { // Adobe APP14 segment
            Span<byte> tag = stackalloc byte[6] { (byte)'A', (byte)'d', (byte)'o', (byte)'b', (byte)'e', (byte)'\0' };
            int ok = 1;
            int i;
            for (i = 0; i < 6; ++i)
               if (stbi__get8(ref z.s) != tag[i])
                  ok = 0;
            L -= 6;
            if (ok != 0)
            {
               stbi__get8(ref z.s); // version
               stbi__get16be(ref z.s); // flags0
               stbi__get16be(ref z.s); // flags1
               z.app14_color_transform = stbi__get8(ref z.s); // color transform
               L -= 6;
            }
         }

         stbi__skip(ref z.s, L);
         return true;
      }

      return stbi__err("unknown marker", "Corrupt JPEG") != 0;
   }

   // after we see SOS
   static bool stbi__process_scan_header(ref stbi__jpeg z)
   {
      int i;
      int Ls = stbi__get16be(ref z.s);
      z.scan_n = stbi__get8(ref z.s);
      if (z.scan_n < 1 || z.scan_n > 4 || z.scan_n > (int)z.s.img_n) return stbi__err("bad SOS component count", "Corrupt JPEG") != 0;
      if (Ls != 6 + 2 * z.scan_n) return stbi__err("bad SOS len", "Corrupt JPEG") != 0;
      for (i = 0; i < z.scan_n; ++i)
      {
         int id = stbi__get8(ref z.s), which;
         int q = stbi__get8(ref z.s);
         for (which = 0; which < (int)z.s.img_n; ++which)
            if (z.img_comp[which].id == id)
               break;
         if (which == (int)z.s.img_n) return false; // no match
         z.img_comp[which].hd = q >> 4; if (z.img_comp[which].hd > 3) return stbi__err("bad DC huff", "Corrupt JPEG") != 0;
         z.img_comp[which].ha = q & 15; if (z.img_comp[which].ha > 3) return stbi__err("bad AC huff", "Corrupt JPEG") != 0;
         z.order[i] = which;
      }

      {
         int aa;
         z.spec_start = stbi__get8(ref z.s);
         z.spec_end = stbi__get8(ref z.s); // should be 63, but might be 0
         aa = stbi__get8(ref z.s);
         z.succ_high = (aa >> 4);
         z.succ_low = (aa & 15);
         if (z.progressive != 0)
         {
            if (z.spec_start > 63 || z.spec_end > 63 || z.spec_start > z.spec_end || z.succ_high > 13 || z.succ_low > 13)
               return stbi__err("bad SOS", "Corrupt JPEG") != 0;
         }
         else
         {
            if (z.spec_start != 0) return stbi__err("bad SOS", "Corrupt JPEG") != 0;
            if (z.succ_high != 0 || z.succ_low != 0) return stbi__err("bad SOS", "Corrupt JPEG") != 0;
            z.spec_end = 63;
         }
      }

      return true;
   }

   static int stbi__free_jpeg_components(ref stbi__jpeg z, int ncomp, int why)
   {
      int i;
      for (i = 0; i < ncomp; ++i)
      {
         if (!z.img_comp[i].raw_data.IsNull)
         {
            STBI_FREE(z.img_comp[i].raw_data);
            z.img_comp[i].raw_data = BytePtr.Null;
            z.img_comp[i].data = BytePtr.Null;
         }
         if (!z.img_comp[i].raw_coeff.IsNull)
         {
            STBI_FREE(z.img_comp[i].raw_coeff);
            z.img_comp[i].raw_coeff = BytePtr.Null;
            z.img_comp[i].coeff = BytePtrConvert<short>.Null;
         }
         if (!z.img_comp[i].linebuf.IsNull)
         {
            STBI_FREE(z.img_comp[i].linebuf);
            z.img_comp[i].linebuf = BytePtr.Null;
         }
      }
      return why;
   }

   static bool stbi__process_frame_header(ref stbi__jpeg z, STBI__SCAN scan)
   {
      ref stbi__context s = ref z.s;
      int Lf, p, i, q, h_max = 1, v_max = 1, c;
      Lf = stbi__get16be(ref s); if (Lf < 11) return stbi__err("bad SOF len", "Corrupt JPEG") != 0; // JPEG
      p = stbi__get8(ref s); if (p != 8) return stbi__err("only 8-bit", "JPEG format not supported: 8-bit only") != 0; // JPEG baseline
      s.img_y = (uint)stbi__get16be(ref s); if (s.img_y == 0) return stbi__err("no header height", "JPEG format not supported: delayed height") != 0; // Legal, but we don't handle it--but neither does IJG
      s.img_x = (uint)stbi__get16be(ref s); if (s.img_x == 0) return stbi__err("0 width", "Corrupt JPEG") != 0; // JPEG requires
      if (s.img_y > STBI_MAX_DIMENSIONS) return stbi__err("too large", "Very large image (corrupt?)") != 0;
      if (s.img_x > STBI_MAX_DIMENSIONS) return stbi__err("too large", "Very large image (corrupt?)") != 0;
      c = stbi__get8(ref s);
      if (c != 3 && c != 1 && c != 4) return stbi__err("bad component count", "Corrupt JPEG") != 0;
      s.img_n = (STBI_CHANNELS)c;
      for (i = 0; i < c; ++i)
      {
         z.img_comp[i].data = BytePtr.Null;
         z.img_comp[i].linebuf = BytePtr.Null;
      }

      if (Lf != 8 + 3 * (int)s.img_n) return stbi__err("bad SOF len", "Corrupt JPEG") != 0;

      z.rgb = 0;

      Span<byte> rgb = stackalloc byte[3] { (byte)'R', (byte)'G', (byte)'B' };

      for (i = 0; i < (int)s.img_n; ++i)
      {


         z.img_comp[i].id = stbi__get8(ref s);
         if ((int)s.img_n == 3 && z.img_comp[i].id == rgb[i])
            ++z.rgb;
         q = stbi__get8(ref s);
         z.img_comp[i].h = (q >> 4); if (z.img_comp[i].h == 0 || z.img_comp[i].h > 4) return stbi__err("bad H", "Corrupt JPEG") != 0;
         z.img_comp[i].v = q & 15; if (z.img_comp[i].v == 0 || z.img_comp[i].v > 4) return stbi__err("bad V", "Corrupt JPEG") != 0;
         z.img_comp[i].tq = stbi__get8(ref s); if (z.img_comp[i].tq > 3) return stbi__err("bad TQ", "Corrupt JPEG") != 0;
      }

      if (scan != STBI__SCAN.load) return true;

      if (!stbi__mad3sizes_valid((int)s.img_x, (int)s.img_y, (int)s.img_n, 0)) return stbi__err("too large", "Image too large to decode") != 0;

      for (i = 0; i < (int)s.img_n; ++i)
      {
         if (z.img_comp[i].h > h_max) h_max = z.img_comp[i].h;
         if (z.img_comp[i].v > v_max) v_max = z.img_comp[i].v;
      }

      // check that plane subsampling factors are integer ratios; our resamplers can't deal with fractional ratios
      // and I've never seen a non-corrupted JPEG file actually use them
      for (i = 0; i < (int)s.img_n; ++i)
      {
         if (h_max % z.img_comp[i].h != 0) return stbi__err("bad H", "Corrupt JPEG") != 0;
         if (v_max % z.img_comp[i].v != 0) return stbi__err("bad V", "Corrupt JPEG") != 0;
      }

      // compute interleaved mcu info
      z.img_h_max = h_max;
      z.img_v_max = v_max;
      z.img_mcu_w = h_max * 8;
      z.img_mcu_h = v_max * 8;
      // these sizes can't be more than 17 bits
      z.img_mcu_x = (int)((s.img_x + z.img_mcu_w - 1) / z.img_mcu_w);
      z.img_mcu_y = (int)((s.img_y + z.img_mcu_h - 1) / z.img_mcu_h);

      for (i = 0; i < (int)s.img_n; ++i)
      {
         // number of effective pixels (e.g. for non-interleaved MCU)
         z.img_comp[i].x = (int)((s.img_x * z.img_comp[i].h + h_max - 1) / h_max);
         z.img_comp[i].y = (int)((s.img_y * z.img_comp[i].v + v_max - 1) / v_max);
         // to simplify generation, we'll allocate enough memory to decode
         // the bogus oversized data from using interleaved MCUs and their
         // big blocks (e.g. a 16x16 iMCU on an image of width 33); we won't
         // discard the extra data until colorspace conversion
         //
         // img_mcu_x, img_mcu_y: <=17 bits; comp[i].h and .v are <=4 (checked earlier)
         // so these muls can't overflow with 32-bit ints (which we require)
         z.img_comp[i].w2 = z.img_mcu_x * z.img_comp[i].h * 8;
         z.img_comp[i].h2 = z.img_mcu_y * z.img_comp[i].v * 8;
         z.img_comp[i].coeff = BytePtrConvert<short>.Null;
         z.img_comp[i].raw_coeff = BytePtr.Null;
         z.img_comp[i].linebuf = BytePtr.Null;
         z.img_comp[i].raw_data = stbi__malloc_mad2(z.img_comp[i].w2, z.img_comp[i].h2, 15);
         if (z.img_comp[i].raw_data.IsNull)
            return stbi__free_jpeg_components(ref z, i + 1, stbi__err("outofmem", "Out of memory")) != 0;
         // align blocks for idct using mmx/sse
         z.img_comp[i].data = z.img_comp[i].raw_data + ((z.img_comp[i].raw_data + 15).Offset & ~15);
         if (z.progressive != 0)
         {
            // w2, h2 are multiples of 8 (see above)
            z.img_comp[i].coeff_w = z.img_comp[i].w2 / 8;
            z.img_comp[i].coeff_h = z.img_comp[i].h2 / 8;
            z.img_comp[i].raw_coeff = stbi__malloc_mad3(z.img_comp[i].w2, z.img_comp[i].h2, sizeof(short), 15);
            if (z.img_comp[i].raw_coeff.IsNull)
               return stbi__free_jpeg_components(ref z, i + 1, stbi__err("outofmem", "Out of memory")) != 0;
            z.img_comp[i].coeff = z.img_comp[i].raw_coeff + ((z.img_comp[i].raw_coeff + 15).Offset & ~15);
         }
      }

      return true;
   }

   // use comparisons since in some cases we handle more than one case (e.g. SOF)
   static bool stbi__DNL(int x) => ((x) == 0xdc);
   static bool stbi__SOI(int x) => ((x) == 0xd8);
   static bool stbi__EOI(int x) => ((x) == 0xd9);
   static bool stbi__SOF(int x) => ((x) == 0xc0 || (x) == 0xc1 || (x) == 0xc2);
   static bool stbi__SOS(int x) => ((x) == 0xda);

   static bool stbi__SOF_progressive(int x) => ((x) == 0xc2);

   static bool stbi__decode_jpeg_header(ref stbi__jpeg z, STBI__SCAN scan)
   {
      int m;
      z.jfif = 0;
      z.app14_color_transform = -1; // valid values are 0,1,2
      z.marker = STBI__MARKER_none; // initialize cached marker to empty
      m = stbi__get_marker(ref z);
      if (!stbi__SOI(m)) return stbi__err("no SOI", "Corrupt JPEG") != 0;
      if (scan == STBI__SCAN.type) return true;
      m = stbi__get_marker(ref z);
      while (!stbi__SOF(m))
      {
         if (!stbi__process_marker(ref z, m)) return false;
         m = stbi__get_marker(ref z);
         while (m == STBI__MARKER_none)
         {
            // some files have extra padding after their blocks, so ok, we'll scan
            if (stbi__at_eof(ref z.s)) return stbi__err("no SOF", "Corrupt JPEG") != 0;
            m = stbi__get_marker(ref z);
         }
      }
      z.progressive = stbi__SOF_progressive(m) ? 1 : 0;
      if (!stbi__process_frame_header(ref z, scan)) return false;
      return true;
   }

   static stbi_uc stbi__skip_jpeg_junk_at_end(ref stbi__jpeg j)
   {
      // some JPEGs have junk at end, skip over it but if we find what looks
      // like a valid marker, resume there
      while (!stbi__at_eof(ref j.s))
      {
         stbi_uc x = stbi__get8(ref j.s);
         while (x == 0xff)
         { // might be a marker
            if (stbi__at_eof(ref j.s)) return STBI__MARKER_none;
            x = stbi__get8(ref j.s);
            if (x != 0x00 && x != 0xff)
            {
               // not a stuffed zero or lead-in to another marker, looks
               // like an actual marker, return it
               return x;
            }
            // stuffed zero has x=0 now which ends the loop, meaning we go
            // back to regular scan loop.
            // repeated 0xff keeps trying to read the next byte of the marker.
         }
      }
      return STBI__MARKER_none;
   }

   // decode image to YCbCr format
   static bool stbi__decode_jpeg_image(ref stbi__jpeg j)
   {
      int m;
      for (m = 0; m < 4; m++)
      {
         j.img_comp[m].raw_data = BytePtr.Null;
         j.img_comp[m].raw_coeff = BytePtr.Null;
      }
      j.restart_interval = 0;
      if (!stbi__decode_jpeg_header(ref j, STBI__SCAN.load)) return false;
      m = stbi__get_marker(ref j);
      while (!stbi__EOI(m))
      {
         if (stbi__SOS(m))
         {
            if (!stbi__process_scan_header(ref j)) return false;
            if (!stbi__parse_entropy_coded_data(ref j)) return false;
            if (j.marker == STBI__MARKER_none)
            {
               j.marker = stbi__skip_jpeg_junk_at_end(ref j);
               // if we reach eof without hitting a marker, stbi__get_marker() below will fail and we'll eventually return 0
            }
            m = stbi__get_marker(ref j);
            if (STBI__RESTART(m))
               m = stbi__get_marker(ref j);
         }
         else if (stbi__DNL(m))
         {
            int Ld = stbi__get16be(ref j.s);
            stbi__uint32 NL = (uint)stbi__get16be(ref j.s);
            if (Ld != 4) return stbi__err("bad DNL len", "Corrupt JPEG") != 0;
            if (NL != j.s.img_y) return stbi__err("bad DNL height", "Corrupt JPEG") != 0;
            m = stbi__get_marker(ref j);
         }
         else
         {
            if (!stbi__process_marker(ref j, m)) return true;
            m = stbi__get_marker(ref j);
         }
      }
      if (j.progressive != 0)
         stbi__jpeg_finish(ref j);
      return true;
   }

   // static jfif-centered resampling (across block boundaries)

   delegate BytePtr resample_row_func(BytePtr _out, Span<byte> in0, Span<byte> in1, int w, int hs);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static stbi_uc stbi__div4(int x) => ((stbi_uc)((x) >> 2));

   static BytePtr resample_row_1(BytePtr _out, Span<byte> in_near, Span<byte> in_far, int w, int hs)
   {
      //STBI_NOTUSED(_out);
      //STBI_NOTUSED(in_far);
      //STBI_NOTUSED(w);
      //STBI_NOTUSED(hs);
      for (int i = 0; i < w; ++i)
      {
         _out[i].Ref = in_near[i];
      }

      // Used to be: return in_near.. but we can't return a span here :-(
      return _out;
   }

   static BytePtr stbi__resample_row_v_2(BytePtr _out, Span<byte> in_near, Span<byte> in_far, int w, int hs)
   {
      // need to generate two samples vertically for every one in input
      int i;
      //STBI_NOTUSED(hs);
      for (i = 0; i < w; ++i)
         _out[i].Ref = stbi__div4(3 * in_near[i] + in_far[i] + 2);
      return _out;
   }

   static BytePtr stbi__resample_row_h_2(BytePtr _out, Span<byte> in_near, Span<byte> in_far, int w, int hs)
   {
      // need to generate two samples horizontally for every one in input
      int i;
      var input = in_near;
      var __out = _out.Span;

      if (w == 1)
      {
         // if only one sample, can't do any interpolation
         __out[0] = __out[1] = input[0];
         return _out;
      }

      __out[0] = input[0];
      __out[1] = stbi__div4(input[0] * 3 + input[1] + 2);
      for (i = 1; i < w - 1; ++i)
      {
         int n = 3 * input[i] + 2;
         __out[i * 2 + 0] = stbi__div4(n + input[i - 1]);
         __out[i * 2 + 1] = stbi__div4(n + input[i + 1]);
      }
      __out[i * 2 + 0] = stbi__div4(input[w - 2] * 3 + input[w - 1] + 2);
      __out[i * 2 + 1] = input[w - 1];

      //STBI_NOTUSED(in_far);
      //STBI_NOTUSED(hs);
      return _out;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static stbi_uc stbi__div16(int x) => ((stbi_uc)((x) >> 4));

   static BytePtr stbi__resample_row_hv_2(BytePtr _out, Span<byte> in_near, Span<byte> in_far, int w, int hs)
   {
      var __out = _out.Span;
      // need to generate 2x2 samples for every one in input
      int i, t0, t1;
      if (w == 1)
      {
         __out[0] = __out[1] = stbi__div4(3 * in_near[0] + in_far[0] + 2);
         return _out;
      }

      t1 = 3 * in_near[0] + in_far[0];
      __out[0] = stbi__div4(t1 + 2);
      for (i = 1; i < w; ++i)
      {
         t0 = t1;
         t1 = 3 * in_near[i] + in_far[i];
         __out[i * 2 - 1] = stbi__div16(3 * t0 + t1 + 8);
         __out[i * 2] = stbi__div16(3 * t1 + t0 + 8);
      }
      __out[w * 2 - 1] = stbi__div4(t1 + 2);

      //STBI_NOTUSED(hs);
      return _out;
   }

   static BytePtr stbi__resample_row_generic(BytePtr _out, Span<byte> in_near, Span<byte> in_far, int w, int hs)
   {
      var __out = _out.Span;
      // resample with nearest-neighbor
      int i, j;
      //STBI_NOTUSED(in_far);
      for (i = 0; i < w; ++i)
         for (j = 0; j < hs; ++j)
            __out[i * hs + j] = in_near[i];
      return _out;
   }

   // this is a reduced-precision calculation of YCbCr-to-RGB introduced
   // to make sure the code produces the same results in both SIMD and scalar
   static int stbi__float2fixed(float x) => (((int)((x) * 4096.0f + 0.5f)) << 8);

   static void stbi__YCbCr_to_RGB_row(BytePtr _out, Span<byte> y, Span<byte> pcb, Span<byte> pcr, int count, int step)
   {
      int i;
      for (i = 0; i < count; ++i)
      {
         int y_fixed = (y[i] << 20) + (1 << 19); // rounding
         int r, g, b;
         int cr = pcr[i] - 128;
         int cb = pcb[i] - 128;
         r = y_fixed + cr * stbi__float2fixed(1.40200f);
         g = (int)(y_fixed + (cr * -stbi__float2fixed(0.71414f)) + ((cb * -stbi__float2fixed(0.34414f)) & 0xffff0000));
         b = y_fixed + cb * stbi__float2fixed(1.77200f);
         r >>= 20;
         g >>= 20;
         b >>= 20;
         if ((uint)r > 255) { if (r < 0) r = 0; else r = 255; }
         if ((uint)g > 255) { if (g < 0) g = 0; else g = 255; }
         if ((uint)b > 255) { if (b < 0) b = 0; else b = 255; }
         _out[0].Ref = (stbi_uc)r;
         _out[1].Ref = (stbi_uc)g;
         _out[2].Ref = (stbi_uc)b;
         _out[3].Ref = 255;
         _out += step;
      }
   }

   // set up the kernels
   static void stbi__setup_jpeg(ref stbi__jpeg j)
   {
      j.idct_block_kernel = stbi__idct_block;
      j.YCbCr_to_RGB_kernel = stbi__YCbCr_to_RGB_row;
      j.resample_row_hv_2_kernel = stbi__resample_row_hv_2;

#if STBI_SSE2
      if (stbi__sse2_available()) {
         j.idct_block_kernel = stbi__idct_simd;
         j.YCbCr_to_RGB_kernel = stbi__YCbCr_to_RGB_simd;
         j.resample_row_hv_2_kernel = stbi__resample_row_hv_2_simd;
      }
#endif

#if STBI_NEON
      j.idct_block_kernel = stbi__idct_simd;
      j.YCbCr_to_RGB_kernel = stbi__YCbCr_to_RGB_simd;
      j.resample_row_hv_2_kernel = stbi__resample_row_hv_2_simd;
#endif
   }

   // clean up the temporary component buffers
   static void stbi__cleanup_jpeg(ref stbi__jpeg j)
   {
      stbi__free_jpeg_components(ref j, (int)j.s.img_n, 0);
   }

   struct stbi__resample
   {
      public resample_row_hv_2_kernel_delegate resample;
      public BytePtr line0, line1;
      public int hs, vs;   // expansion factor in each axis
      public int w_lores; // horizontal pixels pre-expansion
      public int ystep;   // how far through vertical expansion we are
      public int ypos;    // which pre-expansion row we're on
   };

   // fast 0..255 * 0..255 => 0..255 rounded multiplication
   static stbi_uc stbi__blinn_8x8(int x, int y)
   {
      uint t = (uint)(x * y + 128);
      return (stbi_uc)((t + (t >> 8)) >> 8);
   }

   static BytePtr load_jpeg_image(ref stbi__jpeg z, out int out_x, out int out_y, out STBI_CHANNELS comp, STBI_CHANNELS req_comp)
   {
      STBI_CHANNELS n, decode_n;
      bool is_rgb;

      z.s.img_n = STBI_CHANNELS._default; // make stbi__cleanup_jpeg safe

      out_x = out_y = 0;
      comp = STBI_CHANNELS._default;

      // validate req_comp
      if ((int)req_comp < 0 || (int)req_comp > 4) return stbi__errpuc("bad req_comp", "Internal error");

      // load a jpeg image from whichever source, but leave in YCbCr format
      if (!stbi__decode_jpeg_image(ref z)) { stbi__cleanup_jpeg(ref z); return BytePtr.Null; }

      // determine actual number of components to generate
      n = req_comp != STBI_CHANNELS._default ? req_comp : (int)z.s.img_n >= 3 ? STBI_CHANNELS.rgb : STBI_CHANNELS.grey;

      is_rgb = z.s.img_n == STBI_CHANNELS.rgb && (z.rgb == (int)STBI_CHANNELS.rgb || (z.app14_color_transform == 0 && z.jfif == 0));

      if (z.s.img_n == STBI_CHANNELS.rgb && n < STBI_CHANNELS.rgb && !is_rgb)
         decode_n = STBI_CHANNELS.grey_alpha;
      else
         decode_n = z.s.img_n;

      // nothing to do if no components requested; check this now to avoid
      // accessing uninitialized coutput[0] later
      if (decode_n <= 0) { stbi__cleanup_jpeg(ref z); return BytePtr.Null; }

      // resample and color-convert
      {
         int k;
         int i, j;
         BytePtr output;
         Span<BytePtr> coutput = new BytePtr[4] { BytePtr.Null, BytePtr.Null, BytePtr.Null, BytePtr.Null };

         Span<stbi__resample> res_comp = new stbi__resample[4];

         for (k = 0; k < (int)decode_n; ++k)
         {
            ref stbi__resample r = ref res_comp[k];

            // allocate line buffer big enough for upsampling off the edges
            // with upsample factor of 4
            z.img_comp[k].linebuf = (BytePtr)stbi__malloc((int)(z.s.img_x + 3));
            if (z.img_comp[k].linebuf.IsNull) { stbi__cleanup_jpeg(ref z); return stbi__errpuc("outofmem", "Out of memory"); }

            r.hs = z.img_h_max / z.img_comp[k].h;
            r.vs = z.img_v_max / z.img_comp[k].v;
            r.ystep = r.vs >> 1;
            r.w_lores = (int)((z.s.img_x + r.hs - 1) / r.hs);
            r.ypos = 0;
            r.line0 = r.line1 = z.img_comp[k].data;

            if (r.hs == 1 && r.vs == 1) r.resample = resample_row_1;
            else if (r.hs == 1 && r.vs == 2) r.resample = stbi__resample_row_v_2;
            else if (r.hs == 2 && r.vs == 1) r.resample = stbi__resample_row_h_2;
            else if (r.hs == 2 && r.vs == 2) r.resample = z.resample_row_hv_2_kernel;
            else r.resample = stbi__resample_row_generic;
         }

         // can't error after this so, this is safe
         output = (BytePtr)stbi__malloc_mad3((int)n, (int)z.s.img_x, (int)z.s.img_y, 1);
         if (output.IsNull) { stbi__cleanup_jpeg(ref z); return stbi__errpuc("outofmem", "Out of memory"); }

         // now go ahead and resample
         for (j = 0; j < z.s.img_y; ++j)
         {
            BytePtr _out = output + (int)n * (int)z.s.img_x * (int)j;
            for (k = 0; k < (int)decode_n; ++k)
            {
               ref stbi__resample r = ref res_comp[k];
               bool y_bot = r.ystep >= (r.vs >> 1);
               coutput[k] = r.resample(z.img_comp[k].linebuf,
                                        y_bot ? r.line1 : r.line0,
                                        y_bot ? r.line0 : r.line1,
                                        r.w_lores, r.hs);
               if (++r.ystep >= r.vs)
               {
                  r.ystep = 0;
                  r.line0 = r.line1;
                  if (++r.ypos < z.img_comp[k].y)
                     r.line1 += z.img_comp[k].w2;
               }
            }
            if ((int)n >= 3)
            {
               BytePtr y = coutput[0];
               if (z.s.img_n == STBI_CHANNELS.rgb)
               {
                  if (is_rgb)
                  {
                     for (i = 0; i < z.s.img_x; ++i)
                     {
                        _out[0].Ref = y[i].Value;
                        _out[1].Ref = coutput[1][i].Value;
                        _out[2].Ref = coutput[2][i].Value;
                        _out[3].Ref = 255;
                        _out += (int)n;
                     }
                  }
                  else
                  {
                     z.YCbCr_to_RGB_kernel(_out, y, coutput[1], coutput[2], (int)z.s.img_x, (int)n);
                  }
               }
               else if (z.s.img_n == STBI_CHANNELS.rgb_alpha)
               {
                  if (z.app14_color_transform == 0)
                  { // CMYK
                     for (i = 0; i < z.s.img_x; ++i)
                     {
                        stbi_uc m = coutput[3][i].Value;
                        _out[0].Ref = stbi__blinn_8x8(coutput[0][i].Value, m);
                        _out[1].Ref = stbi__blinn_8x8(coutput[1][i].Value, m);
                        _out[2].Ref = stbi__blinn_8x8(coutput[2][i].Value, m);
                        _out[3].Ref = 255;
                        _out += (int)n;
                     }
                  }
                  else if (z.app14_color_transform == 2)
                  { // YCCK
                     z.YCbCr_to_RGB_kernel(_out, y, coutput[1], coutput[2], (int)z.s.img_x, (int)n);
                     for (i = 0; i < z.s.img_x; ++i)
                     {
                        stbi_uc m = coutput[3][i].Value;
                        _out[0].Ref = stbi__blinn_8x8(255 - _out[0].Value, m);
                        _out[1].Ref = stbi__blinn_8x8(255 - _out[1].Value, m);
                        _out[2].Ref = stbi__blinn_8x8(255 - _out[2].Value, m);
                        _out += (int)n;
                     }
                  }
                  else
                  { // YCbCr + alpha?  Ignore the fourth channel for now
                     z.YCbCr_to_RGB_kernel(_out, y, coutput[1], coutput[2], (int)z.s.img_x, (int)n);
                  }
               }
               else
                  for (i = 0; i < z.s.img_x; ++i)
                  {
                     _out[0].Ref = _out[1].Ref = _out[2].Ref = y[i].Value;
                     _out[3].Ref = 255; // not used if n==3
                     _out += (int)n;
                  }
            }
            else
            {
               if (is_rgb)
               {
                  if (n == STBI_CHANNELS.grey)
                     for (i = 0; i < z.s.img_x; ++i)
                        (_out++).Ref = stbi__compute_y(coutput[0][i].Value, coutput[1][i].Value, coutput[2][i].Value);
                  else
                  {
                     for (i = 0; i < z.s.img_x; ++i, _out += 2)
                     {
                        _out[0].Ref = stbi__compute_y(coutput[0][i].Value, coutput[1][i].Value, coutput[2][i].Value);
                        _out[1].Ref = 255;
                     }
                  }
               }
               else if (z.s.img_n == STBI_CHANNELS.rgb_alpha && z.app14_color_transform == 0)
               {
                  for (i = 0; i < z.s.img_x; ++i)
                  {
                     stbi_uc m = coutput[3][i].Value;
                     stbi_uc r = stbi__blinn_8x8(coutput[0][i].Value, m);
                     stbi_uc g = stbi__blinn_8x8(coutput[1][i].Value, m);
                     stbi_uc b = stbi__blinn_8x8(coutput[2][i].Value, m);
                     _out[0].Ref = stbi__compute_y(r, g, b);
                     _out[1].Ref = 255;
                     _out += (int)n;
                  }
               }
               else if (z.s.img_n == STBI_CHANNELS.rgb_alpha && z.app14_color_transform == 2)
               {
                  for (i = 0; i < z.s.img_x; ++i)
                  {
                     _out[0].Ref = stbi__blinn_8x8(255 - coutput[0][i].Value, coutput[3][i].Value);
                     _out[1].Ref = 255;
                     _out += (int)n;
                  }
               }
               else
               {
                  BytePtr y = coutput[0];
                  if (n == STBI_CHANNELS.grey)
                     for (i = 0; i < z.s.img_x; ++i) _out[i].Ref = y[i].Value;
                  else
                     for (i = 0; i < z.s.img_x; ++i) { (_out++).Ref = y[i].Value; (_out++).Ref = 255; }
               }
            }
         }
         stbi__cleanup_jpeg(ref z);
         out_x = (int)z.s.img_x;
         out_y = (int)z.s.img_y;
         comp = z.s.img_n >= STBI_CHANNELS.rgb ? STBI_CHANNELS.rgb : STBI_CHANNELS.grey; // report original components, not output
         return output;
      }
   }


   static bool stbi__jpeg_info_raw(ref stbi__jpeg j, out int x, out int y, out STBI_CHANNELS comp)
   {
      if (!stbi__decode_jpeg_header(ref j, STBI__SCAN.header))
      {
         stbi__rewind(ref j.s);
         x = y = 0;
         comp = STBI_CHANNELS._default;
         return false;
      }
      x = (int)j.s.img_x;
      y = (int)j.s.img_y;
      comp = j.s.img_n >= STBI_CHANNELS.rgb ? STBI_CHANNELS.rgb : STBI_CHANNELS.grey;
      return true;
   }


#endif

   // public domain zlib decode    v0.2  Sean Barrett 2006-11-18
   //    simple implementation
   //      - all input must be provided in an upfront buffer
   //      - all output is written to a single output buffer (can malloc/realloc)
   //    performance
   //      - fast huffman

#if !STBI_NO_ZLIB

   // fast-way is faster to check than jpeg huffman, but slow way is slower
   const int STBI__ZFAST_BITS = 9; // accelerate all cases in default tables
   const int STBI__ZFAST_MASK = ((1 << STBI__ZFAST_BITS) - 1);
   const int STBI__ZNSYMS = 288; // number of symbols in literal/length alphabet

   // zlib-style huffman encoding
   // (jpegs packs from left, zlib from right, so can't share code)
   struct stbi__zhuffman
   {
      public stbi__uint16[] fast = new stbi__uint16[1 << STBI__ZFAST_BITS];
      public stbi__uint16[] firstcode = new stbi__uint16[16];
      public int[] maxcode = new int[17];
      public stbi__uint16[] firstsymbol = new stbi__uint16[16];
      public stbi_uc[] size = new stbi_uc[STBI__ZNSYMS];
      public stbi__uint16[] value = new stbi__uint16[STBI__ZNSYMS];
      public stbi__zhuffman()
      {

      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__bitreverse16(int n)
   {
      n = ((n & 0xAAAA) >> 1) | ((n & 0x5555) << 1);
      n = ((n & 0xCCCC) >> 2) | ((n & 0x3333) << 2);
      n = ((n & 0xF0F0) >> 4) | ((n & 0x0F0F) << 4);
      n = ((n & 0xFF00) >> 8) | ((n & 0x00FF) << 8);
      return n;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__bit_reverse(int v, int bits)
   {
      STBI_ASSERT(bits <= 16);
      // to bit reverse n bits, reverse 16 and shift
      // e.g. 11 bits, bit reverse and shift away 5
      return stbi__bitreverse16(v) >> (16 - bits);
   }

   static bool stbi__zbuild_huffman(ref stbi__zhuffman z, Span<stbi_uc> sizelist, int num)
   {
      int i, k = 0;
      int code;
      Span<int> next_code = stackalloc int[16], sizes = stackalloc int[17];

      // DEFLATE spec for generating codes
      //memset(sizes, 0, sizeof(sizes));
      sizes.Clear();

      //memset(z.fast, 0, sizeof(z.fast));
      Array.Clear(z.fast);

      for (i = 0; i < num; ++i)
         ++sizes[sizelist[i]];
      sizes[0] = 0;
      for (i = 1; i < 16; ++i)
         if (sizes[i] > (1 << i))
            return stbi__err("bad sizes", "Corrupt PNG") != 0;
      code = 0;
      for (i = 1; i < 16; ++i)
      {
         next_code[i] = code;
         z.firstcode[i] = (stbi__uint16)code;
         z.firstsymbol[i] = (stbi__uint16)k;
         code = (code + sizes[i]);
         if (sizes[i] != 0)
            if (code - 1 >= (1 << i)) return stbi__err("bad codelengths", "Corrupt PNG") != 0;
         z.maxcode[i] = code << (16 - i); // preshift for inner loop
         code <<= 1;
         k += sizes[i];
      }
      z.maxcode[16] = 0x10000; // sentinel
      for (i = 0; i < num; ++i)
      {
         int s = sizelist[i];
         if (s != 0)
         {
            int c = next_code[s] - z.firstcode[s] + z.firstsymbol[s];
            stbi__uint16 fastv = (stbi__uint16)((s << 9) | i);
            z.size[c] = (stbi_uc)s;
            z.value[c] = (stbi__uint16)i;
            if (s <= STBI__ZFAST_BITS)
            {
               int j = stbi__bit_reverse(next_code[s], s);
               while (j < (1 << STBI__ZFAST_BITS))
               {
                  z.fast[j] = fastv;
                  j += (1 << s);
               }
            }
            ++next_code[s];
         }
      }
      return true;
   }

   // zlib-from-memory implementation for PNG reading
   //    because PNG allows splitting the zlib stream arbitrarily,
   //    and it's annoying structurally to have PNG call ZLIB call PNG,
   //    we require PNG read all the IDATs and combine them into a single
   //    memory buffer

   struct stbi__zbuf
   {
      public BytePtr zbuffer, zbuffer_end;
      public int num_bits;
      public bool hit_zeof_once;
      public stbi__uint32 code_buffer;

      public BytePtr zout;
      public BytePtr zout_start;
      public BytePtr zout_end;
      public bool z_expandable;

      public stbi__zhuffman z_length = new stbi__zhuffman();
      public stbi__zhuffman z_distance = new stbi__zhuffman();

      public stbi__zbuf()
      {
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static bool stbi__zeof(ref stbi__zbuf z)
   {
      return (z.zbuffer >= z.zbuffer_end);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static stbi_uc stbi__zget8(ref stbi__zbuf z)
   {
      return stbi__zeof(ref z) ? (byte)0 : (z.zbuffer++).Value;
   }

   static void stbi__fill_bits(ref stbi__zbuf z)
   {
      do
      {
         if (z.code_buffer >= (1U << z.num_bits))
         {
            z.zbuffer = z.zbuffer_end;  /* treat this as EOF so we fail. */
            return;
         }
         z.code_buffer |= (uint)stbi__zget8(ref z) << z.num_bits;
         z.num_bits += 8;
      } while (z.num_bits <= 24);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__zreceive(ref stbi__zbuf z, int n)
   {
      uint k;
      if (z.num_bits < n) stbi__fill_bits(ref z);
      k = z.code_buffer & (uint)((1 << n) - 1);
      z.code_buffer >>= n;
      z.num_bits -= n;
      return (int)k;
   }

   static int stbi__zhuffman_decode_slowpath(ref stbi__zbuf a, ref stbi__zhuffman z)
   {
      int b, s, k;
      // not resolved by fast table, so compute it the slow way
      // use jpeg approach, which requires MSbits at top
      k = stbi__bit_reverse((int)a.code_buffer, 16);
      for (s = STBI__ZFAST_BITS + 1; ; ++s)
         if (k < z.maxcode[s])
            break;
      if (s >= 16)
      {
         return -1; // invalid code!
                    // code size is s, so:
      }
      b = (k >> (16 - s)) - z.firstcode[s] + z.firstsymbol[s];
      if (b >= STBI__ZNSYMS)
      {
         return -1; // some data was corrupt somewhere!
      }
      if (z.size[b] != s)
      {
         return -1;  // was originally an assert, but report failure instead.
      }
      a.code_buffer >>= s;
      a.num_bits -= s;
      return z.value[b];
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   static int stbi__zhuffman_decode(ref stbi__zbuf a, ref stbi__zhuffman z)
   {
      int b, s;
      if (a.num_bits < 16)
      {
         if (stbi__zeof(ref a))
         {
            if (!a.hit_zeof_once)
            {
               // This is the first time we hit eof, insert 16 extra padding btis
               // to allow us to keep going; if we actually consume any of them
               // though, that is invalid data. This is caught later.
               a.hit_zeof_once = true;
               a.num_bits += 16; // add 16 implicit zero bits
            }
            else
            {
               // We already inserted our extra 16 padding bits and are again
               // out, this stream is actually prematurely terminated.
               return -1;
            }
         }
         else
         {
            stbi__fill_bits(ref a);
         }
      }
      b = z.fast[a.code_buffer & STBI__ZFAST_MASK];
      if (b != 0)
      {
         s = b >> 9;
         a.code_buffer >>= s;
         a.num_bits -= s;
         return b & 511;
      }
      return stbi__zhuffman_decode_slowpath(ref a, ref z);
   }

   static bool stbi__zexpand(ref stbi__zbuf z, BytePtr zout, int n)  // need to make room for n bytes
   {
      BytePtr q;
      uint cur, limit, old_limit;
      z.zout = zout;
      if (!z.z_expandable) return stbi__err("output buffer limit", "Corrupt PNG") != 0;
      cur = (uint)(z.zout.Offset - z.zout_start.Offset);
      limit = old_limit = (uint)(z.zout_end.Offset - z.zout_start.Offset);
      if (uint.MaxValue - cur < (uint)n) return stbi__err("outofmem", "Out of memory") != 0;
      while (cur + n > limit)
      {
         if (limit > uint.MaxValue / 2) return stbi__err("outofmem", "Out of memory") != 0;
         limit *= 2;
      }
      q = STBI_REALLOC_SIZED(z.zout_start, old_limit, limit);
      //STBI_NOTUSED(old_limit);
      if (q.IsNull) return stbi__err("outofmem", "Out of memory") != 0;
      z.zout_start = q;
      z.zout = q + cur;
      z.zout_end = q + limit;
      return true;
   }

   static int[] stbi__zlength_base = [3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0];

   static int[] stbi__zlength_extra =
      [0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, 0, 0];

   static int[] stbi__zdist_base = [1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577, 0, 0];

   static int[] stbi__zdist_extra = [0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13];

   static bool stbi__parse_huffman_block(ref stbi__zbuf a)
   {
      BytePtr zout = a.zout;
      for (; ; )
      {
         int z = stbi__zhuffman_decode(ref a, ref a.z_length);
         if (z < 256)
         {
            if (z < 0) return stbi__err("bad huffman code", "Corrupt PNG") != 0; // error in huffman codes
            if (zout >= a.zout_end)
            {
               if (!stbi__zexpand(ref a, zout, 1)) return false;
               zout = a.zout;
            }
            (zout++).Ref = (byte)z;
         }
         else
         {
            BytePtr p;
            int len, dist;
            if (z == 256)
            {
               a.zout = zout;
               if (a.hit_zeof_once && a.num_bits < 16)
               {
                  // The first time we hit zeof, we inserted 16 extra zero bits into our bit
                  // buffer so the decoder can just do its speculative decoding. But if we
                  // actually consumed any of those bits (which is the case when num_bits < 16),
                  // the stream actually read past the end so it is malformed.
                  return stbi__err("unexpected end", "Corrupt PNG") != 0;
               }
               return true;
            }
            if (z >= 286) return stbi__err("bad huffman code", "Corrupt PNG") != 0; // per DEFLATE, length codes 286 and 287 must not appear in compressed data
            z -= 257;
            len = stbi__zlength_base[z];
            if (stbi__zlength_extra[z] != 0) len += stbi__zreceive(ref a, stbi__zlength_extra[z]);
            z = stbi__zhuffman_decode(ref a, ref a.z_distance);
            if (z < 0 || z >= 30) return stbi__err("bad huffman code", "Corrupt PNG") != 0; // per DEFLATE, distance codes 30 and 31 must not appear in compressed data
            dist = stbi__zdist_base[z];
            if (stbi__zdist_extra[z] != 0) dist += stbi__zreceive(ref a, stbi__zdist_extra[z]);
            if (zout.Offset - a.zout_start.Offset < dist) return stbi__err("bad dist", "Corrupt PNG") != 0;
            if (len > a.zout_end.Offset - zout.Offset)
            {
               if (!stbi__zexpand(ref a, zout, len)) return false;
               zout = a.zout;
            }
            p = (zout - dist);
            if (dist == 1)
            { // run of one byte; common in images.
               stbi_uc v = p.Value;
               if (len != 0) { do (zout++).Ref = v; while (--len != 0); }
            }
            else
            {
               if (len != 0) { do (zout++).Ref = (p++).Value; while (--len != 0); }
            }
         }
      }
   }

   static stbi_uc[] length_dezigzag = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
   static bool stbi__compute_huffman_codes(ref stbi__zbuf a)
   {
      stbi__zhuffman z_codelength = new stbi__zhuffman();
      Span<stbi_uc> lencodes = stackalloc stbi_uc[286 + 32 + 137];//padding for maximum single op
      Span<stbi_uc> codelength_sizes = stackalloc stbi_uc[19];
      int i, n;

      int hlit = stbi__zreceive(ref a, 5) + 257;
      int hdist = stbi__zreceive(ref a, 5) + 1;
      int hclen = stbi__zreceive(ref a, 4) + 4;
      int ntot = hlit + hdist;

      //memset(codelength_sizes, 0, sizeof(codelength_sizes));
      codelength_sizes.Clear();

      for (i = 0; i < hclen; ++i)
      {
         int s = stbi__zreceive(ref a, 3);
         codelength_sizes[length_dezigzag[i]] = (stbi_uc)s;
      }
      if (!stbi__zbuild_huffman(ref z_codelength, codelength_sizes, 19)) return false;

      n = 0;
      while (n < ntot)
      {
         int c = stbi__zhuffman_decode(ref a, ref z_codelength);
         if (c < 0 || c >= 19) return stbi__err("bad codelengths", "Corrupt PNG") != 0;
         if (c < 16)
            lencodes[n++] = (stbi_uc)c;
         else
         {
            stbi_uc fill = 0;
            if (c == 16)
            {
               c = stbi__zreceive(ref a, 2) + 3;
               if (n == 0) return stbi__err("bad codelengths", "Corrupt PNG") != 0;
               fill = lencodes[n - 1];
            }
            else if (c == 17)
            {
               c = stbi__zreceive(ref a, 3) + 3;
            }
            else if (c == 18)
            {
               c = stbi__zreceive(ref a, 7) + 11;
            }
            else
            {
               return stbi__err("bad codelengths", "Corrupt PNG") != 0;
            }
            if (ntot - n < c) return stbi__err("bad codelengths", "Corrupt PNG") != 0;

            //memset(lencodes + n, fill, c);
            lencodes.Slice(n, c).Fill(fill);

            n += c;
         }
      }
      if (n != ntot) return stbi__err("bad codelengths", "Corrupt PNG") != 0;
      if (!stbi__zbuild_huffman(ref a.z_length, lencodes.Slice(0, hlit), hlit)) return false;
      if (!stbi__zbuild_huffman(ref a.z_distance, lencodes.Slice(hlit), hdist)) return false;
      return true;
   }

   static bool stbi__parse_uncompressed_block(ref stbi__zbuf a)
   {
      Span<stbi_uc> header = stackalloc stbi_uc[4];
      int len, nlen, k;
      if ((a.num_bits & 7) != 0)
         stbi__zreceive(ref a, a.num_bits & 7); // discard
                                                // drain the bit-packed data into header
      k = 0;
      while (a.num_bits > 0)
      {
         header[k++] = (stbi_uc)(a.code_buffer & 255); // suppress MSVC run-time check
         a.code_buffer >>= 8;
         a.num_bits -= 8;
      }
      if (a.num_bits < 0) return stbi__err("zlib corrupt", "Corrupt PNG") != 0;
      // now fill header the normal way
      while (k < 4)
         header[k++] = stbi__zget8(ref a);
      len = header[1] * 256 + header[0];
      nlen = header[3] * 256 + header[2];
      if (nlen != (len ^ 0xffff)) return stbi__err("zlib corrupt", "Corrupt PNG") != 0;
      if (a.zbuffer + len > a.zbuffer_end) return stbi__err("read past buffer", "Corrupt PNG") != 0;
      if (a.zout + len > a.zout_end)
         if (!stbi__zexpand(ref a, a.zout, len)) return false;

      memcpy(a.zout, a.zbuffer, len);

      a.zbuffer += len;
      a.zout += len;
      return true;
   }

   static bool stbi__parse_zlib_header(ref stbi__zbuf a)
   {
      int cmf = stbi__zget8(ref a);
      int cm = cmf & 15;
      /* int cinfo = cmf >> 4; */
      int flg = stbi__zget8(ref a);
      if (stbi__zeof(ref a)) return stbi__err("bad zlib header", "Corrupt PNG") != 0; // zlib spec
      if ((cmf * 256 + flg) % 31 != 0) return stbi__err("bad zlib header", "Corrupt PNG") != 0; // zlib spec
      if ((flg & 32) != 0) return stbi__err("no preset dict", "Corrupt PNG") != 0; // preset dictionary not allowed in png
      if (cm != 8) return stbi__err("bad compression", "Corrupt PNG") != 0; // DEFLATE required for png
                                                                            // window = 1 << (8 + cinfo)... but who cares, we fully buffer output
      return true;
   }

   static stbi_uc[] stbi__zdefault_length =
   [
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
   9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9, 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
   9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9, 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
   9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9, 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
   7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7, 7,7,7,7,7,7,7,7,8,8,8,8,8,8,8,8
   ];
   static stbi_uc[] stbi__zdefault_distance =
   [
   5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5
   ];
   /*
   Init algorithm:
   {
      int i;   // use <= to match clearly with spec
      for (i=0; i <= 143; ++i)     stbi__zdefault_length[i]   = 8;
      for (   ; i <= 255; ++i)     stbi__zdefault_length[i]   = 9;
      for (   ; i <= 279; ++i)     stbi__zdefault_length[i]   = 7;
      for (   ; i <= 287; ++i)     stbi__zdefault_length[i]   = 8;

      for (i=0; i <=  31; ++i)     stbi__zdefault_distance[i] = 5;
   }
   */

   static bool stbi__parse_zlib(ref stbi__zbuf a, bool parse_header)
   {
      int final, type;
      if (parse_header)
         if (!stbi__parse_zlib_header(ref a)) return false;
      a.num_bits = 0;
      a.code_buffer = 0;
      a.hit_zeof_once = false;
      do
      {
         final = stbi__zreceive(ref a, 1);
         type = stbi__zreceive(ref a, 2);
         if (type == 0)
         {
            if (!stbi__parse_uncompressed_block(ref a)) return false;
         }
         else if (type == 3)
         {
            return false;
         }
         else
         {
            if (type == 1)
            {
               // use fixed code lengths
               if (!stbi__zbuild_huffman(ref a.z_length, stbi__zdefault_length, STBI__ZNSYMS)) return false;
               if (!stbi__zbuild_huffman(ref a.z_distance, stbi__zdefault_distance, 32)) return false;
            }
            else
            {
               if (!stbi__compute_huffman_codes(ref a)) return false;
            }
            if (!stbi__parse_huffman_block(ref a)) return false;
         }
      } while (final != 0);
      return true;
   }

   static bool stbi__do_zlib(ref stbi__zbuf a, BytePtr obuf, int olen, bool exp, bool parse_header)
   {
      var output = new MemoryStream();

      var compressedBytes = a.zbuffer.Span.Slice(0, (a.zbuffer_end - a.zbuffer).Offset).ToArray();

      // TODO: Try to use the existing implementation so we don't depend on ZLibStream.. :-()
      if (parse_header)
         new ZLibStream(new MemoryStream(compressedBytes), CompressionMode.Decompress).CopyTo(output);
      else
         new DeflateStream(new MemoryStream(compressedBytes), CompressionMode.Decompress).CopyTo(output);

      var outputBytes = output.ToArray();

      a.zout_start = outputBytes;
      a.zout = a.zout_start + outputBytes.Length;

      return true;
      /*
            a.zout_start = obuf;
            a.zout = obuf;
            a.zout_end = obuf + olen;
            a.z_expandable = exp;

            return stbi__parse_zlib(ref a, parse_header);
      */
   }

   static public BytePtr stbi_zlib_decode_malloc_guesssize(BytePtr buffer, int len, int initial_size, out int outlen)
   {
      stbi__zbuf a = new stbi__zbuf();
      BytePtr p = stbi__malloc(initial_size);
      if (p.IsNull)
      {
         outlen = 0;
         return BytePtr.Null;
      }
      a.zbuffer = buffer;
      a.zbuffer_end = buffer + len;
      if (stbi__do_zlib(ref a, p, initial_size, true, true))
      {
         outlen = (int)(a.zout - a.zout_start).Offset;
         return a.zout_start;
      }
      else
      {
         STBI_FREE(a.zout_start);
         outlen = 0;
         return BytePtr.Null;
      }
   }

#endif

   // public domain "baseline" PNG decoder   v0.10  Sean Barrett 2006-11-18
   //    simple implementation
   //      - only 8-bit samples
   //      - no CRC checking
   //      - allocates lots of intermediate memory
   //        - avoids problem of streaming data between subsystems
   //        - avoids explicit window management
   //    performance
   //      - uses stb_zlib, a PD zlib implementation with fast huffman decoding

#if !STBI_NO_PNG
   struct stbi__pngchunk
   {
      public stbi__uint32 length;
      public stbi__uint32 type;
   }
   ;

   static stbi__pngchunk stbi__get_chunk_header(ref stbi__context s)
   {
      stbi__pngchunk c;
      c.length = stbi__get32be(ref s);
      c.type = stbi__get32be(ref s);
      return c;
   }

   static stbi_uc[] png_sig = [137, 80, 78, 71, 13, 10, 26, 10];

   static bool stbi__check_png_header(ref stbi__context s)
   {
      int i;
      for (i = 0; i < 8; ++i)
         if (stbi__get8(ref s) != png_sig[i]) return stbi__err("bad png sig", "Not a PNG") != 0;
      return true;
   }

   ref struct stbi__png
   {
      public stbi__context s;
      public BytePtr idata, expanded, _out;
      public int depth;
   }


   enum STBI__F : stbi_uc
   {
      none = 0,
      sub = 1,
      up = 2,
      avg = 3,
      paeth = 4,
      // synthetic filter used for first scanline to avoid needing a dummy row of 0s
      avg_first
   };

   static stbi_uc[] first_row_filter =
   [
   (stbi_uc) STBI__F.none,
   (stbi_uc) STBI__F.sub,
   (stbi_uc) STBI__F.none,
   (stbi_uc) STBI__F.avg_first,
   (stbi_uc) STBI__F.sub // Paeth with b=c=0 turns out to be equivalent to sub
   ];

   static int stbi__paeth(int a, int b, int c)
   {
      // This formulation looks very different from the reference in the PNG spec, but is
      // actually equivalent and has favorable data dependencies and admits straightforward
      // generation of branch-free code, which helps performance significantly.
      int thresh = c * 3 - (a + b);
      int lo = a < b ? a : b;
      int hi = a < b ? b : a;
      int t0 = (hi <= thresh) ? lo : c;
      int t1 = (thresh <= lo) ? hi : t0;
      return t1;
   }

   static stbi_uc[] stbi__depth_scale_table = [0, 0xff, 0x55, 0, 0x11, 0, 0, 0, 0x01];

   // adds an extra all-255 alpha channel
   // dest == src is legal
   // img_n must be 1 or 3
   static void stbi__create_png_alpha_expand8(BytePtr dest, BytePtr src, stbi__uint32 x, STBI_CHANNELS img_n)
   {
      int i;
      // must process data backwards since we allow dest==src
      if (img_n == STBI_CHANNELS.grey)
      {
         for (i = (int)x - 1; i >= 0; --i)
         {
            dest[i * 2 + 1].Ref = 255;
            dest[i * 2 + 0].Ref = src[i].Value;
         }
      }
      else
      {
         STBI_ASSERT(img_n == STBI_CHANNELS.rgb);
         for (i = (int)x - 1; i >= 0; --i)
         {
            dest[i * 4 + 3].Ref = 255;
            dest[i * 4 + 2].Ref = src[i * 3 + 2].Value;
            dest[i * 4 + 1].Ref = src[i * 3 + 1].Value;
            dest[i * 4 + 0].Ref = src[i * 3 + 0].Value;
         }
      }
   }

   // create the png data from post-deflated data
   static bool stbi__create_png_image_raw(ref stbi__png a, BytePtr raw, stbi__uint32 raw_len, STBI_CHANNELS out_n, stbi__uint32 x, stbi__uint32 y, int depth, int color)
   {
      int bytes = (depth == 16 ? 2 : 1);
      ref stbi__context s = ref a.s;
      stbi__uint32 i, j, stride = (uint)(x * (int)out_n * bytes);
      stbi__uint32 img_len, img_width_bytes;
      BytePtr filter_buf;
      bool all_ok = true;
      int k;
      STBI_CHANNELS img_n = s.img_n; // copy it into a local for later

      int output_bytes = (int)out_n * bytes;
      int filter_bytes = (int)img_n * bytes;
      int width = (int)x;

      STBI_ASSERT(out_n == s.img_n || out_n == s.img_n + 1);
      a._out = (BytePtr)stbi__malloc_mad3((int)x, (int)y, output_bytes, 0); // extra bytes to write off the end into
      if (a._out.IsNull) return stbi__err("outofmem", "Out of memory") != 0;

      // note: error exits here don't need to clean up a.out individually,
      // stbi__do_png always does on error.
      if (!stbi__mad3sizes_valid((int)img_n, (int)x, depth, 7)) return stbi__err("too large", "Corrupt PNG") != 0;
      img_width_bytes = (uint)((((int)img_n * x * depth) + 7) >> 3);
      if (!stbi__mad2sizes_valid((int)img_width_bytes, (int)y, (int)img_width_bytes)) return stbi__err("too large", "Corrupt PNG") != 0;
      img_len = (img_width_bytes + 1) * y;

      // we used to check for exact match between raw_len and img_len on non-interlaced PNGs,
      // but issue #276 reported a PNG in the wild that had extra data at the end (all zeros),
      // so just check for raw_len < img_len always.
      if (raw_len < img_len) return stbi__err("not enough pixels", "Corrupt PNG") != 0;

      // Allocate two scan lines worth of filter workspace buffer.
      filter_buf = (BytePtr)stbi__malloc_mad2((int)img_width_bytes, 2, 0);
      if (filter_buf.IsNull) return stbi__err("outofmem", "Out of memory") != 0;

      // Filtering for low-bit-depth images
      if (depth < 8)
      {
         filter_bytes = 1;
         width = (int)img_width_bytes;
      }

      for (j = 0; j < y; ++j)
      {
         // cur/prior filter buffers alternate
         BytePtr cur = filter_buf + (j & 1) * img_width_bytes;
         BytePtr prior = filter_buf + (~j & 1) * img_width_bytes;
         BytePtr dest = a._out + stride * j;
         int nk = width * filter_bytes;
         int filter = (raw++).Value;

         // check filter type
         if (filter > 4)
         {
            all_ok = stbi__err("invalid filter", "Corrupt PNG") != 0;
            break;
         }

         // if first row, use special filter that doesn't sample previous row
         if (j == 0) filter = first_row_filter[filter];

         // perform actual filtering
         switch ((STBI__F)filter)
         {
            case STBI__F.none:
               memcpy(cur, raw, nk);
               break;
            case STBI__F.sub:
               memcpy(cur, raw, filter_bytes);
               for (k = filter_bytes; k < nk; ++k)
                  cur[k].Ref = STBI__BYTECAST(raw[k].Value + cur[k - filter_bytes].Value);
               break;
            case STBI__F.up:
               for (k = 0; k < nk; ++k)
                  cur[k].Ref = STBI__BYTECAST(raw[k].Value + prior[k].Value);
               break;
            case STBI__F.avg:
               for (k = 0; k < filter_bytes; ++k)
                  cur[k].Ref = STBI__BYTECAST(raw[k].Value + (prior[k].Value >> 1));
               for (k = filter_bytes; k < nk; ++k)
                  cur[k].Ref = STBI__BYTECAST(raw[k].Value + ((prior[k].Value + cur[k - filter_bytes].Value) >> 1));
               break;
            case STBI__F.paeth:
               for (k = 0; k < filter_bytes; ++k)
                  cur[k].Ref = STBI__BYTECAST(raw[k].Value + prior[k].Value); // prior[k] == stbi__paeth(0,prior[k],0)
               for (k = filter_bytes; k < nk; ++k)
                  cur[k].Ref = STBI__BYTECAST(raw[k].Value + stbi__paeth(cur[k - filter_bytes].Value, prior[k].Value, prior[k - filter_bytes].Value));
               break;
            case STBI__F.avg_first:
               memcpy(cur, raw, filter_bytes);
               for (k = filter_bytes; k < nk; ++k)
                  cur[k].Ref = STBI__BYTECAST(raw[k].Value + (cur[k - filter_bytes].Value >> 1));
               break;
         }

         raw += nk;

         // expand decoded bits in cur to dest, also adding an extra alpha channel if desired
         if (depth < 8)
         {
            stbi_uc scale = (color == 0) ? stbi__depth_scale_table[depth] : (stbi_uc)1; // scale grayscale values to 0..255 range
            BytePtr _in = cur;
            BytePtr _out = dest;
            stbi_uc inb = 0;
            stbi__uint32 nsmp = (uint)(x * (int)img_n);

            // expand bits to bytes first
            if (depth == 4)
            {
               for (i = 0; i < nsmp; ++i)
               {
                  if ((i & 1) == 0) inb = (_in++).Value;
                  (_out++).Ref = (byte)(scale * (inb >> 4));
                  inb <<= 4;
               }
            }
            else if (depth == 2)
            {
               for (i = 0; i < nsmp; ++i)
               {
                  if ((i & 3) == 0) inb = (_in++).Value;
                  (_out++).Ref = (byte)(scale * (inb >> 6));
                  inb <<= 2;
               }
            }
            else
            {
               STBI_ASSERT(depth == 1);
               for (i = 0; i < nsmp; ++i)
               {
                  if ((i & 7) == 0) inb = (_in++).Value;
                  (_out++).Ref = (byte)(scale * (inb >> 7));
                  inb <<= 1;
               }
            }

            // insert alpha=255 values if desired
            if (img_n != out_n)
               stbi__create_png_alpha_expand8(dest, dest, x, img_n);
         }
         else if (depth == 8)
         {
            if (img_n == out_n)
               memcpy(dest, cur, (int)(x * (int)img_n));
            else
               stbi__create_png_alpha_expand8(dest, cur, x, img_n);
         }
         else if (depth == 16)
         {
            // convert the image data from big-endian to platform-native
            Span<stbi__uint16> dest16 = MemoryMarshal.Cast<byte, stbi__uint16>(dest.Span);
            stbi__uint32 nsmp = (uint)(x * (int)img_n);

            if (img_n == out_n)
            {
               int dest16o = 0;
               for (i = 0; i < nsmp; ++i, ++dest16o, cur += 2)
                  dest16[dest16o] = (ushort)((cur[0].Value << 8) | cur[1].Value);
            }
            else
            {
               STBI_ASSERT(img_n + 1 == out_n);
               if (img_n == STBI_CHANNELS.grey)
               {
                  int dest16o = 0;
                  for (i = 0; i < x; ++i, dest16o += 2, cur += 2)
                  {
                     dest16[dest16o + 0] = (ushort)((cur[0].Value << 8) | cur[1].Value);
                     dest16[dest16o + 1] = 0xffff;
                  }
               }
               else
               {
                  STBI_ASSERT(img_n == STBI_CHANNELS.rgb);
                  int dest16o = 0;
                  for (i = 0; i < x; ++i, dest16o += 4, cur += 6)
                  {
                     dest16[dest16o + 0] = (ushort)((cur[0].Value << 8) | cur[1].Value);
                     dest16[dest16o + 1] = (ushort)((cur[2].Value << 8) | cur[3].Value);
                     dest16[dest16o + 2] = (ushort)((cur[4].Value << 8) | cur[5].Value);
                     dest16[dest16o + 3] = 0xffff;
                  }
               }
            }
         }
      }

      STBI_FREE(filter_buf);
      if (!all_ok) return false;

      return true;
   }

   static bool stbi__create_png_image(ref stbi__png a, BytePtr image_data, stbi__uint32 image_data_len, STBI_CHANNELS out_n, int depth, int color, bool interlaced)
   {
      int bytes = (depth == 16 ? 2 : 1);
      int out_bytes = (int)out_n * bytes;
      BytePtr final;
      int p;
      if (!interlaced)
         return stbi__create_png_image_raw(ref a, image_data, image_data_len, out_n, a.s.img_x, a.s.img_y, depth, color);

      // de-interlacing
      final = (BytePtr)stbi__malloc_mad3((int)a.s.img_x, (int)a.s.img_y, out_bytes, 0);
      if (final.IsNull) return stbi__err("outofmem", "Out of memory") != 0;
      for (p = 0; p < 7; ++p)
      {
         Span<int> xorig = stackalloc[] { 0, 4, 0, 2, 0, 1, 0 };
         Span<int> yorig = stackalloc[] { 0, 0, 4, 0, 2, 0, 1 };
         Span<int> xspc = stackalloc[] { 8, 8, 4, 4, 2, 2, 1 };
         Span<int> yspc = stackalloc[] { 8, 8, 8, 4, 4, 2, 2 };
         int i, j, x, y;
         // pass1_x[4] = 0, pass1_x[5] = 1, pass1_x[12] = 1
         x = (int)((a.s.img_x - xorig[p] + xspc[p] - 1) / xspc[p]);
         y = (int)((a.s.img_y - yorig[p] + yspc[p] - 1) / yspc[p]);
         if (x != 0 && y != 0)
         {
            stbi__uint32 img_len = (uint)((((((int)a.s.img_n * x * depth) + 7) >> 3) + 1) * y);
            if (!stbi__create_png_image_raw(ref a, image_data, image_data_len, out_n, (uint)x, (uint)y, depth, color))
            {
               STBI_FREE(final);
               return false;
            }
            for (j = 0; j < y; ++j)
            {
               for (i = 0; i < x; ++i)
               {
                  int out_y = j * yspc[p] + yorig[p];
                  int out_x = i * xspc[p] + xorig[p];
                  memcpy(final + out_y * (int)a.s.img_x * out_bytes + out_x * out_bytes,
                         a._out + (j * x + i) * out_bytes, out_bytes);
               }
            }
            STBI_FREE(a._out);
            image_data += img_len;
            image_data_len -= img_len;
         }
      }
      a._out = final;

      return true;
   }

   static bool stbi__compute_transparency(ref stbi__png z, Span<stbi_uc> tc, STBI_CHANNELS out_n)
   {
      ref stbi__context s = ref z.s;
      stbi__uint32 i, pixel_count = s.img_x * s.img_y;
      BytePtr p = z._out;

      // compute color-based transparency, assuming we've
      // already got 255 as the alpha value in the output
      STBI_ASSERT(out_n == STBI_CHANNELS.grey_alpha || out_n == STBI_CHANNELS.rgb_alpha);

      if (out_n == STBI_CHANNELS.grey_alpha)
      {
         for (i = 0; i < pixel_count; ++i)
         {
            p[1].Ref = (byte)(p[0].Value == tc[0] ? 0 : 255);
            p += 2;
         }
      }
      else
      {
         for (i = 0; i < pixel_count; ++i)
         {
            if (p[0].Value == tc[0] && p[1].Value == tc[1] && p[2].Value == tc[2])
               p[3].Ref = 0;
            p += 4;
         }
      }
      return true;
   }

   static bool stbi__compute_transparency16(ref stbi__png z, Span<stbi__uint16> tc, STBI_CHANNELS out_n)
   {
      ref stbi__context s = ref z.s;
      stbi__uint32 i, pixel_count = s.img_x * s.img_y;
      Span<stbi__uint16> p = MemoryMarshal.Cast<byte, stbi__uint16>(z._out.Span);

      // compute color-based transparency, assuming we've
      // already got 65535 as the alpha value in the output
      STBI_ASSERT(out_n == STBI_CHANNELS.grey_alpha || out_n == STBI_CHANNELS.rgb_alpha);

      int pOffset = 0;

      if (out_n == STBI_CHANNELS.grey_alpha)
      {
         for (i = 0; i < pixel_count; ++i)
         {
            p[pOffset + 1] = (ushort)(p[pOffset + 0] == tc[0] ? 0 : 65535);
            pOffset += 2;
         }
      }
      else
      {
         for (i = 0; i < pixel_count; ++i)
         {
            if (p[pOffset + 0] == tc[0] && p[pOffset + 1] == tc[1] && p[pOffset + 2] == tc[2])
               p[pOffset + 3] = 0;
            pOffset += 4;
         }
      }
      return true;
   }

   static bool stbi__expand_png_palette(ref stbi__png a, BytePtr palette, int len, STBI_CHANNELS pal_img_n)
   {
      int i;
      stbi__uint32 pixel_count = a.s.img_x * a.s.img_y;
      BytePtr p, temp_out, orig = a._out;

      p = (BytePtr)stbi__malloc_mad2((int)pixel_count, (int)pal_img_n, 0);
      if (p.IsNull) return stbi__err("outofmem", "Out of memory") != 0;

      // between here and free(out) below, exitting would leak
      temp_out = p;

      if (pal_img_n == STBI_CHANNELS.rgb)
      {
         for (i = 0; i < pixel_count; ++i)
         {
            int n = orig[i].Value * 4;
            p[0].Ref = palette[n].Value;
            p[1].Ref = palette[n + 1].Value;
            p[2].Ref = palette[n + 2].Value;
            p += 3;
         }
      }
      else
      {
         for (i = 0; i < pixel_count; ++i)
         {
            int n = orig[i].Value * 4;
            p[0].Ref = palette[n].Value;
            p[1].Ref = palette[n + 1].Value;
            p[2].Ref = palette[n + 2].Value;
            p[3].Ref = palette[n + 3].Value;
            p += 4;
         }
      }
      STBI_FREE(a._out);
      a._out = temp_out;

      //STBI_NOTUSED(len);

      return true;
   }


   // #if !STBI_THREAD_LOCAL
   // #define stbi__unpremultiply_on_load  stbi__unpremultiply_on_load_global
   // #define stbi__de_iphone_flag  stbi__de_iphone_flag_global
   // #else
   // static STBI_THREAD_LOCAL int stbi__unpremultiply_on_load_local, stbi__unpremultiply_on_load_set;
   // static STBI_THREAD_LOCAL int stbi__de_iphone_flag_local, stbi__de_iphone_flag_set;
   // 
   // static public void stbi_set_unpremultiply_on_load_thread(int flag_true_if_should_unpremultiply)
   // {
   //    stbi__unpremultiply_on_load_local = flag_true_if_should_unpremultiply;
   //    stbi__unpremultiply_on_load_set = 1;
   // }
   // 
   // static public void stbi_convert_iphone_png_to_rgb_thread(int flag_true_if_should_convert)
   // {
   //    stbi__de_iphone_flag_local = flag_true_if_should_convert;
   //    stbi__de_iphone_flag_set = 1;
   // }
   // 
   // #define stbi__unpremultiply_on_load  (stbi__unpremultiply_on_load_set           \
   //                                        ? stbi__unpremultiply_on_load_local      \
   //                                        : stbi__unpremultiply_on_load_global)
   // #define stbi__de_iphone_flag  (stbi__de_iphone_flag_set                         \
   //                                 ? stbi__de_iphone_flag_local                    \
   //                                 : stbi__de_iphone_flag_global)
   // #endif // STBI_THREAD_LOCAL

   static void stbi__de_iphone(ref stbi__png z)
   {
      ref stbi__context s = ref z.s;
      stbi__uint32 i, pixel_count = s.img_x * s.img_y;
      BytePtr p = z._out;

      if (s.img_out_n == STBI_CHANNELS.rgb)
      {  // convert bgr to rgb
         for (i = 0; i < pixel_count; ++i)
         {
            stbi_uc t = p[0].Value;
            p[0].Ref = p[2].Value;
            p[2].Ref = t;
            p += 3;
         }
      }
      else
      {
         STBI_ASSERT(s.img_out_n == STBI_CHANNELS.rgb_alpha);
         if (stbi__unpremultiply_on_load)
         {
            // convert bgr to rgb and unpremultiply
            for (i = 0; i < pixel_count; ++i)
            {
               stbi_uc a = p[3].Value;
               stbi_uc t = p[0].Value;
               if (a != 0)
               {
                  stbi_uc half = (byte)(a / 2);
                  p[0].Ref = (byte)((p[2].Value * 255 + half) / a);
                  p[1].Ref = (byte)((p[1].Value * 255 + half) / a);
                  p[2].Ref = (byte)((t * 255 + half) / a);
               }
               else
               {
                  p[0].Ref = p[2].Value;
                  p[2].Ref = t;
               }
               p += 4;
            }
         }
         else
         {
            // convert bgr to rgb
            for (i = 0; i < pixel_count; ++i)
            {
               stbi_uc t = p[0].Value;
               p[0].Ref = p[2].Value;
               p[2].Ref = t;
               p += 4;
            }
         }
      }
   }

   static uint STBI__PNG_TYPE(int a, int b, int c, int d) => (((uint)(a) << 24) + ((uint)(b) << 16) + ((uint)(c) << 8) + (uint)(d));

   static bool stbi__parse_png_file(ref stbi__png z, STBI__SCAN scan, STBI_CHANNELS req_comp)
   {
      BytePtr palette = new BytePtr(1024);
      STBI_CHANNELS pal_img_n = 0;
      bool has_trans = false;
      stbi_uc[] tc = [0, 0, 0];
      stbi__uint16[] tc16 = [0, 0, 0];
      stbi__uint32 ioff = 0, idata_limit = 0, pal_len = 0;
      int i;
      int k, color = 0, interlace = 0;
      ref stbi__context s = ref z.s;
      bool first = true, is_iphone = false;

      z.expanded = BytePtr.Null;
      z.idata = BytePtr.Null;
      z._out = BytePtr.Null;

      if (!stbi__check_png_header(ref s)) return false;

      if (scan == STBI__SCAN.type) return true;

      for (; ; )
      {
         stbi__pngchunk c = stbi__get_chunk_header(ref s);

         if (c.type == STBI__PNG_TYPE('C', 'g', 'B', 'I'))
         {
            is_iphone = true;
            stbi__skip(ref s, (int)c.length);
         }
         else if (c.type == STBI__PNG_TYPE('I', 'H', 'D', 'R'))
         {
            int comp, filter;
            if (!first) return stbi__err("multiple IHDR", "Corrupt PNG") != 0;
            first = false;
            if (c.length != 13) return stbi__err("bad IHDR len", "Corrupt PNG") != 0;
            s.img_x = stbi__get32be(ref s);
            s.img_y = stbi__get32be(ref s);
            if (s.img_y > STBI_MAX_DIMENSIONS) return stbi__err("too large", "Very large image (corrupt?)") != 0;
            if (s.img_x > STBI_MAX_DIMENSIONS) return stbi__err("too large", "Very large image (corrupt?)") != 0;
            z.depth = stbi__get8(ref s); if (z.depth != 1 && z.depth != 2 && z.depth != 4 && z.depth != 8 && z.depth != 16) return stbi__err("1/2/4/8/16-bit only", "PNG not supported: 1/2/4/8/16-bit only") != 0;
            color = stbi__get8(ref s); if (color > 6) return stbi__err("bad ctype", "Corrupt PNG") != 0;
            if (color == 3 && z.depth == 16) return stbi__err("bad ctype", "Corrupt PNG") != 0;
            if (color == 3) pal_img_n = STBI_CHANNELS.rgb; else if ((color & 1) != 0) return stbi__err("bad ctype", "Corrupt PNG") != 0;
            comp = stbi__get8(ref s); if (comp != 0) return stbi__err("bad comp method", "Corrupt PNG") != 0;
            filter = stbi__get8(ref s); if (filter != 0) return stbi__err("bad filter method", "Corrupt PNG") != 0;
            interlace = stbi__get8(ref s); if (interlace > 1) return stbi__err("bad interlace method", "Corrupt PNG") != 0;
            if (s.img_x == 0 || s.img_y == 0) return stbi__err("0-pixel image", "Corrupt PNG") != 0;
            if (pal_img_n == 0)
            {
               s.img_n = (STBI_CHANNELS)(((color & 2) != 0 ? 3 : 1) + ((color & 4) != 0 ? 1 : 0));
               if ((1 << 30) / s.img_x / (int)s.img_n < s.img_y) return stbi__err("too large", "Image too large to decode") != 0;
            }
            else
            {
               // if paletted, then pal_n is our final components, and
               // img_n is # components to decompress/filter.
               s.img_n = STBI_CHANNELS.grey;
               if ((1 << 30) / s.img_x / 4 < s.img_y) return stbi__err("too large", "Corrupt PNG") != 0;
            }
            // even with SCAN_header, have to scan to see if we have a tRNS
         }
         else if (c.type == STBI__PNG_TYPE('P', 'L', 'T', 'E'))
         {
            if (first) return stbi__err("first not IHDR", "Corrupt PNG") != 0;
            if (c.length > 256 * 3) return stbi__err("invalid PLTE", "Corrupt PNG") != 0;
            pal_len = c.length / 3;
            if (pal_len * 3 != c.length) return stbi__err("invalid PLTE", "Corrupt PNG") != 0;
            for (i = 0; i < pal_len; ++i)
            {
               palette[i * 4 + 0].Ref = stbi__get8(ref s);
               palette[i * 4 + 1].Ref = stbi__get8(ref s);
               palette[i * 4 + 2].Ref = stbi__get8(ref s);
               palette[i * 4 + 3].Ref = 255;
            }
         }
         else if (c.type == STBI__PNG_TYPE('t', 'R', 'N', 'S'))
         {
            if (first) return stbi__err("first not IHDR", "Corrupt PNG") != 0;
            if (!z.idata.IsNull) return stbi__err("tRNS after IDAT", "Corrupt PNG") != 0;
            if (pal_img_n != 0)
            {
               if (scan == STBI__SCAN.header) { s.img_n = STBI_CHANNELS.rgb_alpha; return true; }
               if (pal_len == 0) return stbi__err("tRNS before PLTE", "Corrupt PNG") != 0;
               if (c.length > pal_len) return stbi__err("bad tRNS len", "Corrupt PNG") != 0;
               pal_img_n = STBI_CHANNELS.rgb_alpha;
               for (i = 0; i < c.length; ++i)
                  palette[i * 4 + 3].Ref = stbi__get8(ref s);
            }
            else
            {
               if (((int)s.img_n & 1) == 0) return stbi__err("tRNS with alpha", "Corrupt PNG") != 0;
               if (c.length != (stbi__uint32)s.img_n * 2) return stbi__err("bad tRNS len", "Corrupt PNG") != 0;
               has_trans = true;
               // non-paletted with tRNS = constant alpha. if header-scanning, we can stop now.
               if (scan == STBI__SCAN.header) { ++s.img_n; return true; }
               if (z.depth == 16)
               {
                  for (k = 0; k < (int)s.img_n && k < 3; ++k) // extra loop test to suppress false GCC warning
                     tc16[k] = (stbi__uint16)stbi__get16be(ref s); // copy the values as-is
               }
               else
               {
                  for (k = 0; k < (int)s.img_n && k < 3; ++k)
                     tc[k] = (stbi_uc)((stbi__get16be(ref s) & 255) * stbi__depth_scale_table[z.depth]); // non 8-bit images will be larger
               }
            }
         }
         else if (c.type == STBI__PNG_TYPE('I', 'D', 'A', 'T'))
         {
            if (first) return stbi__err("first not IHDR", "Corrupt PNG") != 0;
            if (pal_img_n != 0 && pal_len == 0) return stbi__err("no PLTE", "Corrupt PNG") != 0;
            if (scan == STBI__SCAN.header)
            {
               // header scan definitely stops at first IDAT
               if (pal_img_n != 0)
                  s.img_n = pal_img_n;
               return true;
            }
            if (c.length > (1u << 30)) return stbi__err("IDAT size limit", "IDAT section larger than 2^30 bytes") != 0;
            if ((int)(ioff + c.length) < (int)ioff) return false;
            if (ioff + c.length > idata_limit)
            {
               stbi__uint32 idata_limit_old = idata_limit;
               BytePtr p;
               if (idata_limit == 0) idata_limit = c.length > 4096 ? c.length : 4096;
               while (ioff + c.length > idata_limit)
                  idata_limit *= 2;
               //STBI_NOTUSED(idata_limit_old);
               p = (BytePtr)STBI_REALLOC_SIZED(z.idata, idata_limit_old, idata_limit); if (p.IsNull) return stbi__err("outofmem", "Out of memory") != 0;
               z.idata = p;
            }
            if (!stbi__getn(ref s, z.idata + ioff, (int)c.length)) return stbi__err("outofdata", "Corrupt PNG") != 0;
            ioff += c.length;
         }
         else if (c.type == STBI__PNG_TYPE('I', 'E', 'N', 'D'))
         {
            int raw_len, bpl;
            if (first) return stbi__err("first not IHDR", "Corrupt PNG") != 0;
            if (scan != STBI__SCAN.load) return true;
            if (z.idata.IsNull) return stbi__err("no IDAT", "Corrupt PNG") != 0;
            // initial guess for decoded data size to avoid unnecessary reallocs
            bpl = (int)((s.img_x * z.depth + 7) / 8); // bytes per line, per component
            raw_len = (int)(bpl * s.img_y * (int)s.img_n /* pixels */ + s.img_y) /* filter mode per row */;
            z.expanded = (BytePtr)stbi_zlib_decode_malloc_guesssize_headerflag((BytePtr)z.idata, (int)ioff, (int)raw_len, out raw_len, !is_iphone);
            if (z.expanded.IsNull) return false; // zlib should set error
            STBI_FREE(z.idata); z.idata = BytePtr.Null;
            if ((req_comp == s.img_n + 1 && req_comp != STBI_CHANNELS.rgb && pal_img_n == 0) || has_trans)
               s.img_out_n = s.img_n + 1;
            else
               s.img_out_n = s.img_n;
            if (!stbi__create_png_image(ref z, z.expanded, (uint)raw_len, s.img_out_n, z.depth, color, interlace != 0)) return false;
            if (has_trans)
            {
               if (z.depth == 16)
               {
                  if (!stbi__compute_transparency16(ref z, tc16, s.img_out_n)) return false;
               }
               else
               {
                  if (!stbi__compute_transparency(ref z, tc, s.img_out_n)) return false;
               }
            }
            if (is_iphone && stbi__de_iphone_flag && s.img_out_n >= STBI_CHANNELS.rgb)
               stbi__de_iphone(ref z);
            if (pal_img_n != 0)
            {
               // pal_img_n == 3 or 4
               s.img_n = pal_img_n; // record the actual colors we had
               s.img_out_n = pal_img_n;
               if (req_comp >= STBI_CHANNELS.rgb) s.img_out_n = req_comp;
               if (!stbi__expand_png_palette(ref z, palette, (int)pal_len, s.img_out_n))
                  return false;
            }
            else if (has_trans)
            {
               // non-paletted image with tRNS . source image has (constant) alpha
               ++s.img_n;
            }
            STBI_FREE(z.expanded); z.expanded = BytePtr.Null;
            // end of PNG chunk, read and skip CRC
            stbi__get32be(ref s);
            return true;
         }
         else
         {
            // if critical, fail
            if (first) return stbi__err("first not IHDR", "Corrupt PNG") != 0;
            if ((c.type & (1 << 29)) == 0)
            {
#if !STBI_NO_FAILURE_STRINGS
               // not threadsafe
               string invalid_chunk = $"{(char)(c.type >> 24)}{(char)((c.type >> 16) & 0xFF)}{(char)((c.type >> 8) & 0xFF)}{(char)((c.type >> 0) & 0xFF)} PNG chunk not known";
#endif
               return stbi__err(invalid_chunk, "PNG not supported: unknown PNG chunk type") != 0;
            }
            stbi__skip(ref s, (int)c.length);
         }

         // end of PNG chunk, read and skip CRC
         uint crc32 = stbi__get32be(ref s);

         Debug.WriteLine($"Decoded chunk {(char)(c.type >> 24)}{(char)((c.type >> 16) & 0xFF)}{(char)((c.type >> 8) & 0xFF)}{(char)((c.type >> 0) & 0xFF)} with a length of {c.length} and CRC32 {Convert.ToString(crc32, 16)}");
      }
   }

   static BytePtr stbi__do_png(ref stbi__png p, out int x, out int y, out STBI_CHANNELS n, STBI_CHANNELS req_comp, ref stbi__result_info ri)
   {
      BytePtr result = BytePtr.Null;
      x = y = 0;
      n = STBI_CHANNELS._default;
      if ((int)req_comp < 0 || (int)req_comp > 4) stbi__errpuc("bad req_comp", "Internal error");
      if (stbi__parse_png_file(ref p, STBI__SCAN.load, req_comp))
      {
         if (p.depth <= 8)
            ri.bits_per_channel = 8;
         else if (p.depth == 16)
            ri.bits_per_channel = 16;
         else
            return stbi__errpuc("bad bits_per_channel", "PNG not supported: unsupported color depth");
         result = p._out;
         p._out = BytePtr.Null;
         if (req_comp != 0 && req_comp != p.s.img_out_n)
         {
            if (ri.bits_per_channel == 8)
            {
               result = stbi__convert_format((BytePtr)result, p.s.img_out_n, req_comp, (int)p.s.img_x, (int)p.s.img_y);
            }
            else
            {
               // TODO: We are doing a memory allocations here..
               result =
                  MemoryMarshal.Cast<stbi__uint16, byte>(stbi__convert_format16(
                     new Ptr<stbi__uint16>(MemoryMarshal.Cast<byte, stbi__uint16>(result.Span).ToArray()),
                     p.s.img_out_n,
                     req_comp,
                     (int)p.s.img_x,
                     (int)p.s.img_y
                  ).Span).ToArray();
            }
            p.s.img_out_n = req_comp;
            if (result.IsNull) return result;
         }
         x = (int)p.s.img_x;
         y = (int)p.s.img_y;
         n = p.s.img_n;
      }
      STBI_FREE(p._out); p._out = BytePtr.Null;
      STBI_FREE(p.expanded); p.expanded = BytePtr.Null;
      STBI_FREE(p.idata); p.idata = BytePtr.Null;

      return result;
   }


   static bool stbi__png_info_raw(ref stbi__png p, out int x, out int y, out STBI_CHANNELS comp)
   {
      if (!stbi__parse_png_file(ref p, STBI__SCAN.header, 0))
      {
         stbi__rewind(ref p.s);
         x = y = 0;
         comp = STBI_CHANNELS._default;
         return false;
      }
      x = (int)p.s.img_x;
      y = (int)p.s.img_y;
      comp = p.s.img_n;
      return true;
   }
#endif

   // Microsoft/Windows BMP image

#if !STBI_NO_BMP
   static bool stbi__bmp_test_raw(ref stbi__context s)
   {
      bool r;
      int sz;
      if (stbi__get8(ref s) != 'B') return false;
      if (stbi__get8(ref s) != 'M') return false;
      stbi__get32le(ref s); // discard filesize
      stbi__get16le(ref s); // discard reserved
      stbi__get16le(ref s); // discard reserved
      stbi__get32le(ref s); // discard data offset
      sz = (int)stbi__get32le(ref s);
      r = (sz == 12 || sz == 40 || sz == 56 || sz == 108 || sz == 124);
      return r;
   }



   // returns 0..31 for the highest set bit
   static int stbi__high_bit(uint z)
   {
      int n = 0;
      if (z == 0) return -1;
      if (z >= 0x10000) { n += 16; z >>= 16; }
      if (z >= 0x00100) { n += 8; z >>= 8; }
      if (z >= 0x00010) { n += 4; z >>= 4; }
      if (z >= 0x00004) { n += 2; z >>= 2; }
      if (z >= 0x00002) { n += 1;/* >>=  1;*/ }
      return n;
   }

   static int stbi__bitcount(uint a)
   {
      a = (a & 0x55555555) + ((a >> 1) & 0x55555555); // max 2
      a = (a & 0x33333333) + ((a >> 2) & 0x33333333); // max 4
      a = (a + (a >> 4)) & 0x0f0f0f0f; // max 8 per 4, now 8 bits
      a = (a + (a >> 8)); // max 16 per 8 bits
      a = (a + (a >> 16)); // max 32 per 8 bits
      return (int)(a & 0xff);
   }

   // extract an arbitrarily-aligned N-bit value (N=bits)
   // from v, and then make it 8-bits long and fractionally
   // extend it to full full range.
   static uint[] mul_table = [
      0,
      0xff/*0b11111111*/, 0x55/*0b01010101*/, 0x49/*0b01001001*/, 0x11/*0b00010001*/,
      0x21/*0b00100001*/, 0x41/*0b01000001*/, 0x81/*0b10000001*/, 0x01/*0b00000001*/,
   ];
   static int[] shift_table = [
      0, 0,0,1,0,2,4,6,0,
   ];

   static int stbi__shiftsigned(uint v, int shift, int bits)
   {
      if (shift < 0)
         v <<= -shift;
      else
         v >>= shift;
      STBI_ASSERT(v < 256);
      v >>= (8 - bits);
      STBI_ASSERT(bits >= 0 && bits <= 8);
      return (int)(((uint)(v * mul_table[bits])) >> shift_table[bits]);
   }

   struct stbi__bmp_data
   {
      public int bpp, offset, hsz;
      public uint mr, mg, mb, ma, all_a;
      public int extra_read;
   }

   static bool stbi__bmp_set_mask_defaults(ref stbi__bmp_data info, int compress)
   {
      // BI_BITFIELDS specifies masks explicitly, don't override
      if (compress == 3)
         return true;

      if (compress == 0)
      {
         if (info.bpp == 16)
         {
            info.mr = 31u << 10;
            info.mg = 31u << 5;
            info.mb = 31u << 0;
         }
         else if (info.bpp == 32)
         {
            info.mr = 0xffu << 16;
            info.mg = 0xffu << 8;
            info.mb = 0xffu << 0;
            info.ma = 0xffu << 24;
            info.all_a = 0; // if all_a is 0 at end, then we loaded alpha channel but it was all 0
         }
         else
         {
            // otherwise, use defaults, which is all-0
            info.mr = info.mg = info.mb = info.ma = 0;
         }
         return true;
      }
      return false; // error
   }

   static bool stbi__bmp_parse_header(ref stbi__context s, ref stbi__bmp_data info)
   {
      int hsz;
      if (stbi__get8(ref s) != 'B' || stbi__get8(ref s) != 'M') return stbi__err("not BMP", "Corrupt BMP") != 0;
      stbi__get32le(ref s); // discard filesize
      stbi__get16le(ref s); // discard reserved
      stbi__get16le(ref s); // discard reserved
      info.offset = (int)stbi__get32le(ref s);
      info.hsz = hsz = (int)stbi__get32le(ref s);
      info.mr = info.mg = info.mb = info.ma = 0;
      info.extra_read = 14;

      if (info.offset < 0) return stbi__err("bad BMP", "bad BMP") != 0;

      if (hsz != 12 && hsz != 40 && hsz != 56 && hsz != 108 && hsz != 124) return stbi__err("unknown BMP", "BMP type not supported: unknown") != 0;
      if (hsz == 12)
      {
         s.img_x = (uint)stbi__get16le(ref s);
         s.img_y = (uint)stbi__get16le(ref s);
      }
      else
      {
         s.img_x = stbi__get32le(ref s);
         s.img_y = stbi__get32le(ref s);
      }
      if (stbi__get16le(ref s) != 1) return stbi__err("bad BMP", "bad BMP") != 0;
      info.bpp = stbi__get16le(ref s);
      if (hsz != 12)
      {
         int compress = (int)stbi__get32le(ref s);
         if (compress == 1 || compress == 2) return stbi__err("BMP RLE", "BMP type not supported: RLE") != 0;
         if (compress >= 4) return stbi__err("BMP JPEG/PNG", "BMP type not supported: unsupported compression") != 0; // this includes PNG/JPEG modes
         if (compress == 3 && info.bpp != 16 && info.bpp != 32) return stbi__err("bad BMP", "bad BMP") != 0; // bitfields requires 16 or 32 bits/pixel
         stbi__get32le(ref s); // discard sizeof
         stbi__get32le(ref s); // discard hres
         stbi__get32le(ref s); // discard vres
         stbi__get32le(ref s); // discard colorsused
         stbi__get32le(ref s); // discard max important
         if (hsz == 40 || hsz == 56)
         {
            if (hsz == 56)
            {
               stbi__get32le(ref s);
               stbi__get32le(ref s);
               stbi__get32le(ref s);
               stbi__get32le(ref s);
            }
            if (info.bpp == 16 || info.bpp == 32)
            {
               if (compress == 0)
               {
                  stbi__bmp_set_mask_defaults(ref info, compress);
               }
               else if (compress == 3)
               {
                  info.mr = stbi__get32le(ref s);
                  info.mg = stbi__get32le(ref s);
                  info.mb = stbi__get32le(ref s);
                  info.extra_read += 12;
                  // not documented, but generated by photoshop and handled by mspaint
                  if (info.mr == info.mg && info.mg == info.mb)
                  {
                     // ?!?!?
                     return stbi__err("bad BMP", "bad BMP") != 0;
                  }
               }
               else
                  return stbi__err("bad BMP", "bad BMP") != 0;
            }
         }
         else
         {
            // V4/V5 header
            int i;
            if (hsz != 108 && hsz != 124)
               return stbi__err("bad BMP", "bad BMP") != 0;
            info.mr = stbi__get32le(ref s);
            info.mg = stbi__get32le(ref s);
            info.mb = stbi__get32le(ref s);
            info.ma = stbi__get32le(ref s);
            if (compress != 3) // override mr/mg/mb unless in BI_BITFIELDS mode, as per docs
               stbi__bmp_set_mask_defaults(ref info, compress);
            stbi__get32le(ref s); // discard color space
            for (i = 0; i < 12; ++i)
               stbi__get32le(ref s); // discard color space parameters
            if (hsz == 124)
            {
               stbi__get32le(ref s); // discard rendering intent
               stbi__get32le(ref s); // discard offset of profile data
               stbi__get32le(ref s); // discard size of profile data
               stbi__get32le(ref s); // discard reserved
            }
         }
      }
      return true;
   }


#endif

   // Targa Truevision - TGA
   // by Jonathan Dummer
#if !STBI_NO_TGA
   // returns STBI_rgb or whatever, 0 on error
   static STBI_CHANNELS stbi__tga_get_comp(int bits_per_pixel, bool is_grey, out bool is_rgb16)
   {
      // only RGB or RGBA (incl. 16bit) or grey allowed
      is_rgb16 = false;
      switch (bits_per_pixel)
      {
         case 8: return STBI_CHANNELS.grey;
         case 16:
            if (is_grey) return STBI_CHANNELS.grey_alpha;
            goto case 15;
         // fallthrough
         case 15:
            is_rgb16 = true;
            return STBI_CHANNELS.rgb;
         case 24: // fallthrough
         case 32: return (STBI_CHANNELS)(bits_per_pixel / 8);
         default: return 0;
      }
   }




   // read 16bit value and convert to 24bit RGB
   static void stbi__tga_read_rgb16(ref stbi__context s, Span<byte> _out)
   {
      stbi__uint16 px = (stbi__uint16)stbi__get16le(ref s);
      stbi__uint16 fiveBitMask = 31;
      // we have 3 channels with 5bits each
      int r = (px >> 10) & fiveBitMask;
      int g = (px >> 5) & fiveBitMask;
      int b = px & fiveBitMask;
      // Note that this saves the data in RGB(A) order, so it doesn't need to be swapped later
      _out[0] = (stbi_uc)((r * 255) / 31);
      _out[1] = (stbi_uc)((g * 255) / 31);
      _out[2] = (stbi_uc)((b * 255) / 31);

      // some people claim that the most significant bit might be used for alpha
      // (possibly if an alpha-bit is set in the "image descriptor byte")
      // but that only made 16bit test images completely translucent..
      // so let's treat all 15 and 16bit TGAs as RGB with no alpha.
   }
#endif

   // *************************************************************************************************
   // Photoshop PSD loader -- PD by Thatcher Ulrich, integration by Nicolas Schulz, tweaked by STB

#if !STBI_NO_PSD
static int stbi__psd_test(ref stbi__context s)
{
   int r = (stbi__get32be(s) == 0x38425053);
   stbi__rewind(ref s);
   return r;
}

static int stbi__psd_decode_rle(ref stbi__context s, BytePtr p, int pixelCount)
{
   int count, nleft, len;

   count = 0;
   while ((nleft = pixelCount - count) > 0) {
      len = stbi__get8(ref s);
      if (len == 128) {
         // No-op.
      } else if (len < 128) {
         // Copy next len+1 bytes literally.
         len++;
         if (len > nleft) return 0; // corrupt data
         count += len;
         while (len) {
            *p = stbi__get8(ref s);
            p += 4;
            len--;
         }
      } else if (len > 128) {
         stbi_uc   val;
         // Next -len+1 bytes in the dest are replicated from next source byte.
         // (Interpret len as a negative 8-bit int.)
         len = 257 - len;
         if (len > nleft) return 0; // corrupt data
         val = stbi__get8(ref s);
         count += len;
         while (len) {
            *p = val;
            p += 4;
            len--;
         }
      }
   }

   return 1;
}

static void *stbi__psd_load(ref stbi__context s, int *x, int *y, int *comp, int req_comp, stbi__result_info *ri, int bpc)
{
   int pixelCount;
   int channelCount, compression;
   int channel, i;
   int bitdepth;
   int w,h;
   BytePtr out;
   STBI_NOTUSED(ri);

   // Check identifier
   if (stbi__get32be(s) != 0x38425053)   // "8BPS"
      return stbi__errpuc("not PSD", "Corrupt PSD image");

   // Check file type version.
   if (stbi__get16be(s) != 1)
      return stbi__errpuc("wrong version", "Unsupported version of PSD image");

   // Skip 6 reserved bytes.
   stbi__skip(ref s, 6 );

   // Read the number of channels (R, G, B, A, etc).
   channelCount = stbi__get16be(s);
   if (channelCount < 0 || channelCount > 16)
      return stbi__errpuc("wrong channel count", "Unsupported number of channels in PSD image");

   // Read the rows and columns of the image.
   h = stbi__get32be(s);
   w = stbi__get32be(s);

   if (h > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large","Very large image (corrupt?)");
   if (w > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large","Very large image (corrupt?)");

   // Make sure the depth is 8 bits.
   bitdepth = stbi__get16be(s);
   if (bitdepth != 8 && bitdepth != 16)
      return stbi__errpuc("unsupported bit depth", "PSD bit depth is not 8 or 16 bit");

   // Make sure the color mode is RGB.
   // Valid options are:
   //   0: Bitmap
   //   1: Grayscale
   //   2: Indexed color
   //   3: RGB color
   //   4: CMYK color
   //   7: Multichannel
   //   8: Duotone
   //   9: Lab color
   if (stbi__get16be(s) != 3)
      return stbi__errpuc("wrong color format", "PSD is not in RGB color format");

   // Skip the Mode Data.  (It's the palette for indexed color; other info for other modes.)
   stbi__skip(ref s,stbi__get32be(s) );

   // Skip the image resources.  (resolution, pen tool paths, etc)
   stbi__skip(ref s, stbi__get32be(s) );

   // Skip the reserved data.
   stbi__skip(ref s, stbi__get32be(s) );

   // Find out if the data is compressed.
   // Known values:
   //   0: no compression
   //   1: RLE compressed
   compression = stbi__get16be(s);
   if (compression > 1)
      return stbi__errpuc("bad compression", "PSD has an unknown compression format");

   // Check size
   if (!stbi__mad3sizes_valid(4, w, h, 0))
      return stbi__errpuc("too large", "Corrupt PSD");

   // Create the destination image.

   if (!compression && bitdepth == 16 && bpc == 16) {
      out = (BytePtr ) stbi__malloc_mad3(8, w, h, 0);
      ri.bits_per_channel = 16;
   } else
      out = (BytePtr ) stbi__malloc(4 * w*h);

   if (!out) return stbi__errpuc("outofmem", "Out of memory");
   pixelCount = w*h;

   // Initialize the data to zero.
   //memset( out, 0, pixelCount * 4 );

   // Finally, the image data.
   if (compression) {
      // RLE as used by .PSD and .TIFF
      // Loop until you get the number of unpacked bytes you are expecting:
      //     Read the next source byte into n.
      //     If n is between 0 and 127 inclusive, copy the next n+1 bytes literally.
      //     Else if n is between -127 and -1 inclusive, copy the next byte -n+1 times.
      //     Else if n is 128, noop.
      // Endloop

      // The RLE-compressed data is preceded by a 2-byte data count for each row in the data,
      // which we're going to just skip.
      stbi__skip(ref s, h * channelCount * 2 );

      // Read the RLE data by channel.
      for (channel = 0; channel < 4; channel++) {
         BytePtr p;

         p = out+channel;
         if (channel >= channelCount) {
            // Fill this channel with default data.
            for (i = 0; i < pixelCount; i++, p += 4)
               *p = (channel == 3 ? 255 : 0);
         } else {
            // Read the RLE data.
            if (!stbi__psd_decode_rle(s, p, pixelCount)) {
               STBI_FREE(out);
               return stbi__errpuc("corrupt", "bad RLE data");
            }
         }
      }

   } else {
      // We're at the raw image data.  It's each channel in order (Red, Green, Blue, Alpha, ...)
      // where each channel consists of an 8-bit (or 16-bit) value for each pixel in the image.

      // Read the data by channel.
      for (channel = 0; channel < 4; channel++) {
         if (channel >= channelCount) {
            // Fill this channel with default data.
            if (bitdepth == 16 && bpc == 16) {
               stbi__uint16 *q = ((stbi__uint16 *) out) + channel;
               stbi__uint16 val = channel == 3 ? 65535 : 0;
               for (i = 0; i < pixelCount; i++, q += 4)
                  *q = val;
            } else {
               BytePtr p = out+channel;
               stbi_uc val = channel == 3 ? 255 : 0;
               for (i = 0; i < pixelCount; i++, p += 4)
                  *p = val;
            }
         } else {
            if (ri.bits_per_channel == 16) {    // output bpc
               stbi__uint16 *q = ((stbi__uint16 *) out) + channel;
               for (i = 0; i < pixelCount; i++, q += 4)
                  *q = (stbi__uint16) stbi__get16be(s);
            } else {
               BytePtr p = out+channel;
               if (bitdepth == 16) {  // input bpc
                  for (i = 0; i < pixelCount; i++, p += 4)
                     *p = (stbi_uc) (stbi__get16be(s) >> 8);
               } else {
                  for (i = 0; i < pixelCount; i++, p += 4)
                     *p = stbi__get8(ref s);
               }
            }
         }
      }
   }

   // remove weird white matte from PSD
   if (channelCount >= 4) {
      if (ri.bits_per_channel == 16) {
         for (i=0; i < w*h; ++i) {
            stbi__uint16 *pixel = (stbi__uint16 *) out + 4*i;
            if (pixel[3] != 0 && pixel[3] != 65535) {
               float a = pixel[3] / 65535.0f;
               float ra = 1.0f / a;
               float inv_a = 65535.0f * (1 - ra);
               pixel[0] = (stbi__uint16) (pixel[0]*ra + inv_a);
               pixel[1] = (stbi__uint16) (pixel[1]*ra + inv_a);
               pixel[2] = (stbi__uint16) (pixel[2]*ra + inv_a);
            }
         }
      } else {
         for (i=0; i < w*h; ++i) {
            BytePtr pixel = out + 4*i;
            if (pixel[3] != 0 && pixel[3] != 255) {
               float a = pixel[3] / 255.0f;
               float ra = 1.0f / a;
               float inv_a = 255.0f * (1 - ra);
               pixel[0] = (byte) (pixel[0]*ra + inv_a);
               pixel[1] = (byte) (pixel[1]*ra + inv_a);
               pixel[2] = (byte) (pixel[2]*ra + inv_a);
            }
         }
      }
   }

   // convert to desired output format
   if (req_comp && req_comp != 4) {
      if (ri.bits_per_channel == 16)
         out = (BytePtr ) stbi__convert_format16((stbi__uint16 *) out, 4, req_comp, w, h);
      else
         out = stbi__convert_format(out, 4, req_comp, w, h);
      if (out == NULL) return out; // stbi__convert_format frees input on failure
   }

   if (comp) *comp = 4;
   *y = h;
   *x = w;

   return out;
}
#endif

   // *************************************************************************************************
   // Softimage PIC loader
   // by Tom Seddon
   //
   // See http://softimage.wiki.softimage.com/index.php/INFO:_PIC_file_format
   // See http://ozviz.wasp.uwa.edu.au/~pbourke/dataformats/softimagepic/

#if !STBI_NO_PIC
static int stbi__pic_is4(ref stbi__context s,const char *str)
{
   int i;
   for (i=0; i<4; ++i)
      if (stbi__get8(ref s) != (stbi_uc)str[i])
         return 0;

   return 1;
}

static int stbi__pic_test_core(ref stbi__context s)
{
   int i;

   if (!stbi__pic_is4(s,"\x53\x80\xF6\x34"))
      return 0;

   for(i=0;i<84;++i)
      stbi__get8(ref s);

   if (!stbi__pic_is4(s,"PICT"))
      return 0;

   return 1;
}

typedef struct
{
   stbi_uc size,type,channel;
} stbi__pic_packet;

static BytePtr stbi__readval(ref stbi__context s, int channel, BytePtr dest)
{
   int mask=0x80, i;

   for (i=0; i<4; ++i, mask>>=1) {
      if (channel & mask) {
         if (stbi__at_eof(s)) return stbi__errpuc("bad file","PIC file too short");
         dest[i]=stbi__get8(ref s);
      }
   }

   return dest;
}

static void stbi__copyval(int channel,BytePtr dest,BytePtr src)
{
   int mask=0x80,i;

   for (i=0;i<4; ++i, mask>>=1)
      if (channel&mask)
         dest[i]=src[i];
}

static BytePtr stbi__pic_load_core(ref stbi__context s,int width,int height,int *comp, BytePtr result)
{
   int act_comp=0,num_packets=0,y,chained;
   stbi__pic_packet packets[10];

   // this will (should...) cater for even some bizarre stuff like having data
    // for the same channel in multiple packets.
   do {
      stbi__pic_packet *packet;

      if (num_packets==sizeof(packets)/sizeof(packets[0]))
         return stbi__errpuc("bad format","too many packets");

      packet = &packets[num_packets++];

      chained = stbi__get8(ref s);
      packet.size    = stbi__get8(ref s);
      packet.type    = stbi__get8(ref s);
      packet.channel = stbi__get8(ref s);

      act_comp |= packet.channel;

      if (stbi__at_eof(s))          return stbi__errpuc("bad file","file too short (reading packets)");
      if (packet.size != 8)  return stbi__errpuc("bad format","packet isn't 8bpp");
   } while (chained);

   *comp = (act_comp & 0x10 ? 4 : 3); // has alpha channel?

   for(y=0; y<height; ++y) {
      int packet_idx;

      for(packet_idx=0; packet_idx < num_packets; ++packet_idx) {
         stbi__pic_packet *packet = &packets[packet_idx];
         BytePtr dest = result+y*width*4;

         switch (packet.type) {
            default:
               return stbi__errpuc("bad format","packet has bad compression type");

            case 0: {//uncompressed
               int x;

               for(x=0;x<width;++x, dest+=4)
                  if (!stbi__readval(s,packet.channel,dest))
                     return 0;
               break;
            }

            case 1://Pure RLE
               {
                  int left=width, i;

                  while (left>0) {
                     stbi_uc count,value[4];

                     count=stbi__get8(ref s);
                     if (stbi__at_eof(s))   return stbi__errpuc("bad file","file too short (pure read count)");

                     if (count > left)
                        count = (stbi_uc) left;

                     if (!stbi__readval(s,packet.channel,value))  return 0;

                     for(i=0; i<count; ++i,dest+=4)
                        stbi__copyval(packet.channel,dest,value);
                     left -= count;
                  }
               }
               break;

            case 2: {//Mixed RLE
               int left=width;
               while (left>0) {
                  int count = stbi__get8(ref s), i;
                  if (stbi__at_eof(s))  return stbi__errpuc("bad file","file too short (mixed read count)");

                  if (count >= 128) { // Repeated
                     stbi_uc value[4];

                     if (count==128)
                        count = stbi__get16be(s);
                     else
                        count -= 127;
                     if (count > left)
                        return stbi__errpuc("bad file","scanline overrun");

                     if (!stbi__readval(s,packet.channel,value))
                        return 0;

                     for(i=0;i<count;++i, dest += 4)
                        stbi__copyval(packet.channel,dest,value);
                  } else { // Raw
                     ++count;
                     if (count>left) return stbi__errpuc("bad file","scanline overrun");

                     for(i=0;i<count;++i, dest+=4)
                        if (!stbi__readval(s,packet.channel,dest))
                           return 0;
                  }
                  left-=count;
               }
               break;
            }
         }
      }
   }

   return result;
}

static void *stbi__pic_load(ref stbi__context s,int *px,int *py,int *comp,int req_comp, stbi__result_info *ri)
{
   BytePtr result;
   int i, x,y, internal_comp;
   STBI_NOTUSED(ri);

   if (!comp) comp = &internal_comp;

   for (i=0; i<92; ++i)
      stbi__get8(ref s);

   x = stbi__get16be(s);
   y = stbi__get16be(s);

   if (y > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large","Very large image (corrupt?)");
   if (x > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large","Very large image (corrupt?)");

   if (stbi__at_eof(s))  return stbi__errpuc("bad file","file too short (pic header)");
   if (!stbi__mad3sizes_valid(x, y, 4, 0)) return stbi__errpuc("too large", "PIC image too large to decode");

   stbi__get32be(s); //skip `ratio'
   stbi__get16be(s); //skip `fields'
   stbi__get16be(s); //skip `pad'

   // intermediate buffer is RGBA
   result = (BytePtr ) stbi__malloc_mad3(x, y, 4, 0);
   if (!result) return stbi__errpuc("outofmem", "Out of memory");
   memset(result, 0xff, x*y*4);

   if (!stbi__pic_load_core(s,x,y,comp, result)) {
      STBI_FREE(result);
      result=0;
   }
   *px = x;
   *py = y;
   if (req_comp == 0) req_comp = *comp;
   result=stbi__convert_format(result,4,req_comp,x,y);

   return result;
}

static int stbi__pic_test(ref stbi__context s)
{
   int r = stbi__pic_test_core(s);
   stbi__rewind(ref s);
   return r;
}
#endif

   // *************************************************************************************************
   // GIF loader -- public domain by Jean-Marc Lienher -- simplified/shrunk by stb

#if !STBI_NO_GIF
typedef struct
{
   stbi__int16 prefix;
   stbi_uc first;
   stbi_uc suffix;
} stbi__gif_lzw;

typedef struct
{
   int w,h;
   BytePtr out;                 // output buffer (always 4 components)
   BytePtr background;          // The current "background" as far as a gif is concerned
   BytePtr history;
   int flags, bgindex, ratio, transparent, eflags;
   stbi_uc  pal[256][4];
   stbi_uc lpal[256][4];
   stbi__gif_lzw codes[8192];
   BytePtr color_table;
   int parse, step;
   int lflags;
   int start_x, start_y;
   int max_x, max_y;
   int cur_x, cur_y;
   int line_size;
   int delay;
} stbi__gif;

static int stbi__gif_test_raw(ref stbi__context s)
{
   int sz;
   if (stbi__get8(ref s) != 'G' || stbi__get8(ref s) != 'I' || stbi__get8(ref s) != 'F' || stbi__get8(ref s) != '8') return 0;
   sz = stbi__get8(ref s);
   if (sz != '9' && sz != '7') return 0;
   if (stbi__get8(ref s) != 'a') return 0;
   return 1;
}

static int stbi__gif_test(ref stbi__context s)
{
   int r = stbi__gif_test_raw(s);
   stbi__rewind(ref s);
   return r;
}

static void stbi__gif_parse_colortable(ref stbi__context s, stbi_uc pal[256][4], int num_entries, int transp)
{
   int i;
   for (i=0; i < num_entries; ++i) {
      pal[i][2] = stbi__get8(ref s);
      pal[i][1] = stbi__get8(ref s);
      pal[i][0] = stbi__get8(ref s);
      pal[i][3] = transp == i ? 0 : 255;
   }
}

static int stbi__gif_header(ref stbi__context s, stbi__gif *g, int *comp, int is_info)
{
   stbi_uc version;
   if (stbi__get8(ref s) != 'G' || stbi__get8(ref s) != 'I' || stbi__get8(ref s) != 'F' || stbi__get8(ref s) != '8')
      return stbi__err("not GIF", "Corrupt GIF");

   version = stbi__get8(ref s);
   if (version != '7' && version != '9')    return stbi__err("not GIF", "Corrupt GIF");
   if (stbi__get8(ref s) != 'a')                return stbi__err("not GIF", "Corrupt GIF");

   stbi__g_failure_reason = "";
   g.w = stbi__get16le(ref s);
   g.h = stbi__get16le(ref s);
   g.flags = stbi__get8(ref s);
   g.bgindex = stbi__get8(ref s);
   g.ratio = stbi__get8(ref s);
   g.transparent = -1;

   if (g.w > STBI_MAX_DIMENSIONS) return stbi__err("too large","Very large image (corrupt?)");
   if (g.h > STBI_MAX_DIMENSIONS) return stbi__err("too large","Very large image (corrupt?)");

   if (comp != 0) *comp = 4;  // can't actually tell whether it's 3 or 4 until we parse the comments

   if (is_info) return 1;

   if (g.flags & 0x80)
      stbi__gif_parse_colortable(s,g.pal, 2 << (g.flags & 7), -1);

   return 1;
}

static int stbi__gif_info_raw(ref stbi__context s, int *x, int *y, int *comp)
{
   stbi__gif* g = (stbi__gif*) stbi__malloc(sizeof(stbi__gif));
   if (!g) return stbi__err("outofmem", "Out of memory");
   if (!stbi__gif_header(s, g, comp, 1)) {
      STBI_FREE(g);
      stbi__rewind( s );
      return 0;
   }
   if (x) *x = g.w;
   if (y) *y = g.h;
   STBI_FREE(g);
   return 1;
}

static void stbi__out_gif_code(stbi__gif *g, stbi__uint16 code)
{
   BytePtr p, *c;
   int idx;

   // recurse to decode the prefixes, since the linked-list is backwards,
   // and working backwards through an interleaved image would be nasty
   if (g.codes[code].prefix >= 0)
      stbi__out_gif_code(g, g.codes[code].prefix);

   if (g.cur_y >= g.max_y) return;

   idx = g.cur_x + g.cur_y;
   p = &g.out[idx];
   g.history[idx / 4] = 1;

   c = &g.color_table[g.codes[code].suffix * 4];
   if (c[3] > 128) { // don't render transparent pixels;
      p[0] = c[2];
      p[1] = c[1];
      p[2] = c[0];
      p[3] = c[3];
   }
   g.cur_x += 4;

   if (g.cur_x >= g.max_x) {
      g.cur_x = g.start_x;
      g.cur_y += g.step;

      while (g.cur_y >= g.max_y && g.parse > 0) {
         g.step = (1 << g.parse) * g.line_size;
         g.cur_y = g.start_y + (g.step >> 1);
         --g.parse;
      }
   }
}

static BytePtr stbi__process_gif_raster(ref stbi__context s, stbi__gif *g)
{
   stbi_uc lzw_cs;
   stbi__int32 len, init_code;
   stbi__uint32 first;
   stbi__int32 codesize, codemask, avail, oldcode, bits, valid_bits, clear;
   stbi__gif_lzw *p;

   lzw_cs = stbi__get8(ref s);
   if (lzw_cs > 12) return NULL;
   clear = 1 << lzw_cs;
   first = 1;
   codesize = lzw_cs + 1;
   codemask = (1 << codesize) - 1;
   bits = 0;
   valid_bits = 0;
   for (init_code = 0; init_code < clear; init_code++) {
      g.codes[init_code].prefix = -1;
      g.codes[init_code].first = (stbi_uc) init_code;
      g.codes[init_code].suffix = (stbi_uc) init_code;
   }

   // support no starting clear code
   avail = clear+2;
   oldcode = -1;

   len = 0;
   for(;;) {
      if (valid_bits < codesize) {
         if (len == 0) {
            len = stbi__get8(ref s); // start new block
            if (len == 0)
               return g.out;
         }
         --len;
         bits |= (stbi__int32) stbi__get8(ref s) << valid_bits;
         valid_bits += 8;
      } else {
         stbi__int32 code = bits & codemask;
         bits >>= codesize;
         valid_bits -= codesize;
         // @OPTIMIZE: is there some way we can accelerate the non-clear path?
         if (code == clear) {  // clear code
            codesize = lzw_cs + 1;
            codemask = (1 << codesize) - 1;
            avail = clear + 2;
            oldcode = -1;
            first = 0;
         } else if (code == clear + 1) { // end of stream code
            stbi__skip(ref s, len);
            while ((len = stbi__get8(ref s)) > 0)
               stbi__skip(ref s,len);
            return g.out;
         } else if (code <= avail) {
            if (first) {
               return stbi__errpuc("no clear code", "Corrupt GIF");
            }

            if (oldcode >= 0) {
               p = &g.codes[avail++];
               if (avail > 8192) {
                  return stbi__errpuc("too many codes", "Corrupt GIF");
               }

               p.prefix = (stbi__int16) oldcode;
               p.first = g.codes[oldcode].first;
               p.suffix = (code == avail) ? p.first : g.codes[code].first;
            } else if (code == avail)
               return stbi__errpuc("illegal code in raster", "Corrupt GIF");

            stbi__out_gif_code(g, (stbi__uint16) code);

            if ((avail & codemask) == 0 && avail <= 0x0FFF) {
               codesize++;
               codemask = (1 << codesize) - 1;
            }

            oldcode = code;
         } else {
            return stbi__errpuc("illegal code in raster", "Corrupt GIF");
         }
      }
   }
}

// this function is designed to support animated gifs, although stb_image doesn't support it
// two back is the image from two frames ago, used for a very specific disposal format
static BytePtr stbi__gif_load_next(ref stbi__context s, stbi__gif *g, int *comp, int req_comp, BytePtr two_back)
{
   int dispose;
   int first_frame;
   int pi;
   int pcount;
   STBI_NOTUSED(req_comp);

   // on first frame, any non-written pixels get the background colour (non-transparent)
   first_frame = 0;
   if (g.out == 0) {
      if (!stbi__gif_header(s, g, comp,0)) return 0; // stbi__g_failure_reason set by stbi__gif_header
      if (!stbi__mad3sizes_valid(4, g.w, g.h, 0))
         return stbi__errpuc("too large", "GIF image is too large");
      pcount = g.w * g.h;
      g.out = (BytePtr ) stbi__malloc(4 * pcount);
      g.background = (BytePtr ) stbi__malloc(4 * pcount);
      g.history = (BytePtr ) stbi__malloc(pcount);
      if (!g.out || !g.background || !g.history)
         return stbi__errpuc("outofmem", "Out of memory");

      // image is treated as "transparent" at the start - ie, nothing overwrites the current background;
      // background colour is only used for pixels that are not rendered first frame, after that "background"
      // color refers to the color that was there the previous frame.
      memset(g.out, 0x00, 4 * pcount);
      memset(g.background, 0x00, 4 * pcount); // state of the background (starts transparent)
      memset(g.history, 0x00, pcount);        // pixels that were affected previous frame
      first_frame = 1;
   } else {
      // second frame - how do we dispose of the previous one?
      dispose = (g.eflags & 0x1C) >> 2;
      pcount = g.w * g.h;

      if ((dispose == 3) && (two_back == 0)) {
         dispose = 2; // if I don't have an image to revert back to, default to the old background
      }

      if (dispose == 3) { // use previous graphic
         for (pi = 0; pi < pcount; ++pi) {
            if (g.history[pi]) {
               memcpy( &g.out[pi * 4], &two_back[pi * 4], 4 );
            }
         }
      } else if (dispose == 2) {
         // restore what was changed last frame to background before that frame;
         for (pi = 0; pi < pcount; ++pi) {
            if (g.history[pi]) {
               memcpy( &g.out[pi * 4], &g.background[pi * 4], 4 );
            }
         }
      } else {
         // This is a non-disposal case eithe way, so just
         // leave the pixels as is, and they will become the new background
         // 1: do not dispose
         // 0:  not specified.
      }

      // background is what out is after the undoing of the previou frame;
      memcpy( g.background, g.out, 4 * g.w * g.h );
   }

   // clear my history;
   memset( g.history, 0x00, g.w * g.h );        // pixels that were affected previous frame

   for (;;) {
      int tag = stbi__get8(ref s);
      switch (tag) {
         case 0x2C: /* Image Descriptor */
         {
            stbi__int32 x, y, w, h;
            BytePtr o;

            x = stbi__get16le(ref s);
            y = stbi__get16le(ref s);
            w = stbi__get16le(ref s);
            h = stbi__get16le(ref s);
            if (((x + w) > (g.w)) || ((y + h) > (g.h)))
               return stbi__errpuc("bad Image Descriptor", "Corrupt GIF");

            g.line_size = g.w * 4;
            g.start_x = x * 4;
            g.start_y = y * g.line_size;
            g.max_x   = g.start_x + w * 4;
            g.max_y   = g.start_y + h * g.line_size;
            g.cur_x   = g.start_x;
            g.cur_y   = g.start_y;

            // if the width of the specified rectangle is 0, that means
            // we may not see *any* pixels or the image is malformed;
            // to make sure this is caught, move the current y down to
            // max_y (which is what out_gif_code checks).
            if (w == 0)
               g.cur_y = g.max_y;

            g.lflags = stbi__get8(ref s);

            if (g.lflags & 0x40) {
               g.step = 8 * g.line_size; // first interlaced spacing
               g.parse = 3;
            } else {
               g.step = g.line_size;
               g.parse = 0;
            }

            if (g.lflags & 0x80) {
               stbi__gif_parse_colortable(s,g.lpal, 2 << (g.lflags & 7), g.eflags & 0x01 ? g.transparent : -1);
               g.color_table = (BytePtr ) g.lpal;
            } else if (g.flags & 0x80) {
               g.color_table = (BytePtr ) g.pal;
            } else
               return stbi__errpuc("missing color table", "Corrupt GIF");

            o = stbi__process_gif_raster(s, g);
            if (!o) return NULL;

            // if this was the first frame,
            pcount = g.w * g.h;
            if (first_frame && (g.bgindex > 0)) {
               // if first frame, any pixel not drawn to gets the background color
               for (pi = 0; pi < pcount; ++pi) {
                  if (g.history[pi] == 0) {
                     g.pal[g.bgindex][3] = 255; // just in case it was made transparent, undo that; It will be reset next frame if need be;
                     memcpy( &g.out[pi * 4], &g.pal[g.bgindex], 4 );
                  }
               }
            }

            return o;
         }

         case 0x21: // Comment Extension.
         {
            int len;
            int ext = stbi__get8(ref s);
            if (ext == 0xF9) { // Graphic Control Extension.
               len = stbi__get8(ref s);
               if (len == 4) {
                  g.eflags = stbi__get8(ref s);
                  g.delay = 10 * stbi__get16le(ref s); // delay - 1/100th of a second, saving as 1/1000ths.

                  // unset old transparent
                  if (g.transparent >= 0) {
                     g.pal[g.transparent][3] = 255;
                  }
                  if (g.eflags & 0x01) {
                     g.transparent = stbi__get8(ref s);
                     if (g.transparent >= 0) {
                        g.pal[g.transparent][3] = 0;
                     }
                  } else {
                     // don't need transparent
                     stbi__skip(ref s, 1);
                     g.transparent = -1;
                  }
               } else {
                  stbi__skip(ref s, len);
                  break;
               }
            }
            while ((len = stbi__get8(ref s)) != 0) {
               stbi__skip(ref s, len);
            }
            break;
         }

         case 0x3B: // gif stream termination code
            return (BytePtr ) s; // using '1' causes warning on some compilers

         default:
            return stbi__errpuc("unknown code", "Corrupt GIF");
      }
   }
}

static void *stbi__load_gif_main_outofmem(stbi__gif *g, BytePtr out, int **delays)
{
   STBI_FREE(g.out);
   STBI_FREE(g.history);
   STBI_FREE(g.background);

   if (out) STBI_FREE(out);
   if (delays && *delays) STBI_FREE(*delays);
   return stbi__errpuc("outofmem", "Out of memory");
}

static void *stbi__load_gif_main(ref stbi__context s, int **delays, int *x, int *y, int *z, int *comp, int req_comp)
{
   if (stbi__gif_test(s)) {
      int layers = 0;
      BytePtr u = 0;
      BytePtr out = 0;
      BytePtr two_back = 0;
      stbi__gif g;
      int stride;
      int out_size = 0;
      int delays_size = 0;

      STBI_NOTUSED(out_size);
      STBI_NOTUSED(delays_size);

      memset(&g, 0, sizeof(g));
      if (delays) {
         *delays = 0;
      }

      do {
         u = stbi__gif_load_next(s, &g, comp, req_comp, two_back);
         if (u == (BytePtr ) s) u = 0;  // end of animated gif marker

         if (u) {
            *x = g.w;
            *y = g.h;
            ++layers;
            stride = g.w * g.h * 4;

            if (out) {
               void *tmp = (BytePtr) STBI_REALLOC_SIZED( out, out_size, layers * stride );
               if (!tmp)
                  return stbi__load_gif_main_outofmem(&g, out, delays);
               else {
                   out = (BytePtr) tmp;
                   out_size = layers * stride;
               }

               if (delays) {
                  int *new_delays = (int*) STBI_REALLOC_SIZED( *delays, delays_size, sizeof(int) * layers );
                  if (!new_delays)
                     return stbi__load_gif_main_outofmem(&g, out, delays);
                  *delays = new_delays;
                  delays_size = layers * sizeof(int);
               }
            } else {
               out = (BytePtr)stbi__malloc( layers * stride );
               if (!out)
                  return stbi__load_gif_main_outofmem(&g, out, delays);
               out_size = layers * stride;
               if (delays) {
                  *delays = (int*) stbi__malloc( layers * sizeof(int) );
                  if (!*delays)
                     return stbi__load_gif_main_outofmem(&g, out, delays);
                  delays_size = layers * sizeof(int);
               }
            }
            memcpy( out + ((layers - 1) * stride), u, stride );
            if (layers >= 2) {
               two_back = out - 2 * stride;
            }

            if (delays) {
               (*delays)[layers - 1U] = g.delay;
            }
         }
      } while (u != 0);

      // free temp buffer;
      STBI_FREE(g.out);
      STBI_FREE(g.history);
      STBI_FREE(g.background);

      // do the final conversion after loading everything;
      if (req_comp && req_comp != 4)
         out = stbi__convert_format(out, 4, req_comp, layers * g.w, g.h);

      *z = layers;
      return out;
   } else {
      return stbi__errpuc("not GIF", "Image was not as a gif type.");
   }
}

static void *stbi__gif_load(ref stbi__context s, int *x, int *y, int *comp, int req_comp, stbi__result_info *ri)
{
   BytePtr u = 0;
   stbi__gif g;
   memset(&g, 0, sizeof(g));
   STBI_NOTUSED(ri);

   u = stbi__gif_load_next(s, &g, comp, req_comp, 0);
   if (u == (BytePtr ) s) u = 0;  // end of animated gif marker
   if (u) {
      *x = g.w;
      *y = g.h;

      // moved conversion to after successful load so that the same
      // can be done for multiple frames.
      if (req_comp && req_comp != 4)
         u = stbi__convert_format(u, 4, req_comp, g.w, g.h);
   } else if (g.out) {
      // if there was an error and we allocated an image buffer, free it!
      STBI_FREE(g.out);
   }

   // free buffers needed for multiple frame loading;
   STBI_FREE(g.history);
   STBI_FREE(g.background);

   return u;
}

static int stbi__gif_info(ref stbi__context s, int *x, int *y, int *comp)
{
   return stbi__gif_info_raw(s,x,y,comp);
}
#endif

   // *************************************************************************************************
   // Radiance RGBE HDR loader
   // originally by Nicolas Schulz
#if !STBI_NO_HDR
static int stbi__hdr_test_core(ref stbi__context s, const char *signature)
{
   int i;
   for (i=0; signature[i]; ++i)
      if (stbi__get8(ref s) != signature[i])
          return 0;
   stbi__rewind(ref s);
   return 1;
}

static int stbi__hdr_test(ref stbi__context s)
{
   int r = stbi__hdr_test_core(s, "#?RADIANCE\n");
   stbi__rewind(ref s);
   if(!r) {
       r = stbi__hdr_test_core(s, "#?RGBE\n");
       stbi__rewind(ref s);
   }
   return r;
}

const STBI__HDR_BUFLEN  = 1024;
static char *stbi__hdr_gettoken(ref stbi__context z, char *buffer)
{
   int len=0;
   char c = '\0';

   c = (char) stbi__get8(z);

   while (!stbi__at_eof(z) && c != '\n') {
      buffer[len++] = c;
      if (len == STBI__HDR_BUFLEN-1) {
         // flush to end of line
         while (!stbi__at_eof(z) && stbi__get8(z) != '\n')
            ;
         break;
      }
      c = (char) stbi__get8(z);
   }

   buffer[len] = 0;
   return buffer;
}

static void stbi__hdr_convert(float *output, BytePtr input, int req_comp)
{
   if ( input[3] != 0 ) {
      float f1;
      // Exponent
      f1 = (float) ldexp(1.0f, input[3] - (int)(128 + 8));
      if (req_comp <= 2)
         output[0] = (input[0] + input[1] + input[2]) * f1 / 3;
      else {
         output[0] = input[0] * f1;
         output[1] = input[1] * f1;
         output[2] = input[2] * f1;
      }
      if (req_comp == 2) output[1] = 1;
      if (req_comp == 4) output[3] = 1;
   } else {
      switch (req_comp) {
         case 4: output[3] = 1; /* fallthrough */
         case 3: output[0] = output[1] = output[2] = 0;
                 break;
         case 2: output[1] = 1; /* fallthrough */
         case 1: output[0] = 0;
                 break;
      }
   }
}

static float *stbi__hdr_load(ref stbi__context s, int *x, int *y, int *comp, int req_comp, stbi__result_info *ri)
{
   char buffer[STBI__HDR_BUFLEN];
   char *token;
   int valid = 0;
   int width, height;
   BytePtr scanline;
   float *hdr_data;
   int len;
   byte count, value;
   int i, j, k, c1,c2, z;
   const char *headerToken;
   STBI_NOTUSED(ri);

   // Check identifier
   headerToken = stbi__hdr_gettoken(s,buffer);
   if (strcmp(headerToken, "#?RADIANCE") != 0 && strcmp(headerToken, "#?RGBE") != 0)
      return stbi__errpf("not HDR", "Corrupt HDR image");

   // Parse header
   for(;;) {
      token = stbi__hdr_gettoken(s,buffer);
      if (token[0] == 0) break;
      if (strcmp(token, "FORMAT=32-bit_rle_rgbe") == 0) valid = 1;
   }

   if (!valid)    return stbi__errpf("unsupported format", "Unsupported HDR format");

   // Parse width and height
   // can't use sscanf() if we're not using stdio!
   token = stbi__hdr_gettoken(s,buffer);
   if (strncmp(token, "-Y ", 3))  return stbi__errpf("unsupported data layout", "Unsupported HDR format");
   token += 3;
   height = (int) strtol(token, &token, 10);
   while (*token == ' ') ++token;
   if (strncmp(token, "+X ", 3))  return stbi__errpf("unsupported data layout", "Unsupported HDR format");
   token += 3;
   width = (int) strtol(token, NULL, 10);

   if (height > STBI_MAX_DIMENSIONS) return stbi__errpf("too large","Very large image (corrupt?)");
   if (width > STBI_MAX_DIMENSIONS) return stbi__errpf("too large","Very large image (corrupt?)");

   *x = width;
   *y = height;

   if (comp) *comp = 3;
   if (req_comp == 0) req_comp = 3;

   if (!stbi__mad4sizes_valid(width, height, req_comp, sizeof(float), 0))
      return stbi__errpf("too large", "HDR image is too large");

   // Read data
   hdr_data = (float *) stbi__malloc_mad4(width, height, req_comp, sizeof(float), 0);
   if (!hdr_data)
      return stbi__errpf("outofmem", "Out of memory");

   // Load image data
   // image data is stored as some number of sca
   if ( width < 8 || width >= 32768) {
      // Read flat data
      for (j=0; j < height; ++j) {
         for (i=0; i < width; ++i) {
            stbi_uc rgbe[4];
           main_decode_loop:
            stbi__getn(s, rgbe, 4);
            stbi__hdr_convert(hdr_data + j * width * req_comp + i * req_comp, rgbe, req_comp);
         }
      }
   } else {
      // Read RLE-encoded data
      scanline = NULL;

      for (j = 0; j < height; ++j) {
         c1 = stbi__get8(ref s);
         c2 = stbi__get8(ref s);
         len = stbi__get8(ref s);
         if (c1 != 2 || c2 != 2 || (len & 0x80)) {
            // not run-length encoded, so we have to actually use THIS data as a decoded
            // pixel (note this can't be a valid pixel--one of RGB must be >= 128)
            stbi_uc rgbe[4];
            rgbe[0] = (stbi_uc) c1;
            rgbe[1] = (stbi_uc) c2;
            rgbe[2] = (stbi_uc) len;
            rgbe[3] = (stbi_uc) stbi__get8(ref s);
            stbi__hdr_convert(hdr_data, rgbe, req_comp);
            i = 1;
            j = 0;
            STBI_FREE(scanline);
            goto main_decode_loop; // yes, this makes no sense
         }
         len <<= 8;
         len |= stbi__get8(ref s);
         if (len != width) { STBI_FREE(hdr_data); STBI_FREE(scanline); return stbi__errpf("invalid decoded scanline length", "corrupt HDR"); }
         if (scanline == NULL) {
            scanline = (BytePtr ) stbi__malloc_mad2(width, 4, 0);
            if (!scanline) {
               STBI_FREE(hdr_data);
               return stbi__errpf("outofmem", "Out of memory");
            }
         }

         for (k = 0; k < 4; ++k) {
            int nleft;
            i = 0;
            while ((nleft = width - i) > 0) {
               count = stbi__get8(ref s);
               if (count > 128) {
                  // Run
                  value = stbi__get8(ref s);
                  count -= 128;
                  if ((count == 0) || (count > nleft)) { STBI_FREE(hdr_data); STBI_FREE(scanline); return stbi__errpf("corrupt", "bad RLE data in HDR"); }
                  for (z = 0; z < count; ++z)
                     scanline[i++ * 4 + k] = value;
               } else {
                  // Dump
                  if ((count == 0) || (count > nleft)) { STBI_FREE(hdr_data); STBI_FREE(scanline); return stbi__errpf("corrupt", "bad RLE data in HDR"); }
                  for (z = 0; z < count; ++z)
                     scanline[i++ * 4 + k] = stbi__get8(ref s);
               }
            }
         }
         for (i=0; i < width; ++i)
            stbi__hdr_convert(hdr_data+(j*width + i)*req_comp, scanline + i*4, req_comp);
      }
      if (scanline)
         STBI_FREE(scanline);
   }

   return hdr_data;
}

static int stbi__hdr_info(ref stbi__context s, int *x, int *y, int *comp)
{
   char buffer[STBI__HDR_BUFLEN];
   char *token;
   int valid = 0;
   int dummy;

   if (!x) x = &dummy;
   if (!y) y = &dummy;
   if (!comp) comp = &dummy;

   if (stbi__hdr_test(s) == 0) {
       stbi__rewind( s );
       return 0;
   }

   for(;;) {
      token = stbi__hdr_gettoken(s,buffer);
      if (token[0] == 0) break;
      if (strcmp(token, "FORMAT=32-bit_rle_rgbe") == 0) valid = 1;
   }

   if (!valid) {
       stbi__rewind( s );
       return 0;
   }
   token = stbi__hdr_gettoken(s,buffer);
   if (strncmp(token, "-Y ", 3)) {
       stbi__rewind( s );
       return 0;
   }
   token += 3;
   *y = (int) strtol(token, &token, 10);
   while (*token == ' ') ++token;
   if (strncmp(token, "+X ", 3)) {
       stbi__rewind( s );
       return 0;
   }
   token += 3;
   *x = (int) strtol(token, NULL, 10);
   *comp = 3;
   return 1;
}
#endif // STBI_NO_HDR

#if !STBI_NO_PSD
static int stbi__psd_info(ref stbi__context s, int *x, int *y, int *comp)
{
   int channelCount, dummy, depth;
   if (!x) x = &dummy;
   if (!y) y = &dummy;
   if (!comp) comp = &dummy;
   if (stbi__get32be(s) != 0x38425053) {
       stbi__rewind( s );
       return 0;
   }
   if (stbi__get16be(s) != 1) {
       stbi__rewind( s );
       return 0;
   }
   stbi__skip(ref s, 6);
   channelCount = stbi__get16be(s);
   if (channelCount < 0 || channelCount > 16) {
       stbi__rewind( s );
       return 0;
   }
   *y = stbi__get32be(s);
   *x = stbi__get32be(s);
   depth = stbi__get16be(s);
   if (depth != 8 && depth != 16) {
       stbi__rewind( s );
       return 0;
   }
   if (stbi__get16be(s) != 3) {
       stbi__rewind( s );
       return 0;
   }
   *comp = 4;
   return 1;
}

static int stbi__psd_is16(ref stbi__context s)
{
   int channelCount, depth;
   if (stbi__get32be(s) != 0x38425053) {
       stbi__rewind( s );
       return 0;
   }
   if (stbi__get16be(s) != 1) {
       stbi__rewind( s );
       return 0;
   }
   stbi__skip(ref s, 6);
   channelCount = stbi__get16be(s);
   if (channelCount < 0 || channelCount > 16) {
       stbi__rewind( s );
       return 0;
   }
   STBI_NOTUSED(stbi__get32be(s));
   STBI_NOTUSED(stbi__get32be(s));
   depth = stbi__get16be(s);
   if (depth != 16) {
       stbi__rewind( s );
       return 0;
   }
   return 1;
}
#endif

#if !STBI_NO_PIC
static int stbi__pic_info(ref stbi__context s, int *x, int *y, int *comp)
{
   int act_comp=0,num_packets=0,chained,dummy;
   stbi__pic_packet packets[10];

   if (!x) x = &dummy;
   if (!y) y = &dummy;
   if (!comp) comp = &dummy;

   if (!stbi__pic_is4(s,"\x53\x80\xF6\x34")) {
      stbi__rewind(ref s);
      return 0;
   }

   stbi__skip(ref s, 88);

   *x = stbi__get16be(s);
   *y = stbi__get16be(s);
   if (stbi__at_eof(s)) {
      stbi__rewind( s);
      return 0;
   }
   if ( (*x) != 0 && (1 << 28) / (*x) < (*y)) {
      stbi__rewind( s );
      return 0;
   }

   stbi__skip(ref s, 8);

   do {
      stbi__pic_packet *packet;

      if (num_packets==sizeof(packets)/sizeof(packets[0]))
         return 0;

      packet = &packets[num_packets++];
      chained = stbi__get8(ref s);
      packet.size    = stbi__get8(ref s);
      packet.type    = stbi__get8(ref s);
      packet.channel = stbi__get8(ref s);
      act_comp |= packet.channel;

      if (stbi__at_eof(s)) {
          stbi__rewind( s );
          return 0;
      }
      if (packet.size != 8) {
          stbi__rewind( s );
          return 0;
      }
   } while (chained);

   *comp = (act_comp & 0x10 ? 4 : 3);

   return 1;
}
#endif

   // *************************************************************************************************
   // Portable Gray Map and Portable Pixel Map loader
   // by Ken Miller
   //
   // PGM: http://netpbm.sourceforge.net/doc/pgm.html
   // PPM: http://netpbm.sourceforge.net/doc/ppm.html
   //
   // Known limitations:
   //    Does not support comments in the header section
   //    Does not support ASCII image data (formats P2 and P3)

#if !STBI_NO_PNM

static int      stbi__pnm_test(ref stbi__context s)
{
   char p, t;
   p = (char) stbi__get8(ref s);
   t = (char) stbi__get8(ref s);
   if (p != 'P' || (t != '5' && t != '6')) {
       stbi__rewind( s );
       return 0;
   }
   return 1;
}

static void *stbi__pnm_load(ref stbi__context s, int *x, int *y, int *comp, int req_comp, stbi__result_info *ri)
{
   BytePtr out;
   STBI_NOTUSED(ri);

   ri.bits_per_channel = stbi__pnm_info(s, (int *)&s.img_x, (int *)&s.img_y, (int *)&s.img_n);
   if (ri.bits_per_channel == 0)
      return 0;

   if (s.img_y > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large","Very large image (corrupt?)");
   if (s.img_x > STBI_MAX_DIMENSIONS) return stbi__errpuc("too large","Very large image (corrupt?)");

   *x = s.img_x;
   *y = s.img_y;
   if (comp) *comp = s.img_n;

   if (!stbi__mad4sizes_valid(s.img_n, s.img_x, s.img_y, ri.bits_per_channel / 8, 0))
      return stbi__errpuc("too large", "PNM too large");

   out = (BytePtr ) stbi__malloc_mad4(s.img_n, s.img_x, s.img_y, ri.bits_per_channel / 8, 0);
   if (!out) return stbi__errpuc("outofmem", "Out of memory");
   if (!stbi__getn(s, out, s.img_n * s.img_x * s.img_y * (ri.bits_per_channel / 8))) {
      STBI_FREE(out);
      return stbi__errpuc("bad PNM", "PNM file truncated");
   }

   if (req_comp && req_comp != s.img_n) {
      if (ri.bits_per_channel == 16) {
         out = (BytePtr ) stbi__convert_format16((stbi__uint16 *) out, s.img_n, req_comp, s.img_x, s.img_y);
      } else {
         out = stbi__convert_format(out, s.img_n, req_comp, s.img_x, s.img_y);
      }
      if (out == NULL) return out; // stbi__convert_format frees input on failure
   }
   return out;
}

static int      stbi__pnm_isspace(char c)
{
   return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
}

static void     stbi__pnm_skip_whitespace(ref stbi__context s, char *c)
{
   for (;;) {
      while (!stbi__at_eof(s) && stbi__pnm_isspace(*c))
         *c = (char) stbi__get8(ref s);

      if (stbi__at_eof(s) || *c != '#')
         break;

      while (!stbi__at_eof(s) && *c != '\n' && *c != '\r' )
         *c = (char) stbi__get8(ref s);
   }
}

static int      stbi__pnm_isdigit(char c)
{
   return c >= '0' && c <= '9';
}

static int      stbi__pnm_getinteger(ref stbi__context s, char *c)
{
   int value = 0;

   while (!stbi__at_eof(s) && stbi__pnm_isdigit(*c)) {
      value = value*10 + (*c - '0');
      *c = (char) stbi__get8(ref s);
      if((value > 214748364) || (value == 214748364 && *c > '7'))
          return stbi__err("integer parse overflow", "Parsing an integer in the PPM header overflowed a 32-bit int");
   }

   return value;
}

static int      stbi__pnm_info(ref stbi__context s, int *x, int *y, int *comp)
{
   int maxv, dummy;
   char c, p, t;

   if (!x) x = &dummy;
   if (!y) y = &dummy;
   if (!comp) comp = &dummy;

   stbi__rewind(ref s);

   // Get identifier
   p = (char) stbi__get8(ref s);
   t = (char) stbi__get8(ref s);
   if (p != 'P' || (t != '5' && t != '6')) {
       stbi__rewind(ref s);
       return 0;
   }

   *comp = (t == '6') ? 3 : 1;  // '5' is 1-component .pgm; '6' is 3-component .ppm

   c = (char) stbi__get8(ref s);
   stbi__pnm_skip_whitespace(s, &c);

   *x = stbi__pnm_getinteger(s, &c); // read width
   if(*x == 0)
       return stbi__err("invalid width", "PPM image header had zero or overflowing width");
   stbi__pnm_skip_whitespace(s, &c);

   *y = stbi__pnm_getinteger(s, &c); // read height
   if (*y == 0)
       return stbi__err("invalid width", "PPM image header had zero or overflowing width");
   stbi__pnm_skip_whitespace(s, &c);

   maxv = stbi__pnm_getinteger(s, &c);  // read max value
   if (maxv > 65535)
      return stbi__err("max value > 65535", "PPM image supports only 8-bit and 16-bit images");
   else if (maxv > 255)
      return 16;
   else
      return 8;
}

static int stbi__pnm_is16(ref stbi__context s)
{
   if (stbi__pnm_info(s, NULL, NULL, NULL) == 16)
	   return 1;
   return 0;
}
#endif

   static bool stbi__info_main(ref stbi__context s, out int x, out int y, out STBI_CHANNELS comp)
   {
#if !STBI_NO_JPEG
      if (stbi__jpeg_info(ref s, out x, out y, out comp)) return true;
#endif

#if !STBI_NO_PNG
      if (stbi__png_info(ref s, out x, out y, out comp)) return true;
#endif

#if !STBI_NO_GIF
   if (stbi__gif_info(ref s, out x, out y, out comp))  return true;
#endif

#if !STBI_NO_BMP
      if (stbi__bmp_info(ref s, out x, out y, out comp)) return true;
#endif

#if !STBI_NO_PSD
   if (stbi__psd_info(ref s, out x, out y, out comp))  return true;
#endif

#if !STBI_NO_PIC
   if (stbi__pic_info(ref s, out x, out y, out comp))  return true;
#endif

#if !STBI_NO_PNM
   if (stbi__pnm_info(ref s, out x, out y, out comp))  return true;
#endif

#if !STBI_NO_HDR
   if (stbi__hdr_info(ref s, out x, out y, out comp))  return true;
#endif

      // test tga last because it's a crappy test!
#if !STBI_NO_TGA
      if (stbi__tga_info(ref s, out x, out y, out comp))
         return true;
#endif
      return stbi__err("unknown image type", "Image not of any known type, or corrupt") != 0;
   }

   static bool stbi__is_16_main(ref stbi__context s)
   {
#if !STBI_NO_PNG
      if (stbi__png_is16(ref s)) return true;
#endif

#if !STBI_NO_PSD
   if (stbi__psd_is16(s))  return 1;
#endif

#if !STBI_NO_PNM
   if (stbi__pnm_is16(s))  return 1;
#endif
      return false;
   }

#if !STBI_NO_STDIO
static public int stbi_info(BytePtr filename, int *x, int *y, int *comp)
{
    FILE *f = stbi__fopen(filename, "rb");
    int result;
    if (!f) return stbi__err("can't fopen", "Unable to open file");
    result = stbi_info_from_file(f, x, y, comp);
    fclose(f);
    return result;
}

static public int stbi_info_from_file(FILE *f, int *x, int *y, int *comp)
{
   int r;
   stbi__context s;
   long pos = ftell(f);
   stbi__start_file(&s, f);
   r = stbi__info_main(&s,x,y,comp);
   fseek(f,pos,SEEK_SET);
   return r;
}

static public int stbi_is_16_bit(BytePtr filename)
{
    FILE *f = stbi__fopen(filename, "rb");
    int result;
    if (!f) return stbi__err("can't fopen", "Unable to open file");
    result = stbi_is_16_bit_from_file(f);
    fclose(f);
    return result;
}

static public int stbi_is_16_bit_from_file(FILE *f)
{
   int r;
   stbi__context s;
   long pos = ftell(f);
   stbi__start_file(&s, f);
   r = stbi__is_16_main(&s);
   fseek(f,pos,SEEK_SET);
   return r;
}
#endif // !STBI_NO_STDIO


   /*
      revision history:
         2.20  (2019-02-07) support utf8 filenames in Windows; fix warnings and platform ifdefs
         2.19  (2018-02-11) fix warning
         2.18  (2018-01-30) fix warnings
         2.17  (2018-01-29) change sbti__shiftsigned to avoid clang -O2 bug
                            1-bit BMP
                            *_is_16_bit api
                            avoid warnings
         2.16  (2017-07-23) all functions have 16-bit variants;
                            STBI_NO_STDIO works again;
                            compilation fixes;
                            fix rounding in unpremultiply;
                            optimize vertical flip;
                            disable raw_len validation;
                            documentation fixes
         2.15  (2017-03-18) fix png-1,2,4 bug; now all Imagenet JPGs decode;
                            warning fixes; disable run-time SSE detection on gcc;
                            uniform handling of optional "return" values;
                            thread-safe initialization of zlib tables
         2.14  (2017-03-03) remove deprecated STBI_JPEG_OLD; fixes for Imagenet JPGs
         2.13  (2016-11-29) add 16-bit API, only supported for PNG right now
         2.12  (2016-04-02) fix typo in 2.11 PSD fix that caused crashes
         2.11  (2016-04-02) allocate large structures on the stack
                            remove white matting for transparent PSD
                            fix reported channel count for PNG & BMP
                            re-enable SSE2 in non-gcc 64-bit
                            support RGB-formatted JPEG
                            read 16-bit PNGs (only as 8-bit)
         2.10  (2016-01-22) avoid warning introduced in 2.09 by STBI_REALLOC_SIZED
         2.09  (2016-01-16) allow comments in PNM files
                            16-bit-per-pixel TGA (not bit-per-component)
                            info() for TGA could break due to .hdr handling
                            info() for BMP to shares code instead of sloppy parse
                            can use STBI_REALLOC_SIZED if allocator doesn't support realloc
                            code cleanup
         2.08  (2015-09-13) fix to 2.07 cleanup, reading RGB PSD as RGBA
         2.07  (2015-09-13) fix compiler warnings
                            partial animated GIF support
                            limited 16-bpc PSD support
                            // #if unused functions
                            bug with < 92 byte PIC,PNM,HDR,TGA
         2.06  (2015-04-19) fix bug where PSD returns wrong '*comp' value
         2.05  (2015-04-19) fix bug in progressive JPEG handling, fix warning
         2.04  (2015-04-15) try to re-enable SIMD on MinGW 64-bit
         2.03  (2015-04-12) extra corruption checking (mmozeiko)
                            stbi_set_flip_vertically_on_load (nguillemot)
                            fix NEON support; fix mingw support
         2.02  (2015-01-19) fix incorrect assert, fix warning
         2.01  (2015-01-17) fix various warnings; suppress SIMD on gcc 32-bit without -msse2
         2.00b (2014-12-25) fix STBI_MALLOC in progressive JPEG
         2.00  (2014-12-25) optimize JPG, including x86 SSE2 & NEON SIMD (ryg)
                            progressive JPEG (stb)
                            PGM/PPM support (Ken Miller)
                            STBI_MALLOC,STBI_REALLOC,STBI_FREE
                            GIF bugfix -- seemingly never worked
                            STBI_NO_*, STBI_ONLY_*
         1.48  (2014-12-14) fix incorrectly-named assert()
         1.47  (2014-12-14) 1/2/4-bit PNG support, both direct and paletted (Omar Cornut & stb)
                            optimize PNG (ryg)
                            fix bug in interlaced PNG with user-specified channel count (stb)
         1.46  (2014-08-26)
                 fix broken tRNS chunk (colorkey-style transparency) in non-paletted PNG
         1.45  (2014-08-16)
                 fix MSVC-ARM internal compiler error by wrapping malloc
         1.44  (2014-08-07)
                 various warning fixes from Ronny Chevalier
         1.43  (2014-07-15)
                 fix MSVC-only compiler problem in code changed in 1.42
         1.42  (2014-07-09)
                 don't define _CRT_SECURE_NO_WARNINGS (affects user code)
                 fixes to stbi__cleanup_jpeg path
                 added STBI_ASSERT to avoid requiring assert.h
         1.41  (2014-06-25)
                 fix search&replace from 1.36 that messed up comments/error messages
         1.40  (2014-06-22)
                 fix gcc struct-initialization warning
         1.39  (2014-06-15)
                 fix to TGA optimization when req_comp != number of components in TGA;
                 fix to GIF loading because BMP wasn't rewinding (whoops, no GIFs in my test suite)
                 add support for BMP version 5 (more ignored fields)
         1.38  (2014-06-06)
                 suppress MSVC warnings on integer casts truncating values
                 fix accidental rename of 'skip' field of I/O
         1.37  (2014-06-04)
                 remove duplicate typedef
         1.36  (2014-06-03)
                 convert to header file single-file library
                 if de-iphone isn't set, load iphone images color-swapped instead of returning NULL
         1.35  (2014-05-27)
                 various warnings
                 fix broken STBI_SIMD path
                 fix bug where stbi_load_from_file no longer left file pointer in correct place
                 fix broken non-easy path for 32-bit BMP (possibly never used)
                 TGA optimization by Arseny Kapoulkine
         1.34  (unknown)
                 use STBI_NOTUSED in stbi__resample_row_generic(), fix one more leak in tga failure case
         1.33  (2011-07-14)
                 make stbi_is_hdr work in STBI_NO_HDR (as specified), minor compiler-friendly improvements
         1.32  (2011-07-13)
                 support for "info" function for all supported filetypes (SpartanJ)
         1.31  (2011-06-20)
                 a few more leak fixes, bug in PNG handling (SpartanJ)
         1.30  (2011-06-11)
                 added ability to load files via callbacks to accomidate custom input streams (Ben Wenger)
                 removed deprecated format-specific test/load functions
                 removed support for installable file formats (stbi_loader) -- would have been broken for IO callbacks anyway
                 error cases in bmp and tga give messages and don't leak (Raymond Barbiero, grisha)
                 fix inefficiency in decoding 32-bit BMP (David Woo)
         1.29  (2010-08-16)
                 various warning fixes from Aurelien Pocheville
         1.28  (2010-08-01)
                 fix bug in GIF palette transparency (SpartanJ)
         1.27  (2010-08-01)
                 cast-to-stbi_uc to fix warnings
         1.26  (2010-07-24)
                 fix bug in file buffering for PNG reported by SpartanJ
         1.25  (2010-07-17)
                 refix trans_data warning (Won Chun)
         1.24  (2010-07-12)
                 perf improvements reading from files on platforms with lock-heavy fgetc()
                 minor perf improvements for jpeg
                 deprecated type-specific functions so we'll get feedback if they're needed
                 attempt to fix trans_data warning (Won Chun)
         1.23    fixed bug in iPhone support
         1.22  (2010-07-10)
                 removed image *writing* support
                 stbi_info support from Jetro Lauha
                 GIF support from Jean-Marc Lienher
                 iPhone PNG-extensions from James Brown
                 warning-fixes from Nicolas Schulz and Janez Zemva (i.stbi__err. Janez (U+017D)emva)
         1.21    fix use of 'stbi_uc' in header (reported by jon blow)
         1.20    added support for Softimage PIC, by Tom Seddon
         1.19    bug in interlaced PNG corruption check (found by ryg)
         1.18  (2008-08-02)
                 fix a threading bug (local mutable static)
         1.17    support interlaced PNG
         1.16    major bugfix - stbi__convert_format converted one too many pixels
         1.15    initialize some fields for thread safety
         1.14    fix threadsafe conversion bug
                 header-file-only version (#define STBI_HEADER_FILE_ONLY before including)
         1.13    threadsafe
         1.12    const qualifiers in the API
         1.11    Support installable IDCT, colorspace conversion routines
         1.10    Fixes for 64-bit (don't use "unsigned long")
                 optimized upsampling by Fabian "ryg" Giesen
         1.09    Fix format-conversion for PSD code (bad global variables!)
         1.08    Thatcher Ulrich's PSD code integrated by Nicolas Schulz
         1.07    attempt to fix C++ warning/errors again
         1.06    attempt to fix C++ warning/errors again
         1.05    fix TGA loading to return correct *comp and use good luminance calc
         1.04    default float alpha is 1, not 255; use 'void *' for stbi_image_free
         1.03    bugfixes to STBI_NO_STDIO, STBI_NO_HDR
         1.02    support for (subset of) HDR files, float interface for preferred access to them
         1.01    fix bug: possible bug in handling right-side up bmps... not sure
                 fix bug: the stbi__bmp_load() and stbi__tga_load() functions didn't work at all
         1.00    interface to zlib that skips zlib header
         0.99    correct handling of alpha in palette
         0.98    TGA loader by lonesock; dynamically add loaders (untested)
         0.97    jpeg errors on too large a file; also catch another malloc failure
         0.96    fix detection of invalid v value - particleman@mollyrocket forum
         0.95    during header scan, seek to markers in case of padding
         0.94    STBI_NO_STDIO to disable stdio usage; rename all #defines the same
         0.93    handle jpegtran output; verbose errors
         0.92    read 4,8,16,24,32-bit BMP files of several formats
         0.91    output 24-bit Windows 3.0 BMP files
         0.90    fix a few more warnings; bump version number to approach 1.0
         0.61    bugfixes due to Marc LeBlanc, Christopher Lloyd
         0.60    fix compiling as c++
         0.59    fix warnings: merge Dave Moore's -Wall fixes
         0.58    fix bug: zlib uncompressed mode len/nlen was wrong endian
         0.57    fix bug: jpg last huffman symbol before marker was >9 bits but less than 16 available
         0.56    fix bug: zlib uncompressed mode len vs. nlen
         0.55    fix bug: restart_interval not initialized to 0
         0.54    allow NULL for 'int *comp'
         0.53    fix bug in png 3.4; speedup png decoding
         0.52    png handles req_comp=3,4 directly; minor cleanup; jpeg comments
         0.51    obey req_comp requests, 1-component jpegs return as 1-component,
                 on 'test' only check type, not whether we support this variant
         0.50  (2006-11-19)
                 first released version
   */


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
