// Изображение Portable Bitmap
#include "BatchImageConvertor.h"
#include "PBM.h"

#define IPBM_EXIT(c)	fclose (FS); return c;
#define PBM_SetPixel(_W,_H,_R,_G,_B) buf[(_H * *Width + _W) * 3 + 0] = (uchar)_R; buf[(_H * *Width + _W) * 3 + 1] = (uchar)_G;\
								buf[(_H * *Width + _W) * 3 + 2] = (uchar)_B;
#define PBM_GetPixelR(_W,_H) Buffer[(_H * Width + _W) * 3 + 0]
#define PBM_GetPixelG(_W,_H) Buffer[(_H * Width + _W) * 3 + 1]
#define PBM_GetPixelB(_W,_H) Buffer[(_H * Width + _W) * 3 + 2]

// Функция загружает указанное изображение и возвращает его в виде массива пикселей
sint PBM_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	// Переменные
	FILE *FS;
	sint r, g, b, w, h, i;
	uchar type;
	uchar *buf;

	// Контроль
	BIC_CHECK_INPUT_PARAMETERS
	
	// Открытие файла
	BIC_INIT_FILE_READING

	// Чтение версии
	r = fgetc (FS);
	g = fgetc (FS);
	if ((r != 'P') || (g < '1') || (g > '6'))
		{
		IPBM_EXIT (EXEC_INVALID_FILE)
		}
	type = g - '0';

	// Чтение размеров изображения
	if ((fscanf (FS, "%u %u", Width, Height) != 2) || BIC_IMAGE_IS_INVALID)
		{
		IPBM_EXIT (EXEC_INVALID_FILE)
		}
	if (BIC_IMAGE_IS_INVALID)
		{
		IPBM_EXIT (EXEC_INVALID_FILE)
		}

	// Дочитывание длины цветовой шкалы, если не было, но требуется
	if ((type != PBMBitmapAsBinary) && (type != PBMBitmapAsText))
		{
		fscanf (FS, "%u", &r);		// Здесь игнорируется
		}
	fgetc (FS);		// Дочитывание абзаца

	// Чтение изображения
	if ((buf = (uchar *)malloc (*Width * *Height * 3)) == NULL)
		{
		IPBM_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}

	for (h = 0; h < *Height; h++)
		{
		for (w = 0; w < *Width; w++)
			{
			// Чтение и контроль
			if ((r = fgetc (FS)) < 0)
				{
				IPBM_EXIT (EXEC_INVALID_FILE)
				}

			// Разбор
			switch (type)
				{
				// Тип P4
				case PBMBitmapAsBinary:
					for (i = 7; (w < *Width) && (i >= 0); i--)
						{
						if ((r & (1 << i)) == 0)
							{
							PBM_SetPixel (w, h, 255, 255, 255)
							}
						else
							{
							PBM_SetPixel (w, h, 0, 0, 0);
							}
						w++;
						}
					w--;
					break;

				// Тип P1
				case PBMBitmapAsText:
					if ((r < '0') || (r > '1'))	// Абзацы, пробелы и т.п.
						{
						w--;
						continue;
						}

					if (r == '0')
						{
						PBM_SetPixel (w, h, 255, 255, 255);
						}
					else
						{
						PBM_SetPixel (w, h, 0, 0, 0);
						}
					break;

				// Тип P5
				case PBMGreyscaleAsBinary:
					PBM_SetPixel (w, h, r, r, r);
					break;

				// Тип P2
				case PBMGreyscaleAsText:
					if ((r < '0') || (r > '9'))		// Абзацы, пробелы и т.п.
						{
						w--;
						continue;
						}

					fseek (FS, -1, SEEK_CUR);
					fscanf (FS, "%u", &r);
					PBM_SetPixel (w, h, r, r, r);
					break;

				// Тип P6
				case PBMColorAsBinary:
					g = fgetc (FS);
					b = fgetc (FS);
					if ((g < 0) || (b < 0))
						{
						IPBM_EXIT (EXEC_INVALID_FILE)
						}

					PBM_SetPixel (w, h, r, g, b);
					break;

				// Тип P3
				case PBMColorAsText:
					if ((r < '0') || (r > '9'))		// Абзацы, пробелы и т.п.
						{
						w--;
						continue;
						}

					fseek (FS, -1, SEEK_CUR);
					fscanf (FS, "%u %u %u", &r, &g, &b);
					PBM_SetPixel (w, h, r, g, b);
					break;
				}
			}
		}

	// Чтение завершено. Возврат
	*Buffer = buf;
	IPBM_EXIT (EXEC_OK)
	}

// Функция сохраняет указанное изображение в требуемом формате
sint PBM_SaveImage (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar ImageType)
	{
	// Переменные
	FILE *FS;
	sint w, h, b, i;

	// Контроль
	BIC_CHECK_OUTPUT_PARAMETERS
	if ((ImageType < 1) || (ImageType > 6))
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// Открытие файла
	BIC_INIT_FILE_WRITING

	// Запись заголовка
	fprintf (FS, "P%u\n%u\n%u\n", ImageType, Width, Height);
	if ((ImageType != PBMBitmapAsBinary) && (ImageType != PBMBitmapAsText))
		{
		fprintf (FS, "255\n");
		}

	// Запись файла
	for (h = 0; h < Height; h++)
		{
		for (w = 0; w < Width; w++)
			{
			switch (ImageType)
				{
				// Тип P4
				case PBMBitmapAsBinary:
					b = 0;
					for (i = 7; (i >= 0) && (w < Width); i--)
						{
						if (PBM_GetPixelR (w, h) < 128)
							{
							b |= (1 << i);
							}
						w++;
						}
					fprintf (FS, "%c", b);
					w--;
					break;

				// Тип P1
				case PBMBitmapAsText:
					if (PBM_GetPixelR (w, h) < 128)
						{
						fprintf (FS, "1 ");
						}
					else
						{
						fprintf (FS, "0 ");
						}
					break;

				// Тип P6
				case PBMColorAsBinary:
					fprintf (FS, "%c%c%c", PBM_GetPixelR (w, h), PBM_GetPixelG (w, h), PBM_GetPixelB (w, h));
					break;

				// Тип P3
				case PBMColorAsText:
					fprintf (FS, "%u %u %u ", PBM_GetPixelR (w, h), PBM_GetPixelG (w, h), PBM_GetPixelB (w, h));
					break;

				// Тип P5
				case PBMGreyscaleAsBinary:
					fprintf (FS, "%c", PBM_GetPixelR (w, h));
					break;

				// Тип P2
				case PBMGreyscaleAsText:
					fprintf (FS, "%u ", PBM_GetPixelR (w, h));
					break;
				}
			}
		}

	// Завершено
	IPBM_EXIT (EXEC_OK)
	}
