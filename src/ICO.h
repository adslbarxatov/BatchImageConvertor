// Декларации
sint ICO_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer);

#define ICO_TYPE					1
#define ICO_IMAGE_SUBHEADER_SIZE	40

// Заголовок ICO
union ICOHeader
	{
	// Структура заголовка
	struct ICOHeaderStructure
		{
		uint Filler;		// = 0
		uint Type;			// = 1 for ICO
		uint ImagesCount;	// Количество значков в файле
		} ICOHeaderS;

	uchar ICOHeaderPtr[sizeof (struct ICOHeaderStructure)];
	};

union ICOImage
	{
	// Структура заголовка
	struct ICOImageStructure
		{
		uchar Width;
		uchar Height;
		uchar PaletteSize;	// 0 for RGB
		uchar Filler;		// = 0

		uint PlanesCount;	// 0 or 1
		uint BPP;			// Обычно 24 или 32

		ulong Length;
		ulong Offset;
		} ICOImageS;

	uchar ICOImagePtr[sizeof (struct ICOImageStructure)];
	};
