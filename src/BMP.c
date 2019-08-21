// Рисунок BMP

#include "BatchImageConvertor.h"
#include "BMP.h"

#define IBMP_EXIT(c)		fclose (FS); return c;

// Контроль корректности битности изображения
uchar IsBitsPerPixelValueValid (uint Value)
	{
	if ((Value == 1) || (Value == 2) || (Value == 4) ||
		(Value == 8) || (Value == 16) || (Value == 24) ||
		(Value == 32) || (Value == 48) || (Value == 64))
		{
		return 1;
		}
	return 0;
	}

// Функция загружает палитру из файла BMP (можно расширить до загрузки изображения)
sint BMP_PaletteExchange (schar *FileName, union RGBA_Color **Palette, uint *ColorsCount, uchar SetPalette)
	{
		// Переменные
	union BMPHeader bmph;			// Заголовок файла
	union BMPInfoVCore infoVCore;	// Структуры-описатели
	union BMPInfoV3 infoV3;
	union BMPInfoV4 infoV4;
	union BMPInfoV5 infoV5;
	union BMP_RGBBitmasks rgbMasks;
	union BMP_RGBABitmasks rgbaMasks;
	uchar *pixelData;

	FILE *FS;						// Вспомогательные параметры
	ulong colorsCount;
	ulong i;

	union RGB_Color *supPalette;
	union RGBA_Color *resPalette;

	// Контроль
	if (!FileName || !ColorsCount || (SetPalette != 0) && !Palette)
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// Открытие файла
	BIC_INIT_FILE_READING

	// Чтение заголовка и контроль
	if (fread (bmph.Ptr, 1, sizeof (bmph), FS) != sizeof (bmph))
		{
		IBMP_EXIT (EXEC_INVALID_FILE)
		}

	fseek (FS, 0, SEEK_END);
	if ((ftell (FS) != bmph.BMPHeaderS.FileSize) || (bmph.BMPHeaderS.Signature != BMP_SIGNATURE) || 
		(bmph.BMPHeaderS.DataOffset == 0))
		{
		IBMP_EXIT (EXEC_INVALID_FILE)
		}

	fseek (FS, sizeof (bmph), SEEK_SET);
	
	// Чтение описателей
	switch (bmph.BMPHeaderS.FileVersionMarker)
		{
		case BMP_V_CORE:
			if (fread (infoVCore.Ptr, 1, sizeof (infoVCore), FS) != sizeof (infoVCore) || 
				(IsBitsPerPixelValueValid (infoVCore.BMPInfoS.BitsPerPixel) == 0))
				{
				IBMP_EXIT (EXEC_INVALID_FILE)
				}
			if (infoVCore.BMPInfoS.BitsPerPixel > 8)
				{
				IBMP_EXIT (EXEC_NO_PALETTE_AVAILABLE)
				}
			colorsCount = (1 << infoVCore.BMPInfoS.BitsPerPixel);
			break;

		case BMP_V_3:
			if (fread (infoV3.Ptr, 1, sizeof (infoV3), FS) != sizeof (infoV3) || 
				(IsBitsPerPixelValueValid (infoV3.BMPInfoS.BitsPerPixel) == 0))
				{
				IBMP_EXIT (EXEC_INVALID_FILE)
				}
			if ((infoV3.BMPInfoS.BitsPerPixel > 8) && (infoV3.BMPInfoS.ColorTableSize == 0))	// Оптимизационная палитра
				{
				IBMP_EXIT (EXEC_NO_PALETTE_AVAILABLE)
				}

			// Bitmasks
			if ((infoV3.BMPInfoS.CompressionType == BMP_COMPR_BITFIELDS) && 
				(fread (rgbMasks.Ptr, 1, sizeof (rgbMasks), FS) != sizeof (rgbMasks)))
				{
				IBMP_EXIT (EXEC_INVALID_FILE)
				}
			if ((infoV3.BMPInfoS.CompressionType == BMP_COMPR_ALPHABITFIELDS) && 
				(fread (rgbaMasks.Ptr, 1, sizeof (rgbaMasks), FS) != sizeof (rgbaMasks)))
				{
				IBMP_EXIT (EXEC_INVALID_FILE)
				}

			// Количество
			colorsCount = infoV3.BMPInfoS.ColorTableSize;
			if ((infoV3.BMPInfoS.BitsPerPixel <= 8) && (infoV3.BMPInfoS.ColorTableSize == 0))
				{
				colorsCount = (1 << infoV3.BMPInfoS.BitsPerPixel);
				}
			break;

		case BMP_V_4:
			if (fread (infoV4.Ptr, 1, sizeof (infoV4), FS) != sizeof (infoV4) || 
				(IsBitsPerPixelValueValid (infoV4.BMPInfoS.V3Info.BMPInfoS.BitsPerPixel) == 0))
				{
				IBMP_EXIT (EXEC_INVALID_FILE)
				}
			if ((infoV4.BMPInfoS.V3Info.BMPInfoS.BitsPerPixel > 8) && 
				(infoV4.BMPInfoS.V3Info.BMPInfoS.ColorTableSize == 0))	// Оптимизационная палитра
				{
				IBMP_EXIT (EXEC_NO_PALETTE_AVAILABLE)
				}

			// Количество
			colorsCount = infoV4.BMPInfoS.V3Info.BMPInfoS.ColorTableSize;
			if ((infoV4.BMPInfoS.V3Info.BMPInfoS.BitsPerPixel <= 8) && 
				(infoV4.BMPInfoS.V3Info.BMPInfoS.ColorTableSize == 0))
				{
				colorsCount = (1 << infoV4.BMPInfoS.V3Info.BMPInfoS.BitsPerPixel);
				}
			break;

		case BMP_V_5:
			if (fread (infoV5.Ptr, 1, sizeof (infoV5), FS) != sizeof (infoV5) || 
				(IsBitsPerPixelValueValid (infoV5.BMPInfoS.V4Info.BMPInfoS.V3Info.BMPInfoS.BitsPerPixel) == 0))
				{
				IBMP_EXIT (EXEC_INVALID_FILE)
				}
			if ((infoV5.BMPInfoS.V4Info.BMPInfoS.V3Info.BMPInfoS.BitsPerPixel > 8) && 
				(infoV5.BMPInfoS.V4Info.BMPInfoS.V3Info.BMPInfoS.ColorTableSize == 0))	// Оптимизационная палитра
				{
				IBMP_EXIT (EXEC_NO_PALETTE_AVAILABLE)
				}

			// Количество
			colorsCount = infoV5.BMPInfoS.V4Info.BMPInfoS.V3Info.BMPInfoS.ColorTableSize;
			if ((infoV5.BMPInfoS.V4Info.BMPInfoS.V3Info.BMPInfoS.BitsPerPixel <= 8) && 
				(infoV5.BMPInfoS.V4Info.BMPInfoS.V3Info.BMPInfoS.ColorTableSize == 0))
				{
				colorsCount = (1 << infoV5.BMPInfoS.V4Info.BMPInfoS.V3Info.BMPInfoS.BitsPerPixel);
				}
			break;

		default:
			IBMP_EXIT (EXEC_INVALID_FILE)
		}

	// Это может создать определённые проблемы
	/*if ((colorsCount == 0) || (colorsCount > BMP_MAX_COLORS))
		{
		colorsCount = BMP_MAX_COLORS;
		}*/

	// Чтение палитры
	if ((resPalette = (union RGBA_Color *)malloc (colorsCount * sizeof (union RGBA_Color))) == NULL)
		{
		IBMP_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}

	if (bmph.BMPHeaderS.FileVersionMarker == BMP_V_CORE)
		{
		// Создание и перенос вспомогательной палитры
		if ((supPalette = (union RGB_Color *)malloc (colorsCount * sizeof (union RGB_Color))) == NULL)
			{
			IBMP_EXIT (EXEC_MEMORY_ALLOC_FAIL)
			}
		if (fread (supPalette, 1, colorsCount * sizeof (union RGB_Color), FS) != colorsCount * sizeof (union RGB_Color))
			{
			IBMP_EXIT (EXEC_INVALID_FILE)
			}

		for (i = 0; i < colorsCount; i++)
			{
			resPalette[i].RGBA.R = supPalette[i].RGB.R;
			resPalette[i].RGBA.G = supPalette[i].RGB.G;
			resPalette[i].RGBA.B = supPalette[i].RGB.B;
			}

		free (supPalette);
		}
	else
		{
		if (fread (resPalette, 1, colorsCount * sizeof (union RGBA_Color), FS) != colorsCount * sizeof (union RGBA_Color))
			{
			IBMP_EXIT (EXEC_INVALID_FILE)
			}
		}

	// Нормализация палитры
	for (i = 0; i < colorsCount; i++)
		{
		resPalette[i].RGBA.A = resPalette[i].RGBA.R;
		resPalette[i].RGBA.R = resPalette[i].RGBA.B;
		resPalette[i].RGBA.B = resPalette[i].RGBA.A;
		resPalette[i].RGBA.A = 255;
		}

	// Обработка палитры
	if (SetPalette == 0)
		{
		*ColorsCount = colorsCount;
		*Palette = resPalette;
		}
	else
		{
		// Установка новой палитры в файле
		// Сначала требуется дочитать файл
		if ((pixelData = (uchar *)malloc (bmph.BMPHeaderS.FileSize - bmph.BMPHeaderS.DataOffset)) == NULL)
			{
			IBMP_EXIT (EXEC_MEMORY_ALLOC_FAIL)
			}
		if (fread (pixelData, 1, bmph.BMPHeaderS.FileSize - bmph.BMPHeaderS.DataOffset, FS) != 
			bmph.BMPHeaderS.FileSize - bmph.BMPHeaderS.DataOffset)
			{
			IBMP_EXIT (EXEC_INVALID_FILE)
			}
		fclose (FS);

		// Затем открыть его для записи
		BIC_INIT_FILE_WRITING

		fwrite (bmph.Ptr, 1, sizeof (bmph), FS);
		switch (bmph.BMPHeaderS.FileVersionMarker)
			{
			case BMP_V_CORE:
				fwrite (infoVCore.Ptr, 1, sizeof (infoVCore), FS);
				break;

			case BMP_V_3:
				fwrite (infoV3.Ptr, 1, sizeof (infoV3), FS);

				// Bitmasks
				if (infoV3.BMPInfoS.CompressionType == BMP_COMPR_BITFIELDS)
					{
					fwrite (rgbMasks.Ptr, 1, sizeof (rgbMasks), FS);
					}
				if (infoV3.BMPInfoS.CompressionType == BMP_COMPR_ALPHABITFIELDS)
					{
					fwrite (rgbaMasks.Ptr, 1, sizeof (rgbaMasks), FS);
					}
				break;

			case BMP_V_4:
				fwrite (infoV4.Ptr, 1, sizeof (infoV4), FS);
				break;

			case BMP_V_5:
				fwrite (infoV5.Ptr, 1, sizeof (infoV5), FS);
				break;
			}

		// Записать новую палитру
		for (i = 0; i < colorsCount; i++)	// Количество цветов исходного файла
			{
			if (i < *ColorsCount)	// Если хватает переданных, пишем их
				{
				fprintf (FS, "%c%c%c", Palette[0][i].RGBA.B, Palette[0][i].RGBA.G, Palette[0][i].RGBA.R);
				}
			else	// В противном случае используем родную палитру
				{
				fprintf (FS, "%c%c%c", resPalette[i].RGBA.B, resPalette[i].RGBA.G, resPalette[i].RGBA.R);
				}

			if (bmph.BMPHeaderS.FileVersionMarker != BMP_V_CORE)
				{
				fprintf (FS, "%c", 0);
				}
			}

		// Записать оставшийся файл и завершить работу
		fwrite (pixelData, 1, bmph.BMPHeaderS.FileSize - bmph.BMPHeaderS.DataOffset, FS); 
		}

	// Завершено
	IBMP_EXIT (EXEC_OK)
	}
