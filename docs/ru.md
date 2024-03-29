# Batch image convertor: руководство пользователя
> **ƒ** &nbsp;RD AAOW FDL; 26.11.2023; 22:21



### Содержание страницы

- [Общие сведения](#section-1)
- [Поддерживаемые форматы](#section-2)
- [Ссылки для загрузки](https://adslbarxatov.github.io/DPArray/ru#batch-image-convertor)
- [English version](https://adslbarxatov.github.io/BatchImageConvertor)

---

### Общие сведения

Инструмент позволяет преобразовывать группы изображений разных форматов в единый формат
и применять к каждому из них нужные настройки: размер или обрезку, поворот/отражение,
цветовой режим (цветное / оттенки серого / чёрно-белое).

Также этот инструмент позволяет загружать или извлекать палитры, изменять и сохранять их.
Или даже заменить палитру внутри индексированного растрового изображения

&nbsp;



### Поддерживаемые форматы

Все перечисленные форматы (кроме форматов палитр) могут находиться в исходной папке. Остальные будут проигнорированы.

Поддержка части форматов обеспечивается только при наличии библиотеки кодеков. Утилита может работать без неё, но соответствующие форматы поддерживаться не будут

&nbsp;



***Поддерживаемые приложением***

| Расширение входного файла | Чтение | Запись | Основана на |
|-|-|-|-|
| `.bmp`, `.dib`, `.rle` | Да | Да | .NET |
| `.jpe`, `.jpg`, `.jpeg`, `.jfif` | Да | Да | .NET |
| `.gif` | Да | Да | .NET |
| `.png` | Да | Да | .NET |
| `.tif`, `.tiff` | Да | Да | .NET |
| `.wmf`, `.emf` | Да | Нет | .NET |

&nbsp;



***Поддерживаемые библиотекой кодеков***

| Расширение входного файла | Чтение | Запись | Основана на |
|-|-|-|-|
| `.ico` | Да | Нет | Собственный кодек |
| `.pcx`, `.pcc` | Да | Да | Собственный кодек |
| `.pnm`, `.pbm`, `.pgm`, `.ppm` | Да | Да | Собственный кодек |
| `.tga`, `.vda`, `.icb`, `.vst` | Да | Да | [Xash FWGS engine](https://github.com/FWGS/xash3d-fwgs) |

&nbsp;



***Поддерживаемые сторонними утилитами***

| Расширение входного файла | Чтение | Запись | Основана на |
|-|-|-|-|
| `.avif` | Да | Нет | [Инструмент davif](https://github.com/link-u/davif) |
| `.jp2`, `.j2c`, `.j2k`, `.jpc`, `.j2c`, `.jpf`, `.jpx` | Да | Нет | [Инструмент openjp2](https://github.com/uclouvain/openjpeg) |
| `.webp` | Да | Нет | [Инструмент dwebp](https://developers.google.com/speed/webp/docs/dwebp) |

&nbsp;



***Форматы палитр***

| Расширение входного файла | Чтение | Запись | Основана на |
|-|-|-|-|
| `.aco` | Да | Нет | Собственный кодек |
| `.act` | Да | Да | Собственный кодек |
| `.ase` | Да | Нет | Собственный кодек |
| `.pal` (Microsoft) | Да | Да | Собственный кодек |
| `.pal` (JASC) | Да | Да | Собственный кодек |
| `.bmp`, `.dib`, `.rle` | Извлечение | Замена | Собственный кодек |
| `.pcx`, `.pcc` | Извлечение | Нет | Собственный кодек |
