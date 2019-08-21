// Спецификации для TGA
#define PF_RGBA_32		3		// Normal rgba buffer
#define PF_BGRA_32		4		// Big endian RGBA (MacOS)
#define PF_RGB_24		5		// Uncompressed dds or another 24-bit image 
#define PF_BGR_24		6		// Big-endian RGB (MacOS)

#define IMAGE_HAS_ALPHA	2		// Image contains alpha-channel
#define IMAGE_HAS_COLOR	4		// Image contains RGB-channel

// Собственно, изображение
struct rgbdata_t
	{
	uint	width;		// Image width
	uint	height;		// Image height
	ulong	type;		// Compression type
	ulong	flags;		// Misc image flags
	uchar	*buffer;	// Image buffer
	};

// Заголовок TGA-файла
struct tga_t
	{
	uchar	id_length;
	uchar	colormap_type;
	uchar	image_type;
	uint	colormap_index;
	uint	colormap_length;
	uchar	colormap_size;
	uint	x_origin;
	uint	y_origin;
	uint	width;
	uint	height;
	uchar	pixel_size;
	uchar	attributes;
	};

// Декларации
sint Image_LoadTGA (schar *FileName, struct rgbdata_t *Img);
sint Image_SaveTGA (schar *FileName, struct rgbdata_t *Img);
