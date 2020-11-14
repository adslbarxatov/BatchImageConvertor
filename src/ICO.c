#include "BatchImageConvertor.h"
#include "ICO.h"

#define IICO_EXIT(c)	fclose (FS); return c;

// ������� ��������� ����������� �� �����
sint ICO_LoadImage (schar * FileName, uint * Width, uint * Height, uchar * *Buffer)
	{
	// ����������
	uchar* buf;
	FILE* FS;
	uint i, w, h;
	sint idx;

	union ICOHeader icoh;
	union ICOImage* icoi;

	uint maskLineLength;
	uchar* maskLine;

	// �������� �����
	BIC_INIT_FILE_READING

		// ��������
		BIC_CHECK_INPUT_PARAMETERS

		// �������� ��������� � ��� ��������
		if ((fread (icoh.ICOHeaderPtr, 1, sizeof (icoh), FS) != sizeof (icoh)) ||
			(icoh.ICOHeaderS.Filler != 0) || (icoh.ICOHeaderS.Type != ICO_TYPE) || (icoh.ICOHeaderS.ImagesCount < 1))
			{
			IICO_EXIT (EXEC_INVALID_FILE)
			}

	// �������� � �������� ���������� �����������
	if ((icoi = (union ICOImage*)malloc (sizeof (union ICOImage) * icoh.ICOHeaderS.ImagesCount)) == NULL)
		{
		IICO_EXIT (EXEC_MEMORY_ALLOC_FAIL);
		}

	idx = -1;
	*Width = 0;
	*Height = 0;

	for (i = 0; i < icoh.ICOHeaderS.ImagesCount; i++)
		{
		if (fread (icoi + i, 1, sizeof (union ICOImage), FS) != sizeof (union ICOImage))
			{
			free (icoi);
			IICO_EXIT (EXEC_INVALID_FILE)
			}

		// �������� ������� � �����������
		if ((icoi[i].ICOImageS.BPP == 24) && (icoi[i].ICOImageS.Filler == 0) &&
			(icoi[i].ICOImageS.PaletteSize == 0) && (icoi[i].ICOImageS.Width == icoi[i].ICOImageS.Height))
			{
			// ����������� ������ �������� �����������
			if (icoi[i].ICOImageS.Width > * Width)
				{
				*Width = icoi[i].ICOImageS.Width;
				*Height = icoi[i].ICOImageS.Height;
				idx = i;
				}
			}
		}

	// ���� ��� ���������� ��������
	if (idx < 0)
		{
		IICO_EXIT (EXEC_INVALID_FILE)
		}

	// ��������� �����������
	if ((buf = (uchar*)malloc (*Width * *Height * 4)) == NULL)
		{
		free (icoi);
		IICO_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}
	fseek (FS, icoi[idx].ICOImageS.Offset + ICO_IMAGE_SUBHEADER_SIZE, SEEK_SET);

	for (h = *Height; h > 0; h--)
		{
		for (w = 0; w < *Width; w++)
			{
			buf[((h - 1) * *Width + w) * 4 + 2] = fgetc (FS);
			buf[((h - 1) * *Width + w) * 4 + 1] = fgetc (FS);
			buf[((h - 1) * *Width + w) * 4 + 0] = fgetc (FS);
			}
		}

	// ��������� �����
	maskLineLength = (((*Width - 1) >> 5) + 1) << 2;
	if ((maskLine = (uchar*)malloc (maskLineLength)) == NULL)
		{
		free (icoi);
		IICO_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}

	for (h = *Height; h > 0; h--)
		{
		if (fread (maskLine, 1, maskLineLength, FS) != maskLineLength)
			{
			free (icoi);
			free (maskLine);
			IICO_EXIT (EXEC_INVALID_FILE)
			}

		for (w = *Width; w > 0; w--)
			{
			buf[((h - 1) * *Width + w - 1) * 4 + 3] = ((maskLine[(w - 1) >> 3] >> (7 - (w - 1) % 8)) & 0x1) ? 0x00 : 0xFF;
			}
		}

	// ���������
	free (icoi);
	free (maskLine);
	*Buffer = buf;
	return EXEC_OK;
	}
