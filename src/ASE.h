// Спецификации для палитры ASE
#define ASE_MAX_COLORS	1000

// Константы
#define ASE_SIGNATURE	0x46455341	// "ASEF"
#define ASE_VERSION		0x100

#define ASE_GROUPSTART	0x01C0
#define ASE_COLORENTRY	0x0100
#define ASE_GROUPEND	0x02C0

#define ASE_CMYK		0x4B594D43	// "CMYK"
#define ASE_LAB			0x2042414C	// "LAB "
#define ASE_RGB			0x20424752	// "RGB "
#define ASE_Grey		0x79617247	// "Gray"

#define ASE_GLOBAL		0x0000
#define ASE_SPOT		0x0100
#define ASE_NORMAL		0x0200

// Структуры для чтения файлов
union ASEHeader
	{
	struct ASEHeaderStructure
		{
		ulong Signature;
		ulong Version;
		ulong BlocksCount;
		} ASEHeaderS;
	uchar Ptr[sizeof (struct ASEHeaderStructure)];
	};

union ASEBlockType
	{
	struct ASEBlockTypeStructure
		{
		uint BlockType;
		ulong BlockSize;
		} ASEBlockTypeS;
	uchar Ptr[sizeof (struct ASEBlockTypeStructure)];
	};

union ASEBlockHeader
	{
	uint BlockNameLength;
	uchar Ptr[sizeof (uint)];
	};

union ASEColorType
	{
	ulong ColorType;
	uchar Ptr[sizeof (ulong)];
	};

union ASE_RGBColor
	{
	struct ASE_RGBColorStructure
		{
		float Values[3];
		uint Flags;
		} ASE_RGBColorS;
	ulong LEBEPointers[3];
	uchar Ptr[sizeof (struct ASE_RGBColorStructure)];
	};

union ASE_CMYKColor
	{
	struct ASE_CMYKColorStructure
		{
		float Values[4];
		uint Flags;
		} ASE_CMYKColorS;
	ulong LEBEPointers[4];
	uchar Ptr[sizeof (struct ASE_CMYKColorStructure)];
	};

union ASE_GreyColor
	{
	struct ASE_GreyColorStructure
		{
		float Value;
		uint Flags;
		} ASE_GreyColorS;
	ulong LEBEPointer;
	uchar Ptr[sizeof (struct ASE_GreyColorStructure)];
	};

// Декларации
sint ASE_LoadPaletteEx (schar *FileName, union RGB_Color **Palette, uint *ColorsCount);
sint ASE_SavePaletteEx (schar *FileName, union RGB_Color *Palette, uint ColorsCount);
