# Batch image convertor: user guide
> **ƒ** &nbsp;RD AAOW FDL; 26.11.2023; 22:17



### Page contents

- [General information](#general-information)
- [Supported formats](#supported-formats)
- [Download links](https://adslbarxatov.github.io/DPArray#batch-image-convertor)
- [Версия на русском языке](https://adslbarxatov.github.io/BatchImageConvertor/ru)

---

### General information

This tool allows you to convert group of image files (including different formats) into
a single format. If it is necessary, you can specify uniform size or cropping method,
rotation, flip direction and images mode (color / greyscale / bitmap).

Also this tool allows you to load or extract palettes, modify and save them. Or even replace
the palette inside the indexed bitmap image

&nbsp;



### Supported formats

All of these extensions (except palettes) are allowed in the input folder. Other ones will be ignored.

Some formats support provided by codec library. Utility can work without it, but these formats will not be processed

&nbsp;



***Supported by application***

| Input file extension | Read | Write | Based on |
|-|-|-|-|
| `.bmp`, `.dib`, `.rle` | Yes | Yes | .NET |
| `.gif` | Yes | Yes | .NET |
| `.jpe`, `.jpg`, `.jpeg`, `.jfif` | Yes | Yes | .NET |
| `.png` | Yes | Yes | .NET |
| `.tif`, `.tiff` | Yes | Yes | .NET |
| `.wmf`, `.emf` | Yes | No | .NET |

&nbsp;



***Supported by codec library***

| Input file extension | Read | Write | Based on |
|-|-|-|-|
| `.ico` | Yes | No | Own codec |
| `.pcx`, `.pcc` | Yes | Yes | Own codec |
| `.pnm`, `.pbm`, `.pgm`, `.ppm` | Yes | Yes | Own codec |
| `.tga`, `.vda`, `.icb`, `.vst` | Yes | Yes | [Xash FWGS engine](https://github.com/FWGS/xash3d-fwgs) |

&nbsp;



***Supported by side utilities***

| Input file extension | Read | Write | Based on |
|-|-|-|-|
| `.avif` | Yes | No | [davif tool](https://github.com/link-u/davif) |
| `.jp2`, `.j2c`, `.j2k`, `.jpc`, `.j2c`, `.jpf`, `.jpx` | Yes | No | [openjp2 tool](https://github.com/uclouvain/openjpeg) |
| `.webp` | Yes | No | [dwebp tool](https://developers.google.com/speed/webp/docs/dwebp) |

&nbsp;



***Palettes formats***

| Input file extension | Read | Write | Based on |
|-|-|-|-|
| `.aco` | Yes | No | Own codec |
| `.act` | Yes | Yes | Own codec |
| `.ase` | Yes | No | Own codec |
| `.pal` (Microsoft) | Yes | Yes | Own codec |
| `.pal` (JASC) | Yes | Yes | Own codec |
| `.bmp`, `.dib`, `.rle` | Extracting | Replacing | Own codec |
| `.pcx`, `.pcc` | Extracting | No | Own codec |
