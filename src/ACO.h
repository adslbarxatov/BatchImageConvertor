// Спецификации для палитры ACO
union ACOHeader
	{
	struct ACOHeaderStructure
		{
		uint Version;
		uint ColorsCount;
		} ACOHeaderS;
	uchar Ptr[sizeof (struct ACOHeaderStructure)];
	};

union ACOColor
	{
	struct ACOColorStructure
		{
		uint ColorSpace;
		uint ColorValues[4];
		} ACOColorS;
	uchar Ptr[sizeof (struct ACOColorStructure)];
	};

#define ACO_VERSION_1	1
#define ACO_VERSION_2	2
#define ACO_MAX_COLORS	1000

// Simple color spaces
#define ACO_RGB			0
#define ACO_HSB			1
#define ACO_GREY		8

// Needs color profiling
#define ACO_CMYK		2
#define ACO_LAB			7

// Needs color archive
#define ACO_Pantone		3
#define ACO_Focoltone	4
#define ACO_Trumatch	5
#define ACO_Toyo88_CF1050	6
#define ACO_HKS			10

// Декларации
sint ACO_LoadPaletteEx (schar *FileName, union RGB_Color **Palette, uint *ColorsCount);
sint ACO_SavePaletteEx (schar *FileName, union RGB_Color *Palette, uint ColorsCount);
