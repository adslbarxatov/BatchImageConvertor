// ������� Adobe Color Table

#include "BatchImageConvertor.h"
#include "ACT.h"

// ������� ��������� �������� �������
sint ACT_LoadPaletteEx (schar *FileName, union RGBA_Color **Palette, uint *ColorsCount)
	{
	// ����������
	FILE *FS;
	union RGB_Palette_256 supPalette;
	union RGBA_Color *resPalette = NULL;
	sint transparencyColorNumber = 0;	// ��������� � ����; -1 - ���� �� �����
	sint i;

	// ��������
	if (!FileName || !ColorsCount)
		{
		return EXEC_INVALID_PARAMETERS;
		}
	
	// �������� �����
	BIC_INIT_FILE_READING

	// ������
	if (fread (supPalette.Ptr, 1, sizeof (union RGB_Palette_256), FS) != sizeof (union RGB_Palette_256))
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}

	*ColorsCount = ((fgetc (FS) & 0xFF) << 8) | (fgetc (FS) & 0xFF);
	if ((*ColorsCount == 0) || (*ColorsCount > ACT_MAX_COLORS))
		{
		*ColorsCount = ACT_MAX_COLORS;
		}

	transparencyColorNumber = ((fgetc (FS) & 0xFF) << 8) | (fgetc (FS) & 0xFF);
	if ((transparencyColorNumber < -1) || (transparencyColorNumber > 255))
		{
		transparencyColorNumber = -1;
		}

	// ��������� ������
	if ((resPalette = (union RGBA_Color *)malloc (*ColorsCount * sizeof (union RGBA_Color))) == NULL)
		{
		return EXEC_MEMORY_ALLOC_FAIL;
		}

	for (i = 0; i < *ColorsCount; i++)
		{
		resPalette[i].RGBA.R = supPalette.Colors[i].RGB.R;
		resPalette[i].RGBA.G = supPalette.Colors[i].RGB.G;
		resPalette[i].RGBA.B = supPalette.Colors[i].RGB.B;

		resPalette[i].RGBA.A = 255;
		if (i == transparencyColorNumber)
			{
			resPalette[i].RGBA.A = 0;
			}
		}

	// ���������
	fclose (FS);
	*Palette = resPalette;
	return EXEC_OK;
	}

// ������� ��������� �������� �������
sint ACT_SavePaletteEx (schar *FileName, union RGBA_Color *Palette, uint ColorsCount)
	{
	// ����������
	FILE *FS;
	sint i;
	uint transparencyColorNumber = -1;

	// ��������
	if (!FileName || !Palette || (ColorsCount == 0) || (ColorsCount > ACT_MAX_COLORS))
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// �������� �����
	BIC_INIT_FILE_WRITING

	// ������
	for (i = 0; i < ColorsCount; i++)
		{
		fprintf (FS, "%c%c%c", Palette[i].RGBA.R, Palette[i].RGBA.G, Palette[i].RGBA.B);
		if (Palette[i].RGBA.A != 255)
			{
			transparencyColorNumber = i;
			}
		}
	for (i = ColorsCount; i < ACT_MAX_COLORS; i++)
		{
		fprintf (FS, "%c%c%c", 0, 0, 0);
		}

	fprintf (FS, "%c%c%c%c", (ColorsCount >> 8) & 0xFF, ColorsCount & 0xFF,
		(transparencyColorNumber >> 8) & 0xFF, transparencyColorNumber & 0xFF);

	// ���������
	fclose (FS);
	return EXEC_OK;
	}
