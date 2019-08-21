// ������� BMP

#include "BatchImageConvertor.h"
#include "BMP.h"

#define IBMP_EXIT(c)		fclose (FS); return c;

// �������� ������������ �������� �����������
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

// ������� ��������� ������� �� ����� BMP (����� ��������� �� �������� �����������)
sint BMP_PaletteExchange (schar *FileName, union RGBA_Color **Palette, uint *ColorsCount, uchar SetPalette)
	{
		// ����������
	union BMPHeader bmph;			// ��������� �����
	union BMPInfoVCore infoVCore;	// ���������-���������
	union BMPInfoV3 infoV3;
	union BMPInfoV4 infoV4;
	union BMPInfoV5 infoV5;
	union BMP_RGBBitmasks rgbMasks;
	union BMP_RGBABitmasks rgbaMasks;
	uchar *pixelData;

	FILE *FS;						// ��������������� ���������
	ulong colorsCount;
	ulong i;

	union RGB_Color *supPalette;
	union RGBA_Color *resPalette;

	// ��������
	if (!FileName || !ColorsCount || (SetPalette != 0) && !Palette)
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// �������� �����
	BIC_INIT_FILE_READING

	// ������ ��������� � ��������
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
	
	// ������ ����������
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
			if ((infoV3.BMPInfoS.BitsPerPixel > 8) && (infoV3.BMPInfoS.ColorTableSize == 0))	// ��������������� �������
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

			// ����������
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
				(infoV4.BMPInfoS.V3Info.BMPInfoS.ColorTableSize == 0))	// ��������������� �������
				{
				IBMP_EXIT (EXEC_NO_PALETTE_AVAILABLE)
				}

			// ����������
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
				(infoV5.BMPInfoS.V4Info.BMPInfoS.V3Info.BMPInfoS.ColorTableSize == 0))	// ��������������� �������
				{
				IBMP_EXIT (EXEC_NO_PALETTE_AVAILABLE)
				}

			// ����������
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

	// ��� ����� ������� ����������� ��������
	/*if ((colorsCount == 0) || (colorsCount > BMP_MAX_COLORS))
		{
		colorsCount = BMP_MAX_COLORS;
		}*/

	// ������ �������
	if ((resPalette = (union RGBA_Color *)malloc (colorsCount * sizeof (union RGBA_Color))) == NULL)
		{
		IBMP_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}

	if (bmph.BMPHeaderS.FileVersionMarker == BMP_V_CORE)
		{
		// �������� � ������� ��������������� �������
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

	// ������������ �������
	for (i = 0; i < colorsCount; i++)
		{
		resPalette[i].RGBA.A = resPalette[i].RGBA.R;
		resPalette[i].RGBA.R = resPalette[i].RGBA.B;
		resPalette[i].RGBA.B = resPalette[i].RGBA.A;
		resPalette[i].RGBA.A = 255;
		}

	// ��������� �������
	if (SetPalette == 0)
		{
		*ColorsCount = colorsCount;
		*Palette = resPalette;
		}
	else
		{
		// ��������� ����� ������� � �����
		// ������� ��������� �������� ����
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

		// ����� ������� ��� ��� ������
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

		// �������� ����� �������
		for (i = 0; i < colorsCount; i++)	// ���������� ������ ��������� �����
			{
			if (i < *ColorsCount)	// ���� ������� ����������, ����� ��
				{
				fprintf (FS, "%c%c%c", Palette[0][i].RGBA.B, Palette[0][i].RGBA.G, Palette[0][i].RGBA.R);
				}
			else	// � ��������� ������ ���������� ������ �������
				{
				fprintf (FS, "%c%c%c", resPalette[i].RGBA.B, resPalette[i].RGBA.G, resPalette[i].RGBA.R);
				}

			if (bmph.BMPHeaderS.FileVersionMarker != BMP_V_CORE)
				{
				fprintf (FS, "%c", 0);
				}
			}

		// �������� ���������� ���� � ��������� ������
		fwrite (pixelData, 1, bmph.BMPHeaderS.FileSize - bmph.BMPHeaderS.DataOffset, FS); 
		}

	// ���������
	IBMP_EXIT (EXEC_OK)
	}
