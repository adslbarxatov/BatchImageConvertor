// ������������ ��� PCX
#define PCX_MF			0x0A
#define PCX_PALPREF		0x0C

#define PCX_V_2_5		0
#define PCX_V_2_8_PAL	2
#define PCX_V_2_8_NOPAL	3
#define PCX_V_3_0		5

#define PCX_RLE			1

#define PCX_MONOCHROME	0
#define PCX_COLOR		1
#define PCX_GREYSCALE	2

// ��������� PCX
union PCXHeader
	{
	// ��������� ���������
	struct PCXHeaderStructure
		{
		uchar Manufacturer;		// ���������� ���� 10 = ZSoft .PCX
		uchar Version;			// 0 = ������ 2.5
								// 2 = ������ 2.8 � ����������� � �������
								// 3 = ������ 2.8 ��� ���������� � �������
								// 5 = ������ 3.0
		uchar Encoding;			// 1 = ����������� �������� �������
		uchar BitsPerPixel;		// ����� ��� �� ������� � ����
		uint XMin;				// ����������� �������� �����������
		uint YMin;				// ����������� �������� �����������
		uint XMax;				// ������������ �������� �����������
		uint YMax;				// ������������ �������� �����������
		uint HRes;				// �������������� ���������� ���������� ����������
		uint VRes;				// ������������ ���������� ���������� ����������
		union RGB_Palette_16 DefaultPalette;	// ����� �������� �������
		uchar _reserved;
		uchar LayersCount;		// ����� �������� �����
		uint BytesPerLine;		// ����� ���� �� ������ � �������� ���� (��� PCX-������ ������ ������ ���� ������)
		uint PaletteType;		// ������� �������:
								// 1 = �������/�����-�����
								// 2 = �������� ������
		uchar _filler[58];		// ����������� ������ �� ����� ���������
		} PCXHeaderS;

	uchar PCXHeaderPtr[sizeof (struct PCXHeaderStructure)];
	};

// ����������
sint PCX_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer, 
	union RGB_Palette_16 **DefaultPalette, union RGB_Palette_256 **ExtendedPalette);
sint PCX_SaveImage (schar *FileName, uint Width, uint Height, uchar *Buffer);
