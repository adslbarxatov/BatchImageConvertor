// ������� Adobe Color Swatches

#include "BatchImageConvertor.h"
#include "ACO.h"

// ������� ��������� �������
sint ACO_LoadPaletteEx (schar *FileName, union RGB_Color **Palette, uint *ColorsCount)
	{
	// ����������
	FILE *FS;
	union ACOHeader acoh;
	union ACOColor acoc;
	union RGB_Color *palette = NULL;
	union RGB_Color color;

	uchar hasColorNames = 0;
	ulong stringLength, i, d, j;
	sint returnResult = EXEC_OK;


	// ��������
	if (!FileName || !ColorsCount)
		{
		return EXEC_INVALID_PARAMETERS;
		}
	
	// �������� �����
	BIC_INIT_FILE_READING

	// ������ ���������
	if (fread (acoh.Ptr, 1, sizeof (union ACOHeader), FS) != sizeof (union ACOHeader))
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}

	// ����������� �������� ������
	switch (LEBE_i (acoh.ACOHeaderS.Version))
		{
		case ACO_VERSION_1:
			break;

		case ACO_VERSION_2:
			hasColorNames = 1;
			break;

		default:
			fclose (FS);
			return EXEC_INVALID_FILE;
		}

	// ������
	*ColorsCount = LEBE_i (acoh.ACOHeaderS.ColorsCount);
	if ((*ColorsCount == 0) || (*ColorsCount > ACO_MAX_COLORS))
		{
		*ColorsCount = ACO_MAX_COLORS;
		}

	if ((palette = (union RGB_Color *)malloc (*ColorsCount * sizeof (union RGB_Color))) == NULL)
		{
		fclose (FS);
		return EXEC_MEMORY_ALLOC_FAIL;
		}

	for (i = d = 0; i < *ColorsCount; i++)
		{
		// ������ �����
		if (fread (acoc.Ptr, 1, sizeof (union ACOColor), FS) != sizeof (union ACOColor))
			{
			fclose (FS);
			return EXEC_INVALID_FILE;
			}

		// ������ � ����� �����
		switch (LEBE_i (acoc.ACOColorS.ColorSpace))
			{
			case ACO_RGB:
				color = RGBtoRGB (LEBE_i (acoc.ACOColorS.ColorValues[0]),
					LEBE_i (acoc.ACOColorS.ColorValues[1]),
					LEBE_i (acoc.ACOColorS.ColorValues[2]));
				memcpy (palette[i - d].Ptr, color.Ptr, sizeof (union RGB_Color));
				break;

			case ACO_HSB:
				color = HSBtoRGB (LEBE_i (acoc.ACOColorS.ColorValues[0]),
					LEBE_i (acoc.ACOColorS.ColorValues[1]),
					LEBE_i (acoc.ACOColorS.ColorValues[2]));
				memcpy (palette[i - d].Ptr, color.Ptr, sizeof (union RGB_Color));
				break;

			case ACO_GREY:
				color = GreytoRGB (LEBE_i (acoc.ACOColorS.ColorValues[0]));
				memcpy (palette[i - d].Ptr, color.Ptr, sizeof (union RGB_Color));
				break;

			default:
				returnResult = EXEC_UNSUPPORTED_COLORS;
				d++;	// �������� ����� ���������� ���������������� ����� � ����� �������
				break;
			}

		// ������� ����� �����
		if (hasColorNames)
			{
			fread (&stringLength, 1, sizeof (stringLength), FS);
			stringLength = LEBE_l (stringLength);

			if (stringLength > 32)
				{
				fclose (FS);
				return EXEC_INVALID_FILE;
				}

			for (j = 0; j < stringLength * 2; j++)
				{
				fgetc (FS);
				}
			}
		}

	// ���������
	fclose (FS);
	*ColorsCount = *ColorsCount - d;	// ��������� ���������������� ����� �� �������
	*Palette = palette;
	return returnResult;
	}

// ������� ��������� �������
sint ACO_SavePaletteEx (schar *FileName, union RGB_Color *Palette, uint ColorsCount)
	{
	// Not implemented
	return EXEC_NOT_IMPLEMENTED;
	}
