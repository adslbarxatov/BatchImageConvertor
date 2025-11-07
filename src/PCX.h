// Спецификации для PCX
#define PCX_MF			0x0A
#define PCX_PALPREF		0x0C

#define PCX_V_2_5		0
#define PCX_V_2_8_PAL	2
#define PCX_V_2_8_NOPAL	3
#define PCX_V_3_0		5

#define PCX_RLE			1

#define PCX_MONOCHROME	0
#define PCX_COLOR		1
#define PCX_GREYSCALE	2

// Заголовок PCX
union PCXHeader
	{
	// Структура заголовка
	struct PCXHeaderStructure
		{
		uchar Manufacturer;		// Постоянный флаг 10 = ZSoft .PCX
		uchar Version;			// 0 = версия 2.5
								// 2 = версия 2.8 с информацией о палитре
								// 3 = версия 2.8 без информации о палитре
								// 5 = версия 3.0
		uchar Encoding;			// 1 = кодирование длинными сериями
		uchar BitsPerPixel;		// Число бит на пиксель в слое
		uint XMin;				// Минимальная абсцисса изображения
		uint YMin;				// Минимальная ордината изображения
		uint XMax;				// Максимальная абсцисса изображения
		uint YMax;				// Максимальная ордината изображения
		uint HRes;				// Горизонтальное разрешение создающего устройства
		uint VRes;				// Вертикальное разрешение создающего устройства
		union RGB_Palette_16 DefaultPalette;	// Набор цветовой палитры
		uchar _reserved;
		uchar LayersCount;		// Число цветовых слоев
		uint BytesPerLine;		// Число байт на строку в цветовом слое (для PCX-файлов всегда должно быть чётным)
		uint PaletteType;		// Вариант палитры:
								// 1 = цветная/чёрно-белая
								// 2 = градации серого
		uchar _filler[58];		// Заполняется нулями до конца заголовка
		} PCXHeaderS;

	uchar PCXHeaderPtr[sizeof (struct PCXHeaderStructure)];
	};

// Декларации
sint PCX_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer, 
	union RGB_Palette_16 **DefaultPalette, union RGB_Palette_256 **ExtendedPalette);
sint PCX_SaveImage (schar *FileName, uint Width, uint Height, uchar *Buffer, uint Resolution);
