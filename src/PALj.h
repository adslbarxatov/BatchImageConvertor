// Спецификации для палитры JASC PAL
#define JASC_SIGNATURE	"JASC-PAL"
#define JASC_VERSION	"0100"

#define PALj_MAX_COLORS	1000

// Декларации
sint PALj_LoadPaletteEx (schar *FileName, union RGB_Color **Palette, uint *ColorsCount);
sint PALj_SavePaletteEx (schar *FileName, union RGB_Color *Palette, uint ColorsCount);
