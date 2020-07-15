# BatchImageConvertor v 2.13h

A tool for automatic images groups processing

Инструмент для автоматической обработки групп изображений

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

## Requirements / Требования

Needs Windows XP and newer, Framework 4.0 and newer. Interface languages: ru_ru, en_us.
User manual languages: en_us, ru_ru (subs)

Требуется ОС Windows XP и новее, Framework 4.0 и новее. Языки интерфейса: ru_ru, en_us.
Языки руководства пользователя: en_us, ru_ru (субтитры)

## Development policy and EULA / Политика разработки и EULA

This [Policy (ADP)](https://vk.com/@rdaaow_fupl-adp), its positions, conclusion, EULA and application methods
describes general rules that we follow in all of our development processes, released applications and implemented
ideas.
**It must be acquainted by participants and users before using any of laboratory's products.
By downloading them, you agree to this Policy**

Данная [Политика (ADP)](https://vk.com/@rdaaow_fupl-adp), её положения, заключение, EULA и способы применения
описывают общие правила, которым мы следуем во всех наших процессах разработки, вышедших в релиз приложениях
и реализованных идеях.
**Обязательна к ознакомлению всем участникам и пользователям перед использованием любого из продуктов лаборатории.
Загружая их, вы соглашаетесь с этой Политикой**
