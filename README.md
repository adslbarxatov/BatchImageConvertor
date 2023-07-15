# BatchImageConvertor v 3.4
> PCC: 00058F5EEF08AA42



A tool for batch images processing

Инструмент для автоматической обработки групп изображений


#

This tool allows you to convert group of image files (including different formats) into
a single format. If it is necessary, you can specify uniform size or cropping method,
rotation, flip direction and images mode (color / greyscale / bitmap).

Also this tool allows you to load or extract the palettes, modify and save them. Or even replace
the palette inside the indexed bitmap image.

#

Инструмент позволяет преобразовывать группы изображений разных форматов в единый формат
и применять к каждому из них нужные настройки: размер или обрезку, поворот/отражение,
цветовой режим (цветное / оттенки серого / чёрно-белое).

Также этот инструмент позволяет загружать или извлекать палитры, изменять и сохранять их.
Или даже заменить палитру внутри индексированного растрового изображения.

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

Supported by codec library²

.tga, .vda, .icb, .vst         +             +             Xash3D engine
.pcx, .pcc                     +             +               Own codec
.pnm, .pbm, .pgm, .ppm         +             +               Own codec
.ico                           +             -               Own codec

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

- Windows 7 or newer / или новее;
- [Microsoft .NET Framework 4.8](https://go.microsoft.com/fwlink/?linkid=2088631).
- [Microsoft Visual C++ 2015 – 2022 redistributable](https://aka.ms/vs/17/release/vc_redist.x86.exe).

Interface languages / языки интерфейса: ru_ru, en_us.

&nbsp;



## [Development policy and EULA](https://adslbarxatov.github.io/ADP) / [Политика разработки и EULA](https://adslbarxatov.github.io/ADP/ru)

This Policy (ADP), its positions, conclusion, EULA and application methods
describes general rules that we follow in all of our development processes, released applications and implemented ideas.
***It must be acquainted by participants and users before using any of laboratory’s products.
By downloading them, you agree and accept this Policy!***

Данная Политика (ADP), её положения, заключение, EULA и способы применения
описывают общие правила, которым мы следуем во всех наших процессах разработки, вышедших в релиз приложениях
и реализованных идеях.
***Обязательна к ознакомлению для всех участников и пользователей перед использованием любого из продуктов лаборатории.
Загружая их, вы соглашаетесь и принимаете эту Политику!***
