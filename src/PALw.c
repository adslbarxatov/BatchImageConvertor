// Палитра Windows PAL

#include "BatchImageConvertor.h"
#include "PALw.h"

// Функция загружает цветовую палитру
sint PALw_LoadPaletteEx (schar *FileName, union RGBA_Color **Palette, uint *ColorsCount)
	{
	// Переменные
	FILE *FS;
	union WinPALHeader palh;
	ulong fileSize;
	union RGBA_Color *palette = NULL;
	sint i;

	// Контроль
	if (!FileName || !ColorsCount)
		{
		return EXEC_INVALID_PARAMETERS;
		}
	
	// Открытие файла и получение размера
	BIC_INIT_FILE_READING;
	fseek (FS, 0, SEEK_END);
	fileSize = ftell (FS);
	fseek (FS, 0, SEEK_SET);

	// Чтение и контроль
	if (fread (palh.Ptr, 1, sizeof (union WinPALHeader), FS) != sizeof (union WinPALHeader))
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}
	if ((palh.WinPALHeaderS.DATASignature != PAL_DATA_SIGNATURE) || (palh.WinPALHeaderS.PALDataSize != fileSize - 20) ||
		(palh.WinPALHeaderS.PaletteVersion != PAL_VERSION) || (palh.WinPALHeaderS.PALSignature != PAL_SIGNATURE) ||
		(palh.WinPALHeaderS.RIFFDataSize != fileSize - 8) || (palh.WinPALHeaderS.RIFFSignature != RIFF_SIGNATURE))
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}
	if (palh.WinPALHeaderS.ColorsCount != ((palh.WinPALHeaderS.PALDataSize - 4) / sizeof (union RGBA_Color)))
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}
	*ColorsCount = palh.WinPALHeaderS.ColorsCount;
	if ((*ColorsCount == 0) || (*ColorsCount > PALw_MAX_COLORS))
		{
		*ColorsCount = PALw_MAX_COLORS;
		}

	// Чтение палитры
	if ((palette = (union RGBA_Color *)malloc (*ColorsCount * sizeof (union RGBA_Color))) == NULL)
		{
		fclose (FS);
		return EXEC_MEMORY_ALLOC_FAIL;
		}
	if (fread (palette->Ptr, 1, *ColorsCount * sizeof (union RGBA_Color), FS) != *ColorsCount * sizeof (union RGBA_Color))
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}
	for (i = 0; i < *ColorsCount; i++)
		{
		palette[i].RGBA.A = 255;	// Это поле на самом деле является полем параметров, а не альфа-канала
		}

	// Завершено
	fclose (FS);
	*Palette = palette;
	return EXEC_OK;
	}

// Функция сохраняет цветовую палитру
sint PALw_SavePaletteEx (schar *FileName, union RGBA_Color *Palette, uint ColorsCount)
	{
	// Переменные
	FILE *FS;
	union WinPALHeader palh;
	sint i;

	// Контроль
	if (!FileName || !Palette || (ColorsCount == 0) || (ColorsCount > PALw_MAX_COLORS))
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// Формирование структуры
	palh.WinPALHeaderS.ColorsCount = ColorsCount;
	palh.WinPALHeaderS.DATASignature = PAL_DATA_SIGNATURE;
	palh.WinPALHeaderS.PALDataSize = ColorsCount * sizeof (union RGBA_Color) + 4;
	palh.WinPALHeaderS.PaletteVersion = PAL_VERSION;
	palh.WinPALHeaderS.PALSignature = PAL_SIGNATURE;
	palh.WinPALHeaderS.RIFFDataSize = palh.WinPALHeaderS.PALDataSize + 12;
	palh.WinPALHeaderS.RIFFSignature = RIFF_SIGNATURE;

	// Открытие файла
	BIC_INIT_FILE_WRITING;

	// Запись
	fwrite (palh.Ptr, 1, sizeof (union WinPALHeader), FS);
	for (i = 0; i < ColorsCount; i++)
		Palette[i].RGBA.A = 0;	// Это поле на самом деле является полем параметров, а не альфа-канала
	fwrite (Palette->Ptr, 1, ColorsCount * sizeof (union RGBA_Color), FS);

	// Завершено
	fclose (FS);
	return EXEC_OK;
	}
