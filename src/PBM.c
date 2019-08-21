// ����������� Portable Bitmap
#include "BatchImageConvertor.h"
#include "PBM.h"

#define IPBM_EXIT(c)	fclose (FS); return c;
#define PBM_SetPixel(_W,_H,_R,_G,_B) buf[(_H * *Width + _W) * 3 + 0] = (uchar)_R; buf[(_H * *Width + _W) * 3 + 1] = (uchar)_G;\
								buf[(_H * *Width + _W) * 3 + 2] = (uchar)_B;
#define PBM_GetPixelR(_W,_H) Buffer[(_H * Width + _W) * 3 + 0]
#define PBM_GetPixelG(_W,_H) Buffer[(_H * Width + _W) * 3 + 1]
#define PBM_GetPixelB(_W,_H) Buffer[(_H * Width + _W) * 3 + 2]

// ������� ��������� ��������� ����������� � ���������� ��� � ���� ������� ��������
sint PBM_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	// ����������
	FILE *FS;
	sint r, g, b, w, h, i;
	uchar type;
	uchar *buf;

	// ��������
	BIC_CHECK_INPUT_PARAMETERS
	
	// �������� �����
	BIC_INIT_FILE_READING

	// ������ ������
	r = fgetc (FS);
	g = fgetc (FS);
	if ((r != 'P') || (g < '1') || (g > '6'))
		{
		IPBM_EXIT (EXEC_INVALID_FILE)
		}
	type = g - '0';

	// ������ �������� �����������
	if ((fscanf (FS, "%u %u", Width, Height) != 2) || BIC_IMAGE_IS_INVALID)
		{
		IPBM_EXIT (EXEC_INVALID_FILE)
		}
	if (BIC_IMAGE_IS_INVALID)
		{
		IPBM_EXIT (EXEC_INVALID_FILE)
		}

	// ����������� ����� �������� �����, ���� �� ����, �� ���������
	if ((type != PBMBitmapAsBinary) && (type != PBMBitmapAsText))
		{
		fscanf (FS, "%u", &r);		// ����� ������������
		}
	fgetc (FS);		// ����������� ������

	// ������ �����������
	if ((buf = (uchar *)malloc (*Width * *Height * 3)) == NULL)
		{
		IPBM_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}

	for (h = 0; h < *Height; h++)
		{
		for (w = 0; w < *Width; w++)
			{
			// ������ � ��������
			if ((r = fgetc (FS)) < 0)
				{
				IPBM_EXIT (EXEC_INVALID_FILE)
				}

			// ������
			switch (type)
				{
				// ��� P4
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

				// ��� P1
				case PBMBitmapAsText:
					if ((r < '0') || (r > '1'))	// ������, ������� � �.�.
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

				// ��� P5
				case PBMGreyscaleAsBinary:
					PBM_SetPixel (w, h, r, r, r);
					break;

				// ��� P2
				case PBMGreyscaleAsText:
					if ((r < '0') || (r > '9'))		// ������, ������� � �.�.
						{
						w--;
						continue;
						}

					fseek (FS, -1, SEEK_CUR);
					fscanf (FS, "%u", &r);
					PBM_SetPixel (w, h, r, r, r);
					break;

				// ��� P6
				case PBMColorAsBinary:
					g = fgetc (FS);
					b = fgetc (FS);
					if ((g < 0) || (b < 0))
						{
						IPBM_EXIT (EXEC_INVALID_FILE)
						}

					PBM_SetPixel (w, h, r, g, b);
					break;

				// ��� P3
				case PBMColorAsText:
					if ((r < '0') || (r > '9'))		// ������, ������� � �.�.
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

	// ������ ���������. �������
	*Buffer = buf;
	IPBM_EXIT (EXEC_OK)
	}

// ������� ��������� ��������� ����������� � ��������� �������
sint PBM_SaveImage (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar ImageType)
	{
	// ����������
	FILE *FS;
	sint w, h, b, i;

	// ��������
	BIC_CHECK_OUTPUT_PARAMETERS
	if ((ImageType < 1) || (ImageType > 6))
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// �������� �����
	BIC_INIT_FILE_WRITING

	// ������ ���������
	fprintf (FS, "P%u\n%u\n%u\n", ImageType, Width, Height);
	if ((ImageType != PBMBitmapAsBinary) && (ImageType != PBMBitmapAsText))
		{
		fprintf (FS, "255\n");
		}

	// ������ �����
	for (h = 0; h < Height; h++)
		{
		for (w = 0; w < Width; w++)
			{
			switch (ImageType)
				{
				// ��� P4
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

				// ��� P1
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

				// ��� P6
				case PBMColorAsBinary:
					fprintf (FS, "%c%c%c", PBM_GetPixelR (w, h), PBM_GetPixelG (w, h), PBM_GetPixelB (w, h));
					break;

				// ��� P3
				case PBMColorAsText:
					fprintf (FS, "%u %u %u ", PBM_GetPixelR (w, h), PBM_GetPixelG (w, h), PBM_GetPixelB (w, h));
					break;

				// ��� P5
				case PBMGreyscaleAsBinary:
					fprintf (FS, "%c", PBM_GetPixelR (w, h));
					break;

				// ��� P2
				case PBMGreyscaleAsText:
					fprintf (FS, "%u ", PBM_GetPixelR (w, h));
					break;
				}
			}
		}

	// ���������
	IPBM_EXIT (EXEC_OK)
	}
