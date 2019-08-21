// Спецификации для палитры Windows PAL
union WinPALHeader
	{
	struct WinPALHeaderStructure
		{
		ulong RIFFSignature;
		ulong RIFFDataSize;
		ulong PALSignature;
		ulong DATASignature;
		ulong PALDataSize;
		uint PaletteVersion;
		uint ColorsCount;
		} WinPALHeaderS;
	uchar Ptr[sizeof (struct WinPALHeaderStructure)];
	};

#define RIFF_SIGNATURE	0x46464952		// "RIFF"
#define PAL_SIGNATURE	0x204C4150		// "PAL_"
#define PAL_DATA_SIGNATURE	0x61746164	// "data"
#define PAL_VERSION		0x0300

#define PALw_MAX_COLORS	1000

// Декларации
sint PALw_LoadPaletteEx (schar *FileName, union RGBA_Color **Palette, uint *ColorsCount);
sint PALw_SavePaletteEx (schar *FileName, union RGBA_Color *Palette, uint ColorsCount);
