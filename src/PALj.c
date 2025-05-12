// ������� JASC PAL

#include "BatchImageConvertor.h"
#include "PALj.h"

// ������� ��������� �������� �������
sint PALj_LoadPaletteEx (schar *FileName, union RGB_Color **Palette, uint *ColorsCount)
	{
	// ����������
	FILE *FS;
	schar buf[256];
	union RGB_Color *palette = NULL;
	slong i;
	uint colorsCount;
	uint r, g, b;

	// ��������
	if (!FileName || !ColorsCount)
		{
		return EXEC_INVALID_PARAMETERS;
		}
	
	// �������� ����� � ��������� �������
	BIC_INIT_FILE_READING

	// ������ � ��������
	fgets (buf, 256, FS);
	while ((buf[strlen (buf) - 1] == '\x0A') || (buf[strlen (buf) - 1] == '\x0D'))
		{
		buf[strlen (buf) - 1] = '\0';
		}
	if (strcmp (buf, JASC_SIGNATURE) != 0)
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}

	fgets (buf, 256, FS);
	while ((buf[strlen (buf) - 1] == '\x0A') || (buf[strlen (buf) - 1] == '\x0D'))
		{
		buf[strlen (buf) - 1] = '\0';
		}
	if (strcmp (buf, JASC_VERSION) != 0)
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}

	fscanf (FS, "%u", &colorsCount);
	if ((colorsCount == 0) || (colorsCount > PALj_MAX_COLORS))
		{
		colorsCount = PALj_MAX_COLORS;
		}
	*ColorsCount = colorsCount;

	// ������ �������
	if ((palette = (union RGB_Color *)malloc (colorsCount * sizeof (union RGB_Color))) == NULL)
		{
		fclose (FS);
		return EXEC_MEMORY_ALLOC_FAIL;
		}
	for (i = 0; i < colorsCount; i++)
		{
		if (fscanf (FS, "%u %u %u", &r, &g, &b) != sizeof (union RGB_Color))
			{
			fclose (FS);
			return EXEC_INVALID_FILE;
			}

		palette[i].RGB.R = r;
		palette[i].RGB.G = g;
		palette[i].RGB.B = b;
		}

	// ���������
	fclose (FS);
	*Palette = palette;
	return EXEC_OK;
	}

// ������� ��������� �������� �������
sint PALj_SavePaletteEx (schar *FileName, union RGB_Color *Palette, uint ColorsCount)
	{
	// ����������
	FILE *FS;
	sint i;

	// ��������
	if (!FileName || !Palette || (ColorsCount == 0) || (ColorsCount > PALj_MAX_COLORS))
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// �������� �����
	BIC_INIT_FILE_WRITING

	// ������
	fprintf (FS, "%s\n%s\n%u\n", JASC_SIGNATURE, JASC_VERSION, ColorsCount);

	for (i = 0; i < ColorsCount; i++)
		fprintf (FS, "%u %u %u\n", Palette[i].RGB.R, Palette[i].RGB.G, Palette[i].RGB.B);

	// ���������
	fclose (FS);
	return EXEC_OK;
	}
