#define _CRT_SECURE_NO_WARNINGS

// Основные заголовки
#include <stdio.h>
#include <malloc.h>
#include <string.h>

// Стандартное переопределение типов
#include "..\\Generics\\CSTypes.h"

#define BIC_VERSION					2,12,5,0
#define BIC_VERSION_S				"2.12.5.0"
#define BIC_PRODUCT					"Batch image convertor codecs library v 2.12.5L"
#define BIC_COMPANY					"RD AAOW"

#define BIC_API		extern __declspec(dllexport)
#define BIC_CONV	__stdcall

// Максимальные размеры входных изображений
#define IMAGE_MAXWIDTH			(1 << 14)
#define IMAGE_MAXHEIGHT			IMAGE_MAXWIDTH

// Коды ошибок
#define EXEC_OK						0
#define EXEC_INVALID_PARAMETERS		-1
#define EXEC_FILE_UNAVAILABLE		-2
#define EXEC_INVALID_FILE			-3
#define EXEC_MEMORY_ALLOC_FAIL		-4

#define EXEC_NO_PALETTE_AVAILABLE	-11
#define EXEC_UNSUPPORTED_COLORS		-12

#define EXEC_NOT_IMPLEMENTED		-100

// Макросокращения
#define BIC_CHECK_INPUT_PARAMETERS		if (!Width || !Height || !FileName)\
											{\
											return EXEC_INVALID_PARAMETERS;\
											}
#define BIC_INIT_FILE_READING			if ((FS = fopen (FileName, "rb")) == NULL)\
											{\
											return EXEC_FILE_UNAVAILABLE;\
											}
#define BIC_IMAGE_IS_INVALID			(*Width > IMAGE_MAXWIDTH) || (*Height > IMAGE_MAXHEIGHT) || (*Width * *Height == 0)
#define BIC_INIT_FILE_WRITING			if ((FS = fopen (FileName, "wb")) == NULL)\
											{\
											return EXEC_FILE_UNAVAILABLE;\
											}
#define BIC_CHECK_OUTPUT_PARAMETERS		if (!FileName || !Buffer || (Width * Height == 0) || (Width > IMAGE_MAXWIDTH) ||\
											(Height > IMAGE_MAXHEIGHT))\
											{\
											return EXEC_INVALID_PARAMETERS;\
											}

// Стандартные цветовые палитры в 16 и 256 цветов
union RGB_Color
	{
	struct RGBs
		{
		uchar R;
		uchar G;
		uchar B;
		} RGB;
	uchar Ptr[sizeof (struct RGBs)];
	};

union RGBA_Color
	{
	struct RGBAs
		{
		uchar R;
		uchar G;
		uchar B;
		uchar A;
		} RGBA;
	uchar Ptr[sizeof (struct RGBAs)];
	};

union RGB_Palette_256
	{
	union RGB_Color Colors[256];
	uchar Ptr[256 * sizeof (union RGB_Color)];
	};

union RGB_Palette_16
	{
	union RGB_Color Colors[16];
	uchar Ptr[16 * sizeof (union RGB_Color)];
	};

// Функции перегонки цветов
#define BIC_INPUT_COLOR_LIMIT		0xFFFF
#define BIC_ICL		BIC_INPUT_COLOR_LIMIT

union RGB_Color RGBtoRGB (uint R, uint G, uint B);
union RGB_Color GreytoRGB (uint V);
union RGB_Color HSBtoRGB (uint H, uint S, uint B);

union RGB_Color RGBtoRGB_f (float R, float G, float B);
union RGB_Color GreytoRGB_f (float V);

// Функции преобразования значений LittleEndian <-> BigEndian
uint LEBE_i (uint Value);
ulong LEBE_l (ulong Value);

// Общие декларации
BIC_API void BIC_CONV BIC_ReleaseBuffer (uchar *Buffer);
BIC_API schar* BIC_CONV BIC_GetLibVersion ();

// Декларации (все функции работают с изображениями в формате RGBA32)
BIC_API sint BIC_CONV TGA_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer);
BIC_API sint BIC_CONV TGA_Save (schar *FileName, uint Width, uint Height, uchar *Buffer);

BIC_API sint BIC_CONV PCX_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer);
BIC_API sint BIC_CONV PCX_Save (schar *FileName, uint Width, uint Height, uchar *Buffer);

BIC_API sint BIC_CONV PBM_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer);
BIC_API sint BIC_CONV PBM_Save (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar ImageType);

BIC_API sint BIC_CONV JP2_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer);
BIC_API sint BIC_CONV JP2_Save (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar CodecType);

// Декларации для палитр (битность цветов может различаться)
BIC_API sint BIC_CONV PCX_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount);	// RGB
BIC_API sint BIC_CONV PCX_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount);		// RGB

BIC_API sint BIC_CONV BMP_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount);	// RGBA
BIC_API sint BIC_CONV BMP_SetPalette (schar *FileName, uchar *Palette, uint ColorsCount);		// RGBA

BIC_API sint BIC_CONV ACT_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount);	// RGBA
BIC_API sint BIC_CONV ACT_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount);		// RGBA

BIC_API sint BIC_CONV PALw_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount);	// RGBA
BIC_API sint BIC_CONV PALw_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount);		// RGBA

BIC_API sint BIC_CONV PALj_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount);	// RGB
BIC_API sint BIC_CONV PALj_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount);		// RGB

BIC_API sint BIC_CONV ACO_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount);	// RGB
BIC_API sint BIC_CONV ACO_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount);		// RGB

BIC_API sint BIC_CONV ASE_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount);	// RGB
BIC_API sint BIC_CONV ASE_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount);		// RGB
