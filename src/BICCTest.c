#include "BatchImageConvertor.h"
#include "ASE.h"

int main (int argc, char *argv[])
	{
	union RGB_Color *colors;
	uint colorsCount;
	int res = ASE_LoadPaletteEx (argv[1], &colors, &colorsCount);

	return 0;
	}
