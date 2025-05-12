// ������� Windows PAL

#include "BatchImageConvertor.h"
#include "PALw.h"

// ������� ��������� �������� �������
sint PALw_LoadPaletteEx (schar *FileName, union RGBA_Color **Palette, uint *ColorsCount)
	{
	// ����������
	FILE *FS;
	union WinPALHeader palh;
	ulong fileSize;
	union RGBA_Color *palette = NULL;
	sint i;

	// ��������
	if (!FileName || !ColorsCount)
		{
		return EXEC_INVALID_PARAMETERS;
		}
	
	// �������� ����� � ��������� �������
	BIC_INIT_FILE_READING;
	fseek (FS, 0, SEEK_END);
	fileSize = ftell (FS);
	fseek (FS, 0, SEEK_SET);

	// ������ � ��������
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

	// ������ �������
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
		palette[i].RGBA.A = 255;	// ��� ���� �� ����� ���� �������� ����� ����������, � �� �����-������
		}

	// ���������
	fclose (FS);
	*Palette = palette;
	return EXEC_OK;
	}

// ������� ��������� �������� �������
sint PALw_SavePaletteEx (schar *FileName, union RGBA_Color *Palette, uint ColorsCount)
	{
	// ����������
	FILE *FS;
	union WinPALHeader palh;
	sint i;

	// ��������
	if (!FileName || !Palette || (ColorsCount == 0) || (ColorsCount > PALw_MAX_COLORS))
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// ������������ ���������
	palh.WinPALHeaderS.ColorsCount = ColorsCount;
	palh.WinPALHeaderS.DATASignature = PAL_DATA_SIGNATURE;
	palh.WinPALHeaderS.PALDataSize = ColorsCount * sizeof (union RGBA_Color) + 4;
	palh.WinPALHeaderS.PaletteVersion = PAL_VERSION;
	palh.WinPALHeaderS.PALSignature = PAL_SIGNATURE;
	palh.WinPALHeaderS.RIFFDataSize = palh.WinPALHeaderS.PALDataSize + 12;
	palh.WinPALHeaderS.RIFFSignature = RIFF_SIGNATURE;

	// �������� �����
	BIC_INIT_FILE_WRITING;

	// ������
	fwrite (palh.Ptr, 1, sizeof (union WinPALHeader), FS);
	for (i = 0; i < ColorsCount; i++)
		Palette[i].RGBA.A = 0;	// ��� ���� �� ����� ���� �������� ����� ����������, � �� �����-������
	fwrite (Palette->Ptr, 1, ColorsCount * sizeof (union RGBA_Color), FS);

	// ���������
	fclose (FS);
	return EXEC_OK;
	}
