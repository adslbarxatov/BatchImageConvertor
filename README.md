# BatchImageConvertor v 2.3
A tool for automatic images groups processing

#

This tool allows you to convert group of image files (including different formats) into
a single format. If it is necessary, you can specify uniform size, rotation, flip direction
and images mode (color/greyscale/bitmap).

Just place source images to the same folder, select target folder and launch conversion. Progress and
problems that occur will be displayed in the application window.

Some formats support provided by codec library. Utility can work without it, but these formats will not
be processed.

Supported formats: 

```
Supported by application

Input file extension*         Read         Write             Based on
.bmp, .dib, .rle               +          as .bmp              .NET
.png                           +             +                 .NET
.gif                           +             +                 .NET
.tif, .tiff                    +          as .tiff             .NET
.jpe, .jpg, .jpeg, .jfif       +          as .jpeg             .NET
.wmf, .emf                     +             -                 .NET
.ico                           +             -                 .NET

Supported by codec library

.tga, .vda, .icb, .vst         +          as .tga          Xash3D engine
.pcx, .pcc                     +          as .pcx      Codenet parsing manual
.pnm, .pbm, .pgm, .ppm         +          as .pnm                -
.jp2, .j2k, .j2c, .jpc, .jpt   +**     (-) in process     OpenJPEG library

* All of these extensions are allowed in the input folder. Other ones will be ignored
** .j2k, .j2c, .jpc and .jpt reading has not been tested yet. Support have been promised by OpenJPEG developers
```

#

Needs Windows XP and newer, Framework 4.0 and newer. Interface language: ru_ru
