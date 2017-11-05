# BatchImageConvertor v 2.5
A tool for automatic images groups processing

#

This tool allows you to convert group of image files (including different formats) into
a single format. If it is necessary, you can specify uniform size, rotation, flip direction
and images mode (color/greyscale/bitmap).

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

Supported by codec library***

.tga, .vda, .icb, .vst         +          as .tga          Xash3D engine
.pcx, .pcc                     +          as .pcx            Own codec
.pnm, .pbm, .pgm, .ppm         +          as .pnm            Own codec
.jp2, .j2k, .j2c, .jpc, .jpt   +**     (-) in process     OpenJPEG library

- palettes formats

.act                           +             +               Own codec
.pal (Microsoft)               +             +               Own codec
.pal (JASC)                    +             +               Own codec
.aco                           +             -               Own codec
.ase                           +             -               Own codec
.bmp                      Extracting     Replacing           Own codec
.pcx, .pcc                Extracting         -               Own codec

* All of these extensions (except palettes) are allowed in the input folder. Other ones will be ignored
** .j2k, .j2c, .jpc and .jpt reading has not been tested yet. Support have been promised by OpenJPEG developers
*** Some formats support provided by codec library. Utility can work without it, but these formats will not be processed

```

#

Needs Windows XP and newer, Framework 4.0 and newer. Interface language: ru_ru
