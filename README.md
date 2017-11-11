# BatchImageConvertor v 2.6
A tool for automatic images groups processing

#

This tool allows you to convert group of image files (including different formats) into
a single format. If it is necessary, you can specify uniform size, rotation, flip direction
and images mode (color/greyscale/bitmap).

Supported formats: 

```
Supported by application

Input file extension*         Read         Write             Based on
.bmp, .dib, .rle               +             +                 .NET
.png                           +             +                 .NET
.gif                           +             +                 .NET
.tif, .tiff                    +             +                 .NET
.jpe, .jpg, .jpeg, .jfif       +             +                 .NET
.wmf, .emf                     +             -                 .NET
.ico                           +             -                 .NET

Supported by codec library**

.tga, .vda, .icb, .vst         +             +             Xash3D engine
.pcx, .pcc                     +             +               Own codec
.pnm, .pbm, .pgm, .ppm         +             +               Own codec
.jp2, .j2{k|c}, .jp{c|f|x}     +             +***        OpenJPEG library

- palettes formats

.act                           +             +               Own codec
.pal (Microsoft)               +             +               Own codec
.pal (JASC)                    +             +               Own codec
.aco                           +             -               Own codec
.ase                           +             -               Own codec
.bmp                      Extracting     Replacing           Own codec
.pcx, .pcc                Extracting         -               Own codec

* All of these extensions (except palettes) are allowed in the input folder. Other ones will be ignored
** Some formats support provided by codec library. Utility can work without it, but these formats will not be processed
*** Reading tested fully (including alpha-channel). OpenJPEG writer doesn't support alpha-channel for now

```

#

Needs Windows XP and newer, Framework 4.0 and newer. Interface language: ru_ru
