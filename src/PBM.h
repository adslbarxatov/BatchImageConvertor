// Спецификации для Portable bitmap

// Возможные типы изображений
enum PBMImageTypes
	{
	// Чёрно-белое в текстовом представлении
	PBMBitmapAsText = 1,

	// Оттенки серого в текстовом представлении
	PBMGreyscaleAsText = 2,

	// Цветное в текстовом представлении
	PBMColorAsText = 3,

	// Чёрно-белое в бинарном представлении
	PBMBitmapAsBinary = 4,

	// Оттенки серого в бинарном представлении
	PBMGreyscaleAsBinary = 5,

	// Цветное в бинарном представлении
	PBMColorAsBinary = 6
	};

// Декларации
sint PBM_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer);
sint PBM_SaveImage (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar ImageType);
