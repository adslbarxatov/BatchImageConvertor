// ����������
sint ICO_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer);

#define ICO_TYPE					1
#define ICO_IMAGE_SUBHEADER_SIZE	40

// ��������� ICO
union ICOHeader
	{
	// ��������� ���������
	struct ICOHeaderStructure
		{
		uint Filler;		// = 0
		uint Type;			// = 1 for ICO
		uint ImagesCount;	// ���������� ������� � �����
		} ICOHeaderS;

	uchar ICOHeaderPtr[sizeof (struct ICOHeaderStructure)];
	};

union ICOImage
	{
	// ��������� ���������
	struct ICOImageStructure
		{
		uchar Width;
		uchar Height;
		uchar PaletteSize;	// 0 for RGB
		uchar Filler;		// = 0

		uint PlanesCount;	// 0 or 1
		uint BPP;			// ������ 24 ��� 32

		ulong Length;
		ulong Offset;
		} ICOImageS;

	uchar ICOImagePtr[sizeof (struct ICOImageStructure)];
	};
