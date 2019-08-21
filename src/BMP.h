// Спецификации для BMP
#define BMP_SIGNATURE	0x4D42

#define BMP_V_CORE		12
#define BMP_V_3			40
#define BMP_V_4			108
#define BMP_V_5			124

#define BMP_COMPR_RGB				0
#define BMP_COMPR_RLE8				1
#define BMP_COMPR_RLE4				2
#define BMP_COMPR_BITFIELDS			3
#define BMP_COMPR_JPEG				4
#define BMP_COMPR_PNG				5
#define BMP_COMPR_ALPHABITFIELDS	6

#define BMP_MAX_COLORS	256
	
// Заголовок файла
union BMPHeader
	{
	struct BMPHeaderStructure
		{
		uint Signature;
		ulong FileSize;
		ulong _reserved;
		ulong DataOffset;
		ulong FileVersionMarker;
		} BMPHeaderS;
	uchar Ptr[sizeof (struct BMPHeaderStructure)];
	};

// Информационные структуры
union BMPInfoVCore
	{
	struct BMPInfoVCoreStructure
		{
		uint Width;
		uint Height;
		uint LayersCount;
		uint BitsPerPixel;
		} BMPInfoS;
	uchar Ptr[sizeof (struct BMPInfoVCoreStructure)];
	};

union BMPInfoV3
	{
	struct BMPInfoV3Structure
		{
		slong Width;
		slong Height;
		uint LayersCount;
		uint BitsPerPixel;
		ulong CompressionType;
		ulong PixelDataSize;	// Размер пиксельных данных
		slong XPixelsPerMeter;	// Разрешение изображения
		slong YPixelsPerMeter;
		ulong ColorTableSize;	// В ячейках
		ulong UsedColorsCount;
		} BMPInfoS;
	uchar Ptr[sizeof (struct BMPInfoV3Structure)];
	};

union BMPInfoV4
	{
	struct BMPInfoV4Structure
		{
		union BMPInfoV3 V3Info;
		ulong RedMask;
		ulong GreenMask;
		ulong BlueMask;
		ulong AlphaMask;
		ulong ColorSpace;
		uchar CIEXYZEndpoints[36];
		ulong RedGamma;
		ulong GreenGamma;
		ulong BlueGamma;
		} BMPInfoS;
	uchar Ptr[sizeof (struct BMPInfoV4Structure)];
	};

union BMPInfoV5
	{
	struct BMPInfoV5Structure
		{
		union BMPInfoV4 V4Info;
		ulong RasterRenderingPreferences;
		ulong ColorProfileDataOffset;
		ulong ColorProfileDataSize;
		ulong _reserved;
		} BMPInfoS;
	uchar Ptr[sizeof (struct BMPInfoV5Structure)];
	};

union BMP_RGBBitmasks
	{
	struct BMP_RGBBitmasksStructure
		{
		ulong RedBitmask;
		ulong GreenBitmask;
		ulong BlueBitmask;
		} BMP_RGBBitmasksS;
	uchar Ptr[sizeof (struct BMP_RGBBitmasksStructure)];
	};

union BMP_RGBABitmasks
	{
	struct BMP_RGBABitmasksStructure
		{
		ulong RedBitmask;
		ulong GreenBitmask;
		ulong BlueBitmask;
		ulong AlphaBitmask;
		} BMP_RGBABitmasksS;
	uchar Ptr[sizeof (struct BMP_RGBABitmasksStructure)];
	};

// Декларации
sint BMP_PaletteExchange (schar *FileName, union RGBA_Color **Palette, uint *ColorsCount, uchar SetPalette);
