# BatchImageConvertor v 2.12

A tool for automatic images groups processing / Инструмент для автоматической обработки групп изображений

#

This tool allows you to convert group of image files (including different formats) into
a single format. If it is necessary, you can specify uniform size or cropping method,
rotation, flip direction and images mode (color/greyscale/bitmap).

Инструмент позволяет преобразовывать группы изображений разных форматов в единый формат
и применять к каждому из них нужные настройки: размер или обрезку, поворот/отражение,
цветовой режим (цветное/оттенки серого/чёрно-белое).


Supported formats / поддерживаемые форматы:

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

- palettes formats

.act                           +             +               Own codec
.pal (Microsoft)               +             +               Own codec
.pal (JASC)                    +             +               Own codec
.aco                           +             -               Own codec
.ase                           +             -               Own codec
.bmp, .dib, .rle          Extracting     Replacing           Own codec
.pcx, .pcc                Extracting         -               Own codec

*   All of these extensions (except palettes) are allowed in the input folder. Other ones 
    will be ignored
**  Some formats support provided by codec library. Utility can work without it, but these 
    formats will not be processed
*   Все перечисленные форматы (кроме форматов палитр) могут находиться в исходной папке. 
    Остальные будут проигнорированы
**  Поддержка части форматов обеспечивается только при наличии библиотеки кодеков. Утилита 
    может работать без неё, но соответствующие форматы поддерживаться не будут

```

#

We've formalized our [Applications development policy (ADP)](https://vk.com/@rdaaow_fupl-adp).
We're strongly recommend reading it before using our products.

Мы формализовали нашу [Политику разработки приложений (ADP)](https://vk.com/@rdaaow_fupl-adp).
Настоятельно рекомендуем ознакомиться с ней перед использованием наших продуктов.

#

Needs Windows XP and newer, Framework 4.0 and newer. Interface languages: ru_ru, en_us

Требуется ОС Windows XP и новее, Framework 4.0 и новее. Языки интерфейса: ru_ru, en_us
