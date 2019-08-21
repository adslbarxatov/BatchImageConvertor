// ������� Adobe Swatches Exchange

#include "BatchImageConvertor.h"
#include "ASE.h"

#define ASE_EXIT(c)		fclose (FS); return c;

// ������� ��������� �������
sint ASE_LoadPaletteEx (schar *FileName, union RGB_Color **Palette, uint *ColorsCount)
	{
	// ����������
	FILE *FS;
	union RGB_Color *palette = NULL;
	union RGB_Color color;
	sint returnResult = EXEC_OK;

	union ASEHeader aseh;
	ulong blockNameLength, i, j, d;
	union ASEBlockType asebt;
	union ASEBlockHeader asebh;

	union ASEColorType asect;
	union ASE_RGBColor rgb;
	union ASE_CMYKColor cmyk;
	union ASE_GreyColor grey;

	// ��������
	if (!FileName || !ColorsCount)
		{
		return EXEC_INVALID_PARAMETERS;
		}
	
	// �������� �����
	BIC_INIT_FILE_READING

	// ������ ���������
	if ((fread (aseh.Ptr, 1, sizeof (union ASEHeader), FS)) != sizeof (union ASEHeader) || 
		(aseh.ASEHeaderS.Signature != ASE_SIGNATURE) || (aseh.ASEHeaderS.Version != ASE_VERSION))
		{
		ASE_EXIT (EXEC_INVALID_FILE)
		}
	*ColorsCount = LEBE_l (aseh.ASEHeaderS.BlocksCount);

	if ((palette = (union RGB_Color *)malloc (*ColorsCount * sizeof (union RGB_Color))) == NULL)
		{
		ASE_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}

	// ������ �����
	for (i = 0; i < *ColorsCount; i++)
		{
		// ������ ���� �����
		if ((fread (asebt.Ptr, 1, sizeof (union ASEBlockType), FS)) != sizeof (union ASEBlockType))
			{
			ASE_EXIT (EXEC_INVALID_FILE)
			}

		// ������
		switch (asebt.ASEBlockTypeS.BlockType)
			{
			// ����������� ���
			case ASE_GROUPSTART:
				d++;
				break;

			// ����������� ��� (��� ��������)
			case ASE_GROUPEND:
				// ������ ����� �����
				if ((fread (asebh.Ptr, 1, sizeof (union ASEBlockHeader), FS)) != sizeof (union ASEBlockHeader))
					{
					ASE_EXIT (EXEC_INVALID_FILE)
					}

				// ������� �����
				blockNameLength = LEBE_i (asebh.BlockNameLength) * 2;
				for (j = 0; j < blockNameLength; j++)
					{
					fgetc (FS);
					}

				d++;
				break;

			case ASE_COLORENTRY:
				// ������ ����� �����
				if ((fread (asebh.Ptr, 1, sizeof (union ASEBlockHeader), FS)) != sizeof (union ASEBlockHeader))
					{
					ASE_EXIT (EXEC_INVALID_FILE)
					}

				// ������� �����
				blockNameLength = LEBE_i (asebh.BlockNameLength) * 2;
				for (j = 0; j < blockNameLength; j++)
					{
					fgetc (FS);
					}

				// ������ ���� �����
				if ((fread (asect.Ptr, 1, sizeof (union ASEColorType), FS)) != sizeof (union ASEColorType))
					{
					ASE_EXIT (EXEC_INVALID_FILE)
					}

				// ������
				switch (asect.ColorType)
					{
					case ASE_RGB:
						// ������ � ���������� �����
						if ((fread (rgb.Ptr, 1, sizeof (union ASE_RGBColor), FS)) != sizeof (union ASE_RGBColor))
							{
							ASE_EXIT (EXEC_INVALID_FILE)
							}
						rgb.LEBEPointers[0] = LEBE_l (rgb.LEBEPointers[0]);
						rgb.LEBEPointers[1] = LEBE_l (rgb.LEBEPointers[1]);
						rgb.LEBEPointers[2] = LEBE_l (rgb.LEBEPointers[2]);

						// ���������� � �������
						color = RGBtoRGB_f (rgb.ASE_RGBColorS.Values[0], rgb.ASE_RGBColorS.Values[1], rgb.ASE_RGBColorS.Values[2]);
						memcpy (palette[i - d].Ptr, color.Ptr, sizeof (union RGB_Color));

						break;

					case ASE_Grey:
						// ������ � ���������� �����
						if ((fread (grey.Ptr, 1, sizeof (union ASE_GreyColor), FS)) != sizeof (union ASE_GreyColor))
							{
							ASE_EXIT (EXEC_INVALID_FILE)
							}
						grey.LEBEPointer = LEBE_l (grey.LEBEPointer);

						// ���������� � �������
						color = GreytoRGB_f (grey.ASE_GreyColorS.Value);
						memcpy (palette[i - d].Ptr, color.Ptr, sizeof (union RGB_Color));

						break;

					case ASE_LAB:
						// ������� ����� � ����� ������
						if ((fread (rgb.Ptr, 1, sizeof (union ASE_RGBColor), FS)) != sizeof (union ASE_RGBColor))
							{
							ASE_EXIT (EXEC_INVALID_FILE)
							}

						returnResult = EXEC_UNSUPPORTED_COLORS;
						d++;
						break;

					case ASE_CMYK:
						// ������� ����� � ����� ������
						if ((fread (cmyk.Ptr, 1, sizeof (union ASE_CMYKColor), FS)) != sizeof (union ASE_CMYKColor))
							{
							ASE_EXIT (EXEC_INVALID_FILE)
							}

						returnResult = EXEC_UNSUPPORTED_COLORS;
						d++;
						break;

					default:
						ASE_EXIT (EXEC_INVALID_FILE)
						break;
					}

				// ������ ����� ���������
				break;

			default:
				ASE_EXIT (EXEC_INVALID_FILE)
			}
		}

	// ���������
	*ColorsCount = *ColorsCount - d;	// ��������� ���������������� ����� �� �������
	*Palette = palette;
	ASE_EXIT (returnResult)
	}

// ������� ��������� �������
sint ASE_SavePaletteEx (schar *FileName, union RGB_Color *Palette, uint ColorsCount)
	{
	// Not implemented
	return EXEC_NOT_IMPLEMENTED;
	}
