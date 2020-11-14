// Общие заголовки
#include "BatchImageConvertor.h"

#include "TGA.h"
#include "PCX.h"
#include "PBM.h"
#include "JP2.h"
#include "ICO.h"

#include "ACT.h"
#include "BMP.h"
#include "PALw.h"
#include "PALj.h"
#include "ACO.h"
#include "ASE.h"

// Функции формируют значения в формате LittleEndian из значений BigEndian и наоборот
uint LEBE_i (uint Value)
	{
	return ((Value & 0xFF) << 8) | ((Value & 0xFF00) >> 8);
	}

ulong LEBE_l (ulong Value)
	{
	return ((Value & 0xFF) << 24) | ((Value & 0xFF00) << 8) | ((Value & 0xFF0000) >> 8) | ((Value & 0xFF000000) >> 24);
	}

// Интерфейсы для функций TGA (буфер в формате RGBA)
BIC_API sint BIC_CONV TGA_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	// Переменные
	struct rgbdata_t buf;
	sint res;
	uchar hasAlpha = 0;

	// Контроль на корректность переданных параметров
	BIC_CHECK_INPUT_PARAMETERS

	// Запрос на нижний уровень
	if ((res = Image_LoadTGA (FileName, &buf)) < 0)
		{
		return res;
		}

	// Сборка ответа
	*Buffer = buf.buffer;
	*Width = buf.width;
	*Height = buf.height;
	if (buf.type != PF_RGBA_32)
		{
		// Не может быть, но мало ли
		return EXEC_INVALID_FILE;
		}
	if (buf.flags & IMAGE_HAS_ALPHA)
		{
		hasAlpha = 1;
		}

	return EXEC_OK;
	}

BIC_API sint BIC_CONV TGA_Save (schar *FileName, uint Width, uint Height, uchar *Buffer)
	{
	// Переменные
	struct rgbdata_t buf;
	uchar hasAlpha = 0;
	slong i;

	// Контроль на корректность переданных параметров
	BIC_CHECK_OUTPUT_PARAMETERS

	// Поиск альфа-каналов
	for (i = 0; i < Width * Height; i++)
		{
		if (Buffer[i * 4 + 3] != 255)
			{
			hasAlpha++;
			break;
			}
		}

	// Сборка запроса
	buf.flags = ((hasAlpha != 0) ? IMAGE_HAS_ALPHA : 0) | IMAGE_HAS_COLOR;
	buf.height = Height;
	buf.width = Width;
	buf.type = PF_RGBA_32;
	buf.buffer = Buffer;

	// Запрос на нижний уровень
	return Image_SaveTGA (FileName, &buf);
	}

// Интерфейсы для функций PCX (буфер в формате RGB)
BIC_API sint BIC_CONV PCX_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	union RGB_Palette_16 *dp = NULL;
	union RGB_Palette_256 *ep = NULL;
	return PCX_LoadImage (FileName, Width, Height, Buffer, &dp, &ep);
	}

BIC_API sint BIC_CONV PCX_Save (schar *FileName, uint Width, uint Height, uchar *Buffer)
	{
	return PCX_SaveImage (FileName, Width, Height, Buffer);
	}

BIC_API sint BIC_CONV PCX_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	uint Width = 0;
	uint Height = 0;
	uchar *Buffer;
	union RGB_Palette_16 *dp = NULL;
	union RGB_Palette_256 *ep = NULL;
	
	int res = PCX_LoadImage (FileName, &Width, &Height, &Buffer, &dp, &ep);
	if (res < 0)
		{
		return res;
		}

	// Возврат
	*ColorsCount = 16;
	*Palette = dp->Ptr;
	if (ep)
		{
		*ColorsCount = 256;
		*Palette = ep->Ptr;
		}

	return res;
	}

BIC_API sint BIC_CONV PCX_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return EXEC_NOT_IMPLEMENTED;
	}

// Интерфейсы для функций PBM (буфер в формате RGB)
BIC_API sint BIC_CONV PBM_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	return PBM_LoadImage (FileName, Width, Height, Buffer);
	}

BIC_API sint BIC_CONV PBM_Save (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar ImageType)
	{
	return PBM_SaveImage (FileName, Width, Height, Buffer, ImageType);
	}

// Интерфейсы для функций JPEG2000 (буфер в формате RGB)
BIC_API sint BIC_CONV JP2_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	//return JP2_LoadImage (FileName, Width, Height, Buffer);

	// Функция неисправна
	return EXEC_NOT_IMPLEMENTED;
	}

BIC_API sint BIC_CONV JP2_Save (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar CodecType)
	{
	//return JP2_SaveImage (FileName, Width, Height, Buffer, CodecType);
	
	// Функция неисправна
	return EXEC_NOT_IMPLEMENTED;
	}

// Интерфейсы для функций JPEG2000 (буфер в формате RGB)
BIC_API sint BIC_CONV ICO_Load (schar* FileName, uint* Width, uint* Height, uchar** Buffer)
	{
	return ICO_LoadImage (FileName, Width, Height, Buffer);

	// Функция неисправна
	return EXEC_NOT_IMPLEMENTED;
	}

BIC_API sint BIC_CONV ICO_Save (schar* FileName, uint Width, uint Height, uchar* Buffer, uchar CodecType)
	{
	// Функция не используется
	return EXEC_NOT_IMPLEMENTED;
	}

// Интерфейсы для палитры BMP (буфер в формате RGBA, где A - сепаратор)
BIC_API sint BIC_CONV BMP_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGBA_Color *palette;
	int res = BMP_PaletteExchange (FileName, &palette, ColorsCount, 0);
	if (res < 0)
		{
		return res;
		}

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint BIC_CONV BMP_SetPalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return BMP_PaletteExchange (FileName, &((union RGBA_Color *)Palette), &ColorsCount, 1);
	}

// Интерфейсы для палитры ACT (буфер в формате RGB)
BIC_API sint BIC_CONV ACT_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGBA_Color *palette;

	int res = ACT_LoadPaletteEx (FileName, &palette, ColorsCount);
	if (res < 0)
		{
		return res;
		}

	*Palette = palette->Ptr;
	return 0;
	}

BIC_API sint BIC_CONV ACT_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	// Контроль
	if (!FileName || !Palette || (ColorsCount < 1) || (ColorsCount > 256))
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// Транзит
	return ACT_SavePaletteEx (FileName, ((union RGBA_Color *)Palette), ColorsCount);
	}

// Интерфейсы для палитры Microsoft PAL (буфер в формате RGBA, где A - сепаратор)
BIC_API sint BIC_CONV PALw_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGBA_Color *palette;
	int res = PALw_LoadPaletteEx (FileName, &palette, ColorsCount);
	if (res < 0)
		{
		return res;
		}

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint BIC_CONV PALw_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return PALw_SavePaletteEx (FileName, ((union RGBA_Color *)Palette), ColorsCount);
	}

// Интерфейсы для палитры JASC PAL (буфер в формате RGB)
BIC_API sint BIC_CONV PALj_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGB_Color *palette;
	int res = PALj_LoadPaletteEx (FileName, &palette, ColorsCount);
	if (res < 0)
		{
		return res;
		}

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint BIC_CONV PALj_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return PALj_SavePaletteEx (FileName, ((union RGB_Color *)Palette), ColorsCount);
	}

// Интерфейсы для палитры Adobe Color Swatches (буфер в формате RGB)
BIC_API sint BIC_CONV ACO_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGB_Color *palette;
	int res = ACO_LoadPaletteEx (FileName, &palette, ColorsCount);
	if ((res < 0) && (res != EXEC_UNSUPPORTED_COLORS))
		{
		return res;
		}

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint BIC_CONV ACO_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return ACO_SavePaletteEx (FileName, ((union RGB_Color *)Palette), ColorsCount);
	}

// Интерфейсы для палитры Adobe Swatches Exchange (буфер в формате RGB)
BIC_API sint BIC_CONV ASE_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGB_Color *palette;
	int res = ASE_LoadPaletteEx (FileName, &palette, ColorsCount);
	if ((res < 0) && (res != EXEC_UNSUPPORTED_COLORS))
		{
		return res;
		}

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint BIC_CONV ASE_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return ASE_SavePaletteEx (FileName, ((union RGB_Color *)Palette), ColorsCount);
	}

// Интерфейс освобождения памяти для создаваемых буферов
BIC_API void BIC_CONV BIC_ReleaseBuffer (uchar *Buffer)
	{
	if (Buffer)
		free (Buffer);
	}

// Метод возвращает версию библиотеки для контроля совместимости
BIC_API schar* BIC_CONV BIC_GetLibVersion ()
	{
	return BIC_VERSION_S;
	}
