/*
По материалам с codenet.ru

http://www.codenet.ru/progr/formt/pcx1.php

Reviewed by RD_AAOW
*/

#include "BatchImageConvertor.h"
#include "PCX.h"

#define IPCX_EXIT(c)	fclose (FS); return c;

// Функция читает из буфера один блок закодированных данных и запоминает повторитель count и байт
// данных data. Возвращает 0, если чтение всех данных завершено
uchar PCX_GetBlock (uchar *Buf, ulong BufSize, ulong *CurPos, uchar *Byte, ulong *Count)
	{
	*Count = 1;

	if (*CurPos + 1 >= BufSize)
		{
		return 0;
		}

	if ((Buf[*CurPos] & 0xC0) == 0xC0)
		{
		*Count = Buf[*CurPos] & 0x3F;
		*CurPos = *CurPos + 1;

		if (*CurPos + 1 >= BufSize)
			{
			return 0;
			}
		}

	*Byte = Buf[*CurPos];
	*CurPos = *CurPos + 1;
	return 1;
	}

// Загрузка файла
sint PCX_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer, 
	union RGB_Palette_16 **DefaultPalette, union RGB_Palette_256 **ExtendedPalette)
	{
	// Переменные
	union PCXHeader pcxh;			// Заголовок и палитры файла

	FILE *FS;						// Вспомогательные параметры
	ulong i, j, k;
	uchar c;
	ulong count;

	uint LineLength;				// Размер декодированной строки файла, округлённый по основанию 2 вверх
	uchar hasExtendedPalette = 0;	// Флаг наличия расширенной палитры
	ulong dataLength;				// Длина недекодированного блока данных
	
	uchar *buf1, *buf2;				// Буферы чтения
	ulong readPos1 = 0;
	ulong readPos2 = 0;

	// Контроль
	BIC_CHECK_INPUT_PARAMETERS

	// Открытие файла
	BIC_INIT_FILE_READING

	// Загрузка заголовка и его контроль
	if ((fread (pcxh.PCXHeaderPtr, 1, sizeof (pcxh), FS) != sizeof (pcxh)) ||
		(pcxh.PCXHeaderS.Manufacturer != PCX_MF) || (pcxh.PCXHeaderS.Version != PCX_V_3_0) ||
		(pcxh.PCXHeaderS.Encoding != PCX_RLE) || //(pcxh.PCXHeaderS.PaletteType < PCX_COLOR) ||
		(pcxh.PCXHeaderS.PaletteType > PCX_GREYSCALE) ||
		(pcxh.PCXHeaderS.LayersCount != 1) && (pcxh.PCXHeaderS.LayersCount != 3))
		{
		IPCX_EXIT (EXEC_INVALID_FILE)
		}
	if ((*DefaultPalette = (union RGB_Palette_16 *)malloc (sizeof(union RGB_Palette_16))) == NULL)
		{
		IPCX_EXIT (EXEC_MEMORY_ALLOC_FAIL);
		}
	memcpy (*DefaultPalette, pcxh.PCXHeaderS.DefaultPalette.Ptr, sizeof(union RGB_Palette_16));
	
	// Определение размеров изображения и реального размера строки
	*Width = pcxh.PCXHeaderS.XMax - pcxh.PCXHeaderS.XMin + 1;
	*Height = pcxh.PCXHeaderS.YMax - pcxh.PCXHeaderS.YMin + 1;
	if (BIC_IMAGE_IS_INVALID)
		{
		IPCX_EXIT (EXEC_INVALID_FILE)
		}
	LineLength = pcxh.PCXHeaderS.LayersCount * pcxh.PCXHeaderS.BytesPerLine;

	// Чтение дополнительной палитры
	*ExtendedPalette = NULL;
	if (pcxh.PCXHeaderS.BitsPerPixel == 8)
		{
		// Определение наличия дополнительной палитры и её чтение
		fseek (FS, -(1 + (long)sizeof (union RGB_Palette_256)), SEEK_END);
		if (fgetc (FS) == PCX_PALPREF)
			{
			if ((*ExtendedPalette = (union RGB_Palette_256 *)malloc (sizeof(union RGB_Palette_256))) == NULL)
				{
				IPCX_EXIT (EXEC_MEMORY_ALLOC_FAIL);
				}
			if (fread ((*ExtendedPalette)->Ptr, 1, sizeof(union RGB_Palette_256), FS) != sizeof(union RGB_Palette_256))
				{
				IPCX_EXIT (EXEC_INVALID_FILE)
				}
			hasExtendedPalette++;
			}
		}

	// Определение длины блока данных
	fseek (FS, 0, SEEK_END);
	dataLength = ftell (FS) - sizeof (pcxh) - (1 + (long)sizeof (union RGB_Palette_256)) * hasExtendedPalette;

	// Возврат в начало блока данных
	fseek (FS, sizeof (pcxh), SEEK_SET);

	// Чтение блока данных
	if ((buf1 = (uchar *)malloc (dataLength)) == NULL)
		{
		IPCX_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}
	if (fread (buf1, 1, dataLength, FS) != dataLength)
		{
		IPCX_EXIT (EXEC_INVALID_FILE)
		}
	fclose (FS);

	// Декодирование блока данных
	if ((buf2 = (uchar *)malloc (LineLength * *Height)) == NULL)
		{
		free (buf1);
		return EXEC_MEMORY_ALLOC_FAIL;
		}

	// Как оказалось, у некоторых программ есть привычка не дописывать последние строки изображения
	memset (buf2, 0xFF, LineLength * *Height);

	while (PCX_GetBlock (buf1, dataLength, &readPos1, &c, &count))
		for (i = 0; i < count; i++, *(buf2 + readPos2++) = c);

	free (buf1);

	// Сборка изображения
	if ((buf1 = (uchar *)malloc (*Width * *Height * 4)) == NULL)
		{
		free (buf2);
		return EXEC_MEMORY_ALLOC_FAIL;
		}

	// 24 bits
	if (pcxh.PCXHeaderS.LayersCount == 3)
		{
		for (i = 0; i < *Height; i++)
			{
			for (j = 0; j < *Width; j++)
				{
				for (k = 0; k < 3; k++)
					{
					buf1[(i * *Width + j) * 4 + RGBOrder (k)] =
						buf2[(i * 3 + k) * pcxh.PCXHeaderS.BytesPerLine + j];
					}
				buf1[(i * *Width + j) * 4 + 3] = 255;
				}
			}
		}

	// Indexed color/greyscale
	else if ((pcxh.PCXHeaderS.LayersCount == 1) && (pcxh.PCXHeaderS.BitsPerPixel == 8) && hasExtendedPalette)
		{
		for (i = 0; i < *Height; i++)
			{
			for (j = 0; j < *Width; j++)
				{
				for (k = 0; k < 3; k++)
					{
					buf1[(i * *Width + j) * 4 + RGBOrder (k)] =
						*((*ExtendedPalette)->Colors[buf2[pcxh.PCXHeaderS.BytesPerLine * i + j]].Ptr + k);
					}
				buf1[(i * *Width + j) * 4 + 3] = 255;
				}
			}
		}

	// Monochrome
	else if ((pcxh.PCXHeaderS.LayersCount == 1) && (pcxh.PCXHeaderS.BitsPerPixel == 1))
		{
		count = 0;
		for (i = 0; i < *Height; i++)
			{
			for (j = 0; j < *Width; j++, count++)
				{
				for (k = 0; k < 3; k++)
					{
					buf1[(i * *Width + j) * 4 + k] = ((buf2[count >> 3] & (1 << (7 - count % 8))) != 0) ? 255 : 0;
					}
				buf1[(i * *Width + j) * 4 + 3] = 255;
				}

			// Выравнивание
			while (count % 16 != 0)
				count++;
			}
		}

	// Unknown
	else
		{
		free (buf1);
		free (buf2);
		return EXEC_INVALID_FILE;
		}

	// Завершено
	free (buf2);
	*Buffer = buf1;
	return EXEC_OK;
	}

// Функция для записи закодированной пары байтов (или одиночного байта, если он не кодируется) в файл
// Возвращает число записанных байтов
uchar PCX_WriteByte (uchar Val, uchar Count, FILE *F) 
	{
	if (Count)
		{
		if ((Count == 1) && ((0xC0 & Val) != 0xC0))
			{
			fputc (Val, F);
			return 1;
			}
		else
			{
			fputc (0xC0 | Count, F);
			fputc (Val, F);
			return 2;
			}
		}

	return 0;
	}

// Эта функция кодирует содержимое одной строки изображения и записывает его в файл
void PCX_WriteLine (uchar *Buf, ulong BufLength, FILE *F)
	{
	uchar cur, last;
	ulong srcIndex;
	uchar runCount;		// Макс. значение равно 63 

	last = *Buf;
	runCount = 1;

	for (srcIndex = 1; srcIndex < BufLength; srcIndex++) 
		{
		cur = *(++Buf);

		if (cur == last) 
			{
			runCount++;		// Он кодируется 
			if (runCount == 63) 
				{
				PCX_WriteByte (last, runCount, F);
				runCount = 0;
				}
			}
		else	// this != last 
			{   
			if (runCount) 
				{
				PCX_WriteByte (last, runCount, F);
				}
			last = cur;
			runCount = 1;
			}
		} 

	// Завершение 
	if (runCount) 
		{  
		PCX_WriteByte (last, runCount, F);
		}
	}

// Сохранение файла
sint PCX_SaveImage (schar *FileName, uint Width, uint Height, uchar *Buffer, uint Resolution)
	{
	// Переменные
	union PCXHeader pcxh;			// Заголовок и палитры файла
	FILE *FS;
	ulong i, j, k;
	uchar *buf1;

	// Контроль
	BIC_CHECK_OUTPUT_PARAMETERS

	// Открытие файла
	BIC_INIT_FILE_WRITING

	// Формирование и запись заголовка
	memset (pcxh.PCXHeaderPtr, 0x00, sizeof(pcxh));
	pcxh.PCXHeaderS.Manufacturer = PCX_MF;
	pcxh.PCXHeaderS.Version = PCX_V_3_0;
	pcxh.PCXHeaderS.Encoding = PCX_RLE;
	pcxh.PCXHeaderS.BitsPerPixel = 8;
	pcxh.PCXHeaderS.XMin = pcxh.PCXHeaderS.YMin = 0;
	pcxh.PCXHeaderS.XMax = Width - 1;
	pcxh.PCXHeaderS.YMax = Height - 1;
	pcxh.PCXHeaderS.HRes = pcxh.PCXHeaderS.VRes = Resolution;
	pcxh.PCXHeaderS.LayersCount = 3;
	pcxh.PCXHeaderS.BytesPerLine = (Width + Width % 2);
	pcxh.PCXHeaderS.PaletteType = PCX_COLOR;

	fwrite (pcxh.PCXHeaderPtr, 1, sizeof (pcxh), FS);

	// Развёртка изображения
	if ((buf1 = (uchar *)malloc (pcxh.PCXHeaderS.BytesPerLine * Height * pcxh.PCXHeaderS.LayersCount)) == NULL)
		{
		IPCX_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}
	memset (buf1, 0x00, pcxh.PCXHeaderS.BytesPerLine * Height * pcxh.PCXHeaderS.LayersCount);

	for (i = 0; i < Height; i++)
		{
		for (j = 0; j < Width; j++)
			{
			for (k = 0; k < pcxh.PCXHeaderS.LayersCount; k++)
				{
				buf1[(i * pcxh.PCXHeaderS.LayersCount + RGBOrder (k)) * pcxh.PCXHeaderS.BytesPerLine + j] =
					Buffer[(i * Width + j) * pcxh.PCXHeaderS.LayersCount + k];
				}
			}
		}

	// Кодирование и запись
	for (i = 0; i < (ulong)Height * (ulong)pcxh.PCXHeaderS.LayersCount; i++)
		{
		PCX_WriteLine (buf1 + i * pcxh.PCXHeaderS.BytesPerLine, pcxh.PCXHeaderS.BytesPerLine, FS);
		}

	// Завершено
	free (buf1);
	IPCX_EXIT (EXEC_OK)
	}
