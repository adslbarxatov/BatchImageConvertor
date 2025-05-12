#include "BatchImageConvertor.h"
#include "ICO.h"

#define IICO_EXIT(c)	fclose (FS); return c;

// Функция загружает изображение из файла.
// При получении изображения в формате PNG возвращает ненулевой Length и нулевые Width и Height.
// При получении изображения в формате BMP возвращает нулевой Length и одинаковые ненулевые Width и Height
sint ICO_LoadImage (schar *FileName, uint *WidthHeight, uchar **Buffer, ulong *Length)
	{
	// Переменные
	uchar	*buf;
	FILE	*FS;
	uint	i, w, h;
	sint	idx;
	uchar	png = 0;

	union ICOHeader icoh;
	union ICOImage *icoi;

	uint maskLineLength;
	uchar *maskLine;

	// Открытие файла
	BIC_INIT_FILE_READING;

	// Контроль
	if (!WidthHeight || !FileName)
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// Загрузка заголовка и его контроль
	if ((fread (icoh.ICOHeaderPtr, 1, sizeof (icoh), FS) != sizeof (icoh)) ||
		(icoh.ICOHeaderS.Filler != 0) || (icoh.ICOHeaderS.Type != ICO_TYPE) || (icoh.ICOHeaderS.ImagesCount < 1))
		{
		IICO_EXIT (EXEC_INVALID_FILE);
		}

	// Создание и загрузка заголовков изображений
	if ((icoi = (union ICOImage *)malloc (sizeof (union ICOImage) * icoh.ICOHeaderS.ImagesCount)) == NULL)
		{
		IICO_EXIT (EXEC_MEMORY_ALLOC_FAIL);
		}

	idx = -1;
	*WidthHeight = 0;
	*Length = 0;

	for (i = 0; i < icoh.ICOHeaderS.ImagesCount; i++)
		{
		if (fread (icoi + i, 1, sizeof (union ICOImage), FS) != sizeof (union ICOImage))
			{
			free (icoi);
			IICO_EXIT (EXEC_INVALID_FILE)
			}

		// Контроль формата и целостности
		if ((icoi[i].ICOImageS.Filler == 0) && (icoi[i].ICOImageS.PaletteSize == 0) &&
			(icoi[i].ICOImageS.Width == icoi[i].ICOImageS.Height))
			{
			if (icoi[i].ICOImageS.BPP == 32)
				{
				fseek (FS, icoi[i].ICOImageS.Offset, SEEK_SET);

				// PNG (ЙPNG = 0x89504E47)
				if ((fgetc (FS) == 0x89) && (*Length < icoi[i].ICOImageS.Length))
					{
					*Length = icoi[i].ICOImageS.Length;
					idx = i;
					png = 1;
					}

				// 32bpp BMP
				else if (*WidthHeight < icoi[i].ICOImageS.Width)
					{
					*WidthHeight = icoi[i].ICOImageS.Width;
					idx = i;
					}
				}
			else if (!png && (icoi[i].ICOImageS.BPP == 24))
				{
				// 24bpp BMP
				if (*WidthHeight < icoi[i].ICOImageS.Width)
					{
					*WidthHeight = icoi[i].ICOImageS.Width;
					idx = i;
					}
				}
			}
		}

	// Возврат сразу готового изображения
	if (png)
		{
		*WidthHeight = 0;
		
		if ((buf = (uchar *)malloc (*Length)) == NULL)
			{
			free (icoi);
			IICO_EXIT (EXEC_MEMORY_ALLOC_FAIL)
			}

		fseek (FS, icoi[idx].ICOImageS.Offset, SEEK_SET);
		for (i = 0; i < *Length; i++)
			buf[i] = fgetc (FS);

		fclose (FS);
		free (icoi);
		*Buffer = buf;
		return EXEC_OK;
		}

	// Дополнительная обработка BMP
	else
		{
		*Length = 0;
		if (*WidthHeight == 0)
			*WidthHeight = 256;
		}

	// Если нет подходящих форматов
	if (idx < 0)
		{
		IICO_EXIT (EXEC_INVALID_FILE)
		}

	// Получение изображения
	if ((buf = (uchar *)malloc (*WidthHeight * 4 * *WidthHeight)) == NULL)
		{
		free (icoi);
		IICO_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}
	fseek (FS, icoi[idx].ICOImageS.Offset + ICO_IMAGE_SUBHEADER_SIZE, SEEK_SET);

	for (h = *WidthHeight; h > 0; h--)
		{
		for (w = 0; w < *WidthHeight; w++)
			{
			buf[((h - 1) * *WidthHeight + w) * 4 + RGBOrder (2)] = fgetc (FS);
			buf[((h - 1) * *WidthHeight + w) * 4 + RGBOrder (1)] = fgetc (FS);
			buf[((h - 1) * *WidthHeight + w) * 4 + RGBOrder (0)] = fgetc (FS);
			if (icoi[idx].ICOImageS.BPP == 32)
				buf[((h - 1) * *WidthHeight + w) * 4 + 3] = fgetc (FS);
			}
		}

	// Наложение маски
	if (icoi[idx].ICOImageS.BPP == 24)
		{
		maskLineLength = (((*WidthHeight - 1) >> 5) + 1) << 2;
		if ((maskLine = (uchar *)malloc (maskLineLength)) == NULL)
			{
			free (icoi);
			IICO_EXIT (EXEC_MEMORY_ALLOC_FAIL)
			}

		for (h = *WidthHeight; h > 0; h--)
			{
			if (fread (maskLine, 1, maskLineLength, FS) != maskLineLength)
				{
				free (icoi);
				free (maskLine);
				IICO_EXIT (EXEC_INVALID_FILE)
				}

			for (w = *WidthHeight; w > 0; w--)
				{
				buf[((h - 1) * *WidthHeight + w - 1) * 4 + 3] = ((maskLine[(w - 1) >> 3] >> (7 - (w - 1) % 8)) & 0x1) ?
					0x00 : 0xFF;
				}
			}

		free (maskLine);
		}

	// Завершено
	fclose (FS);
	free (icoi);
	*Buffer = buf;
	return EXEC_OK;
	}
