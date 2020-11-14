# BatchImageConvertor v 2.15
> PCC: 0003B36D934C885B


A tool for automatic images groups processing

Инструмент для автоматической обработки групп изображений


#

This tool allows you to convert group of image files (including different formats) into
a single format. If it is necessary, you can specify uniform size or cropping method,
rotation, flip direction and images mode (color/greyscale/bitmap).

#

Инструмент позволяет преобразовывать группы изображений разных форматов в единый формат
и применять к каждому из них нужные настройки: размер или обрезку, поворот/отражение,
цветовой режим (цветное/оттенки серого/чёрно-белое).

&nbsp;



## Supported formats / поддерживаемые форматы:

```
Supported by application

Input file extension¹         Read         Write             Based on
.bmp, .dib, .rle               +             +                 .NET
.png                           +             +                 .NET
.gif                           +             +                 .NET
.tif, .tiff                    +             +                 .NET
.jpe, .jpg, .jpeg, .jfif       +             +                 .NET
.wmf, .emf                     +             -                 .NET
.ico                           +             -                 .NET

Supported by codec library²

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
```

*¹ All of these extensions (except palettes) are allowed in the input folder. Other ones will be ignored*

*² Some formats support provided by codec library. Utility can work without it, but these formats will not be processed*

*¹ Все перечисленные форматы (кроме форматов палитр) могут находиться в исходной папке. Остальные будут проигнорированы*

*²  Поддержка части форматов обеспечивается только при наличии библиотеки кодеков. Утилита может работать без неё, но соответствующие форматы поддерживаться не будут*

&nbsp;



## Requirements / Требования

Needs Windows XP or newer, Framework 4.0 or newer. Interface languages: ru_ru, en_us.
User manual languages: en_us, ru_ru (subs)

Требуется ОС Windows XP или новее, Framework 4.0 или новее. Языки интерфейса: ru_ru, en_us.
Языки руководства пользователя: en_us, ru_ru (субтитры)

&nbsp;



## Development policy and EULA / Политика разработки и EULA

This [Policy (ADP)](https://vk.com/@rdaaow_fupl-adp), its positions, conclusion, EULA and application methods
describes general rules that we follow in all of our development processes, released applications and implemented
ideas.
**It must be acquainted by participants and users before using any of laboratory's products.
By downloading them, you agree to this Policy**

#

Данная [Политика (ADP)](https://vk.com/@rdaaow_fupl-adp), её положения, заключение, EULA и способы применения
описывают общие правила, которым мы следуем во всех наших процессах разработки, вышедших в релиз приложениях
и реализованных идеях.
**Обязательна к ознакомлению всем участникам и пользователям перед использованием любого из продуктов лаборатории.
Загружая их, вы соглашаетесь с этой Политикой**
