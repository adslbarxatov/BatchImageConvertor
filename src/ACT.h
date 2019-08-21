// Спецификации для палитры ACT
#define ACT_MAX_COLORS	256

// Декларации
sint ACT_LoadPaletteEx (schar *FileName, union RGBA_Color **Palette, uint *ColorsCount);
sint ACT_SavePaletteEx (schar *FileName, union RGBA_Color *Palette, uint ColorsCount);
