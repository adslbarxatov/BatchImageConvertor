// ����� ���������
#include "BatchImageConvertor.h"

#include "TGA.h"
#include "PCX.h"
#include "PBM.h"
#include "ICO.h"

#include "ACT.h"
#include "BMP.h"
#include "PALw.h"
#include "PALj.h"
#include "ACO.h"
#include "ASE.h"

// ������� ��������� �������� � ������� LittleEndian �� �������� BigEndian � ��������
uint LEBE_i (uint Value)
	{
	return ((Value & 0xFF) << 8) | ((Value & 0xFF00) >> 8);
	}

ulong LEBE_l (ulong Value)
	{
	return ((Value & 0xFF) << 24) | ((Value & 0xFF00) << 8) | ((Value & 0xFF0000) >> 8) | ((Value & 0xFF000000) >> 24);
	}

// ���������� ��� ������� TGA (����� � ������� RGBA)
BIC_API sint TGA_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	// ����������
	struct rgbdata_t buf;
	sint res;
	uchar hasAlpha = 0;

	// �������� �� ������������ ���������� ����������
	BIC_CHECK_INPUT_PARAMETERS

	// ������ �� ������ �������
	if ((res = Image_LoadTGA (FileName, &buf)) < 0)
		return res;

	// ������ ������
	*Buffer = buf.buffer;
	*Width = buf.width;
	*Height = buf.height;
	if (buf.type != PF_RGBA_32)
		{
		// �� ����� ����, �� ���� ��
		return EXEC_INVALID_FILE;
		}
	if (buf.flags & IMAGE_HAS_ALPHA)
		{
		hasAlpha = 1;
		}

	return EXEC_OK;
	}

BIC_API sint TGA_Save (schar *FileName, uint Width, uint Height, uchar *Buffer)
	{
	// ����������
	struct rgbdata_t buf;
	uchar hasAlpha = 0;
	slong i;

	// �������� �� ������������ ���������� ����������
	BIC_CHECK_OUTPUT_PARAMETERS

	// ����� �����-�������
	for (i = 0; i < Width * Height; i++)
		{
		if (Buffer[i * 4 + 3] != 255)
			{
			hasAlpha++;
			break;
			}
		}

	// ������ �������
	buf.flags = ((hasAlpha != 0) ? IMAGE_HAS_ALPHA : 0) | IMAGE_HAS_COLOR;
	buf.height = Height;
	buf.width = Width;
	buf.type = PF_RGBA_32;
	buf.buffer = Buffer;

	// ������ �� ������ �������
	return Image_SaveTGA (FileName, &buf);
	}

// ���������� ��� ������� PCX (�������� ����� � ������� RGBA, ��� A = 255; ������� ����� � ������� RGB)
BIC_API sint PCX_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	union RGB_Palette_16 *dp = NULL;
	union RGB_Palette_256 *ep = NULL;
	return PCX_LoadImage (FileName, Width, Height, Buffer, &dp, &ep);
	}

BIC_API sint PCX_Save (schar *FileName, uint Width, uint Height, uchar *Buffer)
	{
	return PCX_SaveImage (FileName, Width, Height, Buffer);
	}

// ���������� ��� ������� PCX palette (����� � ������� RGB)
BIC_API sint PCX_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	uint Width = 0;
	uint Height = 0;
	uchar *Buffer;
	union RGB_Palette_16 *dp = NULL;
	union RGB_Palette_256 *ep = NULL;
	
	int res = PCX_LoadImage (FileName, &Width, &Height, &Buffer, &dp, &ep);
	if (res < 0)
		return res;

	// �������
	if (ep)
		{
		*ColorsCount = 256;
		*Palette = ep->Ptr;
		}
	else
		{
		*ColorsCount = 16;
		*Palette = dp->Ptr;
		}

	BIC_ReleaseBuffer (Buffer);	// �� �����
	return res;
	}

BIC_API sint PCX_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return EXEC_NOT_IMPLEMENTED;
	}

// ���������� ��� ������� PBM (����� � ������� RGBA, ��� A = 255; ������� ����� � ������� RGB)
BIC_API sint PBM_Load (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	return PBM_LoadImage (FileName, Width, Height, Buffer);
	}

BIC_API sint PBM_Save (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar ImageType)
	{
	return PBM_SaveImage (FileName, Width, Height, Buffer, ImageType);
	}

// ���������� ��� ������� ICO (����� � ������� RGBA)
BIC_API sint ICO_Load (schar* FileName, uint* WidthHeight, uchar** Buffer, ulong *Length)
	{
	return ICO_LoadImage (FileName, WidthHeight, Buffer, Length);

	// ������� ����������
	return EXEC_NOT_IMPLEMENTED;
	}

BIC_API sint ICO_Save (schar* FileName, uint Width, uint Height, uchar* Buffer, uchar CodecType)
	{
	// ������� �� ������������
	return EXEC_NOT_IMPLEMENTED;
	}

// ���������� ��� ������� BMP (����� � ������� RGBA, ��� A � ���������)
BIC_API sint BMP_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGBA_Color *palette;
	int res = BMP_PaletteExchange (FileName, &palette, ColorsCount, 0);
	if (res < 0)
		return res;

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint BMP_SetPalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return BMP_PaletteExchange (FileName, &((union RGBA_Color *)Palette), &ColorsCount, 1);
	}

// ���������� ��� ������� ACT (����� � ������� RGB)
BIC_API sint ACT_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGBA_Color *palette;

	int res = ACT_LoadPaletteEx (FileName, &palette, ColorsCount);
	if (res < 0)
		return res;

	*Palette = palette->Ptr;
	return 0;
	}

BIC_API sint ACT_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	// ��������
	if (!FileName || !Palette || (ColorsCount < 1) || (ColorsCount > 256))
		return EXEC_INVALID_PARAMETERS;

	// �������
	return ACT_SavePaletteEx (FileName, ((union RGBA_Color *)Palette), ColorsCount);
	}

// ���������� ��� ������� Microsoft PAL (����� � ������� RGBA, ��� A � ���������)
BIC_API sint PALw_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGBA_Color *palette;
	int res = PALw_LoadPaletteEx (FileName, &palette, ColorsCount);
	if (res < 0)
		return res;

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint PALw_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return PALw_SavePaletteEx (FileName, ((union RGBA_Color *)Palette), ColorsCount);
	}

// ���������� ��� ������� JASC PAL (����� � ������� RGB)
BIC_API sint PALj_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGB_Color *palette;
	int res = PALj_LoadPaletteEx (FileName, &palette, ColorsCount);
	if (res < 0)
		return res;

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint PALj_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return PALj_SavePaletteEx (FileName, ((union RGB_Color *)Palette), ColorsCount);
	}

// ���������� ��� ������� Adobe Color Swatches (����� � ������� RGB)
BIC_API sint ACO_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGB_Color *palette;
	int res = ACO_LoadPaletteEx (FileName, &palette, ColorsCount);
	if ((res < 0) && (res != EXEC_UNSUPPORTED_COLORS))
		return res;

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint ACO_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return ACO_SavePaletteEx (FileName, ((union RGB_Color *)Palette), ColorsCount);
	}

// ���������� ��� ������� Adobe Swatches Exchange (����� � ������� RGB)
BIC_API sint ASE_LoadPalette (schar *FileName, uchar **Palette, uint *ColorsCount)
	{
	union RGB_Color *palette;
	int res = ASE_LoadPaletteEx (FileName, &palette, ColorsCount);
	if ((res < 0) && (res != EXEC_UNSUPPORTED_COLORS))
		return res;

	*Palette = palette->Ptr;
	return res;
	}

BIC_API sint ASE_SavePalette (schar *FileName, uchar *Palette, uint ColorsCount)
	{
	return ASE_SavePaletteEx (FileName, ((union RGB_Color *)Palette), ColorsCount);
	}

// ��������� ������������ ������ ��� ����������� �������
BIC_API void BIC_ReleaseBuffer (uchar *Buffer)
	{
	if (Buffer)
		free (Buffer);
	}

/*// ����� ���������� ������ ���������� ��� �������� �������������
BIC_API schar* BIC_GetLibVersion ()
	{
	return BIC_VERSION_S;
	}*/
